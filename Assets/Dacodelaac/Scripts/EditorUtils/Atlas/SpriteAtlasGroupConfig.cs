#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dacodelaac.EditorUtils.Atlas
{
    /// <summary>
    /// Cấu hình cách gom sprite thành atlas. Asset này COMMIT vào git để cả team gom giống nhau.
    /// Nguyên tắc gom: theo nhóm sprite CÙNG xuất hiện / CÙNG biến mất trên màn hình,
    /// KHÔNG gom theo thư mục cho tiện — gom bừa vào 1 atlas lớn là đổi draw call lấy tràn RAM.
    /// </summary>
    public class SpriteAtlasGroupConfig : ScriptableObject
    {
        public const string DefaultPath =
            "Assets/Dev/Editor/SpriteAtlasGroups.asset";

        [Tooltip("Nơi sinh ra file .spriteatlas.")]
        [SerializeField] private string outputFolder = "Assets/Dev/Sprites/Atlases";

        [Tooltip("Sprite lớn hơn ngưỡng này (px) sẽ bị Audit cảnh báo — nên để NGOÀI atlas.")]
        [SerializeField] private int oversizeThreshold = 512;

        [Tooltip("Phạm vi Audit quét tìm sprite chưa được gom.")]
        [SerializeField] private string auditScope = "Assets/Dev";

        [SerializeField] private List<Group> groups = new List<Group>();

        public string OutputFolder => outputFolder;
        public int OversizeThreshold => oversizeThreshold;
        public string AuditScope => auditScope;
        public List<Group> Groups => groups;

        [Serializable]
        public class Group
        {
            [Tooltip("Tên file atlas sinh ra. Vd: UI_Common, UI_Home, UI_Gameplay, Theme_Sheep.")]
            [SerializeField] private string atlasName = "UI_Common";

            [Tooltip("Folder hoặc sprite lẻ gom vào atlas này. Kéo thả folder từ Project vào đây.")]
            [SerializeField] private List<UnityEngine.Object> packables = new List<UnityEngine.Object>();

            [Tooltip("2048 cho mobile. 4096 rủi ro trên máy yếu.")]
            [SerializeField] private int maxTextureSize = 2048;

            [Tooltip("Khoảng đệm giữa các sprite, chống rìa lem khi scale.")]
            [SerializeField] private int padding = 4;

            [Tooltip("Xếp sát theo hình dạng — tiết kiệm chỗ nhưng hại với sprite 9-slice/UI.")]
            [SerializeField] private bool tightPacking;

            [Tooltip("Bỏ tick nếu atlas này load động (theme/skin) và không nên nằm sẵn trong build chính.")]
            [SerializeField] private bool includeInBuild = true;

            /* AtlasName có setter vì cửa sổ tool tạo group mới bằng object-initializer. */
            public string AtlasName { get => atlasName; set => atlasName = value; }
            public List<UnityEngine.Object> Packables => packables;
            public int MaxTextureSize => maxTextureSize;
            public int Padding => padding;
            public bool TightPacking => tightPacking;
            public bool IncludeInBuild => includeInBuild;

            public string Describe() => $"{atlasName} ({packables.Count} mục, max {maxTextureSize})";
        }
    }
}
#endif
