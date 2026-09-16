using System.Linq;
using Dacodelaac.Attributes;
using Dacodelaac.ObjectPooling;
#if UNITY_EDITOR
using Dacodelaac.Utils;
using UnityEditor;
#endif
using UnityEngine;

namespace Dacodelaac.Core
{
    public class BaseSO : ScriptableObject, IEntity
    {
        [SerializeField, NamedId] string id;
        [Header("Base")]
        [SerializeField] private Pools pools;
        [SerializeField] private Ticker ticker;
        [SerializeField] private bool earlyTick;
        [SerializeField] private bool tick;
        [SerializeField] private bool lateTick;
        [SerializeField] private bool fixedTick;

        public string Id => id;

        /* Class con chỉ ĐỌC, không ghi đè được reference. */
        protected Pools GetPools() => pools;
        protected Ticker GetTicker() => ticker;

#if UNITY_EDITOR
        /* Chỉ dành cho Pools.AutoBind / Ticker.AutoBind — internal nên không lọt ra ngoài assembly. */
        internal void SetPools(Pools value) => pools = value;
        internal void SetTicker(Ticker value) => ticker = value;

        [ContextMenu("ResetId")]
        public void ResetId()
        {
            id = NamedIdAttribute.ToSnakeCase(name);
            EditorUtility.SetDirty(this);
        }
#endif

        public void Enable()
        {
            BindVariable();
            ListenEvents();
            SubTick();
            DoEnable();
        }

        public void Disable()
        {
            DoDisable();
            UnsubTick();
            StopListenEvents();
            UnbindVariable();
        }

        public virtual void BindVariable()
        {
        }

        public virtual void ListenEvents()
        {
        }

        public void DoEnable()
        {
        }

        void SubTick()
        {
            if (earlyTick) ticker.SubEarlyTick(this);
            if (tick) ticker.SubTick(this);
            if (lateTick) ticker.SubLateTick(this);
            if (fixedTick) ticker.SubFixedTick(this);
        }

        public virtual void Initialize()
        {
        }

        public virtual void EarlyTick()
        {
        }

        public virtual void Tick()
        {
        }

        public virtual void LateTick()
        {
        }

        public virtual void FixedTick()
        {
        }
        
        public virtual void CleanUp()
        {
        }

        public void DoDisable()
        {
        }

        void UnsubTick()
        {
            if (earlyTick) ticker.UnsubEarlyTick(this);
            if (tick) ticker.UnsubTick(this);
            if (lateTick) ticker.UnsubLateTick(this);
            if (fixedTick) ticker.UnsubFixedTick(this);
        }

        public virtual void StopListenEvents()
        {
        }

        public virtual void UnbindVariable()
        {
        }
        
#if UNITY_EDITOR
        void Reset()
        {
            ticker = AssetUtils.FindAssetAtFolder<Ticker>(new string[] {"Assets"}).FirstOrDefault();
            pools = AssetUtils.FindAssetAtFolder<Pools>(new string[] {"Assets"}).FirstOrDefault();
            EditorUtility.SetDirty(this);
        }
#endif
    }
}