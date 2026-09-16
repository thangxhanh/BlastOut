using System;
using Dacodelaac.Core;
using Dacodelaac.DataStorage;
using Dacodelaac.Events;
using Dev.Scripts.Sound;

using UnityEngine;
using Event = Dacodelaac.Events.Event;

namespace Dev.Scripts.Launcher
{
    public class GameLauncher : BaseLauncher
    {
        [SerializeField] private Event loadingEventDoneEvent;

        /* Cùng vai trò với mảng systems của Launcher. Có ở đây để bấm Play thẳng một scene
           gameplay trong Editor (không qua LauncherScene) vẫn khởi tạo được hệ thống.
           SO.Initialize() là idempotent nên bị gọi lại từ Launcher cũng vô hại. */
        [SerializeField] private BaseSO[] systems;

        private void Start()
        {
            Initialize();
            loadingEventDoneEvent.Raise();
        }

        public override void Initialize()
        {
            /* Init TRƯỚC base.Initialize(), vì base spawn prefab và mono trong đó
               có thể gọi ngay tới các hệ thống này. */
            foreach (var so in systems)
            {
                if (so != null) so.Initialize();
            }

            base.Initialize();

            /* Áp thiết lập đã lưu NGAY khi khởi động. Thiếu bước này thì mixer luôn mở hết cỡ
               lúc vào game, bất kể người chơi đã tắt nhạc từ lần trước. */
            if (SoundManager.Instance) SoundManager.Instance.ApplySavedSettings();
        }
        
        public void OnPauseGame(bool pause)
        {
            Time.timeScale = pause ? 0f : 1f;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                GameData.Save();
            }
        }

        public void OnApplicationQuit()
        {
            GameData.Save();
        }
    }
}