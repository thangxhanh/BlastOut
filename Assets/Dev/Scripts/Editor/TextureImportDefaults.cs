using UnityEditor;

namespace Dev.Scripts.Editor
{
    /// <summary>
    /// Đặt mặc định lúc NHẬP ảnh, thay cho việc sửa tay từng file.
    ///
    /// Lọc theo <see cref="TextureImporterType"/> nên dùng chung được cho cả project 2D lẫn 3D:
    /// ảnh không phải Sprite thì bỏ qua, không cần đổi Default Behavior Mode của project.
    /// </summary>
    public class TextureImportDefaults : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;

            /* Chỉ can thiệp lần nhập ĐẦU. Ảnh đã có .meta nghĩa là ai đó đã chọn rồi —
               ghi đè ở đây thì mỗi lần Reimport lại thổi bay lựa chọn của họ. */
            if (!importer.importSettingsMissing) return;

            if (importer.textureType != TextureImporterType.Sprite) return;

            /* Ô này không phải property của TextureImporter — phải đi qua TextureImporterSettings,
               đọc ra rồi ghi lại cả cụm. */
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);

            /* Sinh polygon va chạm cho MỌI sprite là thừa: phần lớn sprite chỉ để hiển thị.
               Sprite nào thật sự cần collider 2D thì bật lại bằng tay. */
            settings.spriteGenerateFallbackPhysicsShape = false;

            importer.SetTextureSettings(settings);
        }
    }
}
