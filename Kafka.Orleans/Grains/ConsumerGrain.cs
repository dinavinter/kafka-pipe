using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Gigya.LiveTesting.Grains.Conts;
using KafkaWeb.Grains;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using TopicMessage = Confluent.Kafka.ConsumeResult<string, string>;

namespace KafkaWeb
{
    public class ConsumerGrain : Grain, IConsumerGrain
    {
        private IDisposable _timerRegistrationSearch;
        private IDisposable _timerRegistrationSaveState;
        private   CancellationTokenSource _cts = new();

        private readonly IPersistentState<ConsumerState> _jobState;
        private readonly IOptions<ConsumerSettings> _consumerOptions;

        // private IAsyncStream<TopicMessage> _stream;

        // private ConsumerSettings _settings;

        // private ConsumerConfig _consumerConfig;
        private IConsumer<string, string> _consumer;
        private Guid _id;

        public ConsumerGrain(
            [PersistentState("topic-consumer-status", Storage.StatusStorage)]
            IPersistentState<ConsumerState> store,
            IOptions<ConsumerSettings> consumerOptions)
        {
            _jobState = store;
            _consumerOptions = consumerOptions;
        }

        public Task Cancel()
        {
            _cts.Cancel();
            return Task.CompletedTask;
        }

        public override async Task OnActivateAsync()
        {
            await _jobState.ReadStateAsync();
            var clientConfig = new ClientConfig
            {
                BootstrapServers = _consumerOptions.Value.BootstrapServers 
            };
            _id = this.GetPrimaryKey();

            _consumer = new ConsumerBuilder<string, string>(new ConsumerConfig(clientConfig)
            {
                GroupId = _consumerOptions.Value.Group,
                // The offset to start reading from if there are no committed offsets (or there was an error in retrieving offsets).
                AutoOffsetReset = AutoOffsetReset.Earliest,
                // Do not commit offsets.
                EnableAutoCommit = true
            }).Build();

            _jobState.State ??= new ConsumerState();

 
            await base.OnActivateAsync();
        }

        public Task Stop()
        {
            try
            {
                _timerRegistrationSaveState?.Dispose();
                _timerRegistrationSearch?.Dispose();
                _consumer.Dispose();
            }
            finally
            {
                _timerRegistrationSaveState = null;
                _timerRegistrationSearch = null;
            }

            return Task.CompletedTask;
        }

        public Task Subscribe(string topic)
        {
            if (!_consumer.Subscription.Contains(topic))
                _consumer.Subscribe(topic);

            _jobState.State.Subscription = _consumer.Subscription;

            if (_timerRegistrationSearch == null)
                Consume( );
            return Task.CompletedTask;
        }


        public Task<Guid> Consume(TimeSpan? dueTime = null, TimeSpan? period = null)
        {
            //todo add reminder to  consume every period 

            _timerRegistrationSearch =
                RegisterTimer(asyncCallback: TimerCallback,
                    /* will be passed to asyncCallback when the timer ticks*/
                    state: new ConsumerState(),
                    /* specifies a quantity of time to wait before issuing the first timer tick.*/
                    dueTime: dueTime ?? TimeSpan.FromMilliseconds(10),
                    /*specifies the amount of time that passes from the moment the Task returned by asyncCallback is resolved*/
                    period: period ?? TimeSpan.FromMinutes(1));

            _timerRegistrationSaveState =
                RegisterTimer(asyncCallback: async _ =>
                    {
                        await _jobState.ReadStateAsync();
                        if (_jobState.State == null)
                            _jobState.State = new ConsumerState();

                        await _jobState.WriteStateAsync();
                    },
                    /* will be passed to asyncCallback when the timer ticks*/
                    state: new ConsumerState(),
                    /* specifies a quantity of time to wait before issuing the first timer tick.*/
                    dueTime: TimeSpan.FromMilliseconds(20),
                    /*specifies the amount of time that passes from the moment the Task returned by asyncCallback is resolved*/
                    period: TimeSpan.FromMinutes(1));

            return Task.FromResult(_id);
        }

        public async Task TimerCallback(object state)
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var timer = new CancellationTokenSource(50);
                    _cts.Token.ThrowIfCancellationRequested();
                    // CancellationTokenSource.CreateLinkedTokenSource(timer.Token, _cts.Token)
                    //     .Token
                    var cr = _consumer.Consume(_cts.Token);


                    if (cr.Message.Value.Contains($"SiteId\":{_consumerOptions.Value.SiteId}"))
                    {
                       await GetStreamProvider(StreamProvider.OutputStream)
                            .GetStream<TopicMessage>(this._id, cr.Topic)
                            .OnNextAsync(cr);
                        

                        _jobState.State.Handled++;
                        _jobState.State.LastHandled = DateTimeOffset.Now;
                        _jobState.State.LastMsg = cr;
                        if (!_jobState.State.Topics.TryGetValue(cr.Topic, out var topic))
                        {
                            topic = new TopicConsumerState();
                            _jobState.State.Topics[cr.Topic] = topic;
                        }

                        topic.Handled++;
                        topic.LastHandled = DateTimeOffset.Now;
                        topic.LastMsg = cr;
                    }

                    await Task.Delay(20);
                }
            }
            catch (OperationCanceledException)
            {
                // Ctrl+C was pressed.
                Console.WriteLine($"Ctrl+C pressed, consumer exiting");
                await Stop();
            }

            await _jobState.WriteStateAsync();
        }
    }


    public class ConsumerState
    {
        public int Handled { get; set; }
        public DateTimeOffset LastHandled { get; set; }
        public TopicMessage LastMsg { get; set; }
        public Dictionary<string, TopicConsumerState> Topics { get; set; }
        public List<string> Subscription { get; set; }
    }

    public class TopicConsumerState
    {
        public int Handled { get; set; }
        public DateTimeOffset LastHandled { get; set; }
        public TopicMessage LastMsg { get; set; }
    }
}