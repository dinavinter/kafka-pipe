using System;
using System.Threading;
using System.Threading.Tasks;
using Gigya.LiveTesting.Grains.Conts;
using KafkaWeb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;

namespace Sample.ServerSide.Services
{
    public class Silo : IHostedService
    {
        private IHostBuilder host = new HostBuilder()
            // .ConfigureAppConfiguration(builder => { builder.AddCommandLine(args); })
            .ConfigureLogging(builder =>
            {
                builder.AddConsole();
                builder.AddFilter("Orleans.Runtime.Management.ManagementGrain", LogLevel.Warning);
                builder.AddFilter("Orleans.Runtime.SiloControl", LogLevel.Warning);
            })
            .ConfigureServices(services =>
            {
                services.Configure<ConsoleLifetimeOptions>(options => { options.SuppressStatusMessages = true; });
            })
            .UseOrleans(builder =>
            {
                builder.ConfigureApplicationParts(manager =>
                    manager.AddApplicationPart(typeof(IConsumerGrain).Assembly).WithReferences());

                builder.UseLocalhostClustering();
                builder.AddMemoryGrainStorageAsDefault();
                builder.AddSimpleMessageStreamProvider(StreamProvider.OutputStream);
                builder.AddMemoryGrainStorage(Storage.StatusStorage);
            });


        public Task StartAsync(CancellationToken cancellationToken)
        {
            return host.RunConsoleAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}