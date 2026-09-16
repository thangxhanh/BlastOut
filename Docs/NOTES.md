# Ghi chú — kiến thức cần nhớ

> Nơi ghi những thứ **hay quên và hay làm sai**, phần lớn là asset/settings — tức là phần AI không tự làm được, phải nhớ để làm tay.
>
> Quan hệ với hai file kia: [CLAUDE.md](../CLAUDE.md) là **luật bắt buộc** (ngắn gọn, không giải thích), [AI_WORKFLOW.md](AI_WORKFLOW.md) là **quy trình làm việc**, file này là **chi tiết + vì sao**.
>
> 📄 **Khó đọc markdown thô?** Mở [NOTES.html](NOTES.html) — cùng nội dung, double-click là trình duyệt render sẵn heading và bảng.

---

## 1. Texture

Texture là khoản RAM lớn nhất của một game 2D mobile. Sai settings không làm crash ngay — nó làm máy yếu bị hệ điều hành kill sau 15–20 phút chơi, và **không lộ ra trong Editor** vì máy dev thừa RAM.

### 1.1 Mipmap

**Là gì:** chuỗi bản thu nhỏ liên tiếp (1/2, 1/4, 1/8...) sinh sẵn lúc import. Khi vẽ, GPU tự chọn mức gần nhất với kích thước thật trên màn hình.

| | **Bật** | **Tắt** |
|---|---|---|
| **Dùng cho** | Texture 3D ở xa, hoặc đổi khoảng cách liên tục so với camera | UI, sprite 2D, vật thể luôn ở khoảng cách cố định |
| **Được** | Khử răng cưa/moiré, hết nhấp nháy khi vật đi xa. GPU đọc texture nhỏ hơn → cache hit tốt hơn, thường **nhanh hơn** dù tốn RAM | Tiết kiệm **33%** bộ nhớ của texture đó |
| **Mất** | +33% bộ nhớ (tổng mọi mức mip = 1/3 ảnh gốc) | Ảnh bị thu nhỏ nhiều sẽ rỗ và nhấp nháy khi chuyển động |

**Bẫy hay dính:** import type `Sprite (2D and UI)` mặc định **tắt** mipmap, nhưng type `Default` mặc định **bật**. Ảnh UI lỡ để `Default` là âm thầm tốn thêm 33% mà không ai thấy — kiểm bằng dòng info ở mục 1.6.

### 1.2 Nén

| Format | bit/pixel | Dùng khi |
|---|---|---|
| **ASTC 4x4** | 8 | Ảnh cần nét: chữ, icon nhỏ, ảnh hiện to trên màn hình |
| **ASTC 6x6** | 3.56 | **Mặc định cho phần lớn sprite** |
| **ASTC 8x8** | 2 | Nền, ảnh ít chi tiết, ảnh vốn đã mờ |
| **ETC2 RGBA** | 8 | Fallback cho Android cũ không hỗ trợ ASTC |
| **RGBA32** | 32 | Không nén. Chỉ khi gradient bị band nặng — tốn gấp **4 lần** ASTC 4x4 |

- **ASTC là lựa chọn mặc định cho mobile** (Android hiện đại + toàn bộ iOS). Block càng lớn càng nhẹ càng mờ — chọn theo mức chi tiết của ảnh, không chọn một số cho cả game.
- **Crunch compression** chỉ giảm dung lượng **file cài đặt**, giải nén về ETC/DXT lúc load nên **RAM lúc chạy không đổi**. Nó cũng **không dùng được với ASTC**. Import rất lâu. Chỉ bật khi đang phải cắt size APK.

### 1.3 Max Size

Đây là kích thước ảnh được nạp lên, **không phải** kích thước nó hiện trên màn hình. Ảnh 2048 mà chỉ hiện ở 200px là phí khoảng **100 lần** diện tích pixel.

Quy tắc: đặt Max Size sát với kích thước lớn nhất ảnh thật sự hiển thị trên thiết bị mục tiêu. Background full-screen thì theo cạnh dài màn hình, và để **ngoài** atlas (CLAUDE.md §9.4).

**Bẫy kích thước:** ASTC cần cạnh chia hết cho block size, ETC2 cần chia hết cho 4. Không thoả thì Unity có thể lặng lẽ rơi về định dạng không nén — **RAM tăng 4–8 lần mà không có warning nào**. Luôn xác nhận bằng dòng info (mục 1.6), đừng tin vào ô mình vừa chọn.

### 1.4 Read/Write Enabled — gấp đôi RAM

Bật = Unity giữ thêm một bản sao trên RAM CPU bên cạnh bản trên GPU. Chỉ bật khi code thật sự gọi `GetPixels` / `SetPixels` / `ReadPixels` / `EncodeToPNG`. Mặc định tắt — đừng bật "cho chắc".

### 1.5 Vài ô khác

