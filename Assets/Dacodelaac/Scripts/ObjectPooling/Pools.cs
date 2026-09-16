using System;
using System.Collections.Generic;
using Dacodelaac.Core;
using Dacodelaac.DebugUtils;
using Dacodelaac.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dacodelaac.ObjectPooling
{
    [CreateAssetMenu(menuName = "ObjectPooling/Pools")]
    public class Pools : BaseSO, ISerializationCallbackReceiver
    {
        [SerializeField] private PoolData[] poolDatas;

        /* Queue chứa PooledObjectId thay vì GameObject để despawn không phải GetComponent lại. */
        private Dictionary<GameObject, Queue<PooledObjectId>> waitPool;
        private Dictionary<GameObject, PooledObjectId> activePool;
        private Transform container;
        private bool initialized;

        public override void Initialize()
        {
            if (initialized) return;
            initialized = true;

            waitPool = new Dictionary<GameObject, Queue<PooledObjectId>>();
            activePool = new Dictionary<GameObject, PooledObjectId>();
            container = new GameObject("Pool").transform;
            DontDestroyOnLoad(container.gameObject);

            PreSpawn();
        }

        private void PreSpawn()
        {
            foreach (var data in poolDatas)
            {
                for (var i = 0; i < data.PreSpawn; i++)
                {
                    SpawnNew(data.Prefab);
                }
            }
        }

        private void SpawnNew(GameObject prefab)
        {
            var gameObject = Instantiate(prefab);
            var id = gameObject.AddComponent<PooledObjectId>();
            id.SetPrefab(prefab);

            activePool.Add(gameObject, id);

            DeSpawn(gameObject, false);
        }

        public void DeSpawn(GameObject gameObject, bool destroy = false)
        {
            /* Có mặt trong activePool là điều kiện đủ: object chưa pool bao giờ, hoặc đã despawn rồi,
               đều rớt ở đây — nên không cần kiểm tra thêm trong waitPool. */
            if (!activePool.TryGetValue(gameObject, out var id))
            {
                Dacoder.LogError($"{gameObject.name} is not a pooled object, or is already despawned!");
                return;
            }

            activePool.Remove(gameObject);

            CleanUp(gameObject);

            if (destroy)
            {
                Destroy(gameObject);
                return;
            }

            gameObject.SetActive(false);
            gameObject.transform.SetParent(container, false);

            if (!waitPool.TryGetValue(id.Prefab, out var stack))
            {
                stack = new Queue<PooledObjectId>();
                waitPool.Add(id.Prefab, stack);
            }

            stack.Enqueue(id);
        }

        public void DespawnAll()
        {
            /* Copy ra mảng vì DeSpawn sẽ sửa activePool trong lúc duyệt. */
            var arr = new GameObject[activePool.Count];
            activePool.Keys.CopyTo(arr, 0);

            foreach (var o in arr)
            {
                if (o != null) DeSpawn(o);
            }
        }

        public void DestroyAll()
        {
            foreach (var stack in waitPool.Values)
            {
                foreach (var id in stack)
                {
                    if (id != null) Destroy(id.gameObject);
                }
            }

            waitPool.Clear();
        }

        public T Spawn<T>(T type, Transform parent = null, bool initialize = true) where T : Component
        {
            return Spawn(type.gameObject, parent, initialize).GetComponent<T>();
        }

        public GameObject Spawn(GameObject prefab, Transform parent = null, bool initialize = true)
        {
            if (!waitPool.TryGetValue(prefab, out var stack))
            {
                stack = new Queue<PooledObjectId>();
                waitPool.Add(prefab, stack);
            }

            if (stack.Count == 0)
            {
                /* SpawnNew kết thúc bằng DeSpawn -> enqueue vào đúng stack này. */
                SpawnNew(prefab);
            }

            var id = stack.Dequeue();
            var gameObject = id.gameObject;

            gameObject.transform.SetParent(parent, false);

            if (parent == null)
            {
                SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
            }

            gameObject.SetActive(true);

            if (initialize)
            {
                Initialize(gameObject);
            }

            activePool.Add(gameObject, id);

            return gameObject;
        }

        private static void Initialize(GameObject go)
        {
            var monos = go.GetComponentsInChildren<BaseMono>(true);
            foreach (var mono in monos)
            {
                mono.Initialize();
            }
        }

        private static void CleanUp(GameObject go)
        {
            var monos = go.GetComponentsInChildren<BaseMono>(true);
            foreach (var mono in monos)
            {
                mono.CleanUp();
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            initialized = false;
        }

#if UNITY_EDITOR
        [ContextMenu("Auto Bind")]
        public void AutoBind()
        {
            var soes = AssetUtils.FindAssetAtFolder<BaseSO>(new string[] {"Assets"});
            foreach (var so in soes)
            {
                so.SetPools(this);
                EditorUtility.SetDirty(so);
            }

            var goes = AssetUtils.FindAssetAtFolder<GameObject>(new string[] {"Assets"});
            foreach (var go in goes)
            {
                var monoes = go.GetComponentsInChildren<BaseMono>(true);
                foreach (var mono in monoes)
                {
                    mono.SetPools(this);
                    EditorUtility.SetDirty(mono);
                }
            }
        }
#endif
    }

    [Serializable]
    public class PoolData
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int preSpawn;

        public GameObject Prefab => prefab;
        public int PreSpawn => preSpawn;
    }
}
