# Blast Out

Mobile physics puzzle — Unity 6000.3.24f1, Android portrait.

> Cách chạy, công cụ sửa level, kiến trúc và nguồn asset: [Docs/DEVELOPMENT.md](Docs/DEVELOPMENT.md).

---

## 1. Game idea

Điều khiển một khẩu pháo, bắn đạn nổ để **hất các khối gỗ rơi xuống vùng thu** ở đáy màn. Hết khối là thắng, hết đạn mà còn khối là thua.

Một lượt: **kéo ở bất kỳ đâu để ngắm** (kiểu ná, có đường bay hiện sẵn) → thả để bắn → **chạm lần nữa để kích nổ giữa không trung** → chờ mọi thứ đứng yên rồi mới kết luận, vì một khối còn đang lăn vẫn có thể rơi vào vùng thu.

Cố ý **không có máu, không có khối vỡ vụn** — khối chỉ bị đẩy. Nhờ vậy vật lý *đọc được*: nhìn là đoán được lực sẽ đẩy khối đi đâu, và mọi thất bại đều tự giải thích được.

9 level. Bảy màn đầu mỗi màn thêm **đúng một** thứ mới; hai màn cuối bắt dùng hai thứ đã học cùng lúc.

| # | Thứ mới | Quyết định nó tạo ra |
|---|---|---|
| 1 | kéo–thả | *(không thể thua khi còn đạn — màn dạy chơi)* |
| 2 | kích nổ giữa không trung | hai khối xa nhau, bắn lẻ thì thiếu đạn → chọn **thời điểm nổ** |
| 3 | bệ chạy ngang | ngắm đúng mà sai nhịp vẫn trượt → chọn **lúc nào bắn** |
| 4 | thùng nổ dây chuyền | **1 viên, 2 khối**, bắn thẳng là chắc thua → chọn **bắn cái gì** |
| 5 | pháo đứng trên bệ chạy | điểm bắn cũng di động → chọn **đứng đâu thì bắn** |
| 6 | đạn tách ba + bệ chạy vòng | tách sớm thì toả quá rộng, tách muộn thì không với tới |
| 7 | mục tiêu **cấm** chạm | lần đầu "nổ càng gần càng tốt" là **sai** → né bán kính của chính mình |
| 8 | bệ chạy **+** mục tiêu cấm | không chỉ cần *thời điểm đúng* mà phải là *thời điểm an toàn* |
| 9 | thùng dây chuyền **+** mục tiêu cấm | hai thùng nhìn y hệt nhau, một viên đạn — chọn sai là thua |

---

## 2. Gameplay additions

Ngoài aim-and-shoot, **5** yếu tố đổi quyết định của người chơi:

1. **Kích nổ chủ động giữa không trung** — nổ sớm thì yếu, nổ muộn thì đạn đã chạm đất. Đạn chạm vật thể cũng tự nổ, nên quên chạm không bao giờ làm mất lượt.
2. **Thùng nổ dây chuyền** — bán kính và lực lớn hơn hẳn đạn thường, buộc chọn *bắn cái gì trước*.
3. **Đạn tách ba** — lúc kích nổ thì tách thành 3 mảnh hình quạt thay vì nổ, đổi câu hỏi từ "nổ ở đâu" sang "toả ra lúc nào".
4. **Bệ chuyển động** (ngang/dọc/vòng) — vật đặt trên được ma sát kéo theo; khẩu pháo cũng đứng được trên bệ chạy.
5. **Mục tiêu cấm chạm** — vụ nổ với tới là thua ngay. Đây là đối trọng của bốn thứ trên: không có nó thì mọi quyết định đều quy về *"làm sao văng được nhiều nhất"*, tức **sức mạnh không có mặt trái**.

Thêm loại đạn mới = thêm một nhánh trong `BlastProjectile.Detonate`. Thêm quỹ đạo bệ = thêm một nhánh trong `PlatformMotion`. Cả hai không đụng luật thắng/thua.

---

## 3. Important decisions

**1. Luật chơi tách hẳn khỏi Unity.** `BlastSession` là C# thuần giữ phase và điều kiện thắng/thua — không biết prefab, chỉ báo ra "phase đã đổi". Ranh giới này đắt nhất nếu sửa sau, vì View bám vào prefab.

**2. Level là dữ liệu, không phải scene.** Mỗi level là một ScriptableObject. Thêm level = tạo asset rồi kéo vào mảng, **không viết dòng gameplay nào**. Số liệu vật lý nằm trong `BlastTuning`, chỉnh game feel không cần build lại.

