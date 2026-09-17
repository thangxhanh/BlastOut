# Blast Out

Mobile physics puzzle — Unity 6000.3.24f1, Android portrait.

> Cách chạy, cách dựng lại scene, công cụ sửa level và nguồn asset: xem
> [Docs/DEVELOPMENT.md](Docs/DEVELOPMENT.md).

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

9 level. Bảy màn đầu mỗi màn thêm **đúng một** thứ mới buộc người chơi nghĩ khác đi:

| # | Thứ mới | Quyết định mà nó tạo ra |
|---|---|---|
| 1 | kéo–thả | *(không thể thua khi còn đạn — màn dạy chơi)* |
| 2 | kích nổ giữa không trung | hai khối xa nhau, bắn lẻ thì thiếu đạn → phải chọn **thời điểm nổ** |
| 3 | bệ chạy ngang | ngắm đúng mà sai nhịp vẫn trượt → phải chọn **lúc nào bắn** |
| 4 | thùng nổ dây chuyền | **1 viên, 2 khối** — bắn thẳng là chắc chắn thua → phải chọn **bắn cái gì** |
| 5 | pháo đứng trên bệ chạy | chính điểm bắn cũng di động → phải chọn **đứng đâu thì bắn** |
| 6 | đạn tách ba + bệ chạy vòng | tách sớm thì chùm toả quá rộng, tách muộn thì không với tới |
| 7 | mục tiêu **cấm** chạm | lần đầu "nổ càng gần càng tốt" là **sai** → phải né bán kính của chính mình |

Hai màn cuối không dạy gì mới — chúng bắt dùng **hai thứ đã học cùng lúc**, và chỉ ở đó người chơi mới phải cân nhắc đánh đổi thay vì áp dụng một quy tắc:

| # | Kết hợp | Quyết định |
|---|---|---|
| 8 | bệ chạy + mục tiêu cấm | khối đi qua đi lại, có lúc ở gần thứ không được chạm → không chỉ cần *thời điểm đúng* mà phải là *thời điểm an toàn* |
| 9 | thùng dây chuyền + mục tiêu cấm | hai thùng nhìn y hệt nhau, **một viên đạn**: thùng nào cũng hạ được khối, nhưng một cái nổ ra là thua |

---

## 2. Gameplay additions

Ngoài aim-and-shoot, có **5** yếu tố thay đổi quyết định của người chơi:

1. **Kích nổ chủ động giữa không trung.** Chạm màn lúc đạn đang bay là nổ ngay tại đó. Đây là quyết định đắt nhất trong game: nổ sớm thì yếu, nổ muộn thì đạn đã chạm đất. Đạn chạm vật thể cũng tự nổ, nên *quên chạm không bao giờ làm mất lượt*.
2. **Thùng nổ dây chuyền.** Bán kính và lực lớn hơn hẳn đạn thường. Người chơi phải chọn **bắn cái gì trước**, không chỉ bắn vào đâu.
3. **Đạn tách ba (Splitter).** Lúc kích nổ thì tách thành 3 mảnh bay hình quạt thay vì nổ. Đổi câu hỏi từ "nổ ở đâu" sang "toả ra lúc nào".
4. **Bệ chuyển động** (ngang / dọc / vòng tròn). Thùng và khối đặt trên được ma sát kéo theo. Khẩu pháo cũng đứng được trên bệ chạy — khi đó *điểm bắn* thay đổi theo thời gian.
5. **Mục tiêu cấm chạm.** Vụ nổ với tới nó là thua ngay. Đây là đối trọng của tất cả những thứ trên: không có nó thì mọi quyết định đều quy về *"làm sao văng được nhiều nhất"* — **sức mạnh không có mặt trái**. Có nó thì bán kính nổ trở thành một vùng phải **né**, và vòng xung kích vẽ đúng tầm sát thương mới thật sự có ích thay vì chỉ để đẹp.

Thêm loại đạn mới = thêm một nhánh trong `BlastProjectile.Detonate`. Thêm quỹ đạo bệ mới = thêm một nhánh trong `PlatformMotion`. Cả hai đều không đụng tới luật thắng/thua.

---

## 3. Important decisions

