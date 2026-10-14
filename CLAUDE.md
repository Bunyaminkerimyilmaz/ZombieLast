# Last Haven: Day & Night — Agent Talimatları

Bu dosya, bu Unity projesinde çalışan AI agent için kalıcı kurallardır. Her görevde geçerlidir.

## Proje

3D, izometrik/tepeden kameralı zombi hayatta kalma + üs savunma oyunu. Gündüz topla ve inşa et, gece dalgalara karşı jeneratörü koru.

| Dosya | Amaç |
|---|---|
| `Docs/GDD.md` | Tasarımın tek doğru kaynağı. Tüm sayılar ve kurallar buradan alınır. Prompt'larda "GDD §7" gibi bölüm numarasıyla anılır. |
| `Docs/PROGRESS.md` | İlerleme günlüğü. **Her görevin başında oku, sonunda güncelle.** |
| `Docs/BUILD_PLAN.md` | Kullanıcının görev listesi. Sadece sana verilen görevi yap; ileriki görevlerin işini önceden yapma. |

## Teknoloji

| Alan | Seçim | Not |
|---|---|---|
| Motor | Unity 6 LTS (6000.x) | Unity 6 API: `rb.linearVelocity`, `FindObjectsByType` / `FindAnyObjectByType` (sadece editör araçlarında) |
| Editör kontrolü | MCP for Unity (CoplayDev) | Agent editöre MCP araçlarıyla bağlanır (aşağıdaki bölüm) |
| Render | URP | Post-process: URP Volume |
| Girdi | Input System paketi | Action'lar **kodda** oluşturulur (`InputReader`). `.inputactions` asset'i kullanılmaz. |
| Yapay zekâ | AI Navigation paketi | `using Unity.AI.Navigation;` → `NavMeshSurface`. Düşmanlar `NavMeshAgent`. Barikatlar `NavMeshObstacle` (carve). |
| Kamera | Cinemachine 3.x | `using Unity.Cinemachine;` → `CinemachineCamera`, `CinemachineFollow`, `CinemachineImpulseSource`. **Cinemachine 2 API'si (`CinemachineVirtualCamera`) kullanma.** |
| Arayüz | UI Toolkit | UXML + USS + TSS dosyaları `Assets/_Project/UI/` altında. uGUI/Canvas kullanma. |
| Test | Unity Test Framework | EditMode testleri |
| Havuz | `UnityEngine.Pool.ObjectPool<T>` | `IPoolService` üzerinden |

Asmdef'lerde paket referanslarını unutma: `Unity.InputSystem`, `Unity.AI.Navigation`, `Unity.Cinemachine`, `Unity.RenderPipelines.Universal.Runtime`, `Unity.RenderPipelines.Core.Runtime`.

## 🔌 Unity MCP ile çalışma (en önemli bölüm)

Bu projede **MCP for Unity** bağlıdır. Editörü MCP araçlarıyla görebilir ve kontrol edebilirsin:
- konsolu okuma
- asset refresh ve derleme
- menü çalıştırma
- hiyerarşi ve component inceleme
- Play Mode
- test çalıştırma

Oturumun başında hangi Unity araçlarının mevcut olduğuna bak; araç adları sürüme göre değişebilir.

**Temel ilke: MCP senin gözün ve elin; kalıcı içeriğin kaynağı koddur.**
Sahneyi, prefab'ları ve data'yı MCP ile tek tek tıklar gibi kurma; ekip 5 kişi ve sahne dosyaları git'te kolay çakışır. Kalıcı her şeyi **üreten editör araçlarını** yaz, sonra onları MCP ile **kendin çalıştır**. Böylece her şey tek tıkla yeniden üretilebilir kalır.

### Doğrulama döngüsü (her görevde, rapor vermeden önce)

