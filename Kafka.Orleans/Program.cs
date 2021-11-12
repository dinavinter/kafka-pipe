using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gigya.LiveTesting.Grains.Conts;
using KafkaWeb;
using KafkaWeb.Grains;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans;
using Orleans.Hosting;

namespace Kafka.Orleans
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new HostBuilder()
                .ConfigureAppConfiguration(builder => { builder.AddCommandLine(args); })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddFilter("Orleans.Runtime.Management.ManagementGrain", LogLevel.Warning);
                    builder.AddFilter("Orleans.Runtime.SiloControl", LogLevel.Warning);
                })
                .ConfigureServices(services =>
                {
                    services.AddOptions<ConsumerSettings>();
                    services.Configure<ConsoleLifetimeOptions>(
                        options => { options.SuppressStatusMessages = true; });
                })
                .UseOrleans(builder =>
                {
                    builder.ConfigureApplicationParts(manager =>
                        manager.AddApplicationPart(typeof(ConsumerGrain).Assembly).WithReferences());

                    builder.UseLocalhostClustering();
                    builder.AddMemoryGrainStorageAsDefault();
                    builder.AddSimpleMessageStreamProvider(StreamProvider.OutputStream);
                    builder.AddMemoryGrainStorage(Storage.StatusStorage);
                }).RunConsoleAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}