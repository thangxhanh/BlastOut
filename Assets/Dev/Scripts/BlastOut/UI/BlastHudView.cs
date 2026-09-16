using System;
using Dacodelaac.Core;
using Dev.Scripts.BlastOut.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dev.Scripts.BlastOut.UI
{
    /* HUD chỉ hiển thị và phát tín hiệu bấm nút — không quyết định gì. Nó nhận phase đã đổi rồi vẽ
       lại, và khi người chơi bấm nút thì bắn event ra ngoài; ai xử lý thắng/thua là việc của
       controller. Nhờ vậy đổi bố cục UI không phải đụng một dòng gameplay nào. */
    public class BlastHudView : BaseMono
    {
        [Header("Thông tin")]
        [SerializeField] TMP_Text levelLabel;
        [SerializeField] TMP_Text ammoLabel;
        [SerializeField] TMP_Text hintLabel;

        [Header("Panel kết quả")]
        [SerializeField] GameObject resultRoot;
        [SerializeField] TMP_Text resultLabel;
        [SerializeField] Button actionButton;
        [SerializeField] TMP_Text actionLabel;

        [Header("Luôn hiện")]
        [SerializeField] Button restartButton;

        [Header("Màu")]
        [SerializeField] Color wonColor = new Color(0.55f, 0.98f, 0.82f);
        [SerializeField] Color lostColor = new Color(1f, 0.75f, 0.65f);

        /* Nút chính trong panel: Next khi thắng, Retry khi thua. Controller đọc phase để biết làm gì. */
        public event Action ActionPressed;
        public event Action RestartPressed;

        public override void Initialize()
        {
            base.Initialize();

            if (actionButton) actionButton.onClick.AddListener(RaiseAction);
            if (restartButton) restartButton.onClick.AddListener(RaiseRestart);

            if (resultRoot) resultRoot.SetActive(false);
        }

        public override void CleanUp()
        {
            if (actionButton) actionButton.onClick.RemoveListener(RaiseAction);
            if (restartButton) restartButton.onClick.RemoveListener(RaiseRestart);
        }

        public void ShowLevel(int number, string hint)
        {
            if (levelLabel) levelLabel.text = $"LEVEL {number:00}";

            if (hintLabel)
            {
                hintLabel.text = hint;
                hintLabel.gameObject.SetActive(!string.IsNullOrEmpty(hint));
            }

            if (resultRoot) resultRoot.SetActive(false);
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
                    ShowResult("LEVEL CLEAR", "NEXT", wonColor);
                    break;
                case BlastPhase.Lost:
                    ShowResult("OUT OF AMMO", "RETRY", lostColor);
                    break;
                default:
                    if (resultRoot) resultRoot.SetActive(false);
                    break;
            }
        }

        void ShowResult(string message, string action, Color color)
        {
            if (hintLabel) hintLabel.gameObject.SetActive(false);
            if (resultLabel)
            {
                resultLabel.text = message;
                resultLabel.color = color;
            }
            if (actionLabel) actionLabel.text = action;
            if (resultRoot) resultRoot.SetActive(true);
        }

        void RaiseAction() => ActionPressed?.Invoke();
        void RaiseRestart() => RestartPressed?.Invoke();
    }
}
