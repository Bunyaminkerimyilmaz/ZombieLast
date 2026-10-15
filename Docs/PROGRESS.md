# İlerleme Günlüğü

> Bu dosyayı agent her görevin başında okur ve sonunda günceller. Kısa ve güncel tut.

## Durum

- **Son tamamlanan görev:** Gün 4 · 4.3
- **Sıradaki görev:** Gün 5 · 5.1
- **Kilometre taşı:** 🏁 M1 (yürü, nişan al, ateş et) commit'lendi (`16b04a8`). Gün 4 bitti: düşmanlar oyunda (F1–F4 ile doğuyor), vuruş hissi var

## Tamamlanan görevler

| Görev | Özet | Önemli dosyalar |
|---|---|---|
| 1.1 Proje iskeleti | `Assets/_Project` klasör yapısı, 13 modül asmdef'i + Editor + Tests.EditMode, yer tutucu `AssemblyInfo.cs`'ler, `.gitignore`, `.gitattributes` (LFS satırları yorumda) | `Assets/_Project/Scripts/*/LastHaven.*.asmdef`, `Editor/LastHaven.Editor.asmdef`, `Tests/EditMode/LastHaven.Tests.EditMode.asmdef`, `/.gitignore`, `/.gitattributes` |
| 1.2 Core altyapısı | `EventBus<T>` (copy-on-write) + `EventBusRegistry`, `ServiceLocator`, hasar sözleşmesi (`DamageType`, `DamageInfo`, `IDamageable`), `TargetType`, `ITargetable`, `HealthModel` (saf C#), `Health` (MonoBehaviour), `DamagedEvent`/`DiedEvent`, `GameLayers`, `GameTags`. 42 EditMode testi | `Scripts/Core/Messaging/`, `Core/Services/ServiceLocator.cs`, `Core/Damage/`, `Core/Targeting/`, `Core/Health/`, `Core/Events/HealthEvents.cs`, `Core/GameLayers.cs`, `Core/GameTags.cs`, `Tests/EditMode/Core/` |
| 1.3 Üretici altyapısı + test haritası | `IGenerationStep` / `ISceneBuildStep` + `StepDiscovery` (TypeCache), `SceneBuildContext`, `GameSceneBuilder`, `ProjectSetup`, `GenerationPipeline`, 5 menü. 18 yer tutucu URP/Lit materyal. `MapLayoutConfig` + saf `ObstacleLayout`. Game.unity: zemin + NavMeshSurface, üs dairesi, 25 engel, sınır duvarları, güneş, 4 `SpawnZone`. `CompassDirection`, `NavMeshBaker`. 57 EditMode testi | `Editor/Tools/`, `Editor/Generators/`, `Editor/SceneBuild/` (+ `Map/`), `Core/CompassDirection.cs`, `Enemies/Navigation/NavMeshBaker.cs`, `Waves/SpawnZone.cs`, `Tests/EditMode/Generation/`, `Scenes/Game.unity`, `Data/Map/MapLayoutConfig.asset`, `Materials/Placeholder/M_*.mat` |
| 2.1 Girdi sistemi | Core: `IInputReader`, `InputMode`. Player: `GameInputActions` (GDD §4 bağlamaları kodda; Gameplay/BuildMode/UI haritaları), `InputActionNames`, `LocomotionActions`, `InputReader` (servis, mod, son cihaz, Start bas/tut). DebugTools: `InputDebugLogger` (varsayılan kapalı). `InputSceneStep` → `[Systems]/Input`. 76 yeni EditMode testi (toplam 133) | `Scripts/Core/Input/`, `Scripts/Player/Input/`, `Scripts/DebugTools/InputDebugLogger.cs`, `Editor/SceneBuild/InputSceneStep.cs`, `Tests/EditMode/Player/GameInputActionsTests.cs` |
| 2.2 Oyuncu hareketi ve nişan | Core: `IPlayer`, `ITargetRegistry` + saf `TargetSet` + `TargetRegistry` (servis), `IDamageFilter` (`Health.DamageFilter`). Player: `PlayerData` (GDD §5), saf `DashModel` ve `PlanarMath`, `PlayerMotor` (CharacterController, kamera yaw'ı, yerçekimi, atılma), `PlayerAim` (fare: nişan düzlemine ışın; gamepad: sağ çubuk), `PlayerCharacter` (kök: IPlayer + ITargetable + hasar filtresi). `PlayerGenerator` (data + prefab), `TargetingSceneStep`, `PlayerSceneStep`, geçici `CameraSceneStep`. 26 yeni EditMode testi (toplam 159) | `Scripts/Core/Player/`, `Scripts/Core/Targeting/`, `Scripts/Player/{Data,Movement,Aim}/`, `Scripts/Player/PlayerCharacter.cs`, `Editor/Generators/PlayerGenerator.cs`, `Editor/SceneBuild/{Targeting,Player,Camera}SceneStep.cs`, `Data/Player/PlayerData.asset`, `Prefabs/Player/Player.prefab` |
| 2.3 Kamera | Core: `CameraPreset` + `CameraPresets.Next`, `ICameraService` (Camera, CurrentPreset, Yaw, SetPreset, boş Shake). Player: `CameraConfig` (GDD §4 preset'leri, look-ahead, takip yumuşatması), `CameraPresetSettings`, saf `LookAhead`, `CameraRig` (Cinemachine 3: Brain + CinemachineCamera + CinemachineFollow, CameraTarget), `CameraPresetHotkey` (F9, sadece editör/dev build). `PlayerMotor`/`PlayerAim` kamerayı `ICameraService`'ten alıyor; geçici `MainCameraCache` silindi. `CameraGenerator`, `CameraSceneStep` (yeniden yazıldı). 12 yeni EditMode testi (toplam 171) | `Scripts/Core/Camera/`, `Scripts/Player/Camera/`, `Editor/Generators/CameraGenerator.cs`, `Editor/SceneBuild/CameraSceneStep.cs`, `Data/Camera/CameraConfig.asset` |
| 3.1 Stat sistemi ve silah verisi | Core/Stats: `StatType` (11 stat), `StatModifier` (Flat / PercentAdd + kaynak id), saf `StatCollection`, `PerkFlag`, `IRunModifiers` + saf `RunModifiers` + `RunModifiersHost` (kayıt bileşeni). Combat: `WeaponData`. `WeaponsGenerator` (Pistol, Shotgun, Rifle; GDD §6), `RunModifiersSceneStep`. 22 yeni EditMode testi (toplam 193) | `Scripts/Core/Stats/`, `Scripts/Combat/Weapons/WeaponData.cs`, `Editor/Generators/WeaponsGenerator.cs`, `Editor/SceneBuild/RunModifiersSceneStep.cs`, `Data/Weapons/*.asset`, `Tests/EditMode/Core/{StatCollection,RunModifiers}Tests.cs` |
| 3.2 Ateş etme | Core: `CombatEvents` (6 olay), `HitTargetType` + `HitTargetTypes.FromLayer`, `DamageableLookup` (collider → IDamageable önbelleği), saf `Countdown`. Combat: saf `WeaponRuntime`, `WeaponUpgradeType`, `WeaponUpgradeConfig`, `HitscanShooter` + `HitscanRequest` + `ShotResult`, saf `SpreadPattern`. Player: `WeaponLoadoutData` + `WeaponSlotDefinition`, saf `WeaponSlots` ve `WeaponArsenal`, `WeaponController`. DebugTools: `TestDummy`, `CombatDebugLogger`. `PlayerWeaponsGenerator`, `TestDummiesGenerator` (+ açma/kapama menüsü), `TestDummiesConfig`, `TestDummiesSceneStep`; `PlayerGenerator` var olan prefab'a eksik `WeaponController`'ı ekliyor. 54 yeni EditMode testi (toplam 247) | `Scripts/Core/Events/CombatEvents.cs`, `Core/Damage/{HitTargetType,DamageableLookup}.cs`, `Core/Timing/Countdown.cs`, `Scripts/Combat/{Weapons,Hitscan}/`, `Scripts/Player/Weapons/`, `Scripts/Player/Data/{WeaponLoadoutData,WeaponSlotDefinition}.cs`, `Scripts/DebugTools/{TestDummy,CombatDebugLogger}.cs`, `Editor/Generators/{PlayerWeapons,TestDummies}Generator.cs`, `Editor/SceneBuild/TestDummies*`, `Data/Player/WeaponLoadout.asset`, `Data/Weapons/WeaponUpgradeConfig.asset`, `Data/Debug/TestDummiesConfig.asset`, `Tests/EditMode/Combat/` |
| 3.3 Havuzlama ve vuruş efektleri | Core/Pooling: `IPoolService`, `IPoolable`, `PoolStats`, `PoolService` (`ObjectPool<T>` ile prefab başına `GameObjectPool` + `PooledInstance`), `PooledLifetime`. Core: `ShotTracedEvent` (saçma başına iz olayı; `HitscanRequest`'e `WeaponId` + `TracerOrigin`). Feedback: `Tracer` (LineRenderer), `ImpactEffect` + `ImpactProfile` (ParticleSystem; et/kıvılcım), `MuzzleFlash` (nokta ışık), `TracerPalette`, `CombatFeedbackConfig`, `FeedbackDirector`. Editor: `FxMaterials` (additive/alpha), `FeedbackPrefabs`, `FeedbackGenerator`, `PoolSceneStep`, `FeedbackSceneStep`. 24 yeni EditMode testi (toplam 271) | `Scripts/Core/Pooling/`, `Core/Events/CombatEvents.cs`, `Scripts/Combat/Hitscan/`, `Scripts/Feedback/`, `Editor/Generators/{Feedback*,FxMaterials}.cs`, `Editor/SceneBuild/{Pool,Feedback}SceneStep.cs`, `Data/Feedback/CombatFeedbackConfig.asset`, `Prefabs/Feedback/{Tracer,Impact,MuzzleFlash}.prefab`, `Materials/Fx/M_Fx{Additive,Alpha}.mat`, `Tests/EditMode/{Core/PoolServiceTests,Feedback/CombatFeedbackTests}.cs` |
| 4.1 Düşman temeli ve hedefleme | Core: `EnemyKilledEvent`, `TargetTypes.IsStructure`. Enemies: `EnemyData` (GDD §7 + zamanlama/gezinme ayarları), saf `TargetSelector` + `TargetSelectorSettings` (kural 1–3, 0.5 sn, rastgele faz), saf `EnemyStateMachine` + `IEnemyBody` + 4 durum (Spawning/Chasing/Attacking/Dead) + `EnemyBehaviourSettings`, `MeleeDamage`, `TargetDistance`, `TargetComponentCache`, `TrackedTarget`, `EnemyNavigator`, `Enemy` (kök: NavMeshAgent + Health + IPoolable + IEnemyBody), geçici `GeneratorPlaceholder`. Editor: `GeneratorPlaceholderSceneStep`. Prefab/data üretimi yok (4.2). 33 yeni EditMode testi (toplam 304) | `Scripts/Core/Events/EnemyEvents.cs`, `Core/Targeting/TargetType.cs`, `Scripts/Enemies/{Enemy,EnemyNavigator,GeneratorPlaceholder}.cs`, `Scripts/Enemies/{Data,Targeting,States,Combat}/`, `Editor/SceneBuild/GeneratorPlaceholderSceneStep.cs`, `Tests/EditMode/Enemies/` |
| 4.2 Düşman tipleri ve debug spawner | Core: `IEnemySpawner` (string id ile). Enemies: `EnemySpawner` (servis; havuz + `NavMesh.SamplePosition` + rastgele yön; oyun başında ön ısıtma), `EnemySpawnerConfig` + `EnemyPoolEntry`, saf `EnemyTypeLookup`. DebugTools: `DebugSpawner` (F1–F4 + OnGUI listesi, sadece editör/dev build). Editor: `EnemyDefinitions` (GDD §7/§18 tablosu), `EnemyPrefabs`, `EnemiesGenerator`, `EnemySpawnerSceneStep`, `DebugSpawnerSceneStep`. Walker/Runner/Tank data + prefab. 28 yeni EditMode testi (toplam 332) | `Scripts/Core/Enemies/IEnemySpawner.cs`, `Scripts/Enemies/Spawning/`, `Scripts/DebugTools/DebugSpawner.cs`, `Editor/Generators/{EnemiesGenerator,EnemyDefinitions,EnemyPrefabs}.cs`, `Editor/SceneBuild/{EnemySpawner,DebugSpawner}SceneStep.cs`, `Data/Enemies/{Walker,Runner,Tank,EnemySpawnerConfig}.asset`, `Prefabs/Enemies/{Walker,Runner,Tank}.prefab`, `Tests/EditMode/Enemies/EnemyTypeLookupTests.cs`, `Tests/EditMode/Generation/EnemyGenerationTests.cs` |
| 4.3 Vuruş hissi ve AnimationDriver | Core: `AnimParams`, `AnimationDriver` (Animator varsa parametre, yoksa prosedürel yedek) + `AnimatorParameters`, saf `ProceduralPose` + `ProceduralPoseSettings` (öne atılma, sarsılma, devrilme), `HitFlash` + `FlashTarget` (MaterialPropertyBlock), `VisualRoot`, `PlayerEvents` (`PlayerDamagedEvent`, `PlayerDiedEvent`). Enemies: saf `Knockback` ve `HealthBarLayout`, `EnemyHitReaction` (flaş + vuruş animasyonu + geri itilme), `EnemyHealthBar` (billboard, 3 sn); `EnemyData`'ya geri itilme mesafesi/süresi; `Enemy` saldırı/hız/ölümü sürücüye verir. Player: saf `HitInvulnerability`, `PlayerAnimation`; `PlayerCharacter` 0.3 sn dokunulmazlık + kırmızı flaş + olaylar; `PlayerMotor.Speed`. Editor: `VisualFeedbackSetup`, `HealthBarMaterials`, `EnemyHealthBarBuilder`; düşman ve oyuncu üreticileri var olan prefab'lara eksik bileşenleri ekliyor. 45 yeni EditMode testi (toplam 377) | `Scripts/Core/Animation/`, `Scripts/Core/Visuals/`, `Scripts/Core/Events/PlayerEvents.cs`, `Scripts/Enemies/Feedback/`, `Scripts/Player/{Animation,Damage}/`, `Editor/Generators/{VisualFeedbackSetup,HealthBarMaterials,EnemyHealthBarBuilder}.cs`, `Materials/HealthBar/`, `Tests/EditMode/Core/{ProceduralPose,AnimationDriver,HitFlash}Tests.cs`, `Tests/EditMode/Enemies/{Knockback,HealthBarLayout}Tests.cs`, `Tests/EditMode/Player/HitInvulnerabilityTests.cs` |

## Assembly'ler

| Assembly | Referanslar |
|---|---|
| LastHaven.Core | — |
| LastHaven.Combat | Core |
| LastHaven.Player | Core, Combat, Unity.InputSystem, Unity.Cinemachine |
| LastHaven.Enemies | Core, Unity.AI.Navigation |
| LastHaven.Waves | Core, Enemies |
| LastHaven.Building | Core, Combat |
| LastHaven.Economy / GameFlow / Progression | Core |
| LastHaven.UI / Audio / Feedback / DebugTools | 9 oynanış modülü (Core…Progression); birbirlerini referans almaz. DebugTools ayrıca Unity.InputSystem (F-tuşu kısayolları) |
| LastHaven.Editor (sadece Editor) | 13 modülün hepsi, Unity.AI.Navigation, Unity.Cinemachine, Unity.RenderPipelines.Universal.Editor |
| LastHaven.Tests.EditMode | 13 modülün hepsi + LastHaven.Editor + Unity.InputSystem + UnityEngine/UnityEditor.TestRunner + nunit.framework.dll; `UNITY_INCLUDE_TESTS` |

Paket referansları: Unity.AI.Navigation (Enemies, Editor), Unity.InputSystem (Player, DebugTools, Tests.EditMode), Unity.Cinemachine (Player, Editor), Unity.RenderPipelines.Universal.Editor (Editor; FX materyalinde `BaseShaderGUI.SetupMaterialBlendMode` için). Runtime URP referansı henüz yok; ihtiyaç olan görevde eklenecek.

## Editör menüleri

| Menü | Ne yapar |
|---|---|
| `Tools/Last Haven/Rebuild All` | Sırayla: Setup Project → Generate Default Data → Generate Prefabs → Build Game Scene |
| `Tools/Last Haven/Setup Project` | `GameLayers.All`'ı TagManager'a sabit indekslerle, `GameTags.Custom`'ı tag olarak yazar. Her değişikliği loglar |
| `Tools/Last Haven/Generate Default Data` | Tüm `IGenerationStep.GenerateData()`: yer tutucu materyaller, `MapLayoutConfig`, `PlayerData`, `CameraConfig`, silahlar + `WeaponUpgradeConfig` (`Data/Weapons/`), `WeaponLoadout`, `TestDummiesConfig`, FX materyalleri (`Materials/Fx/`), `CombatFeedbackConfig`, düşman verileri + `EnemySpawnerConfig` (`Data/Enemies/`), can barı materyalleri (`Materials/HealthBar/`). Varsa dokunmaz (config'e sadece eksik tipler eklenir) |
| `Tools/Last Haven/Generate Prefabs` | Tüm `IGenerationStep.GeneratePrefabs()`: Player prefab'ı, efekt prefab'ları (`Prefabs/Feedback/`), düşman prefab'ları (`Prefabs/Enemies/`). Varsa sadece eksikleri tamamlar (boş data/prefab referansları, eksik `WeaponController` ve referansları, eksik vuruş hissi bileşenleri ve can barı) |
| `Tools/Last Haven/Build Game Scene` | Game.unity'yi açar/oluşturur, `[Systems]` ve `[Gameplay]`'i silip tüm `ISceneBuildStep`'lerle kurar, kaydeder, Build Settings'e ekler. `[Environment]`'a dokunmaz |
| `Tools/Last Haven/Options/Build Test Dummies` | Onay kutulu seçenek: test kuklaları sahneye kurulsun mu (`TestDummiesConfig._enabled`). Değiştirince Build Game Scene'i çalıştır |

**Adım sıraları** (araya ekleme için 100'lük boşluk):

| Tür | Adım | Order |
|---|---|---|
| Generation | `PlaceholderMaterialsGenerator` | 0 |
| Generation | `MapGenerator` | 100 |
| Generation | `PlayerGenerator` → `Data/Player/PlayerData.asset`, `Prefabs/Player/Player.prefab` | 200 |
| Generation | `CameraGenerator` → `Data/Camera/CameraConfig.asset` | 300 |
| Generation | `WeaponsGenerator` → `Data/Weapons/` (Pistol, Shotgun, Rifle, WeaponUpgradeConfig) | 400 |
| Generation | `PlayerWeaponsGenerator` → `Data/Player/WeaponLoadout.asset` (Player prefab'ına `PlayerGenerator` bağlar) | 450 |
| Generation | `TestDummiesGenerator` → `Data/Debug/TestDummiesConfig.asset` | 500 |
| Generation | `FeedbackGenerator` → `Materials/Fx/`, `Data/Feedback/CombatFeedbackConfig.asset`, `Prefabs/Feedback/` (Tracer, Impact, MuzzleFlash) | 600 |
| Generation | `EnemiesGenerator` → `Data/Enemies/` (Walker, Runner, Tank, EnemySpawnerConfig), `Materials/HealthBar/` (2 materyal), `Prefabs/Enemies/` (Walker, Runner, Tank) | 700 |
| Scene | `InputSceneStep` → `[Systems]/Input` (`InputReader`, kapalı `InputDebugLogger`) | 0 |
| Scene | `TargetingSceneStep` → `[Systems]/TargetRegistry` | 10 |
| Scene | `RunModifiersSceneStep` → `[Systems]/RunModifiers` (`RunModifiersHost`) | 20 |
| Scene | `PoolSceneStep` → `[Systems]/Pool` (`PoolService`) | 30 |
| Scene | `FeedbackSceneStep` → `[Systems]/Feedback` (`FeedbackDirector` + config) | 40 |
| Scene | `EnemySpawnerSceneStep` → `[Systems]/EnemySpawner` (`EnemySpawner` + config) | 50 |
| Scene | `MapSceneStep` → `[Gameplay]/Map` (Ground, BaseArea, Obstacles, Boundary) | 100 |
| Scene | `LightingSceneStep` → `[Gameplay]/Lighting/Sun` | 200 |
| Scene | `GeneratorPlaceholderSceneStep` → `[Gameplay]/GeneratorPlaceholder` (GEÇİCİ; Gün 5'te silinecek) | 250 |
| Scene | `WavesSceneStep` → `[Gameplay]/SpawnZones` | 300 |
| Scene | `PlayerSceneStep` → `[Gameplay]/Player` (prefab örneği, `MapLayoutConfig.PlayerSpawnPosition`) | 400 |
| Scene | `CameraSceneStep` → `[Gameplay]/CameraRig` (`CameraRig`, `CameraPresetHotkey`) + `CameraTarget`, `Main Camera` (Brain), `CinemachineCamera` (Follow) | 500 |
| Scene | `TestDummiesSceneStep` → `[Gameplay]/TestDummies` (`CombatDebugLogger`) + `Dummy_1..3`. Seçenek kapalıysa hiçbir şey kurmaz | 600 |
| Scene | `DebugSpawnerSceneStep` → `[Systems]/DebugSpawner` (`DebugSpawner`; F1–F3 verileri, F4 için 4 `SpawnZone`) | 700 |

## Servisler ve olaylar

| Ad | Tür | Modül | Not |
|---|---|---|---|
| `DamagedEvent` | Olay | Core (`Events/HealthEvents.cs`) | `Health` gönderir: Target, Damage (DamageInfo), AppliedAmount, CurrentHealth, MaxHealth |
| `DiedEvent` | Olay | Core (`Events/HealthEvents.cs`) | `Health` gönderir, `DamagedEvent`'ten sonra: Target, KillingBlow |
| `IInputReader` | Servis | Core (`Input/IInputReader.cs`) → `Player.InputReader` | `[Systems]/Input`. Property'ler + C# event'leri (global EventBus değil). `SetMode(InputMode)` |
| `ITargetRegistry` | Servis | Core (`Targeting/ITargetRegistry.cs`) → `Core.TargetRegistry` | `[Systems]/TargetRegistry`. Mantık saf `TargetSet`'te (testlerde doğrudan kullanılabilir) |
| `IPlayer` | Servis | Core (`Player/IPlayer.cs`) → `Player.PlayerCharacter` | Oyuncu prefab'ının kökü; Awake'te kaydolur |
| `ICameraService` | Servis | Core (`Camera/ICameraService.cs`) → `Player.CameraRig` | `[Gameplay]/CameraRig`. `Yaw` hareket/gamepad nişanı için; `Shake` Gün 14'e kadar boş |
| `IRunModifiers` | Servis | Core (`Stats/IRunModifiers.cs`) → saf `Core.RunModifiers` | `[Systems]/RunModifiers` (`RunModifiersHost` Awake'te oluşturup kaydeder). C# event'leri: `StatChanged(StatType)`, `FlagAdded(PerkFlag)` |
| `WeaponFiredEvent` | Olay | Core (`Events/CombatEvents.cs`) | `WeaponController` gönderir, tetik başına bir kez: Source, WeaponId, Origin (namlu), Direction (saçılmasız), PelletCount |
| `ShotHitEvent` | Olay | Core (`Events/CombatEvents.cs`) | `HitscanShooter` gönderir, isabet eden her saçma için (hasar uygulandıktan sonra): Source, Target, TargetType (`HitTargetType`), Point, Normal, Direction, Damage. Iskalama olay göndermez |
| `ShotTracedEvent` | Olay | Core (`Events/CombatEvents.cs`) | `HitscanShooter` gönderir, **her saçma için** (ıska dahil), isabette o saçmanın `ShotHitEvent`'inden sonra: Source, WeaponId, Start (namlu), End (isabet / menzil sonu), Direction, IsHit. Mermi izi bunu kullanır |
| `IPoolService` | Servis | Core (`Pooling/IPoolService.cs`) → `Core.PoolService` | `[Systems]/Pool`. `Get(prefab, pos, rot)`, `Get<T>(T prefab, …)`, `Release`, `Prewarm`, `GetStats`. Havuzlanan objeler `IPoolable` (`OnSpawned`/`OnDespawned`) alır; `PooledLifetime` süre sonunda iade eder |
| `WeaponChangedEvent` | Olay | Core (`Events/CombatEvents.cs`) | `WeaponController`: SlotIndex (0 tabanlı), WeaponId, DisplayName. Oyun başında da bir kez |
| `AmmoChangedEvent` | Olay | Core (`Events/CombatEvents.cs`) | `WeaponController`, sadece eldeki silah için: WeaponId, Ammo, MagSize (etkili), Reserve, HasInfiniteReserve |
| `ReloadStartedEvent` / `ReloadFinishedEvent` | Olay | Core (`Events/CombatEvents.cs`) | `WeaponController`: WeaponId + Duration / Completed (false = silah değişince iptal) |
| `EnemyKilledEvent` | Olay | Core (`Events/EnemyEvents.cs`) | `Enemy` gönderir, ölüm anında (ceset batmadan önce): Position, EnemyId, MoneyReward, XpReward (taban, int), KilledByPlayer (öldüren hasarın kaynağı `Player` tag'li mi) |
| `IEnemySpawner` | Servis | Core (`Enemies/IEnemySpawner.cs`) → `Enemies.EnemySpawner` | `[Systems]/EnemySpawner`. `Spawn(enemyId, position)` → kök obje veya null (uyarıyla). Enemies'i gören modüller için ayrıca `EnemySpawner.Spawn(EnemyData, position)` → `Enemy` |
| `PlayerDamagedEvent` | Olay | Core (`Events/PlayerEvents.cs`) | `PlayerCharacter` gönderir, uygulanan her hasarda (dokunulmazlıkla engellenen gönderilmez): Player, Damage, AppliedAmount, CurrentHealth, MaxHealth. Aynı vuruşun `DamagedEvent`'i de gelir |
| `PlayerDiedEvent` | Olay | Core (`Events/PlayerEvents.cs`) | `PlayerCharacter` gönderir, `PlayerDamagedEvent`'ten sonra: Player, Position, KillingBlow |

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

**2026-10-14 · 2.2 Oyuncu hareketi ve nişan**
- **Kök bileşenin adı `PlayerCharacter`** (`Player` değil): `LastHaven.Player` namespace'iyle çakışırdı; başka namespace'lerde `Player` yazınca namespace'e çözülür.
- **Dash dokunulmazlığı hasarı gerçekten engelliyor:** Core'a `IDamageFilter` + `Health.DamageFilter` eklendi. `PlayerCharacter` kendini filtre olarak atar: `PlayerMotor.IsInvulnerable` (= atılıyor) iken hasar uygulanmaz ve olay gönderilmez. 4.3'teki 0.3 sn hasar sonrası dokunulmazlık da aynı filtreye eklenecek.
- **DashModel (saf C#):** Ayarlar her `TryStart`'ta verilir; Play'de data değişse veya 3.1'deki DashCooldown çarpanı gelse de geçerli olur. Hız = mesafe / süre. Son karede süre aşılırsa fazlası kırpılır, toplam mesafe tam çıkar. **Bekleme süresi atılmanın başladığı anda başlar.** Atılma bitmeden yeni atılma başlamaz (bekleme süresi atılma süresinden kısa olsa da).
- **Atılma yönü:** O karenin hareket yönü; hareket girdisi yoksa baktığı (nişan) yön. `DashPressed` olayı atılmayı ister, atılma `Update`'te başlar. Ölüyken atılma ve yürüme yok; yerçekimi sürer.
- **Hareket:** `PlanarMath.CameraRelative(input, yaw)`. Yerçekimi `Physics.gravity`'den gelir; yerdeyken dikey hız -2 m/sn tutulur (`isGrounded` kararlı kalsın diye). Kamera yaw'ı 2.3'ten beri `ICameraService.Yaw`'dan gelir.
- **Nişan düzlemi:** Ayaktan `PlayerData.AimHeight` (1.2 m) yükseklikte. Namlu yüksekliğiyle aynı olunca mermi imlecin altındaki gövde hizasına gider. Üretici `Muzzle`'ı bu yüksekliğe koyar; data değişirse prefab'daki Muzzle elle taşınmalı.
- **Gamepad nişanı:** Sağ çubuk kameraya göre çevrilir. Çubuk boştaysa oyuncu yürüme yönüne döner, o da yoksa son yönde kalır. Görev metninde yoktu; sağ çubuğu kullanmayan oyuncu da dönebilsin diye eklendi. `AimPoint` = oyuncu + yön × `GamepadAimDistance` (6 m). Fare imleci oyuncuya 0.1 m'den yakınsa yön değişmez. Dönüş anlık.
- **PlayerAim** `[DefaultExecutionOrder(10)]`: `PlayerMotor`'dan sonra çalışır; o karenin pozisyonunu ve yürüme yönünü kullanır.
- **PlayerData:** GDD §5'in hepsi (can, hasar sonrası dokunulmazlık, hız, dash, toplama yarıçapı) + nişan ayarları. Yeniden doğma süresi PlayerData'da değil; 6.1'de `GameConfig`'e gelecek.
- **TargetRegistry:** Tür başına liste. `FindNearest` mesafeyi **yatay düzlemde (XZ)** ölçer, `maxDistance` dahildir. `IsTargetable` false olanları ve yok edilmiş Unity nesnelerini atlar. `GetAll` bellek ayırmaz; hedeflenemeyenler dahil hepsini döndürür. `Register`/`Unregister` bool döner.
- **Servis + kayıt zamanlaması:** Servisler Start'ta `TryGet` ile alınır; yoksa uyarı loglanır, exception atılmaz. İlk abonelik/kayıt Start'ta yapılır, sonrası `OnEnable`/`OnDisable`'da (`_started` bayrağı). `PlayerCharacter.IsTargetable` = bileşen etkin ve oyuncu canlı.
- **Prefab:** Kök `Player` (Player layer + tag) üzerinde: CharacterController (boy 2, yarıçap 0.5, merkez y = 1 + skinWidth; görsel kapsül yere otursun diye), `Health`, `PlayerMotor`, `PlayerAim`, `PlayerCharacter`. `Visual/Body` (kapsül) + `Visual/Nose` (küp): collider'sız, `M_Player`. `Muzzle` kökün çocuğu: (0, AimHeight, 0.7). Prefab varsa sadece boş `_data` alanları doldurulur (`EditorAssetUtility.AssignIfMissing` + `SavePrefabAsset`).
- **Ortak primitive:** `PrimitiveFactory.Create` (Editor/Tools). `SceneBuildContext.CreatePrimitive` artık bunu kullanıyor.
- **Başlangıç noktası:** `MapLayoutConfig.PlayerSpawnPosition` = (0, 0, -3), merkezin 3 m güneyi. Yeni alan; mevcut asset'te varsayılan değerle gelir.
- **Geçici sabit kamera** (`CameraSceneStep`): Kamera olmadan fare nişanı test edilemiyordu. GDD §4 izometrik açısı (pitch 50, yaw 45, 22 m, FOV 35), başlangıç noktasına bakar, MainCamera tag'li, AudioListener'lı. 2.3'te Cinemachine kurulumuyla değiştirildi.
- ⚠️ **Test assembly'sindeki MonoBehaviour GameObject'e eklenemez:** Tests.EditMode sadece Editor platformunda; Unity "editor script" diyerek `AddComponent`'i reddeder. Testte "yok edilmiş Unity nesnesi" gerekiyorsa ScriptableObject kullan (`TargetRegistryTestTarget`).
- **MCP yokken derleme kontrolü:** Unity'nin ürettiği csproj'lar scratchpad'e kopyalanır, kaynaklar glob ile eklenir ve `dotnet build` ile derlenir. Yeni dosyaları da görür. Unity içi doğrulamanın yerini tutmaz.
- **MCP duman testi notları:**
  - ⚠️ `execute_code` editör güncellemesinde çalışır. Oradan okunan `IInputReader.MoveInput` sanal tuş basılıyken bile sıfır görünebilir (editör ve oyun girdi durumu ayrı). Hareketi pozisyon farkından ölç.
  - Bırakma olayı gönderilmeyen sanal tuş basılı kalır; testten sonra boş state gönder ve cihazları sil.
  - Kısa olayları (0.18 sn atılma) yakalamak için `Time.timeScale = 0.01` yap, kontrol et, sonra 1'e geri al.
  - Oyuncuyu ışınlamak için önce CharacterController'ı kapat, pozisyonu ver, sonra aç.

**2026-10-14 · 2.3 Kamera**
- **Tek CinemachineCamera, sabit açı:** Rotasyon bileşeni yok; CM3 açıyı CinemachineCamera'nın transform'undan alır. `CameraRig.SetPreset` rotasyonu, `CinemachineFollow.FollowOffset`'i (= rotasyon × geri × mesafe) ve lens FOV'unu yazar. Açı hiç değişmediği için W her preset'te ekranın yukarısıdır.
- **Preset geçişi anlık** (blend yok). F9 geliştirici kısayolu ve ileride ayarlar menüsü için yeterli. Yumuşak geçiş istenirse iki CinemachineCamera + öncelik ile brain blend'ine geçilebilir.
- **`ICameraService.Yaw` eklendi** (görev listesinde yoktu). Hareket ve gamepad nişanı yaw'ı buradan alır. Kameranın transform açısından okunmaz; hem bir kare gecikme olmaz hem de pitch 90'a yaklaşınca euler açısı bozulmaz. Preset pitch'i bu yüzden 1–89 ile sınırlı.
- **CinemachineFollow:** `BindingMode.WorldSpace`, `PositionDamping` = (d, d, d), d = `CameraConfig.FollowDamping` = **0.3** (GDD'de yok; tepkili ama yumuşak olsun diye seçildi). Ayarlar runtime'da `CameraRig.Awake`'te config'ten tekrar uygulanır; sahne adımı aynı değerleri editör önizlemesi için de yazar.
- **Look-ahead:** `CameraTarget` = oyuncu + (nişan noktası − oyuncu) yatayda, uzunluğu en fazla `LookAheadDistance` (3 m) olacak şekilde kırpılır. İmleç yakınsa kayma da azdır. `CameraRig` `[DefaultExecutionOrder(20)]`: `PlayerAim`'den (10) sonra, Update'te çalışır; Brain LateUpdate'te kullanır. Başlangıçta hedef oyuncuya konur ve `PreviousStateIsValid = false` ile sönümlemesiz oturulur.
- **Kurulum:** `[Gameplay]/CameraRig` (`CameraRig` + `CameraPresetHotkey`) altında `CameraTarget`, `Main Camera` (Camera, AudioListener, CinemachineBrain, MainCamera tag) ve `CinemachineCamera` (CinemachineCamera + CinemachineFollow). Kameralar editörde de varsayılan preset'le başlangıç noktasına bakar. Referanslar `SerializedObject` ile bağlanır.
- **F9 kısayolu** (`CameraPresetHotkey`): Gövdesi `#if UNITY_EDITOR || DEVELOPMENT_BUILD` içinde; sınıf her build'de var (sahnede "missing script" olmasın). Kendi `InputAction`'ını (`<Keyboard>/f9`) kodda kurar. Oyun girdisi olmadığı için `IInputReader`'a eklenmedi. Her basışta `[Camera] Preset: X` loglar.
- `BindingMode` `Unity.Cinemachine.TargetTracking` namespace'indedir (ayrı using gerekir).
- ⚠️ **MCP duman testi:** Input System ayarı `PointersAndKeyboardsRespectGameViewFocus`. Unity/Game view odakta değilken sanal klavye olayları oyuna **gitmez** (2.2'de odak varken çalışmıştı). Test sırasında Unity odakta değilse girdiye bağlı kontrolleri servis/bileşen üzerinden doğrula (ör. `ICameraService.SetPreset`, `CameraPresetHotkey.OnPerformed` reflection ile). Proje ayarını testte değiştirme.
- ⚠️ Duman testi sırasında kullanıcı Unity'de Play'i aynı anda kullanırsa oturum kapanabilir ve oyuncu kendiliğinden hareket ediyor gibi görünür. Beklenmedik hareket/kapanmada önce `InternalEditorUtility.isApplicationActive`'e ve Editor.log'a bak.

**2026-10-14 · 3.1 Stat sistemi ve silah verisi**
- **Formül:** Sonuç = (taban + Σflat) × max(0, 1 + Σyüzde). Yüzdeler **toplanır, birbirine çarpılmaz** (Hızlı Tetik ×2 = +%30). PercentAdd değeri **oran** olarak tutulur: 0.15 = +%15, -0.4 = -%40. Çarpan 0'ın altına inmez.
- **StatType (11):** PlayerDamage, PlayerFireRate, PlayerMagSize, PlayerMaxHealth, DashCooldown, PickupRadius, CoinValue, TurretDamage, TurretFireRate, TurretRange, BarricadeMaxHealth. Hepsi GDD'deki bir perk/market/gece etkisine karşılık gelir. Hareket hızı veya doldurma süresini değiştiren bir etki GDD'de olmadığı için eklenmedi. Enum değerleri açıkça numaralı (SO'larda int saklanır): **sırayı değiştirme, yenisini sona ekle.** `PerkFlag` için de aynısı geçerli.
- **Silah yükseltmeleri ve taret seviyeleri `IRunModifiers`'ta değil:** Silah yükseltmesi her silaha ayrıdır (3.2 `WeaponRuntime`); taret seviyeleri yapı verisindedir (Gün 9). `IRunModifiers` sadece run genelindeki etkiler içindir: perk'ler, market Mıknatıs, gece olayları.
- **StatCollection:** Taban değer tutmaz; tüketici kendi data'sından verir (`Get(stat, base)`). Flat ve yüzde toplamları stat başına önbelleklidir; `Get` O(1) çalışır ve bellek ayırmaz. Her ekle/çıkar işleminde etkilenen statın toplamı baştan hesaplanır, float kayması birikmez. `Changed(StatType)` etkilenen her stat için bir kez çağrılır, tüm toplamlar güncellendikten sonra. `RemoveBySource` kaldırılan sayıyı döner. Kaynak id'leri ordinal karşılaştırılır, boş olamaz (`ArgumentException`).
- **RunModifiers:** Saf C# sınıfı, `IRunModifiers`'ı uygular. `RunModifiersHost` (kayıt bileşeni) Awake'te oluşturup **saf nesneyi** ServiceLocator'a kaydeder, OnDestroy'da siler; yeni sahne = yeni run. Bayraklar bool dizisinde tutulur, kaldırma yok (perk'ler run boyunca kalır). `FlagAdded` sadece ilk eklemede çağrılır. Değişim olayları C# event'i (EventBus değil): tüketiciler az ve servise doğrudan bağlı.
- **WeaponData:**
  - `_spreadAngle` koninin **toplam** açısıdır; her mermi ±yarısı kadar sapar (tabanca 2° → ±1°). 3.2'deki HitscanShooter buna uymalı.
  - `ReserveAmmo` -1 ise sonsuzdur (`InfiniteReserve`, `HasInfiniteReserve`). `FireInterval` = 1 / fireRate.
  - Market fiyatı ve yükseltme değerleri (§6) WeaponData'ya eklenmedi; market (Gün 10) ve 3.2 kendi verisini kuracak.
- **Ses/VFX alanları:** `_fireSound` şimdilik `AudioClip`, `_muzzleVfx` `GameObject` (prefab); ikisi de opsiyonel. 14.2'de `SoundData` gelince `_fireSound` tipi değişecek (şu an boş olduğu için veri kaybı olmaz).
- **Silah asset'leri:** `Data/Weapons/Pistol.asset`, `Shotgun.asset`, `Rifle.asset`; id'ler `pistol`, `shotgun`, `rifle`; adlar Tabanca, Pompalı, Tüfek. Değerler `WeaponsGenerator`'daki GDD §6 tablosundan sadece **ilk oluşturmada** yazılır, sonra dokunulmaz. Mermi izi renkleri GDD'de yok, ayırt edilsin diye seçildi: tabanca açık sarı, pompalı turuncu, tüfek açık camgöbeği. `WeaponsGenerator.LoadOrCreateAll()` diğer üreticilere hazır liste verir.
- Combat'taki yer tutucu `AssemblyInfo.cs` silindi (artık gerçek kod var).

**2026-10-14 · 3.2 Ateş etme**
- **Etkili değer:** `IRunModifiers.Get(Player*, WeaponData × yükseltme çarpanı)`. Yükseltmeler sadece hasar, atış hızı ve şarjöre; perk'ler `PlayerDamage`, `PlayerFireRate`, `PlayerMagSize`'a. Doldurma süresi, saçılma, menzil ve saçma sayısı sadece data'dan.
- **Yükseltmeler:** `WeaponUpgradeConfig` (Combat SO, `Data/Weapons/WeaponUpgradeConfig.asset`), tüm silahlar için ortak; seviyeler silah başına `WeaponRuntime`'da. Seviye artışları **toplanır** (hasar 5. seviyede ×1.5). Maks. seviye GDD'deki fiyat sayısından: 5 / 3 / 3. Fiyatlar eklenmedi (market, Gün 10). `TryUpgrade` ödeme yapmaz. `WeaponUpgradeType` şimdilik Combat'ta; market ihtiyaç duyarsa Core'a taşınır.
- **Şarjör yuvarlama:** en yakın tam sayı, x.5 yukarı (6 × 1.25 = 7.5 → 8), en az 1. Kapasite artınca mevcut mermi değişmez; dolu şarjör de doldurulabilir hale gelir.
- **Mermi:** Saçma sayısından bağımsız, atış başına 1 mermi. Sonsuz yedekte `Reserve` 0'dır, `HasInfiniteReserve` true.
- **Atış zamanlayıcısı:** Bekleme süresi sadece pozitifken azalır; 0'ın altına inen kısım (en fazla bir kare) sonraki atışa devreder. Böylece atış hızı FPS'ten bağımsız (0.07 sn aralık 60 FPS'te ~14.3/sn, devretmese 12/sn olurdu). Devreden süre en fazla bir aralık: takılmadan sonra seri atış olmaz. Karede en fazla 1 atış. Atış hızı perk'lerle 0'a inse de 0.01/sn'ye kırpılır.
- **Doldurma:** R ile (`TryStartReload`); şarjör boşken bir sonraki `Tick` kendiliğinden başlatır (yedek varsa). Doldururken ateş edilemez, ateş doldurmayı kesmez. **Silah değişince doldurma iptal** (`ReloadFinishedEvent.Completed = false`); boş silaha dönülünce kendiliğinden yeniden başlar. **Sadece eldeki silah Tick edilir:** cepteki silahın atış bekleme süresi durur (hızlı silah değiştirerek bekleme atlanamaz).
- **Saçılma yatay düzlemde** (dünya yukarı ekseni etrafında): 3D koni pompalının (±9°) saçmalarını 7.6 m'de yere gömerdi. **Katmanlı örnekleme:** koni saçma sayısı kadar dilime bölünür, her saçma kendi diliminde rastgele. Tek saçmada tüm koni.
- **HitscanShooter:** Saçma başına tek `Physics.Raycast` (delme yok, trigger'lar yok sayılır). Önce hasar (`DamageType.Bullet`), sonra `ShotHitEvent`. Ölü hedefe isabet olay gönderir ama hasarı 0. `ShotHitEvent.Target` = IDamageable'ın objesi (kök), yoksa collider'ın objesi. Sonuç listesi tekrar kullanılır (bellek ayırmaz), bir sonraki `Fire`'a kadar geçerli. Rastgelelik `UnityEngine.Random`.
- **`HitTargetType`** (Core): Surface / Enemy / Structure / Player, collider layer'ından (`HitTargetTypes.FromLayer`). 3.3'teki vuruş efekti buna bakacak.
- **`DamageableLookup`** (Core): "Update'te GetComponent yok" kuralı için collider → IDamageable önbelleği (`GetComponentInParent`, null da önbelleğe girer). Yok edilmiş hedef tekrar aranır. ⚠️ Collider'a sonradan IDamageable eklenirse görmez; bileşenler prefab'da hazır olmalı. Düşman yakın dövüşü ve patlamalar da bunu kullanmalı.
- **`Countdown`** (Core/Timing, saf): doldurma ve kukla yeniden doğması kullanıyor; oyuncu yeniden doğması (6.1) da kullanabilir.
- **Oyuncu mermisinin maskesi** (`WeaponLoadoutData._hitMask`): Ground | Obstacle | Enemy. Yapılar ve oyuncu yok: mermi kendi barikatının/taretinin üstünden geçer. Default layer'daki görünmez sınır duvarları da yok.
- **Işın başlangıcı:** Işın oyuncunun ekseninden, namlu yüksekliğinde başlar (namlu − yön × namlunun ileri uzaklığı); menzil o kadar uzar. Oyuncuya yapışık düşman ve namlunun girdiği duvar da vurulur. `WeaponFiredEvent.Origin` görsel için namlunun kendisi.
- **Yön:** `PlayerAim.AimDirection` (yatay). `WeaponController` `[DefaultExecutionOrder(15)]`: PlayerAim'den (10) sonra.
- **Silah bileşenleri:** `WeaponController` (MonoBehaviour) sadece girdi, Update ve atış. Silahlar ve olaylar saf `WeaponArsenal`'da (runtime'lar, seçim, `WeaponChanged`/`AmmoChanged`/doldurma olayları); yuva mantığı saf `WeaponSlots`'ta. Yuvalar ve başta açık olanlar `WeaponLoadoutData`'da (`Data/Player/WeaponLoadout.asset`: Tabanca açık, Pompalı, Tüfek). Kilitli yuva seçilemez, tekerlek kilitlileri atlar ve başa sarar. Silah değişimi anlık. Kilitli yuvaya basınca `[Weapon] N. yuva kilitli` logu. Ölüyken ateş ve doldurma yok. `AmmoChangedEvent` sadece eldeki silah için. `WeaponArsenal.Unlock(slot)` hazır (market Gün 10'da `WeaponUnlockRequestedEvent` ile bağlanacak). Test için inspector'da `WeaponController` sağ tık → **Debug/Tüm silahları aç** (Play'de).
- **Prefab güncelleme:** `PlayerGenerator` var olan Player prefab'ına eksik `WeaponController`'ı **ekliyor** ve boş `_loadout`/`_muzzle`'ı dolduruyor (`EditorAssetUtility.EditPrefab`: `LoadPrefabContents` → düzenle → değişiklik varsa kaydet). Sadece ekleme yapar; var olan bileşen ve ayarlara dokunmaz. Yeni sistemler eski prefab'lara böyle eklenebilir.
- **Test kuklaları:** `TestDummiesConfig` (editör SO, `Data/Debug/`): açık/kapalı, konumlar, can 100, yeniden doğma 3 sn. Varsayılan konumlar r = 16 m (üs 14 m'nin dışı, engel halkası 18 m'nin içi), doğu-güneydoğuda 2.5 m arayla; oyuncuya ~15 m. Güney daha yakın olurdu ama izometrik kamerada ekranın altında kalıyordu. Kukla: kök (Enemy layer, CapsuleCollider, Health, `TestDummy`) + `Visual` (gri kapsül, collider'sız). Tag'siz, hedef kaydına girmez. Ölünce collider kapanır ve Visual gizlenir. Yeni materyal `M_TestDummy` (#9A9A9A, GDD'de yok).
- **`CombatDebugLogger`** (DebugTools): HUD gelene kadar silah değişimini ve doldurmayı loglar; mermi ve isabet logları onay kutusuyla (varsayılan kapalı). Test kuklalarıyla aynı seçenekle kurulur (`[Gameplay]/TestDummies` objesinde).
- **MCP duman testi:** Unity odakta olmadığı için ateş `WeaponController.Fire` reflection ile, girdi olayları `InputReader`'ın event alanları (`WeaponSlotPressed` vb.) reflection ile tetiklendi. Nişan için `PlayerAim` kapatılıp `_aimDirection` yazıldı. Oyuncuyu ışınlarken CharacterController kapatıldı.

**2026-10-14 · 3.3 Havuzlama ve vuruş efektleri**
- **Mermi izi için yeni olay `ShotTracedEvent`** (Core): Iskalayan saçma `ShotHitEvent` göndermediği için iz çizilemiyordu. `HitscanShooter` artık her saçma için (ıska dahil) bu olayı atar: Start (namlu), End (isabet / menzil sonu), Direction, IsHit, WeaponId. `ShotHitEvent`'in anlamı değişmedi. `HitscanRequest`'e iki opsiyonel alan eklendi: `WeaponId` ve `TracerOrigin` (verilmezse ışın başlangıcı). `WeaponController` bunları dolduruyor (görevde Player yoktu; tek satırlık zorunlu değişiklik). Taretler (Gün 9) aynı alanları verirse izleri kendiliğinden çizilir.
- **İz rengi:** `TracerPalette` (Feedback) `WeaponData.TracerColor`'ı silah id'siyle (ordinal) eşler; bilinmeyen id `CombatFeedbackConfig.DefaultTracerColor` (beyaz) alır. Silah listesi config'te; üretici eksik silahları ekler.
- **Namludan geriye düşen iz çizilmez:** Işın oyuncu ekseninden başladığı için yapışık hedefte isabet namlunun gerisinde olabilir. `FeedbackDirector.ShouldDrawTracer`: namludan ileriye en az `MinTracerLength` (0.3 m) uzanmayan iz atlanır.
- **PoolService:**
  - Prefab başına `GameObjectPool` (`ObjectPool<PooledInstance>`, `collectionCheck: false`, üst sınır yok). Örnekler `[Systems]/Pool/<prefab adı>` altında durur (etkinken de). Yeni örnek kapalı `_Staging` kabında oluşturulur, kapatılıp asıl kaba taşınır: Awake/OnEnable ilk `Get`'e kadar çalışmaz. Adı prefab adıyla aynı ("(Clone)" yok).
  - `Get`: konum/rotasyon → `SetActive(true)` → `IPoolable.OnSpawned` (kökte ve çocuklarda, oluşturulurken bir kez toplanır). `Release`: `OnDespawned` → `SetActive(false)` → başka yere taşındıysa kaba geri. Callback istisnaları loglanır, havuz bozulmaz.
  - `Get<T>(T prefab)`: bileşen örnek başına önbellekli (tekrar GetComponent yok). `Release` obje → kayıt sözlüğüyle (GetComponent yok).
  - Çift `Release` uyarı + yok sayılır. Havuzdan gelmeyen obje uyarıyla yok edilir. Havuz dışında Destroy edilmiş örnek `Get`'te uyarıyla atlanır. Null prefab hata loglar, null döner.
  - `Prewarm(prefab, n)`: havuzda **en az n kapalı örnek** olacak şekilde eksikleri oluşturur; `OnSpawned` çağrılmaz. Prewarm sayısı aşılırsa (oynanışta Instantiate) prefab başına **bir kez uyarı** loglanır → config'teki sayıyı artır.
  - `GetStats(prefab)` → `PoolStats` (Created / Active / Inactive). Duman testinde "ateşte Instantiate yok" kontrolü `Created` sayısıyla yapıldı.
  - Play dışında (EditMode testleri) Destroy yerine DestroyImmediate.
- **PooledLifetime:** Süre `OnSpawned`'da ve `OnEnable`'da baştan başlar (sahneye elle konmuş objede de çalışsın), `OnDespawned`'da durur. Bitince `IPoolService.Release`; servis yoksa Destroy. `Progress` (0 → 1) görsellerin solması için; iz ve namlu ışığı ayrı zamanlayıcı tutmaz.
- **Efekt ayarları prefab'larda, havuz/iz ayarları config'te:** Scene step `[Systems]`'i her seferinde yeniden kurduğu için sahnedeki bileşene yazılan ayar kaybolurdu. `CombatFeedbackConfig` (Feedback SO, `Data/Feedback/`): üç prefab referansı, prewarm (iz 16, vuruş 24, namlu ışığı 4), silah listesi, varsayılan iz rengi, min. iz uzunluğu. 14.1'deki `FeedbackConfig` (sarsıntı, hit-stop) ayrı bir SO olabilir ya da buna eklenebilir.
- **Prefab'lar** (`Prefabs/Feedback/`), tek katman (kök = görsel; efektte değiştirilecek model yok, CLAUDE.md'deki Visual ayrımı varlık prefab'ları için):
  - `Tracer`: LineRenderer (dünya uzayı, 2 nokta, kameraya dönük, `M_FxAdditive`) + `PooledLifetime` 0.05 sn + `Tracer` (kalınlık 0.08 m; baş parlak, namlu tarafı %20; ömür boyunca saydamlaşır).
  - `Impact`: ParticleSystem (döngü yok, playOnAwake yok, yayım kapalı → `Emit`; koni 35°, dünya uzayı, ömür 0.15–0.4 sn, boyut ömür boyunca küçülür, Stretch render) + `PooledLifetime` 0.6 sn + `ImpactEffect`. Obje isabet normaline bakar. Profiller: **et** (düşman ve oyuncu: koyu kırmızı, 12 parçacık, `M_FxAlpha`), **kıvılcım** (yüzey ve yapı: sarı-turuncu, 10 parçacık, hızlı, `M_FxAdditive`). Renk/hız/boyut/yerçekimi/materyal `ImpactProfile`'da, ortak ayarlar ParticleSystem'de.
  - `MuzzleFlash`: Point Light (sıcak sarı #FFC46B civarı, menzil 5 m, gölgesiz, Realtime) + `PooledLifetime` 0.05 sn + `MuzzleFlash` (şiddet 6 → 0). Namlu konumunda doğar, oyuncuyu takip etmez (0.05 sn).
  - Değerler GDD'de yok; isteğe göre seçildi. Duman testinde oyun kamerasından (22 m) kıvılcım küçük kaldığı için boyut ve sayı bir kez artırıldı.
- **FX materyalleri** (`Materials/Fx/`): URP'nin varsayılan parçacık materyalinin (`ParticlesUnlit.mat`, Particles/Unlit + yumuşak daire dokusu) kopyası; `_Surface`/`_Blend` yazılıp `BaseShaderGUI.SetupMaterialBlendMode` çağrılır. `M_FxAdditive` (SrcAlpha/One): iz ve kıvılcım. `M_FxAlpha`: kan. Bunun için Editor asmdef'ine `Unity.RenderPipelines.Universal.Editor` eklendi.
- **Ek ışıklar:** URP PC asset'inde Additional Lights = Per Pixel, nesne başına 4. Namlu ışığı bu yüzden görünüyor; ayar değiştirilmedi.
- **`WeaponData.MuzzleVfx` henüz kullanılmıyor:** Görevde istenmedi. Bağlanınca o prefab'ın kendini iade eden bir bileşeni (`PooledLifetime`) olmalı, yoksa havuzdan çıkıp geri dönmez.
- Feedback'teki yer tutucu `AssemblyInfo.cs` silindi.
- **MCP duman testi:** Gerçek zamanlı seri atış `EditorApplication.update`'e bağlanan geçici bir lambdayla yapıldı (`TryFire` + reflection ile `Fire`; 8 sn sonra kendini çıkarır). Kısa efektlerin ekran görüntüsü için `Time.timeScale = 0.005`. Yakın plan için `manage_camera screenshot` + `view_position`/`view_target`. Ekran görüntüleri `Temp/Screenshots/`'a (git dışı) kaydedildi.
- URP, `QualitySettings` üzerinden ilk kez yüklenen `Mobile_RPAsset`'i kendiliğinden yükseltti (`k_AssetVersion` 12 → 13, yeni varsayılan alanlar). İçerik değişikliği değil; olduğu gibi bırakıldı.

**2026-10-15 · 4.1 Düşman temeli ve hedefleme**
- **Yerleşim:** Hepsi Enemies'te; Core'a sadece `EnemyKilledEvent` ve `TargetTypes.IsStructure` (Player dışındaki her tür yapı) eklendi. Geçici `GeneratorPlaceholder` da Enemies'te (sadece Core'a bağlı; Gün 5'te silinecek).
- **EnemyData:**
  - `TargetType?` Unity'de serileşmediği için `_hasPreferredTarget` + `_preferredTarget` tutulur; `PreferredTarget` property'si nullable döner. Ödüller `int`.
  - GDD'de olmayan ayarlar da data'da, varsayılanlarıyla: `spawnDuration` 0.5, `retargetInterval` 0.5, `corpseDuration` 2, `sinkDuration` 1 (GDD'de yok), `burrowDepth` 2.2 m (ölçekle çarpılır), `disengageMargin` 0.3 m, `repathDistance` 0.5 m.
  - Ses/VFX: `attackSound`, `deathSound` (AudioClip), `deathVfx` (GameObject); henüz kullanılmıyor. `Prefab` alanının tipi `Enemy`.
  - **Ölçek:** Spawn'da kökün ölçeği = prefab kökünün ölçeği × `Scale`. 4.2'de GDD §18 ölçekleri Visual'a verildi, `Scale` 1 kaldı.
- **Durum makinesi:** `EnemyStateMachine` (saf) + 4 `internal` durum sınıfı. Unity'ye `IEnemyBody` köprüsüyle dokunur; `Enemy` bunu uygular, testler sahte gövde kullanır. Durum nesneleri düşman başına bir kez oluşur ve havuz döngülerinde yeniden kullanılır. `Begin` saldırı beklemesini sıfırlar ve Spawning'den başlatır.
- **Yerden çıkma:** Ajan açık ama `updatePosition = false`; kök `burrowDepth` derinlikten yüzeye çıkar. Bitince transform ajan konumuna oturur ve `updatePosition = true` olur. `baseOffset` kullanılmadı, çünkü ölçekle nasıl davrandığı belirsiz. Spawning sırasında hasar alınabilir.
- **Hedef seçimi** (`TargetSelector.Select`, statik ve saf):
  1. Aggro yarıçapındaki en yakın oyuncu.
  2. Tercih edilen türün yarıçap içindeki en yakını.
  3. En yakın jeneratör (mesafe sınırı yok).

  Sınırlar dahil, mesafe XZ'de ölçülür.
  - `Tick`: Aralık dolunca değerlendirir, faz korunur. İlk değerlendirme `Random.Range(0, interval)` sonra.
  - Seçili hedef hedeflenemez olursa (öldü veya yok edildi) aralığı beklemeden yeniden değerlendirir. Hiç hedef yoksa her frame değil, aralıkta bakar.
  - Spawning'de de çalışır (düşman yüzeye çıkınca hedef hazır olur), Dead'de durur.
- **Saldırı menzili hedefin yüzeyine ölçülür:** Mesafe, düşmanın merkezinden hedef collider'ının AABB'sine yatay uzaklıktır; collider yoksa hedefin merkezine. Merkeze ölçülseydi 2×2 jeneratörde 1.5 m'lik menzil asla dolmazdı. Izgara yapıları 90°'lik adımlarla döndüğü için AABB tam ayak izidir. Düşman `≤ attackRange`'te saldırıya geçer, `> attackRange + disengageMargin`'de kovalamaya döner.
- **Saldırı hızı:** `AttackCooldown` makinededir ve düşman ölü değilken her durumda azalır. Menzile ilk girişte hemen vurur. Menzilden çıkıp tekrar girmek beklemeyi sıfırlamaz. 0'ın altına inen süre devretmez (en fazla 1 kare kayma; 1 saldırı/sn'de ~%1.6).
- **Hasar:** `MeleeDamage.Calculate` hedef yapıysa `structureDamageMultiplier` uygular. `DamageInfo`:
  - tür `Melee`, kaynak düşman objesi
  - vuruş noktası: hedef AABB'sinin düşmana en yakın noktası (gövde ortası yüksekliğinde)
  - yön yatay
- **Hedef bileşenleri:** `TargetComponentCache` (statik) ITargetable'ı IDamageable ve Collider'a (`GetComponentInParent`) eşler; hedef değişince bir kez çağrılır. `TrackedTarget` mesafe ve vuruş noktasını hesaplar.
- **Gezinme** (`EnemyNavigator`, düz C#):
  - `SetDestination` sadece şu durumlarda çağrılır: hedef değişti, yol yok veya hedef son yol hedefinden `repathDistance`'tan fazla kaydı.
  - Halt: `isStopped` + hız 0 + `updateRotation` kapalı. Attacking'de düşman yerinde, ajanın `angularSpeed`'iyle döner.
  - Ajan kapalıyken metotlar bir şey yapmaz. Warp başarısızsa uyarı loglanır.
- **Ölüm:** `Health.Died` geldiğinde:
  1. Collider ve ajan kapanır, hedef bırakılır.
  2. Dead durumuna geçilir ve `EnemyKilledEvent` gönderilir.
  3. Ceset 2 sn bekler, 1 sn'de `burrowDepth` kadar batar, sonra `IPoolService.Release` (havuz yoksa Destroy).

  Devrilme animasyonu 4.3'te (AnimationDriver).
- **Sahneye elle konmuş düşman** Start'ta kendini spawn eder. Havuzdan gelmediği için ölünce PoolService onu uyarıyla yok eder.
- **GeneratorPlaceholder:**
  - `[Gameplay]/GeneratorPlaceholder`, (0,0,0), Structure layer, Generator tag.
  - BoxCollider 2×2×2, Health (1500, bileşen alanında; GeneratorData Gün 5'te), sarı küp Visual. Ölünce log basar.
  - NavMeshObstacle var ama **carve yok**; ajanlar yerel kaçınmayla etrafından geçer. Carve olsaydı hedef noktası (merkez) NavMesh dışında kalırdı.
- **MCP:** Bu oturumda Claude Code'un MCP araçları başlangıçta bağlanamadı (ECONNREFUSED). Sunucu ayaktaydı; araçlar scratchpad'deki bir betikle `curl` üzerinden doğrudan JSON-RPC olarak çağrıldı (`initialize` → `mcp-session-id` başlığı → `tools/call`). `execute_code` CodeDom (C# 6) ile derliyor.
- **Duman testi:** Play'de geçici bir şablon (çalışma anında oluşturulan EnemyData + Enemy) `PoolService` ile 3 kez çekildi. Doğrulananlar:
  - Yerden çıkma (y −2.12 → 0.08).
  - Kuzey/batı düşmanları jeneratöre, oyuncuya 5 m'deki düşman oyuncuya gitti (1.49 m'de durup 10 hasar/sn).
  - Oyuncu ölünce hedef jeneratöre döndü; 6 m'ye giren düşman oyuncuya döndü.
  - Üçü de jeneratör yüzeyine ≤ 1.5 m'den vurdu.
  - Ölüm: ajan ve collider kapandı, `EnemyKilledEvent` (KilledByPlayer true/false) geldi, ~3 sn sonra y −2.12'de havuza döndü.
  - Havuzdan yeniden çekilen düşmanda yeni örnek oluşmadı; temiz başladı.
  - Konsolda hata veya uyarı yok.

**2026-10-15 · 4.2 Düşman tipleri ve debug spawner**
- **`IEnemySpawner` string id alır** (`Spawn(string enemyId, Vector3)` → `GameObject` / null): Core `EnemyData`'yı göremez; CLAUDE.md'deki "modüller arası veri referansı = string id" kuralı. Görevdeki `Spawn(EnemyData, position)` `EnemySpawner`'da ayrıca public (`Enemy` döner); id'li sürüm buna yönlenir. Waves (5.2) `NightData`'daki `EnemyData.Id`'yi verecek.
- **Tip listesi `EnemySpawnerConfig`'te** (`Data/Enemies/`): satır başına `EnemyData` + ön ısıtma sayısı, NavMesh'e yerleştirme mesafesi 3 m. Listede olmayan id doğurulamaz (uyarı). Id eşlemesi saf `EnemyTypeLookup` (ordinal; boş/tekrar eden id atlanır ve uyarılır). Üretici config'e eksik tipleri ekler; Gün 12'deki tipler `EnemyDefinitions`'a eklenince kendiliğinden gelir.
- **Ön ısıtma** (GDD'de yok): Yürüyen 40, Koşucu 15, Tank 5 → oyun başında 60 örnek. Aşılırsa havuz prefab başına bir kez uyarır; dalgalar (5.2/5.3) gelince gerekirse config'ten artır.
- **Yerleştirme:** `NavMesh.SamplePosition(nokta, 3 m, AllAreas)`; bulunamazsa uyarı + null. Yön rastgele (Y ekseni); düşman yürümeye başlayınca yola döner. Havuz servisi yoksa Instantiate'e düşülmez, uyarı + null. Servis `Start`'tan önce çağrılsa da havuzu tembel alır.
- **Ölçek Visual'da, `EnemyData.Scale` = 1** (4.1'deki ⚠️ notu çözüldü): Gövde ölçeği `Visual/Body` kapsülünde; kökteki CapsuleCollider ve NavMeshAgent yarıçap/boyu gövdeye göre üretici tarafından yazılır. Kullanıcı modeli değiştirirse ölçü kaybolmaz; `Scale` runtime çarpanı olarak boşta kalır.
  - Koşucu "ince kapsül (×0.8)": sadece genişlik (0.8, 1, 0.8) → yarıçap 0.4, boy 2.
  - Tank "büyük kapsül (×1.6)": eşit (1.6) → yarıçap 0.8, boy 3.2.
  - Gömülme derinliği gövde boyunun 1.1 katı (Yürüyen/Koşucu 2.2, Tank 3.52); Tank yerden çıkarken toprağın üstünde görünmesin diye.
- **Tank verisi:** aggro 3 m, tercih Barikat (15 m), yapılara ×2, `knockbackResistance` 1 (geri itilmez). Diğerleri aggro 6, direnç 0.
- **Prefab** (`Prefabs/Enemies/<Tip>.prefab`): kök Enemy layer + **Enemy tag**; NavMeshAgent (açısal hız 540°/sn, ivme 20 m/sn², durma mesafesi 0; hız spawn'da veriden), CapsuleCollider (trigger değil; hitscan trigger'ları yok sayar), Health, Enemy. `Visual/Body` collider'sız kapsül, GDD §18 materyali. Rigidbody yok (görevde yok). Ajan değerleri GDD'de yok: varsayılan 120°/sn ve 8 m/sn² Koşucu'yu köşelerde savuruyordu. Prefab varsa sadece boş `Enemy._data` ve `EnemyData._prefab` doldurulur.
- **DebugSpawner** (DebugTools, `[Systems]/DebugSpawner`):
  - Gövdesi `#if UNITY_EDITOR || DEVELOPMENT_BUILD`; serileşen alanlar (veriler, `_edgeCount` 20, `_edgeZones`) her build'de var (serileşme düzeni değişmesin).
  - Kısayollar kendi `InputAction`'larıyla (F1–F4); oyun girdisi olmadığı için `IInputReader`'a eklenmedi. Bunun için DebugTools asmdef'ine `Unity.InputSystem` eklendi.
  - İmleç noktası: `Mouse.current` → `ICameraService.Camera` ışını → zemin/engel (`NavMeshBakeMask`), değmezse y = 0 düzlemi. Engelin üstüne tıklanırsa SamplePosition en yakın zemine indirir.
  - F4: 4 `SpawnZone`'dan biri rastgele, 20 nokta bölgenin dairesi içinde düzgün rastgele. Hepsi aynı anda doğar.
  - Sol üst OnGUI kutusu; metin Awake'te bir kez kurulur (ad `EnemyData.DisplayName`'den).
  - Her basışta `[Debug] ...` logu; imleç harita dışındaysa `EnemySpawner` uyarısı.
- **Duman testi** (MCP; Unity odakta olmadığı için kısayol metotları reflection ile, imleç `InputState.Change(mouse.position, …)` ile):
  - Ön ısıtma 40/15/5; 20 + 4 + 4 doğuşa rağmen havuzda yeni örnek oluşmadı (Created sabit).
  - F4 → 20 Yürüyen Doğu bölgesinde; F1–F3 imleç noktasına (−8, 6), (9, 9), (−12, −10).
  - Engel arkasındaki Tank'ın düz yolu engelle kapalı, yolu 4 köşeli; duvar arkasındaki Yürüyen dolaşıp jeneratöre ulaştı. Koşucu 6 m'ye giren oyuncuya döndü.
  - Saldırılar: kalabalık jeneratörü (1500) ~10 sn'de, Koşucu oyuncuyu öldürdü. Tank vuruşu 70 (35 × 2), 1.67 sn arayla.
  - İki tabanca atışı Koşucu'yu öldürdü (35 → 15 → 0); `EnemyKilledEvent` (12 para, 1 XP, KilledByPlayer true); ~3 sn sonra havuza döndü.
  - ⚠️ `execute_code` ile `Application.runInBackground = true` editörde **`ProjectSettings.asset`'e yazılıyor** (PlayerSettings). Testten sonra `PlayerSettings.runInBackground = false` + `SaveAssets` ile geri alındı. Sonraki testlerde de geri al.

**2026-10-15 · 4.3 Vuruş hissi ve AnimationDriver**
- **Yerleşim:** `AnimationDriver` ve `HitFlash` Core'da, çünkü hem Enemies hem Player kullanıyor. `VisualRoot.Name` ("Visual") tek kaynak; `PlayerGenerator.VisualName` artık ona bağlı.
- **AnimationDriver** (kökte):
  - `_visual` alanı üretici tarafından bağlanır; boşsa `Visual` çocuğu aranır.
  - **Ya Animator ya yedek.** Animator sadece controller'ı atanmışsa kullanılır. Animator'u olup controller'ı olmayan model yedekle oynar (Gün 16'daki "controller'sız prefab" uyarısıyla uyumlu).
  - Animator modunda controller'da olmayan parametre sessizce atlanır (`AnimatorParameters`, liste controller başına bir kez okunur).
  - `Speed` gerçek yatay hızdır (m/sn): oyuncuda `CharacterController.velocity`, düşmanda `agent.velocity`. Blend tree eşikleri m/sn ile kurulmalı (Yürüyen 2.2, Koşucu 5, oyuncu 6).
  - `SetSpeed`, `SetDashing`, `TriggerFire`, `TriggerReload` sadece Animator'a yazılır; görevde yedekleri istenmediği için prosedürel karşılıkları yok.
- **Prosedürel yedek** (`ProceduralPose`, saf). Visual'ın ebeveyn uzayındaki dinlenme pozuna eklenir; dönüş Visual'ın orijini (ayaklar) etrafındadır. Değerler GDD'de yok, `AnimationDriver._procedural`'da ayarlanır:
  - Öne atılma: 0.35 m, 0.2 sn, sinüs (git-gel). Saldırı denemesinde oynar.
  - Sarsılma: yana 0.08 m, 25 Hz, 0.15 sn, sönümlü.
  - Devrilme: 90°, 0.4 sn, t² (düşüş gibi hızlanır). Yön her ölümde rastgele. `ResetPose`'a kadar yatık kalır.
  - Devrilirken Visual `toppleLift` kadar kalkar. Üretici bunu gövde yarıçapına eşitler (Yürüyen 0.5, Koşucu 0.4, Tank 0.8, oyuncu 0.5); yatan kapsül yarı yarıya yere gömülmesin diye.
  - Yeni vuruş/saldırı oynayanı baştan başlatır; atılma, sarsılma ve devrilme üst üste binebilir.
  - Havuzdan çıkışta (`IPoolable.OnSpawned`) poz ve Animator (`Rebind`) sıfırlanır. **Oyuncu yeniden doğarken (6.1) `AnimationDriver.ResetPose()` ve `HitInvulnerability` sıfırlanmalı.**
- **HitFlash:**
  - Visual altındaki renderer'larda `_BaseColor` ve/veya `_EmissionColor` MaterialPropertyBlock ile yazılır. İkisi de olmayan renderer listeye girmez. Bitince `SetPropertyBlock(null)`; materyale dokunulmaz.
  - Yer tutucu materyallerde `_EMISSION` kapalı olduğu için flaşı `_BaseColor` sağlar: ışıklı beyaz/kırmızı, parlama değil. ⚠️ Dokulu modellerde (`_BaseMap`) beyaz `_BaseColor` dokuyu değiştirmez; flaş görünmeyebilir (16.1'de bakılacak).
  - Renk ve süre prefab'da. Düşman: beyaz, 0.08 sn (GDD §7). Oyuncu: kırmızı (1, 0.12, 0.12), 0.15 sn (GDD'de sadece "kırmızı" yazıyor; dokunulmazlıktan kısa seçildi).
  - Yeni flaş süreyi baştan başlatır. Öldüren vuruş da flaşlar.
- **Geri itilme:**
  - Mesafe ve süre `EnemyData`'da: `_knockbackDistance` 0.3 m (GDD §7), `_knockbackDuration` 0.1 sn (GDD'de yok). Etkili mesafe = mesafe × (1 − `knockbackResistance`); Tank 1 → itilmez.
  - Yön `DamageInfo.HitDirection`'ın yatay kısmı; yatayda sıfırsa itilmez.
  - Her kare `agent.Move` + `agent.velocity = 0`: ajan itmeye karşı yürümez; itme bitince kendi ivmesiyle yeniden hızlanır.
  - **Yeni vuruş itmeyi baştan başlatır, üst üste eklemez.** Pompalının 8 saçması 0.3 m iter, 2.4 m değil. Tüfek serisi (10 atış/sn) düşmanı sürekli ~3 m/sn geride tutar; düşman ~0.4 sn'de öldüğü için dengeyi bozmuyor. His testinde fazla gelirse süre veya mesafe `EnemyData`'dan ayarlanır.
  - Yerden çıkarken (Spawning) ve ölüyken itilmez. Öldüren vuruş itmez; devrilme `Enemy`'de.
- **EnemyHitReaction** ayrı bileşen. `Enemy` zaten ~220 satır; vuruş tepkisi (flaş, `TriggerHit`, itme) oraya taşındı. `Enemy` sadece saldırı/hız/ölüm çağrılarını yapar.
- **Can barı** (`EnemyHealthBar`):
  - Kökün çocuğu `HealthBar`, Visual dışında; flaş ve devrilme etkilemez. Başın 0.35 m üstünde.
  - Arka plan + dolgu quad'ları, URP/Unlit (`Materials/HealthBar/`): koyu gri / kırmızı. Gölgesiz, collider'sız.
  - Boyutlar: dolgu 0.9 × 0.1 m, kenar 0.02 m (GDD'de yok; oyun kamerasından ~70 px).
  - Billboard kameraya paralel (rotasyon = kamera rotasyonu). Kamera `ICameraService`'ten ilk gösterimde alınır.
  - Görünürlük renderer'ların `enabled`'ı ile; obje kapatılmaz, abonelik kopmasın diye. Her hasarda 3 sn baştan başlar.
  - **Ölünce hemen gizlenir** (görev metninde yoktu; cesedin üstünde boş bar kalmasın diye). Havuza dönüş ve havuzdan çıkışta da gizlenir.
- **Oyuncu dokunulmazlığı** (`HitInvulnerability`, saf): zaman damgasıyla (`Time.time`) çalışır, Update sırasına bağlı değil. `PlayerCharacter.IsInvulnerable` = atılıyor VEYA pencere içinde; `IDamageFilter` bunu kullanır. Engellenen hasar olay, flaş ve animasyon üretmez.
- **PlayerAnimation:** `Speed` ve `IsDashing` her kare. `TriggerFire`, `WeaponFiredEvent.Source == oyuncu` ise. `TriggerReload`, `ReloadStartedEvent`'te (olayda kaynak yok; şimdilik sadece oyuncu dolduruyor). Hasar alınca `TriggerHit`, ölünce `TriggerDie`. Öldüren vuruşta sadece Die.
- **`RequireComponent(AnimationDriver)` kullanılmadı** (`Enemy`'de denendi, geri alındı). Unity, var olan prefab'lara gerekli bileşeni yüklerken kendiliğinden ve varsayılan ayarlarla ekliyor; üretici ise ayarları sadece ilk eklemede yazıyor. Bu yüzden `Enemy` sürücüyü null kontrolüyle kullanıyor. `PlayerAnimation` RequireComponent ile istiyor, ama üretici sürücüyü ondan önce ekliyor. ⚠️ Yeni bir `RequireComponent` eklerken bu tuzağa dikkat et.
- **Var olan prefab'lar** (`EditPrefab`): oyuncuya 5, her düşmana 6 eksik bileşen/referans eklendi. İkinci Rebuild All'da 0 değişiklik (idempotent).
- **Duman testi** (MCP; `Time.timeScale` 0.02–0.1 ile kısa efektler yakalandı, saldırı ve ölüm `EditorApplication.update` izleyicisiyle kare kare kaydedildi):
  - Yürüyen vurulunca flaş başladı ve bitince blok temizlendi; sarsılma bitince Visual dinlenme pozuna döndü.
  - Hasar yönünde tam 0.300 m itildi.
  - Can barı göründü (40/60 → 0.67), kamerayla aynı açıda (50°, 45°), 3 sn sonra gizlendi.
  - Saldırıda Visual 0.35 m öne atıldı. Ölünce devrildi: 0.2 sn'de ~23°, 0.5 sn'de yatay, Koşucu'da 0.4 m kalkık. Havuza dönen örnekte poz, flaş ve bar temiz.
  - Tank: flaş ve sarsılma var, itilme yok.
  - Oyuncu: 10 `PlayerDamagedEvent` + 1 `PlayerDiedEvent`, devrildi, Muzzle kökte kaldı. Diriltilince (`SetMax` + `ResetPose`) ilk vuruşta kırmızı flaş; aynı karedeki 2. vuruş engellendi; 0.3 sn sonra hasar yeniden işledi.
  - Konsolda hata/uyarı yok. `runInBackground` geri alındı; ProjectSettings'te değişiklik yok.

## Bilinen hatalar / TODO

<!-- - [ ] Açıklama (görev numarası) -->
- [x] ~~Proje henüz git deposu değil.~~ Depo var (tek commit: "project setup"); sonraki görevlerin dosyalarının çoğu henüz commit'lenmedi (`git status`). (1.1)
- [ ] Şablondan kalanlar duruyor: `Assets/InputSystem_Actions.inputactions` (kullanılmayacak, girdi kodda), `Assets/Scenes/SampleScene.unity`. İstenirse silinebilir. SampleScene Build Settings'te hâlâ 0. sırada. (1.1, 1.3)
  - ⚠️ `InputSystem_Actions` **project-wide actions** olarak kayıtlı (Project Settings → Input System Package). Runtime'da kendiliğinden açılır. Bizim action'larla çakışmaz ama boşuna çalışır. Silmeden önce oradaki kaydı kaldır. UI Toolkit'in runtime gezinmesi bu asset'in UI haritasını kullanıyor olabilir; karar UI görevlerinde (Gün 10) verilmeli. (2.1)
- [ ] Üs alanı yarıçapı `MapLayoutConfig` ile ileride gelecek BaseArea config'inde iki kez tutulacak. Gün 5'te tek kaynağa bağla. (1.3)
- [x] ~~Ateş görünmez; ıskalayan saçmanın izi için olay yok.~~ 3.3'te `ShotTracedEvent` ve efektlerle çözüldü. (3.2)
- [ ] `WeaponData.MuzzleVfx` (opsiyonel namlu efekti prefab'ı) hiçbir yerde kullanılmıyor; sadece varsayılan namlu ışığı var. Bağlanınca prefab'da `PooledLifetime` şart. (3.3)
- [ ] Efekt değerleri (iz kalınlığı, parçacık sayısı/boyutu, ışık şiddeti) sadece ekran görüntüsüyle ayarlandı; gerçek hızda his testi kullanıcıda. Prefab'lardan ayarlanır. (3.3)
- [ ] İlk MCP duman testinde Play'e girer girmez bir tabanca mermisi harcandı (12 → 11, isabet yok). Sonra `IsFireHeld` ve fare tuşu false'tu; ikinci Play'de tekrar olmadı (12/12). Muhtemelen o sırada oyuna ulaşan bir fare tıklaması. Elle Play'e basınca tekrar ederse incele. (3.2)
- [ ] Test kuklası konumları `MapLayoutConfig`'ten bağımsız. Üs yarıçapı veya engel halkası değişirse kuklalar üsse ya da engel içine düşebilir; `TestDummiesConfig`'ten elle taşı. (3.2)
- [ ] `GeneratorPlaceholder` ve `GeneratorPlaceholderSceneStep` Gün 5'te (5.1) silinecek. Gerçek jeneratörde carve kullanılırsa `SetDestination(merkez)` NavMesh dışını hedefler; yol kısmi (`PathPartial`) çıkabilir ve kural 4 (Gün 8) ile karışabilir. O görevde dene. (4.1)
- [ ] Düşman ve oyuncu fiziksel olarak çarpışmaz (NavMeshAgent collider'lara bakmaz). Canlı oyuncuda sorun yok, düşman 1.5 m'de duruyor. Ama yoluna çıkan **ölü** oyuncunun CharacterController'ı düşman collider'ından itiliyor (duman testinde ~2 m kaydı). 6.1'de yeniden doğma gelince bak. (4.1)
- [ ] `TargetComponentCache` yok edilen hedeflerin kayıtlarını silmiyor; sadece Play başında temizleniyor. Barikatlar havuzlanırsa sorun yok. Yıkılınca Destroy ediliyorlarsa (Gün 8) kayıtlar run boyunca birikir. (4.1)
- [ ] NavMesh tek ajan tipiyle (Humanoid, yarıçap 0.5) bake ediliyor; Tank'ın ajan yarıçapı 0.8. Yol bulma 0.5'e göre olduğu için Tank engel köşelerine ~0.3 m girebilir (sadece görsel; collider'lar birbirini itmez). Rahatsız ederse 2. ajan tipi + ikinci NavMeshSurface gerekir. (4.2)
- [ ] Düşmanlarda Rigidbody yok. Diken tuzağı (9.3) trigger ile çalışacaksa tuzağa ya da düşmana kinematik Rigidbody gerekir. (4.2)
- [ ] F4'teki 20 Yürüyen aynı anda doğuyor ve ilk saniyelerde yerel kaçınmayla birbirini itiyor; dalga sistemi (5.2) aralıklı doğuracak. (4.2)
- [ ] Oyuncu ölünce Visual devrik kalıyor, dokunulmazlık penceresi açık kalabilir. Yeniden doğmada (6.1) `AnimationDriver.ResetPose()` çağır ve `PlayerCharacter`'daki `HitInvulnerability`'yi sıfırla (şu an dışarıya açık bir sıfırlama metodu yok; 6.1'de ekle). (4.3)
- [ ] Hit flash `_BaseColor` ile çalışıyor; dokulu modellerde (beyaz `_BaseColor` = değişiklik yok) görünmeyebilir. Emisyonu açık materyalde parlar. 16.1'de farklı shader'larla doğrula. (4.3)
- [ ] `ReloadStartedEvent`'te kaynak yok; `PlayerAnimation` her doldurmayı oyuncunun sayıyor. Taretler doldurma olayı gönderirse (Gün 9) olaya kaynak ekle. (4.3)
- [ ] `Enemy.cs` ~225 satır (sınır ~200). Yeni davranış eklenirken (Gün 8/12) bölünmeli; aday: ölüm/ödül kısmı. (4.3)
- [ ] `ZombieLast.slnx` `.gitignore`'da ama depoda takipli; Unity her derlemede proje sırasını değiştiriyor. İstenirse `git rm --cached ZombieLast.slnx` ile takipten çıkarılabilir. (4.1)
