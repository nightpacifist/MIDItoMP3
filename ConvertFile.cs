using System.Diagnostics;
using System.IO;
using System.Text;

namespace MIDItoMP3
{
    /// <summary>
    /// Конвертація через зовнішні програми. Методи блокуючі — викликати поза UI-потоком.
    /// </summary>
    class ConvertFile
    {
        public static void ConvertMidiToMp3(string fluidsynthExe, string lameExe, string midiPath, string sf2Path, string mp3Path)
        {
            if (!File.Exists(midiPath)) throw new FileNotFoundException(Loc.T("MidiNotFound"), midiPath);
            if (!File.Exists(sf2Path)) throw new FileNotFoundException(Loc.T("SoundFontNotFound"), sf2Path);

            string tempWav = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".wav");
            Debug.WriteLine(tempWav);
            try
            {
                // 1) MIDI -> WAV через fluidsynth
                string fsArgs = $"-F \"{tempWav}\" -r 44100 -ni \"{sf2Path}\" \"{midiPath}\"";
                RunProcess(fluidsynthExe, fsArgs, throwOnError: true);

                if (!File.Exists(tempWav)) throw new InvalidOperationException(Loc.T("WavFailed"));

                // 2) WAV -> MP3 через lame
                string lameArgs = $"--preset standard \"{tempWav}\" \"{mp3Path}\"";
                RunProcess(lameExe, lameArgs, throwOnError: true);
            }
            finally
            {
                if (File.Exists(tempWav))
                {
                    try { File.Delete(tempWav); } catch { /* ignore */ }
                }
            }
        }

        /// <param name="mode">
        /// <see cref="Mp4Mode.Compatible"/> — перекодувати в H.264 SDR BT.709 (HDR з iPhone тонально стискається), відтворюється всюди.
        /// <see cref="Mp4Mode.Original"/> — лише перепакувати потоки без перекодування: кольори й HDR як в оригіналі.
        /// </param>
        public static void ConvertMovToMp4(string ffmpegExe, string ffprobeExe, string movPath, string mp4Path, Mp4Mode mode)
        {
            if (!File.Exists(movPath)) throw new FileNotFoundException(Loc.T("VideoNotFound"), movPath);

            var info = VideoInfo.Probe(ffprobeExe, movPath);

            // -map замість автоматичного вибору: MOV з iPhone містить службові доріжки (метадані, Dolby Vision), які MP4 не приймає
            var args = new StringBuilder($"-y -nostdin -i \"{movPath}\" -map 0:v:0");
            if (info.AudioStreamIndex is int audio)
                args.Append($" -map 0:{audio}");

            if (mode == Mp4Mode.Original)
            {
                args.Append(" -c:v copy");
                // hvc1 — тег, з яким HEVC відкривають QuickTime/iOS/Windows (ffmpeg за замовчуванням пише hev1)
                if (info.VideoCodec == "hevc")
                    args.Append(" -tag:v hvc1");
                args.Append(info.AudioCodec is "aac" or "alac" or "mp3" or "ac3" or "eac3" ? " -c:a copy" : " -c:a aac -b:a 192k");
            }
            else
            {
                args.Append($" -vf \"{CompatibleVideoFilter(info)}\"");
                // Явні теги, щоб плеєр не вгадував матрицю (деякі для невідомого відео беруть BT.601 — і кольори зсуваються)
                // -level не задаємо: 4.1 замало для 4K/60 з iPhone, libx264 сам вибере потрібний
                args.Append(" -c:v libx264 -preset fast -crf 20 -profile:v high -pix_fmt yuv420p");
                args.Append(" -colorspace bt709 -color_primaries bt709 -color_trc bt709 -color_range tv");
                args.Append(" -c:a aac -b:a 192k");
            }

            args.Append($" -movflags +faststart \"{mp4Path}\"");
            RunProcess(ffmpegExe, args.ToString(), throwOnError: true);
        }

        private static string CompatibleVideoFilter(VideoInfo info)
        {
            string range = info.ColorRange == "pc" ? "full" : "limited";

            if (info.IsHdr)
            {
                // HDR (HLG або PQ, BT.2020) → лінійне світло → BT.709 → тональне стискання → SDR BT.709.
                // Без цього 10-бітний HDR просто обрізається до 8 біт, і плеєр показує його з хибними кольорами.
                string tin = info.ColorTransfer == "smpte2084" ? "smpte2084" : "arib-std-b67";
                // m=gbr обов'язково: лінійне світло zimg вміє тримати лише в RGB, інакше "no path between colorspaces"
                return $"zscale=tin={tin}:min=bt2020nc:pin=bt2020:rin={range}:t=linear:npl=100:p=bt2020:m=gbr,format=gbrpf32le," +
                       "zscale=p=bt709,tonemap=tonemap=hable:desat=0," +
                       "zscale=t=bt709:m=bt709:r=limited,format=yuv420p";
            }

            // SDR: якщо теги відсутні, вважаємо джерело BT.709 (так пише iPhone і майже всі HD-камери)
            string matrix = info.ColorSpace is "bt470bg" or "smpte170m" ? "bt601" : "bt709";
            return $"scale=in_color_matrix={matrix}:in_range={range}:out_color_matrix=bt709:out_range=limited,format=yuv420p";
        }

        public static string RunProcess(string exe, string args, bool throwOnError)
        {
            Debug.WriteLine($"{exe} {args}");
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var p = new Process())
            {
                p.StartInfo = psi;

                var stdoutBuilder = new StringBuilder();
                var stderrBuilder = new StringBuilder();

                p.OutputDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                        stdoutBuilder.AppendLine(e.Data);
                };
                p.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data != null)
                        stderrBuilder.AppendLine(e.Data);
                };

                if (!p.Start())
                    throw new InvalidOperationException(Loc.T("ProcessStartFailed", exe));

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();

                string stdout = stdoutBuilder.ToString();
                string stderr = stderrBuilder.ToString();

                Debug.WriteLine($"Process Output ({Path.GetFileName(exe)}):\nSTDOUT: {stdout}\nSTDERR: {stderr}");

                if (p.ExitCode != 0 && throwOnError)
                {
                    throw new InvalidOperationException(
                        Loc.T("ProcessFailed", Path.GetFileName(exe), p.ExitCode) + $"\nSTDOUT: {stdout}\nSTDERR: {stderr}"
                    );
                }

                return stdout;
            }
        }
    }
}
