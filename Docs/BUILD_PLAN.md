# Last Haven — Yapım Planı (agent ile adım adım)

Bu plan, oyunu VS Code'daki Claude agent'ına **tek tek prompt göndererek** yaptırman için hazırlandı.

- **Kapsam:** 16 çalışma günü × 3 görev = 48 prompt.
- **Asset'ler** (model, animasyon, ses) en sonda elle eklenir. O zamana kadar her şey renkli primitive şekillerle çalışır.
- **"Gün"** bir çalışma oturumu demek (~3–5 saat). Hızına göre iki günü birleştirebilir ya da bir günü bölebilirsin.

---

## Nasıl çalışıyoruz?

### VS Code + Claude Code + Unity MCP

Agent, **MCP for Unity** üzerinden açık olan Unity Editor'e bağlanır. Böylece kendisi:
- konsolu okur ve derleme hatalarını düzeltir,
- `Tools → Last Haven → ...` menülerini çalıştırır,
- testleri koşturur,
- Play Mode'a girip kısa bir duman testi yapar ve hiyerarşiyi inceler.

Sana kalan iş: oyunun **hissini** test etmek (kontroller, zorluk, eğlence) ve commit atmak.

### Neden yine de "editör araçları"?

MCP ile agent sahneyi tek tek tıklar gibi de kurabilir, ama kalıcı içerik için bunu yapmıyoruz:
- 5 kişilik ekipte sahne ve prefab dosyaları git'te kolay çakışır. Üretici araçlar sayesinde çakışınca tek tıkla yeniden üretilir.
- Her şey kodda olduğu için tekrarlanabilir ve gözden geçirilebilir.

Kısacası: **kalıcı içerik = kod + üretici araçlar; MCP = agent'ın gözü ve eli.** Ayrıntılı kurallar `CLAUDE.md`'de.

### Her görevin döngüsü

1. **Unity açık** olsun; MCP sunucusu Unity'nin içinde çalışır.
2. Claude Code'da **yeni sohbet** aç. Bağlam temiz kalır; agent'ın hafızası `Docs/PROGRESS.md`'de durur.
3. Görevin prompt'unu kopyala, yapıştır, gönder.
   - Büyük görevlerde önce **plan modunda** çalıştırıp planı onaylayabilirsin.
4. Agent kodu yazar, derler, konsolu temizler, üreticileri çalıştırır, testleri ve duman testini yapar, sonra rapor verir.
5. Raporun **"Senin test etmen gerekenler"** kısmını Play'e basıp kendin dene.
6. Sorun varsa aşağıdaki **yardımcı prompt'ları** kullan. **Kırmızı hata varken bir sonraki göreve geçme.**
7. Çalışıyorsa commit at. Mesaj örneği: `[Gün 3.2] Ateş etme`.

> 💡 Agent MCP araçlarını ilk kullandığında izin isteyecek. Sık kullanılan Unity araçlarına (konsol okuma, test, menü çalıştırma) bu proje için kalıcı izin verebilirsin.

### Yol haritası

| Gün | Tema | Gün sonunda |
|---|---|---|
| 0 | Kurulum | Boş Unity projesi + VS Code + git + Unity MCP bağlı |
| 1 | Temel iskelet | Modüller, Core, sahne üretici |
| 2 | Oyuncu ve kamera | Karakter yürüyor, nişan alıyor |
| 3 | Silahlar | 🏁 **M1:** Ateş edip kukla vurabiliyorsun |
| 4 | Düşmanlar | Zombiler kovalıyor, saldırıyor, ölüyor |
| 5 | Jeneratör ve dalgalar | Gece dalgası oynanıyor |
| 6 | Gün/gece döngüsü + HUD | 🏁 **M2:** İlk oynanabilir döngü |
| 7 | Ekonomi | Jeton, kombo, faiz |
| 8 | İnşa | Barikat kur, yükselt, sat, onar |
| 9 | Taret, tuzak, yakıt | Otomatik savunma ve güç yönetimi |
| 10 | Market | 🏁 **M3:** MVP tamam |
| 11 | Gündüz aktiviteleri | Sandıklar, istihbarat, ara kontrol |
| 12 | İçerik | Tükürücü, Patlayan, Boss, gece olayları |
| 13 | Perk'ler | 🏁 **M4:** İçerik tamam |
| 14 | Oyun hissi, ses, menüler | Cilalı oyun |
| 15 | Denge, performans, build | Dağıtılabilir build |
| 16 | Asset hazırlığı | 🏁 **M5:** Asset eklemeye hazır |

---

## Yardımcı prompt'lar

**A — Derleme / konsol hatası**
```text
Unity konsolunda hata var. Konsolu MCP ile oku, hataları bul ve düzelt; konsol temizlenene kadar tekrarla. Sadece hatayı düzelt, başka değişiklik yapma.
```

**B — Konsol hatasını yapıştırarak** (MCP bağlı değilse)
```text
Unity Console'da şu hata var:
<HATA METNİNİ BURAYA YAPIŞTIR>
Önce nedenini 2-3 cümleyle açıkla, sonra düzelt. Kapsamı genişletme.
```

**C — Davranış hatası** (derleniyor ama yanlış çalışıyor)
```text
[Hata] Görev X.Y sonrası:
- Ne yaptım: ...
- Ne bekliyordum: ...
- Ne oldu: ...
Önce olası nedenleri listele. Kodu inceleyerek ve gerekirse MCP ile Play Mode'da hiyerarşiye, component değerlerine ve konsola bakarak en olasısını bul, sonra düzelt. Emin değilsen geçici Debug.Log ekleyip duman testiyle doğrula; doğrulayınca log'u kaldır.
```

**D — Önce plan**
```text
Bu görevi uygulamadan önce kısa bir plan çıkar: hangi dosyalar, hangi sınıflar, hangi olaylar/servisler, hangi üretici güncellemeleri. Onayımı bekle.
```

**E — Commit**
```text
Değişiklikleri incele ve tek bir commit at. Mesaj formatı: "[Gün X.Y] kısa açıklama". Üretilmiş Library/Temp dosyalarının girmediğinden emin ol.
```

**F — Durum özeti** (uzun aradan sonra)
```text
Docs/PROGRESS.md'yi ve kodu inceleyip projenin şu anki durumunu 5 maddede özetle: ne çalışıyor, ne yarım, bilinen hatalar, sıradaki görev.
```

**G — MCP bağlantısı yok gibi**
```text
Unity MCP araçlarını görebiliyor musun? Görebiliyorsan listele ve konsolu oku. Göremiyorsan bana neyi kontrol etmem gerektiğini adım adım söyle.
```

---

## Gün 0 — Kurulum

### Unity ve VS Code

1. **Unity Hub**'dan en güncel **Unity 6 LTS (6000.x)** sürümünü kur. Build Support modüllerini ekibin işletim sistemine göre seç.
2. **New Project → Universal 3D (URP)** şablonu, proje adı `LastHaven`.
3. **Window → Package Manager → Unity Registry**'den şunları kur:
   - **Input System** (zaten yüklüyse geç; "enable backend" sorarsa *Yes* de, editör yeniden başlar)
   - **AI Navigation**
   - **Cinemachine** (3.x)
   - Test Framework zaten yüklüdür.
4. **Edit → Project Settings → Player → Active Input Handling:** `Input System Package (New)`.
5. **Edit → Preferences → External Tools → External Script Editor:** Visual Studio Code. Ardından *Regenerate project files*.
6. VS Code eklentilerini kur: **Unity** (Microsoft), **C# Dev Kit**, **Claude Code**. Claude Code eklentisini ilk açışta Claude hesabınla giriş yap (Claude Code için Pro, Max, Team veya Enterprise plan gerekir; ücretsiz plan yetmez).
7. **Claude Code'u terminale de kur.** MCP for Unity, Claude Code'u bu komut satırı sürümü sayesinde bulur:
   - macOS: `curl -fsSL https://claude.ai/install.sh | bash`
   - Windows (PowerShell): `irm https://claude.ai/install.ps1 | iex`

   Kurulumdan sonra terminalde `claude` yaz ve aynı hesapla giriş yap.
8. Bu paketteki `CLAUDE.md` dosyasını ve `Docs/` klasörünü Unity projesinin **köküne** kopyala (`Assets/` klasörünün yanına).
9. *(İsteğe bağlı)* Şablonun getirdiği örnek içerikleri silebilirsin: `Assets/TutorialInfo`, `Readme` asset'i, `SampleScene`.
10. Git kurulu değilse önce kur (macOS: `xcode-select --install`, Windows: git-scm.com). Unity'nin git adresinden paket kurabilmesi için de gerekli. Sonra proje kökünde `git init` çalıştır, GitHub'da private repo aç ve ekibi davet et. İlk commit'i Görev 1.1'den sonra at (o görevde `.gitignore` oluşacak).

### Unity MCP

11. **uv** kur (MCP sunucusu Python ile çalışır):
    - macOS: `brew install uv` (Homebrew yoksa: `curl -LsSf https://astral.sh/uv/install.sh | sh`)
    - Windows: `winget install --id=astral-sh.uv -e`
12. Unity'de **Window → Package Manager → + → Add package from git URL** ve şu adresi gir:
    `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`
    - Ekipte herkes aynı sürümü kullansın diye sondaki `#main` yerine GitHub **Releases** sayfasındaki son sürüm etiketini yaz (ör. `#v10.3.0`). Bu adres `Packages/manifest.json`'a girer ve commit'lenir.