**1. Luật chơi tách hẳn khỏi Unity.** `BlastSession` (Core) là C# thuần giữ phase và điều kiện thắng/thua — không biết prefab, không biết popup, chỉ báo ra "phase đã đổi". `BlastGameController` (View) nối input ↔ model ↔ thế giới vật lý. Ranh giới này là thứ đắt nhất nếu sửa sau, vì View bám vào prefab.

**2. Level là dữ liệu, không phải scene.** Mỗi level là một `BlastLevelConfig` (ScriptableObject); `BlastLevelSet` giữ thứ tự chơi. Thêm level = tạo thêm một asset rồi kéo vào mảng — **không viết một dòng gameplay nào, không tạo scene mới**. Toàn bộ số liệu vật lý nằm trong `BlastTuning`, chỉnh game feel không cần build lại.

**3. Một điểm vào duy nhất mỗi frame.** Chỉ `BlastGameController` đăng ký tick; mọi thứ khác được nó gọi xuống (`ManualTick`). Không component nào có `Update()` riêng. Nhờ vậy thứ tự trong một frame là đọc được, và bật/tắt cả gameplay chỉ là bật/tắt một object.

**4. Bệ chạy: hai lỗi khác nhau cùng làm khối tụt khỏi bệ.** Lỗi thứ nhất — đẩy bệ trong `Tick()` khiến vận tốc đặt lệch pha với lúc va chạm được giải; chuyển sang `FixedTick` thì trôi giảm từ `0.363 → 0.409` (tăng liên tục) xuống còn `0.128`. Lỗi thứ hai tinh vi hơn: quỹ đạo dạng `sin` có vị trí bằng 0 tại `t = 0` nhưng **vận tốc đã là cực đại**, nên bệ lao đi hết tốc lực ngay từ trạng thái đứng yên. Nhân biên độ với một hàm tăng dần (smoothstep) làm cả vận tốc lẫn gia tốc xuất phát từ 0 — trôi về đúng `0.000`. Nếu không sửa, khối tự rơi vào vùng thu khi người chơi chưa bắn phát nào.

---

## 4. Technical & performance

### Đã kiểm tra gì

**Scenario nặng nhất: nổ dây chuyền (Level 4, 6, 9).** Một viên đạn kích thùng nổ, thùng quét tiếp nhiều vật thể, mỗi vật thể nhận xung lực + mô-men xoắn, đồng thời bốn hệ hạt, mảnh vỡ và một tiếng nổ phát cùng lúc. Đây là lúc nhiều thứ xảy ra nhất trong một frame.

Đã đo trong Editor, đọc thẳng trạng thái runtime qua Unity MCP:

| Hạng mục | Trước | Sau |
|---|---|---|
| Chờ sau một cú bắn hỏng (đạn bay ra ngoài màn) | **6.4s** | **0.634s** |
| Khối trôi trên bệ chạy | 0.363 → 0.409, tăng liên tục | **0.000** |
| Vận tốc bệ tại thời điểm khởi động | 1.60 (cực đại ngay lập tức) | **0.00**, tăng dần |
| Level 4: nổ trúng khối | thùng nổ theo → thắng | thùng còn nguyên → **thua** (đúng ý đồ) |
| Level 7: cửa sổ nổ hợp lệ | **0.05** unit (không ai thắng nổi) | **0.85** unit |

### Hai lỗi đáng giá nhất — đều không thấy được khi đọc code

**Bố cục tràn ra ngoài màn.** `orthographicSize` là nửa chiều *cao*, nên để cố định một giá trị thì màn càng dài khung nhìn càng **hẹp**: 9:16 cho 4.50 unit mỗi bên, 9:19.5 chỉ còn 3.69, 9:21 còn 3.43. Level dựng theo 9:16 nên **ở 9:19.5 — tỉ lệ của phần lớn máy Android hiện nay — cả 9 level đều có vật thể nằm ngoài màn**; Level 5 thậm chí đã tràn ngay ở 9:16.

Sửa bằng cách khớp camera theo **bề ngang**: `size = max(minHalfHeight, requiredHalfWidth / aspect)`. Vẫn giữ sàn chiều cao, vì trên màn vuông mà chỉ khớp bề ngang thì khung lùn lại và vùng thu ở đáy bị đẩy ra ngoài. Kiểm lại cả 9 level trên 7 tỉ lệ (4:3 → 9:21): mép xa nhất 4.70, nằm trong ngưỡng 4.9.