**3. Một điểm vào duy nhất mỗi frame.** Chỉ `BlastGameController` đăng ký tick, mọi thứ khác được nó gọi xuống. Không component nào có `Update()` riêng, nên thứ tự trong một frame là đọc được.

**4. Bệ chạy phải đi theo nhịp vật lý.** Đẩy bệ trong `Tick()` khiến vận tốc lệch pha với lúc va chạm được giải, khối tụt dần khỏi bệ dù ma sát tối đa. Thêm nữa, quỹ đạo `sin` có vị trí bằng 0 tại `t=0` nhưng **vận tốc đã cực đại** — bệ lao đi hết tốc lực từ trạng thái đứng yên. Chuyển sang `FixedTick` và nhân biên độ với smoothstep: độ trôi `0.409 → 0.000`.

---

## 4. Technical & performance

**Scenario nặng nhất: nổ dây chuyền** (Level 4, 6, 9) — một viên đạn kích thùng nổ, thùng quét tiếp nhiều vật thể, đồng thời bốn lớp hạt, mảnh vỡ và tiếng nổ cùng lúc.

### Hai lỗi đáng giá nhất — đều không thấy được khi đọc code

**Bố cục tràn ra ngoài màn.** `orthographicSize` là nửa chiều *cao*, nên màn càng dài khung nhìn càng **hẹp**: 9:16 cho 4.50 unit mỗi bên, 9:19.5 còn 3.69, 9:21 còn 3.43. Level dựng theo 9:16 nên ở 9:19.5 — tỉ lệ của phần lớn máy Android — **cả 9 level đều có vật thể nằm ngoài màn**. Sửa bằng cách khớp camera theo **bề ngang**, vẫn giữ sàn chiều cao để vùng thu không bị đẩy ra ngoài trên màn vuông. Kiểm lại 9 level × 7 tỉ lệ: mép xa nhất 4.70, trong ngưỡng 4.9.

**`Rigidbody2D.position` chưa đồng bộ.** Vụ nổ đo khoảng cách bằng `body.position`, nhưng builder đặt vị trí qua `transform` và rigidbody chỉ chép lại ở bước vật lý kế tiếp — nên ở lượt bắn đầu mọi vật thể đều còn mang toạ độ `(0,0)`. Hệ quả: vụ nổ với tới những thứ ở rất xa. Triệu chứng nhìn thấy chỉ là *"đạn hơi mạnh"*.

### Số đo

| Hạng mục | Trước | Sau |
|---|---|---|
| Chờ sau cú bắn hỏng (đạn ra ngoài màn) | 6.4s | **0.634s** |
| Khối trôi trên bệ chạy | 0.409 và tăng | **0.000** |
| Level 7: cửa sổ nổ hợp lệ | 0.05 unit (bất khả thi) | **0.85 unit** |
| Draw call (Level 9, lúc nổ) | — | **33** (tĩnh 13), render 0.35 ms |
| `Material` / `Texture2D` sau 36 lượt chơi | 91 / 1216 | **94 / 1221**, đứng yên từ vòng hai |

Kiểm rò rỉ bằng cách chơi tự động 9 level × 2 lượt, chụp snapshot, rồi lặp thêm một vòng để phân biệt *rò rỉ* với *nạp trễ một lần*. Số đứng yên ở vòng hai ⇒ không rò rỉ. Object sống trong scene giữ nguyên 94, khớp pool đã cấu hình.

### Rủi ro và cách xử lý

- **Cấp phát lúc chơi** — đạn, hiệu ứng nổ, mảnh vỡ, `AudioSource` đều qua pool dựng sẵn; `BlastResolver` giữ buffer cho `OverlapCircle`. Không `Instantiate`/`Destroy` nào trong gameplay loop.
- **Shader bị strip** — material VFX tạo bằng `Shader.Find` nhưng **chỉ trong editor tool**, kết quả là asset thật mà prefab tham chiếu. Gọi lúc chạy thì shader bị strip và máy thật ra màu hồng.
- **Treo game vì hitstop** — khựng hình đặt `timeScale = 0`, nên phần này tick bằng `unscaledDeltaTime`; `CleanUp()` trả về 1 phòng khi thoát scene giữa chừng.
- **Chưa cần Sprite Atlas** — 33 draw call và 0.35 ms render là quá nhẹ để đáng đánh đổi thêm phức tạp.

### ⚠️ Chưa làm