1. Kodu yaz, asset refresh / derlemeyi tetikle ve derleme bitene kadar bekle. Domain reload sırasında MCP bağlantısı kısa süre kopabilir; birkaç saniye sonra tekrar dene.
2. Konsolu oku. `error CS` veya exception varsa düzelt ve tekrarla. **Kırmızı hata varken görevi bitmiş sayma.**
3. Üreticiler değiştiyse `Tools/Last Haven/Rebuild All` menüsünü çalıştır ve sahneyi kaydet.
4. EditMode testlerini çalıştır; hepsi yeşil olmalı.
5. **Duman testi:**
   - `Game.unity` açıkken Play Mode'a gir ve ~10 sn bekle.
   - Konsolu oku.
   - Gerekirse hiyerarşi ve component değerleriyle beklenen durumu doğrula (ör. faz, düşman sayısı, oyuncunun canı).
   - Play Mode'dan çık.
6. Bulduğun sorunları düzelt ve döngüyü tekrarla.

### Kurallar

1. **Play Mode'dayken kod veya asset değiştirme.** Önce Play Mode'dan çık.
2. **`.unity`, `.prefab`, `.asset`, `.meta`, `.controller`, `.mixer` dosyalarını metin olarak elle yazma.** Bunları üretici araçlar oluşturur. Metin tabanlı kaynakları doğrudan yazabilirsin: `.cs`, `.asmdef`, `.uxml`, `.uss`, `.tss`, `.json`, `.md`, `.gitignore`.
3. **MCP ile doğrudan sahne düzenlemesi** sadece geçici deneme ve hata ayıklama içindir. Kalıcı olacaksa üreticiye taşı, deneme değişikliğini geri al. `[Environment]` köküne asla dokunma.
4. Oyunun **hissini** (kontroller, zorluk, eğlence) sen test edemezsin. Kullanıcıya net test adımları ver.
5. MCP araçları görünmüyorsa kullanıcıya söyle (Unity açık mı, MCP penceresi bağlı mı). Geçici olarak Unity log dosyasından oku:
   - macOS: `~/Library/Logs/Unity/Editor.log`
   - Windows: `%LOCALAPPDATA%\Unity\Editor\Editor.log`

### Üretici mimarisi (`Assets/_Project/Editor/`)

- Her modülün kendi dosyası olur: `Editor/Generators/<Modül>Generator.cs`. Hepsi `IGenerationStep` arayüzünü uygular (`Order`, `Name`, `GenerateData()`, `GeneratePrefabs()`).
- Sahne kurulumu da modül bazlıdır: `Editor/SceneBuild/<Modül>SceneStep.cs` → `ISceneBuildStep` (`Order`, `Build(SceneBuildContext)`).
- Ana menü adımları `TypeCache.GetTypesDerivedFrom<>()` ile bulup sırayla çalıştırır. Yeni modül eklemek merkezi dosyaya dokunmayı gerektirmez (ekipte merge çakışması olmasın diye).
- Menüler: `Tools/Last Haven/Setup Project`, `Generate Default Data`, `Generate Prefabs`, `Build Game Scene`, `Rebuild All` (dördünü sırayla çalıştırır), `Validate Project`.
- **Üreticiler idempotent olmalı:**
  - Data ve prefab: **varsa dokunma, yoksa oluştur.** Kullanıcının inspector ayarları ve eklediği modeller korunur. Sadece eksik referansları doldurabilirsin. Üzerine yazmak için ayrı `Force Regenerate ...` menüsü olur; açıkça istenmedikçe kullanılmaz.
  - Sahne: sadece kendi kök objelerini (`[Systems]`, `[Gameplay]`) silip yeniden kurar. **`[Environment]` köküne asla dokunmaz** (manuel çevre/art için). Yoksa boş olarak oluşturur.
- Yeni bir sistem eklediğinde ilgili üreticileri de güncelle. Kimse inspector'da elle sürükle-bırak yapmak zorunda kalmasın. Referansları üretici içinde `AssetDatabase.LoadAssetAtPath` + `SerializedObject` ile bağla.

### Proje sabitleri

- Layer ve tag'ler `Core/GameLayers.cs` ve `Core/GameTags.cs`'te tanımlıdır. `Setup Project` menüsü bunları TagManager'a sabit indekslerle ekler:
  - `Ground=6, Player=7, Enemy=8, Structure=9, Pickup=10, Obstacle=11, EnemyProjectile=12, Interactable=13`
