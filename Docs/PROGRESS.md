# Blast Out — Nhật ký tiến độ

> Ghi lại **trạng thái hiện tại** và **các quyết định đã chốt** của bài test JURRAC.
> Đọc file này trước khi làm tiếp — kể cả khi mở một phiên AI mới — để không nghiên cứu lại
> hay bàn lại những thứ đã quyết. Cập nhật file này sau mỗi cụm việc.
>
> Cập nhật lần cuối: **2026-09-16** · Commit mới nhất: xem `git log --oneline`

---

## 0. Tóm tắt 30 giây

- **Game:** Blast Out — bắn đạn có trọng lực, **chạm màn hình để kích nổ giữa không trung**, sóng nổ **đẩy** khối mục tiêu rơi xuống vùng thu ở đáy màn. Không phá vỡ gì cả.
- **Đã xong:** code gameplay (Model / Config / View), tool Editor dựng scene, Level 1 dạng asset dữ liệu. Compile sạch.
- **Chưa làm được:** **chưa chạy thử lần nào trong Unity.** Việc đầu tiên của phiên sau là mở Editor và bấm Play.
- **Repo:** https://github.com/thangxhanh/BlastOut

---

## 1. Bài test

| | |
|---|---|
| Nguồn | `JURRAC - Game Developer Practical Test.docx` |
| Thể loại | Mobile physics puzzle — player điều khiển launcher/cannon, ngắm và bắn projectile |
| Engine / Platform | Unity 6000.1.17f1 · Android · **portrait** |
| Thời gian | Đề ghi 2–3 ngày, **kế hoạch làm trong 2 ngày** |
| Khối lượng | 5–7 level ngắn, win/fail, restart nhanh, build Android chạy được |
| Bắt buộc | **≥ 2 yếu tố gameplay thật sự thay đổi quyết định** ngoài aim-and-shoot. Đổi visual không tính |
| Level 1 | Tự hiểu cách chơi, không cần tutorial dài |
| Level sau | Progression về **cách quan sát / suy nghĩ**, không chỉ tăng số target hay độ khó ngắm |
| Kiến trúc | Thêm/sửa level không viết lại core; thêm target/rule mới không sửa phần lớn hệ thống |
| Performance | Profile kịch bản nặng nhất, ghi vào README: kiểm tra gì, bottleneck, xử lý gì, số trước/sau |

**Nộp:** APK · Git repo **giữ nguyên commit history** · video gameplay 1–2 phút · README ngắn.

**README phải trả lời 7 mục:** Game idea · Gameplay additions · Important decisions (3–5) · Technical & performance · AI/Tool usage · Nếu có thêm 24h (≤3 việc) · Nếu chỉ có 24h (bỏ gì, giữ gì).

**Ngoài scope:** ads, IAP, analytics, backend, account, nhiều menu, art lớn, framework phức tạp để khoe.

**Tiêu chí chấm:** player experience · gameplay design · engineering · technical judgment · ownership & AI leverage · product sense.

---

## 2. Quyết định sản phẩm — đã chốt

> Chỉ mở lại khi có **dữ liệu mới** (vd: chơi thử thấy không vui). Không mở lại vì "nghĩ lại thấy hay hơn".

### 2.1 Vì sao chọn hướng này

Đã nghiên cứu 5 nhóm game aim-and-shoot trên store và chấm theo đúng tiêu chí của đề + khả thi 2 ngày:

| Nhóm | Ví dụ | Điểm | Vì sao |
|---|---|---|---|
| Phá kết cấu | Angry Birds, Crush the Castle | 5.4 | **Loại.** Tháp sập hỗn loạn → vi phạm "physics dễ đọc"; tuning tốn giờ; nặng nhất trên Android |
| Trick-shot nảy tường | Ricochet Kills, Mr Bullet | 8.6 | Decision sâu, deterministic, rẻ |
| Pachinko | Peggle, Peglin | 6.0 | **Loại.** Sau 2–3 lần nảy là ngẫu nhiên → quyết định giả |
| Knock-down / vật chứa | Cannon Shot!, Cannon Ball 3D | 7.2 | Dễ làm nhưng decision nông |
| Timing-release | King Oddball | 8.2 | Timing làm trục quyết định thứ hai |

→ **Lai trick-shot + timing.**

