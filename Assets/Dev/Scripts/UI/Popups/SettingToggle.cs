using UnityEngine;
using UnityEngine.UI;

namespace Dev.Scripts.UI.Popups
{
    /// <summary>
    /// 1 ô toggle vuông trong <see cref="SettingsPopup"/> (Music / Sound / Haptic). Tự quản hình theo
    /// trạng thái on/off qua 2 cách (dùng cái nào GÁN cái đó — có thể dùng cả hai):
    /// <list type="bullet">
    /// <item>SPRITE-SWAP (in-game): nền tint nâu(on)/xám(off) + icon đổi sprite on/off.</item>
    /// <item>GAMEOBJECT ON/OFF (Home): bật <see cref="onObject"/> khi on / <see cref="offObject"/> khi off
    /// (2 hình gạt On/Off dựng sẵn trong node — reskin Home dùng kiểu này).</item>
    /// </list>
    /// Trạng thái THẬT do SettingsPopup đọc/ghi qua GameData; component này chỉ vẽ.
    /// </summary>
    [RequireComponent(typeof(Image), typeof(Button))]
    public class SettingToggle : MonoBehaviour
    {
        [SerializeField] Image background;   // nền vuông (= Image của chính node này)
        [SerializeField] Image icon;         // glyph nhạc / loa / rung (con "Icon")
        [SerializeField] Sprite onIcon;
        [SerializeField] Sprite offIcon;
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Sprite offSprite;

        [Header("Kiểu GameObject On/Off (Home) — trống = không dùng")]
        [Tooltip("GameObject HIỆN khi BẬT (con 'On'). Trống = bỏ qua.")]
        [SerializeField] GameObject onObject;
        [Tooltip("GameObject HIỆN khi TẮT (con 'Off'). Trống = bỏ qua.")]
        [SerializeField] GameObject offObject;

        public Button Button { get; private set; }

        void Reset()
        {
            background = GetComponent<Image>();
            var ic = transform.Find("Icon");
            if (ic) icon = ic.GetComponent<Image>();
            var on = transform.Find("On");
            if (on) onObject = on.gameObject;
            var off = transform.Find("Off");
            if (off) offObject = off.gameObject;
        }

        /// <summary>Cache Button (gọi trước khi wire onClick). Idempotent.</summary>
        public void Init()
        {
            if (!Button) Button = GetComponent<Button>();
            if (!background) background = GetComponent<Image>();
        }

        /// <summary>Cập nhật hình theo trạng thái bật/tắt (sprite-swap + bật/tắt GameObject On/Off — cái nào gán mới chạy).</summary>
        public void SetState(bool on)
        {
            if (background && (onSprite || offSprite)) background.sprite = on ? onSprite : offSprite;
            if (icon && (onIcon || offIcon)) icon.sprite = on ? onIcon : offIcon;
            if (onObject) onObject.SetActive(on);
            if (offObject) offObject.SetActive(!on);
        }
    }
}