- **Alpha Is Transparency** — bật cho ảnh có viền mềm/bóng đổ. Unity nhân màu lan ra vùng trong suốt, tránh viền đen khi scale.
- **Generate Physics Shape** — tắt nếu sprite không dùng collider 2D. Bật là sinh + lưu polygon thừa cho mọi sprite.
- **Non-Power of 2** — với Sprite để `None`. Để Unity tự scale lên POT là nó đổi cả pixel ảnh.
- **Filter Mode** — `Bilinear` mặc định; `Point` chỉ cho pixel art. `Aniso Level` = 1 cho 2D/UI (chỉ có tác dụng khi có mipmap).

### 1.6 Kiểm tra thật — làm trước mỗi lần build

1. **Dòng info cuối Inspector của texture** — ghi format thật sau khi nén + dung lượng RAM thật. Đây là **nguồn sự thật duy nhất**: các ô phía trên chỉ là *yêu cầu*, Unity có quyền không đáp ứng và tự đổi mà không báo.
2. **Window → Sprite Atlas Tool → tab "Soát lỗi"** — sprite chưa gom, trùng atlas, quá lớn.
3. **Memory Profiler** — snapshot rồi sort theo `Texture2D`. Xem CLAUDE.md §9.6.
4. Sprite đã nằm trong atlas thì **setting nén lấy theo atlas**, không phải theo sprite gốc. Chỉnh ở asset atlas, sửa ở sprite là vô nghĩa.

### 1.7 Bảng chốt theo loại ảnh

| Loại ảnh | Mipmap | Nén | Max Size | Read/Write |
|---|---|---|---|---|
| Icon / button UI | Tắt | ASTC 4x4–6x6 | Vừa đủ hiển thị | Tắt |
| Background full-screen | Tắt | ASTC 8x8, **ngoài atlas** | Theo cạnh dài màn hình | Tắt |
| Sprite gameplay 2D | Tắt | ASTC 6x6 | Vừa đủ hiển thị | Tắt |
| Texture 3D (model ở xa) | **Bật** | ASTC 6x6 | 512–1024 | Tắt |
| Ảnh cần đọc pixel bằng code | Tắt | Không nén | Nhỏ nhất có thể | **Bật** |

### 1.8 Định dạng ảnh nguồn

Mục 1.2 nói về định dạng **đầu ra** Unity nén sang. Mục này nói về **file gốc** bạn kéo vào project — hai thứ khác nhau.

**Điều quan trọng nhất: Unity nén lại mọi ảnh khi import.** File nguồn không nằm trong bản build và không quyết định RAM lúc chạy. Nó chỉ quyết định *chất lượng đầu vào* của lần nén đó, và dung lượng repo.

| Định dạng | Alpha | Mất chất lượng | Dùng khi |
|---|---|---|---|
| **PNG** | Có | Không | **Mặc định cho gần như mọi thứ** |
| **PSD** | Có | Không | Muốn giữ layer để sửa; Unity tự làm phẳng khi import |
| **TGA / TIFF** | Có | Không | Pipeline 3D quen dùng; nặng hơn PNG, không hơn gì về chất lượng |
| **EXR / HDR** | Có | Không | Dữ liệu HDR: skybox, lightmap, HDRI |
| **JPG** | **Không** | **Có** | Đừng dùng — xem bên dưới |

**Vì sao không JPG.** Nó không có alpha, và nó nén mất dữ liệu **trước khi** Unity kịp làm gì. Nhiễu quanh cạnh sắc do JPG sinh ra chính là loại tín hiệu mà ASTC xử lý tệ nhất — nén chồng lên nhiễu thì hỏng thêm một lần nữa, mà **không hề nhẹ đi**: dung lượng đầu ra do block size quyết định, nguồn nặng nhẹ không đổi được con số đó.

**Bẫy hay gặp:** chạy PNG qua TinyPNG/công cụ nén ảnh để mong nhẹ APK. Vô ích — Unity nén lại hết, build không nhỏ đi một byte. Tệ hơn: mấy công cụ đó giảm về bảng màu 8-bit, tức là *làm hỏng nguồn* để đổi lấy một thứ không tồn tại. Chỉ dùng khi muốn repo nhẹ, và biết rõ mình đang đánh đổi.

**Độ sâu màu:** 8-bit/kênh là đủ cho mọi ảnh thường. Chỉ heightmap, normal map và dữ liệu HDR mới cần 16-bit hoặc float — ảnh thường để 16-bit thì Unity cũng hạ xuống, chỉ tốn thêm thời gian import.

**Kích thước nguồn:** giữ ảnh gốc lớn hơn hoặc bằng mức sẽ hiển thị, rồi khống chế bằng Max Size (mục 1.3). Hạ kích thước ngay ở file nguồn là mất luôn đường lùi khi sau này cần bản nét hơn.


---

## 2. Âm thanh

### 2.1 Định dạng nguồn

Y hệt nguyên tắc ở mục 1.8: **Unity mã hoá lại mọi file âm thanh khi import**, nên file nguồn không nằm trong build. Chỉ dùng định dạng **không mất dữ liệu**:

