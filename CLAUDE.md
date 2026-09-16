# Chuẩn dự án Unity (Dacodelaac Framework)

> File này là **bộ chuẩn gốc**. `UnityCodeBase` là project template — mỗi game mới được clone từ đây nên file này đi theo mọi project. Khi tạo game mới: copy cả thư mục `UnityCodeBase`, đổi tên, giữ nguyên `Assets/Dacodelaac`, code game đặt vào `Assets/Dev`.

Mọi project Unity trong `D:\ThangLM` phải tuân theo các quy ước dưới đây để dùng chung cách tạo popup, lưu data và cấu trúc dự án.

> 📌 **Làm việc với AI:** file này nói *code phải như thế nào*. Còn *quy trình làm việc với AI để code ra được như thế* — phân vùng việc nào giao AI việc nào không, 8 bước cho một task, các bẫy đã gặp thật trong codebase — xem **[AI_WORKFLOW.md](Docs/AI_WORKFLOW.md)**.

---

## 1. Cấu trúc dự án

Hai vùng code tách biệt rõ ràng:

```
<Project>/Assets/
├── Dacodelaac/              # FRAMEWORK DÙNG CHUNG — KHÔNG sửa theo từng game
│   └── Scripts/
│       ├── Core/            # BaseMono, BaseSO, BaseLauncher, Launcher, Ticker
│       ├── UI/Popups/       # BasePopup, BasePopupController (+ Buttons, Layouts, Tabs, SwipeUI)
│       ├── DataStorage/     # GameData, DataStorage, PersistentData
│       ├── Events/          # Event system (ScriptableObject)
│       ├── Variables/       # Biến lưu được (IntegerVariable, BooleanVariable...)
│       ├── Ads/             # AdMgr, RootManager, Singleton<T>, InAppManager
│       ├── Sound/           # SoundMusic, SoundBG
│       ├── ObjectPooling/   # Pools, PooledObjectId
│       ├── LevelSystem/     # LevelManager, Level, LevelData
│       ├── Collections/ Attributes/ Utils/ RemoteConfig/ DebugUtils/
│
├── Dev/                     # CODE RIÊNG TỪNG GAME
│   └── Scripts/
│       ├── <GameName>/      # Gameplay riêng: Core (logic), Gameplay (runtime), Editor (tool)
│       ├── Common/          # MonoSingleton, Constants, Enum, Extensions, Helper, GameUtils
│       ├── Controller/      # SoundManager, SFXController
│       ├── Launcher/        # GameLauncher
│       ├── Level/ GamePlay/ UI/ Utils/
│       └── Demigiant/       # DOTween (thư viện animation)
│
├── Resources/  Scenes/  Plugins/
└── Firebase/ GoogleMobileAds/ MaxSdk/ Adjust/   # SDK ads & analytics
```

**Quy tắc vàng:**
- **`Dacodelaac/` = bất khả xâm phạm theo từng game.** Đây là code chung. Không nhét logic riêng của game vào đây. Sửa framework chỉ khi nâng cấp cho TẤT CẢ game (và phải đồng bộ lại template).
- **`Dev/` = nơi duy nhất chứa code game.** Mọi gameplay, UI, manager riêng đặt ở đây, gom dưới `Dev/Scripts/<GameName>/`.
- Tách logic thuần (Core/Model) khỏi phần visual/runtime (Gameplay/Controller) — xem cách `StackSort` chia `Core/` (BoardModel) vs `Gameplay/` (BoardController).

---

## 2. Popup — luôn dùng `BasePopup` + `BasePopupController`

> ⚠️ **KHÔNG dựng popup bằng code thủ công per-game** (anti-pattern: StackSort tự build UI win/lose bằng code). Mọi popup (Settings, Win, Lose, Shop, Pause, Daily...) phải là prefab kế thừa `BasePopup`.

### Tạo một popup mới
1. Tạo prefab UI có sẵn 3 component (BasePopup tự `[RequireComponent]`): `Canvas`, `CanvasGroup`, `GraphicRaycaster`.
2. Viết script kế thừa `BasePopup`, override các hook cần dùng:

