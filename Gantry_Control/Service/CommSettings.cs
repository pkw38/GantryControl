using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Gantry_Control.Service
{
    internal class CommSettings
    {
        public const string FileName = "commsettings.json";

        public string Ip { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 8999;

        public static CommSettings Load()
        {
            var path = Path.Combine(AppContext.BaseDirectory, FileName);
            try
            {
                if (File.Exists(path))
                {
                    var settings = JsonSerializer.Deserialize<CommSettings>(File.ReadAllText(path),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (settings != null)
                    {
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CommSettings] Load failed, using defaults: {ex.Message}");
            }

            return new CommSettings();
        }
    }
}
