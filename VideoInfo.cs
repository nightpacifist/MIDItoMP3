using System.Text.Json;

namespace MIDItoMP3
{
    public enum Mp4Mode { Compatible, Original }

    /// <summary>Параметри відео, потрібні для вибору конвертації (через ffprobe).</summary>
    public record VideoInfo(
        string VideoCodec,
        string? ColorSpace,
        string? ColorTransfer,
        string? ColorPrimaries,
        string? ColorRange,
        int? AudioStreamIndex,
        string? AudioCodec)
    {
        /// <summary>HLG (iPhone) або PQ (HDR10 / Dolby Vision).</summary>
        public bool IsHdr => ColorTransfer is "arib-std-b67" or "smpte2084";

        public static VideoInfo Probe(string ffprobeExe, string path)
        {
            string json = ConvertFile.RunProcess(ffprobeExe,
                $"-v error -show_entries stream=index,codec_type,codec_name,color_space,color_transfer,color_primaries,color_range -of json \"{path}\"",
                throwOnError: true);

            using var doc = JsonDocument.Parse(json);
            var streams = doc.RootElement.GetProperty("streams").EnumerateArray().ToList();

            var video = streams.FirstOrDefault(s => Str(s, "codec_type") == "video");
            if (video.ValueKind == JsonValueKind.Undefined)
                throw new InvalidOperationException(Loc.T("NoVideoStream"));

            // Перша аудіодоріжка, яку ffmpeg уміє декодувати (у нових iPhone буває ще й просторове аудіо APAC без декодера)
            var audio = streams.FirstOrDefault(s => Str(s, "codec_type") == "audio" && Str(s, "codec_name") is string c && c != "none");

            return new VideoInfo(
                Str(video, "codec_name") ?? "",
                Str(video, "color_space"),
                Str(video, "color_transfer"),
                Str(video, "color_primaries"),
                Str(video, "color_range"),
                audio.ValueKind == JsonValueKind.Undefined ? null : audio.GetProperty("index").GetInt32(),
                audio.ValueKind == JsonValueKind.Undefined ? null : Str(audio, "codec_name"));
        }

        private static string? Str(JsonElement e, string name) =>
            e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
    }
}
