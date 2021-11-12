using Gigya.LiveTesting.Grains.Conts;
using KafkaWeb;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;

await Host.CreateDefaultBuilder(args)
    .UseOrleans(builder =>
    {
        builder.UseLocalhostClustering();
        builder.AddMemoryGrainStorageAsDefault();
        builder.AddSimpleMessageStreamProvider(StreamProvider.OutputStream);
        builder.AddMemoryGrainStorage( Storage.StatusStorage);
        builder.ConfigureLogging(builder =>
        {
            builder.AddConsole();
            builder.AddFilter("Orleans.Runtime.Management.ManagementGrain", LogLevel.Warning);
            builder.AddFilter("Orleans.Runtime.SiloControl", LogLevel.Warning);
        });
        builder.ConfigureApplicationParts(manager =>
            manager.AddApplicationPart(typeof(IConsumerGrain).Assembly).WithReferences());
        builder.ConfigureApplicationParts(manager =>
            manager.AddApplicationPart(typeof(ConsumerGrain).Assembly).WithReferences());
    })
    .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
    .RunConsoleAsync();