using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Dacodelaac.Core;
using Dacodelaac.DataStorage;
using Dacodelaac.DebugUtils;
using Dacodelaac.Events;
using UnityEngine;
using Event = Dacodelaac.Events.Event;
using Random = UnityEngine.Random;

namespace Dacodelaac.LevelSystem
{
    [CreateAssetMenu(menuName = "LevelSystem/LevelManager")]
    public class LevelManager : BaseSO, ISerializationCallbackReceiver
    {
        [SerializeField] RunOutOfLevel runOutOfLevel;
        [SerializeField] DictionaryEvent analyticsLevelStartEvent;
        [SerializeField] DictionaryEvent analyticsLevelCompletedEvent;
        [SerializeField] DictionaryEvent analyticsLevelFailedEvent;
        [SerializeField] DictionaryEvent analyticsLevelRestartEvent;
        [SerializeField] Event levelEndedEvent;
        
#if UNITY_EDITOR
        [SerializeField] LevelData levelTest;
#endif
        [SerializeField] LevelData[] levels;

        const string LEVEL_ID_KEY = "LEVEL_ID";
        const string PLAY_STATE_KEY = "PLAY_STATE";
        const string COMPLETED_LEVEL_COUNT_KEY = "COMPLETED_LEVEL_COUNT";

        public string LevelID
        {
            get => GameData.Get(LEVEL_ID_KEY, "");
            set => GameData.Set(LEVEL_ID_KEY, value);
        }

        public PlayState State
        {
            get => GameData.Get(PLAY_STATE_KEY, PlayState.FirstTimePlay);
            set => GameData.Set(PLAY_STATE_KEY, value);
        }

        public int CompletedLevelCount
        {
            get => GameData.Get(COMPLETED_LEVEL_COUNT_KEY, 0);
            set => GameData.Set(COMPLETED_LEVEL_COUNT_KEY, value);
        }
        
        public int GetLevelAttempt(string id) => GameData.Get($"attempt_{id}", 0);
        public void SetLevelAttempt(string id, int value) => GameData.Set($"attempt_{id}", value);

        public int LevelIndex => State == PlayState.Completed ? CompletedLevelCount - 1 : CompletedLevelCount;
        public int LevelCount => levels.Length;

        bool initialized;
        
        public override void Initialize()
        {
            if (initialized) return;
            initialized = true;
            
#if UNITY_EDITOR
            if (levelTest) return;
#endif
            for (var i = 0; i < levels.Length; i++)
            {
                levels[i].Initialize(i);
            }

            switch (State)
            {
                case PlayState.FirstTimePlay:
                    OnFirstTimePlay();
                    break;
                case PlayState.Playing:
                    break;
                case PlayState.Completed:
                    OnNextLevel();
                    break;
                case PlayState.Failed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!GetCurrentLevel())
            {
                LevelID = GetNextLevel().Id;
            }
        }
        
        public LevelData GetLevel(string id)
        {
            return levels.FirstOrDefault(l => l.Id == id);
        }

        public LevelData GetLevel(int index)
        {
            return levels[index];
        }

        public LevelData GetCurrentLevel()
        {
#if UNITY_EDITOR
            if (levelTest)
            {
                return levelTest;
            }
#endif
            if (!initialized)
            {
                Initialize();
            }
            var level = GetLevel(LevelID);
            if (level)
            {
                for (var i = 0; i < levels.Length; i++)
                {
                    levels[i].DisplayIndex = CompletedLevelCount - level.Index + i;
                }
            }

            return level;
        }