13. **Window → MCP for Unity**'yi aç. Sunucu çalışmıyorsa *Start Server*'a bas. Sonra **Configure All Detected Clients** ile Claude Code'u yapılandır. Pencerede bağlantının bağlı (yeşil) göründüğünü kontrol et.
14. VS Code'u **proje kök klasöründe** aç (`CLAUDE.md`'nin otomatik okunması için şart) ve Claude Code panelini başlat. Yapılandırmadan sonra VS Code'u bir kez yeniden başlatman gerekebilir.
15. Bağlantıyı aşağıdaki prompt'la test et.

### 0.1 MCP bağlantı testi
```text
[Gün 0 · 0.1] MCP bağlantı testi
Unity MCP araçlarını listele. Sonra:
1. Konsolu oku ve özetle.
2. Aktif sahnenin hiyerarşisini özetle.
3. Geçici bir küp oluştur, pozisyonunu (0, 1, 0) yap, doğrula, sonra sil.
Hiçbir dosyayı değiştirme, sahneyi kaydetme. Sonucu raporla.
```

**Sorun giderme:** Araçlar görünmüyorsa sırayla kontrol et:
- Unity açık mı?
- MCP for Unity penceresi bağlı görünüyor mu?
- Terminalde `uv --version` çalışıyor mu?
- VS Code'u yeniden başlattın mı?

Hâlâ olmuyorsa *Configure* yerine MCP penceresinin gösterdiği manuel kurulum komutunu kullan.

> **Neden bu MCP?** MCP for Unity (CoplayDev) ücretsiz, açık kaynak ve aktif geliştiriliyor. Unity'nin resmi MCP'si (AI Assistant paketi) şu an beta; Unity AI aboneliği ve Unity Cloud bağlantısı istiyor. Plan belirli araç adlarına bağlı olmadığı için ileride resmi MCP'ye geçmek kolay.

---

## Gün 1 — Temel iskelet

### 1.1 Proje iskeleti
```text
[Gün 1 · 1.1] Proje iskeleti
CLAUDE.md'yi ve Docs/GDD.md'yi baştan sona oku.

Yap:
1. CLAUDE.md'deki klasör yapısını Assets/_Project altında oluştur (boş klasörlere .gitkeep).
2. Her modül için asmdef oluştur: LastHaven.Core, .Combat, .Player, .Enemies, .Waves, .Building, .Economy, .GameFlow, .Progression, .UI, .Audio, .Feedback, .DebugTools. Referansları CLAUDE.md'deki bağımlılık tablosuna göre isimle ver. Paket referanslarını henüz ekleme (ihtiyaç olunca eklenecek).
3. Her modüle derlenebilmesi için küçük bir AssemblyInfo.cs koy (namespace LastHaven.<Modül>, boş internal static sınıf).
4. LastHaven.Editor (sadece Editor platformu, tüm modüllere referans) ve LastHaven.Tests.EditMode (Test Framework kurallarına uygun: UNITY_INCLUDE_TESTS, nunit, TestRunner referansları; tüm modüllere referans) asmdef'leri.
5. Proje köküne Unity için .gitignore ve .gitattributes. Git LFS kalıplarını (fbx, obj, png, psd, tga, exr, wav, mp3, ogg) .gitattributes içinde YORUM SATIRI olarak hazırla; Gün 16'da açılacak.
6. Docs/PROGRESS.md'yi güncelle.

Bu görevde oyun kodu yazma, sadece yapı.
Kabul: Unity derleme hatası vermiyor; Project penceresinde _Project yapısı ve asmdef'ler görünüyor.
```

### 1.2 Core altyapısı
```text
[Gün 1 · 1.2] Core altyapısı
Modül: Core (+ Tests)

Yap:
1. IEvent arayüzü ve statik generic EventBus<T> (where T : struct, IEvent): Subscribe, Unsubscribe, Raise. Raise sırasında abone listesi değişirse güvenli olsun. SubsystemRegistration'da sıfırlansın.
2. ServiceLocator: Register<T>, Unregister<T>, Get<T> (yoksa açıklayıcı hata), TryGet<T>. Aynı tip iki kez kaydedilirse uyarı. SubsystemRegistration'da sıfırlansın.
3. Hasar sözleşmesi:
   - DamageType enum: Bullet, Melee, Explosion, Trap.
   - DamageInfo readonly struct: Amount, DamageType, Source (GameObject), HitPoint, HitDirection.
   - IDamageable: TakeDamage(DamageInfo), IsAlive.
   - TargetType enum: Player, Generator, Barricade, Turret, Trap.
   - ITargetable: Transform, TargetType, IsTargetable.
4. HealthModel (saf C#): Max, Current, TakeDamage, Heal (Max'ı aşmaz), SetMax; C# event'leri Damaged, Healed, Died. Ölüyken hasar almaz.
5. Health (MonoBehaviour): HealthModel'i sarar, IDamageable uygular, Core/Events/HealthEvents.cs içindeki DamagedEvent ve DiedEvent'i EventBus'a gönderir.
6. GameLayers (CLAUDE.md'deki sabit indeksler + LayerMask yardımcıları) ve GameTags sabit sınıfları.
7. EditMode testleri: EventBus (abone ol/çık/raise, raise içinde abonelikten çıkma), ServiceLocator, HealthModel (hasar, ölüm, ölüyken hasar, iyileştirme sınırı).

Kabul: Test Runner'da (Window → General → Test Runner → EditMode) tüm testler yeşil.
```

### 1.3 Editör üretici araçları ve test sahnesi
```text
[Gün 1 · 1.3] Editör üreticilerinin temeli + test haritası
Modül: Editor, Core, Waves, Enemies

Yap:
1. Üretici altyapısı (CLAUDE.md "Üretici mimarisi"): IGenerationStep, ISceneBuildStep, SceneBuildContext (kök objelere erişim, materyal sağlayıcı), TypeCache ile adım keşfi. Menüler: Tools/Last Haven/Setup Project, Generate Default Data, Generate Prefabs, Build Game Scene, Rebuild All.
2. Setup Project: GameLayers'taki layer'ları TagManager'a sabit indekslerle ve gerekli tag'leri ekler; ne yaptığını loglar.
3. PlaceholderMaterials: GDD §18'deki renkler için URP/Lit materyallerini Materials/Placeholder altında üretir (varsa dokunmaz). Diğer üreticiler materyali buradan ister.
4. Build Game Scene: Scenes/Game.unity'yi oluşturur veya açar. Kökler [Systems], [Gameplay], [Environment] ([Environment]'a dokunma kuralı geçerli). [Gameplay] altına GDD §17'ye göre:
   - 80×80 zemin (Ground layer)
   - üs alanı dairesi (görsel)
   - sabit seed'li ~25 engel (Obstacle layer)
   - görünmez sınır duvarları
   - directional light
   Sahneyi Build Settings'e ekler.
5. Core: CompassDirection enum (North, East, South, West). Waves: SpawnZone bileşeni (Direction, Radius, renkli gizmo); sahneye 4 tane yerleştir.
6. Enemies: NavMeshBaker (CLAUDE.md 'Proje sabitleri') + zemine NavMeshSurface. Gerekli asmdef'lere Unity.AI.Navigation referansı ekle.

Kabul: Tools → Last Haven → Rebuild All → Game.unity açılıyor; zemin, engeller ve 4 spawn gizmosu görünüyor. Play'e basınca Console'da NavMesh bake log'u var, AI Navigation overlay'inde NavMesh görünüyor.
```

---

## Gün 2 — Oyuncu ve kamera

### 2.1 Girdi sistemi
```text
[Gün 2 · 2.1] Girdi sistemi
Modül: Core (arayüz), Player

Yap:
1. Core'a IInputReader:
   - MoveInput (Vector2), AimScreenPosition (Vector2), AimStick (Vector2), IsUsingGamepad, IsFireHeld, IsInteractHeld.
   - C# event'leri: ReloadPressed, DashPressed, WeaponSlotPressed(int), WeaponCyclePressed(int yön), BuildModePressed, RotatePressed, UpgradePressed, SellPressed, BuildSlotPressed(int), CancelPressed, ShopPressed, ReadyPressed, PausePressed, ConfirmPressed.
   - SetMode(InputMode.Gameplay / BuildMode / UI).
2. Player modülüne InputReader (MonoBehaviour): GDD §4'teki klavye/fare ve gamepad bağlamalarını tamamen KODDA oluşturur (InputAction + composite). Action map'ler: Gameplay, BuildMode, UI. BuildMode'da 1–5 tuşları BuildSlotPressed'e gider. Son kullanılan cihazı takip etsin. Awake'te ServiceLocator'a kaydolur, OnEnable/OnDisable'da action'ları açar/kapar.
3. Player asmdef'ine Unity.InputSystem referansı.
4. Scene step: [Systems]/Input objesi.

Kabul: Derleme hatası yok; Play'de geçici bir debug log tuşlara basınca olayları yazıyor (logu görev sonunda kaldır veya DEBUG bayrağına bağla).
```

### 2.2 Oyuncu hareketi
```text
[Gün 2 · 2.2] Oyuncu hareketi ve nişan
Modül: Core (arayüz), Player, Editor

Yap:
1. PlayerData SO (GDD §5).
2. Core'a IPlayer: Transform, Position, AimPoint, AimDirection, IsAlive, PickupRadius.
3. PlayerMotor: CharacterController ile kamera yaw'ına göre hareket, yerçekimi, dash (süre boyunca dokunulmazlık bayrağı, bekleme süresi).
4. PlayerAim: fare modunda imleçten oyuncu yüksekliğindeki yatay düzleme ray atıp oyuncuyu o noktaya döndürür; gamepad'de sağ çubuk yönüne döner.
5. Player kök bileşeni: Health (PlayerData'dan), ITargetable(Player), Player layer; ServiceLocator'a IPlayer olarak kaydolur.
6. Core'a ITargetRegistry (Register, Unregister, FindNearest(TargetType, pos, maxDist), GetAll(TargetType)) + TargetRegistry implementasyonu ([Systems] altında). Oyuncu kendini kaydetsin.
7. Generators:
   - PlayerData asset'i.
   - Player prefab'ı: kök CharacterController + scriptler; "Visual" altında mavi kapsül + burun küpü; Visual dışında "Muzzle" transform'u.
8. Scene step: oyuncuyu merkezin 3 m güneyine koy.

Kabul: Play'de WASD ile yürüyor, fareye dönüyor, Space ile atılıyor (kamera henüz sabit olabilir).
```