- **WAV** (PCM 16-bit) — mặc định, dùng cho mọi thứ.
- **AIFF** — tương đương WAV, quen thuộc bên macOS.
- **Đừng đưa MP3 / OGG vào làm nguồn.** Chúng đã vứt dữ liệu rồi; Unity nén tiếp là mất lần hai, mà dung lượng đầu ra không nhỏ hơn chút nào.

### 2.2 Load Type — chỗ hay hỏng nhất

| Load Type | RAM | Dùng cho |
|---|---|---|
| **Decompress On Load** | Cao — giải nén thành PCM nằm trong RAM | SFX **ngắn**, phát liên tục |
| **Compressed In Memory** | Thấp — giữ nén, giải mã lúc phát | SFX dài hơn, giọng nói |
| **Streaming** | Gần như không — đọc dần từ đĩa | Nhạc nền, ambience dài |

**Lỗi kinh điển: để nhạc nền ở `Decompress On Load`.** Một bài 3 phút, stereo, 44.1 kHz khi giải nén là:

```
44100 × 2 byte × 2 kênh × 180 giây ≈ 32 MB RAM
```

Cho **một** bài hát. Đổi sang `Streaming` là gần như bằng 0.

Ngược lại, **đừng Streaming cho SFX ngắn**: mỗi clip streaming giữ một handle file và buffer riêng, nhiều clip nhỏ cùng stream còn tốn hơn là nạp thẳng. Streaming dành cho 1–2 clip dài phát đồng thời.

### 2.3 Compression Format

| Format | Tỉ lệ | Chi phí CPU lúc phát | Dùng cho |
|---|---|---|---|
| **PCM** | 1:1 | Không | Clip cực ngắn, cần chính xác tuyệt đối |
| **ADPCM** | ~3.5:1 | Rất rẻ | **SFX ngắn** — click, pop, va chạm, bước chân |
| **Vorbis** | Cao, có thanh Quality | Đáng kể mỗi voice đang phát | **Nhạc nền**, clip dài, giọng nói |

Điểm dễ bỏ qua: **Vorbis tốn CPU cho từng voice đang phát**. Mười SFX Vorbis nổ cùng lúc là một cú giật khung hình. ADPCM giải mã rẻ hơn hẳn — đó là lý do nó hợp với SFX bắn ra dồn dập, dù file to hơn.

Thanh **Quality** chỉ có tác dụng với Vorbis. Mặc định 100 là thừa; nhạc nền để khoảng **70** thường không ai nghe ra khác biệt trên loa điện thoại.

### 2.4 Hai nút giảm một nửa gần như miễn phí

- **Force To Mono** — cắt đôi cả dung lượng lẫn RAM. Loa điện thoại gần như không tái tạo được stereo, và phần lớn SFX vốn không có thông tin không gian. Bật cho **mọi SFX**; nhạc nền thì cân nhắc.
- **Sample Rate Setting → Override 22050 Hz** — cắt đôi lần nữa. Tai người khó phân biệt ở SFX ngắn phát qua loa nhỏ. Giữ 44100 cho nhạc.

Hai nút này cộng lại thường tiết kiệm nhiều hơn mọi lựa chọn compression format.

### 2.5 Bảng chốt theo loại âm

| Loại | Load Type | Format | Mono | Sample Rate |
|---|---|---|---|---|
| SFX ngắn (click, pop, coin) | Decompress On Load | ADPCM | **Có** | 22050 |
| SFX dài / giọng nói | Compressed In Memory | Vorbis (~70) | **Có** | 44100 |
| Nhạc nền | **Streaming** | Vorbis (~70) | Tuỳ | 44100 |
| Ambience loop dài | **Streaming** | Vorbis | Có | 22050 |

Thêm hai ô nữa cho clip dài: tắt **Preload Audio Data** và bật **Load In Background** để nhạc không chặn lúc vào scene.

### 2.6 Số voice — trần cứng của cả game

`Project Settings → Audio`:

```
m_RealVoiceCount: 32       ← số âm phát THẬT cùng lúc
m_VirtualVoiceCount: 512   ← số âm được theo dõi, phần vượt bị ảo hoá
```

Vượt 32 thì Unity tự tắt tiếng những voice **ưu tiên thấp** — dùng `AudioSource.priority` để quyết định cái nào bị hy sinh, đừng để nó chọn ngẫu nhiên. Mỗi con quái trên map giữ một loop là ăn một suất trong 32 chỗ đó.

### 2.7 Đặt mặc định lúc import

Không có ô nào trong Project Settings đặt sẵn Load Type hay Compression. Hai cách giống hệt phần texture (mục 1.8):

- **Preset Manager** — đóng dấu toàn bộ AudioImporter.
- **`OnPreprocessAudio`** trong `AssetPostprocessor` — chạm đúng field cần, rẽ theo đường dẫn (`/Music/` thì Streaming, `/SFX/` thì ADPCM + mono).

