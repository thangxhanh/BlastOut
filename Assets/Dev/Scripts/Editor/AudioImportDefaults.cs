using UnityEditor;
using UnityEngine;

namespace Dev.Scripts.Editor
{
    /// <summary>
    /// Đặt mặc định lúc NHẬP âm thanh, rẽ theo thư mục.
    ///
    /// SFX và nhạc nền cần thiết lập ngược nhau nên một Preset chung không phục vụ được cả hai:
    /// SFX là nhiều–ngắn–phải kêu ngay, nhạc là một–dài–chậm vài chục ms không ai biết.
    /// Chi tiết và lý do: Docs/NOTES.md §2.
    /// </summary>
    public class AudioImportDefaults : AssetPostprocessor
    {
        const string MusicFolder = "/Audio/Music/";
        const string SfxFolder = "/Audio/SFX/";

        void OnPreprocessAudio()
        {
            var importer = (AudioImporter)assetImporter;

            /* Chỉ can thiệp lần nhập ĐẦU — xem TextureImportDefaults. */
            if (!importer.importSettingsMissing) return;

            if (assetPath.Contains(MusicFolder)) ApplyMusic(importer);
            else if (assetPath.Contains(SfxFolder)) ApplySfx(importer);
        }

        /* Streaming: bài 3 phút stereo 44.1kHz mà giải nén là ~32MB RAM cho MỘT bài. */
        static void ApplyMusic(AudioImporter importer)
        {
            var settings = importer.defaultSampleSettings;

            settings.loadType = AudioClipLoadType.Streaming;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.7f;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = false;

            importer.defaultSampleSettings = settings;
            importer.forceToMono = false;
            importer.loadInBackground = true;
        }

        /* ADPCM chứ không Vorbis: Vorbis tốn CPU cho TỪNG voice đang phát, mà SFX thì hay nổ
           chục cái cùng lúc. File to hơn nhưng giải mã rẻ hơn hẳn. */
        static void ApplySfx(AudioImporter importer)
        {
            var settings = importer.defaultSampleSettings;

            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.ADPCM;
            settings.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
            settings.sampleRateOverride = 22050;
            settings.preloadAudioData = true;

            importer.defaultSampleSettings = settings;
            importer.forceToMono = true;
            importer.loadInBackground = false;
        }
    }
}
