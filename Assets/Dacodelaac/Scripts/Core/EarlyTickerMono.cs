using Dacodelaac.Core;
using UnityEngine;

namespace Dacodelaac.Scripts.Core
{
    /* Tách khỏi TickerMono chỉ vì execution order: -1000 đảm bảo EarlyTick chạy trước mọi Update
       khác trong frame, kể cả các script dùng ExecutionOrder.First (-3). Ticker tự AddComponent
       component này lên GameObject của TickerMono. */
    [DefaultExecutionOrder(-1000)]
    public class EarlyTickerMono : MonoBehaviour
    {
        public Ticker Ticker { get; set; }

        void Update()
        {
            if (Ticker) Ticker.EarlyTick();
        }
    }
}