Cùng file với `TextureImportDefaults` cũng được.

---

## 3. Batching & draw call

### 3.1 Đang tối ưu cái gì

Mỗi **draw call** là một lần CPU bảo GPU "vẽ cái này với trạng thái này". Trên mobile, CPU thường là chỗ nghẽn trước GPU, nên số draw call quan trọng hơn số tam giác.

Trong Stats window, con số đáng nhìn là **SetPass calls** chứ không phải Batches: SetPass là lần đổi shader/material — phần đắt thật. Hai batch cùng material rẻ hơn nhiều so với hai batch khác material.

Bốn cơ chế gộp dưới đây **không thay thế nhau** — mỗi cái phục vụ một loại nội dung, và hay bị nhầm lẫn.

### 3.2 Static batching

Gộp mesh của các object **đứng yên** thành một vertex buffer chung **lúc build**.

- Bật ở `Player Settings → Rendering → Static Batching`. Từng object phải tick cờ **Batching Static**.
- **Không được di chuyển/xoay/scale lúc chạy** — transform bị nướng cứng vào mesh gộp.
- Muốn giảm draw call thì phải **chung material**.

**Cái giá: nhân bản dữ liệu đỉnh.** 100 cây cùng một mesh 1.000 đỉnh:

| | Bộ nhớ |
|---|---|
| Không static batching | 1 mesh + 100 transform ≈ **40 KB** |
| Có static batching | mesh gộp 100.000 đỉnh ≈ **4 MB** |

Gấp ~100 lần, nằm trong cả build lẫn RAM. **Static batching đổi RAM lấy draw call** — lời với vài chục object *khác nhau*, lỗ nặng với hàng trăm *bản sao*.

### 3.3 GPU Instancing

Đúng thứ cần cho **nhiều bản sao giống hệt**: một mesh, một material, vẽ N lần trong một draw call, dữ liệu per-instance đẩy riêng.

- Bật bằng ô **Enable GPU Instancing** trên **material**.
- **Không nhân bản đỉnh** — khác hẳn static batching, đây là lý do chính để chọn nó.
- **Chạy được với object di chuyển**, không cần cờ Static.
- Không dùng được với `SkinnedMeshRenderer`.

**Lưu ý về MaterialPropertyBlock:** MPB chỉ đi cùng instancing khi shader có khai báo thuộc tính instanced (`UNITY_INSTANCING_BUFFER_START/END`). Shader thường mà gán MPB khác nhau thì mỗi object một draw call.

Đây là chỗ §9.4 của CLAUDE.md va vào batching: object cần màu riêng thì chấp nhận draw call riêng, object chỉ đứng làm nền thì để chung material. Không có cả hai.

### 3.4 Dynamic batching — để tắt

Gộp mesh nhỏ **mỗi frame bằng CPU**. Chỉ nhận mesh rất nhỏ, và chi phí CPU thường lớn hơn phần tiết kiệm được trên phần cứng hiện đại. Chính Unity khuyến nghị tắt. Project này đang `m_DynamicBatching: 0` ở mọi platform — giữ nguyên.

### 3.5 Canvas batching — phần quan trọng nhất với game 2D/UI

UI **không** dùng ba cơ chế trên. Unity gộp các phần tử trong **cùng một Canvas** theo material/texture, **theo thứ tự vẽ**.

**Luật cốt lõi: đổi bất cứ thứ gì bên trong một Canvas thì Canvas đó dựng lại toàn bộ mesh.** Đây là lỗi hiệu năng UI phổ biến nhất — một ô đếm điểm nhảy số mỗi frame kéo theo cả HUD dựng lại mỗi frame.

**Cách chữa: tách Canvas.** UI tĩnh một Canvas, phần thay đổi liên tục (điểm, thời gian, thanh máu) một Canvas riêng. Canvas con lồng trong Canvas cha có tác dụng cô lập — dựng lại chỉ nằm trong phạm vi Canvas con.

**Thứ tự vẽ cắt batch.** Text(atlas A) → Image(atlas B) → Text(atlas A) là **3 batch**, dù chỉ có 2 texture. Xếp các phần tử cùng atlas liền nhau về mặt thứ tự vẽ thì gộp được. Đây là lý do thật của việc gom atlas theo nhóm cùng xuất hiện (§9.4).

**Ẩn UI: tắt component `Canvas`, đừng `SetActive(false)` cả GameObject.** Tắt GameObject là huỷ mesh, bật lại phải dựng từ đầu. Tắt riêng component `Canvas` thì ngừng vẽ mà mesh còn nguyên, bật lại gần như không tốn gì.

> Trong project này `BasePopup` đang dùng `gameObject.SetActive(false)` (BasePopup.cs:67). Với popup mở thỉnh thoảng thì không sao. Nhưng popup nào bật/tắt liên tục thì nên đổi sang tắt `Canvas`.