**Chơi APK trên máy thật.** Không phải vì lo hiệu năng — 33 draw call và 0.35 ms render thì máy tầm trung nào cũng dư sức. Ba thứ khác mới cần thiết bị thật mới thấy:

- **Shader bị strip** — Editor chạy đẹp, máy thật ra màu hồng;
- **Texture nén sai định dạng** — chỉ lộ khi build qua đường nén của Android;
- **HUD bị tai thỏ che** — nhãn `LEVEL`/`BOMB` và nút `RESTART` đều nằm sát mép trên, mà game **chưa xử lý safe area**.

---

## 5. AI / Tool usage

**Claude Code** + **Unity MCP** (điều khiển Editor từ agent).

- **Dựng scene và prefab bằng code.** Scene/prefab là asset nhị phân, diff git không đọc được; dựng bằng code thì *"đã đổi gì"* hiện rõ trong diff và dựng lại được y hệt bằng một lệnh menu.
- **Đo trạng thái runtime trực tiếp** thay vì đoán bằng mắt. Mọi số ở mục 4 đều lấy bằng cách chạy code trong Play mode. Ba lỗi nặng nhất tìm ra theo cách này, cả ba đều không nhìn thấy được: *bệ chạy lệch nhịp* (mắt chỉ thấy "hơi lạ"), *`body.position` chưa đồng bộ* (triệu chứng là "đạn hơi mạnh"), *bố cục tràn màn* (ở tỉ lệ Editor thì bình thường).

Ngược lại, có loại lỗi công cụ đo **không bắt được**, và cả ba lần đều do người chơi thật phát hiện: Level 4 *tự giải*, Level 7 *không thể thắng*, đạn tách *nhìn không ra là đã tách*. Điểm chung: chúng **đúng về kỹ thuật** — số liệu đẹp, không lỗi nào — nhưng sai về trải nghiệm. Có lần tôi còn "verify" cú tách thành công vì đếm đúng 3 mảnh, trong khi cả ba nổ ngay frame sau đó.

Output của AI **bị sửa hoặc bỏ**: bố cục level, bộ số trong `BlastTuning` (đạn ban đầu quá mạnh), chọn sprite (tên file Kenney đánh số nên phải mở từng ảnh xem), và cách báo trạng thái chờ (bản đầu làm mờ cả khẩu pháo — tô đỏ riêng nòng đọc ra *"vừa bắn, còn nóng"* thay vì chỉ *"đang khoá"*).

---

## 6. Nếu có thêm 24 giờ

1. **Chơi thử với người thật rồi cân lại độ khó.** Thứ tự 9 level dựa trên suy luận thiết kế. Level 4 từng *tự giải* và Level 7 từng *không thể thắng* — cả hai đều đúng về kỹ thuật, chỉ lộ ra khi có người chơi. *Kỳ vọng:* biết màn nào làm người chơi bỏ cuộc, sửa bố cục thay vì sửa số.

2. **Safe area.** HUD hiện nằm sát mép trên nên tai thỏ hoặc camera đục lỗ sẽ che mất nhãn `LEVEL`/`BOMB` và nút `RESTART`. Đây không phải chuyện thẩm mỹ: người chơi mất chỗ bấm restart thì kẹt luôn ở màn đang chơi. *Kỳ vọng:* HUD co vào vùng an toàn trên mọi máy.

3. **Khoảnh khắc thắng.** Hiện chỉ là một dòng chữ hiện ra, trong khi đây là phần thưởng cho toàn bộ chuỗi quyết định. *Kỳ vọng:* slow-motion ngắn khi khối cuối rơi vào vùng thu, kèm camera dõi theo.

*Đã cân nhắc rồi loại:* profile hiệu năng và Sprite Atlas. Số đo cho thấy không có vấn đề để giải — đưa vào danh sách chỉ để trông có vẻ kỹ lưỡng thì chiếm mất chỗ của ba việc trên.

## Nếu chỉ có 24 giờ

**Giữ bằng mọi giá:** vòng chơi hoàn chỉnh (ngắm → bắn → kích nổ → thắng/thua → restart nhanh), Level 1 đủ rõ để tự hiểu cách chơi, và **APK chạy được**.

**Giảm scope:** rút còn 4 level — Level 1 (dạy chơi), 3 (bệ chạy), 4 (thùng nổ), 7 (mục tiêu cấm). Bốn màn này đã đủ chứng minh có *decision* thật.

**Bỏ:** đạn tách ba, bệ chạy vòng, và toàn bộ phần đánh bóng hiệu ứng — cắt nhiều giờ mà không đụng trải nghiệm lõi.