### 2.3 Kamera
```text
[Gün 2 · 2.3] Kamera
Modül: Core (arayüz), Player, Editor

Yap:
1. CameraConfig SO: GDD §4'teki iki preset (İzometrik, Tepeden), look-ahead mesafesi, takip yumuşatması.
2. Core'a ICameraService: Camera, CurrentPreset, SetPreset(CameraPreset), Shake(float strength). Shake şimdilik boş; Gün 14'te dolacak.
3. Player/Camera/CameraRig:
   - Cinemachine 3 kullanır: Main Camera'da CinemachineBrain; CinemachineCamera + CinemachineFollow.
   - Kamera açısı preset'ten gelir.
   - Takip hedefi, oyuncu ile nişan noktası arasında look-ahead kadar kayan bir "CameraTarget" objesidir.
4. PlayerMotor kamera yaw'ını ICameraService'ten alsın.
5. Geliştirme kolaylığı: F9 ile preset değişsin (#if UNITY_EDITOR || DEVELOPMENT_BUILD).
6. Asmdef'lere Unity.Cinemachine referansı. Scene step: kamera kurulumu.

Kabul: Kamera oyuncuyu yumuşakça takip ediyor, fareye doğru hafifçe kayıyor; F9 izometrik ↔ tepeden geçiş yapıyor ve W her iki modda da ekranın yukarısı.
```

---

## Gün 3 — Silahlar · 🏁 M1

### 3.1 Stat sistemi ve silah verisi
```text
[Gün 3 · 3.1] Stat sistemi ve silah verisi
Modül: Core, Combat, Editor

Yap:
1. Core/Stats:
   - StatType enum: GDD §5, §6, §11, §13'te geçen tüm çarpanlar (PlayerDamage, PlayerFireRate, PlayerMagSize, PlayerMaxHealth, DashCooldown, PickupRadius, CoinValue, TurretDamage, TurretFireRate, TurretRange, BarricadeMaxHealth...).
   - StatModifier: Flat / PercentAdd, kaynak id.
   - StatCollection (saf C#): taban değere modifier'ları uygular, değişince event.
2. Core: IRunModifiers ve RunModifiers implementasyonu (saf C# + kayıt bileşeni).
   - Metotlar: Get(StatType, baseValue), AddModifier, RemoveBySource, HasFlag(PerkFlag), AddFlag.
   - PerkFlag enum: Ricochet, Vampire, ThornWalls, ShockTurret, ChainExplosion, Hunter.
3. Combat: WeaponData SO. Alanlar: id, displayName, damage, pelletCount, fireRate, magSize, reserveAmmo (-1 = sonsuz), reloadTime, spreadAngle, range, tracerColor, ve opsiyonel fireSound / muzzleVfx referansları.
4. Generator: Pistol, Shotgun, Rifle asset'leri (GDD §6).
5. EditMode testleri: StatCollection (flat + yüzde birleşimi, kaynak bazlı kaldırma), RunModifiers.

Kabul: Testler yeşil; Data/Weapons altında 3 asset GDD değerleriyle.
```

### 3.2 Ateş etme
```text
[Gün 3 · 3.2] Ateş etme
Modül: Core (olaylar), Combat, Player, Editor

Yap:
1. Combat/WeaponRuntime (saf C#): şarjör, yedek mermi, atış zamanlayıcısı, doldurma durumu, yükseltme seviyeleri. Etkili değer = WeaponData × yükseltme × IRunModifiers.
2. Combat/HitscanShooter (yeniden kullanılabilir; taretler de kullanacak):
   - Girdi: origin, yön, saçılma, saçma sayısı, menzil, LayerMask.
   - IDamageable'a DamageInfo uygular.
   - Vuruş sonuçlarını döndürür ve her vuruş için ShotHitEvent atar (nokta, normal, hedef türü, kaynak).
3. Player/WeaponController:
   - 3 slot; başlangıçta sadece Tabanca açık.
   - 1–3 tuşları ve tekerlek ile değiştirme.
   - Basılı tutunca fireRate'e göre otomatik atış.
   - R ile doldurma; şarjör boşalınca otomatik doldurma.
   - Atış Muzzle'dan, nişan yönüne.
4. Core/Events/CombatEvents.cs: WeaponFiredEvent, ShotHitEvent, WeaponChangedEvent, AmmoChangedEvent, ReloadStartedEvent, ReloadFinishedEvent.
5. Scene step: üs dışına 3 test kuklası (Health'li gri kapsül, ölünce 3 sn sonra dolu canla geri gelir), [Gameplay]/TestDummies altında. SceneBuilder'da kapatılabilir bir seçenek olsun.
6. EditMode testleri: WeaponRuntime (şarjör, doldurma, sonsuz yedek).

Kabul: Sol tıkla ateş, R doldurma, 1-2-3 değiştirme (2-3 kilitli); kuklalar hasar alıp ölüyor (şimdilik debug log yeterli).
```

### 3.3 Havuzlama ve vuruş efektleri
```text
[Gün 3 · 3.3] Havuzlama ve vuruş efektleri
Modül: Core, Feedback, Editor

Yap:
1. Core:
   - IPoolService: Get(prefab, pos, rot), Release(instance), Prewarm(prefab, n).
   - IPoolable: OnSpawned, OnDespawned.
   - PoolService implementasyonu (UnityEngine.Pool.ObjectPool<T>, [Systems] altında).
   - PooledLifetime bileşeni: süre sonunda kendini havuza iade eder.
2. Feedback:
   - Tracer: LineRenderer, ~0.05 sn, WeaponData rengi.
   - ImpactEffect: kodla kurulan küçük ParticleSystem; düşmanda kırmızı, yüzeyde kıvılcım.
   - MuzzleFlash: kısa point light.
   - FeedbackDirector WeaponFiredEvent ve ShotHitEvent'i dinleyip bu efektleri havuzdan oynatır. Combat, Feedback'i bilmez.
3. Generator: Tracer, Impact, MuzzleFlash prefab'ları.

Kabul: Ateş edince iz, namlu ışığı ve vuruş kıvılcımı görünüyor; Profiler'da ateş sırasında Instantiate yok.
```

> 🏁 **M1:** Karakter yürüyor, nişan alıyor, ateş ediyor. Commit'le, ekiple paylaş.

---

## Gün 4 — Düşmanlar

### 4.1 Düşman temeli ve hedefleme
```text
[Gün 4 · 4.1] Düşman temeli ve hedefleme
Modül: Core (olaylar), Enemies

Yap:
1. EnemyData SO (GDD §7): id, displayName, maxHealth, moveSpeed, damage, attackRate, attackRange, moneyReward, xpReward, aggroRadius, preferredTarget (TargetType?), preferredTargetRadius, structureDamageMultiplier, knockbackResistance, scale, prefab referansı, opsiyonel ses/VFX.
2. Enemy (kök MonoBehaviour): NavMeshAgent + Health + IPoolable. Durum makinesi saf C# state'lerle: Spawning (0.5 sn yerden çıkma), Chasing, Attacking, Dead.
3. TargetSelector (saf C#, test edilebilir): GDD §7 hedef seçim kuralları 1–3. Adayları ITargetRegistry'den alır, 0.5 sn'de bir değerlendirir. Tüm düşmanlar aynı frame'de değerlendirmesin diye başlangıç zamanı rastgele kaydırılsın.
4. Saldırı: menzildeyse attackRate'e göre hedefin IDamageable'ına Melee hasar verir; hedef yapıysa structureDamageMultiplier uygulanır.
5. Ölüm: Core/Events/EnemyEvents.cs → EnemyKilledEvent (pozisyon, enemyId, moneyReward, xpReward, KilledByPlayer bool). Ajan kapanır, 2 sn sonra yere batar ve havuza döner.
6. Jeneratör henüz yok: scene step üs merkezine geçici GeneratorPlaceholder koysun (Health 1500, ITargetable Generator; Gün 5'te kaldırılacak).
7. EditMode testleri: TargetSelector (aggro, tercih edilen hedef, varsayılan jeneratör).

Kabul: Testler yeşil. (Oyun içi test 4.2'de.)
```

### 4.2 Düşman tipleri ve debug spawner
```text
[Gün 4 · 4.2] Düşman tipleri ve debug spawner
Modül: Enemies, DebugTools, Editor

Yap:
1. Generators:
   - Walker, Runner, Tank EnemyData asset'leri (GDD §7). Spitter, Exploder ve Brute Gün 12'de gelecek.
   - Her tip için ayrı prefab: kökte NavMeshAgent, CapsuleCollider, Enemy, Health, Enemy layer; Visual'da GDD §18 şekil, renk, ölçek. Prefab referansı EnemyData'ya otomatik bağlansın.
2. Enemies/EnemySpawner: Spawn(EnemyData, position) havuzdan çeker, NavMesh.SamplePosition ile geçerli noktaya koyar. Core'a IEnemySpawner arayüzü ekle (DebugTools ve Waves kullanacak).
3. DebugTools/DebugSpawner (sadece Editor veya Development build):
   - F1: imleçteki noktaya Walker. F2: Runner. F3: Tank.
   - F4: rastgele kenardan 20 Walker.
   - Sol üstte kısayol listesi (OnGUI yeterli).

Kabul: F1-F4 ile zombiler çıkıyor, engellerin etrafından dolaşarak oyuncuya/jeneratöre gidiyor, menzilde saldırıyor; vurulunca ölüp havuza dönüyorlar.
```

