using System;
using System.Text.Json;

namespace KafkaWeb.Grains
{

    public class ConsumerSettings
    {         
        public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        public string Topic { get; set; } = "il1-prod-UserManagementNotificationsPartitioned";
        public string ToTopic { get; set; } =  "cl-502736321467";
        public string Group { get; set; } =  "lovely-group-2";
        public string SiteId { get; set; } =  "502736321467_";
        public string BootstrapServers { get; set; } = "il1a-kfk4-br1:9092,il1a-kfk4-br2:9092";

  
    }
}