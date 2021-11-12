// using System;
// using System.Collections.Generic;
// using System.Text;
// using System.Text.Json;
// using System.Threading;
// using System.Threading.Tasks;
// using Confluent.Kafka;
// using Microsoft.Extensions.Hosting;
//
// namespace KafkaSelect
// {
//     public    class TopicMessage: ConsumeResult<string,string>
//     {
//         
//     }
//     public class  KafkaConsumer:IHostedService
//     {
//         
//         public List<TopicMessage> Events = new List<TopicMessage>();
//
//         public Task StartAsync(CancellationToken cancellationToken)
//         {
//             // Configure the client with credentials for connecting to Confluent.
//             // Don't do this in production code. For more information, see 
//             // https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets.
//             var clientConfig = new ClientConfig();
//             clientConfig.BootstrapServers="il1a-kfk4-br1:9092,il1a-kfk4-br2:9092";
//             // clientConfig.SecurityProtocol=Confluent.Kafka.SecurityProtocol.Ssl;
//             // clientConfig.SaslMechanism=Confluent.Kafka.SaslMechanism.Plain;
//             // clientConfig.SaslUsername="<api-key>";
//             // clientConfig.SaslPassword="<api-secret>";
//             // clientConfig.SslCaLocation = "probe"; // /etc/ssl/certs
//             // await Produce("recent_changes", clientConfig);
//              Consume("il1-prod-UserManagementNotificationsPartitioned", clientConfig);
//  
//              return Task.CompletedTask;
//         }
//
//         public Task StopAsync(CancellationToken cancellationToken)
//         {
//             Console.WriteLine("Exiting");
//             return Task.CompletedTask;
//
//         }
//         
//         
//         void Consume(string topicName, ClientConfig config)
// {
//     Console.WriteLine($"{nameof(Consume)} starting");
//  
//     // Configure the consumer group based on the provided configuration. 
//     var consumerConfig = new ConsumerConfig(config);
//     consumerConfig.GroupId = "c-group";
//     // The offset to start reading from if there are no committed offsets (or there was an error in retrieving offsets).
//     consumerConfig.AutoOffsetReset = AutoOffsetReset.Earliest;
//     // Do not commit offsets.
//     consumerConfig.EnableAutoCommit = false;
//     // Enable canceling the Consume loop with Ctrl+C.
//     CancellationTokenSource cts = new CancellationTokenSource();
//     Console.CancelKeyPress += (_, e) => {
//         e.Cancel = true; // prevent the process from terminating.
//         cts.Cancel();
//     };
//  
//     // Build a consumer that uses the provided configuration.
//     using (var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build())
//     {
//         // Subscribe to events from the topic.
//         consumer.Subscribe(topicName);
//         try
//         {
//             // Run until the terminal receives Ctrl+C. 
//             while (true)
//             {
//                 // Consume and deserialize the next message.
//                 var cr = consumer.Consume(cts.Token);
//                 var key = cr.Message.Key; 
//
//
//                 Console.WriteLine($"Consumed record with value {cr.Message.Value}");
//                 if (cr.Message.Value.Contains("SiteId\":502736321467"))
//                 {
//                     // Parse the JSON to extract the URI of the edited page.
//                     // var jsonDoc = JsonDocument.Parse(cr.Message.Value);
//                     Events.Add(cr);
//                     // For consuming from the recent_changes topic. 
//                     // var metaElement = jsonDoc.RootElement.GetProperty("meta");
//                     // var uriElement = metaElement.GetProperty("uri");
//                     // var uri = uriElement.GetString();
//                     // For consuming from the ksqlDB sink topic.
//                     // var editsElement = jsonDoc.RootElement.GetProperty("NUM_EDITS");
//                     // var edits = editsElement.GetInt32();
//                     // var uri = $"{cr.Message.Key}, edits = {edits}";
//                     Console.WriteLine($"Consumed record with jsonDoc {cr}");
//                 }
//             }
//         }
//         catch (OperationCanceledException)
//         {
//             // Ctrl+C was pressed.
//             Console.WriteLine($"Ctrl+C pressed, consumer exiting");
//         }
//         finally
//         {
//             consumer.Close();
//         }
//     }
// }     
//     }
// }