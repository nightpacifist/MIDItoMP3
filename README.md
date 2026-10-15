# MIDItoMP3

**[Українська](#українська) · [English](#english)**

---

## Українська

Невелика програма для Windows, яка конвертує:

- **MIDI → MP3** (`.mid`, `.midi`) — мелодія озвучується вбудованим SoundFont FluidR3_GM;
- **MOV → MP4** (`.mov`, наприклад відео з iPhone) — у двох режимах на вибір.

Інтерфейс українською та англійською, мова перемикається однією кнопкою.

### Вимоги

- Windows 10 або 11 (64-біт).
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) — якщо програма не запускається і просить встановити .NET, завантажте саме **.NET Desktop Runtime 8.x (x64)**.
- Зовнішні програми, якими користується конвертер:

| Для чого | Програма | Де взяти |
|---|---|---|
| MIDI → MP3 | FluidSynth (`fluidsynth.exe`) | [github.com/FluidSynth/fluidsynth/releases](https://github.com/FluidSynth/fluidsynth/releases) |
| MIDI → MP3 | LAME (`lame.exe`) | [rarewares.org/mp3-lame-bundle.php](https://www.rarewares.org/mp3-lame-bundle.php) |
| MOV → MP4 | FFmpeg (`ffmpeg.exe` і `ffprobe.exe`) | [gyan.dev/ffmpeg/builds](https://www.gyan.dev/ffmpeg/builds/) або `winget install Gyan.FFmpeg` |

Якщо вам потрібна лише одна з конвертацій, достатньо програм тільки для неї.

### Встановлення та запуск

1. Завантажте архів з останнього релізу на сторінці [Releases](../../releases). У ньому сама програма та SoundFont; FluidSynth, LAME і FFmpeg до архіву **не входять** — встановіть їх за посиланнями з таблиці вище.
2. Розпакуйте архів у будь-яку папку (наприклад, `C:\Programs\MIDItoMP3`).
3. Запустіть `MIDItoMP3.exe`. Якщо Windows покаже «Windows захистила ваш комп'ютер», натисніть **«Докладніше» → «Виконати однаково»** (програма не має цифрового підпису).

Програма шукає FluidSynth, LAME і FFmpeg у такому порядку:

1. Шлях, який ви вказали вручну (кнопка **«Шляхи до програм…»**).
2. Системна змінна `PATH`.
3. Папки поруч із `MIDItoMP3.exe`:
   ```
   MIDItoMP3.exe
   fluidsynth\bin\fluidsynth.exe
   lame3\lame.exe
   ffmpeg\bin\ffmpeg.exe
   ffmpeg\bin\ffprobe.exe
   ```

Якщо програму не знайдено, під час конвертації з'явиться запит — можна одразу вказати, де лежить потрібний `.exe`, і шлях запам'ятається.

### Як користуватися

1. Виберіть файл кнопкою **«Вибрати файл»** або просто перетягніть його у вікно програми.
2. Натисніть потрібну кнопку:
   - **«Конвертувати MIDI в MP3»** — для `.mid` / `.midi`.
   - **«MOV → MP4 сумісний»** — відео стискається заново у звичайний формат H.264. HDR з iPhone перетворюється у звичайні кольори. Довше, але файл відкриється на будь-якому пристрої.
   - **«MOV → MP4 оригінал»** — відео не змінюється, лише перекладається з MOV у MP4. Кольори, HDR і якість як в оригіналі, і все за кілька секунд. Але HEVC/HDR відкривають не всі програми.
3. Дочекайтеся напису **«Вже готово!»**.

Готовий файл з'являється **в тій самій папці**, що й вихідний. До його назви додаються дата й час конвертації, а розширення стає `.mp3` або `.mp4`. Наприклад, `song.mid` → `song_2026-10-15_14-30-05.mp3`.

> 💡 Попередні результати ніколи не перезаписуються. Якщо файл із такою назвою вже існує (наприклад, дві конвертації за одну секунду), до назви додається `_2`, `_3` тощо.

### Налаштування

- **«Шляхи до програм…»** — вікно, де можна вказати шлях до кожної програми вручну. Порожнє поле означає автоматичний пошук.
- Кнопка **English / Українська** перемикає мову інтерфейсу.
- Налаштування зберігаються у `%AppData%\MIDItoMP3\settings.json`.

### Якщо щось пішло не так

- **«Програму не знайдено»** — встановіть відповідну програму (див. таблицю вище) або вкажіть шлях до її `.exe` вручну.
- **«Помилка конвертації»** — у вікні помилки буде текст від FluidSynth / LAME / FFmpeg; найчастіше причина — пошкоджений або непідтримуваний вхідний файл.
- Якщо програма аварійно закрилася, у папці `%AppData%\MIDItoMP3\` може з'явитися файл `unhandled_error.log` або `startup_error.log` — додайте його до [issue](../../issues).

### Збирання з вихідного коду

Потрібен [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```
dotnet build -c Release MIDItoMP3.sln
dotnet run --project MIDItoMP3.csproj
```

SoundFont `FluidR3_GM.sf2` (142 МБ) не зберігається в репозиторії — візьміть його з архіву будь-якого релізу й покладіть у `FluidR3_GM\` перед збиранням.

Зовнішні програми до збірки не додаються — встановіть їх або скопіюйте у папку з `MIDItoMP3.exe` за схемою вище.

### Подяки

- [FluidSynth](https://www.fluidsynth.org/), [LAME](https://lame.sourceforge.io/), [FFmpeg](https://ffmpeg.org/)
- SoundFont **Fluid (R3)** © Frank Wen, ліцензія MIT (див. `FluidR3_GM\README.txt`)
- [Material Design In XAML](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)

### Ліцензія

[MIT](LICENSE). FluidSynth, LAME і FFmpeg — окремі проєкти зі своїми ліцензіями; ця програма їх не містить, а лише запускає.

---

## English

A small Windows app that converts:

- **MIDI → MP3** (`.mid`, `.midi`) — the music is rendered with the bundled FluidR3_GM SoundFont;
- **MOV → MP4** (`.mov`, e.g. iPhone videos) — in two modes of your choice.

The interface is available in Ukrainian and English; switch with a single button.

### Requirements

- Windows 10 or 11 (64-bit).
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) — if the app does not start and asks you to install .NET, download **.NET Desktop Runtime 8.x (x64)**.
- External programs used for conversion:

| Used for | Program | Where to get it |
|---|---|---|
| MIDI → MP3 | FluidSynth (`fluidsynth.exe`) | [github.com/FluidSynth/fluidsynth/releases](https://github.com/FluidSynth/fluidsynth/releases) |
| MIDI → MP3 | LAME (`lame.exe`) | [rarewares.org/mp3-lame-bundle.php](https://www.rarewares.org/mp3-lame-bundle.php) |
| MOV → MP4 | FFmpeg (`ffmpeg.exe` and `ffprobe.exe`) | [gyan.dev/ffmpeg/builds](https://www.gyan.dev/ffmpeg/builds/) or `winget install Gyan.FFmpeg` |

If you only need one kind of conversion, you only need the programs for it.

### Installation and launch

1. Download the archive from the latest release on the [Releases](../../releases) page. It contains the app and the SoundFont; FluidSynth, LAME and FFmpeg are **not** included — install them using the links in the table above.
2. Extract the archive to any folder (e.g. `C:\Programs\MIDItoMP3`).
3. Run `MIDItoMP3.exe`. If Windows shows "Windows protected your PC", click **"More info" → "Run anyway"** (the app is not digitally signed).

The app looks for FluidSynth, LAME and FFmpeg in this order:

1. A path you set manually (the **"Program paths…"** button).
2. The system `PATH` variable.
3. Folders next to `MIDItoMP3.exe`:
   ```
   MIDItoMP3.exe
   fluidsynth\bin\fluidsynth.exe
   lame3\lame.exe
   ffmpeg\bin\ffmpeg.exe
   ffmpeg\bin\ffprobe.exe
   ```

If a program is not found, the app asks you during conversion — you can point it to the right `.exe` right away, and the path is remembered.

### How to use

1. Choose a file with **"Choose file"** or simply drag it onto the window.
2. Click the button you need:
   - **"Convert MIDI to MP3"** — for `.mid` / `.midi`.
   - **"MOV → MP4 compatible"** — the video is re-encoded to standard H.264. iPhone HDR is converted to normal colors. Slower, but the file plays on any device.
   - **"MOV → MP4 original"** — the video is left untouched, only moved from MOV to MP4. Colors, HDR and quality stay as in the original, in a few seconds. But not every app can play HEVC/HDR.
3. Wait for **"All done!"**.

The result appears **in the same folder** as the source file. The conversion date and time are added to its name, and the extension becomes `.mp3` or `.mp4`. For example, `song.mid` → `song_2026-10-15_14-30-05.mp3`.

> 💡 Earlier results are never overwritten. If a file with that name already exists (e.g. two conversions within the same second), `_2`, `_3`, etc. is appended to the name.

### Settings

- **"Program paths…"** — a window where you can set the path to each program manually. An empty field means automatic search.
- The **Українська / English** button switches the interface language.
- Settings are stored in `%AppData%\MIDItoMP3\settings.json`.

### Troubleshooting

- **"Program not found"** — install the program (see the table above) or set the path to its `.exe` manually.
- **"Conversion error"** — the error window shows the output from FluidSynth / LAME / FFmpeg; most often the input file is damaged or unsupported.
- If the app crashes, an `unhandled_error.log` or `startup_error.log` file may appear in `%AppData%\MIDItoMP3\` — please attach it to an [issue](../../issues).

### Building from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```
dotnet build -c Release MIDItoMP3.sln
dotnet run --project MIDItoMP3.csproj
```

The `FluidR3_GM.sf2` SoundFont (142 MB) is not stored in the repository — take it from any release archive and put it into `FluidR3_GM\` before building.

External programs are not included in the build — install them or copy them next to `MIDItoMP3.exe` using the layout above.

### Credits

- [FluidSynth](https://www.fluidsynth.org/), [LAME](https://lame.sourceforge.io/), [FFmpeg](https://ffmpeg.org/)
- **Fluid (R3)** SoundFont © Frank Wen, MIT license (see `FluidR3_GM\README.txt`)
- [Material Design In XAML](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)

### License

[MIT](LICENSE). FluidSynth, LAME and FFmpeg are separate projects with their own licenses; this app does not include them, it only runs them.