**Mechanic "chạm để nổ" không mới** — Angry Birds (chim Bomb, chim Blues tách 3) và Worms (Cluster Bomb hẹn giờ) đã có. Đề **không chấm originality**, nên dùng mechanic đã được kiểm chứng là lợi thế. Để không bị đọc là clone, thêm **2 điểm khác biệt có chủ đích**:

1. **Nổ = lực đẩy, không sát thương.** Khối không vỡ, chỉ bị hất. Chuyển tư duy từ *"phá cái gì"* sang *"đẩy cái gì đi đâu"*.
2. **Người chơi chọn thứ tự đạn** (Angry Birds phát hàng đợi cố định).

### 2.2 Core loop

```
Kéo bất kỳ đâu (kiểu ná) → đường chấm dự đoán → thả để bắn
  → chạm màn hình lúc đạn đang bay = KÍCH NỔ
  → sóng nổ đẩy khối → khối rơi xuống vùng thu
  → hết khối = THẮNG · hết đạn mà còn khối = THUA · chạm để chơi lại
```

### 2.3 Hai mechanic tạo quyết định

| # | Mechanic | Trục quyết định | Trạng thái code |
|---|---|---|---|
| 1 | Kích nổ giữa không trung | **Timing** — cùng đường ngắm, nổ sớm/muộn ra kết cục khác | ✅ Bomb · ✅ Splitter (chưa level nào dùng) |
| 2 | Thùng nổ lây dây chuyền | **Thứ tự** — bắn thùng thay vì bắn khối | ✅ `ExplosiveBarrel` (chưa level nào dùng) |
| 3 | *(stretch)* Chọn vị trí bệ bắn | Vị trí bắn | ⬜ Chưa làm — cắt đầu tiên nếu thiếu giờ |

### 2.4 Kế hoạch level — mỗi level dạy đúng một điều

| Lv | Dạy gì | Nội dung | Đạn | Trạng thái |
|---|---|---|---|---|
| 1 | Bắn & trọng lực | 1 khối trên bệ hẹp, trống trải. **Không thể thua** | 2 Bomb | ✅ asset |
| 2 | Vòng cung | Tường thấp chắn giữa, phải bắn cầu vồng | 2 | ⬜ |
| 3 | **Timing nổ** | Khối nấp dưới mái che → phải nổ đúng lúc | 1 | ⬜ |
| 4 | **Chain reaction** | 3 khối quanh 1 thùng → bắn thùng | 1 | ⬜ |
| 5 | **Thứ tự** | Phải dọn đường trước rồi mới bắn | 2 | ⬜ |
| 6 | Tổng hợp | 3 tầng, Splitter + Bomb, >1 lời giải | 2 | ⬜ |

### 2.5 Mockup

HTML mockup 8 màn hình (ngắm / nổ / đẩy / thắng / thua / Lv1 / Lv4 / Lv6):
https://claude.ai/artifact/E6Vnwhosq7Rc4QZFuntSC5 *(link riêng tư)*

Bảng màu vật liệu: đỏ `#E8503A` khối · cam `#F0A63C` thùng nổ · xám thép `#5B6479` bệ · xanh lá `#2ED09A` vùng thu · nền `#0B1224`.

---

## 3. Kiến trúc

### 3.1 Cây thư mục

```
Assets/Dev/Scripts/BlastOut/
├── BlastSceneLauncher.cs        Khởi tạo scene theo thứ tự khai báo
├── Core/                        MODEL — C# thuần, không using UnityEngine
│   ├── BlastSession.cs          Luật thắng/thua + số đạn, báo ra qua event
│   ├── BlastPhase.cs            Aiming → Flying → Resolving → Won | Lost
│   └── AmmoType.cs              Bomb, Splitter
├── Config/                      CONFIG — ScriptableObject
│   ├── BlastTuning.cs           Mọi số liệu vật lý
│   └── BlastLevelConfig.cs      Bố cục 1 level: bệ, khối, thùng, đạn, vị trí pháo
├── Gameplay/                    VIEW — MonoBehaviour
│   ├── BlastGameController.cs   Điều phối; BaseMono DUY NHẤT đăng ký Ticker
│   ├── AimController.cs         Input kéo–thả kiểu ná
│   ├── TrajectoryPreview.cs     Đường chấm (công thức đạn đạo)
│   ├── BlastProjectile.cs       Bay, nổ khi chạm màn hình hoặc va chạm
│   ├── ProjectilePool.cs        Tái dùng đạn
│   ├── BlastResolver.cs         Quét hình tròn → gọi IBlastable
│   ├── IBlastable.cs            ĐIỂM CẮM cho mọi vật thể chịu sóng nổ
│   ├── TargetBlock.cs           Khối mục tiêu (bị đẩy)
│   ├── ExplosiveBarrel.cs       Thùng nổ lây
│   ├── CollectZone.cs           Trigger đáy màn
│   └── LevelBuilder.cs          Đọc BlastLevelConfig → spawn prefab
└── UI/
    └── BlastHudView.cs          Level, số đạn, gợi ý, banner thắng/thua

Assets/Dev/Scripts/Editor/BlastOut/     namespace Dev.Scripts.BlastOut.Authoring
├── BlastOutSceneBuilder.cs      Menu Tools/Blast Out/Build Level 1 Scene
├── BlastOutAssetFactory.cs      Sprite, tuning, level_01
├── BlastOutPrefabFactory.cs     Prefab bệ/khối/thùng/đạn/chấm
└── SerializedFieldWriter.cs     Ghi field [SerializeField] private
```

