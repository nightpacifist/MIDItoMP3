using System.ComponentModel;
using System.Windows.Data;

namespace MIDItoMP3
{
    /// <summary>
    /// Тексти інтерфейсу двома мовами. У XAML: {Binding [Ключ], Source={x:Static local:Loc.Instance}} —
    /// оновлюється одразу після перемикання мови. У коді: Loc.T("Ключ", аргументи…).
    /// </summary>
    public sealed class Loc : INotifyPropertyChanged
    {
        public const string Ukrainian = "uk";
        public const string English = "en";

        public static Loc Instance { get; } = new();

        private string _language = ExternalTools.Settings.Language == English ? English : Ukrainian;

        public string Language
        {
            get => _language;
            set
            {
                if (_language == value) return;
                _language = value;
                ExternalTools.Settings.Language = value;
                ExternalTools.Settings.Save();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Binding.IndexerName));
            }
        }

        public void Toggle() => Language = _language == Ukrainian ? English : Ukrainian;

        public string this[string key] => (_language == English ? En : Uk).TryGetValue(key, out var s) ? s : key;

        public static string T(string key, params object[] args) => args.Length == 0 ? Instance[key] : string.Format(Instance[key], args);

        public static string[] Phrases => Instance._language == English ? PhrasesEn : PhrasesUk;

        public event PropertyChangedEventHandler? PropertyChanged;

        private static readonly Dictionary<string, string> Uk = new()
        {
            ["MainTitle"] = "MIDI → MP3, MOV → MP4 Конвертер",
            ["SelectFile"] = "Вибрати файл",
            ["ConvertMidi"] = "Конвертувати MIDI в MP3",
            ["MovCompatible"] = "MOV → MP4 сумісний",
            ["MovCompatibleHint"] = "відкриється будь-де",
            ["MovCompatibleTip"] = "Відео стискається заново у звичайний формат. HDR з iPhone перетворюється у звичайні кольори. Довше, але файл відкриється на будь-якому пристрої.",
            ["MovOriginal"] = "MOV → MP4 оригінал",
            ["MovOriginalHint"] = "швидко, без втрат якості",
            ["MovOriginalTip"] = "Відео не змінюється — лише перекладається з MOV у MP4. Кольори, HDR і якість як в оригіналі, за кілька секунд. Але HEVC/HDR відкривають не всі програми.",
            ["ToolPathsButton"] = "Шляхи до програм…",
            ["SwitchLanguage"] = "English",
            ["SwitchLanguageTip"] = "Switch to English",
            ["FileFilter"] = "MIDI-файли (*.mid;*.midi)|*.mid;*.midi|Відео (*.mov)|*.mov",
            ["StatusConverting"] = "Конвертую…",
            ["StatusDone"] = "Вже готово!",
            ["StatusFailed"] = "Не вдалося конвертувати",
            ["ConversionErrorTitle"] = "Помилка конвертації",
            ["NoFileText"] = "Спочатку виберіть файл.",
            ["NoFileTitle"] = "Немає файлу",
            ["WrongFileText"] = "Вибраний файл не є {0}-файлом.",
            ["WrongFileTitle"] = "Неправильний файл",
            ["ToolMissingText"] = "Не знайдено {0} ({1}): програми немає ні в PATH, ні поруч із MIDItoMP3.\n\nВказати шлях вручну?",
            ["ToolMissingTitle"] = "Програму не знайдено",

            ["ToolsTitle"] = "Шляхи до програм",
            ["ToolsIntro"] = "Залиште поле порожнім, щоб програма шукалася автоматично: спершу в PATH, потім поруч із MIDItoMP3.exe.",
            ["Browse"] = "Огляд…",
            ["Automatic"] = "Автоматично",
            ["Cancel"] = "Скасувати",
            ["Save"] = "Зберегти",
            ["BrowseTitle"] = "Вкажіть шлях до {0}",
            ["ExeFilter"] = "Програми (*.exe)",
            ["PathMissingText"] = "Файл не існує:\n{0}",
            ["PathMissingTitle"] = "Неправильний шлях",
            ["ToolManual"] = "Використовується вказаний шлях",
            ["ToolManualMissing"] = "Файл за вказаним шляхом не існує",
            ["ToolInPath"] = "Знайдено в PATH: {0}",
            ["ToolBundled"] = "Знайдено поруч із програмою: {0}",
            ["ToolNotFound"] = "Не знайдено. Вкажіть шлях вручну.",

            ["UnhandledErrorTitle"] = "Невловлена помилка",
            ["StartupErrorTitle"] = "Помилка запуску",

            ["MidiNotFound"] = "MIDI не знайдено",
            ["SoundFontNotFound"] = "SoundFont не знайдено",
            ["WavFailed"] = "Не вдалося згенерувати WAV із fluidsynth.",
            ["VideoNotFound"] = "Відео не знайдено",
            ["NoVideoStream"] = "У файлі немає відеодоріжки.",
            ["ProcessStartFailed"] = "Не вдалося запустити процес: {0}",
            ["ProcessFailed"] = "Процес {0} завершився з кодом {1}.",
        };

        private static readonly Dictionary<string, string> En = new()
        {
            ["MainTitle"] = "MIDI → MP3, MOV → MP4 Converter",
            ["SelectFile"] = "Choose file",
            ["ConvertMidi"] = "Convert MIDI to MP3",
            ["MovCompatible"] = "MOV → MP4 compatible",
            ["MovCompatibleHint"] = "plays anywhere",
            ["MovCompatibleTip"] = "The video is re-encoded into a standard format. iPhone HDR is converted to normal colors. Slower, but the file plays on any device.",
            ["MovOriginal"] = "MOV → MP4 original",
            ["MovOriginalHint"] = "fast, no quality loss",
            ["MovOriginalTip"] = "The video is left untouched — only moved from MOV to MP4. Colors, HDR and quality stay as in the original, in a few seconds. But not every app can play HEVC/HDR.",
            ["ToolPathsButton"] = "Program paths…",
            ["SwitchLanguage"] = "Українська",
            ["SwitchLanguageTip"] = "Перемкнути на українську",
            ["FileFilter"] = "MIDI files (*.mid;*.midi)|*.mid;*.midi|Video (*.mov)|*.mov",
            ["StatusConverting"] = "Converting…",
            ["StatusDone"] = "All done!",
            ["StatusFailed"] = "Conversion failed",
            ["ConversionErrorTitle"] = "Conversion error",
            ["NoFileText"] = "Choose a file first.",
            ["NoFileTitle"] = "No file",
            ["WrongFileText"] = "The selected file is not a {0} file.",
            ["WrongFileTitle"] = "Wrong file",
            ["ToolMissingText"] = "{0} ({1}) was not found: it is neither in PATH nor next to MIDItoMP3.\n\nSpecify the path manually?",
            ["ToolMissingTitle"] = "Program not found",

            ["ToolsTitle"] = "Program paths",
            ["ToolsIntro"] = "Leave a field empty to find the program automatically: first in PATH, then next to MIDItoMP3.exe.",
            ["Browse"] = "Browse…",
            ["Automatic"] = "Automatic",
            ["Cancel"] = "Cancel",
            ["Save"] = "Save",
            ["BrowseTitle"] = "Locate {0}",
            ["ExeFilter"] = "Programs (*.exe)",
            ["PathMissingText"] = "File does not exist:\n{0}",
            ["PathMissingTitle"] = "Invalid path",
            ["ToolManual"] = "Using the specified path",
            ["ToolManualMissing"] = "No file at the specified path",
            ["ToolInPath"] = "Found in PATH: {0}",
            ["ToolBundled"] = "Found next to the app: {0}",
            ["ToolNotFound"] = "Not found. Specify the path manually.",

            ["UnhandledErrorTitle"] = "Unhandled error",
            ["StartupErrorTitle"] = "Startup error",

            ["MidiNotFound"] = "MIDI file not found",
            ["SoundFontNotFound"] = "SoundFont not found",
            ["WavFailed"] = "fluidsynth failed to produce a WAV file.",
            ["VideoNotFound"] = "Video not found",
            ["NoVideoStream"] = "The file has no video track.",
            ["ProcessStartFailed"] = "Failed to start process: {0}",
            ["ProcessFailed"] = "Process {0} exited with code {1}.",
        };

        private static readonly string[] PhrasesUk =
        {
            "Твоя музика сьогодні зробить світ кращим",
            "Звук — це твоє полотно, малюй сміливо",
            "Ніхто не відчує ритм так, як ти",
            "Маленький акорд — велика емоція",
            "Навіть тиша має ритм — просто слухай",
            "Якщо звучить дивно, можливо, це вже новий стиль",
            "Не існує фальшивих нот, є тільки експерименти",
            "Кожен трек — це ще один крок до шедевру",
            "Мікшуй, поки не відчуєш магію",
            "Руки втомились? Вуха ще працюють",
            "Музика — це енергія, не математика",
            "Якщо не звучить, просто додай басу",
            "Один день без звуку — втрачений день",
            "Твій ритм — це твоя підписка на вічність",
            "Навіть Моцарт колись не попадав у темп",
            "Не шукай натхнення — включи метроном",
            "Вчора ти слухав, сьогодні твориш",
            "Аранжування — це розмова без слів",
            "Не бійся тиші, вона підкреслює ноти",
            "Гармонія — це коли хаос звучить красиво",
            "Поки інші мовчать, ти створюєш",
            "Якщо сумніваєшся — додай ще один шар звуку",
            "Твоя музика говорить гучніше, ніж ти",
            "Кожен проєкт — це маленький концерт",
            "Сьогоднішній рендер — завтрашній шедевр",
            "Музика — це коли серце натискає «Record»",
            "Ти не просто компонуєш — ти керуєш емоціями",
            "Якщо немає настрою, створи його сама",
            "Музика — це хімія душі й звуку",
            "Не шукай ідеального семплу, створи його",
            "Кожен новий трек — це нова версія тебе",
            "Звуки не помиляються, вони просто шукають місце",
            "Зроби, щоб навіть тиша аплодувала",
            "Пам’ятай: MIDI — лише початок магії",
            "Натхнення не спізнюється, воно прогрівається",
            "Не порівнюй себе з іншими — порівнюй з учорашньою собою",
            "Твій слух — це інструмент, бережи його",
            "Не бійся зламати правила — музика їх не має",
            "Якщо все звучить занадто чисто — додай трохи душі",
            "Навіть хаос має свій BPM",
            "Хтось колись почує це і посміхнеться",
            "Головне — щоб спочатку сподобалося тобі",
            "Ти не просто музикант — ти творець настрою Всесвіту"
        };

        // Той самий порядок, що й у PhrasesUk: при перемиканні мови показується переклад тієї ж фрази
        private static readonly string[] PhrasesEn =
        {
            "Your music will make the world better today",
            "Sound is your canvas — paint boldly",
            "Nobody feels the rhythm like you do",
            "A small chord, a big emotion",
            "Even silence has a rhythm — just listen",
            "If it sounds weird, maybe it's already a new style",
            "There are no wrong notes, only experiments",
            "Every track is one more step toward a masterpiece",
            "Keep mixing until you feel the magic",
            "Tired hands? Your ears still work",
            "Music is energy, not math",
            "If it doesn't sound right, just add more bass",
            "A day without sound is a day lost",
            "Your rhythm is your subscription to eternity",
            "Even Mozart was off-beat once",
            "Don't wait for inspiration — turn on the metronome",
            "Yesterday you listened, today you create",
            "Arranging is a conversation without words",
            "Don't fear silence, it makes the notes stand out",
            "Harmony is when chaos sounds beautiful",
            "While others stay silent, you create",
            "When in doubt, add one more layer of sound",
            "Your music speaks louder than you do",
            "Every project is a little concert",
            "Today's render is tomorrow's masterpiece",
            "Music is when your heart presses \"Record\"",
            "You don't just compose — you conduct emotions",
            "No mood? Create one yourself",
            "Music is the chemistry of soul and sound",
            "Don't search for the perfect sample, make it",
            "Every new track is a new version of you",
            "Sounds don't make mistakes, they're just finding their place",
            "Make even the silence applaud",
            "Remember: MIDI is only the beginning of the magic",
            "Inspiration is never late, it's just warming up",
            "Don't compare yourself to others — compare yourself to who you were yesterday",
            "Your hearing is an instrument, take care of it",
            "Don't be afraid to break the rules — music has none",
            "If everything sounds too clean, add a bit of soul",
            "Even chaos has its own BPM",
            "Someday someone will hear this and smile",
            "What matters most is that you like it first",
            "You're not just a musician — you're the creator of the Universe's mood"
        };
    }
}
