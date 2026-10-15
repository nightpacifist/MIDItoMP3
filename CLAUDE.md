# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Small Windows desktop app (WPF, .NET 8, `net8.0-windows`) that converts MIDI → MP3 and MOV → MP4 by shelling out to external command-line tools. UI uses MaterialDesignThemes (DeepPurple/Lime, MaterialDesign2 defaults). The UI is bilingual (**Ukrainian** default, English) with a toggle button in the main window. All user-facing strings and exception messages live in `Loc.cs` (two dictionaries plus the motivational phrases, in matching order) — never hardcode UI text; add every new string to **both** dictionaries. XAML binds via `{Binding [Key], Source={x:Static local:Loc.Instance}}` (updates live on switch); code uses `Loc.T("Key", args…)`. The chosen language is saved in `settings.json`. Code comments stay in Ukrainian.

## Commands

```
dotnet build MIDItoMP3.sln            # Debug build
dotnet build -c Release MIDItoMP3.sln
dotnet run --project MIDItoMP3.csproj
```

There are no tests and no linter configured.

## Architecture

- `MainWindow.xaml(.cs)` — single window, code-behind event handlers (file picker, drag-and-drop onto the window, two convert buttons, "Шляхи до програм…" button). Handlers validate the input file, resolve tools via `EnsureTool` (prompts for a path if a tool isn't found), then run conversion through `RunConversion`, which does `await Task.Run(...)` and toggles `ViewModel.IsBusy` (buttons bind to `IsIdle`). Conversion errors are caught there and shown in a MessageBox.
- `MainViewModel.cs` — `FilePath` (selected input) is separate from `Status` (label text: file name, random motivational phrase at startup, converting/done). `Status` is stored as a `Func<string>` (`SetStatus`) so it re-translates when the language changes. Output paths are built in the handlers by `MakeOutputPath`: input name + `_yyyy-MM-dd_HH-mm-ss` timestamp + new extension (with `_2`, `_3`… on collision), so earlier results are never overwritten; output lands next to the input.
- `ConvertFile.cs` — blocking conversion logic; takes tool exe paths as parameters. Call it off the UI thread.
  - MIDI → MP3: `fluidsynth` renders to a temp WAV (44.1 kHz, SoundFont at `AppContext.BaseDirectory\FluidR3_GM\FluidR3_GM.sf2`), then `lame --preset standard`; temp WAV deleted in `finally`.
  - MOV → MP4: first `VideoInfo.Probe` (ffprobe) reads codec/color tags, then one of two modes (`Mp4Mode`): **Compatible** — libx264 SDR BT.709 with explicit color tags; HDR (HLG/PQ from iPhone) is tone-mapped via `zscale`+`tonemap=hable` (the first zscale needs `m=gbr`, else zimg "no path between colorspaces"). **Original** — remux with `-c copy` (`-tag:v hvc1` for HEVC). Only video stream 0 and the first decodable audio are mapped (iPhone MOVs carry `mebx` metadata tracks MP4 rejects).
  - `RunProcess` captures stdout/stderr and throws on non-zero exit.
- `App.xaml.cs` — global `DispatcherUnhandledException` handler shows a MessageBox and writes `unhandled_error.log` / `startup_error.log` to `%AppData%\MIDItoMP3\` (next to `settings.json`).

## External tool dependencies (important)

`ExternalTools.cs` defines the tools (`FluidSynth`, `Lame`, `Ffmpeg`, `Ffprobe`) and resolves each in this order:

1. Manual path saved in `%AppData%\MIDItoMP3\settings.json` (`ToolSettings.cs`), if the file exists.
2. `PATH` — process, user and machine values are all checked, so tools installed while the app runs are found.
3. Bundled next to the exe: `fluidsynth\bin\fluidsynth.exe`, `lame3\lame.exe`, `ffmpeg\bin\ffmpeg.exe`, `ffmpeg\bin\ffprobe.exe` (relative to `AppContext.BaseDirectory`).

Manual paths are edited in `ToolsWindow` or set when `EnsureTool` prompts. The bundled tools are **not** in the csproj — they were copied into `bin\Debug\net8.0-windows\` by hand. The SoundFont (`FluidR3_GM\`) *is* copied to output via the csproj. `FluidR3_GM.sf2` (142 MB, over GitHub's 100 MB limit) is git-ignored and ships only in release archives; external tools are never bundled in releases, only linked from the README.
