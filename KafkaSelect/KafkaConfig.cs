// using System;
// using Orleans.Hosting;
//
// namespace KafkaSelect
// {
//     public class KafkaStreamConfiguration : ISiloBuilderConfigure
//     {
//         private readonly Func<DiscoveryConfig> _discovery;
//         private readonly IEnvironment _environment;
//
//         public KafkaStreamConfiguration(Func<DiscoveryConfig> discovery, IEnvironment environment)
//         {
//             _discovery = discovery;
//             _environment = environment;
//         }
//
//         public void Configure(ISiloHostBuilder hostBuilder)
//         {
//
//             hostBuilder.ConfigureContainer<ISiloHostBuilder>((context, builder) =>
//                 builder
//                     .AddMemoryGrainStorage("PubSubStore")
//                 
//                     .AddKafka(StreamConventions.StreamProvider)
//                     .WithOptions(options =>
//                     {
//                         options.BrokerList = _discovery().Services["KafkaBrokersMain"].Hosts.Split(',');
//                         options.ConsumerGroupId = $"{StreamConventions.CompareResponseConsumer}.{_environment.Zone}.{_environment.DeploymentEnvironment}";
//                         options.ConsumeMode = ConsumeMode.StreamEnd;
//                         options.AddTopic(StreamConventions.MainResponseNamespace);
//                     
//                         // context.Configuration.GetSection("KafkaStreamOptions").Bind(options);
//  
//                     })
//                     .AddJson()
//                     .AddLoggingTracker()
//                     .Build() 
//             );
//             
//             
//
//         }
//     }
//
// }