using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Điểm cắm cho mọi thứ chịu được sóng nổ.
       Đây là một trong bốn thứ CLAUDE.md §9.3 bắt định nghĩa ngay từ đầu: đã biết chắc game sẽ có
       thêm loại vật thể phản ứng với vụ nổ (khối, thùng nổ, sau này là cột chống / bệ di động),
       nên tách interface ngay thay vì để BlastResolver phải biết từng loại cụ thể.

       Thêm loại mới = implement interface này, không sửa BlastResolver. */
    public interface IBlastable
    {
        void ApplyBlast(Vector2 origin, float force, float radius);
    }
}