Asset sinh ra khi chạy tool: `Assets/Dev/{Sprites,Data,Prefabs}/BlastOut/` và `Assets/Dev/Scenes/BlastOut.unity`.

### 3.2 Luồng một phát bắn

```
AimController.HandleInput ──Fired(velocity)──▶ BlastGameController.OnFired
    session.TryFire()           Aiming → Flying, ammo--
    ProjectilePool.Get → BlastProjectile.Launch

[chạm màn hình]  BlastGameController.TickFlight → projectile.Detonate()
    BlastResolver.Blast(origin)
        Physics2D.OverlapCircle → IBlastable.ApplyBlast
            TargetBlock     → AddForce (giảm tuyến tính theo khoảng cách)
            ExplosiveBarrel → chờ 0.12s → Blast tiếp (dây chuyền)
    despawn → flying.Count == 0 → session.OnProjectileSpent()   Flying → Resolving

CollectZone.OnTriggerEnter2D ──BlockCollected──▶ session.OnTargetCollected()   (hết khối → Won)

BlastGameController.TickSettle   mọi vật đứng yên ≥ 0.4s, hoặc quá 4s
    session.OnPhysicsSettled()   → Won | Lost | Aiming
```

### 3.3 Mở rộng thế nào

| Muốn thêm | Làm gì | Không phải sửa |
|---|---|---|
| Level mới | Tạo `BlastLevelConfig` asset | Code, scene |
| Loại vật thể phản ứng với nổ | Class mới implement `IBlastable` + prefab + mảng trong config | `BlastResolver`, `BlastSession` |
| Loại đạn mới | Thêm enum `AmmoType` + nhánh trong `BlastProjectile.Detonate` | Luật chơi, input |
| Chỉnh game feel | Sửa `blast_tuning.asset` | Code, không cần build lại |

---

## 4. Quyết định kỹ thuật — nguyên liệu cho README

| Quyết định | Lý do | Đánh đổi |
|---|---|---|
| **Nổ = lực đẩy, không có HP** | Physics đọc được; bỏ được cả hệ thống HP, destruction, debris | Mất cảm giác "phá hủy đã tay" — bù bằng hitstop + screen shake |
| **Model C# thuần** (`BlastSession`) | Luật thắng/thua test được không cần scene; đúng §9.2 CLAUDE.md | Thêm một lớp event giữa model và view |
| **Chờ vật lý lắng mới kết luận** | Khối còn đang lăn vẫn có thể rơi vào vùng thu | Trễ ~0.4s sau vụ nổ; có timeout 4s chống treo |
| **Đường ngắm bằng công thức**, không dùng `PhysicsScene2D` | Trước va chạm đầu, quỹ đạo là parabol thuần → công thức cho kết quả đúng tuyệt đối, không phải nuôi physics scene song song | Không dự đoán được đường nảy — cố ý, để không giải hộ người chơi |
| **Một điểm tick duy nhất** | Thứ tự trong frame đọc được; bật/tắt gameplay = bật/tắt 1 object | Controller biết nhiều thứ hơn |
| **`IBlastable` định nghĩa ngay** | §9.3: điểm cắm đã biết chắc sẽ có → phải tính trước | Một interface khi mới có 2 implementation |
| **`BlastResolver` không lọc layer** | Bỏ nhóm bug "không có gì xảy ra vì quên set layer"; vài chục collider nên đủ nhanh | Phải xem lại nếu level có hàng trăm collider |
| **Kéo tự do, không phải chạm vào pháo** | Ngón cái che mất pháo ở portrait | Người chơi mới có thể chưa hiểu phải kéo ngược |
| **Chạm bất kỳ đâu để nổ** | Bắt trúng viên đạn nhỏ đang bay nhanh là ức chế | Không có |
| **Level 1 không thể thua** | Dạy thao tác bằng level design, không bằng popup | Level 1 không có thử thách |

