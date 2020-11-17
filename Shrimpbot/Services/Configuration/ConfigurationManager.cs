using Newtonsoft.Json;
using System.IO;

namespace Shrimpbot.Services.Configuration
{

    static class ConfigurationManager
    {
        public static string ConfigurationPath;
        static ConfigurationManager()
        {
            ConfigurationPath = Path.Combine(Directory.GetCurrentDirectory(), "Configuration");
        }
        public static ConfigurationFile Read()
        {
            if (!File.Exists(Path.Combine(ConfigurationPath, "config.json")))
            {
                Write(new ConfigurationFile());
            }
            using StreamReader file = File.OpenText(Path.Combine(ConfigurationPath, "config.json"));
            JsonSerializer jsonSerializer = new JsonSerializer();
            return (ConfigurationFile)jsonSerializer.Deserialize(file, typeof(ConfigurationFile));
        }
        public static void Write(ConfigurationFile config)
        {
            if (!Directory.Exists(ConfigurationPath)) Directory.CreateDirectory(ConfigurationPath);
            using (StreamWriter file = File.CreateText(Path.Combine(ConfigurationPath, "config.json")))
            {
                new JsonSerializer().Serialize(file, config);
            }
        }
    }

}