### 4.3 Vuruş hissi ve AnimationDriver
```text
[Gün 4 · 4.3] Vuruş hissi ve AnimationDriver
Modül: Core, Enemies, Player

Yap:
1. Core/AnimationDriver (CLAUDE.md "Görsel ayrımı"):
   - API: SetSpeed, TriggerAttack, TriggerHit, TriggerDie, SetDashing, TriggerFire, TriggerReload.
   - Visual altında Animator varsa parametreleri ayarlar; yoksa prosedürel yedek oynatır: saldırıda öne atılma, vuruşta sarsılma, ölümde rastgele yöne devrilme.
   - Parametre isimleri AnimParams sabit sınıfında.
2. Enemies:
   - Vuruşta 0.08 sn beyaz flaş (MaterialPropertyBlock, Visual altındaki tüm Renderer'lar; _BaseColor/_EmissionColor yoksa atla).
   - Hasar yönünde geri itilme (knockbackResistance ile; kısa süreli agent.Move).
   - Tüm animasyonlar AnimationDriver üzerinden.
3. Enemies: küçük billboard can barı (quad). Sadece hasar aldıktan sonra 3 sn görünür, havuz dostu.
4. Player:
   - Hasar alınca kırmızı flaş ve 0.3 sn dokunulmazlık.
   - Core/Events/PlayerEvents.cs → PlayerDamagedEvent, PlayerDiedEvent.
   - Oyuncu da AnimationDriver kullansın.

Kabul: Vurulan zombi beyaz parlıyor ve geri sekiyor, can barı beliriyor, ölünce devriliyor; zombi saldırınca öne atılıyor; oyuncu hasar alınca kırmızı parlıyor.
```

---

## Gün 5 — Jeneratör ve dalgalar

### 5.1 Jeneratör ve üs alanı
```text
[Gün 5 · 5.1] Jeneratör ve üs alanı
Modül: Core (arayüzler), Building, Editor

Yap:
1. GeneratorData SO (GDD §12): can, yakıt kapasitesi, taban tüketim, taret başı tüketim, acil yakıt fiyatı ve miktarı.
2. Building/Generator:
   - Health + ITargetable(Generator).
   - Ölünce Core/Events/BaseEvents.cs içindeki GeneratorDestroyedEvent.
   - Core'a IPowerService (IsPowered) ve PowerChangedEvent. Yakıt tüketimi Gün 9'da; şimdilik IsPowered hep true.
3. Building/BaseArea: Core'a IBaseArea (Center, Radius, Contains(pos)) arayüzü. GDD §11 yarıçapı.
4. BaseLights: jeneratör çevresinde 4 nokta ışık; PowerChangedEvent ile açılıp kapanır.
5. GeneratorPlaceholder'ı kaldır. Generator prefab ve data üret; scene step merkeze koysun.

Kabul: Zombiler jeneratöre yürüyüp saldırıyor; jeneratör ölünce Console'da GeneratorDestroyedEvent logu.
```

### 5.2 Dalga sistemi
```text
[Gün 5 · 5.2] Dalga sistemi
Modül: Core (arayüz), Waves

Yap:
1. NightData SO: nightNumber, SpawnGroup listesi (EnemyData, count, spawnInterval, startDelay, directions listesi), modifier referansı (şimdilik null; Gün 12), bossData (opsiyonel).
2. WaveConfig SO: sıralı NightData listesi, maxAliveEnemies (GDD §8).
3. WaveRunner (saf C#, test edilebilir): zaman tabanlı grup işleme, canlı düşman tavanı ve kuyruk, bitiş koşulu. Spawn isteklerini callback ile dışarı verir.
4. WaveSpawner (MonoBehaviour):
   - Core'a IWaveService: StartNight(int nightIndex), TotalNights, IsNightRunning, AliveCount, GetTonightInfo(int nightIndex) → yönler, düşman tipi/adetleri, olay adı.
   - WaveRunner'ı sürer; spawn için ilgili yöndeki SpawnZone içinde rastgele nokta seçip IEnemySpawner'ı kullanır.
5. Olaylar (Core/Events/WaveEvents.cs): NightStartedEvent, WaveProgressEvent (kalan düşman), NightCompletedEvent.
6. EditMode testleri: WaveRunner (zamanlama, tavan/kuyruk, bitiş koşulu).

Kabul: Testler yeşil.
```

### 5.3 10 gecenin verisi
```text
[Gün 5 · 5.3] 10 gecenin verisi ve doğrulama
Modül: Waves, Editor, DebugTools

Yap:
1. Generator:
   - GDD §8 tablosundaki 10 NightData ve WaveConfig.
   - Henüz olmayan düşman tipleri (Tükürücü, Patlayan, Brute) ve gece olayları için grupları atla ama TODO olarak logla.
   - Generator, Gün 12'de eksik grupları ve referansları sonradan doldurabilecek şekilde yazılsın ("eksikse ekle" mantığı).
2. Tools/Last Haven/Validate Project: şimdilik NightData kontrolleri (boş grup, 0 adet, null EnemyData, yönsüz grup). Sonuçları Console'a ve Docs/VALIDATION_REPORT.md'ye yazsın. Aynı doğrulama tüm NightData'lar için EditMode testi olarak da çalışsın.
3. DebugTools: F5 → sıradaki geceyi hemen başlat; F6 → tüm canlı düşmanları öldür.

Kabul: F5 ile 1. gece başlıyor, zombiler kuzeyden akıyor; hepsi ölünce NightCompletedEvent logu düşüyor.
```

---

## Gün 6 — Gün/gece döngüsü + HUD · 🏁 M2

### 6.1 Oyun akışı durum makinesi
```text
[Gün 6 · 6.1] Oyun akışı
Modül: Core (arayüz/olaylar), GameFlow, Player

Yap:
1. GameConfig SO (GDD §9, §15): gündüz süresi, alacakaranlık süresi, son uyarı süresi, erken hazır bonusu (para/10 sn), respawn süresi.
2. GameStateMachine (saf C#, test edilebilir) + GameFlowController (MonoBehaviour):
   - Akış: Boot → Day → Dusk → Night → Dawn → Day ...
   - Son gece biterse → Victory. GeneratorDestroyedEvent → GameOver.
   - Toplam gece sayısı IWaveService.TotalNights'tan gelir.
3. Core:
   - IGameStateService: CurrentPhase, NightIndex, PhaseTimeRemaining.
   - Core/Events/GameFlowEvents.cs: PhaseChangedEvent (Phase, NightIndex, Duration), DayTimerTickEvent (saniyede 1), DayEndingSoonEvent, EarlyReadyEvent (kalan sn), DawnEvent, DawnContinueRequestedEvent, GameOverEvent, VictoryEvent.
4. Davranış:
   - Day süresi bitince veya ReadyPressed gelince → Dusk.
   - Night'ta IWaveService.StartNight çağrılır; NightCompletedEvent → Dawn.
   - Dawn'dan çıkış DawnContinueRequestedEvent ile olur (UI gelene kadar Enter).
5. Player/PlayerRespawn: PlayerDiedEvent → GameConfig süresi sonra IBaseArea.Center yanında tam canla doğ; PlayerRespawnedEvent.
6. EditMode testleri: tüm geçişler, zafer, game over, erken hazır.

Kabul: Play → 120 sn gündüz (veya Enter) → gece dalgası → bitince şafak → Enter → gündüz 2. Console logları akışı doğruluyor.
```

### 6.2 Gün/gece ışığı ve atmosfer
```text
[Gün 6 · 6.2] Gün/gece ışığı ve atmosfer
Modül: GameFlow, Player, Editor

Yap:
1. LightingProfile SO (Day, Night): directional light rotasyonu/rengi/yoğunluğu, ambient renk, fog açık/kapalı/renk/yoğunluk.
2. GameFlow/Lighting/DayNightLighting: PhaseChangedEvent'i dinler; Dusk boyunca gündüz→gece, Dawn başında gece→gündüz yumuşak geçiş yapar.
3. URP Volume: scene step iki Global Volume kursun (DayVolume, NightVolume). Profiller Vignette, Color Adjustments ve Bloom içersin; editörde üretilsin. Faz geçişinde weight'ler yumuşakça değişsin.
4. Player/Flashlight: kökte (Visual dışında) Spot Light; gece açık, gündüz kapalı.
5. Gerekli asmdef'lere URP referansları.

Kabul: Alacakaranlıkta 4 sn içinde gece mavisine geçiş, fener ve üs ışıkları yanıyor; şafakta geri dönüyor.
```