```csharp
using Dacodelaac.UI.Popups;

namespace Dev.Scripts.UI
{
    public class SettingsPopup : BasePopup
    {
        // Nhận data truyền vào lúc Show, setup UI tại đây
        protected override void BeforeShow(object data = null)
        {
            var info = data as SettingsData; // có thể null
            // bind toggle Music/Sound/Vibration từ GameData...
        }

        protected override void AfterShown()   { /* sau khi animation xong */ }
        protected override void BeforeDismiss() { /* trước khi đóng */ }

        public void OnCloseButton() => Close(); // Close() tự gọi Controller.Dismiss(this)
    }
}
```

3. Gắn prefab vào `BasePopupController.popupPrefabs` (Controller nằm trên 1 Canvas riêng). Controller tự instantiate, set sorting order, quản lý stack.

### Hook vòng đời `BasePopup`
`AfterInstantiate` → `BeforeShow(data)` → `AfterShown` → `BeforeDismiss` → `AfterDismissed`; khi bị popup khác đè: `BeforePause`/`AfterPaused`, lúc quay lại: `BeforeResume`/`AfterResumed`. Animation (scale + fade qua DOTween) đã có sẵn — chỉ chỉnh `duration`/`ease`/`animatedGroup` trong Inspector.

### Show / Dismiss
```csharp
[SerializeField] BasePopupController controller;

controller.Show<SettingsPopup>(animated: true, BasePopupController.ShowAction.PauseCurrent, data);
controller.Dismiss<SettingsPopup>(true);
controller.DismissCurrent(true);
var p = controller.GetPopup<SettingsPopup>();
```

**`ShowAction`:**
- `DoNothing` — mở chồng lên, giữ popup hiện tại (overlay).
- `DismissCurrent` — đóng popup hiện tại rồi mở cái mới (thay thế).
- `PauseCurrent` — tạm ẩn popup hiện tại, đóng cái mới thì tự `Resume` lại (stack đúng nghĩa).

Controller quản lý popup theo `LinkedList` (LIFO) + sorting order tự tăng. `Current` = popup trên cùng.

---

## 3. Lưu data — luôn đi qua `GameData`

> ⚠️ **KHÔNG dùng `PlayerPrefs` trực tiếp cho game state.** Mọi đọc/ghi đi qua `GameData` (static API) → `DataStorage` (file nhị phân `data.dat` + backup `.dat-bak`, atomic write).

### Giá trị đơn giản
```csharp
using Dacodelaac.DataStorage;

GameData.Set("coin", 100);
int coin = GameData.Get<int>("coin", 0);   // default = 0 nếu chưa có
GameData.Save();                            // flush ra disk
```

### Khóa riêng của game — thêm bằng `partial class GameData`
Mỗi game tự thêm các property có kiểu rõ ràng. **Lưu ý quan trọng:** file partial phải nằm trong `namespace Dacodelaac.DataStorage` để truy cập được `Storage` (private).

```csharp
// Assets/Dev/Scripts/<GameName>/GameData.<GameName>.cs
namespace Dacodelaac.DataStorage
{
    public static partial class GameData
    {
        const string LEVEL_KEY = "current_level";

        public static int CurrentLevel
        {
            get => Storage.Get(LEVEL_KEY, 0);
            set => Storage.Set(LEVEL_KEY, value);
        }
    }
}
```

Dùng: `GameData.CurrentLevel++; GameData.Save();`

### Dữ liệu phức tạp / có cấu trúc — `IDataPersistent`
```csharp
public class PlayerData : IDataPersistent
{
    public int coins;
    public int level;

    public string Key => "player_data";
    public void StoreData(PersistentData data) { data.Set("coins", coins); data.Set("level", level); }
    public void LoadData(PersistentData data)  { coins = data.Get("coins", 0); level = data.Get("level", 1); }
}

var player = new PlayerData();
GameData.Load(player);   // đọc từ file
GameData.Store(player);  // ghi vào bộ nhớ
GameData.Save();         // flush ra disk
```

### Khi nào gọi `Save()`
- Tại các mốc quan trọng: qua màn, mua hàng (IAP), đổi settings, nhận thưởng.
- Tự động khi pause/quit — **đã xử lý trong `GameLauncher`**, đừng xóa:
  ```csharp
  void OnApplicationPause(bool p) { if (p) GameData.Save(); }
  void OnApplicationQuit()        { GameData.Save(); }
  ```

### Settings có sẵn
`GameData.Music`, `GameData.Sound`, `GameData.Vibration` (bool, mặc định true), `GameData.NoAds`, `GameData.GameSessionCount`. Dùng lại, đừng tạo key trùng.

