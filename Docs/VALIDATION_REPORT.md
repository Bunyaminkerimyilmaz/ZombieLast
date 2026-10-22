# Doğrulama Raporu

> `Tools/Last Haven/Validate Project` üretir; elle düzenleme, menüyü yeniden çalıştır.
> Son çalıştırma: 2026-10-22 22:16

**Sonuç:** 0 hata, 26 uyarı.

## Zorunlu referanslar (Data)

- Assets/_Project/Data altında 109 ScriptableObject kontrol edildi (gece verisi kendi bölümünde). Boş bırakılabilir alanlar: Tooltip'inde "Opsiyonel" geçenler, ses ve görsel (AudioClip, SoundData, Sprite, Texture).
- Sorun yok.

## Prefab görsel katmanı (Visual)

- Assets/_Project/Prefabs altında 20 varlık prefab'ı kontrol edildi (hariç: Assets/_Project/Prefabs/Feedback/, Assets/_Project/Prefabs/Audio/).
- Sorun yok.

## Build Settings sahneleri

- Assets/_Project/Scenes altında 2 sahne, Build Settings'te 3 satır kontrol edildi.
- Sorun yok.

## Düşman verisi ve prefab bağlantıları

- 7 EnemyData, prefab'ları ve Assets/_Project/Data/Enemies/EnemySpawnerConfig.asset kontrol edildi; 7 tip gece/gezgin verisinde kullanılıyor.
- Sorun yok.

## Gece verisi (NightData)

- 10 NightData ve Assets/_Project/Data/Waves/WaveConfig.asset (10 gece) kontrol edildi.
- Sorun yok.

## Ses verisi (SoundData)

- Assets/_Project/Data altında 26 SoundData kontrol edildi; 26 tanesinde klip yok (uyarı: o ses sessiz kalır, oyun bozulmaz).

| Önem | Asset | Sorun |
|---|---|---|
| Uyarı | `Assets/_Project/Data/Audio/Music/Music_Boss.asset` | Music_Boss: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Music/Music_Day.asset` | Music_Day: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Music/Music_GameOver.asset` | Music_GameOver: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Music/Music_Night.asset` | Music_Night: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Music/Music_Victory.asset` | Music_Victory: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_BossSpawned.asset` | Sfx_BossSpawned: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_CoinCollected.asset` | Sfx_CoinCollected: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_DayEndingSoon.asset` | Sfx_DayEndingSoon: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_EnemyKilled.asset` | Sfx_EnemyKilled: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_LevelUp.asset` | Sfx_LevelUp: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_Phase_Dawn.asset` | Sfx_Phase_Dawn: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_Phase_Day.asset` | Sfx_Phase_Day: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_Phase_Dusk.asset` | Sfx_Phase_Dusk: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_Phase_Night.asset` | Sfx_Phase_Night: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_PowerLost.asset` | Sfx_PowerLost: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_PurchaseCompleted.asset` | Sfx_PurchaseCompleted: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_ShotHit_Flesh.asset` | Sfx_ShotHit_Flesh: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_ShotHit_Surface.asset` | Sfx_ShotHit_Surface: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_StructureDestroyed.asset` | Sfx_StructureDestroyed: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_StructurePlaced.asset` | Sfx_StructurePlaced: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_WeaponFired_Default.asset` | Sfx_WeaponFired_Default: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_WeaponFired_Pistol.asset` | Sfx_WeaponFired_Pistol: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_WeaponFired_Rifle.asset` | Sfx_WeaponFired_Rifle: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_WeaponFired_Shotgun.asset` | Sfx_WeaponFired_Shotgun: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Sfx_WeaponFired_Turret.asset` | Sfx_WeaponFired_Turret: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
| Uyarı | `Assets/_Project/Data/Audio/Sfx/Ui_Click.asset` | Ui_Click: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle). |