### 6.3 HUD temeli (UI Toolkit)
```text
[Gün 6 · 6.3] HUD temeli
Modül: UI, GameFlow, Editor

Yap:
1. UI asset üretimi:
   - Editörde PanelSettings asset'i (Scale With Screen Size, 1920×1080).
   - Runtime tema: UI/Themes/Runtime.tss, içeriği: @import url("unity-theme://default"); — bu dosyayı doğrudan yaz ve PanelSettings'e ata.
   - Theme.uss: renk değişkenleri, font boyutları, ortak sınıflar (.panel, .bar, .bar-fill, .label-big ...).
2. HUD.uxml + HUDController:
   - Gösterilenler: evre adı + zamanlayıcı, Gece X/10, oyuncu canı (bar), silah adı + mermi, jeneratör canı (bar), kalan düşman (sadece gece).
   - Güncellemeler olaylarla yapılır; zamanlayıcı DayTimerTickEvent ile.
3. GameFlow/StatsTracker (saf C# + köprü): gece boyunca öldürme sayısını toplar (para ve kombo Gün 7'de eklenecek).
4. DawnPanel.uxml: o gecenin istatistikleri + "Devam" butonu (→ DawnContinueRequestedEvent).
5. GameOverPanel / VictoryPanel: basit; "Yeniden Başlat" sahneyi yeniden yükler.
6. Scene step: [Systems]/UI altında UIDocument'lar ve referanslar.

Kabul: Tam döngü HUD ile okunabiliyor; şafakta panel çıkıyor, Devam ile gündüze geçiliyor; jeneratör ölünce Oyun Bitti paneli.
```

> 🏁 **M2:** İlk oynanabilir döngü. Ekipten 2-3 kişiye oynat; ilk izlenimleri PROGRESS.md'ye not et.

---

## Gün 7 — Ekonomi

### 7.1 Cüzdan ve faiz
```text
[Gün 7 · 7.1] Cüzdan ve faiz
Modül: Core (arayüz/olaylar), Economy, UI

Yap:
1. EconomyConfig SO (GDD §10): başlangıç para/yakıt, faiz oranı ve tavanı, erken hazır bonusu, jeton ömrü ve yanıp sönme süresi, kombo penceresi ve eşikleri, satış iade oranı.
2. Wallet (saf C#): Money, Fuel, Add, TrySpend, CanAfford; değişince event.
3. Core: IWallet arayüzü; Core/Events/EconomyEvents.cs → MoneyChangedEvent, FuelChangedEvent, InterestPaidEvent.
4. WalletService (MonoBehaviour): kaydı ve olayları yönetir.
5. Faiz: DawnEvent'te paranın %10'u (tavanlı). Erken hazır bonusu: EarlyReadyEvent'te.
6. HUD: para ve yakıt göstergesi.
7. EditMode testleri: Wallet, faiz, bonus.

Kabul: HUD'da 300 para / 100 yakıt; şafakta faiz ekleniyor; erken Enter'da bonus geliyor.
```

### 7.2 Jeton düşürme ve toplama
```text
[Gün 7 · 7.2] Jeton düşürme ve toplama
Modül: Economy, Player, Editor

Yap:
1. Economy/Coin (havuzlanabilir):
   - EnemyKilledEvent'te ödül 1–5 jetona bölünür.
   - Toplam değer = moneyReward × kombo çarpanı (7.3'te gelecek, şimdilik 1) × IRunModifiers CoinValue.
   - Kodla küçük yaylar çizerek saçılır; yerde döner.
2. Ömür: GDD §10 (son 3 sn yanıp söner), sonra havuza döner.
3. Mıknatıs:
   - IPlayer.PickupRadius = PlayerData × IRunModifiers PickupRadius (Player tarafında).
   - Yarıçap içine giren jeton hızlanarak oyuncuya uçar.
   - Temasta IWallet.Add ve CoinCollectedEvent (değer, pozisyon).
4. Generator: Coin prefab (Pickup layer, altın yassı silindir).

Kabul: Zombiler jeton saçıyor, yaklaşınca jetonlar oyuncuya uçuyor, para artıyor; toplanmayanlar yanıp sönüp kayboluyor.
```

### 7.3 Kombo çarpanı
```text
[Gün 7 · 7.3] Kombo çarpanı
Modül: Economy, GameFlow, UI

Yap:
1. ComboTracker (saf C#): GDD §10 kuralları. PlayerDamagedEvent'te ve pencere dolunca sıfırlanır. ComboChangedEvent (sayı, çarpan, kalan pencere oranı).
2. Coin değeri öldürme anındaki çarpanı kullansın.
3. HUD: sağ ortada büyük kombo sayısı + çarpan + azalan pencere barı. Eşik atlanınca USS transition ile kısa büyüme.
4. StatsTracker: kazanılan para, en yüksek kombo, faiz. Şafak ekranında göster.
5. EditMode testleri: ComboTracker (eşikler, sıfırlanma, pencere).

Kabul: Hızlı öldürünce kombo artıyor, ×1.5/×2/×3 görünüyor, jetonlar daha değerli; hasar alınca sıfırlanıyor.
```

---

## Gün 8 — İnşa

### 8.1 İnşa modu ve yerleştirme
```text
[Gün 8 · 8.1] İnşa modu ve yerleştirme
Modül: Core (olaylar), Building

Yap:
1. BuildableData SO (GDD §11): id, displayName, footprint (Vector2Int), levels listesi (cost, maxHealth, stat değerleri), prefab, carvesNavMesh, opsiyonel icon.
2. GridService (saf C#): 1 m hücre, dünya↔hücre dönüşümü, dolu hücre takibi, IBaseArea içinde mi kontrolü, jeneratör çevresi yasak bölgesi, döndürülmüş footprint.
3. BuildModeController:
   - Sadece Day evresinde çalışır. B ile açılıp kapanır (InputMode.BuildMode).
   - 1–5 ile yapı seçilir.
   - İmlecin altındaki hücrede yarı saydam hayalet önizleme: yeşil/kırmızı (para da kontrol edilir). Q ile 90° döner.
   - Sol tık: IWallet.TrySpend + yerleştir. Sağ tık veya B: iptal.
   - Gece başlarken otomatik kapanır.
4. Core/Events/BuildingEvents.cs: BuildModeChangedEvent, BuildSelectionChangedEvent, StructurePlacedEvent, StructureRemovedEvent.
5. EditMode testleri: GridService.

Kabul: Testler yeşil. (Yerleştirilecek gerçek yapı 8.2'de gelecek; bu görevde test için geçici bir küp BuildableData'sı kullanılabilir.)
```

### 8.2 Barikat, yükseltme, satış
```text
[Gün 8 · 8.2] Barikat, yükseltme, satış
Modül: Building, Editor

Yap:
1. Building/Structure (temel bileşen):
   - Health, ITargetable (tipine göre), seviye, toplam harcanan para, Upgrade(), Sell().
   - ITargetRegistry'ye kaydolur.
   - Seviye değişince Visual rengini GDD §18'e göre günceller.
2. Barricade: NavMeshObstacle (carve), GDD §11 seviyeleri.
3. İnşa modunda var olan yapının üzerine gelince:
   - Vurgulama + StructureHoveredEvent (UI tooltip Gün 10'da).
   - U: yükselt. X: sat (%50 iade).
4. Yapı yıkılınca: StructureDestroyedEvent, grid hücreleri boşalır, kısa yıkılma efekti (Feedback).
5. Generator: Barricade data + prefab.

Kabul: Gündüz B → 1 → barikat kur (para düşüyor), U ile metale/betona yükselt, X ile sat; zombiler barikatın etrafından dolaşıyor.
```

### 8.3 Onarım ve tıkalı yol
```text
[Gün 8 · 8.3] Onarım ve tıkalı yol hedeflemesi
Modül: Core (arayüz), Building, Enemies, DebugTools

Yap:
1. Core: IInteractable (Prompt metni, CanInteract, HoldDuration, OnHoldProgress, OnInteract) ve InteractionPromptEvent.
2. Player/Interactor: en yakın IInteractable'ı bulur, E basılı tutulunca ilerletir.
3. Structure: hasarlıysa IInteractable olarak onarım sunar (GDD §11 hızı ve maliyeti; para biterse durur). İlerleme yapının üstünde küçük bar.
4. Enemies/TargetSelector'a kural 4: NavMeshAgent.pathStatus == PathPartial ise yol sonuna en yakın Structure hedeflenir. Testini ekle.
5. Tank tercihinin (barikat) çalıştığını doğrula.
6. Kalabalık ajanlar barikat önünde birbirini kilitlemesin: avoidancePriority rastgele.
7. DebugTools: F7 → +1000 para, F8 → +50 yakıt.

Kabul: Üssü tamamen barikatla çevir → zombiler en yakın barikata saldırıp kırıyor; E ile barikat onarılıyor, para azalıyor.
```

---

## Gün 9 — Taret, tuzak, yakıt

### 9.1 Taret
```text
[Gün 9 · 9.1] Taret
Modül: Building, Editor

Yap:
1. Turret (Structure + ITargetable Turret):
   - Hedefleme: menzildeki en yakın canlı düşman. 0.2 sn'de bir Physics.OverlapSphereNonAlloc ile Enemy layer taranır.
   - Kafa sadece Y ekseninde döner; hizalanınca Combat/HitscanShooter ile ateş eder. Tracer ve efektler olaylar sayesinde otomatik çalışır.
2. IPowerService.IsPowered false iken ateş etmez; kafa aşağı eğilir, gösterge ışığı söner.
3. Seviyeler GDD §11. Statlara IRunModifiers çarpanları uygulanır (TurretDamage, TurretFireRate, TurretRange).
4. İnşa modunda seçiliyken ve hover'da menzil dairesi önizlemesi.
5. Generator: Turret data + prefab.

Kabul: Taret kur → gece zombilere dönüp ateş ediyor; yükseltince daha sert vuruyor.
```

### 9.2 Yakıt ve güç
```text
[Gün 9 · 9.2] Yakıt ve güç
Modül: Building, UI

Yap:
1. Generator: Night evresinde GDD §12 tüketimi (taban + taret başına), IWallet.Fuel'den düşer.
   - Yakıt 0 olunca güç kesilir (PowerChangedEvent, PowerLostEvent).
   - Yakıt gelince güç geri döner.
2. Acil yakıt: gece jeneratöre yakınken IInteractable (1 sn basılı tut, GDD §12 fiyatı).
3. HUD: yakıt barı, tahmini kalan süre, güç yokken yanıp sönen "GÜÇ YOK" uyarısı.

Kabul: F8 ile yakıtı az bırak → gece yakıt bitiyor, ışıklar ve taretler kapanıyor; E ile acil yakıt alınca geri açılıyor.
```

