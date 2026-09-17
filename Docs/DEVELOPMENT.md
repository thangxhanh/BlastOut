# Blast Out — ghi chú cho người tiếp tục dự án

Phần này không nằm trong yêu cầu bài test; nó dành cho người mở project lên và cần sửa gì đó.
Nội dung trả lời đề bài nằm ở [README.md](../README.md).

---

## Chạy thử

Mở `Assets/Dev/Scenes/GameScene.unity`, đặt Game view sang một tỉ lệ **dọc** bất kỳ rồi bấm Play.
Camera khớp theo bề ngang nên mọi tỉ lệ từ 4:3 đến 9:21 đều thấy đủ bố cục.

Điều khiển: kéo ở bất kỳ đâu để ngắm, thả để bắn, chạm lần nữa lúc đạn đang bay để kích nổ.

---

## Dựng lại scene

**Tools → Blast Out → Build Game Scene**

Lệnh này dựng lại từ đầu: sprite hình học, prefab, hiệu ứng, HUD, và toàn bộ 9 level. Chạy được
nhiều lần, kết quả giống hệt nhau.

Vì sao dựng bằng code thay vì làm tay trong Editor:

- Scene và prefab là asset nhị phân — diff git không đọc được. Dựng bằng code thì *"đã đổi gì"*
  hiện rõ trong diff.
- Dựng lại được y hệt trên máy khác bằng một lệnh, không phụ thuộc thao tác tay.

**Lưu ý:** lệnh này bị chặn khi Editor đang ở Play mode, và nó *im lặng* không chạy — chỉ có dòng
`InvalidOperationException: This cannot be used during play mode` trong Console. Thoát Play trước.

Code ở `Assets/Dev/Scripts/Editor/BlastOut/`:

| File | Việc |
|---|---|
| `BlastOutSceneBuilder` | dựng scene, nối mọi thành phần lại |
| `BlastOutPrefabFactory` | prefab: khối, thùng nổ, bệ, đạn, mục tiêu cấm |
| `BlastOutVfxFactory` | hệ hạt: vụ nổ bốn lớp, mảnh vỡ |
| `BlastOutLevelFactory` | 9 level |
| `BlastOutAssetFactory` | sprite hình học, bảng màu, `BlastTuning` |
| `SerializedFieldWriter` | gán field `private` khi dựng bằng code |

---

## Sửa level

Chọn một asset trong `Assets/Dev/Data/BlastOut/` rồi nhìn sang **Scene View**: cả bố cục hiện ra và
kéo được bằng chuột. Sửa tới đâu ghi vào asset tới đó, không có bước load/save.

Các vòng vẽ kèm mới là thứ đáng nhìn:

| Vòng | Ý nghĩa |
|---|---|
| **đỏ** | vùng **không được nổ** quanh mục tiêu cấm |
| **cam** | tầm nổ của thùng — biết dây chuyền có lan sang thứ không nên chạm không |
| **lam** | đường đi của bệ chạy |

Level 7 từng *không thể thắng* vì vùng cấm nuốt gần hết chỗ được phép nổ. Đọc dãy toạ độ trong
Inspector thì không thấy; nhìn hai vòng chồng nhau thì thấy ngay.

### Hai ràng buộc phải tự kiểm khi đổi bố cục

1. **Mọi thứ phải nằm trong ±4.9 theo trục X.** Đây là `requiredHalfWidth` trên `BlastGameController`
   — bề ngang mà camera bảo đảm luôn nhìn thấy ở mọi tỉ lệ màn hình. Với bệ chạy, tính cả biên độ:
   `|tâm| + nửa rộng + biên độ`.

2. **Cửa sổ nổ hợp lệ phải đủ rộng.** Khi level có mục tiêu cấm, điểm nổ bị kẹp giữa hai cận:
   - xa mục tiêu cấm hơn `BlastRadius`, và
   - cách khối ít nhất `0.25` (gần hơn thì `TargetBlock` hất **thẳng lên** thay vì sang ngang, khối
     rơi lại đúng chỗ cũ).

   Hai cận này từng chồng nhau chỉ còn `0.05` unit ở Level 7 — level trông hợp lý nhưng không ai
   thắng được.

---

## Kiến trúc

```
Assets/Dev/Scripts/BlastOut/
├── Core/        C# thuần, không biết Unity — luật chơi
│   ├── BlastSession      phase + điều kiện thắng/thua
│   ├── PlatformMotion    quỹ đạo bệ chạy, tính thuần từ thời gian
│   ├── BlastPhase, AmmoType
├── Config/      ScriptableObject — dữ liệu
│   ├── BlastLevelConfig  một level
│   ├── BlastLevelSet     thứ tự chơi
│   └── BlastTuning       toàn bộ số liệu vật lý
├── Gameplay/    MonoBehaviour — thế giới vật lý và hiển thị
└── UI/          HUD
```

Hai điều dễ phá vỡ nếu không biết:

- **Chỉ `BlastGameController` đăng ký tick.** Mọi thứ khác được nó gọi xuống (`ManualTick`). Đừng
  thêm `Update()` vào component gameplay — thứ tự trong một frame sẽ không còn đọc được.
- **Bệ chạy phải chạy ở `FixedTick`.** Đẩy theo nhịp render thì vận tốc đặt lệch pha với lúc va chạm
  được giải, và vật đặt trên tụt dần khỏi bệ dù ma sát đã tối đa.

---

## Asset

Toàn bộ art và âm thanh đều **CC0** — public domain, dùng được cả cho mục đích thương mại.

| Nguồn | Dùng cho |
|---|---|
| [Kenney — Physics Assets](https://kenney.nl/assets/physics-assets) | khối gỗ, bệ kim loại |
| [Kenney — Top-down Tanks Redux](https://opengameart.org/content/top-down-tanks-redux) | thân + nòng pháo, thùng phuy |
| [Kenney — UI Pack](https://kenney.nl/assets/ui-pack) | nút bấm |
| [Kenney — Particle Pack](https://kenney.nl/assets/particle-pack) | flash, vòng xung kích, khói, tia lửa |
| [Kenney — Interface Sounds](https://kenney.nl/assets/interface-sounds) | âm thanh thắng/thua |
| [100 CC0 SFX](https://opengameart.org/content/100-cc0-sfx) | tiếng bắn, nổ, va chạm, thu khối |

Các pack Kenney có kèm file license gốc trong thư mục sprite. Hai pack âm thanh không kèm file nào
trong bản tải về, nên nguồn từng file được ghi lại ở
`Assets/Dev/Audio/SFX/BlastOut/CREDITS.txt`.
