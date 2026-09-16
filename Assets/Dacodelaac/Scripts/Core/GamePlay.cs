using System.Collections;
using Dacodelaac.Collections;
using Dacodelaac.DataStorage;
using Dacodelaac.Events;
using Dacodelaac.LevelSystem;
using Dacodelaac.Utils;
using Dacodelaac.Variables;
using DG.DemiLib.Attributes;
using UnityEngine;
using Event = Dacodelaac.Events.Event;

namespace Dacodelaac.Core
{
    [DeScriptExecutionOrder(ExecutionOrder.Second)]
    public class GamePlay : BaseMono
    {
        [SerializeField] LevelManager levelManager;
        [SerializeField] Event loadingEventDoneEvent;
        [SerializeField] LoadingScreenEvent loadingScreenEvent;
        
        void Start()
        {
            InitData();
            LoadLevel();
            
            loadingEventDoneEvent.Raise();
        }

        void InitData()
        {
            levelManager.Initialize();
        }

        void LoadLevel()
        {
            var levelData = levelManager.GetCurrentLevel();
            levelManager.OnLevelStart(levelData.Id);
        }
        
        void OnLevelCompleted()
        {
            levelManager.OnLevelCompleted("");
        }

        void OnLevelFailed()
        {
            levelManager.OnLevelFailed("");
        }

        public void OnLevelRestart()
        {
            levelManager.OnLevelRestart("");
        }

        public void OnNextLevel()
        {
            levelManager.OnNextLevel();
            loadingScreenEvent.Raise(new LoadingScreenData
            {
                Scene = "GameScene",
                IsLaunching = false,
                LaunchCondition = () => true,
                MinLoadTime = 1,
            });
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                GameData.Save();
            }
        }

        void OnApplicationQuit()
        {
            GameData.Save();
        }
    }
}