using Dacodelaac.Core;
using Dev.Scripts.BlastOut.Core;
using TMPro;
using UnityEngine;

namespace Dev.Scripts.BlastOut.UI
{
    /* HUD chỉ hiển thị, không quyết định gì. Nó nhận phase đã đổi rồi vẽ lại — không hỏi ngược
       gameplay, không tự đóng mở level. Nhờ vậy thay HUD này bằng popup thật sau đó không phải
       đụng tới một dòng gameplay nào. */
    public class BlastHudView : BaseMono
    {
        [SerializeField] TMP_Text levelLabel;
        [SerializeField] TMP_Text ammoLabel;
        [SerializeField] TMP_Text hintLabel;
        [SerializeField] TMP_Text bannerLabel;
        [SerializeField] GameObject bannerRoot;

        [Header("Màu banner")]
        [SerializeField] Color wonColor = new Color(0.55f, 0.98f, 0.82f);
        [SerializeField] Color lostColor = new Color(1f, 0.75f, 0.65f);

        public void ShowLevel(int number, string hint)
        {
            if (levelLabel) levelLabel.text = $"LEVEL {number:00}";

            if (hintLabel)
            {
                hintLabel.text = hint;
                hintLabel.gameObject.SetActive(!string.IsNullOrEmpty(hint));
            }

            if (bannerRoot) bannerRoot.SetActive(false);
        }

        public void ShowAmmo(int remaining, int total)
        {
            if (!ammoLabel) return;
            ammoLabel.text = $"AMMO {remaining}/{total}";
        }

        public void ShowPhase(BlastPhase phase)
        {
            switch (phase)
            {
                /* Chuỗi UI để ASCII: atlas LiberationSans SDF dựng sẵn của TMP không có dấu tiếng
                   Việt, để nguyên "HOÀN THÀNH" là ra một hàng ô vuông trên máy thật. */
                case BlastPhase.Won:
                    ShowBanner("LEVEL CLEAR - TAP TO CONTINUE", wonColor);
                    break;
                case BlastPhase.Lost:
                    ShowBanner("OUT OF AMMO - TAP TO RETRY", lostColor);
                    break;
                default:
                    if (bannerRoot) bannerRoot.SetActive(false);
                    break;
            }
        }

        void ShowBanner(string message, Color color)
        {
            if (hintLabel) hintLabel.gameObject.SetActive(false);
            if (!bannerRoot || !bannerLabel) return;

            bannerLabel.text = message;
            bannerLabel.color = color;
            bannerRoot.SetActive(true);
        }
    }
}