---

## 5. Trạng thái

### 5.1 Đã xong

- [x] Nghiên cứu thị trường, chốt hướng game (§2)
- [x] Mockup HTML 8 màn hình
- [x] Clone `UnityCodeBase` → `D:\ThangLM\BlastOut` (robocopy, bỏ `Library/Temp/obj/*.csproj`)
- [x] Code gameplay đủ vòng: ngắm → bắn → nổ → đẩy → thu → thắng/thua → chơi lại
- [x] Bomb, Splitter, thùng nổ dây chuyền
- [x] Level 1 dạng asset dữ liệu
- [x] Tool Editor dựng scene + prefab + đăng ký Build Settings
- [x] **Compile sạch** cả runtime lẫn editor bằng Roslyn của Unity 6000.1.17f1
- [x] Push lên GitHub

### 5.2 Mức độ chắc chắn

| Đã verify | **CHƯA verify** |
|---|---|
| Compile không lỗi | Chạy trong Unity Editor |
| Tên field trong tool khớp với script | Tool dựng scene chạy không lỗi |
| Không file nào > 50MB trong history | Số liệu vật lý có vui không |
| Không có SDK key thật trong repo | Cảm ứng trên máy Android thật |
| | Build APK |

### 5.3 Cách chạy

1. Mở `D:\ThangLM\BlastOut` bằng Unity **6000.1.17f1**, đợi import xong
2. Menu **Tools → Blast Out → Build Level 1 Scene**
3. Game view → tỉ lệ **dọc 9:16** → Play
4. Kéo bất kỳ đâu để ngắm (ngược hướng muốn bắn) · thả để bắn · **chạm lần nữa lúc đạn bay để nổ**

---

## 6. Việc tiếp theo — theo thứ tự ưu tiên

**Ngày 1 (còn lại)**
- [ ] **Mở Unity, chạy tool, bấm Play** — chưa có gì được kiểm chứng lúc chạy, làm trước mọi thứ
- [ ] Commit `.meta` và asset do tool sinh ra
- [ ] Chỉnh `blast_tuning.asset` trong Editor (xem §7)
- [ ] **Build APK thử ngay** — đừng để cuối ngày 2. Rủi ro lớn: Firebase/EDM4U resolve gradle lỗi
- [ ] **Luồng chuyển level:** controller đang giữ đúng 1 `level`, thắng xong chỉ chơi lại màn đó. Cần danh sách level + sang màn khi thắng

**Ngày 2**
- [ ] Level 2–6 (tạo asset, không đụng code)
- [ ] Game feel: hitstop 60–80ms khi nổ, screen shake, particle nổ, SFX bắn/nổ/thu/thắng/thua
- [ ] Profile kịch bản nặng nhất (§10.2), ghi số liệu
- [ ] README đủ 7 mục
- [ ] Quay video 1–2 phút
- [ ] *(stretch)* Chọn vị trí bệ bắn

**Cắt theo thứ tự nếu thiếu giờ:** vị trí bệ bắn → level 6 → Splitter.
**Giữ bằng mọi giá:** đường chấm, kích nổ giữa không trung, dây chuyền, chơi lại 1 chạm, hitstop + shake.

---

## 7. Số liệu cần tinh chỉnh

Tất cả trong `Assets/Dev/Data/BlastOut/blast_tuning.asset` — sửa trong Inspector, không cần build lại.

| Field | Hiện tại | Ghi chú |
|---|---|---|
| `blastForce` | 12 | Cố ý để mạnh tay, dễ giảm hơn tăng |
| `blastRadius` | 3 | Level 1 cần rộng để không thể thua |
| `maxLaunchSpeed` | 20 | Level 1 cần ~60° gần full lực mới tới khối. Không tới thì tăng |
| `minLaunchSpeed` | 8 | |
| `projectileGravityScale` | 2.2 | Cao = cung ngắn, gắt, dễ đoán hơn |
| `blastUpwardBias` | 0.45 | Hất bổng dễ đọc hơn trượt ngang |
| `barrelChainDelay` | 0.12s | Đủ để **thấy** dây chuyền. Về 0 là trông như một vụ nổ to |
| `settleHoldTime` / `settleTimeout` | 0.4s / 4s | Cảm giác "chờ kết quả" lâu thì giảm |