**`raycastTarget`** — tắt trên mọi Image/Text không cần bấm. Không liên quan batching nhưng mỗi lần chạm là một vòng duyệt qua chúng.

### 3.6 Sprite batching (2D)

`SpriteRenderer` gộp theo **texture + material + sorting layer/order**. Sprite cùng một atlas gộp được; khác atlas thì cắt. Và giống UI, **xen kẽ sorting order làm vỡ batch**: A(atlas1) → B(atlas2) → C(atlas1) là 3 batch.

### 3.7 Chọn cái nào

| Nội dung | Dùng |
|---|---|
| Địa hình, nhà cửa, vật trang trí đứng yên | Static batching |
| Nhiều bản sao giống hệt (cây, đá, đạn, quái) | **GPU Instancing** |
| Mesh nhỏ di chuyển | Không có gì — giảm số object thì hơn |
| UI | Tách Canvas + gom atlas |
| Sprite 2D | Sprite Atlas + không xen kẽ sorting order |

### 3.8 Đo

- **Stats window** — `Batches`, `SetPass calls`, `Saved by batching`.
- **Frame Debugger** — nói rõ từng draw call gộp được không, và **vì sao không**.

Như §9.6 của CLAUDE.md: đo trước và sau. Tick Static hay bật Instancing xong mà không đo thì không biết mình vừa giúp hay vừa làm hại.

---

## 4. Đo hiệu năng

### 4.1 Ba công cụ, ba câu hỏi khác nhau

| Công cụ | Trả lời câu hỏi | Trạng thái |
|---|---|---|
| **Profiler** | *Frame này chậm ở đâu?* | Có sẵn — `Window → Analysis → Profiler` |
| **Frame Debugger** | *Vì sao không gộp được draw call?* | Có sẵn — `Window → Analysis → Frame Debugger` |
| **Memory Profiler** | *Cái gì đang chiếm RAM, và có rò không?* | **Package riêng** — `com.unity.memoryprofiler` |

Hay bị nhầm nhất: module Memory **trong** Profiler chỉ cho **tổng số**; muốn biết *object nào* thì phải cài Memory Profiler. §9.6 của CLAUDE.md nói tới cái thứ hai.

### 4.2 Luật chung: đo trên máy thật

Số trong Editor gần như vô nghĩa — Editor có overhead riêng, chạy Mono chứ không phải IL2CPP, GPU khác, và không có chuyện nóng máy tụt xung.

1. `Build Settings` → tick **Development Build** + **Autoconnect Profiler**.
2. Mở Profiler, chọn thiết bị ở dropdown trên cùng (mặc định là `Editor`).
3. Android không thấy máy thì mở cổng bằng tay: `adb forward tcp:34999 localabstract:Unity-<bundle-id>`.

Chọn máy **tầm trung**, không phải máy mạnh nhất bạn có.

**Deep Profile** ghi lại mọi lời gọi hàm — chậm tới mức làm sai lệch chính con số đang đo. Dùng để tìm *hàm nào*, đừng bao giờ đọc số tuyệt đối từ nó.

### 4.3 Profiler — module CPU

Click một frame trên biểu đồ → khung dưới có hai chế độ: **Timeline** (nhìn theo luồng, thấy cái gì chặn cái gì) và **Hierarchy** (bảng sắp xếp được — dùng cái này để truy).

Hai cột đáng nhìn:

- **Self ms** — thời gian của chính hàm đó, không tính hàm con. Chi phí thật nằm ở đây, không phải Total.
- **GC Alloc** — **con số quan trọng nhất trên mobile**.

**Thanh VSync to KHÔNG phải vấn đề.** Nó nghĩa là bạn làm xong sớm và đang chờ — dấu hiệu tốt, nhưng rất hay bị hiểu ngược.

**Mục tiêu GC Alloc trong gameplay ổn định là 0 B/frame.** Mỗi byte cấp phát rồi cũng dẫn tới một lần thu gom, và thu gom là một cú khựng hình. Đây chính là lý do §9.4 cấm LINQ / nối chuỗi / `new` / `GetComponent` trong `Tick()` — Profiler là chỗ duy nhất chứng minh luật đó có được tuân thủ hay không.

**Nhận dạng thủ phạm qua tên hàm:**

| Thấy trong Hierarchy | Nguyên nhân |
|---|---|
| `GC.Collect` + frame giật đều đặn | Cấp phát trong vòng lặp |
| `Canvas.SendWillRenderCanvases` | **UI dựng lại** — xem §3.5, tách Canvas |
| `Camera.Render` cao | Draw call / overdraw → sang Frame Debugger |
| `Physics.Processing` | Quá nhiều collider |
| `Animator.Update` | Quá nhiều Animator cùng chạy |

### 4.4 Profiler — module Rendering

`SetPass calls`, `Draw calls`, `Batches`, `Triangles` — đúng bộ số ở §3.8. Nhưng ở đây chỉ biết **bao nhiêu**, không biết **vì sao**.

