using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Gigya.LiveTesting.Grains.Conts;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Streams;
using TopicMessage = Confluent.Kafka.ConsumeResult<string, string>;

namespace KafkaWeb.Grains
{
    public interface IOutputGrain : IGrainWithGuidKey
    {
        Task Start(string stream, string toTopic);
        Task Stop();
    }

    [CollectionAgeLimit(AlwaysActive = true)]
    // [ImplicitStreamSubscription(StreamProvider.OutputStream)]
    public class OutputGrain : Grain, IOutputGrain
    {
        private readonly IOptions<ConsumerSettings> _consumerOptions;
        private Guid _id;
        private IProducer<string, string> _producer;
        private StreamSubscriptionHandle<TopicMessage> _subscribion;

        public OutputGrain(IOptions<ConsumerSettings> consumerOptions)
        {
            _consumerOptions = consumerOptions;
        }

        public override async Task OnActivateAsync()
        {
            var clientConfig = new ProducerConfig()
            {
                BootstrapServers = _consumerOptions.Value.BootstrapServers
            };
            _id = this.GetPrimaryKey();

            _producer = new ProducerBuilder<string, string>(new ConsumerConfig(clientConfig)
            {
                GroupId = _consumerOptions.Value.Group,
                // The offset to start reading from if there are no committed offsets (or there was an error in retrieving offsets).
                AutoOffsetReset = AutoOffsetReset.Earliest,
                // Do not commit offsets.
                EnableAutoCommit = true
            }).Build();


            await base.OnActivateAsync();
        }

        public async Task Start(string stream, string toTopic = null)
        {
            _subscribion = await GetStreamProvider(StreamProvider.OutputStream)
                .GetStream<TopicMessage>(this._id, stream)
                .SubscribeAsync(list =>
                    onBatch(list.Select(e => e.Item),
                    toTopic ?? _consumerOptions.Value.ToTopic));
        }


        public async Task Stop()
        {
            await _subscribion.UnsubscribeAsync();
        }

        private async Task onBatch(IEnumerable<TopicMessage> @select, string toTopic)
        {
            foreach (var cr in @select)
            {
                await _producer.ProduceAsync(toTopic, cr.Message);
            }
        }
    }
}