**`Rigidbody2D.position` chưa đồng bộ.** Vụ nổ đo khoảng cách bằng `body.position`, nhưng level builder đặt vị trí qua `transform` và rigidbody chỉ chép lại ở *bước vật lý kế tiếp*. Nên ở lượt bắn đầu tiên, mọi vật thể đều còn mang toạ độ `(0, 0)`:

```
A transform=(-1.20, -0.19) | body=(0.00, 0.00)
B transform=( 2.40, -0.19) | body=(0.00, 0.00)
```

Hệ quả: vụ nổ "với tới" những thứ ở rất xa, khối bị đẩy sai cả hướng lẫn độ mạnh. Triệu chứng nhìn thấy chỉ là *"đạn hơi mạnh"* — phải in hai toạ độ cạnh nhau mới ra nguyên nhân. Đổi sang đo bằng `transform.position` ở cả `TargetBlock`, `ExplosiveBarrel` và `BlastProjectile`.

### Rủi ro lớn nhất và cách xử lý

**Cấp phát trong lúc chơi** — nguồn giật hình số một trên mobile:
- Đạn qua `ProjectilePool`, hiệu ứng nổ qua vòng 6 bản dựng sẵn, mảnh vỡ 3 bản, âm thanh 4 `AudioSource` quay vòng — **không `Instantiate`/`Destroy` nào trong gameplay loop**.
- `BlastResolver` giữ sẵn buffer `List<Collider2D>` cho `OverlapCircle`, không cấp phát mỗi lần nổ.
- Trong `Tick()`: không `GetComponent`, không `Camera.main`, không LINQ. Mọi reference kéo sẵn bằng `[SerializeField]` lúc dựng scene.

**Shader bị strip khỏi build.** Material VFX tạo bằng `Shader.Find`, nhưng **chỉ trong editor tool** và kết quả là asset thật trên đĩa mà prefab tham chiếu tới. Gọi `Shader.Find` lúc chạy thì shader không được asset nào tham chiếu sẽ bị strip: Editor chạy đẹp, máy thật ra màu hồng.

**Treo game vì hitstop.** Khựng hình đặt `Time.timeScale = 0`; nếu đếm bằng `Time.deltaTime` thì đồng hồ đứng luôn và game treo vĩnh viễn. Toàn bộ phần này tick bằng `unscaledDeltaTime`, và `CleanUp()` trả `timeScale` về 1 phòng trường hợp thoát scene giữa lúc đang khựng. Đã verify: `1 → 0 → 1`, tự nhả.

**Quy mô hiện tại rất nhỏ** — mỗi level dưới 10 rigidbody động, vài chục collider, một hệ hạt bốn lớp lúc nổ. Đây là lý do chưa tối ưu thêm: chưa có vấn đề thật để giải. Số draw call thì **chưa đo** — sprite chưa gom Sprite Atlas nên gần như chắc chắn còn giảm được, nhưng nói một con số cụ thể lúc này chỉ là đoán.

### ⚠️ Chưa làm

Ba thứ dưới đây **chưa chạy**, và không thể kết luận thay bằng số liệu Editor:

- **Chơi APK trên máy thật** — chỉ ở đây mới lộ ra shader strip, nén texture và RAM thật.
- **Memory Profiler** — chụp 2 snapshot cách nhau vài level, so số lượng `Material` và `Texture2D`. Tăng đều theo level = rò rỉ.
- **Frame Debugger** — đếm draw call thật.

---

## 5. AI / Tool usage

Dùng **Claude Code** kết hợp **Unity MCP** (điều khiển Editor từ agent). Vài chỗ nó thật sự rút ngắn thời gian:

