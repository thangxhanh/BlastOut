# Blast Out

Mobile physics puzzle — Unity 6000.3.24f1, Android portrait.

Mở `Assets/Dev/Scenes/GameScene.unity`, đặt Game view tỉ lệ dọc 9:16 rồi bấm Play.
Toàn bộ scene dựng lại được bằng một lệnh: **Tools → Blast Out → Build Game Scene**.

---

## 1. Game idea

Người chơi điều khiển một khẩu pháo, bắn đạn nổ để **hất các khối gỗ rơi xuống vùng thu** ở đáy màn.
Hết khối là thắng, hết đạn mà còn khối là thua.

Vòng chơi một lượt:

1. **Ngắm** — chạm và kéo ở *bất kỳ đâu* trên màn (kiểu ná), đường bay hiện sẵn bằng chuỗi chấm.
2. **Bắn** — thả tay.
3. **Quyết định giữa không trung** — chạm màn lần nữa để kích nổ đạn *ngay tại chỗ nó đang bay*.
4. **Chờ lắng** — game chỉ kết luận thắng/thua khi mọi thứ đã đứng yên, vì một khối còn đang lăn vẫn có thể rơi vào vùng thu.

Cố ý **không có hệ thống máu, không có khối vỡ vụn**. Khối chỉ bị đẩy. Nhờ vậy vật lý *đọc được*: người chơi nhìn là đoán được lực sẽ đẩy khối đi đâu, và mọi thất bại đều tự giải thích được.

6 level, mỗi màn thêm **đúng một** thứ mới buộc người chơi nghĩ khác đi:

| # | Thứ mới | Quyết định mà nó tạo ra |
|---|---|---|
| 1 | kéo–thả | *(không thể thua khi còn đạn — màn dạy chơi)* |
| 2 | kích nổ giữa không trung | hai khối xa nhau, bắn lẻ thì thiếu đạn → phải chọn **thời điểm nổ** |
| 3 | bệ chạy ngang | ngắm đúng mà sai nhịp vẫn trượt → phải chọn **lúc nào bắn** |
| 4 | thùng nổ dây chuyền | **1 viên, 2 khối** — bắn thẳng là chắc chắn thua → phải chọn **bắn cái gì** |
| 5 | pháo đứng trên bệ chạy | chính điểm bắn cũng di động → phải chọn **đứng đâu thì bắn** |
| 6 | đạn tách ba + bệ chạy vòng | tách sớm thì chùm toả quá rộng, tách muộn thì không với tới |

---

## 2. Gameplay additions

Ngoài aim-and-shoot, có **4** yếu tố thay đổi quyết định của người chơi:

1. **Kích nổ chủ động giữa không trung.** Chạm màn lúc đạn đang bay là nổ ngay tại đó. Đây là quyết định đắt nhất trong game: nổ sớm thì yếu, nổ muộn thì đạn đã chạm đất. Đạn chạm vật thể cũng tự nổ, nên *quên chạm không bao giờ làm mất lượt*.
2. **Thùng nổ dây chuyền.** Bán kính và lực lớn hơn hẳn đạn thường. Người chơi phải chọn **bắn cái gì trước**, không chỉ bắn vào đâu.
3. **Đạn tách ba (Splitter).** Lúc kích nổ thì tách thành 3 mảnh bay hình quạt thay vì nổ. Đổi câu hỏi từ "nổ ở đâu" sang "toả ra lúc nào".
4. **Bệ chuyển động** (ngang / dọc / vòng tròn). Thùng và khối đặt trên được ma sát kéo theo. Khẩu pháo cũng đứng được trên bệ chạy — khi đó *điểm bắn* thay đổi theo thời gian.

Thêm loại đạn mới = thêm một nhánh trong `BlastProjectile.Detonate`. Thêm quỹ đạo bệ mới = thêm một nhánh trong `PlatformMotion`. Cả hai đều không đụng tới luật thắng/thua.

---

## 3. Important decisions

**1. Luật chơi tách hẳn khỏi Unity.** `BlastSession` (Core) là C# thuần giữ phase và điều kiện thắng/thua — không biết prefab, không biết popup, chỉ báo ra "phase đã đổi". `BlastGameController` (View) nối input ↔ model ↔ thế giới vật lý. Ranh giới này là thứ đắt nhất nếu sửa sau, vì View bám vào prefab.

**2. Level là dữ liệu, không phải scene.** Mỗi level là một `BlastLevelConfig` (ScriptableObject); `BlastLevelSet` giữ thứ tự chơi. Thêm level = tạo thêm một asset rồi kéo vào mảng — **không viết một dòng gameplay nào, không tạo scene mới**. Toàn bộ số liệu vật lý nằm trong `BlastTuning`, chỉnh game feel không cần build lại.

