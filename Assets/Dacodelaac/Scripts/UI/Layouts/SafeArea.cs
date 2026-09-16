using Dacodelaac.Core;
using UnityEngine;

namespace Dacodelaac.UI.Layouts
{
    public class SafeArea : BaseMono
    {
        /* Screen.safeArea là native call và chỉ đổi khi xoay màn hình / vào-ra fullscreen.
           Poll mỗi frame là lãng phí, nên chỉ kiểm tra lại khi kích thước màn hình đổi. */
        const int CHECK_INTERVAL = 30;

        RectTransform rect;
        Rect lastSafeArea = new Rect(0, 0, 0, 0);
        int lastWidth;
        int lastHeight;
        int frameCounter;

        public override void Initialize()
        {
            rect = GetComponent<RectTransform>();
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            ApplySafeArea(Screen.safeArea);
        }

        public override void Tick()
        {
            if (++frameCounter < CHECK_INTERVAL) return;
            frameCounter = 0;

            if (Screen.width == lastWidth && Screen.height == lastHeight) return;

            lastWidth = Screen.width;
            lastHeight = Screen.height;

            var safeArea = Screen.safeArea;
            if (safeArea != lastSafeArea) ApplySafeArea(safeArea);
        }

        void ApplySafeArea(Rect r)
        {
            lastSafeArea = r;

            var anchorMin = r.position;
            var anchorMax = r.position + r.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
        }
    }
}