        LevelData GetNextLevel()
        {
#if UNITY_EDITOR
            if (levelTest)
            {
                return levelTest;
            }
#endif
            if (CompletedLevelCount >= levels.Length)
            {
                switch (runOutOfLevel)
                {
                    case RunOutOfLevel.PlayFromBeginning:
                        return levels[CompletedLevelCount % levels.Length];
                    case RunOutOfLevel.PlayLastLevel:
                        return levels[levels.Length - 1];
                    case RunOutOfLevel.PlayRandomLevel:
                        // return levels[Random.Range(0, levels.Length)];
                        var currentLevel = GetCurrentLevel();
                        if (currentLevel.Index % 6 == 5)
                        {
                            return levels[Random.Range(1, levels.Length / 6) * 6];
                        }
                        else
                        {
                            return levels[(currentLevel.Index + 1) % levels.Length];
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                return levels[CompletedLevelCount];
            }
        }

        void OnFirstTimePlay()
        {
            State = PlayState.Playing;
#if UNITY_EDITOR
            if (levelTest) return;
#else
            LevelID = levels[0].Id;
#endif
            GameData.Save();
        }

        public void OnLevelStart(string id)
        {
#if UNITY_EDITOR
            if (levelTest) return;
#endif
            State = PlayState.Playing;
            SetLevelAttempt(id, GetLevelAttempt(id) + 1);
            GameData.Save();
            
            analyticsLevelStartEvent.Raise(new Dictionary<string, object>()
            {
                {"index", GetLevel(id).Index + 1},
                {"name", GetLevel(id).Index},
                {"id", id},
                {"attemptNum", GetLevelAttempt(id)}
            });
        }

        public void OnLevelCompleted(string id)
        {
#if UNITY_EDITOR
            if (levelTest) return;
#endif
            State = PlayState.Completed;
            CompletedLevelCount++;
            GameData.Save();
            
            Dacoder.Log($"Level {GetLevel(id).Index + 1} Completed");
            levelEndedEvent.Raise();
            analyticsLevelCompletedEvent.Raise(new Dictionary<string, object>()
            {
                {"index", GetLevel(id).Index + 1},
                {"name", GetLevel(id).Index},
                {"id", id},
                {"attemptNum", GetLevelAttempt(id)}
            });
        }

        public void OnLevelFailed(string id)
        {
#if UNITY_EDITOR
            if (levelTest) return;
#endif
            State = PlayState.Failed;
            GameData.Save();

            Dacoder.Log($"Level {GetLevel(id).Index + 1} failed");
            levelEndedEvent.Raise();
            analyticsLevelFailedEvent.Raise(new Dictionary<string, object>()
            {
                {"index", GetLevel(id).Index + 1},
                {"name", GetLevel(id).Index},
                {"id", id},
                {"attemptNum", GetLevelAttempt(id)}
            });
        }

        public void OnLevelRestart(string id)
        {
#if UNITY_EDITOR
            if (levelTest) return;
#endif
            var hasWon = State == PlayState.Completed; 
            if (hasWon)
            {
                CompletedLevelCount--;
            }
            State = PlayState.Playing;
            GameData.Save();
            levelEndedEvent.Raise();
            analyticsLevelRestartEvent.Raise(new Dictionary<string, object>()
            {
                {"index", GetLevel(id).Index + 1},
                {"name", GetLevel(id).Index},
                {"id", id},
                {"attemptNum", GetLevelAttempt(id)},
            });
        }

        public void OnNextLevel()
        {
#if UNITY_EDITOR
            if (levelTest) return;
#endif
            LevelID = GetNextLevel().Id;
            State = PlayState.Playing;
            GameData.Save();
        }

        public void SetCurrentLevel(LevelData level)
        {
#if UNITY_EDITOR
            if (levelTest) return;
#endif
            var index = Array.IndexOf(levels, level);
            if (index != -1)
            {
                LevelID = level.Id;
                CompletedLevelCount = index;
                State = PlayState.Playing;
                GameData.Save();
            }
            else
            {
                Dacoder.LogError("Drag level to LevelManager first!");
            }
        }
        
        [ContextMenu("Check Id")]
        public void CheckId()
        {
            var dupes = levels.GroupBy(x => new {x.Id}).Where(x => x.Skip(1).Any()).ToArray();
            if (dupes.Length > 0)
            {
                foreach (var dupe in dupes)
                {
                    Dacoder.LogError(dupe.Key);
                }
            }
            else
            {
                Dacoder.Log("No duplicate id");
            }
        }

        enum RunOutOfLevel
        {
            PlayFromBeginning,
            PlayLastLevel,
            PlayRandomLevel
        }

        public enum PlayState
        {
            FirstTimePlay,
            Playing,
            Completed,
            Failed
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            initialized = false;
        }
    }   
}