**Nơi lưu file:** Editor → `<Project>/TempDataStorage/data.dat`; thiết bị → `Application.persistentDataPath`.

---

## 4. Singleton

- **Manager là MonoBehaviour** → kế thừa `MonoSingleton<T>` (truy cập `XXX.Instance`, tự tạo nếu chưa có).
  ```csharp
  public class GameManager : MonoSingleton<GameManager> { }
  ```
- **Service không phải Mono** (ads, config) → `Singleton<T>` trong `Dacodelaac.Ads`, override `Init()`.

---

## 5. Khởi tạo scene — `GameLauncher` / `BaseLauncher`

- Mỗi scene gameplay có 1 `GameLauncher : BaseLauncher`. `Start()` gọi `Initialize()`.
- `BaseLauncher.Initialize()` sẽ: init `Pools` → spawn các prefab trong mảng `prefabs` → gọi `Initialize()` trên mọi `BaseMono` con.
- Đặt manager/hệ thống khởi tạo theo thứ tự vào mảng `prefabs` này thay vì rải `Awake()` khắp nơi.

---

## 6. Vòng đời `BaseMono` (thay cho Awake/Update)

Mọi script gameplay nên kế thừa `BaseMono` và dùng hook thay vì MonoBehaviour mặc định:
- `Initialize()` — khởi tạo (được Launcher gọi).
- `DoEnable()` / `DoDisable()` — thay `OnEnable`/`OnDisable`.
- `ListenEvents()` / `StopListenEvents()` — đăng ký/hủy event.
- `BindVariable()` / `UnbindVariable()` — bind biến.
- `EarlyTick / Tick / LateTick / FixedTick` — bật bằng checkbox trong Inspector; gom update vào hệ thống `Ticker` (không dùng `Update()` rải rác).

---

## 7. Events & Variables (giao tiếp lỏng lẻo)

- **Events** (ScriptableObject): tạo asset `Event` / `IntegerEvent` / `StringEvent`..., `Raise()` để bắn, `AddListener()` hoặc component `EventListener` để nghe. Dùng cho giao tiếp giữa hệ thống không phụ thuộc trực tiếp (vd `loadingEventDoneEvent.Raise()`).
- **Variables** (ScriptableObject): `IntegerVariable`, `BooleanVariable`... đặt `isSavable = true` để tự persist qua `GameData`.

---

## 8. Namespace

- Code framework: `Dacodelaac.*` — **không bao giờ chỉnh sửa.**
- Code game: đặt dưới **`Dev.Scripts.<Feature>`** (vd `Dev.Scripts.Launcher`, `Dev.Scripts.Controller`). Chọn một quy ước và giữ nhất quán toàn project.
- **Ngoại lệ:** file partial mở rộng `GameData` **phải** là `namespace Dacodelaac.DataStorage`.
- `_Root.Scripts.Pattern` (MonoSingleton) là namespace cũ — chấp nhận giữ, nhưng code mới ưu tiên gom về `Dev.Scripts.Common`.

---

## 9. Kiến trúc & Performance

> Mục này tồn tại vì một lý do cụ thể: code sinh ra nhanh (kể cả do AI) luôn trôi về **dồn vào file đang mở** và **sinh asset bằng code thay vì kéo asset trong Editor**. Dự án nhỏ không lộ, dự án to thì toang. Các luật dưới đây là ràng buộc bắt buộc, không phải khuyến nghị.

### 9.1 Trần độ phức tạp

- **300 dòng/file, 400 dòng/class.** Vượt trần = phải tách trước khi thêm tính năng mới.
- **`partial` KHÔNG được tính là tách.** Tách file mà vẫn chung một class thì vẫn chung toàn bộ private state, vẫn không test riêng được, độ phức tạp không giảm. Chỉ dùng `partial` cho code generate và cho `GameData` (mục 3).
- Tách **theo trách nhiệm**: mỗi class trả lời được một câu "class này làm gì" trong một câu ngắn, không có chữ "và".

### 9.2 Ba tầng bắt buộc cho gameplay