**3. Một điểm vào duy nhất mỗi frame.** Chỉ `BlastGameController` đăng ký tick; mọi thứ khác được nó gọi xuống (`ManualTick`). Không component nào có `Update()` riêng. Nhờ vậy thứ tự trong một frame là đọc được, và bật/tắt cả gameplay chỉ là bật/tắt một object.

**4. Bệ chạy: hai lỗi khác nhau cùng làm khối tụt khỏi bệ.** Lỗi thứ nhất — đẩy bệ trong `Tick()` khiến vận tốc đặt lệch pha với lúc va chạm được giải; chuyển sang `FixedTick` thì trôi giảm từ `0.363 → 0.409` (tăng liên tục) xuống còn `0.128`. Lỗi thứ hai tinh vi hơn: quỹ đạo dạng `sin` có vị trí bằng 0 tại `t = 0` nhưng **vận tốc đã là cực đại**, nên bệ lao đi hết tốc lực ngay từ trạng thái đứng yên. Nhân biên độ với một hàm tăng dần (smoothstep) làm cả vận tốc lẫn gia tốc xuất phát từ 0 — trôi về đúng `0.000`. Nếu không sửa, khối tự rơi vào vùng thu khi người chơi chưa bắn phát nào.

**5. Cắt bỏ toàn bộ tầng meta.** Project gốc là template có sẵn Firebase, AdMob, AppLovin, Adjust, IAP, notification, anti-cheat. Đề ghi rõ những thứ này ngoài scope, nên **xoá hẳn** thay vì để đó: `Assets/` từ ~100MB còn **16MB**, và người đọc code không phải lội qua thứ không liên quan để tìm gameplay.

---

## 4. Technical & performance

### Đã kiểm tra gì

**Scenario nặng nhất: Level 4/6 — nổ dây chuyền.** Một viên đạn kích thùng nổ, thùng quét tiếp nhiều vật thể, mỗi vật thể nhận xung lực + mô-men xoắn, đồng thời 4 hệ hạt và một tiếng nổ phát cùng lúc. Đây là lúc nhiều thứ xảy ra nhất trong một frame.

Đã đo trong Editor (qua Unity MCP, đọc thẳng trạng thái runtime):

| Hạng mục | Trước | Sau |
|---|---|---|
| Chờ sau một cú bắn hỏng (đạn bay ra ngoài màn) | **6.4s** | **0.634s** |
| Khối trôi trên bệ chạy | 0.363 → 0.409, tăng liên tục | **0.000** |
| Vận tốc bệ tại thời điểm khởi động | 1.60 (cực đại ngay lập tức) | **0.00**, tăng dần |
| Level 4: nổ trúng khối | thùng nổ theo → thắng | thùng còn nguyên → **thua** (đúng ý đồ) |
| Level 4: nổ trúng thùng | thắng | **thắng** (đường thắng vẫn còn) |

### Rủi ro lớn nhất và cách xử lý

**Cấp phát trong lúc chơi.** Đây là nguồn giật hình số một trên mobile. Cách xử lý:
- Đạn qua `ProjectilePool`, hiệu ứng nổ qua vòng 6 bản dựng sẵn, âm thanh qua 4 `AudioSource` quay vòng — **không `Instantiate`/`Destroy` nào trong gameplay loop**.
- `BlastResolver` giữ sẵn buffer `List<Collider2D>` cho `OverlapCircle`, không cấp phát mỗi lần nổ.
- Trong `Tick()`: không `GetComponent`, không `Camera.main`, không LINQ. Mọi reference kéo sẵn bằng `[SerializeField]` lúc dựng scene.

**Shader bị strip khỏi build.** Material VFX tạo bằng `Shader.Find`, nhưng **chỉ trong editor tool** và kết quả là asset thật trên đĩa mà prefab tham chiếu tới. Gọi `Shader.Find` lúc chạy thì shader không được asset nào tham chiếu sẽ bị strip: Editor chạy đẹp, máy thật ra màu hồng.

**Treo game vì hitstop.** Khựng hình đặt `Time.timeScale = 0`; nếu đếm bằng `Time.deltaTime` thì đồng hồ đứng luôn và game treo vĩnh viễn. Toàn bộ phần này tick bằng `unscaledDeltaTime`, và `CleanUp()` trả `timeScale` về 1 phòng trường hợp thoát scene giữa lúc đang khựng. Đã verify: `1 → 0 → 1`, tự nhả.

**Quy mô hiện tại rất nhỏ** — mỗi level dưới 10 rigidbody, 2 draw call chính cho sprite. Đây là lý do chưa cần tối ưu thêm: chưa có vấn đề thật để giải.

### ⚠️ Chưa làm — cần chạy trước khi submit

Ba thứ dưới đây **chưa chạy** và không thể kết luận thay bằng số liệu Editor:

- **Build APK lên máy thật** — chỉ ở đây mới lộ ra shader strip, nén texture và RAM thật.
- **Memory Profiler** — chụp 2 snapshot cách nhau vài level, so số lượng `Material` và `Texture2D`. Tăng đều theo level = rò rỉ.
- **Frame Debugger** — đếm draw call thật; sprite hiện chưa gom vào Sprite Atlas.

