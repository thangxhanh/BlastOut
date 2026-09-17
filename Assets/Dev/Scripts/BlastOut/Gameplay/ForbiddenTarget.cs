using System;
using Dacodelaac.Core;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Mục tiêu CẤM chạm: vụ nổ với tới nó là thua ngay.

       Đây là đối trọng của toàn bộ phần còn lại. Không có nó thì mọi quyết định đều quy về "làm sao
       văng được nhiều nhất" — sức mạnh không có mặt trái. Có nó thì bán kính nổ trở thành một vùng
       phải NÉ, và vòng xung kích vẽ đúng tầm sát thương mới thật sự có ích.

       Cố ý KHÔNG có Rigidbody2D: nó đứng yên tuyệt đối, vừa là vật cản vừa là mốc cố định để người
       chơi tính đường. Một mục tiêu cấm bị đẩy trôi đi thì không ai tính trước được điều gì. */
    public class ForbiddenTarget : BaseMono, IBlastable
    {
        [SerializeField] SpriteRenderer view;

        Action hit;
        bool spent;

        /* Builder nối lại mỗi lần dựng level — object bị huỷ và tạo lại nên không giữ đăng ký cũ. */
        public void BindHit(Action handler)
        {
            hit = handler;
            spent = false;
        }

        public void ApplyBlast(Vector2 origin, float force, float radius)
        {
            /* Một lần là đủ: nổ dây chuyền có thể gọi tới đây nhiều lần trong cùng một khoảnh khắc,
               mà thua thì chỉ thua một lần. */
            if (spent) return;
            if (Vector2.Distance((Vector2)transform.position, origin) > radius) return;

            spent = true;
            if (view) view.color = Color.red;
            hit?.Invoke();
        }
    }
}
