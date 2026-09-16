using UnityEngine;

namespace Dacodelaac.Core
{
    public class TickerMono : MonoBehaviour
    {
        public Ticker Ticker { get; set; }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
            CancelInvoke();
        }

        /* EarlyTick nằm ở EarlyTickerMono (execution order thấp hơn), không gọi ở đây. */
        void Update()
        {
            if (Ticker) Ticker.Tick();
        }

        void FixedUpdate()
        {
            if (Ticker) Ticker.FixedTick();
        }

        void LateUpdate()
        {
            if (Ticker) Ticker.LateTick();
        }
    }
}