| Tầng | Là gì | Được biết | Ví dụ |
|---|---|---|---|
| **Model** (`Core/`) | C# thuần, **không** MonoBehaviour | Luật chơi. Không biết Unity | `BoardModel` |
| **View** (`Gameplay/`) | MonoBehaviour, chỉ hiển thị | Cách vẽ. **Không** biết luật chơi | `CardView` |
| **Config** | ScriptableObject | Số liệu | Level data, balance |

Model không được `using UnityEngine` cho logic (trừ struct toán như `Vector2Int`). Đây là ranh giới quan trọng nhất: **View bám vào prefab, nên tách sai ở đây thì sau này sửa phải dựng lại prefab.**

### 9.3 Khi nào áp design pattern / tính trước mở rộng

**Mặc định: KHÔNG.** Áp pattern sớm cho mọi thứ tạo ra phân mảnh — interface cho class chỉ có 1 implementation, factory cho 2 loại object, event bus cho 3 listener. Kết quả cũng khó bảo trì y như god class, chỉ khác là rải ra thay vì dồn lại.

**Quy tắc quyết định — hỏi "sửa sau đắt hay rẻ?":**

| Sửa sau **RẺ** → không tính trước | Sửa sau **ĐẮT** → phải tính trước |
|---|---|
| Thêm loại tile/màu/booster mới | **Schema save data** — người chơi đã có data cũ, đổi là mất tiến độ |
| Thêm popup, thêm màn hình | **Ranh giới Model/View** — sửa sau phải dựng lại prefab |
| Tối ưu thuật toán | **Số liệu balance** — hardcode rải rác thì sau phải tìm khắp nơi |
| Đổi animation, đổi tween | **Điểm cắm gameplay đã biết chắc sẽ có** (obstacle, booster, luật đặc biệt) |
| Đổi UI layout | **Ranh giới meta ↔ gameplay** — game mới = thay gameplay, giữ meta |

Cột trái: viết thẳng, đơn giản nhất chạy được, refactor khi có **3 ca dùng thật** (rule of three). Cột phải: thiết kế trước cả khi mới có 1 ca, vì chi phí sửa sau không nằm ở code mà ở asset và data người chơi.

Bốn thứ cụ thể phải làm ngay từ đầu mỗi game:
1. **Version cho save data** — `GameData` có key `data_version`, có nhánh migrate. Thiếu cái này thì mọi thay đổi schema về sau đều là breaking.
2. **Số liệu ra ScriptableObject** — không hardcode trong MonoBehaviour.
3. **Chỗ cắm cho luật đặc biệt** — nếu đã biết game sẽ có obstacle/booster, định nghĩa điểm mở rộng ngay. Bằng chứng cái giá của việc không làm: `BoardController.Obstacles.cs` ~1000 dòng bám ngược vào controller.
4. **Hợp đồng meta ↔ gameplay** — gameplay chỉ báo ra "thắng/thua/điểm", không tự gọi popup, không tự cộng coin.

### 9.4 Performance — luật cứng

**Material** (nguồn tràn RAM số một):
- Đọc material: `sharedMaterial`. Đổi màu/property theo từng object: **`MaterialPropertyBlock`**. **Không dùng `renderer.material`** — đó là API phía instance, đi qua nó trên đường chạy lặp là sinh material clone.
- Material là **asset**, nhận qua `[SerializeField]`. **Không `new Material()` runtime, không `Shader.Find()` runtime** — shader không được asset nào tham chiếu sẽ bị **strip khỏi build**: Editor chạy đẹp, máy thật ra màu hồng.
- Nếu buộc phải `new Material()` runtime: **phải `Destroy()`** khi clear/đổi scene. Material là `UnityEngine.Object` — mất reference *không* đủ để GC thu hồi.

```csharp
// ❌ sinh clone mỗi lần gọi
r.material = mat;
r.material.color = c;

// ✅
r.sharedMaterial = mat;
mpb ??= new MaterialPropertyBlock();
r.GetPropertyBlock(mpb); mpb.SetColor(BaseColorId, c); r.SetPropertyBlock(mpb);
```

**Cấp phát & vòng lặp:**
- Spawn/despawn qua **`Pools`** (đã có trong framework). Không `Instantiate`/`Destroy` trong gameplay loop.
- Trong `Tick()`: không `GetComponent`, không `Camera.main`, không `GameObject.Find`, không LINQ, không nối chuỗi, không `new`. Cache hết ở `Initialize()`.

