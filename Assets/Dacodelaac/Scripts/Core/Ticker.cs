using System.Collections.Generic;
using Dacodelaac.Scripts.Core;
using Dacodelaac.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Dacodelaac.Core
{
    [CreateAssetMenu]
    public class Ticker : ScriptableObject
    {
        [SerializeField] TickerMono tickerMonoPrefab;

        readonly TickList earlyTicks = new TickList();
        readonly TickList ticks = new TickList();
        readonly TickList lateTicks = new TickList();
        readonly TickList fixedTicks = new TickList();

        TickerMono tickerMono;

        public void SubEarlyTick(IEntity entity)
        {
            Validate();
            earlyTicks.Add(entity);
        }

        public void UnsubEarlyTick(IEntity entity)
        {
            earlyTicks.Remove(entity);
        }

        public void SubTick(IEntity entity)
        {
            Validate();
            ticks.Add(entity);
        }

        public void UnsubTick(IEntity entity)
        {
            ticks.Remove(entity);
        }

        public void SubLateTick(IEntity entity)
        {
            Validate();
            lateTicks.Add(entity);
        }

        public void UnsubLateTick(IEntity entity)
        {
            lateTicks.Remove(entity);
        }

        public void SubFixedTick(IEntity entity)
        {
            Validate();
            fixedTicks.Add(entity);
        }

        public void UnsubFixedTick(IEntity entity)
        {
            fixedTicks.Remove(entity);
        }

        public void EarlyTick()
        {
            earlyTicks.Compact();
            var count = earlyTicks.Count;
            for (var i = 0; i < count; i++) earlyTicks[i]?.EarlyTick();
        }

        public void Tick()
        {
            ticks.Compact();
            var count = ticks.Count;
            for (var i = 0; i < count; i++) ticks[i]?.Tick();
        }

        public void LateTick()
        {
            lateTicks.Compact();
            var count = lateTicks.Count;
            for (var i = 0; i < count; i++) lateTicks[i]?.LateTick();
        }

        public void FixedTick()
        {
            fixedTicks.Compact();
            var count = fixedTicks.Count;
            for (var i = 0; i < count; i++) fixedTicks[i]?.FixedTick();
        }

        void Validate()
        {
            if (tickerMono) return;

            tickerMono = Instantiate(tickerMonoPrefab);
            tickerMono.name = "Ticker";
            tickerMono.Ticker = this;

            /* Component riêng, execution order thấp hơn, để EarlyTick thật sự chạy trước mọi Update
               khác trong frame — thay vì chỉ chạy trước Tick vài dòng trong cùng một Update. */
            var early = tickerMono.gameObject.AddComponent<EarlyTickerMono>();
            early.Ticker = this;
        }

        /* Thay cho event Action: mỗi lần "+=" / "-=" trên multicast delegate cấp phát một mảng
           invocation-list mới, và "entity.Tick" còn tạo thêm một delegate object mỗi lần được nhắc
           tới. Pooled object bật/tắt liên tục ⇒ rác GC đều đặn.

           Danh sách này giữ lại đặc tính an toàn của delegate: xoá trong lúc đang duyệt chỉ đánh dấu
           null (không dồn mảng, nên index của phần tử khác không đổi), và phần tử thêm mới trong lúc
           duyệt sẽ không được gọi ở frame đó. */
        sealed class TickList
        {
            readonly List<IEntity> entities = new List<IEntity>();
            bool dirty;

            public int Count => entities.Count;
            public IEntity this[int index] => entities[index];

            public void Add(IEntity entity)
            {
                entities.Add(entity);
            }

            public void Remove(IEntity entity)
            {
                var index = entities.IndexOf(entity);
                if (index < 0) return;

                entities[index] = null;
                dirty = true;
            }

            /* Dọn các slot null. Chỉ gọi ngay trước vòng lặp, tức luôn ở ngoài lúc đang duyệt. */
            public void Compact()
            {
                if (!dirty) return;
                dirty = false;

                for (var i = entities.Count - 1; i >= 0; i--)
                {
                    if (entities[i] == null) entities.RemoveAt(i);
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Auto Bind")]
        public void AutoBind()
        {
            var soes = AssetUtils.FindAssetAtFolder<BaseSO>(new string[] {"Assets"});
            foreach (var so in soes)
            {
                so.SetTicker(this);
                EditorUtility.SetDirty(so);
            }

            var goes = AssetUtils.FindAssetAtFolder<GameObject>(new string[] {"Assets"});
            foreach (var go in goes)
            {
                var monoes = go.GetComponentsInChildren<BaseMono>(true);
                foreach (var mono in monoes)
                {
                    mono.SetTicker(this);
                    EditorUtility.SetDirty(mono);
                }
            }
        }
#endif
    }
}
