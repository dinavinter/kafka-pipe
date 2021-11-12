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
        public int IntervalLimit = 500;

        public string Topic =  "il1-prod-UserManagementNotificationsPartitioned";
        public string Group =  "lovely-group";
        public string SiteId =  "lovely-group";
        public Guid JobId = Guid.NewGuid();
        public string BootstrapServers = "il1a-kfk4-br1:9092,il1a-kfk4-br2:9092";

        public string GetGrainKey() => JsonSerializer.Serialize(this, SerializerOptions);

        public ulong HashCode() => 
            (ulong) JsonSerializer.Serialize(this).GetHashCode();

        public static string DefaultGrainKey() => _defaultGrainKey.Value;
        private static readonly Lazy<string> _defaultGrainKey = new Lazy<string>(() => JsonSerializer.Serialize(new ConsumerSettings(), SerializerOptions));

        public static ConsumerSettings GetFromGrainKey(string key) => 
            JsonSerializer.Deserialize<ConsumerSettings>(key, SerializerOptions);
    }
}