### 9.3 Diken tuzağı ve durum efektleri
```text
[Gün 9 · 9.3] Diken tuzağı ve yavaşlatma
Modül: Core (arayüz), Building, Enemies, Combat, Editor

Yap:
1. Core: IStatusEffectReceiver (ApplySlow(oran, süre), IsSlowed).
2. Enemies/StatusEffects: en güçlü yavaşlatma geçerli olur ve agent.speed'e uygulanır; Visual'da hafif mavi tint.
3. Building/SpikeTrap (Structure, NavMesh'i kesmez, trigger alanı): GDD §11 tik hasarı + yavaşlatma; dayanıklılık bitince kırılır.
4. Combat: hedef yavaşlamışsa ve PerkFlag.Hunter aktifse +%50 hasar (perk Gün 13'te gelecek, kanca şimdi).
5. Generator: SpikeTrap data + prefab.

Kabul: Tuzaktan geçen zombiler mavileşip yavaşlıyor ve hasar alıyor; tuzak bir süre sonra kırılıyor.
```

---

## Gün 10 — Market · 🏁 M3

### 10.1 Market mantığı
```text
[Gün 10 · 10.1] Market mantığı
Modül: Core (arayüz/olaylar), Economy, Player, Combat, Editor

Yap:
1. ShopItemData SO: id, displayName, description, category (Weapons/Upgrades/Supplies), seviyeli fiyat listesi, maxLevel, effect türü (UnlockWeapon, WeaponUpgrade, BuyAmmo, HealthKit, BuyFuel, PickupRadius), hedef id (ör. weaponId) ve değer.
2. ShopService:
   - Core'a IShopService: GetItems, GetLevel, CanBuy, Buy.
   - Sadece Day evresinde satış yapar; seviye takibi tutar.
   - Etkiler olaylarla uygulanır: WeaponUnlockRequestedEvent, WeaponUpgradePurchasedEvent, AmmoPurchasedEvent, HealRequestedEvent, PurchaseCompletedEvent. Economy, Player/Combat'ı bilmez.
3. Player/Combat tarafında bu olayları dinleyen kodlar: silah açma, yükseltme seviyesi, yedek mermi, iyileştirme. Yakıt ve mıknatıs Economy/Player'da.
4. Generator: GDD §6 ve §10'daki tüm market öğeleri.
5. EditMode testleri: ShopService (para yetmezse, maks seviye, gece engeli).

Kabul: Testler yeşil.
```

### 10.2 Market arayüzü
```text
[Gün 10 · 10.2] Market arayüzü
Modül: UI

Yap:
1. Shop.uxml + uss:
   - Tab ile açılır, sadece gündüz. Açıkken InputMode.UI; oyun durmaz.
   - Sekmeler: Silahlar / Yükseltmeler / Malzeme.
   - Kart: ad, açıklama, seviye noktaları, fiyat, Satın Al butonu. Para yetmiyorsa kart soluk, maks seviyede "MAKS".
2. Satın alınca kartta kısa USS animasyonu.
3. Gece başlarken açıksa otomatik kapanır.
4. Klavye/gamepad ile gezilebilir (focus sırası).

Kabul: Gündüz Tab → pompalı al → 2 tuşuyla kuşan → yükseltme al → mermi al; gece Tab açılmıyor.
```

### 10.3 İnşa çubuğu ve etkileşim ipuçları
```text
[Gün 10 · 10.3] İnşa çubuğu, tooltip, etkileşim ipucu
Modül: UI

Yap:
1. İnşa modunda alt çubuk: yapı kartları (kısayol numarası, ad, fiyat, ikon yoksa renk kutusu); seçili vurgulu, para yetmeyen soluk.
2. StructureHoveredEvent → tooltip: ad, seviye, can, yükseltme fiyatı (U), satış iadesi (X).
3. InteractionPrompt widget'ı: dünya pozisyonunu ekrana yansıtır ("E — Onar (12)", "E — Yakıt (30)"), basılı tutma ilerleme halkası.
4. Tüm HUD metinlerini ve renklerini Theme.uss'e taşı; sabit kodlanmış stil kalmasın.

Kabul: İnşa modunda alt çubuk ve tooltip'ler doğru; onarım/yakıt ipuçları ilerleme halkasıyla görünüyor.
```

> 🏁 **M3 — MVP:** Gündüz market + inşa, gece savunma + jeton, şafakta faiz. Ekiple tam oyun testi yap ve PROGRESS.md'ye denge notları al.

---

## Gün 11 — Gündüz aktiviteleri

### 11.1 Ganimet sandıkları
```text
[Gün 11 · 11.1] Ganimet sandıkları
Modül: Economy, UI, Editor

Yap:
1. LootTableData SO (GDD §9): yakın/uzak eşik, ödül seçenekleri ve ağırlıkları (para aralığı, yakıt, mermi paketi).
2. LootCrateSpawner:
   - Her Day başında GDD sayısı kadar sandığı dış halkaya koyar (NavMesh üstünde, engellerle çakışmayan rastgele nokta).
   - Gece başında açılmamış sandıkları kaldırır.
3. LootCrate (IInteractable, 2 sn basılı tut):
   - Para jeton olarak saçılır; yakıt ve mermi FuelPickup/AmmoPickup olarak.
   - LootCollectedEvent.
4. HUD: ekran dışındaki en yakın 3 sandığın yönünü gösteren kenar göstergeleri.
5. Generator: LootCrate, FuelPickup, AmmoPickup prefab'ları.

Kabul: Gündüz 6 sandık çıkıyor, kenar okları yön gösteriyor, uzaktakiler daha iyi ödül veriyor; gece kalanlar kayboluyor.
```

### 11.2 Gündüz tehlikesi ve istihbarat
```text
[Gün 11 · 11.2] Gündüz gezginleri ve istihbarat
Modül: Waves, Enemies, UI

Yap:
1. Waves/DayWanderers:
   - Gündüz başında dış halkada 3–6 Walker "gezgin" modda doğar: üsse gitmez, NavMesh üstünde rastgele dolaşır.
   - Oyuncu aggro yarıçapına girerse saldırır.
   - Gece başlayınca normal moda geçer. Enemy'ye Wander durumu ekle.
2. İstihbarat (Day evresinde, IWaveService.GetTonightInfo):
   - Ekran kenarında saldırı yönü okları.
   - Sağ üst panelde düşman tipi + adet listesi ve gece olayı.
   - Aktif SpawnZone'larda kırmızı ışık sütunu.
3. Son 15 sn: büyük geri sayım + ekran kenarında kırmızı nabız (DayEndingSoonEvent).
4. HUD'da "Hazırım (Enter) — +X bonus" ipucu.

Kabul: Gündüz kuzey/doğu okları ve "20 Yürüyen" listesi görünüyor; dışarıda gezgin zombilerle karşılaşılıyor; son 15 sn uyarısı çalışıyor.
```

### 11.3 Ara kontrol (yeni özellik yok)
```text
[Gün 11 · 11.3] Mimari ara kontrol
Yeni özellik ekleme.

1. Tüm kodu CLAUDE.md kurallarına göre tara:
   - asmdef bağımlılık ihlalleri
   - magic number
   - Update içinde GetComponent / alloc / LINQ
   - havuz dışı Instantiate
   - abonelikten çıkılmayan olaylar
   - 200 satırı geçen sınıflar
   - Core'da biriken gereksiz şeyler
2. Validate Project'i genişlet:
   - null referanslı zorunlu SO alanları
   - Visual alt objesi olmayan prefab'lar
   - Build Settings'te olmayan sahneler
   - EnemyData prefab bağlantıları
3. Bulguları önce önem sırasıyla listele, sonra kritik ve orta olanları düzelt. Düzeltmediklerini PROGRESS.md "Bilinen hatalar / TODO"ya yaz.

Kabul: Testler yeşil, Validate Project temiz (veya bilinen maddeler listede).
```

---

## Gün 12 — İçerik: yeni düşmanlar, boss, gece olayları

### 12.1 Tükürücü ve Patlayan
```text
[Gün 12 · 12.1] Tükürücü ve Patlayan
Modül: Core, Enemies, Editor

Yap:
1. Saldırıyı strateji olarak ayır:
   - IEnemyAttack: mevcut kod MeleeAttack olur; yanına RangedAttack ve SuicideExplodeAttack.
   - EnemyData hangi saldırıyı kullanacağını [SerializeReference] ile belirtsin.
2. Core/Combat/AreaDamage: alan hasarı yardımcısı. NonAlloc kullanır; yapılara çarpan uygular; mesafeyle azalan hasar opsiyonel.
3. Tükürücü:
   - Menzilde durur, yay çizen havuzlanmış asit mermisi atar (EnemyProjectile layer). Çarptığı IDamageable'a hasar verir.
   - Tercih ettiği hedef taret.
4. Patlayan:
   - Yapıya veya oyuncuya değince ya da öldürülünce GDD §7 alan hasarı verir.
   - Yakındaki Patlayanları 0.15 sn gecikmeyle zincirleme tetikler.
   - Yürürken Visual nabız gibi büyüyüp küçülür.
5. Generator: Spitter + Exploder data ve prefab. NightData'lardaki eksik grupları GDD §8'e göre doldur.

Kabul: Gece 4'te Tükürücüler taretlere uzaktan ateş ediyor; Gece 6'da Patlayanlar barikatta zincirleme patlıyor (F5 ile geceleri atlayarak test et).
```

