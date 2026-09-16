# Quy trình làm việc với AI trong Unity dev

> File này đi theo mọi project clone từ `UnityCodeBase`. Bổ trợ cho [CLAUDE.md](../CLAUDE.md):
> `CLAUDE.md` nói **code phải như thế nào**, file này nói **làm việc với AI ra sao để code ra được như thế**.

---

## 1. Nguyên tắc gốc

Câu hỏi đúng không phải *"AI code cả hay chỉ hỗ trợ"* mà là **"AI có thấy được kết quả việc nó làm không"**.

- **Thấy được** (compile error, log, test pass/fail) → giao cả module, người chỉ review diff.
- **Không thấy được** (prefab binding, cảm giác tween, level có vui không, RAM thật) → AI viết nháp, người quyết.

Trong Unity phần "không thấy được" lớn hơn web/backend rất nhiều, vì logic nằm một nửa trong `.cs`, một nửa trong `.prefab`/`.unity`/Inspector.

**Hệ quả quan trọng nhất:** AI không có tay trong Editor, nên nó **thay thế asset workflow bằng code workflow** — không kéo được material thì viết hàm sinh material, không dựng được scene thì viết SceneBuilder. Đó là nguồn gốc phần lớn nợ kỹ thuật, không phải vì AI viết thuật toán dở.

---

## 2. Phân vùng công việc

| Vùng | Ai làm | Vì sao |
|---|---|---|
| Logic thuần: `Core/` model, thuật toán, level generator/validator, `GameData.<Game>.cs`, extension, utility | **AI code cả** | Không phụ thuộc scene, verify được bằng test/console |
| Editor tool, refactor, đọc crash log → tìm nguyên nhân | **AI code cả** | Đầu ra kiểm chứng ngay, sai thì rollback rẻ |
| MonoBehaviour gameplay, popup script, UI controller, tích hợp SDK ads/IAP/analytics | **AI code + người verify kỹ** | Đúng cú pháp nhưng dễ sai vòng đời, sai thứ tự init |
| Dựng prefab, kéo reference Inspector, scene setup, animation curve, tuning số cho "cảm giác", balance | **Người làm, AI chỉ hướng dẫn** | AI không nhìn thấy Inspector, không cảm được |
| **Sprite Atlas**, texture setting, Project Settings, build setting | **Người làm** | Là asset + settings, gần như 0 dòng code |
| Sửa tay `.unity` / `.prefab` / `.asset` / `.meta`, đổi guid, đụng `Library/` | **Cấm AI** | YAML fileID/guid — hỏng là mất prefab, diff không review nổi |
| `git reset --hard`, `push -f`, xoá file hàng loạt | **Cấm AI tự chạy** | Không hoàn tác được |

---

## 3. Quy trình một task

**0. Git sạch trước khi bắt đầu.** Không để AI code chồng lên thay đổi chưa commit — mất khả năng phân biệt "lỗi của AI" với "lỗi của mình".

**1. Mô tả task như hợp đồng, không như ước muốn.** Nêu: input gì → output gì, file đặt ở đâu, dùng lại class nào có sẵn.

> ❌ "Làm popup shop cho game"
> ✅ "Tạo `ShopPopup : BasePopup` ở `Assets/Dev/Scripts/UI/`, hook `BeforeShow(object data)` nhận `ShopData`, đọc coin từ `GameData`. Prefab tôi tự dựng, chỉ cần script + list `[SerializeField]` cần kéo."

**2. Bắt lập kế hoạch trước khi code.** Task > 1 file: yêu cầu AI đọc code cũ và trình bày cách làm trước, duyệt rồi mới cho code. Sửa một kế hoạch sai mất 2 phút, sửa 400 dòng code sai mất 1 tiếng.

**3. Chốt interface trước khi có thân hàm.** Tên class, chữ ký method, event nào bắn, key `GameData` nào thêm.

**4. Cắt nhỏ — mỗi lát một commit.** Một lát = một thứ chạy được. Commit nhỏ là cái phanh: hỏng thì `git checkout` một file.

**5. Compile là cổng bắt buộc.** Quay lại Unity, chờ compile, đọc Console. **Chưa compile sạch thì chưa được coi là xong** — code chưa qua compiler chỉ là văn bản trông giống code.

**6. Test in-editor, báo lỗi bằng dữ liệu.** Không nói "vẫn lỗi". Nói: log đầy đủ + stack trace + bước tái hiện + kết quả mong đợi.

**7. Đọc diff trước khi commit.** Quét 3 thứ: file nào ngoài phạm vi bị đụng, `.meta` nào bị xoá, vi phạm nào với `CLAUDE.md`.

**8. Có quy ước mới → ghi vào `CLAUDE.md`.** Phải nhắc AI cùng một điều lần thứ hai = lỗi của tài liệu, không phải của AI.

---

## 4. Prompt: nói cái cần, đừng nói cái nghe hay

**Chỉ vào file mẫu có sẵn hiệu quả hơn mô tả bằng lời rất nhiều:**