**Mọi lời gọi dưới đây đều rơi xuống native C++ — lấy một lần rồi giữ, đừng gọi lặp:**

| Thay vì | Hãy làm | Vì sao |
|---|---|---|
| `GetComponent<T>()` mỗi lần cần | **`[SerializeField] T x;` kéo sẵn trong Inspector** | Không tốn gì lúc chạy. Đây là lựa chọn số một |
| — nếu không kéo sẵn được | Cache vào field ở `Initialize()` | Vẫn hơn gọi lặp |
| `transform.position = a; transform.rotation = b;` | Cache `transform` vào field; đổi cả hai bằng `SetPositionAndRotation(a, b)` | Mỗi lần chạm `transform` là một lần gọi native |
| `go.tag == "Player"` / `go.name` | Cache sẵn, hoặc dùng enum/id riêng | Sinh GC.Alloc mỗi lần đọc |
| `anim.SetTrigger("Jump")` | `static readonly int JumpId = Animator.StringToHash("Jump")` | String → int được băm lại mỗi lần gọi |
| `mat.SetColor("_BaseColor", c)` | `static readonly int BaseColorId = Shader.PropertyToID("_BaseColor")` | Như trên |

**Không viết magic method rỗng.** `void Update() { }` hay `void Start() { }` không có thân vẫn bị Unity đưa vào danh sách và gọi mỗi frame cho **từng** component. Xoá hẳn nếu không dùng — trong framework này thì dùng hook của `BaseMono` (mục 6) thay vì `Update()`.

**Tự giải phóng `UnityEngine.Object` tạo bằng code.** `Texture2D`, `Sprite`, `Material`, `RenderTexture` sinh runtime **không** được GC thu hồi khi mất reference. Phải `Destroy()` trong `OnDestroy()` / lúc clear scene.

**Log phải biến mất khỏi bản release.** Dùng `Dacoder` (đã gắn `[Conditional]`), không gọi thẳng `Debug.Log`. `[Conditional]` xoá cả **lời gọi lẫn việc tính đối số** ở call site — nên chuỗi nội suy `$"..."` cũng không bị build. `#if` bên trong thân hàm **không** làm được điều đó.

**Asset:** texture UI tắt mipmap; bật nén (ASTC cho mobile). Chi tiết từng ô settings, bảng chọn format và cách kiểm tra — xem **[NOTES.md](Docs/NOTES.md) §1**.

**Sprite Atlas** — AI không tự làm được (đây là asset + Project Settings, gần như 0 dòng code), nên phải làm tay:
- Bật **Project Settings → Editor → Sprite Packer** trước. Mặc định là `Disabled` — atlas có tạo cũng không được pack.
- Gom **theo nhóm cùng xuất hiện / cùng biến mất trên màn hình** (`UI_Common`, `UI_Home`, `UI_Gameplay`, mỗi theme một atlas). **Không** gom cả game vào một atlas lớn: mở màn nào cũng nạp trọn khối vào RAM — đổi draw call lấy đúng cái tràn RAM đang muốn tránh.
- Max size **2048** cho mobile. Background full-screen để **ngoài** atlas.
- Atlas **không** giảm draw call nếu sprite khác material/shader, hoặc nằm trên nhiều Canvas. Đo bằng Frame Debugger trước/sau, đừng mặc định gom xong là nhanh.
- Tool có sẵn: **Window → Sprite Atlas Tool** — sinh atlas từ config, và soát sprite chưa gom / bị trùng atlas / quá lớn. Chạy tab "Soát lỗi" trước mỗi lần build.
  - Code tool ở `Assets/Dacodelaac/Scripts/EditorUtils/Atlas/` (dùng chung mọi game).
  - Config `SpriteAtlasGroups.asset` ở `Assets/Dev/Editor/` (cách gom là **riêng từng game**) — commit vào git.

### 9.5 Khai báo field & phạm vi truy cập

**Mặc định là `private`. `public` phải có lý do.**

Field `public` biến mọi thứ thành API công khai: bất kỳ script nào cũng sửa được, và khi có bug bạn không khoanh vùng được ai đã ghi vào đó. Nó cũng khiến việc đổi kiểu/đổi tên về sau thành breaking change.

