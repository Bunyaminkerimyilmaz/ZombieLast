# Ses Asset Listesi

> Last Haven: Day & Night'ın ihtiyaç duyduğu tüm ses efektleri ve müzikler. Kaynaklar: GDD §18, `Editor/Generators/AudioDefinitions.cs` (üretilen `SoundData`'lar), `Core/Events/` (sesin bağlanabileceği oyun olayları). 16.2'deki `ASSET_GUIDE.md` bu listeyi temel alabilir.

## Nasıl okunur

| Durum | Anlamı | Klibi ekleyince ne olur |
|---|---|---|
| ✅ **Hazır** | `SoundData` asset'i var, olaya bağlı, klip listesi boş | Klibi `Data/Audio/...` asset'inin **Clips** listesine sürükle, hemen çalar |
| 🔌 **Olay var** | Oyun olayı zaten gönderiliyor ama `AudioEventMap`'te satırı yok | Kod gerekir: `AudioDefinitions` + `AudioEventMap`'e satır (küçük iş) |
| 🛠 **Kanca yok** | Ne olay ne ses alanı var | Kod gerekir: yeni olay veya bileşende `SoundData` alanı |

- **Öncelik:** **MVP** = oyunun "okunaklı kaos"u (GDD §2) için şart. **Sonra** = cila.
- **Varyasyon:** Önerilen klip sayısı. `SoundData` her çalışta listeden rastgele birini seçer. Sık çalan seslerde 3–5 varyasyon tekrar hissini kırar.
- **2D / 3D:** 3D sesler olayın konumundan duyulur (2 m'ye kadar tam ses, 45 m'de sıfır; dinleyici oyuncunun üstünde). 2D sesler her yerden aynı duyulur.

**Toplam: 109 asset.**

| | Hazır ✅ | Olay var 🔌 | Kanca yok 🛠 |
|---|---|---|---|
| Müzik | 5 | — | 1 |
| Efekt / ambiyans | 21 | 34 | 48 |
| **MVP** | 26 | 15 | 20 |

MVP toplamı **61**. Bunların 26'sı bugün kod değişikliği olmadan sadece klip sürükleyerek çalışır.

---

## 1. Müzik

