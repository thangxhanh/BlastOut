using Dacodelaac.Core;
using Dacodelaac.DataStorage;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Âm thanh gameplay. Giữ sẵn một vòng AudioSource và quay vòng dùng lại — nổ dây chuyền phát
       vài tiếng liền nhau, mà PlayOneShot trên một source đơn thì tiếng sau cắt ngang tiếng trước.

       Không dùng 3D sound: game gói gọn trong một màn hình, gắn âm theo vị trí chỉ làm tiếng nổ ở
       mép màn nhỏ đi vô cớ. */
    public class BlastAudio : BaseMono
    {
        [SerializeField] AudioSource[] sources;

        [Header("Clip")]
        [SerializeField] AudioClip shoot;
        [SerializeField] AudioClip explosion;
        [SerializeField] AudioClip impact;
        [SerializeField] AudioClip collect;
        [SerializeField] AudioClip win;
        [SerializeField] AudioClip lose;

        [Header("Âm lượng")]
        [SerializeField] float sfxVolume = 0.8f;
        [Tooltip("Tiếng nổ của thùng to hơn của đạn thường — khớp với việc nó cũng mạnh hơn.")]
        [SerializeField] float bigBlastRadius = 2.5f;

        [Tooltip("Khoảng cách tối thiểu giữa hai tiếng va chạm. Một vụ nổ hất mấy khối cùng lúc, " +
                 "không chặn thì chúng đập xuống gần như đồng thời và nghe thành một tiếng rè.")]
        [SerializeField] float impactCooldown = 0.08f;

        int next;
        float lastImpactTime = -999f;

        public void PlayShoot() => Play(shoot, 1f);

        public void PlayImpact()
        {
            if (Time.time - lastImpactTime < impactCooldown) return;
            lastImpactTime = Time.time;
            Play(impact, 0.7f);
        }

        public void PlayCollect() => Play(collect, 1f);
        public void PlayWin() => Play(win, 1f);
        public void PlayLose() => Play(lose, 1f);

        /* Nhận cả vị trí để khớp chữ ký sự kiện của BlastResolver, dù không dùng tới. */
        public void PlayExplosion(Vector2 position, float radius)
        {
            Play(explosion, radius >= bigBlastRadius ? 1f : 0.75f);
        }

        void Play(AudioClip clip, float scale)
        {
            /* Tôn trọng công tắc Sound có sẵn trong GameData, không tự tạo key riêng. */
            if (!clip || sources == null || sources.Length == 0 || !GameData.Sound) return;

            var source = sources[next];
            next = (next + 1) % sources.Length;

            /* Lệch cao độ mỗi lần phát: nghe mười tiếng nổ giống hệt nhau là ra ngay chất máy móc. */
            source.pitch = Random.Range(0.94f, 1.06f);
            source.PlayOneShot(clip, sfxVolume * scale);
        }
    }
}