| Nhu cầu | Cách khai báo |
|---|---|
| Chỉ dùng trong chính class | `private T x;` |
| **Cần kéo trong Inspector** | `[SerializeField] private T x;` |
| Class con cần dùng | `protected T x;` |
| Class con cần dùng **và** kéo Inspector | `[SerializeField] protected T x;` |
| Bên ngoài cần **đọc** | `public T X => x;` hoặc `public T X { get; private set; }` |
| Bên ngoài cần **ghi** | Viết method có tên rõ nghĩa (`SetLevel(int)`), đừng phơi field |

```csharp
// ❌
public int coin;
public SoundManager soundManager;
public Transform target;

// ✅
[SerializeField] SoundManager soundManager;   // kéo sẵn trong Inspector
[SerializeField] protected Transform target;  // class con dùng tới
int coin;                                     // nội bộ
public int Coin => coin;                      // ngoài chỉ đọc
```

Lưu ý: `[SerializeField]` **vẫn serialize được field `private`** — đó chính là điểm mấu chốt. Không cần `public` chỉ để kéo được trong Inspector.

Điều này gắn thẳng với mục 9.4: `[SerializeField]` kéo sẵn reference là cách bỏ `GetComponent` triệt để nhất — vừa đúng đóng gói, vừa nhanh hơn.

**Ví dụ mẫu — `BaseMono` / `BaseSO`.** Hai class này từng để `[SerializeField] public Pools pools;` vì `AutoBind()` cần ghi vào từ bên ngoài. Cách làm đúng:

```csharp
[SerializeField] private Pools pools;
[SerializeField] private Ticker ticker;

// Class con chỉ ĐỌC, không ghi đè được reference
protected Pools GetPools() => pools;
protected Ticker GetTicker() => ticker;

#if UNITY_EDITOR
// Chỉ cho editor tool. internal ⇒ không lọt ra ngoài assembly, #if ⇒ không vào bản build
internal void SetPools(Pools value) => pools = value;
internal void SetTicker(Ticker value) => ticker = value;
#endif
```

Ba tầng khoá: `private` cho chính nó, `protected` getter cho class con, `internal` setter chỉ tồn tại trong Editor. Không ai ngoài assembly ghi được, và runtime không có đường ghi.

> **Đổi `public X x;` → `[SerializeField] private X x;` mà GIỮ NGUYÊN tên field thì dữ liệu prefab/scene không mất** — Unity serialize theo tên. Đổi tên hoặc đổi kiểu mới mất.

### 9.6 Gate đo — mỗi cụm feature một lần

AI không chạm được ba thứ này, nên không ai đo thay được:
1. **Memory Profiler** — chụp 2 snapshot cách nhau ~10 level, so số lượng `Material` và `Texture2D`. Tăng đều theo level = leak.
2. **Frame Debugger** — đếm draw call.
3. **Build thật lên máy** — shader strip, nén texture, RAM thật chỉ lộ ra ở đây.

### 9.7 Cờ biên dịch hay biến bật/tắt

**Mặc định là biến.** `#if` chỉ dùng khi biến không làm được.

| Hỏi | Dùng |
|---|---|
| Tắt đi có bỏ được **package / quyền / framework native / khai báo lên store** không? | `#if` |
| Code có **không compile được** ở cấu hình kia không? (`DllImport("__Internal")` trên Android, `iOSNotificationCenter` trên Android) | `#if` |
| Cần **xoá cả chi phí tính đối số** ở call site? (`[Conditional]` — xem mục 9.4) | `#if` |
| Chỉ đổi **nhánh nào chạy**? | `[SerializeField] bool` |
| Có lúc cần **cả hai cùng chạy** (dự phòng, A/B, bật tắt từ xa)? | `[SerializeField] bool` — `#if` không làm được |

**Cái giá của `#if`:** mỗi cờ nhân đôi số cấu hình phải biên dịch, và nhánh đang tắt thì **không ai compile cho tới ngày phát hành**. Bằng chứng thật trong repo này: `NotificationService` có một lỗi cú pháp (`Body = e.message,x`) nằm im cho tới khi bỏ cờ `NOTIFICATION` mới lộ ra — vì cờ đó chưa từng được bật.

**Cờ chỉ đúng khi thật sự gỡ được thứ gì đó.** `NOTIFICATION` từng có vẻ hợp lý (bỏ quyền `POST_NOTIFICATIONS`), nhưng package `com.unity.mobile.notifications` vẫn nằm trong `manifest.json` và asmdef vẫn tham chiếu — quyền vào manifest bất kể cờ. Cờ không gỡ được gì thì chỉ còn lại cái giá.

