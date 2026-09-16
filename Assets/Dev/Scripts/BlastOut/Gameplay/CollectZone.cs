using System;
using Dacodelaac.Core;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Dải trigger ở đáy màn. Khối rơi vào đây là "đã thu".
       Zone chỉ báo ra sự kiện — nó không biết luật thắng thua, không cộng điểm, không gọi popup
       (CLAUDE.md §9.3: gameplay chỉ báo ra thắng/thua). */
    [RequireComponent(typeof(BoxCollider2D))]
    public class CollectZone : BaseMono
    {
        [SerializeField] BoxCollider2D area;
        [Tooltip("Mảng màu cho người chơi thấy vùng thu ở đâu. Được giãn khớp với collider.")]
        [SerializeField] Transform visual;

        public event Action<TargetBlock> BlockCollected;

        public void SetTopEdge(float topY, float width)
        {
            /* Vùng thu kéo sâu xuống dưới đáy camera để khối bay nhanh không xuyên qua giữa hai frame. */
            const float depth = 6f;

            area.size = new Vector2(width, depth);
            area.offset = Vector2.zero;
            transform.position = new Vector3(0f, topY - depth * 0.5f, 0f);

            /* Vùng thu phải NHÌN THẤY được: mục tiêu của level đọc bằng mắt, không cần chữ. */
            if (visual) visual.localScale = new Vector3(width, depth, 1f);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<TargetBlock>(out var block)) return;

            block.gameObject.SetActive(false);
            BlockCollected?.Invoke(block);
        }
    }
}