- NavMesh oyun başında runtime'da bake edilir:
  - `NavMeshBaker` bileşeni, `[DefaultExecutionOrder(-1000)]`
  - Physics Colliders; sadece Ground ve Obstacle layer'ları
  - Editörde NavMesh asset yönetimi yok.

## Klasör yapısı

```
Assets/_Project/
  Scripts/<Modül>/          Her modül kendi asmdef'i ile: LastHaven.<Modül>
  Editor/                   LastHaven.Editor (sadece Editor platformu)
    Generators/  SceneBuild/  Tools/
  Tests/EditMode/           LastHaven.Tests.EditMode
  Data/<Kategori>/          ScriptableObject asset'leri (üretici oluşturur)
  Prefabs/<Kategori>/       Üretilen prefab'lar
  Scenes/                   MainMenu.unity, Game.unity
  UI/                       UXML, USS, TSS, PanelSettings
  Materials/Placeholder/    Renk kodlu placeholder materyaller
  Art/  Audio/              Manuel asset'ler — agent içeriğine dokunmaz
```

## Modüller ve bağımlılıklar

| Modül | İçerik | Referans alabilir |
|---|---|---|
| **Core** | EventBus, ServiceLocator, tüm servis arayüzleri, olaylar, Stats, DamageInfo, Pool, TargetRegistry, AnimationDriver, sabitler | — |
| **Combat** | WeaponData, WeaponRuntime, HitscanShooter | Core |
| **Player** | InputReader, hareket, nişan, silah kontrolü, kamera, fener, respawn | Core, Combat |
| **Enemies** | EnemyData, Enemy, durum makinesi, hedefleme, saldırılar, boss, NavMeshBaker | Core |
| **Waves** | SpawnZone, NightData, WaveConfig, WaveSpawner, gece olayları, gündüz gezginleri | Core, Enemies |
| **Building** | Jeneratör, BaseArea, ızgara, inşa modu, barikat, taret, tuzak, onarım | Core, Combat |
| **Economy** | Cüzdan, jeton, kombo, market, ganimet sandıkları | Core |
| **GameFlow** | Durum makinesi (gün/gece), GameConfig, istatistik, ışık | Core |
| **Progression** | XP, seviye, perk'ler | Core |
| **UI, Audio, Feedback, DebugTools** | Yaprak modüller | Hepsi (kimse bunları referans almaz) |
| **Editor, Tests** | Üreticiler, testler | Hepsi |

Tabloda olmayan bir bağımlılık gerekiyorsa **önce Core'a arayüz veya olay koyarak çöz.** Modüller arası veri referansı gerekiyorsa string `id` kullan (ör. market → silah açma: `WeaponUnlockRequestedEvent { WeaponId }`).

## Mimari kurallar (SOLID)

- **İletişim:**
  - Olaylar: `EventBus<T>.Raise(...)`, `Subscribe`, `Unsubscribe`. Olaylar `readonly struct` ve `IEvent` uygular. Modüller arası olaylar Core'da, alan bazlı ayrı dosyalarda tutulur (`Core/Events/CombatEvents.cs`, `EnemyEvents.cs`, `EconomyEvents.cs`...). Böylece aynı dosyada çakışma olmaz.
  - Servisler: `ServiceLocator.Register<IWallet>(this)` **Awake**'te, `ServiceLocator.Get<IWallet>()` **Start**'ta veya sonrasında çağrılır. Arayüzler Core'dadır.
- **Veri:** Ayarlanabilir tüm sayılar ScriptableObject'tedir (`XxxData`, `XxxConfig`). Kodda magic number olmaz. Varsayılan değerler GDD'den gelir.
- **Mantık ile Unity'yi ayır:** Hesaplama ve durum saf C# sınıflarında (ör. `HealthModel`, `WaveRunner`, `ComboTracker`) durur. MonoBehaviour sadece Unity'ye köprü olur. Saf sınıflar EditMode testi alır.
- **Tek sorumluluk:** ~200 satırı geçen sınıfı böl.
- **Havuzlama:** Mermi izi, düşman, jeton, VFX, hasar yazısı ve ses kaynakları `IPoolService` üzerinden kullanılır. Oynanış sırasında `Instantiate`/`Destroy` yapılmaz.
- **Statik durum:** Static alanlar `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` ile sıfırlanır.
- **Abonelik:** `OnEnable`'da abone ol, `OnDisable`'da çık.
- **Yasaklar:**
  - Runtime'da `GameObject.Find` / `FindObjectOfType`
  - `Update` içinde `GetComponent`, LINQ veya string birleştirme
  - Public alan → yerine `[SerializeField] private` + property
  - Singleton `Instance` → yerine ServiceLocator
  - `SendMessage`
  - `tag == "..."` → yerine `CompareTag(GameTags.X)`
