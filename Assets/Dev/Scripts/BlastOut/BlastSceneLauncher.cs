using Dacodelaac.Core;
using UnityEngine;

namespace Dev.Scripts.BlastOut
{
    /* Điểm khởi động của scene gameplay.

       BaseLauncher chỉ khởi tạo những BaseMono nằm trong prefab mà nó spawn — nó không biết gì về
       object có sẵn trong scene. Scene test này dựng thẳng object trong scene (nhìn thấy và sửa
       được ngay trong Editor, không phải mở prefab), nên cần thêm một bước: gọi Initialize theo
       ĐÚNG THỨ TỰ khai báo trong mảng dưới đây.

       Thứ tự là thứ quan trọng nhất ở đây: BlastGameController phải chạy sau cùng vì nó đọc
       reference của những thành phần còn lại. */
    public class BlastSceneLauncher : BaseLauncher
    {
        [Tooltip("Khởi tạo theo đúng thứ tự trong mảng. Controller để cuối cùng.")]
        [SerializeField] BaseMono[] sceneSystems;

        void Start()
        {
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();

            for (var i = 0; i < sceneSystems.Length; i++)
            {
                if (sceneSystems[i]) sceneSystems[i].Initialize();
            }
        }
    }
}
