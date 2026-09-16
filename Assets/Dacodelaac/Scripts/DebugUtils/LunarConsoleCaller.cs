using System;
using Dacodelaac.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dacodelaac.DebugUtils
{
    public class LunarConsoleCaller : BaseMono, IPointerClickHandler
    {
        float lastTimeClick;
        int count = 0;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Time.time - lastTimeClick > 0.5f)
            {
                count = 1;
            }
            else
            {
                count++;
                if (count >= 10)
                {
                    count = 0;
                    Toggle();
                }
            }

            lastTimeClick = Time.time;
        }

        void Toggle()
        {
            // if (!LunarConsolePlugin.LunarConsole.isConsoleEnabled)
            // {
            //     LunarConsolePlugin.LunarConsole.SetConsoleEnabled(true);
            // }
            //
            // LunarConsolePlugin.LunarConsole.Show();
        }
    }
}