- **Dựng scene và prefab bằng code.** Scene/prefab là asset nhị phân, diff git không đọc được. Dựng bằng code thì *"đã đổi gì"* hiện rõ trong diff, và dựng lại được y hệt trên máy khác bằng một lệnh menu.
- **Đo trạng thái runtime trực tiếp** thay vì đoán bằng mắt. Mọi số ở mục 4 đều lấy bằng cách chạy code trong Play mode. Ba lỗi nặng nhất tìm ra theo cách này, và cả ba đều **không nhìn thấy được**:
  - *bệ chạy theo nhịp render* — nhìn bằng mắt chỉ thấy "hơi lạ", so số mới thấy độ trôi tăng đều;
  - *`body.position` chưa đồng bộ* — triệu chứng là "đạn hơi mạnh", phải in hai toạ độ cạnh nhau mới lộ;
  - *bố cục tràn màn* — ở tỉ lệ Editor thì hoàn toàn bình thường.
- **Tìm và thẩm định asset CC0**, đọc file license trong từng pack trước khi đưa vào repo.

Ngược lại, có một loại lỗi mà công cụ đo **không bắt được**, và cả ba lần đều do người chơi thật phát hiện: Level 4 *tự giải*, Level 7 *không thể thắng*, đạn tách *nhìn không ra là đã tách*. Điểm chung là chúng đúng về mặt kỹ thuật — số liệu đẹp, không lỗi nào — nhưng sai về mặt trải nghiệm. Có lần tôi còn "verify" cú tách đạn thành công vì đếm đúng 3 mảnh, trong khi cả ba nổ ngay frame sau đó.

Những chỗ output của AI **bị sửa hoặc bỏ**: bố cục level, bộ số trong `BlastTuning` (đạn ban đầu quá mạnh, bắn đâu cũng trúng), chọn sprite (tên file Kenney đánh số nên phải mở từng ảnh ra xem), và cách báo trạng thái chờ (bản đầu làm mờ cả khẩu pháo — tô đỏ riêng nòng đọc ra *"vừa bắn, còn nóng"* thay vì chỉ *"đang khoá"*).

---

## 6. Nếu có thêm 24 giờ

Theo thứ tự ưu tiên:

1. **Profile trên máy thật + Sprite Atlas.** *Vấn đề:* toàn bộ kết luận hiệu năng hiện dựa trên Editor, mà Editor không nói gì về shader strip, nén texture hay RAM thật. *Vì sao quan trọng với người chơi:* một cú giật hình đúng lúc vụ nổ làm hỏng chính khoảnh khắc mà cả game xây dựng để dẫn tới. *Kỳ vọng:* xác nhận 60fps ổn định trên máy tầm trung, và biết chắc không rò rỉ material.

2. **Chơi thử với người thật rồi cân lại độ khó.** *Vấn đề:* thứ tự 9 level dựa trên suy luận thiết kế, chưa có ai ngoài tác giả chơi hết. *Vì sao quan trọng:* Level 4 từng *tự giải* và Level 7 từng *không thể thắng* — cả hai chỉ lộ ra khi chơi, đọc code không thấy. *Kỳ vọng:* biết màn nào làm người chơi bỏ cuộc, và sửa bố cục thay vì sửa số.

3. **Khoảnh khắc thắng.** *Vấn đề:* thắng hiện chỉ là một dòng chữ hiện ra. *Vì sao quan trọng:* đây là phần thưởng cho toàn bộ chuỗi quyết định — nó đang là phần nhạt nhất trong game. *Kỳ vọng:* slow-motion ngắn khi khối cuối rơi vào vùng thu, cộng camera dõi theo nó.

## Nếu chỉ có 24 giờ

**Giữ bằng mọi giá:** vòng chơi hoàn chỉnh (ngắm → bắn → kích nổ → thắng/thua → restart nhanh), Level 1 đủ rõ để tự hiểu cách chơi, và **APK chạy được**. Một game nhỏ mà chơi trọn vẹn vẫn hơn hẳn một project lớn dở dang.

**Giảm scope:** rút từ 9 xuống 4 level — giữ Level 1 (dạy chơi), Level 3 (bệ chạy), Level 4 (thùng nổ) và Level 7 (mục tiêu cấm), vì bốn màn này đã đủ chứng minh có *decision* thật chứ không chỉ tăng độ khó ngắm bắn.

**Bỏ:** đạn tách ba, bệ chạy vòng tròn, và toàn bộ phần đánh bóng hiệu ứng — cắt được nhiều giờ mà không đụng tới thứ làm nên trải nghiệm lõi.
