using System;
using System.Threading.Tasks;
using Orleans;

namespace KafkaWeb
{
    public interface IConsumerGrain : IGrainWithGuidKey
    {
        public Task<Guid> Consume(TimeSpan? dueTime = null, TimeSpan? period = null);
        public Task Stop();

        Task Subscribe(string topic);
    }
}