---

## 5. AI / Tool usage

Dùng **Claude Code** kết hợp **Unity MCP** (điều khiển Editor từ agent). Vài chỗ nó thật sự rút ngắn thời gian:

- **Dựng scene và prefab bằng code** (`Assets/Dev/Scripts/Editor/BlastOut/`). Scene/prefab là asset nhị phân, diff git không đọc được. Dựng bằng code thì "đã đổi gì" hiện rõ trong diff, và dựng lại được y hệt trên máy khác bằng một lệnh menu.
- **Đo trạng thái runtime trực tiếp** thay vì đoán bằng mắt. Các số ở mục 4 — độ trôi của khối, thời gian chờ, đường kính vòng xung kích — đều lấy bằng cách chạy code trong Play mode qua MCP. Bug "bệ chạy theo nhịp render" được tìm ra theo cách này, nhìn bằng mắt thì chỉ thấy "hơi lạ".
- **Tìm và thẩm định asset CC0**, đọc file license trong từng pack trước khi đưa vào repo.

Những chỗ output của AI **bị sửa hoặc bỏ**: bố cục level (phải tự chơi mới biết Level 4 tự giải), bộ số trong `BlastTuning` (đạn ban đầu quá mạnh, bắn đâu cũng trúng), và chọn sprite (tên file Kenney đánh số nên phải mở từng ảnh ra xem).

---

## 6. Nếu có thêm 24 giờ

Theo thứ tự ưu tiên:

1. **Profile trên máy thật + Sprite Atlas.** *Vấn đề:* toàn bộ kết luận hiệu năng hiện dựa trên Editor, mà Editor không nói gì về shader strip, nén texture hay RAM thật. *Vì sao quan trọng với người chơi:* một cú giật hình đúng lúc vụ nổ làm hỏng chính khoảnh khắc mà cả game xây dựng để dẫn tới. *Kỳ vọng:* xác nhận 60fps ổn định trên máy tầm trung, và biết chắc không rò rỉ material.

2. **Chơi thử với người thật rồi cân lại độ khó.** *Vấn đề:* thứ tự 6 level đang dựa trên suy luận thiết kế, chưa có ai ngoài tác giả chơi. *Vì sao quan trọng:* Level 4 từng *tự giải* mà chỉ khi chơi mới lộ ra — những chỗ như thế không thể tìm bằng đọc code. *Kỳ vọng:* biết được màn nào làm người chơi bỏ cuộc, và sửa bố cục thay vì sửa số.

3. **Khoảnh khắc thắng.** *Vấn đề:* thắng hiện chỉ là một dòng chữ hiện ra. *Vì sao quan trọng:* đây là phần thưởng cho toàn bộ chuỗi quyết định — nó đang là phần nhạt nhất trong game. *Kỳ vọng:* slow-motion ngắn khi khối cuối rơi vào vùng thu, cộng camera dõi theo nó.

## Nếu chỉ có 24 giờ

**Giữ bằng mọi giá:** vòng chơi hoàn chỉnh (ngắm → bắn → kích nổ → thắng/thua → restart nhanh), Level 1 đủ rõ để tự hiểu cách chơi, và **APK chạy được**. Một game nhỏ mà chơi trọn vẹn vẫn hơn hẳn một project lớn dở dang.

**Giảm scope:** rút từ 6 xuống 3 level — giữ Level 1 (dạy chơi), Level 3 (bệ chạy) và Level 4 (thùng nổ), vì ba màn này đã đủ chứng minh có *decision* thật chứ không chỉ tăng độ khó ngắm bắn.

**Bỏ:** đạn tách ba, bệ chạy vòng tròn, và toàn bộ phần đánh bóng hiệu ứng — cắt được nhiều giờ mà không đụng tới thứ làm nên trải nghiệm lõi.

---

## Asset

Toàn bộ art và âm thanh đều **CC0** (public domain, dùng được cả thương mại). File license gốc đi kèm trong từng thư mục.

| Nguồn | Dùng cho |
|---|---|
| [Kenney — Physics Assets](https://kenney.nl/assets/physics-assets) | khối gỗ, bệ kim loại |
| [Kenney — Top-down Tanks Redux](https://opengameart.org/content/top-down-tanks-redux) | thân + nòng pháo, thùng phuy |
| [Kenney — UI Pack](https://kenney.nl/assets/ui-pack) | nút bấm |
| [Kenney — Particle Pack](https://kenney.nl/assets/particle-pack) | flash, vòng xung kích, khói, tia lửa |
| [Kenney — Interface Sounds](https://kenney.nl/assets/interface-sounds) | âm thanh thắng/thua |
| [100 CC0 SFX](https://opengameart.org/content/100-cc0-sfx) | tiếng bắn, nổ, va chạm, thu khối |
