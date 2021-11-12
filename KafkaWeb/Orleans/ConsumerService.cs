using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Gigya.LiveTesting.Grains.Conts;
using KafkaWeb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using TopicMessage = Confluent.Kafka.ConsumeResult<string, string>;

namespace Sample.ServerSide.Services
{

 
    
    public class ConsumerService
    {
  
        private readonly ILogger<ConsumerService> logger;
        private readonly IClusterClient client;

        public ConsumerService(ILogger<ConsumerService> logger, IClusterClient client)
        {
            this.logger = logger;
            this.client = client;
        }

         
       
        public async Task<IAsyncStream<Confluent.Kafka.ConsumeResult<string, string>>> GetStream<T>(Guid ownerKey, string topic , string siteId)
        {
           return await client.GetGrain<IConsumerGrain>(ownerKey)
                .Subscribe(topic, siteId);
           
        }

        private class TodoItemObserver<T> : IAsyncObserver<T>
        {
            private readonly ILogger<ConsumerService> logger;
            private readonly Func<T, Task> action;

            public TodoItemObserver(ILogger< ConsumerService> logger, Func<T, Task> action)
            {
                this.logger = logger;
                this.action = action;
            }

            public Task OnCompletedAsync() => Task.CompletedTask;

            public Task OnErrorAsync(Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Task.CompletedTask;
            }

            public Task OnNextAsync(T item, StreamSequenceToken token = null) => action(item);
        }
    
    }
    
    public static class ConsumerServiceExtensions
    {
        public static IServiceCollection AddConsumerService(this IServiceCollection services)
        {
            services.AddSingleton<ConsumerService>();
            return services;
        }
    }
}