# İlerleme Günlüğü

> Bu dosyayı agent her görevin başında okur ve sonunda günceller. Kısa ve güncel tut.

## Durum

- **Son tamamlanan görev:** Gün 2 · 2.1
- **Sıradaki görev:** Gün 2 · 2.2
- **Kilometre taşı:** —

## Tamamlanan görevler

| Görev | Özet | Önemli dosyalar |
|---|---|---|
| 1.1 Proje iskeleti | `Assets/_Project` klasör yapısı, 13 modül asmdef'i + Editor + Tests.EditMode, yer tutucu `AssemblyInfo.cs`'ler, `.gitignore`, `.gitattributes` (LFS satırları yorumda) | `Assets/_Project/Scripts/*/LastHaven.*.asmdef`, `Editor/LastHaven.Editor.asmdef`, `Tests/EditMode/LastHaven.Tests.EditMode.asmdef`, `/.gitignore`, `/.gitattributes` |
| 1.2 Core altyapısı | `EventBus<T>` (copy-on-write) + `EventBusRegistry`, `ServiceLocator`, hasar sözleşmesi (`DamageType`, `DamageInfo`, `IDamageable`), `TargetType`, `ITargetable`, `HealthModel` (saf C#), `Health` (MonoBehaviour), `DamagedEvent`/`DiedEvent`, `GameLayers`, `GameTags`. 42 EditMode testi | `Scripts/Core/Messaging/`, `Core/Services/ServiceLocator.cs`, `Core/Damage/`, `Core/Targeting/`, `Core/Health/`, `Core/Events/HealthEvents.cs`, `Core/GameLayers.cs`, `Core/GameTags.cs`, `Tests/EditMode/Core/` |
| 1.3 Üretici altyapısı + test haritası | `IGenerationStep` / `ISceneBuildStep` + `StepDiscovery` (TypeCache), `SceneBuildContext`, `GameSceneBuilder`, `ProjectSetup`, `GenerationPipeline`, 5 menü. 18 yer tutucu URP/Lit materyal. `MapLayoutConfig` + saf `ObstacleLayout`. Game.unity: zemin + NavMeshSurface, üs dairesi, 25 engel, sınır duvarları, güneş, 4 `SpawnZone`. `CompassDirection`, `NavMeshBaker`. 57 EditMode testi | `Editor/Tools/`, `Editor/Generators/`, `Editor/SceneBuild/` (+ `Map/`), `Core/CompassDirection.cs`, `Enemies/Navigation/NavMeshBaker.cs`, `Waves/SpawnZone.cs`, `Tests/EditMode/Generation/`, `Scenes/Game.unity`, `Data/Map/MapLayoutConfig.asset`, `Materials/Placeholder/M_*.mat` |
| 2.1 Girdi sistemi | Core: `IInputReader`, `InputMode`. Player: `GameInputActions` (GDD §4 bağlamaları kodda; Gameplay/BuildMode/UI haritaları), `InputActionNames`, `LocomotionActions`, `InputReader` (servis, mod, son cihaz, Start bas/tut). DebugTools: `InputDebugLogger` (varsayılan kapalı). `InputSceneStep` → `[Systems]/Input`. 76 yeni EditMode testi (toplam 133) | `Scripts/Core/Input/`, `Scripts/Player/Input/`, `Scripts/DebugTools/InputDebugLogger.cs`, `Editor/SceneBuild/InputSceneStep.cs`, `Tests/EditMode/Player/GameInputActionsTests.cs` |

## Assembly'ler

| Assembly | Referanslar |
|---|---|
| LastHaven.Core | — |
| LastHaven.Combat | Core |
| LastHaven.Player | Core, Combat, Unity.InputSystem |
| LastHaven.Enemies | Core, Unity.AI.Navigation |
| LastHaven.Waves | Core, Enemies |
| LastHaven.Building | Core, Combat |
| LastHaven.Economy / GameFlow / Progression | Core |
| LastHaven.UI / Audio / Feedback / DebugTools | 9 oynanış modülü (Core…Progression); birbirlerini referans almaz |
| LastHaven.Editor (sadece Editor) | 13 modülün hepsi, Unity.AI.Navigation |
| LastHaven.Tests.EditMode | 13 modülün hepsi + LastHaven.Editor + Unity.InputSystem + UnityEngine/UnityEditor.TestRunner + nunit.framework.dll; `UNITY_INCLUDE_TESTS` |

Paket referansları: Unity.AI.Navigation (Enemies, Editor), Unity.InputSystem (Player, Tests.EditMode). Cinemachine ve URP henüz yok; ihtiyaç olan görevde eklenecek.

## Editör menüleri

| Menü | Ne yapar |
|---|---|
| `Tools/Last Haven/Rebuild All` | Sırayla: Setup Project → Generate Default Data → Generate Prefabs → Build Game Scene |
| `Tools/Last Haven/Setup Project` | `GameLayers.All`'ı TagManager'a sabit indekslerle, `GameTags.Custom`'ı tag olarak yazar. Her değişikliği loglar |
| `Tools/Last Haven/Generate Default Data` | Tüm `IGenerationStep.GenerateData()`: yer tutucu materyaller, `MapLayoutConfig`. Varsa dokunmaz |
| `Tools/Last Haven/Generate Prefabs` | Tüm `IGenerationStep.GeneratePrefabs()` (şimdilik içi boş) |
| `Tools/Last Haven/Build Game Scene` | Game.unity'yi açar/oluşturur, `[Systems]` ve `[Gameplay]`'i silip tüm `ISceneBuildStep`'lerle kurar, kaydeder, Build Settings'e ekler. `[Environment]`'a dokunmaz |

**Adım sıraları** (araya ekleme için 100'lük boşluk):

| Tür | Adım | Order |
|---|---|---|
| Generation | `PlaceholderMaterialsGenerator` | 0 |
| Generation | `MapGenerator` | 100 |
| Scene | `InputSceneStep` → `[Systems]/Input` (`InputReader`, kapalı `InputDebugLogger`) | 0 |
| Scene | `MapSceneStep` → `[Gameplay]/Map` (Ground, BaseArea, Obstacles, Boundary) | 100 |
| Scene | `LightingSceneStep` → `[Gameplay]/Lighting/Sun` | 200 |
| Scene | `WavesSceneStep` → `[Gameplay]/SpawnZones` | 300 |

## Servisler ve olaylar

| Ad | Tür | Modül | Not |
|---|---|---|---|
| `DamagedEvent` | Olay | Core (`Events/HealthEvents.cs`) | `Health` gönderir: Target, Damage (DamageInfo), AppliedAmount, CurrentHealth, MaxHealth |
| `DiedEvent` | Olay | Core (`Events/HealthEvents.cs`) | `Health` gönderir, `DamagedEvent`'ten sonra: Target, KillingBlow |
| `IInputReader` | Servis | Core (`Input/IInputReader.cs`) → `Player.InputReader` | `[Systems]/Input`. Property'ler + C# event'leri (global EventBus değil). `SetMode(InputMode)` |

## Kararlar

<!-- Belirsiz noktalarda verilen kararlar: tarih, görev, karar, gerekçe -->

**2026-10-14 · Kurulum notları (0.1–0.2)**
- Proje yolu: `C:\Dev\ZombieLast` (OneDrive dışında).
- Unity 6.3 LTS (6000.3.12f1), MCP for Unity v10.3.0, HTTP Local: `http://127.0.0.1:8080/mcp`
- MCP sunucusu Claude Code'a user scope ile elle kaydedildi: `claude mcp add --scope user --transport http UnityMCP http://127.0.0.1:8080/mcp`
- uv için kullanıcı ortam değişkeni `UV_PYTHON=3.12` (yeni Python sürümünde pyyaml derlenemiyordu).
- 0.1 MCP bağlantı testi geçti.

**2026-10-14 · 1.1 Proje iskeleti**
- `Data/<Kategori>` ve `Prefabs/<Kategori>` alt klasörleri önceden açılmadı; kategoriyi ilgili üretici ilk kez yazdığında oluşturur. Şimdilik `Data/` ve `Prefabs/` boş + `.gitkeep`.
- Yaprak modüller (UI, Audio, Feedback, DebugTools) birbirini referans almaz ("kimse bunları referans almaz" kuralı); sadece 9 oynanış modülünü görür.
- Tests.EditMode şimdilik `LastHaven.Editor`'ü referans almıyor; üretici testleri gerektiğinde eklenir.
- Editor ve Tests.EditMode'a da yer tutucu `AssemblyInfo.cs` kondu; böylece boşken de derlenip referansları doğrulanıyor. Gerçek kod gelince silinebilir.
- Her asmdef'te `rootNamespace` = assembly adı (IDE yeni dosyada doğru namespace'i verir).
- ⚠️ Namespace tuzağı: `LastHaven.Editor` assembly'si içinde `Editor` adı namespace'e çözülür. Custom inspector yazarken `UnityEditor.Editor` tam adını kullan.
- `.gitattributes`: Unity YAML türleri `eol=lf` + isteğe bağlı `merge=unityyamlmerge` (kurulum komutu dosyanın içinde). `.asset` için `text` zorlanmadı; ikili `.asset`'ler (ör. LightingData) `text=auto` ile korunur.
- `.gitignore`: `.vscode/` commit'lenir (ortak ayarlar); `*.csproj`, `*.sln`, `*.slnx` ve `.claude/settings.local.json` yok sayılır.
- Unity AI paketleri (`com.unity.ai.assistant`, `com.unity.ai.inference`) kaldırıldı (kullanıcı isteği). Abonelik olmadığı için her domain reload'da kırmızı `NoSubscription` hatası basıyorlardı. Hiçbir paket onlara bağlı değildi; MCP for Unity etkilenmez.
- Domain reload sırasında konsolda görülen `MCP-FOR-UNITY: [WebSocket] Unexpected receive error` uyarısı normal (bağlantı yeniden kuruluyor), yok sayılabilir.

**2026-10-14 · 1.2 Core altyapısı**
- Core'un tüm tipleri tek namespace'te: `LastHaven.Core`. Alt klasörler (`Messaging/`, `Services/`, `Damage/`, `Targeting/`, `Health/`, `Events/`) sadece düzen için.
- **EventBus:** Abone listesi copy-on-write dizi; `Raise` bellek ayırmaz. Raise sırasında eklenen dinleyici bir sonraki Raise'den itibaren çağrılır. Raise sırasında çıkarılan dinleyici, henüz çağrılmadıysa o Raise'de de çağrılmaz (havuza dönen nesne olay almasın diye). Aynı dinleyici iki kez eklenmez. Dinleyici istisna fırlatırsa `Debug.LogException` ile loglanır, diğerleri çalışmaya devam eder.
- **EventBus sıfırlama:** `[RuntimeInitializeOnLoadMethod]` generic sınıfta çalışmıyor. Bu yüzden her `EventBus<T>` static constructor'da kendini `EventBusRegistry`'ye kaydeder; registry `SubsystemRegistration`'da hepsini temizler.
- **ServiceLocator:** `Get<T>` kayıt yoksa `InvalidOperationException` fırlatır. `Unregister<T>(service)` sadece kayıtlı nesne aynıysa siler; sahne geçişinde eski nesnenin `OnDestroy`'u yenisinin kaydını silemez. Yok edilmiş Unity nesneleri kayıtsız sayılır. Aynı tip tekrar kaydedilirse uyarı verilir ve yeni kayıt geçerli olur.
- **Can değerleri `float`:** Yükseltme ve perk çarpanları (+%10 vb.) kesirli hasar üretir.
- **HealthModel:** `TakeDamage` ve `Heal` gerçekte uygulanan miktarı döndürür (aşırı hasar kırpılır). Ölüyken hasar almaz ve iyileşmez. `SetMax(max, refill: false)` mevcut canı yeni maksimuma kırpar; `refill: true` canı doldurur ve ölüyse diriltir. Havuzdan çıkan düşman, yeniden doğan oyuncu ve test kuklası bunu kullanacak. "Kalın Deri" perk'i ileride `SetMax` + `Heal` ile yapılır. Model event'leri: `Damaged(float)`, `Healed(float)`, `Died()`.
- **Health:** Model tembel kurulur; başka bir bileşen kendi `Awake`'inde `SetMax` çağırsa da çalışır, EditMode testlerinde de (Awake yokken) çalışır. Sahibine yerel C# event'leri verir: `Damaged(DamagedEvent)`, `Healed(float)`, `Died(DiedEvent)`. Bunlar global olaydan önce çağrılır. Böylece 100 düşmanın her biri global olayı filtrelemek zorunda kalmaz.
- **GameLayers:** İndeks sabitleri, ad sabitleri (`XxxName`), maske sabitleri (`XxxMask`), `NavMeshBakeMask` (Ground | Obstacle), `All` listesi (Setup Project için), `ToMask`, `Combine`, `Contains`.
- **GameTags:** Yerleşik `Untagged`, `Player`, `MainCamera` ve projeye özel `Generator`, `Enemy`, `Structure`, `Pickup`. `GameTags.Custom` listesini Setup Project (1.3) TagManager'a ekleyecek. O zamana kadar özel tag'lerle `CompareTag` kullanılmamalı.
- Core'un `AssemblyInfo.cs`'i artık yer tutucu değil: testlerin `internal` sıfırlama üyelerine (`EventBus<T>.Clear`, `SubscriberCount`, `EventBusRegistry.ResetAll`, `ServiceLocator.Clear`) erişmesi için `InternalsVisibleTo("LastHaven.Tests.EditMode")` içerir. Tests.EditMode'daki yer tutucu silindi.
- Duman testi `Game.unity` olmadığı için (1.3'te gelecek) `SampleScene`'de yapıldı.

**2026-10-14 · 1.3 Üretici altyapısı + test haritası**
- **Adım keşfi:** `StepDiscovery.CreateAll<T>` → `TypeCache`, soyut olmayan + parametresiz constructor'lı tipler. Önce `Order`, eşitlikte tam tip adı (ordinal) ile sıralanır. Bir adım exception atarsa loglanır, diğerleri devam eder; özet satırı kırmızı olur.
- ⚠️ Test assembly'sinde `IGenerationStep`/`ISceneBuildStep` uygulayan sınıf yazma; TypeCache onu da bulur ve gerçek menüler çalıştırır.
- **SceneBuildContext:** `Systems`, `Gameplay`, `GetMaterial(id)`, `GetOrCreateGroup`, `CreateObject`, `CreatePrimitive(…, withCollider)`. `[Environment]` bilerek verilmez.
- **Build Game Scene** sahneyi kendisi kaydeder. Açık sahne Game.unity değilse ve kaydedilmemiş değişiklik varsa Unity kaydetme diyaloğu açar. MCP ile çalıştırmadan önce açık sahneyi kaydet veya Game.unity'yi aç, yoksa diyalog editörü bekletir.
- **Build Settings:** Game.unity sona eklendi. Şablondan kalan SampleScene hâlâ 0. indekste; MainMenu gelince (14.3) sıra düzenlenecek.
- **Yer tutucu materyaller:** `PlaceholderMaterialId` enum'u → `Materials/Placeholder/M_<Id>.mat`, URP/Lit `_BaseColor`. Renkler `PlaceholderMaterials.GetColor`'da hex olarak durur (GDD §18). Barikat için üç materyal var: Wood/Metal/Concrete. `GetOrCreate` tembel çalışır, prefab üreticileri sırayı düşünmeden çağırabilir. Materyaller "Generate Default Data" aşamasında üretilir.
- **MapLayoutConfig sadece editör için bir SO** (`LastHaven.Editor` assembly'si, `Data/Map/MapLayoutConfig.asset`). Sadece sahne üreticisi okur. Varsayılanlar: 80 m zemin, üs 14 m, spawn 33 m / r 6 m, seed 1337, 25 engel, halka 18–38 m, boşluk 2 m, duvar 5 m yükseklik / 1 m kalınlık. Seed'i değiştirip Build Game Scene ile yeni yerleşim alınır.
- **Engel yerleşimi** (`ObstacleLayout`, saf C#): `System.Random(seed)`, alana göre düzgün yarıçap, rastgele yaw. Her engel çevre dairesiyle (dönüşten bağımsız) kontrol edilir: halkanın tamamen içinde, birbirinden ve üs alanı + 4 spawn bölgesinden `spacing` kadar uzak. Yer kalmazsa daha az engel koyar ve uyarı loglar. Şablonlar: Car 4×1.5×2, Rock 2×1.6×2 (±%30), WallSegment 5×2×0.5.
- Engeller tek primitive küp (BoxCollider, Obstacle layer). Kök/Visual ayrımı yapılmadı; bunlar varlık prefab'ı değil sahne geometrisi, gerçek art `[Environment]`'a gelecek.
- **Üs alanı dairesi:** Unity silindiri (20 kenarlı), 0.02 m kalınlık, collider yok, Default layer, gölge düşürmez. ⚠️ Yarıçap (14) şimdilik `MapLayoutConfig`'te. BaseArea/IBaseArea gelince (Gün 5) tek kaynağa bağlanmalı.
- **Sınır duvarları:** sadece BoxCollider, renderer yok. Zemin kenarının hemen dışında (±40.5). **Default layer**'dalar, NavMesh bake'ine girmezler; duvar tepesinde NavMesh şeridi oluşmaz. Oyuncuyu ve fiziği durdururlar.
- **Güneş:** `[Gameplay]/Lighting/Sun`, (50°, -30°), sıcak beyaz, şiddet 1.2, yumuşak gölge, `RenderSettings.sun`. Değerler şimdilik `LightingSceneStep` sabitlerinde; gün/gece ışığı (Gün 6) gelince config'e taşınır.
- **NavMeshBaker** (Enemies): `[DefaultExecutionOrder(-1000)]`, `RequireComponent(NavMeshSurface)`, Awake'te `BuildNavMesh()` ve süreyi loglar. Veri oluşmazsa hata loglar. `ConfigureSurface(surface)` (All, PhysicsColliders, `GameLayers.NavMeshBakeMask`) scene step'te ve `Reset`'te kullanılır. Runtime'da inspector ayarını ezmez. Bake ~107 ms, ~340 üçgen.
- **CompassDirection** (Core): enum + `CompassDirections.All` + `ToVector3()` (Kuzey +Z, Doğu +X).
- **SpawnZone** (Waves): `Direction`, `Radius`, `Center`. Gizmo her zaman çizilir (seçili olmasa da): yarı saydam disk, kenar çizgisi, merkez küresi ve harita merkezine çizgi. Renkler: K mavi, D sarı, G yeşil, B kırmızı. Scene step alanları `SerializedObject` ile (`_direction`, `_radius`) yazar.
- `Editor`, `Enemies`, `Waves`'teki yer tutucu `AssemblyInfo.cs`'ler silindi (artık gerçek kod var).
- Game.unity'de henüz kamera yok (2.3'te gelecek). Play'de Game view "No cameras rendering" der; kontrol Scene view'dan yapılır.

**2026-10-14 · 2.1 Girdi sistemi**
- **Olaylar C# event'i** (`IInputReader` üzerinde), EventBus değil: tek kaynaklı, yüksek frekanslı ve sadece birkaç tüketicisi var.
- **Üç harita, tek mod:** `SetMode` sadece o modun haritasını açar. Birden fazla modda geçerli girdiler (Pause, Ready, Shop, Cancel, Confirm, ToggleBuildMode, hareket/nişan) her haritada aynı adla tekrar tanımlanır; `InputReader` aynı adlı action'ların hepsini aynı olaya bağlar. Mod, bileşen kapalıyken de saklanır; `OnEnable`'da uygulanır.
- **Bağlama tablosu** (GDD §4'te olmayanlar *italik*):
  - Gameplay: Move WASD/sol çubuk · Aim fare/sağ çubuk · Fire LMB/RT · Reload R/West · Dash Space/South · WeaponSlot 1–3 · WeaponCycle tekerlek/North · Interact E/East · BuildMode B/LB · Shop Tab/Select · Ready Enter/Start basılı · Pause Esc/Start kısa.
  - BuildMode: Move, Aim · BuildMode B/LB · BuildSlot 1–5 · BuildCycle D-pad sol/sağ · Rotate Q/RB · Upgrade U/D-pad yukarı · Sell X/D-pad aşağı · *Confirm (yerleştir) LMB/RT/South* · Cancel RMB/East · Ready, Pause (Gameplay ile aynı).
  - UI: *Confirm Enter/South* · *Cancel Esc/East* · *Shop Tab/Select* (market Tab ile kapansın diye) · *Pause Start (hemen)*.
  - BuildMode'da Fire, Dash, Reload, Interact ve silah tuşları yok; UI'da hareket yok (`MoveInput`/`AimStick` sıfır).
  - UI'da Esc → `CancelPressed` (Pause değil). Duraklat menüsü/market Cancel'ı "kapat" olarak yorumlamalı; gamepad'de Start → `PausePressed`.
- **Ek olay `BuildCyclePressed(int)`:** Görev listesinde yoktu. GDD §4'teki "Yapı seç: D-pad sol/sağ" doğrudan yuva değil sıra değiştirme olduğu için `WeaponCyclePressed` gibi ayrı olay eklendi.
- **Yuva indeksleri 0 tabanlı:** 1 tuşu → `WeaponSlotPressed(0)` / `BuildSlotPressed(0)`.
- **Tekerlek yönü:** aşağı = +1 (sıradaki), yukarı = -1 (önceki). Gamepad Y = +1. Sıra action'ları PassThrough eksen; değerin işareti yön, 0 yok sayılır (`GameInputActions.ToCycleDirection`).
- **Gamepad Start, bas/tut ayrımı:** Gameplay/BuildMode'da tek `GamepadStart` action'ı, `hold` interaction'ı ile. Süre dolarsa `ReadyPressed`, dolmadan bırakılırsa `PausePressed`. Süre kodda değil: Project Settings → Input System Package → **Default Hold Time** (şu an 0.4 sn). Mod değişince harita kapanırken gelen canceled, tuş hâlâ basılıysa Pause sayılmaz.
- **Son kullanılan cihaz:** Her haritanın `actionTriggered`'ı ile izlenir: Gamepad → true, Keyboard/Mouse → false. Fare nişanı bu yüzden **PassThrough**: Value olsaydı harita açılırken başlangıç kontrolü fareden tetiklenir ve gamepad kullanan oyuncu her mod değişiminde klavye/fareye geçmiş sayılırdı. `AimScreenPosition` son değeri tutar; başlangıç değeri `OnEnable`'da `Mouse.current`'tan alınır.
- Hareket Value tipinde: mod değişirken basılı tutulan WASD yeni haritada hemen okunur.
- `IsFireHeld` / `IsInteractHeld` sadece Gameplay haritası açıkken true olabilir.
- **Debug log:** `DebugTools/InputDebugLogger`, `[Systems]/Input` üzerinde **varsayılan kapalı**. Play'de onay kutusunu açınca olayları ve mod/cihaz/basılı tutma/yön değişimlerini `[Input] ...` olarak yazar. Sağ tık menüsünde mod değiştirme var (Mode → Gameplay/BuildMode/UI).
- `AddAction(..., interactions:)` interaction'ı action'a değil **bağlamaya** yazar; testler bağlamayı kontrol eder.
- Player ve DebugTools'taki yer tutucu `AssemblyInfo.cs`'ler silindi.
- ⚠️ **MCP ile Play Mode testi:** Unity penceresi odakta değilse Play Mode kare ilerletmez (`Time.frameCount` 1'de kalır). Duman testinde `execute_code` ile `Application.runInBackground = true` yapıldı (sadece o Play oturumu için). Tuşlar sanal cihazlarla (`InputSystem.AddDevice` + `QueueStateEvent`) simüle edildi, test sonunda cihazlar silindi. Not: aynı karede kuyruğa giren fare olayları birleştirilir (scroll toplanır); tekerlek testinde her olayı ayrı çağrıda gönder.

## Bilinen hatalar / TODO

<!-- - [ ] Açıklama (görev numarası) -->
- [ ] Proje henüz git deposu değil (`git init` yapılmadı, Gün 0 adım 10). Kullanıcı sonraki günlerde yapacak. (1.1)
- [ ] Şablondan kalanlar duruyor: `Assets/InputSystem_Actions.inputactions` (kullanılmayacak, girdi kodda), `Assets/Scenes/SampleScene.unity`. İstenirse silinebilir. SampleScene Build Settings'te hâlâ 0. sırada. (1.1, 1.3)
  - ⚠️ `InputSystem_Actions` **project-wide actions** olarak kayıtlı (Project Settings → Input System Package). Runtime'da kendiliğinden açılır. Bizim action'larla çakışmaz ama boşuna çalışır. Silmeden önce oradaki kaydı kaldır. UI Toolkit'in runtime gezinmesi bu asset'in UI haritasını kullanıyor olabilir; karar UI görevlerinde (Gün 10) verilmeli. (2.1)
- [ ] Game.unity'de kamera yok, Play'de Game view boş görünür. 2.3'te gelecek. (1.3)
- [ ] Üs alanı yarıçapı `MapLayoutConfig` ile ileride gelecek BaseArea config'inde iki kez tutulacak. Gün 5'te tek kaynağa bağla. (1.3)