### 12.2 Boss: Brute
```text
[Gün 12 · 12.2] Boss: Brute
Modül: Core (olaylar), Enemies, UI, Editor

Yap:
1. Enemies/BossBehaviour (Enemy'ye ek bileşen), GDD §7:
   - Her 6 sn'de bir yere vurma: önce 1 sn yerde kırmızı uyarı dairesi, sonra AreaDamage + CameraShakeRequestedEvent.
   - %50 can altında öfke: hız ×1.4, renk koyulaşır.
   - Geri itilmez.
2. Olaylar: BossSpawnedEvent, BossHealthChangedEvent, BossDefeatedEvent.
3. HUD: ekranın üstünde boss adı + büyük can barı.
4. Generator: Brute (3000) ve Brute_Final (6000) data ve prefab'ları; Gece 5 ve 10'a bağla.

Kabul: Gece 5'te boss geliyor, uyarı dairesi görünüyor, darbesi alan hasarı veriyor, %50'de hızlanıyor; ölünce bar kayboluyor.
```

### 12.3 Gece olayları
```text
[Gün 12 · 12.3] Gece olayları
Modül: Core (olaylar), Waves, GameFlow, Economy, Building, Editor

Yap:
1. NightModifierData SO: id, displayName, description, enemyCountMultiplier, coinMultiplier, enemySpeedMultiplier, fog override, light color override, periyodik güç kesintisi (interval, duration).
2. Waves/NightModifierController:
   - Gece başında NightModifierActivatedEvent (tüm veriyi taşır), gece bitince NightModifierEndedEvent.
   - Düşman sayısı ve hız çarpanlarını WaveRunner/spawn tarafında uygular.
3. Dinleyiciler:
   - GameFlow/Lighting: fog ve ışık rengi.
   - Economy: jeton çarpanı, IRunModifiers'a geçici modifier (gece bitince kaldırılır).
   - Building/Generator: Fırtına güç kesintileri.
4. Fırtına: şimşek flaşı (directional light anlık parlama) + hafif CameraShakeRequestedEvent.
5. Generator: Kan Ayı, Sis, Fırtına (GDD §14); NightData'lara bağla. İstihbarat panelinde olay adı ve açıklaması görünsün.

Kabul: Gece 4 sisli, Gece 5 kırmızı ve jetonlar ×2, Gece 7'de periyodik elektrik kesintisi ve şimşek.
```

---

## Gün 13 — Perk'ler · 🏁 M4

### 13.1 XP ve seviye
```text
[Gün 13 · 13.1] XP ve seviye
Modül: Core (olaylar), Progression, UI

Yap:
1. ProgressionConfig SO: seviye eşiği formülü (GDD §13), nadirlik ağırlıkları.
2. ExperienceTracker (saf C# + köprü):
   - EnemyKilledEvent'ten XP alır.
   - Aynı anda birden fazla seviye atlanabilir; bekleyen seviyeler kuyruğa girer.
   - XpChangedEvent, LevelUpEvent.
3. HUD: ekranın altında ince XP barı + seviye numarası.
4. EditMode testleri: eşik formülü, çoklu seviye atlama.

Kabul: Zombi öldürdükçe XP barı doluyor, seviye atlayınca LevelUpEvent logu.
```

### 13.2 Perk sistemi
```text
[Gün 13 · 13.2] Perk sistemi
Modül: Progression, Combat, Player, Building, Enemies, Editor

Yap:
1. PerkData SO: id, displayName, description, rarity, maxStacks, opsiyonel icon, efekt listesi ([SerializeReference]):
   - StatModifierEffect (StatType, Flat/Percent, değer)
   - FlagEffect (PerkFlag)
   - HealEffect
2. PerkService:
   - Aktif perk'leri ve yığın sayılarını tutar.
   - ApplyPerk → IRunModifiers'a uygular, PerkAcquiredEvent atar.
3. PerkOfferGenerator (saf C#, seed'lenebilir): ağırlıklı rastgele 3 farklı perk seçer, maks yığına ulaşanları dışarıda bırakır.
4. Flag kancalarını tamamla:
   - Ricochet (Combat): ikinci en yakın düşmana %50 hasar.
   - Vampire (Player): öldürmede +2 can.
   - ThornWalls (Building): barikat yakın dövüş hasarının %30'unu yansıtır.
   - ShockTurret (Building): taret vuruşu yavaşlatır.
   - ChainExplosion (Enemies): oyuncunun öldürdüğü düşman %10 şansla AreaDamage.
   - Hunter: 9.3'te hazır, doğrula.
5. Generator: GDD §13'teki 14 perk.
6. EditMode testleri: PerkOfferGenerator, yığın limiti, stat uygulaması.

Kabul: Testler yeşil; DebugTools'a geçici "rastgele perk ver" tuşu (F10) ile etkiler oyunda gözlemleniyor.
```

### 13.3 Seviye kartları arayüzü
```text
[Gün 13 · 13.3] Seviye kartları
Modül: Core, UI

Yap:
1. Core: IPauseService (Pause(object source), Resume(object source), IsPaused; birden fazla kaynak desteklenir, Time.timeScale'i yönetir) + implementasyon.
2. LevelUpEvent geldiğinde:
   - Oyun duraklar, 3 kart çıkar: nadirlik renginde çerçeve, ad, açıklama, mevcut yığın "2/3".
   - Seçim tıklama veya 1–3 tuşları ile; kısa animasyondan sonra devam eder.
   - Kuyrukta başka seviye varsa sıradaki kartlar gelir.
3. HUD sol alt: edinilen perk'lerin küçük listesi (ikon yoksa baş harf + nadirlik rengi), üzerine gelince açıklama.

Kabul: Seviye atlayınca oyun donuyor, kart seçilince devam ediyor; perk etkisi hissediliyor; perk listesi HUD'da.
```

> 🏁 **M4 — İçerik tamam:** 10 gece baştan sona oynanabilir. Tam bir run oynayıp süreyi ve zorluğu not et.

---

## Gün 14 — Oyun hissi, ses, menüler

### 14.1 Oyun hissi (juice)
```text
[Gün 14 · 14.1] Oyun hissi
Modül: Player (kamera), Feedback, UI

Yap:
1. ICameraService.Shake: CinemachineImpulseSource + kamerada CinemachineImpulseListener. CameraShakeRequestedEvent'i dinlesin. Ayarlarda kapatılabilsin.
2. FeedbackConfig SO + FeedbackDirector (olay → efekt eşlemesi):
   - Atış: minik sarsıntı; pompalı: orta.
   - Normal ölüm: küçük sarsıntı. Tank/boss ölümü: büyük sarsıntı + 0.06 sn hit-stop (IPauseService ile çakışmasın).
   - Patlama: sarsıntı.
   - Oyuncu hasarı: ekran kenarında kırmızı flaş.
3. UI: hasar sayıları (ShotHitEvent; havuzlanmış etiketler, dünyadan ekrana yansıtılır, yüksek hasar daha büyük) ve jeton "+12" altın yazıları.
4. Düşük can (<%30): Volume vignette nabzı.
5. Evre geçişlerinde ekran ortasında büyük başlık ("GECE 3", "ŞAFAK", "BOSS") fade animasyonu.

Kabul: Oyun belirgin şekilde daha "tok" hissediyor; ayarlardan sarsıntı kapatılabiliyor (ayarlar ekranı 14.3'te, şimdilik config bayrağı).
```

### 14.2 Ses altyapısı
```text
[Gün 14 · 14.2] Ses altyapısı
Modül: Core (arayüz), Audio, Editor

Yap:
1. SoundData SO: klip listesi (rastgele seçilir), volume, pitch aralığı, spatial (2D/3D), min/max mesafe, maxSimultaneous, cooldown.
2. AudioService:
   - Core'a IAudioService: PlaySfx(SoundData, pos), PlayUi(SoundData).
   - Havuzlanmış AudioSource'lar.
   - AudioMixer gruplarını (Master/Music/SFX) kullanır; mixer asset'i editör aracıyla üretilsin.
3. AudioEventMap SO + AudioEventBinder: olay → SoundData eşlemesi.
   - Kapsanacak olaylar: WeaponFired (silaha göre), ShotHit, EnemyKilled, CoinCollected, PurchaseCompleted, StructurePlaced/Destroyed, PowerLost, DayEndingSoon, BossSpawned, LevelUp, PhaseChanged, UI tıklama.
   - Klip yoksa sessizce atla.
4. MusicController: evre bazlı müzik (Day, Night, Boss, GameOver, Victory), crossfade ile.
5. Generator: tüm SoundData'ları boş kliple üret, AudioEventMap'i doldur.

Kabul: Derleme temiz, Validate Project boş klipleri "uyarı" olarak listeliyor (hata değil). Test için bir SoundData'ya herhangi bir klip sürüklenince çalıyor.
```

### 14.3 Menüler ve sahne akışı
```text
[Gün 14 · 14.3] Menüler ve sahne akışı
Modül: Core (arayüz), UI, GameFlow, Editor

Yap:
1. Editor: Build Main Menu Scene → Scenes/MainMenu.unity. İçerik: Oyna, Ayarlar, Çıkış; arka planda yavaş dönen kamera ve birkaç placeholder zombi.
2. Pause menüsü (Esc): Devam, Ayarlar, Ana Menü. IPauseService kullanır.
3. Ayarlar:
   - Genel/Müzik/SFX ses, kamera preset'i (İzometrik/Tepeden), ekran sarsıntısı, tam ekran.
   - Core'a ISettingsService; PlayerPrefs ile kaydedilir, değişince SettingsChangedEvent.
4. Oyun Bitti ve Zafer ekranlarını tamamla: ulaşılan gece, öldürme, kazanılan para, en yüksek kombo, alınan perk'ler. Butonlar: Tekrar Oyna, Ana Menü.
5. Build Settings sırası: MainMenu, Game.

Kabul: MainMenu → Oyna → oyun → Esc menüsü → Ana Menü dönüşü sorunsuz; ayarlar yeniden açılışta korunuyor.
```