- **Görsel ayrımı:** Her varlık prefab'ı iki katmanlıdır:
  - Kök: mantık, collider, NavMeshAgent.
  - `Visual` alt objesi: sadece mesh ve animasyon.

  Mantık kodu `Visual` içine doğrudan bağımlı olmaz. Animasyonlar `AnimationDriver` üzerinden yönetilir: Animator varsa parametreleri ayarlar, yoksa prosedürel yedek animasyonu oynatır. Namlu gibi bağlantı noktaları (`Muzzle`) `Visual` dışında durur. Böylece kullanıcı sonradan modeli değiştirir ve kod bozulmaz.
- **Ses/VFX/ikon referansları** data'da opsiyonel alanlardır. Null ise sessizce atla, hata verme.
- **UI metinleri** Türkçedir ve UXML'de veya SO'ların `displayName` / `description` alanlarında durur. Kodda dağınık string olmaz.

## Kod stili

- Namespace `LastHaven.<Modül>`. İsimler İngilizce, yorumlar kısa ve Türkçe olabilir. Public API'lerde `/// <summary>` kullanılır.
- Private alanlar `_camelCase`, metot ve property'ler `PascalCase`, arayüzler `IXxx`, olaylar `XxxEvent`, SO'lar `XxxData` / `XxxConfig`.
- Inspector alanlarında `[SerializeField] private` + `[Header]`, `[Tooltip]`, `[Min]` kullanılır.
- Zamanlayıcı mantığı saf C#'ta durur. Coroutine veya `Awaitable` sadece köprü olarak kullanılır.
- Polimorfik listeler için `[SerializeReference]` kullanılır.
- **Kültür tuzağı (tr-TR):** Türkçe Windows'ta büyük/küçük harf dönüşümü bozulur (`"ID".ToLower()` → `"ıd"`, `"id".ToUpper()` → `"İD"`).
  - String id karşılaştırmalarında `StringComparison.Ordinal` kullan (ör. `string.Equals(a, b, StringComparison.Ordinal)`).
  - Dönüşümlerde `ToLowerInvariant()` / `ToUpperInvariant()` kullan. Kültüre bağlı `ToLower()` / `ToUpper()` kullanma.

## Görev protokolü (her prompt'ta uygula)

1. `Docs/PROGRESS.md`'yi ve GDD'nin ilgili bölümünü oku. Mevcut kodu incele; var olanı yeniden yazma, genişlet.
2. **Sadece istenen görevi yap.** Kapsamı genişletme. Belirsizlikte en basit makul seçimi yap ve PROGRESS.md'nin "Kararlar" bölümüne yaz. Sadece gerçekten engelleyiciyse soru sor.
3. Gerekirse üretici editör araçlarını güncelle.
4. Saf mantık için EditMode testi yaz veya güncelle.
5. **Doğrulama döngüsünü** MCP ile uygula: derleme, konsol, Rebuild All, testler, duman testi.
6. Bitince bu formatta özet ver:
   - **Değişen / yeni dosyalar**
   - **Yaptığım doğrulamalar:** derleme, testler, duman testi, her birinin sonucu
   - **Senin test etmen gerekenler → beklenen sonuç:** his, kontroller, görsel
   - **Bilinen eksikler**
7. `Docs/PROGRESS.md`'yi güncelle: tamamlanan görev, kararlar, yeni menüler, bilinen hatalar.
8. Kullanıcı istemedikçe git commit atma.
