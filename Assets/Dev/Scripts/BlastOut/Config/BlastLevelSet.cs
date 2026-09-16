using Dacodelaac.Core;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Config
{
    /* Chỗ lưu toàn bộ level theo đúng thứ tự chơi. Thêm level, bớt level hay đổi thứ tự chỉ là
       sửa mảng này trong Inspector — gameplay không đổi một dòng nào. Controller đọc level đang
       chơi từ đây theo tiến độ, thay vì mỗi màn cắm cứng một config. */
    [CreateAssetMenu(menuName = "BlastOut/Level Set", fileName = "level_set")]
    public class BlastLevelSet : BaseSO
    {
        [Tooltip("Thứ tự trong mảng = thứ tự chơi. Phần tử 0 là level đầu tiên.")]
        [SerializeField] BlastLevelConfig[] levels;

        public int Count => levels?.Length ?? 0;

        /* Trả về level ở index, tự kẹp trong khoảng hợp lệ để một index hỏng không làm crash —
           thà chơi lại level cuối còn hơn ném exception giữa lượt. */
        public BlastLevelConfig Get(int index)
        {
            if (Count == 0) return null;
            return levels[Mathf.Clamp(index, 0, Count - 1)];
        }
    }
}