### 4.5 Frame Debugger — trả lời "vì sao không gộp"

Bấm **Enable**, nó đóng băng một frame và cho bạn bước qua **từng draw call theo đúng thứ tự vẽ**. Chọn một bước thì Game view hiện đúng trạng thái vẽ tới bước đó — thấy được thứ tự dựng hình.

**Tính năng đáng giá nhất nằm ở khung phải: dòng giải thích vì sao draw call này không gộp được với cái trước.** Ví dụ hay gặp:

- *Objects have different materials* — khác material.
- *Objects have different Renderer.sortingOrder* — xen kẽ thứ tự vẽ (§3.6).
- *Non-instanced properties set for instanced shader* — gán MPB lên shader instancing (§3.3).

Đây là công cụ duy nhất nói thẳng lý do. Đoán mò chỗ này rất tốn thời gian.

Frame Debugger cũng nối được vào máy thật qua cùng dropdown với Profiler.

### 4.6 Memory Profiler — tìm rò rỉ

Cài `com.unity.memoryprofiler` qua Package Manager. Chụp **snapshot** từ Editor hoặc từ máy đang nối.

Xem theo tab **Unity Objects**, sắp theo dung lượng hoặc số lượng.

**Quy trình tìm rò theo §9.6:**

1. Chụp snapshot A ở màn hình đầu.
2. Chơi qua ~10 level.
3. Chụp snapshot B.
4. Bật chế độ **Compare**, nhìn cột chênh lệch của `Texture2D`, `Material`, `Mesh`.

Số lượng **tăng đều theo level mà không giảm** = rò. Ba nguồn hay gặp nhất:

- `new Material()` lúc chạy mà không `Destroy()` — §9.4 nói rõ: mất reference **không** đủ để GC thu hồi `UnityEngine.Object`.
- `Texture2D` / `RenderTexture` sinh bằng code, không giải phóng.
- Object trong pool spawn ra mà không despawn.

**Phân biệt Managed và Native:** object C# thuần do GC lo. Còn `Texture2D`, `Material`, `Mesh`, `RenderTexture` là **native** — phải gọi `Destroy()`. Đây là ranh giới gây rò nhiều nhất.

### 4.7 Quy trình

1. Build Development + Autoconnect lên máy tầm trung.
2. Chơi bình thường, nhìn biểu đồ CPU tìm chỗ gai.
3. Click đúng frame gai → Hierarchy → sắp theo **Self ms**, rồi sắp theo **GC Alloc**.
4. Xác nhận bằng công cụ chuyên: **Frame Debugger** cho phần vẽ, **Memory Profiler** cho RAM.

Bước 4 mới là chỗ ra kết luận. Profiler chỉ nói *có vấn đề ở đâu*.

---

## 5. Overdraw

### 5.1 Vì sao nó đắt trên mobile

GPU mobile là **tile-based**. Mỗi pixel bị vẽ chồng thêm một lớp là một lần đọc–trộn–ghi nữa. Chi phí tính theo **diện tích màn hình × số lớp**, không theo số tam giác — nên một tấm trong suốt phủ cả màn hình đắt hơn nhiều so với một mô hình vài nghìn đỉnh.

Hình **đục** được loại sớm bằng early-Z: cái bị che thì không vẽ. Hình **trong suốt thì không** — phải vẽ từ sau ra trước và trộn từng lớp. Mà UI thì gần như trong suốt hoàn toàn, nên với game 2D/UI đây thường là chi phí GPU lớn nhất.

### 5.2 Nhìn thấy nó

`Scene view → dropdown Draw Mode → **Overdraw**`. Càng sáng càng nhiều lớp chồng. Mở popup lên rồi nhìn — chỗ trắng xoá là chỗ phải sửa.

### 5.3 Framework popup của project này

Mở một popup là chồng sẵn 5–6 lớp:

```
PopupDim (full màn, alpha 0.6)
  └ Card (gần full)
      └ TitleBar → TitleText → Message → Button → chữ trong Button
```

Và `ShowAction.DoNothing` mở popup chồng lên popup — **hai lớp Dim full màn cùng lúc**, trong khi popup dưới vẫn đang được vẽ đầy đủ dù không ai nhìn thấy.

### 5.4 Bốn cách chữa, theo thứ tự đáng làm

1. **Tắt `Canvas` của popup bị che.** Popup nằm dưới vẫn vẽ đủ mọi lớp. Tắt component `Canvas` là ngừng vẽ mà mesh còn nguyên (§3.5). `ShowAction.PauseCurrent` nên làm việc này.
2. **Một lớp Dim dùng chung**, do controller quản, thay vì mỗi popup một cái. Hai popup chồng nhau thì nền tối gấp đôi — vừa xấu vừa tốn.
3. **Background full màn để đục.** Ảnh nền không cần alpha thì tắt kênh alpha đi; lớp đục chặn được mọi thứ phía sau.
4. **Sprite `Mesh Type: Tight`.** Sprite có nhiều vùng trong suốt (hiệu ứng, tia sáng) mà để `Full Rect` là vẽ cả phần rỗng. `Tight` cắt lưới bám theo hình.