Camera: `orthographicSize = 8` (cao 16 unit, rộng 9 unit ở 9:16).

---

## 8. Bẫy đã gặp — đừng giẫm lại

| Bẫy | Triệu chứng | Cách xử lý |
|---|---|---|
| `git clone` từ `UnityCodeBase` | `fatal: detected dubious ownership` (thư mục do SID khác sở hữu) | Copy bằng `robocopy` rồi `git init` mới |
| **Khai báo `Reset()` trong class con của `BaseMono`** | Che mất `Reset()` private của base → `ticker`/`pools` lặng lẽ null | Không khai báo `Reset()` ở class con; gán ref bằng tool |
| `BaseLauncher` | Chỉ `Initialize()` BaseMono **trong prefab nó spawn**, bỏ qua object có sẵn trong scene | `BlastSceneLauncher` gọi thêm `Initialize()` theo mảng có thứ tự |
| Font TMP mặc định | LiberationSans SDF **không có dấu tiếng Việt** → chữ thành ô vuông trên máy | Chuỗi HUD để ASCII tiếng Anh |
| Namespace `Dev.Scripts.Editor.BlastOut` | Che namespace `Dev.Scripts.BlastOut` khi tra tên | Dùng `Dev.Scripts.BlastOut.Authoring` |
| Method trùng tên type (`Color`, `Enum`, `Array`) | Nhập nhằng tên trong chính class đó | Đặt tên `Tint`, `EnumIndex`, `ArrayOf` |
| `[MenuItem(..., priority = 0)]` | CS0246 — `priority` là tham số constructor, không phải property | `[MenuItem("...", false, 0)]` |
| API Unity 6 đổi tên | `velocity`/`drag`/`angularDrag` báo obsolete | `linearVelocity`, `linearDamping`, `angularDamping` |
| `FirebaseCppApp-11_8_1.bundle` 155MB | GitHub từ chối file > 100MB, push fail | Bỏ khỏi history + `.gitignore` (xem §9) |
| `git filter-branch` | **Xoá file khỏi ổ đĩa** khi checkout lại HEAD mới | Backup trước, khôi phục sau |
| Path scratchpad quá dài | `Set-Content` báo không tìm thấy path (MAX_PATH) | Dùng `Temp/` của project |
| Máy không có Python / dotnet trong PATH | Script hỏng | Dùng `dotnet.exe` đi kèm Unity (§11) |

---

## 9. Nợ kỹ thuật đã biết

- **Tool dựng scene bằng code.** `Docs/AI_WORKFLOW.md` §1 gọi đích danh đây là *nguồn gốc phần lớn nợ kỹ thuật*. Làm vì AI không có tay trong Editor. **Cách giữ nó không thành nợ:** chạy tool **một lần** để có scaffold, sau đó sửa scene/prefab **trong Editor** — không sửa tool rồi chạy lại. Nếu buộc phải chạy lại, tool sẽ **ghi đè** prefab và `level_01.asset`.
- **`ProjectilePool` tự viết**, không dùng `Dacodelaac.Pools` — Pools cần khai báo `PoolData` sẵn trong asset, thừa cho 1 prefab ở 1 scene. Chuyển sang Pools nếu có nhiều prefab đạn.
- **`ExplosiveBarrel` cấp phát `new WaitForSeconds` mỗi lần nổ.** Nhỏ, nhưng là ứng viên tốt để ghi số trước/sau trong phần profiling.
- **Input dùng `Input.GetMouseButton`** (Input Manager cũ — project đang để `activeInputHandler: 0`). Chạm trên Android đi qua mô phỏng chuột; cần thử đa chạm trên máy thật.
- **SDK ads/analytics vẫn nằm trong project** (Firebase, AdMob, MAX, Adjust) dù ngoài scope — framework Dacodelaac tham chiếu tới chúng nên chưa gỡ. Nếu build Android kẹt ở gradle, đây là chỗ nghi đầu tiên.
- **History đã viết lại một lần** (trước khi push lần đầu) để bỏ 2 binary Firebase desktop. Bản trên máy vẫn còn, chỉ không được track. Windows Editor dùng bản `.dll` vẫn được track.

---

## 10. Ghi chép cho README

### 10.1 AI / Tool usage — ví dụ thật

