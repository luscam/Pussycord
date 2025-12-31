using System.Text.Json;

namespace Pussycord;

public class AppSettings
{
    public bool blockScience { get; set; } = true;
    public bool sandboxMode { get; set; } = true;
    public bool blockCamera { get; set; } = false;
    public bool blockMic { get; set; } = true;
}

public static class ConfigManager
{
    private static readonly string _path = @"C:\pcord\settings.json";
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = false };
    
    public static event Action<AppSettings> OnSettingsChanged;

    public static AppSettings Load()
    {
        if (!File.Exists(_path))
        {
            var defaultSettings = new AppSettings();
            Save(defaultSettings);
            return defaultSettings;
        }

        try
        {
            string json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            string json = JsonSerializer.Serialize(settings, _options);
            File.WriteAllText(_path, json);
            OnSettingsChanged?.Invoke(settings);
        }
        catch { }
    }

    public static void UpdateRaw(string json)
    {
        try
        {
            File.WriteAllText(_path, json);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            if (settings != null)
            {
                OnSettingsChanged?.Invoke(settings);
            }
        }
        catch { }
    }
}