| Asset | Durum | Ne zaman çalar | Ruh hali / içerik | Süre | Döngü | Öncelik |
|---|---|---|---|---|---|---|
| `Music/Music_Day` | ✅ | Gündüz ve şafak | Sakin, hafif melankolik; az perküsyon | 1.5–3 dk | Evet (dikişsiz) | MVP |
| `Music/Music_Night` | ✅ | Alacakaranlık + gece (boss yokken) | Gerilim, sürükleyici ritim, düşük synth / davul | 2–3 dk | Evet | MVP |
| `Music/Music_Boss` | ✅ | Gecede boss yaşarken (5. ve 10. gece) | Ağır, epik, yoğun perküsyon | 1.5–2 dk | Evet | MVP |
| `Music/Music_GameOver` | ✅ | Jeneratör yıkılınca | Kısa, kasvetli kapanış (stinger) | 5–15 sn | Hayır | MVP |
| `Music/Music_Victory` | ✅ | 10. gece bitince (tahliye) | Rahatlama, umut; zafer teması | 10–30 sn | Hayır | MVP |
| `Music_MainMenu` | 🛠 | Ana menü | Ana temanın sakin hali | 1–2 dk | Evet | Sonra (GDD'de yok) |

Müzikler arasında 1.5 sn crossfade var. Döngüler baştan sona tıkırtısız birleşmeli.

---

## 2. Hazır efektler (✅, sadece klip ekle)

Hepsi `Assets/_Project/Data/Audio/Sfx/` altında.

### Silah ve çatışma

| Asset | Ne zaman çalar | 2D/3D | Önerilen içerik | Varyasyon | Süre |
|---|---|---|---|---|---|
| `Sfx_WeaponFired_Pistol` | Tabanca ateşi | 3D | Kuru, keskin tabanca atışı | 3–4 | 0.2–0.5 sn |
| `Sfx_WeaponFired_Shotgun` | Pompalı ateşi | 3D | Gür pompalı patlaması + kısa pompa sesi | 2–3 | 0.5–1 sn |
| `Sfx_WeaponFired_Rifle` | Tüfek ateşi (10/sn) | 3D | Kısa, sert otomatik tüfek atışı; kuyruğu kısa olmalı | 4–5 | 0.15–0.3 sn |
| `Sfx_WeaponFired_Turret` | Taret ateşi | 3D | Mekanik, hafif elektronik otomatik atış | 3 | 0.15–0.3 sn |
| `Sfx_WeaponFired_Default` | Listede olmayan silah (yedek) | 3D | Genel atış | 1–2 | 0.2–0.4 sn |
| `Sfx_ShotHit_Flesh` | Mermi düşmana/oyuncuya isabet | 3D | Islak, boğuk vuruş | 4–5 | 0.1–0.3 sn |
| `Sfx_ShotHit_Surface` | Mermi yapıya/zemine/engele isabet | 3D | Sekme, beton/metal tıkırtısı | 3–4 | 0.1–0.4 sn |
| `Sfx_EnemyKilled` | Herhangi bir düşman ölümü | 3D | Zombi ölüm hırıltısı + düşme | 4–5 | 0.5–1.2 sn |

### Ekonomi ve inşa

| Asset | Ne zaman çalar | 2D/3D | Önerilen içerik | Varyasyon | Süre |
|---|---|---|---|---|---|
| `Sfx_CoinCollected` | Jeton toplandı | 2D | Parlak metalik "ding" (perde zaten rastgele) | 2–3 | 0.1–0.3 sn |
| `Sfx_PurchaseCompleted` | Market alımı | 2D | Yazar kasa / para sesi | 1–2 | 0.3–0.8 sn |
| `Sfx_StructurePlaced` | Yapı yerleştirildi | 3D | Ahşap/metal yere oturma, çekiç | 2–3 | 0.3–0.8 sn |
| `Sfx_StructureDestroyed` | Yapı yıkıldı | 3D | Kırılma, çökme | 2–3 | 0.8–1.5 sn |

### Duyurular (2D, tek kopya)

| Asset | Ne zaman çalar | Önerilen içerik | Süre |
|---|---|---|---|
| `Sfx_BossSpawned` | Boss doğdu | Derin kükreme + gong / bas vuruşu | 2–4 sn |
| `Sfx_PowerLost` | Güç kesildi (yakıt bitti, Fırtına kesintisi, jeneratör yıkıldı) | Elektrik sönme "vııın…" + röle tıkı | 1–2 sn |
| `Sfx_DayEndingSoon` | Gündüzün son 15 saniyesi başladı | Uyarı sireni / alarm çanı | 1–2 sn |
| `Sfx_LevelUp` | Seviye atlandı (kartlar açılırken) | Yükselen, ödüllendirici arpej | 1–1.5 sn |
| `Sfx_Phase_Day` | Gündüz başladı | Horoz / sabah teması, kısa stinger | 1–3 sn |
| `Sfx_Phase_Dusk` | Alacakaranlık (4 sn) | Uğursuz yükselen gerilim | 2–4 sn |
| `Sfx_Phase_Night` | Gece başladı | Uzaktan zombi uluması + davul vuruşu | 2–4 sn |
| `Sfx_Phase_Dawn` | Şafak (gece atlatıldı) | Rahatlama akoru | 2–3 sn |
| `Ui_Click` | Her buton tıklaması (fare, Enter, gamepad A) | Kısa, yumuşak tık | 0.05–0.15 sn |

---

## 3. Olayı olan ama bağlanmamış efektler (🔌)

Önerilen asset adları `Data/Audio/Sfx/` kalıbında. Kod tarafında sadece eşleme satırı gerekir.

| Önerilen asset | Olay | Ne zaman | 2D/3D | Önerilen içerik | Öncelik |
|---|---|---|---|---|---|
| `Sfx_Explosion` | `ExplosionEvent` | Patlayan zombi, Zincirleme Son perk'i | 3D | Islak, organik patlama + bas | **MVP** |
| `Sfx_Reload_Pistol` / `_Shotgun` / `_Rifle` | `ReloadStartedEvent` | Şarjör değiştirme başladı | 2D | Şarjör çıkarma (pompalı: fişek sürme) | **MVP** |
| `Sfx_ReloadFinished` | `ReloadFinishedEvent` | Şarjör takıldı | 2D | Sürgü çekme "çak" | **MVP** |
| `Sfx_WeaponSwitch` | `WeaponChangedEvent` | 1/2/3 ile silah değişimi | 2D | Kumaş + metal tıkırtısı | **MVP** |
| `Sfx_PlayerHurt` | `PlayerDamagedEvent` | Oyuncu hasar aldı | 2D | Kısa nefes/inleme | **MVP** |
| `Sfx_PlayerDied` | `PlayerDiedEvent` | Oyuncu öldü | 2D | Düşüş + boğuk efekt | **MVP** |
| `Sfx_GeneratorHit` | `DamagedEvent` (hedef jeneratör) | Jeneratör vuruluyor | 3D | Metal gümleme + kıvılcım | **MVP** |
| `Sfx_GeneratorDestroyed` | `GeneratorDestroyedEvent` | Jeneratör yıkıldı | 2D | Büyük patlama + elektrik çatırtısı | **MVP** |
| `Sfx_BossDefeated` | `BossDefeatedEvent` | Boss öldü | 2D | Ağır devrilme + zafer vuruşu | **MVP** |
| `Sfx_DayCountdownTick` | `DayTimerTickEvent` (son 15 sn) | Geri sayımda saniyede bir | 2D | Saat tiki, son 5'te daha sert | **MVP** |
| `Sfx_StructureHit_Wood` / `_Metal` / `_Concrete` | `DamagedEvent` (hedef yapı) | Zombi barikata vuruyor (seviyeye göre) | 3D | Ahşap çatırtı / metal gong / beton gümbürtü | **MVP** |
| `Sfx_PowerRestored` | `PowerChangedEvent` (true) | Güç geri geldi | 2D | Jeneratör çalışma + ışık "klik" | Sonra |
| `Sfx_StructureUpgraded` | `StructureUpgradedEvent` | Barikat/taret yükseltildi | 3D | Çekiç + metal takma, parlak sonlu | Sonra |
| `Sfx_StructureSold` | `StructureRemovedEvent` (Sold) | Yapı satıldı | 3D | Söküm + bozuk para | Sonra |
| `Sfx_LootCrateOpened` | `LootCollectedEvent` | Sandık açıldı | 3D | Kilit + kapak açılması; uzak sandıkta daha zengin ses | Sonra |
| `Sfx_AmmoPickedUp` | `AmmoPickedUpEvent` | Mermi paketi alındı | 2D | Fişek şıkırtısı | Sonra |
| `Sfx_PlayerRespawned` | `PlayerRespawnedEvent` | 10 sn sonra yeniden doğdu | 2D | Kısa "yeniden başlama" tonu | Sonra |
| `Sfx_FuelAdded` | `FuelChangedEvent` (Delta > 0) | Yakıt alındı (market, sandık, acil yakıt) | 2D | Bidon çalkalanması / dökme | Sonra |
| `Sfx_InterestPaid` | `InterestPaidEvent` | Şafakta faiz | 2D | Bozuk para yığını | Sonra |
| `Sfx_ComboTierUp` | `ComboChangedEvent` (çarpan arttı) | ×1.5 / ×2 / ×3'e çıkış | 2D | Yükselen "ding" (her kademe bir perde üstte) | Sonra |
| `Sfx_ComboLost` | `ComboChangedEvent` (sıfırlandı) | Kombo bitti | 2D | Düşen kısa ton | Sonra |
| `Sfx_EarlyReady` | `EarlyReadyEvent` | "Hazırım" ile erken başlatma | 2D | Kararlı "onay" + bonus parası | Sonra |
| `Sfx_NightEvent_BloodMoon` | `NightModifierActivatedEvent` | Kan Ayı başladı | 2D | Derin drone + kalp atışı | Sonra |
| `Sfx_NightEvent_Fog` | `NightModifierActivatedEvent` | Sis başladı | 2D | Boğuk rüzgâr süpürmesi | Sonra |
| `Sfx_NightEvent_Storm` | `NightModifierActivatedEvent` | Fırtına başladı | 2D | Gök gürültüsü | Sonra |
| `Sfx_PerkSelected` | `PerkAcquiredEvent` | Perk kartı seçildi | 2D | Kart çevirme + nadirliğe göre parlaklık | Sonra |
| `Sfx_Heal` | `HealRequestedEvent` | Can paketi alındı | 2D | Yumuşak iyileşme tonu | Sonra |
| `Sfx_BuildModeEnter` / `_Exit` | `BuildModeChangedEvent` | B ile inşa moduna giriş/çıkış | 2D | Plan kâğıdı / alet çantası | Sonra |
| `Ui_BuildSelect` | `BuildSelectionChangedEvent` | İnşa çubuğunda yapı seçimi | 2D | Hafif tık | Sonra |

---

## 4. Kancası olmayan efektler ve ambiyanslar (🛠)

### Düşmanlar

`EnemyData`'da kullanılmayan `_attackSound` / `_deathSound` (AudioClip) alanları var. Bu sesler için alan tipi `SoundData`'ya çevrilmeli. Tipe göre ölüm sesi bağlanırsa genel `Sfx_EnemyKilled` yerine bunlar çalar.

| Önerilen asset | Ne zaman | 2D/3D | Önerilen içerik | Öncelik |
|---|---|---|---|---|
| `Zmb_Walker_Idle` | Yürüyen, ara ara | 3D | Yavaş inilti/hırıltı (5–6 varyasyon) | **MVP** |
| `Zmb_Walker_Attack` | Yakın dövüş vuruşu | 3D | Hırlama + pençe | **MVP** |
| `Zmb_Walker_Death` | Ölüm | 3D | Boğuk hırıltı | Sonra |
| `Zmb_Runner_Idle` | Koşucu, ara ara | 3D | Tiz çığlık, hızlı soluma | **MVP** |
| `Zmb_Runner_Attack` | Vuruş | 3D | Kısa çığlıklı saldırı | Sonra |
| `Zmb_Runner_Death` | Ölüm | 3D | Kesik çığlık | Sonra |
| `Zmb_Tank_Idle` | Tank, ara ara | 3D | Derin böğürme | **MVP** |
| `Zmb_Tank_Attack` | Vuruş (yapıya ×2) | 3D | Ağır yumruk + gümbürtü | **MVP** |
| `Zmb_Tank_Footstep` | Yürürken | 3D | Ağır adım, yer titremesi | Sonra |
| `Zmb_Tank_Death` | Ölüm | 3D | Uzun kükreme + ağır düşüş | Sonra |
| `Zmb_Spitter_Spit` | Tükürük fırlatma (`RangedAttack`) | 3D | Gırtlaktan tükürme | **MVP** |
| `Zmb_Spitter_Impact` | Mermi yere/hedefe değdi (`ArcProjectile`) | 3D | Asidik "fısss" sıçrama | **MVP** |
| `Zmb_Spitter_Idle` | Ara ara | 3D | Gurultulu boğaz sesi | Sonra |
| `Zmb_Exploder_Fuse` | Patlayan yürürken (nabız ile senkron) | 3D, döngü | Fokurdama / şişme sesi | **MVP** |
| `Boss_Brute_SlamWarning` | Vuruştan 1 sn önce, yerde uyarı dairesi çıkınca (`GroundSlamAttack`) | 3D | Yükselen hırlama / nefes alma | **MVP** |
| `Boss_Brute_SlamImpact` | Yer darbesi (4 m alan) | 3D | Çok ağır gümbürtü, yer çatlaması | **MVP** |
| `Boss_Brute_Rage` | %50 can altında öfke (`BossRage`) | 2D | Uzun öfke kükremesi | **MVP** |
| `Boss_Brute_Footstep` | Yürürken | 3D | Dev adım | Sonra |
| `Boss_Brute_Death` | Ölüm | 3D | Ağır devrilme (`Sfx_BossDefeated` ile birlikte) | Sonra |

### Oyuncu

| Önerilen asset | Ne zaman | 2D/3D | Önerilen içerik | Öncelik |
|---|---|---|---|---|
| `Sfx_Dash` | Atılma (`DashModel`) | 2D | Hızlı "vuş" | **MVP** |
| `Sfx_DryFire` | Şarjör boşken tetik | 2D | Boş tetik "tık" | **MVP** |
| `Sfx_PlayerFootstep` | Yürürken | 3D | Toprak/beton adım (4–6 varyasyon) | Sonra |
| `Sfx_Flashlight` | Gece fener açılınca (`Flashlight`) | 2D | Düğme tıkı | Sonra |

### Yapılar ve üs

| Önerilen asset | Ne zaman | 2D/3D | Önerilen içerik | Öncelik |
|---|---|---|---|---|
| `Sfx_RepairLoop` | E ile onarım sürerken (`StructureRepair`) | 3D, döngü | Çekiç / kaynak | **MVP** |
| `Sfx_GeneratorHum` | Güç varken sürekli (`[Gameplay]` jeneratör) | 3D, döngü | Motor uğultusu; güç kesilince durur | **MVP** |
| `Sfx_SpikeTrapTick` | Tuzak hasar tiki, 0.5 sn'de bir (`TrapTickTimer`) | 3D | Kısa metal batma | **MVP** |
| `Sfx_SpikeTrapBreak` | Tuzak dayanıklılığı bitti | 3D | Metal kırılma | Sonra |
| `Sfx_BuildInvalid` | Geçersiz yere yerleştirme / para yetersiz | 2D | Kısa "bzzt" hata sesi | **MVP** |
| `Sfx_BuildRotate` | Q ile döndürme | 2D | Hafif tık | Sonra |
| `Sfx_TurretRotate` | Taret hedefe dönerken (`TurretAim`) | 3D, döngü | Servo motor vızıltısı | Sonra |
| `Sfx_TurretPowerDown` / `_Up` | Taret güç kesilince durur / açılır | 3D | Elektrik sönme / açılma | Sonra |
| `Sfx_LowFuelWarning` | Yakıt azaldığında (ör. %20) | 2D | Kesik bip | Sonra |
| `Sfx_EmergencyFuel` | Acil yakıt (gece, E 1 sn; `EmergencyFuelStation`) | 3D | Pompa + dökme | Sonra |

### Ganimet

| Önerilen asset | Ne zaman | 2D/3D | Önerilen içerik | Öncelik |
|---|---|---|---|---|
| `Sfx_LootCrateOpening` | E basılı tutulurken (2 sn) (`LootCrate`) | 3D, döngü | Levye / kilit zorlama | Sonra |
| `Sfx_CoinScatter` | Düşman ölünce jetonlar saçılırken | 3D | Yere düşen bozuk paralar | Sonra |

### Ambiyans (döngü)

| Önerilen asset | Ne zaman | Önerilen içerik | Öncelik |
|---|---|---|---|
| `Amb_Day` | Gündüz | Hafif rüzgâr, uzakta kuşlar, terk edilmiş şehir | **MVP** |
| `Amb_Night` | Gece | Cırcır böcekleri, soğuk rüzgâr, uzaktan zombi iniltileri | **MVP** |
| `Amb_Storm_Rain` | Fırtına gecesi | Yağmur + ara ara gök gürültüsü (şimşek flaşıyla senkron olabilir) | Sonra |
| `Amb_Fog` | Sis gecesi | Boğuk, yoğun rüzgâr | Sonra |
| `Amb_Horde` | Gece, canlı düşman sayısına göre ses düzeyi | Kalabalık zombi uğultusu | Sonra |

### Arayüz

| Önerilen asset | Ne zaman | Öncelik |
|---|---|---|
| `Ui_Hover` | Buton üzerine gelme / odak değişimi | Sonra |
| `Ui_Back` | Esc / geri | Sonra |
| `Ui_Error` | Para yetersiz, kilitli öğe | **MVP** |
| `Ui_PanelOpen` / `Ui_PanelClose` | Market (Tab), duraklatma, ayarlar | Sonra |
| `Ui_CardsAppear` | Seviye kartları ekrana gelirken | Sonra |
| `Ui_SliderTick` | Ayarlarda ses kaydırıcısı (%5 adım) | Sonra |

---

## 5. Üretim notları

- **Format:**
  - Efektler: WAV, 48 kHz, 16 bit. 3D sesler **mono**, 2D sesler stereo olabilir.
  - Müzik: WAV veya OGG, stereo. Döngü noktaları dikişsiz.
- **Ses düzeyi:**
  - Efektlerin tepesi −1 dBFS'e normalize edilsin.
  - Müzikler kendi aralarında yaklaşık eşit yükseklikte (~−16 LUFS).
  - Göreli denge `SoundData` volume ve mixer'dan yapılır.
- **Kısalık:** Sık çalan seslerin (tüfek, isabet, jeton) kuyruğu kısa olmalı. Uzun yankı yüzlerce zombide çamura döner.
- **Unity import:**
  - Kısa efektler: *Decompress On Load*.
  - 3D sesler: *Force To Mono*.
  - Müzik: *Streaming*, Vorbis (kalite ~70).
- **Klasör:** Klipler `Assets/_Project/Audio/Sfx/<Kategori>/` ve `Assets/_Project/Audio/Music/` altına. `Data/Audio/` sadece `SoundData` asset'leri içindir.
- **İsimlendirme:** Klip dosyaları `sfx_weapon_pistol_fire_01.wav` gibi küçük harf + numaralı varyasyon.
- **Lisans:**
  - Kenney ve Sonniss GDC paketleri ticari kullanıma açık.
  - Freesound'da CC-BY olanlar atıf ister, CC-BY-NC olanlar ticari projede kullanılamaz.
  - Her klibin kaynağını ve lisansını not edin.
