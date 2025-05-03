using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RconBroadcast {
    public static class ConfigManager {
        private static string FilePath {
            get {
                return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "./", "rcon-config.json");
            }
        }

        private static readonly JsonSerializerOptions _SerializerOptions = new() { WriteIndented = true };
        public static async Task SaveConfigAsync(RconConfig config) {
            var json = JsonSerializer.Serialize(config, _SerializerOptions);
            await File.WriteAllTextAsync(FilePath, json);
        }

        public static async Task<RconConfig> LoadConfigAsync() {
            if(!File.Exists(FilePath)) return new RconConfig();
            var json = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<RconConfig>(json) ?? new RconConfig();
        }
    }

    public static class IntervalParser {
        public static TimeSpan ParseInterval(string interval) {
            if(string.IsNullOrWhiteSpace(interval)) {
                return TimeSpan.Zero;
            }

            if(TimeSpan.TryParse(interval.Replace("m", ":00").Replace("h", ":00:00"), out TimeSpan result)) {
                return result;
            }

            return interval.EndsWith('s') && int.TryParse(interval[..^1], out int seconds)
                ? TimeSpan.FromSeconds(seconds)
                : throw new FormatException($"Invalid interval format: {interval}");
        }
    }

    public class RconConfig {
        public ServerConfig[] Servers { get; set; } = [];
        public string ReconnectInterval { get; set; } = "5m";

        [JsonIgnore]
        public TimeSpan ParsedInterval => IntervalParser.ParseInterval(ReconnectInterval);
    }

    public class ServerConfig {
        public string ServerIp { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = 25575;
        public string Password { get; set; } = string.Empty;
        public CommandConfig[] Commands { get; set; } = [];
    }

    public class CommandConfig {
        public string Command { get; set; } = string.Empty;
        public string Interval { get; set; } = string.Empty;

        [JsonIgnore]
        public TimeSpan ParsedInterval => IntervalParser.ParseInterval(Interval);
    }
}