Particle là nguồn overdraw lớn thứ hai sau UI — nhiều hạt lớn trong suốt chồng nhau tốn hơn mọi thứ khác trong scene.

---

## 6. Dung lượng build

### 6.1 Vì sao quan tâm

Với game casual, dung lượng ảnh hưởng thẳng tới **tỉ lệ cài đặt** — người ta bỏ ngang khi thấy con số quá to, nhất là khi đang dùng 4G. Đây là chỉ số marketing, không chỉ là chỉ số kỹ thuật.

### 6.2 Nhìn cái gì đang nặng

Sau mỗi lần build, `Editor.log` có mục **Build Report** liệt kê asset theo dung lượng chưa nén, kèm bảng chia theo loại (Textures / Meshes / Sounds / Animations / Scripts / Included DLLs).

Đây là **nguồn sự thật duy nhất** cho câu "cái gì làm build to". Gần như không ai biết nó tồn tại, và mọi phỏng đoán không có nó đều là đoán mò.

### 6.3 Thư viện thường nặng hơn asset

Với template này, phần lớn dung lượng không phải ảnh mà là **SDK**. Riêng mediation đang có **12 adapter**:

```
BidMachine · ByteDance · Chartboost · Facebook · Fyber
Google · GoogleAdManager · InMobi · IronSource · Mintegral · UnityAds · Vungle
```

Mỗi cái là code native thật. Gỡ adapter không dùng trong **AppLovin Integration Manager** là cách giảm size nhanh nhất, và không ảnh hưởng gì tới doanh thu nếu mạng đó vốn không có fill.

### 6.4 Các setting

Project này đã đúng sẵn: `scriptingBackend: IL2CPP`, `managedStrippingLevel: Medium`, `stripEngineCode: 1`.

- **Android App Bundle (AAB)** thay vì APK — Play chỉ giao đúng ABI và mật độ màn hình của máy đó, người dùng tải ít hơn hẳn.
- **Managed Stripping Level** — `Medium` là mức hợp lý. Lên `High` cắt được thêm nhưng **hay làm hỏng code chạy bằng reflection**: deserialize JSON, callback từ SDK, `[Serializable]` chỉ được tham chiếu gián tiếp.
- **`link.xml`** — khi stripping cắt nhầm, đây là chỗ khai báo giữ lại type/assembly. Triệu chứng: chạy trong Editor thì đúng, build ra máy thật thì null hoặc mất field.
- **Texture** (§1.2) chỉ ship một định dạng nén, không phải tất cả — nên chọn ASTC không làm build phình.

### 6.5 Thư mục `Resources`

**Mọi thứ trong `Resources/` đều vào build**, dùng hay không dùng, và được index lúc khởi động. Đây là chỗ rác tích tụ âm thầm.

Project này đang sạch: gần hết 48 file nằm dưới `GoogleMobileAds/Editor/Resources/` — thư mục `Editor` nên không vào build.

---

## 7. Mấy thứ vặt hay bị bỏ quên

### 7.1 GC — làm sao xuống 0 B/frame

§4.3 đặt mục tiêu, đây là cách đạt:

- **Boxing** — truyền struct vào tham số kiểu `object`/interface, dùng enum làm key `Dictionary` mà không có comparer, `string.Format` với số.
- **Nối chuỗi** — dùng `StringBuilder`, hoặc cache sẵn chuỗi không đổi. Chuỗi nội suy `$"..."` trong `Tick()` là cấp phát mỗi frame.
- **`foreach`** qua `List<T>` thì không cấp phát (enumerator là struct); qua `IList<T>` / `IEnumerable<T>` thì **có** — vì bị boxing.
- **Physics** — dùng `RaycastNonAlloc`, `OverlapSphereNonAlloc` với mảng cấp sẵn.
- **List / Dictionary** — cấp sẵn dung lượng (`new List<T>(64)`) để không phải cấp lại khi lớn dần.
- **Closure** — lambda bắt biến bên ngoài sinh object mỗi lần tạo. Tránh trong vòng lặp nóng.

**Incremental GC** (`Player Settings`) chia việc thu gom ra nhiều frame. Nó **không giảm** lượng cấp phát, chỉ làm cú khựng bớt gắt — vẫn phải sửa gốc.

### 7.2 Shader variant và warmup

Shader **biên dịch lần đầu được dùng**, nên hiệu ứng nào xuất hiện lần đầu cũng kèm một cú khựng. Càng nhiều `#pragma multi_compile` thì số variant càng nhân lên.

Chữa bằng **`ShaderVariantCollection`**: chơi thử để nó ghi lại các variant thật sự dùng, rồi gọi `WarmUp()` lúc load.