- **Nghiên cứu thị trường** ~20 game aim-and-shoot, chấm theo đúng tiêu chí đề → tránh được bẫy Angry Birds (destruction physics hỗn loạn, tốn giờ tuning).
- **Mockup HTML 8 màn hình trước khi viết dòng Unity nào** → chốt HUD, bảng màu, progression trong vài phút.
- **Compile thử bằng Roslyn của Unity mà không mở Editor** → bắt được lỗi `MenuItem(priority = 0)` và namespace bị che trước khi mở project.
- **Quét trước khi push** → phát hiện binary Firebase 155MB sẽ làm push fail; xác nhận các ID AdMob là ID test công khai của Google, không phải key thật.
- **Chỗ AI sai và phải sửa:**
  - Ban đầu đề xuất `PhysicsScene2D.Simulate` cho đường ngắm → đổi sang công thức vì đúng tuyệt đối mà rẻ hơn.
  - Chuỗi HUD tiếng Việt sẽ thành ô vuông với font TMP mặc định → đổi sang ASCII.
  - Suýt khai báo `Reset()` trong class con làm mất ref của `BaseMono`.

### 10.2 Kế hoạch profiling

**Kịch bản nặng nhất:** level 6 — nhiều thùng nổ lây dây chuyền cùng lúc.

Đo bằng Unity Profiler trên máy Android thật (Development Build + Autoconnect Profiler):
- `Physics2D.Simulate` trong khung hình dây chuyền nổ
- **GC.Alloc mỗi frame lúc nổ** — kỳ vọng ~0 nhờ buffer `List<Collider2D>` dùng lại trong `BlastResolver`; nghi phạm đã biết: `new WaitForSeconds` trong `ExplosiveBarrel`
- Draw call — sprite cùng một texture trắng, kỳ vọng batch tốt
- FPS thấp nhất trong 2 giây sau vụ nổ

---

## 11. Lệnh hay dùng

### Compile thử không cần mở Unity

Cần `Library/ScriptAssemblies` của `UnityCodeBase` (chứa `Dacodelaac.Runtime.dll` đã build sẵn). Chạy trong PowerShell từ thư mục project:

```powershell
$U   = "C:\Program Files\Unity\Hub\Editor\6000.1.17f1\Editor\Data"
$dn  = "$U\NetCoreRuntime\dotnet.exe"; $csc = "$U\DotNetSdkRoslyn\csc.dll"
$sa  = "D:\ThangLM\UnityCodeBase\Library\ScriptAssemblies"; $out = "Temp\cscheck"
New-Item -ItemType Directory -Force $out | Out-Null
$refs  = Get-ChildItem "$U\Managed\UnityEngine" -Filter *.dll | Where-Object Name -notlike "UnityEditor*" | ForEach-Object FullName
$refs += "$U\Managed\UnityEditor.dll", "$sa\Dacodelaac.Runtime.dll", "$sa\Unity.TextMeshPro.dll", "$sa\UnityEngine.UI.dll"
$refs += Get-ChildItem "$U\NetStandard\ref\2.1.0" -Filter *.dll | ForEach-Object FullName
$r = $refs | ForEach-Object { "/r:$_" }

# Runtime
$src = Get-ChildItem "Assets\Dev\Scripts\BlastOut" -Recurse -Filter *.cs | ForEach-Object FullName
($r + "/target:library","/nostdlib+","/langversion:9","/define:UNITY_EDITOR","/nowarn:0649","/out:$out\BlastOut.Runtime.dll" + $src | ForEach-Object { "`"$_`"" }) | Set-Content -Encoding utf8 "$out\rt.rsp"
& $dn $csc "@$out\rt.rsp"

# Editor (tham chiếu thêm bản runtime vừa build)
$src = Get-ChildItem "Assets\Dev\Scripts\Editor\BlastOut" -Recurse -Filter *.cs | ForEach-Object FullName
($r + "/r:$out\BlastOut.Runtime.dll","/target:library","/nostdlib+","/langversion:9","/define:UNITY_EDITOR","/nowarn:0649","/out:$out\BlastOut.Editor.dll" + $src | ForEach-Object { "`"$_`"" }) | Set-Content -Encoding utf8 "$out\ed.rsp"
& $dn $csc "@$out\ed.rsp"
```

Không in ra dòng `error` nào là sạch. `CS0649` (field chưa gán) là bình thường với `[SerializeField] private`.

### Git

```bash
git log --oneline
git push origin main
```
