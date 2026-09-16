using UnityEngine;

namespace Dacodelaac.Core
{
    public abstract class MonoSingleton<T> : BaseMono where T : MonoSingleton<T>
    {
        private static T m_Instance;
        static bool shuttingDown;

        public static T Instance
        {
            get
            {
                if (m_Instance != null || shuttingDown || !Application.isPlaying) return m_Instance;
                m_Instance = FindFirstObjectByType(typeof(T)) as T;

                if (m_Instance == null)
                {
                    m_Instance = new GameObject("Temp Instance of " + typeof(T), typeof(T))
                        .GetComponent<T>();
                }

                return m_Instance;
            }
        }

        protected virtual void Awake()
        {
            if (m_Instance == null)
                m_Instance = this as T;
            else if (m_Instance != this)
            {
                /* Destroy chứ không DestroyImmediate: DestroyImmediate huỷ object ngay giữa lúc
                   Unity đang duyệt vòng đời scene, không an toàn trong runtime. */
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (this == m_Instance)
                m_Instance = null;
        }

        private void OnApplicationQuit()
        {
            m_Instance = null;
            shuttingDown = true;
        }
    }
}