Hiện toàn project chỉ còn **`DEBUG_LOG_ON`** là cờ tính năng, và nó bắt buộc phải là cờ vì `[Conditional]`.

---

## 10. Comment

> Luật này tồn tại vì comment do AI viết luôn trôi về kiểu **kể lại quá trình sửa** — "trước đây X, giờ Y", "bản cũ quên Z". Đó là việc của `git log`. Người đọc code sáu tháng sau cần biết **code này làm gì**, không cần biết nó từng sai ra sao.

**Comment trả lời "cái gì" và "vì sao", không trả lời "đã sửa gì".**

| ❌ Không viết | ✅ Viết |
|---|---|
| "Trước đây hàm này nhận MaxSdk.AdInfo, giờ đổi sang..." | "Kiểu trung lập để mạng nào cũng dùng được." |
| "Bản cũ gọi Hide() trước khi check null nên crash" | "Chưa load xong mà ẩn là NullReferenceException." |
| "Đã gộp 3 class trùng lại thành một" | "Inter thường và splash chỉ khác tham số." |
| "Tôi đã thêm one-shot để tránh gọi hai lần" | "One-shot: nhả callback trước khi gọi, tránh lần sau gọi lại callback cũ." |

**Độ dài:** mặc định **1–2 dòng**. Quá 4 dòng phải có lý do thật sự — thường là một cái bẫy không đọc code mà thấy được (thứ tự khởi tạo, hành vi lạ của SDK, ràng buộc từ nền tảng).

**Không comment thứ code đã nói rõ.** `// tăng biến đếm` trên `count++` là rác. Comment cho **cái không nhìn thấy được**: vì sao chọn cách này, điều gì hỏng nếu làm khác.

**XML doc (`///`)** chỉ dùng cho API mà nơi khác gọi tới — public method, interface, class. Chi tiết nội bộ dùng `//` hoặc `/* */`.

**Tỉ lệ tham khảo:** comment quá ~15% số dòng một file thường là dấu hiệu đang kể chuyện thay vì giải thích. Ngoại lệ: file thuần khai báo API (interface, struct dữ liệu) — ở đó doc chính là nội dung, không phải phần thêm.

---

## 11. Checklist tạo game mới

1. Copy thư mục `UnityCodeBase`, đổi tên project. Giữ nguyên `Assets/Dacodelaac`.
2. Toàn bộ code game đặt trong `Assets/Dev/Scripts/<GameName>/`.
3. Popup = prefab kế thừa `BasePopup`, gắn vào `BasePopupController`. **Không** tự dựng UI popup bằng code.
4. Lưu data qua `GameData`; thêm key riêng bằng `partial class GameData` (namespace `Dacodelaac.DataStorage`).
5. Boot scene bằng `GameLauncher : BaseLauncher`; giữ phần `GameData.Save()` khi pause/quit.
6. Manager dùng `MonoSingleton<T>`.
7. Tận dụng settings/event/variable có sẵn thay vì viết lại.
8. Tách **Model (C# thuần) / View (MonoBehaviour) / Config (ScriptableObject)** ngay từ file đầu tiên — xem mục 9.2.
9. Thêm key `data_version` + nhánh migrate vào `GameData` **trước khi** phát hành bản đầu.
10. Số liệu balance để trong ScriptableObject, không hardcode.
11. Material lấy qua `[SerializeField]`; đổi màu bằng `MaterialPropertyBlock`. Không `new Material()` / `Shader.Find()` runtime — xem mục 9.4.
12. Field mặc định `private`; cần Inspector thì `[SerializeField] private`, class con dùng thì `protected`. **Không dùng field `public`** — xem mục 9.5.
13. Reference tới component khác: **kéo sẵn bằng `[SerializeField]`**, chỉ cache `GetComponent` khi không kéo được. Không gọi `GetComponent` lặp lại lúc chạy — xem mục 9.4.
14. Không để magic method rỗng (`Update()`, `Start()`... không có thân). Dùng hook `BaseMono` (mục 6).
15. Log qua `Dacoder`, không gọi thẳng `Debug.Log` — `[Conditional]` mới cắt được cả chi phí dựng chuỗi ở call site.
