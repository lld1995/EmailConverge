using System;
using System.IO;
using System.Text.Json;

namespace EmailConverge.Services
{
    public class AiConfig
    {
        public string Endpoint { get; set; } = "http://192.168.191.2:30010/v1";
        public string ApiKey { get; set; } = "none";
        public string Model { get; set; } = "qwen3-30b-nothinking-2";

        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EmailConverge",
            "aiconfig.json");

        public static AiConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    return JsonSerializer.Deserialize<AiConfig>(json) ?? new AiConfig();
                }
            }
            catch
            {
            }
            return new AiConfig();
        }

        public void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch
            {
            }
        }
    }
}
