using Dacodelaac.Collections;
using UnityEngine;

namespace Dacodelaac.Core
{
    public class BaseTickMono : BaseMono, ITick
    {
        [SerializeField] TickCollection earlyTickCollection;
        [SerializeField] TickCollection tickCollection;
        [SerializeField] TickCollection lateTickCollection;
        [SerializeField] TickCollection fixedTickCollection;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (earlyTickCollection) earlyTickCollection.Add(this);
            if (tickCollection) tickCollection.Add(this);
            if (lateTickCollection) lateTickCollection.Add(this);
            if (fixedTickCollection) fixedTickCollection.Add(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (earlyTickCollection) earlyTickCollection.Remove(this);
            if (tickCollection) tickCollection.Remove(this);
            if (lateTickCollection) lateTickCollection.Remove(this);
            if (fixedTickCollection) fixedTickCollection.Remove(this);
        }

        /* override chứ không phải khai báo mới: nếu tạo slot virtual thứ hai thì đường IEntity
           (Ticker gọi) và đường ITick (TickCollection gọi) sẽ trỏ vào hai thân hàm khác nhau. */
        public override void EarlyTick()
        {
        }

        public override void Tick()
        {
        }

        public override void LateTick()
        {
        }

        public override void FixedTick()
        {
        }
    }
}