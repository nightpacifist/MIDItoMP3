using System.IO;

namespace MIDItoMP3
{
    public enum ToolSource { Manual, SystemPath, Bundled, NotFound }

    /// <param name="Id">Ключ у файлі налаштувань.</param>
    /// <param name="BundledRelativePath">Де шукати програму поруч із MIDItoMP3.exe.</param>
    public record ExternalTool(string Id, string DisplayName, string ExeName, string BundledRelativePath);

    /// <summary>
    /// Пошук зовнішніх програм. Порядок: шлях, вказаний вручну → PATH → папка поруч із програмою.
    /// </summary>
    public static class ExternalTools
    {
        public static readonly ExternalTool FluidSynth = new("fluidsynth", "FluidSynth", "fluidsynth.exe", @"fluidsynth\bin\fluidsynth.exe");
        public static readonly ExternalTool Lame = new("lame", "LAME", "lame.exe", @"lame3\lame.exe");
        public static readonly ExternalTool Ffmpeg = new("ffmpeg", "FFmpeg", "ffmpeg.exe", @"ffmpeg\bin\ffmpeg.exe");

        public static readonly ExternalTool Ffprobe = new("ffprobe", "FFprobe", "ffprobe.exe", @"ffmpeg\bin\ffprobe.exe");

        public static readonly ExternalTool[] All = { FluidSynth, Lame, Ffmpeg, Ffprobe };

        public static ToolSettings Settings { get; } = ToolSettings.Load();

        public static string? GetManualPath(ExternalTool tool) =>
            Settings.ToolPaths.TryGetValue(tool.Id, out var path) ? path : null;

        public static void SetManualPath(ExternalTool tool, string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                Settings.ToolPaths.Remove(tool.Id);
            else
                Settings.ToolPaths[tool.Id] = path.Trim();
        }

        public static (string? Path, ToolSource Source) Resolve(ExternalTool tool)
        {
            var manual = GetManualPath(tool);
            if (manual != null && File.Exists(manual))
                return (manual, ToolSource.Manual);

            return ResolveAutomatic(tool);
        }

        /// <summary>Пошук без урахування ручних налаштувань.</summary>
        public static (string? Path, ToolSource Source) ResolveAutomatic(ExternalTool tool)
        {
            var fromPath = FindInPath(tool.ExeName);
            if (fromPath != null)
                return (fromPath, ToolSource.SystemPath);

            var bundled = Path.Combine(AppContext.BaseDirectory, tool.BundledRelativePath);
            if (File.Exists(bundled))
                return (bundled, ToolSource.Bundled);

            return (null, ToolSource.NotFound);
        }

        private static string? FindInPath(string exeName)
        {
            return PathDirectories()
                .Select(dir => Path.Combine(dir, exeName))
                .FirstOrDefault(File.Exists);
        }

        // PATH процесу фіксується при запуску, тому додатково читаємо актуальні
        // значення користувача й системи — щоб підхопити програму, встановлену під час роботи.
        private static IEnumerable<string> PathDirectories()
        {
            var targets = new[] { EnvironmentVariableTarget.Process, EnvironmentVariableTarget.User, EnvironmentVariableTarget.Machine };
            return targets
                .Select(t => Environment.GetEnvironmentVariable("PATH", t))
                .Where(v => !string.IsNullOrEmpty(v))
                .SelectMany(v => v!.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Select(d => Environment.ExpandEnvironmentVariables(d.Trim('"')))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }
}