> ❌ "viết code sạch, tách logic ra"
> ✅ "viết `MergeController` tách khỏi model đúng như `BoardModel` tách khỏi `BoardController`"

**Không bao giờ prompt "dùng design pattern cho dễ mở rộng".** AI sẽ đẻ interface cho class có đúng 1 implementation, factory cho 2 loại object, event bus cho 3 listener. Over-abstraction khó bảo trì y như god class, chỉ khác là rải ra thay vì dồn lại — và khó phát hiện hơn vì nhìn có vẻ "sạch". Thay vào đó nói đúng thứ cần: *"BoardModel là C# thuần, không MonoBehaviour"*, *"số liệu này để trong ScriptableObject"*.

Xem thêm [CLAUDE.md §9.3](../CLAUDE.md) — khi nào đáng tính trước mở rộng, khi nào không.

---

## 5. Bẫy đã gặp thật trong codebase này

Ghi lại để không lặp lại — tất cả đều là phát hiện từ code thật, không phải giả định.

| Bẫy | Bằng chứng | Cách chặn |
|---|---|---|
| **God class** — AI thêm vào file đang mở vì đó là đường ít rủi ro nhất | `PuzzleBase/.../BoardController.cs` = **2561 dòng** | Trần 300 dòng/file (§9.1), chốt kiến trúc ở bước 2 |
| **`partial` giả tách** — bảo AI "tách ra", nó chọn `partial` vì an toàn nhất, nhưng vẫn chung state, độ phức tạp không giảm | `StackSort` tách `BoardController` thành 3 file partial | `partial` không được tính là tách (§9.1) |
| **Sinh material bằng code** thay vì asset kéo Inspector | `MakeUnlit`, `MakeUnlitColor`, `ParticleMat`, cả `StackSortSceneBuilder` | Material qua `[SerializeField]` (§9.4) |
| **`renderer.material`** trên đường chạy lặp → sinh material clone | `BoardFx.cs:64` `r.material = mat` trong hàm gọi mỗi lần bắn FX | `sharedMaterial` + `MaterialPropertyBlock` |
| **`new Material()` runtime không được `Destroy()`** — Material là `UnityEngine.Object`, mất reference không đủ để GC | Cache nằm ở instance field → mỗi lần reload scene là một bộ mới | `Destroy()` khi clear/đổi scene |
| **`Shader.Find()` runtime** — shader không được asset nào tham chiếu sẽ bị strip khỏi build | Nhiều chỗ trong `BoardController`, `BoardFx` | Editor chạy đẹp, máy thật ra màu hồng — chỉ lộ khi build |
| **Không có Sprite Atlas** — AI không nhắc vì thiếu atlas game vẫn chạy y hệt | Cả 3 project: 0 atlas, `m_SpritePackerMode: 0` | Window → Sprite Atlas Tool |

Bốn cái đầu trong `CLAUDE.md` (popup dựng bằng code, `PlayerPrefs`, `Update()` rải rác, singleton tự viết) AI vẫn trôi về nếu prompt không nhắc — kiểm ở bước 7.

---

## 6. Setup một lần

1. **Unity MCP** — quan trọng nhất. Cho AI đọc Console và trạng thái compile, tức là đóng được vòng lặp bước 5–6. Không có nó, mỗi lần sửa lỗi tốn một vòng copy-paste tay.
2. **asmdef** — project này đã có đủ: `Dacodelaac.Runtime`, `Dacodelaac.Editor`, `Dev.Runtime`, `Dev.Editor`. `Dacodelaac.Runtime` không reference ngược `Dev` → ranh giới framework/game được compiler bảo vệ, không chỉ là lời dặn.
3. **`.claude/settings.json`** — allowlist lệnh read-only (`git status`, `git diff`, `git log`) để bớt bấm approve.

---

## 7. Đặt code mới vào đâu

| Loại | Nơi đặt |
|---|---|
| Gameplay, UI, manager riêng game | `Assets/Dev/Scripts/<GameName>/` |
| Editor tool **generic** (không tham chiếu code game) | `Assets/Dacodelaac/Scripts/EditorUtils/` |
| Editor tool **của riêng game** (tham chiếu class game) | `Assets/Dev/Scripts/Editor/` |
| Config/data riêng game của một tool dùng chung | `Assets/Dev/` — kể cả khi code tool nằm ở `Dacodelaac` |

Ví dụ: `LevelEditor.cs` ở `Dev` vì nó tham chiếu `LevelMap`. Sprite Atlas Tool ở `Dacodelaac` vì nó không tham chiếu gì của game — nhưng config `SpriteAtlasGroups.asset` vẫn ở `Dev`, vì cách gom atlas phụ thuộc màn hình của từng game.

> Nhắc lại: framework được **copy** vào từng project, sửa `Dacodelaac` ở một project KHÔNG lan sang project khác. Nâng cấp dùng chung → sửa ở `UnityCodeBase` rồi đồng bộ sang các project cần.
