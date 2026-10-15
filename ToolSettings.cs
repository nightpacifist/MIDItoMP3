using System.IO;
using System.Text.Json;

namespace MIDItoMP3
{
    /// <summary>
    /// Шляхи до зовнішніх програм, вказані користувачем вручну, і мова інтерфейсу.
    /// Зберігаються в %AppData%\MIDItoMP3\settings.json.
    /// </summary>
    public class ToolSettings
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MIDItoMP3", "settings.json");

        public Dictionary<string, string> ToolPaths { get; set; } = new();

        /// <summary>"uk" або "en" (див. <see cref="Loc"/>).</summary>
        public string Language { get; set; } = Loc.Ukrainian;

        public static ToolSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                    return JsonSerializer.Deserialize<ToolSettings>(File.ReadAllText(SettingsPath)) ?? new ToolSettings();
            }
            catch
            {
                // пошкоджений файл налаштувань — починаємо з чистого
            }
            return new ToolSettings();
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