---

## Gün 15 — Denge, performans, build

### 15.1 Debug paneli ve denge tablosu
```text
[Gün 15 · 15.1] Debug paneli ve denge tablosu
Modül: DebugTools, Editor

Yap:
1. Tüm debug kısayollarını tek bir panelde topla (F12 aç/kapa, sadece Editor veya Development build). İçerik:
   - +para, +yakıt, ölümsüzlük
   - geceyi atla, N. geceye git
   - tüm düşmanları öldür, perk ver
   - zaman ölçeği 0.5× / 1× / 2× / 4×
   - FPS ve canlı düşman sayısı
2. Editor: Tools/Last Haven/Export Balance Sheet → tüm SO değerlerini Docs/BALANCE.md'ye tablolar halinde yazar. Gece başına toplam düşman canı ve toplam para ödülü hesaplanmış olsun.
3. BALANCE.md'yi oku ve kısa bir analiz ekle:
   - gece başına kazanılabilir para ile gereken savunma maliyeti
   - zorluk eğrisi
   - riskli noktalar
   Önerilerini yaz, değerleri DEĞİŞTİRME.

Kabul: Panel çalışıyor; BALANCE.md tablolar ve önerilerle oluştu.
```

### 15.2 Performans
```text
[Gün 15 · 15.2] Performans
Modül: Tümü

Yap:
1. DebugTools'a stres testi: 200 düşman + 6 taret (tavanı bu test için devre dışı bırak).
2. Kod taraması:
   - Update içinde GC allocation (LINQ, string, closure, boxing, GetComponent)
   - NonAlloc kullanılmayan fizik sorguları
   - aynı frame'e yığılan hedef hesaplamaları
   - her frame güncellenen UI
   - gereksiz FindNearest çağrıları
3. Bulguları düzelt.
4. Bana Profiler'da neye bakacağımı 5 maddede anlat (CPU, GC Alloc, NavMesh, Physics, Rendering).

Kabul: Stres testinde editörde stabil FPS ve oynanış sırasında frame başına GC Alloc ≈ 0.
```

### 15.3 İlk build ve hata temizliği
```text
[Gün 15 · 15.3] İlk build ve hata temizliği
Modül: Tümü, Editor

Yap:
1. PROGRESS.md'deki "Bilinen hatalar / TODO" listesini gözden geçir; oynanışı etkileyenleri düzelt.
2. Player Settings ayarları editör aracıyla yapılsın: ürün adı, şirket adı, versiyon 0.1.0, varsayılan pencere modu.
3. Tools/Last Haven/Build/Windows ve .../macOS menüleri: BuildPipeline ile Builds/<platform>/ klasörüne. Builds/ .gitignore'da olsun.
4. Docs/QA_CHECKLIST.md: 1. geceden 10. geceye uçtan uca test edilmesi gereken ~25 madde.

Kabul: Build alınıyor ve build'de oyun açılıp oynanıyor (debug paneli build'de kapalı).
```

---

## Gün 16 — Asset hazırlığı · 🏁 M5

### 16.1 Asset entegrasyon altyapısı
```text
[Gün 16 · 16.1] Asset entegrasyon araçları
Modül: Core, Editor

Yap:
1. AnimationDriver'ı gözden geçir. Standart parametreler AnimParams'ta olsun:
   - Speed (float), IsDashing (bool)
   - Attack, Hit, Die, Fire, Reload (trigger)
   Animator yoksa prosedürel yedek çalışmaya devam etsin.
2. Tools/Last Haven/Art/Create Animator Controllers:
   - Player, Enemy ve Boss için bu parametreleri ve boş state'leri içeren AnimatorController'ları Art/Animations altında üretir: Locomotion blend tree (Idle/Walk/Run), Attack, Hit, Die.
   - Varsa dokunmaz. Kullanıcı sadece klipleri sürükleyecek.
3. Tools/Last Haven/Art/Swap Visual (EditorWindow):
   - Prefab ve model (FBX veya prefab) seçilir.
   - Visual altındaki placeholder kaldırılır, model konur.
   - Collider yüksekliğine göre ölçek önerir.
   - Animator varsa ilgili controller'ı bağlar.
   - Muzzle gibi bağlantı noktalarını korur.
   - Undo destekler.
4. Hit flash'ın farklı shader'larda güvenli çalıştığını doğrula.

Kabul: Herhangi bir ücretsiz test modeliyle Swap Visual → oyun çalışmaya devam ediyor.
```

### 16.2 Asset rehberi
```text
[Gün 16 · 16.2] Asset rehberi
Docs/ASSET_GUIDE.md oluştur:

1. İhtiyaç listesi tablosu. Her model, animasyon, ses, müzik, VFX ve UI ikonu için: ad, kullanıldığı yer, bağlanacağı prefab/SO alanı, önerilen format/boyut, öncelik (MVP / sonra).
2. Import ayarları:
   - ölçek (1 birim = 1 m, karakter ~1.8 m)
   - Rig (Humanoid / Generic)
   - Mixamo animasyonları için adımlar
   - Loop Time
   - Root Motion kapalı (hareket kodla yapılıyor)
3. Klasör ve isimlendirme kuralları (Art/Models/Enemies/Walker/...).
4. Git LFS: .gitattributes'taki satırları açma, `git lfs install`, ekip kuralları.
5. Adım adım iki örnek: "Yeni zombi modelini ekleme" ve "Silah sesini bağlama".
6. Uygun kaynak türleri (Unity Asset Store, Mixamo, Kenney, Quaternius vb.) ve lisans kontrolü uyarısı.

Ayrıca .gitattributes'taki LFS satırlarını aç.
```

> ✋ **Manuel aşama:** `ASSET_GUIDE.md`'yi takip ederek modelleri, animasyonları ve sesleri ekle.

### 16.3 Asset sonrası kontrol (asset'leri ekledikten sonra gönder)
```text
[Gün 16 · 16.3] Asset sonrası kontrol
Asset'leri ekledim.

1. Validate Project'i genişlet:
   - klibi boş SoundData'lar
   - Animator'u olup controller'ı olmayan prefab'lar
   - Visual'ında hâlâ placeholder olanlar
   - bounds'u anormal (çok büyük/küçük) modeller
   - pembe/eksik materyaller
   - LFS'e girmemiş büyük dosyalar
2. Ben menüyü çalıştırınca Docs/VALIDATION_REPORT.md'yi oku.
3. Bana öncelik sırasına göre yapılacaklar listesi ver. Kodla çözülebilenleri düzelt.
```

> 🏁 **M5:** Asset'li, cilalı, build alınabilen ilk sürüm. 🎉

---

## Ekiple paralel çalışma

İlk 6 gün **tek kişi** ile ilerlemek en güvenlisi; temel mimari oturmalı. M2'den sonra modül sahipliğiyle iş paralelleşir:

| Kişi | Görevler | Modül |
|---|---|---|
| 1 | 7.1–7.3, 10.1, 11.1 | Economy |
| 2 | 8.1–9.3 | Building |
| 3 | 11.2, 12.1–12.3 | Enemies / Waves |
| 4 | 13.1–13.2, 14.2 | Progression / Audio |
| 5 | 10.2–10.3, 13.3, 14.1, 14.3 | UI / Feedback |

**Çakışmayı önleyen kurallar** (CLAUDE.md bunları zaten zorunlu kılıyor):

- Herkes kendi modül klasöründe çalışır. Olaylar Core'da **alan bazlı ayrı dosyalarda** tutulur.
- Üreticiler modül başına ayrı dosyadadır (`<Modül>Generator.cs`). Merkezi menü dosyasına kimse dokunmaz.
- `Game.unity` üreticiyle kurulur. Merge çakışmasında kendi sürümünü al ve `Build Game Scene`'i yeniden çalıştır. `[Environment]` köküne sadece bir kişi (art sorumlusu) dokunur.
- Core'a yeni arayüz veya olay ekleyen kişi bunu ekip kanalında duyurur. Core değişiklikleri küçük PR'larla yapılır.
- Herkes kendi branch'inde çalışır: `feature/<gün.görev>-kısa-ad`. Merge'den önce testler yeşil ve `Validate Project` temiz olmalı.
- Herkes kendi makinesinde kendi Unity'si, MCP'si ve agent'ıyla çalışır. MCP paketinin sürümü `Packages/manifest.json`'da sabit bir etiketle durur; güncelleme kararı ekipçe verilir.
- Merge'den sonra agent'a "Rebuild All'ı çalıştır, testleri koş, duman testi yap" demek, entegrasyonu hızlıca doğrular.

## Sonrası (v2 fikirleri)

GDD §19'daki fikirler: NPC hayatta kalanlar, karakter sınıfları, kalıcı ilerleme, ses/gürültü sistemi, yeni zombi tipleri, sonsuz mod, co-op. Her biri için aynı kalıpla yeni bir "Gün" planı yazdırabilirsin:

```text
GDD §19'daki "<özellik>" için Docs/BUILD_PLAN.md formatında 3 görevlik yeni bir gün planı yaz: her görev tek modüle odaklı, kabul kriterli ve CLAUDE.md kurallarına uygun olsun. Önce GDD'ye ilgili tasarım bölümünü ekle.
```