Đi kèm với cảnh báo ở §9.4 của CLAUDE.md: shader **không được asset nào tham chiếu sẽ bị strip khỏi build** — Editor chạy đẹp, máy thật ra màu hồng.

### 7.3 TextMeshPro — font atlas

TMP có hai kiểu atlas chữ:

- **Dynamic** — glyph nào chưa có thì nướng vào atlas **lúc chạy**. Tiện, nhưng gây khựng lúc chữ mới xuất hiện lần đầu, và atlas phình dần.
- **Static** — nướng sẵn trọn bộ ký tự lúc build. Đoán trước được, không phình.

**Tiếng Việt là ca đáng chú ý**: bộ dấu làm số glyph lớn hơn hẳn tiếng Anh. Dùng dynamic thì atlas cứ lớn dần theo chữ người chơi gặp.

> Dấu hiệu nhận biết: file `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset` liên tục bị đánh dấu thay đổi trong git — đó chính là TMP đang nướng glyph mới vào atlas dự phòng. Project này đang có đúng triệu chứng đó.

Atlas 1024×1024 thường là đủ; để 4096 là phí RAM. Và mỗi font fallback trong chuỗi đều bị duyệt khi thiếu glyph, nên chuỗi dài thì tra chậm.

### 7.4 Boxing và unboxing

**Boxing** là gói một *value type* (`int`, `bool`, `enum`, struct, `Vector3`...) vào một object trên **heap** để nó đi được qua chỗ nhận `object` hoặc interface. **Unboxing** là lôi giá trị ra.

Điểm mấu chốt: **boxing cấp phát trên heap**. Mỗi lần box là một mẩu rác — nên nó nằm ngay đầu danh sách khi truy mục tiêu 0 B/frame ở §4.3.

```csharp
int i = 5;
object o = i;        // BOXING   — cấp phát
int back = (int)o;   // UNBOXING — không cấp phát, nhưng có kiểm tra kiểu
```

**Năm chỗ nó xảy ra mà không ai gõ chữ "box":**

1. **Truyền value type vào tham số `object`** — kể cả `params object[]`, vì cấp phát luôn cả mảng. Ví dụ ngay trong framework: `Dacoder.Log(params object[] os)`. May là `Dacoder` có `[Conditional]` nên bản release xoá sạch cả lời gọi lẫn đối số — đúng lý do §9.4 bắt dùng `Dacoder` thay `Debug.Log`.

2. **Chuỗi nội suy.** Unity dùng **C# 9**, chưa có `DefaultInterpolatedStringHandler` của C# 10, nên `$"Level {level}"` dịch thành `string.Format(string, object)` và **box** `level`.

3. **`foreach` qua interface** — ca khó thấy nhất:

   ```csharp
   List<int> list;
   foreach (var x in list) { }        // KHÔNG box, enumerator là struct

   IEnumerable<int> seq = list;
   foreach (var x in seq) { }         // BOX enumerator mỗi vòng lặp
   ```

   Cùng dữ liệu, chỉ khác kiểu khai báo biến.

4. **Struct gọi qua interface** — `IComparable c = 5;` box.

5. **Enum làm key `Dictionary`** — từng là lỗi kinh điển vì comparer mặc định box mỗi lần tra. Bản Unity mới đã vá phần lớn, nên đừng sửa mù: nhìn cột GC Alloc rồi mới quyết.

**Boxing KHÔNG phải cấm tuyệt đối — chỉ cấm ở đường nóng.** Ví dụ ngay trong framework:

```csharp
public class PersistentData : AbstractPersistentData<string, object>
public void Set(K key, V value) => data[key] = value;   // V = object
```

`GameData.Set("coin", 100)` có box thật. Nhưng save data phải chứa nhiều kiểu trong một dictionary nên `object` là cách hợp lý, và nó chỉ chạy lúc qua màn hay mua hàng — **không nằm trong vòng lặp frame**. Một lần box khi bấm nút là vô hình; một lần box mỗi frame × 60 là rác liên tục.

**Cách tránh khi cần:**

- **Generic thay cho `object`** — `void Log<T>(T value)` không box, runtime sinh bản riêng cho từng value type.
- **Chọn đúng overload** — `StringBuilder.Append(int)` không box, `Append(object)` thì có.
- **Khai biến bằng kiểu cụ thể**, đừng để `IEnumerable<T>` nếu không cần.
- **Cache giá trị đã box** nếu buộc phải box lặp lại cùng một giá trị.

**Cách nhìn thấy:** chỉ có cột **`GC Alloc`** trong Profiler Hierarchy (§4.3). Boxing không có cảnh báo biên dịch, không gạch chân trong IDE — nó chỉ hiện ra dưới dạng vài chục byte mỗi frame ở một hàm mà nhìn code thì tưởng không cấp phát gì.

---

> Thêm ghi chú mới bên dưới, mỗi chủ đề một mục.
