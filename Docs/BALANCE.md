# Denge Tablosu

> `Tools/Last Haven/Export Balance Sheet` üretir. İşaretler arasındaki tablolar her dışa aktarımda veriden (ScriptableObject'ler) yeniden yazılır; elle düzenleme, değeri SO'da değiştirip menüyü çalıştır. İşaretlerin dışındaki metin (analiz) korunur.

<!-- balance:auto:begin -->
## Düşmanlar

| Düşman | id | Can | Hız (m/sn) | Hasar | Saldırı/sn | DPS | Menzil (m) | Yapıya × | Aggro (m) | Para | XP | Para / 100 can | İlk gece |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Yürüyen | `walker` | 60 | 2.2 | 10 | 1 | 10 | 1.5 | 1 | 6 | 10 | 1 | 16.67 | 1 |
| Koşucu | `runner` | 35 | 5 | 6 | 1.5 | 9 | 1.3 | 1 | 6 | 12 | 1 | 34.29 | 2 |
| Tank | `tank` | 400 | 1.6 | 35 | 0.6 | 21 | 2 | 2 | 3 | 60 | 5 | 15 | 3 |
| Tükürücü | `spitter` | 70 | 2.5 | 12 | 0.5 | 6 | 10 | 1 | 6 | 25 | 2 | 35.71 | 4 |
| Patlayan | `exploder` | 50 | 3.5 | 60 | 1 | 60 | 1.2 | 2 | 6 | 20 | 2 | 40 | 6 |
| Brute | `brute` | 3000 | 1.8 | 40 | 0.17 | 6.67 | 4 | 1 | 6 | 500 | 30 | 16.67 | 5 |
| Brute (Final) | `brute_final` | 6000 | 1.8 | 40 | 0.17 | 6.67 | 4 | 1 | 6 | 500 | 30 | 8.33 | 10 |

DPS = hasar × saldırı/sn (tek hedefe, yapı çarpanı hariç). Patlayan tek seferlik alan hasarı verir (DPS anlamsız). Saldırı biçimi (yakın / mermi / patlama / yer darbesi) ve diğer alanlar "Tüm değerler" bölümünde.

## Geceler

| Gece | Yönler | Olay | İçerik | Düşman | Toplam can | Para (taban) | XP | Son spawn (sn) | Gereken DPS | Kümülatif can | Kümülatif para |
|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | North | — | 20 Yürüyen | 20 | 1200 | 200 | 20 | 13.3 | 90 | 1200 | 200 |
| 2 | North, East | — | 30 Yürüyen, 8 Koşucu | 38 | 2080 | 396 | 38 | 20.3 | 102 | 3280 | 596 |
| 3 | North, East, South | — | 35 Yürüyen, 15 Koşucu, 2 Tank | 52 | 3425 | 650 | 60 | 23.8 | 144 | 6705 | 1246 |
| 4 | East, West | Sis | 40 Yürüyen, 15 Koşucu, 3 Tank, 6 Tükürücü | 64 | 4545 | 910 | 82 | 27.3 | 166 | 11250 | 2156 |
| 5 | North, East, South, West | Kan Ayı | 60 Yürüyen, 15 Koşucu, Boss: Brute | 76 | 7125 | 2560 | 105 | 41.3 | 173 | 18375 | 4716 |
| 6 | North, South | — | 50 Yürüyen, 20 Koşucu, 4 Tank, 8 Patlayan | 82 | 5700 | 1140 | 106 | 34.3 | 166 | 24075 | 5856 |
| 7 | East, South, West | Fırtına | 55 Yürüyen, 25 Koşucu, 5 Tank, 10 Tükürücü, 8 Patlayan | 103 | 7275 | 1560 | 141 | 37.8 | 192 | 31350 | 7416 |
| 8 | North, East, South, West | — | 70 Yürüyen, 30 Koşucu, 6 Tank, 12 Tükürücü, 12 Patlayan | 130 | 9090 | 1960 | 178 | 48.3 | 188 | 40440 | 9376 |
| 9 | North, East, South, West | Sis | 80 Yürüyen, 35 Koşucu, 8 Tank, 15 Tükürücü, 15 Patlayan | 153 | 11025 | 2375 | 215 | 55.3 | 199 | 51465 | 11751 |
| 10 | North, East, South, West | Kan Ayı | 90 Yürüyen, 45 Koşucu, 9 Tank, 15 Patlayan, Boss: Brute (Final) | 160 | 17325 | 5560 | 240 | 62.3 | 278 | 68790 | 17311 |

- Para (taban) = Σ ödül × gecenin jeton çarpanı. Kombo (×1.5 / ×2 / ×3), Açgözlü perk'i ve toplanmayıp kaybolan jetonlar hesaba katılmaz; gerçek gelir bunun altında da üstünde de olabilir.
- Son spawn = gece başından son düşmanın doğma anı (grup gecikmesi + (adet − 1) × aralık). Canlı düşman tavanı (120) dolarsa gece uzar.
- Gereken DPS = toplam can / son spawn: düşmanlar birikmeden ölmesi için oyuncu + taretlerin ortalama vermesi gereken en az hasar (ıskalama, geri itilme ve yürüme süresi hariç).

## Gece başına kazanılabilir para

- Başlangıç: 300 para, 100 yakıt.
- Sandık: günde 6 tane; günlük beklenen 177 para + 60.31 yakıt (uzak payı halkanın alanından; aralığın ortası). Hepsinin açılabildiği varsayılır.
- Gezgin: günde ortalama 4.5 düşman, 45 para (öldürülürse; gece başlayınca dalgaya katılır).
- Erken hazır en çok 60 para (gündüz hiç kullanılmadan); sandık/gezgin geliriyle birbirini kısar, toplam bu yüzden üst sınırdır.
- Faiz: şafakta paranın %10'u, en çok 50 (tavan için şafakta ≥ 500 para gerekir).

| Gece | Öldürme (taban) | Sandık (beklenen) | Gezgin (beklenen) | Erken hazır (en çok) | Faiz (en çok) | Toplam | Kümülatif (başlangıç dahil) |
|---|---|---|---|---|---|---|---|
| 1 | 200 | 177 | 45 | 60 | 50 | 532 | 832 |
| 2 | 396 | 177 | 45 | 60 | 50 | 728 | 1559 |
| 3 | 650 | 177 | 45 | 60 | 50 | 982 | 2541 |
| 4 | 910 | 177 | 45 | 60 | 50 | 1242 | 3782 |
| 5 | 2560 | 177 | 45 | 60 | 50 | 2892 | 6674 |
| 6 | 1140 | 177 | 45 | 60 | 50 | 1472 | 8145 |
| 7 | 1560 | 177 | 45 | 60 | 50 | 1892 | 10037 |
| 8 | 1960 | 177 | 45 | 60 | 50 | 2292 | 12329 |
| 9 | 2375 | 177 | 45 | 60 | 50 | 2707 | 15035 |
| 10 | 5560 | 177 | 45 | 60 | 0 | 5842 | 20877 |

### Yakıt

Tüketim sadece gece: taban 0.4/sn + taret başına 0.05/sn. Kapasite 150. Acil yakıt 10 birim / 30 para. Gece süresi en az son spawn anıdır; gerçek gece daha uzundur (son düşmanların ölmesi).

| Gece | Son spawn (sn) | Yakıt (taretsiz, en az) | Taret başına + | Sandık yakıtı (beklenen, önceki gün) |
|---|---|---|---|---|
| 1 | 13.3 | 5.32 | 0.67 | 60.31 |
| 2 | 20.3 | 8.12 | 1.01 | 60.31 |
| 3 | 23.8 | 9.52 | 1.19 | 60.31 |
| 4 | 27.3 | 10.92 | 1.36 | 60.31 |
| 5 | 41.3 | 16.52 | 2.06 | 60.31 |
| 6 | 34.3 | 13.72 | 1.71 | 60.31 |
| 7 | 37.8 | 15.12 | 1.89 | 60.31 |
| 8 | 48.3 | 19.32 | 2.41 | 60.31 |
| 9 | 55.3 | 22.12 | 2.77 | 60.31 |
| 10 | 62.3 | 24.92 | 3.12 | 60.31 |

## Silahlar

| Silah | id | Hasar × saçma | Atış/sn | Ham DPS | Şarjör | Doldurma (sn) | Sürekli DPS | Yedek | Menzil (m) | Saçılma (°) |
|---|---|---|---|---|---|---|---|---|---|---|
| Tabanca | `pistol` | 20 × 1 | 4 | 80 | 12 | 1.2 | 57.14 | ∞ | 25 | 2 |
| Tüfek | `rifle` | 18 × 1 | 10 | 180 | 30 | 1.8 | 112.5 | 180 | 35 | 4 |
| Pompalı | `shotgun` | 12 × 8 | 1.1 | 105.6 | 6 | 2 | 77.27 | 36 | 12 | 18 |

Sürekli DPS = şarjör hasarı / (şarjörü boşaltma + doldurma). Tüm saçmaların isabet ettiği varsayılır; yükseltme ve perk'ler hariç. Yükseltme fiyatları "Market", yükseltme yüzdeleri "Tüm değerler" bölümünde.

## Yapılar

| Yapı | Boyut | Seviye | Ad | Can | Maliyet | Toplam harcama | Satış iadesi | Statlar |
|---|---|---|---|---|---|---|---|---|
| Barikat | 3×1 | 1 | Ahşap | 300 | 50 | 50 | 25 | — |
|  |  | 2 | Metal | 800 | 120 | 170 | 85 | — |
|  |  | 3 | Beton | 2000 | 300 | 470 | 235 | — |
| Diken Tuzağı | 2×2 | 1 | Standart | 40 | 80 | 80 | 40 | Damage 15, SlowFraction 0.4, TickInterval 0.5 |
| Taret | 2×2 | 1 | Standart | 250 | 250 | 250 | 125 | Damage 8, FireRate 5, Range 12 (DPS 40) |
|  |  | 2 | Ağır Namlu | 250 | 300 | 550 | 275 | Damage 12.8, FireRate 5, Range 12 (DPS 64) |
|  |  | 3 | Gelişmiş | 250 | 500 | 1050 | 525 | Damage 12.8, FireRate 6.5, Range 15 (DPS 83.2) |

Jeneratör: 1500 can. Statlar seviyenin kendi değerleridir (çarpan değil). Taret DPS = hasar × atış hızı (tek hedef, perk'siz).

## Market

| Öğe | Sekme | Etki | Hedef | Değer | Fiyatlar | Tüm seviyeler | Ön koşul |
|---|---|---|---|---|---|---|---|
| Pompalı | Weapons | UnlockWeapon | `shotgun` | 0 | 400 | 400 | — |
| Tüfek | Weapons | UnlockWeapon | `rifle` | 0 | 900 | 900 | — |
| Tabanca Hasarı | Upgrades | WeaponUpgrade | `pistol` | 0 | 150 / 300 / 500 / 800 / 1200 | 2950 | — |
| Tabanca Atış Hızı | Upgrades | WeaponUpgrade | `pistol` | 0 | 200 / 450 / 800 | 1450 | — |
| Tabanca Şarjörü | Upgrades | WeaponUpgrade | `pistol` | 0 | 150 / 350 / 600 | 1100 | — |
| Pompalı Hasarı | Upgrades | WeaponUpgrade | `shotgun` | 0 | 150 / 300 / 500 / 800 / 1200 | 2950 | `unlock_shotgun` |
| Pompalı Atış Hızı | Upgrades | WeaponUpgrade | `shotgun` | 0 | 200 / 450 / 800 | 1450 | `unlock_shotgun` |
| Pompalı Şarjörü | Upgrades | WeaponUpgrade | `shotgun` | 0 | 150 / 350 / 600 | 1100 | `unlock_shotgun` |
| Tüfek Hasarı | Upgrades | WeaponUpgrade | `rifle` | 0 | 150 / 300 / 500 / 800 / 1200 | 2950 | `unlock_rifle` |
| Tüfek Atış Hızı | Upgrades | WeaponUpgrade | `rifle` | 0 | 200 / 450 / 800 | 1450 | `unlock_rifle` |
| Tüfek Şarjörü | Upgrades | WeaponUpgrade | `rifle` | 0 | 150 / 350 / 600 | 1100 | `unlock_rifle` |
| Mıknatıs | Upgrades | PickupRadius | — | 1 | 100 / 200 / 350 | 650 | — |
| Pompalı Mermisi | Supplies | BuyAmmo | `shotgun` | 12 | 40 | tekrar alınır | `unlock_shotgun` |
| Tüfek Mermisi | Supplies | BuyAmmo | `rifle` | 60 | 50 | tekrar alınır | `unlock_rifle` |
| Can Paketi | Supplies | HealthKit | — | 50 | 60 | tekrar alınır | — |
| Yakıt | Supplies | BuyFuel | — | 10 | 15 | tekrar alınır | — |

## Seviye ve perk'ler

Sonraki seviye için XP = 10 + 6 × seviye. Başlangıç seviyesi 1. Seviye başına 3 kart.

| Gece | Gece XP | Kümülatif XP | Gece sonu seviye (en çok) | Toplam perk kartı (en çok) |
|---|---|---|---|---|
| 1 | 20 | 20 | 2 | 1 |
| 2 | 38 | 58 | 3 | 2 |
| 3 | 60 | 118 | 5 | 4 |
| 4 | 82 | 200 | 7 | 6 |
| 5 | 105 | 305 | 9 | 8 |
| 6 | 106 | 411 | 10 | 9 |
| 7 | 141 | 552 | 12 | 11 |
| 8 | 178 | 730 | 14 | 13 |
| 9 | 215 | 945 | 16 | 15 |
| 10 | 240 | 1185 | 18 | 17 |

| Perk | id | Nadirlik | Ağırlık | Maks. yığın | Açıklama |
|---|---|---|---|---|---|
| Hızlı Tetik | `quick_trigger` | Common | 60 | 3 | Atış hızı +%15. |
| Ağır Mermi | `heavy_rounds` | Common | 60 | 3 | Silah hasarı +%15. |
| Kalın Deri | `thick_skin` | Common | 60 | 3 | Maks. can +25 ve 25 can yeniler. |
| Mıknatıs | `magnet` | Common | 60 | 2 | Jeton toplama yarıçapı +%50. |
| Sağlam Duvar | `sturdy_walls` | Common | 60 | 2 | Barikatların maks. canı +%30. |
| Açgözlü | `greedy` | Rare | 30 | 2 | Jeton değeri +%25. |
| Adrenalin | `adrenaline` | Rare | 30 | 1 | Atılma bekleme süresi −%40. |
| Taret Ustası | `turret_master` | Rare | 30 | 2 | Taret atış hızı +%25. |
| Vampir | `vampire` | Rare | 30 | 1 | Her öldürme 2 can yeniler. |
| Avcı | `hunter` | Rare | 30 | 1 | Yavaşlamış düşmanlara +%50 hasar. |
| Seken Mermi | `ricochet` | Epic | 10 | 1 | Mermi vurduğu düşmandan en yakın ikinci düşmana seker (%50 hasar). |
| Dikenli Duvar | `thorn_walls` | Epic | 10 | 1 | Barikatlar aldıkları yakın dövüş hasarının %30'unu saldırana yansıtır. |
| Şok Taret | `shock_turret` | Epic | 10 | 1 | Taret vuruşları düşmanı 1 sn %25 yavaşlatır. |
| Zincirleme Son | `chain_explosion` | Epic | 10 | 1 | Öldürdüğün düşman %10 şansla patlar (3 m, 40 hasar). |

Tüm perk'lerin toplam yığını: 24.

## Tüm değerler (ham)

Oynanış SO'larının inspector'da görünen tüm alanları (`Data/` altında: Player, Weapons, Enemies, Waves, Base, Building, Economy, Shop, Loot, Progression, GameFlow). Sunum verisi (ses, ışık, kamera, UI, efekt, harita, debug) hariç.

### Player

<details><summary><b>PlayerData</b> (PlayerData)</summary>

| Alan | Değer |
|---|---|
| maxHealth | 100 |
| hitInvulnerabilityDuration | 0.3 |
| moveSpeed | 6 |
| dashDistance | 5 |
| dashDuration | 0.18 |
| dashCooldown | 2 |
| aimHeight | 1.2 |
| gamepadAimDistance | 6 |
| pickupRadius | 3 |
| vampireHealPerKill | 2 |

</details>

<details><summary><b>WeaponLoadout</b> (WeaponLoadoutData)</summary>

| Alan | Değer |
|---|---|
| slots | (3) |
| slots[0].weapon | Pistol |
| slots[0].unlockedAtStart | evet |
| slots[1].weapon | Shotgun |
| slots[1].unlockedAtStart | hayır |
| slots[2].weapon | Rifle |
| slots[2].unlockedAtStart | hayır |
| upgrades | WeaponUpgradeConfig |
| perks | CombatPerkConfig |
| hitMask | 2368 |

</details>

### Weapons

<details><summary><b>CombatPerkConfig</b> (CombatPerkConfig)</summary>

| Alan | Değer |
|---|---|
| hunterDamageBonus | 0.5 |
| ricochetDamageFraction | 0.5 |
| ricochetRadius | 8 |
| ricochetTargetLayers | 256 |
| ricochetBufferSize | 32 |

</details>

<details><summary><b>Pistol</b> (WeaponData)</summary>

| Alan | Değer |
|---|---|
| id | pistol |
| displayName | Tabanca |
| damage | 20 |
| pelletCount | 1 |
| fireRate | 4 |
| spreadAngle | 2 |
| range | 25 |
| magSize | 12 |
| reserveAmmo | -1 |
| reloadTime | 1.2 |
| tracerColor | #FFE08AFF |
| fireSound | — |
| muzzleVfx | — |

</details>

<details><summary><b>Rifle</b> (WeaponData)</summary>

| Alan | Değer |
|---|---|
| id | rifle |
| displayName | Tüfek |
| damage | 18 |
| pelletCount | 1 |
| fireRate | 10 |
| spreadAngle | 4 |
| range | 35 |
| magSize | 30 |
| reserveAmmo | 180 |
| reloadTime | 1.8 |
| tracerColor | #8FE3FFFF |
| fireSound | — |
| muzzleVfx | — |

</details>

<details><summary><b>Shotgun</b> (WeaponData)</summary>

| Alan | Değer |
|---|---|
| id | shotgun |
| displayName | Pompalı |
| damage | 12 |
| pelletCount | 8 |
| fireRate | 1.1 |
| spreadAngle | 18 |
| range | 12 |
| magSize | 6 |
| reserveAmmo | 36 |
| reloadTime | 2 |
| tracerColor | #FF9F43FF |
| fireSound | — |
| muzzleVfx | — |

</details>

<details><summary><b>WeaponUpgradeConfig</b> (WeaponUpgradeConfig)</summary>

| Alan | Değer |
|---|---|
| damagePerLevel | 0.1 |
| damageMaxLevel | 5 |
| fireRatePerLevel | 0.08 |
| fireRateMaxLevel | 3 |
| magSizePerLevel | 0.25 |
| magSizeMaxLevel | 3 |

</details>

### Enemies

<details><summary><b>BossConfig</b> (BossConfig)</summary>

| Alan | Değer |
|---|---|
| enrageHealthFraction | 0.5 |
| enragedSpeedMultiplier | 1.4 |
| enragedTint | #0D000099 |

</details>

<details><summary><b>Brute</b> (EnemyData)</summary>

| Alan | Değer |
|---|---|
| id | brute |
| displayName | Brute |
| maxHealth | 3000 |
| moveSpeed | 1.8 |
| damage | 40 |
| attackRate | 0.17 |
| attackRange | 4 |
| structureDamageMultiplier | 1 |
| attack | GroundSlamAttack |
| attack.radius | 4 |
| attack.windup | 1 |
| attack.edgeDamageFraction | 1 |
| attack.damageLayers | 640 |
| attack.frontOffset | 0 |
| attack.centerHeight | 0.5 |
| attack.cameraShake | 0.6 |
| attack.warningVfx | SlamWarning |
| attack.impactVfx | SlamFx |
| attack.vfxPrewarm | 2 |
| attack.bufferSize | 32 |
| aggroRadius | 6 |
| hasPreferredTarget | hayır |
| preferredTarget | Barricade |
| preferredTargetRadius | 15 |
| moneyReward | 500 |
| xpReward | 30 |
| knockbackResistance | 1 |
| knockbackDistance | 0.3 |
| knockbackDuration | 0.1 |
| scale | 1 |
| prefab | Brute |
| spawnDuration | 0.5 |
| retargetInterval | 0.5 |
| corpseDuration | 2 |
| sinkDuration | 1 |
| burrowDepth | 5.5 |
| disengageMargin | 0.3 |
| repathDistance | 0.5 |
| minAvoidancePriority | 30 |
| maxAvoidancePriority | 70 |
| attackSound | — |
| deathSound | — |
| deathVfx | — |

</details>

<details><summary><b>BruteFinal</b> (EnemyData)</summary>

| Alan | Değer |
|---|---|
| id | brute_final |
| displayName | Brute (Final) |
| maxHealth | 6000 |
| moveSpeed | 1.8 |
| damage | 40 |
| attackRate | 0.17 |
| attackRange | 4 |
| structureDamageMultiplier | 1 |
| attack | GroundSlamAttack |
| attack.radius | 4 |
| attack.windup | 1 |
| attack.edgeDamageFraction | 1 |
| attack.damageLayers | 640 |
| attack.frontOffset | 0 |
| attack.centerHeight | 0.5 |
| attack.cameraShake | 0.6 |
| attack.warningVfx | SlamWarning |
| attack.impactVfx | SlamFx |
| attack.vfxPrewarm | 2 |
| attack.bufferSize | 32 |
| aggroRadius | 6 |
| hasPreferredTarget | hayır |
| preferredTarget | Barricade |
| preferredTargetRadius | 15 |
| moneyReward | 500 |
| xpReward | 30 |
| knockbackResistance | 1 |
| knockbackDistance | 0.3 |
| knockbackDuration | 0.1 |
| scale | 1 |
| prefab | BruteFinal |
| spawnDuration | 0.5 |
| retargetInterval | 0.5 |
| corpseDuration | 2 |
| sinkDuration | 1 |
| burrowDepth | 5.5 |
| disengageMargin | 0.3 |
| repathDistance | 0.5 |
| minAvoidancePriority | 30 |
| maxAvoidancePriority | 70 |
| attackSound | — |
| deathSound | — |
| deathVfx | — |

</details>

<details><summary><b>EnemyPerkConfig</b> (EnemyPerkConfig)</summary>

| Alan | Değer |
|---|---|
| chainExplosionChance | 0.1 |
| chainExplosionRadius | 3 |
| chainExplosionDamage | 40 |
| chainExplosionEdgeDamageFraction | 1 |
| chainExplosionLayers | 256 |
| chainExplosionHeight | 1 |
| chainExplosionBufferSize | 64 |
| explosionVfx | ExplosionFx |
| vfxPrewarm | 4 |

</details>

<details><summary><b>EnemySpawnerConfig</b> (EnemySpawnerConfig)</summary>

| Alan | Değer |
|---|---|
| enemies | (7) |
| enemies[0].data | Walker |
| enemies[0].prewarm | 40 |
| enemies[1].data | Runner |
| enemies[1].prewarm | 15 |
| enemies[2].data | Tank |
| enemies[2].prewarm | 5 |
| enemies[3].data | Spitter |
| enemies[3].prewarm | 15 |
| enemies[4].data | Exploder |
| enemies[4].prewarm | 15 |
| enemies[5].data | Brute |
| enemies[5].prewarm | 1 |
| enemies[6].data | BruteFinal |
| enemies[6].prewarm | 1 |
| navMeshSampleDistance | 3 |

</details>

<details><summary><b>Exploder</b> (EnemyData)</summary>

| Alan | Değer |
|---|---|
| id | exploder |
| displayName | Patlayan |
| maxHealth | 50 |
| moveSpeed | 3.5 |
| damage | 60 |
| attackRate | 1 |
| attackRange | 1.2 |
| structureDamageMultiplier | 2 |
| attack | SuicideExplodeAttack |
| attack.radius | 3 |
| attack.edgeDamageFraction | 1 |
| attack.damageLayers | 640 |
| attack.contactLayers | 640 |
| attack.contactCheckInterval | 0.1 |
| attack.chainRadius | 3 |
| attack.chainDelay | 0.15 |
| attack.explosionVfx | ExplosionFx |
| attack.vfxPrewarm | 8 |
| attack.bufferSize | 32 |
| aggroRadius | 6 |
| hasPreferredTarget | hayır |
| preferredTarget | Barricade |
| preferredTargetRadius | 15 |
| moneyReward | 20 |
| xpReward | 2 |
| knockbackResistance | 0 |
| knockbackDistance | 0.3 |
| knockbackDuration | 0.1 |
| scale | 1 |
| prefab | Exploder |
| spawnDuration | 0.5 |
| retargetInterval | 0.5 |
| corpseDuration | 0 |
| sinkDuration | 0 |
| burrowDepth | 1.32 |
| disengageMargin | 0.3 |
| repathDistance | 0.5 |
| minAvoidancePriority | 30 |
| maxAvoidancePriority | 70 |
| attackSound | — |
| deathSound | — |
| deathVfx | — |

</details>

<details><summary><b>Runner</b> (EnemyData)</summary>

| Alan | Değer |
|---|---|
| id | runner |
| displayName | Koşucu |
| maxHealth | 35 |
| moveSpeed | 5 |
| damage | 6 |
| attackRate | 1.5 |
| attackRange | 1.3 |
| structureDamageMultiplier | 1 |
| attack | MeleeAttack |
| aggroRadius | 6 |
| hasPreferredTarget | hayır |
| preferredTarget | Barricade |
| preferredTargetRadius | 15 |
| moneyReward | 12 |
| xpReward | 1 |
| knockbackResistance | 0 |
| knockbackDistance | 0.3 |
| knockbackDuration | 0.1 |
| scale | 1 |
| prefab | Runner |
| spawnDuration | 0.5 |
| retargetInterval | 0.5 |
| corpseDuration | 2 |
| sinkDuration | 1 |
| burrowDepth | 2.2 |
| disengageMargin | 0.3 |
| repathDistance | 0.5 |
| minAvoidancePriority | 30 |
| maxAvoidancePriority | 70 |
| attackSound | — |
| deathSound | — |
| deathVfx | — |

</details>

<details><summary><b>Spitter</b> (EnemyData)</summary>

| Alan | Değer |
|---|---|
| id | spitter |
| displayName | Tükürücü |
| maxHealth | 70 |
| moveSpeed | 2.5 |
| damage | 12 |
| attackRate | 0.5 |
| attackRange | 10 |
| structureDamageMultiplier | 1 |
| attack | RangedAttack |
| attack.projectile | AcidProjectile |
| attack.horizontalSpeed | 12 |
| attack.gravity | 20 |
| attack.minFlightTime | 0.35 |
| attack.hitRadius | 0.25 |
| attack.maxLifetime | 4 |
| attack.hitLayers | 2752 |
| attack.prewarm | 16 |
| attack.impactVfx | — |
| aggroRadius | 6 |
| hasPreferredTarget | evet |
| preferredTarget | Turret |
| preferredTargetRadius | 15 |
| moneyReward | 25 |
| xpReward | 2 |
| knockbackResistance | 0 |
| knockbackDistance | 0.3 |
| knockbackDuration | 0.1 |
| scale | 1 |
| prefab | Spitter |
| spawnDuration | 0.5 |
| retargetInterval | 0.5 |
| corpseDuration | 2 |
| sinkDuration | 1 |
| burrowDepth | 2.2 |
| disengageMargin | 0.3 |
| repathDistance | 0.5 |
| minAvoidancePriority | 30 |
| maxAvoidancePriority | 70 |
| attackSound | — |
| deathSound | — |
| deathVfx | — |

</details>

<details><summary><b>Tank</b> (EnemyData)</summary>

| Alan | Değer |
|---|---|
| id | tank |
| displayName | Tank |
| maxHealth | 400 |
| moveSpeed | 1.6 |
| damage | 35 |
| attackRate | 0.6 |
| attackRange | 2 |
| structureDamageMultiplier | 2 |
| attack | MeleeAttack |
| aggroRadius | 3 |
| hasPreferredTarget | evet |
| preferredTarget | Barricade |
| preferredTargetRadius | 15 |
| moneyReward | 60 |
| xpReward | 5 |
| knockbackResistance | 1 |
| knockbackDistance | 0.3 |
| knockbackDuration | 0.1 |
| scale | 1 |
| prefab | Tank |
| spawnDuration | 0.5 |
| retargetInterval | 0.5 |
| corpseDuration | 2 |
| sinkDuration | 1 |
| burrowDepth | 3.52 |
| disengageMargin | 0.3 |
| repathDistance | 0.5 |
| minAvoidancePriority | 30 |
| maxAvoidancePriority | 70 |
| attackSound | — |
| deathSound | — |
| deathVfx | — |

</details>

<details><summary><b>Walker</b> (EnemyData)</summary>

| Alan | Değer |
|---|---|
| id | walker |
| displayName | Yürüyen |
| maxHealth | 60 |
| moveSpeed | 2.2 |
| damage | 10 |
| attackRate | 1 |
| attackRange | 1.5 |
| structureDamageMultiplier | 1 |
| attack | MeleeAttack |
| aggroRadius | 6 |
| hasPreferredTarget | hayır |
| preferredTarget | Barricade |
| preferredTargetRadius | 15 |
| moneyReward | 10 |
| xpReward | 1 |
| knockbackResistance | 0 |
| knockbackDistance | 0.3 |
| knockbackDuration | 0.1 |
| scale | 1 |
| prefab | Walker |
| spawnDuration | 0.5 |
| retargetInterval | 0.5 |
| corpseDuration | 2 |
| sinkDuration | 1 |
| burrowDepth | 2.2 |
| disengageMargin | 0.3 |
| repathDistance | 0.5 |
| minAvoidancePriority | 30 |
| maxAvoidancePriority | 70 |
| attackSound | — |
| deathSound | — |
| deathVfx | — |

</details>

### Waves

<details><summary><b>DayWanderersConfig</b> (DayWanderersConfig)</summary>

| Alan | Değer |
|---|---|
| enemy | Walker |
| minCount | 3 |
| maxCount | 6 |
| minRadius | 20 |
| maxRadius | 38 |
| minPlayerDistance | 15 |
| spawnAttempts | 20 |
| minLegDistance | 4 |
| maxLegDistance | 10 |
| minPause | 1 |
| maxPause | 3 |
| maxLegDuration | 12 |
| arrivalTolerance | 0.5 |
| navMeshSampleDistance | 3 |

</details>

<details><summary><b>BloodMoon</b> (NightModifierData)</summary>

| Alan | Değer |
|---|---|
| id | blood_moon |
| displayName | Kan Ayı |
| description | Düşmanlar 1.5 kat kalabalık, jetonlar 2 kat değerli. |
| enemyCountMultiplier | 1.5 |
| enemySpeedMultiplier | 1 |
| coinMultiplier | 2 |
| overrideFog | hayır |
| fogColor | #242933FF |
| fogDensity | 0.04 |
| overrideLight | evet |
| lightColor | #F23324FF |
| ambientColor | #521414FF |
| powerOutages | hayır |
| outageInterval | 25 |
| outageDuration | 5 |

</details>

<details><summary><b>Fog</b> (NightModifierData)</summary>

| Alan | Değer |
|---|---|
| id | fog |
| displayName | Sis |
| description | Görüş azalır (~12 m), düşmanlar %10 hızlı. |
| enemyCountMultiplier | 1 |
| enemySpeedMultiplier | 1.1 |
| coinMultiplier | 1 |
| overrideFog | evet |
| fogColor | #242933FF |
| fogDensity | 0.04 |
| overrideLight | hayır |
| lightColor | #F23324FF |
| ambientColor | #521414FF |
| powerOutages | hayır |
| outageInterval | 25 |
| outageDuration | 5 |

</details>

<details><summary><b>Storm</b> (NightModifierData)</summary>

| Alan | Değer |
|---|---|
| id | storm |
| displayName | Fırtına |
| description | Her 25 sn'de şimşek: 5 sn güç kesintisi (taretler ve üs ışıkları durur). |
| enemyCountMultiplier | 1 |
| enemySpeedMultiplier | 1 |
| coinMultiplier | 1 |
| overrideFog | hayır |
| fogColor | #242933FF |
| fogDensity | 0.04 |
| overrideLight | hayır |
| lightColor | #F23324FF |
| ambientColor | #521414FF |
| powerOutages | evet |
| outageInterval | 25 |
| outageDuration | 5 |

</details>

<details><summary><b>Night01</b> (NightData)</summary>

| Alan | Değer |
|---|---|
| nightNumber | 1 |
| groups | (1) |
| groups[0].enemy | Walker |
| groups[0].count | 20 |
| groups[0].spawnInterval | 0.7 |
| groups[0].startDelay | 0 |
| groups[0].directions | (1) |
| groups[0].directions[0] | North |
| modifier | — |
| bossData | — |
| bossStartDelay | 20 |
| bossDirection | North |

</details>

<details><summary><b>Night02</b> (NightData)</summary>

| Alan | Değer |
|---|---|
| nightNumber | 2 |
| groups | (2) |
| groups[0].enemy | Walker |
| groups[0].count | 30 |
| groups[0].spawnInterval | 0.7 |
| groups[0].startDelay | 0 |
| groups[0].directions | (2) |
| groups[0].directions[0] | North |
| groups[0].directions[1] | East |
| groups[1].enemy | Runner |
| groups[1].count | 8 |
| groups[1].spawnInterval | 0.9 |
| groups[1].startDelay | 0 |
| groups[1].directions | (2) |
| groups[1].directions[0] | North |
| groups[1].directions[1] | East |
| modifier | — |
| bossData | — |
| bossStartDelay | 20 |
| bossDirection | North |

</details>

<details><summary><b>Night03</b> (NightData)</summary>

| Alan | Değer |
|---|---|
| nightNumber | 3 |
| groups | (3) |
| groups[0].enemy | Walker |
| groups[0].count | 35 |
| groups[0].spawnInterval | 0.7 |
| groups[0].startDelay | 0 |
| groups[0].directions | (3) |
| groups[0].directions[0] | North |
| groups[0].directions[1] | East |
| groups[0].directions[2] | South |
| groups[1].enemy | Runner |
| groups[1].count | 15 |
| groups[1].spawnInterval | 0.9 |
| groups[1].startDelay | 0 |
| groups[1].directions | (3) |
| groups[1].directions[0] | North |
| groups[1].directions[1] | East |
| groups[1].directions[2] | South |
| groups[2].enemy | Tank |
| groups[2].count | 2 |
| groups[2].spawnInterval | 1.2 |
| groups[2].startDelay | 11.3 |
| groups[2].directions | (3) |
| groups[2].directions[0] | North |
| groups[2].directions[1] | East |
| groups[2].directions[2] | South |
| modifier | — |
| bossData | — |
| bossStartDelay | 20 |
| bossDirection | North |

</details>

<details><summary><b>Night04</b> (NightData)</summary>

| Alan | Değer |
|---|---|
| nightNumber | 4 |
| groups | (4) |
| groups[0].enemy | Walker |
| groups[0].count | 40 |
| groups[0].spawnInterval | 0.7 |
| groups[0].startDelay | 0 |
| groups[0].directions | (2) |
| groups[0].directions[0] | East |
| groups[0].directions[1] | West |
| groups[1].enemy | Runner |
| groups[1].count | 15 |
| groups[1].spawnInterval | 0.9 |
| groups[1].startDelay | 0 |
| groups[1].directions | (2) |
| groups[1].directions[0] | East |
| groups[1].directions[1] | West |
| groups[2].enemy | Tank |
| groups[2].count | 3 |
| groups[2].spawnInterval | 1.2 |
| groups[2].startDelay | 12.4 |
| groups[2].directions | (2) |
| groups[2].directions[0] | East |
| groups[2].directions[1] | West |
| groups[3].enemy | Spitter |
| groups[3].count | 6 |
| groups[3].spawnInterval | 1 |
| groups[3].startDelay | 11.1 |
| groups[3].directions | (2) |
| groups[3].directions[0] | East |
| groups[3].directions[1] | West |
| modifier | Fog |
| bossData | — |
| bossStartDelay | 20 |
| bossDirection | North |

</details>

<details><summary><b>Night05</b> (NightData)</summary>

| Alan | Değer |
|---|---|
| nightNumber | 5 |
| groups | (2) |
| groups[0].enemy | Walker |
| groups[0].count | 40 |
| groups[0].spawnInterval | 0.7 |
| groups[0].startDelay | 0 |
| groups[0].directions | (4) |
| groups[0].directions[0] | North |
| groups[0].directions[1] | East |
| groups[0].directions[2] | South |
| groups[0].directions[3] | West |
| groups[1].enemy | Runner |
| groups[1].count | 10 |
| groups[1].spawnInterval | 0.9 |
| groups[1].startDelay | 0 |
| groups[1].directions | (4) |
| groups[1].directions[0] | North |
| groups[1].directions[1] | East |
| groups[1].directions[2] | South |
| groups[1].directions[3] | West |
| modifier | BloodMoon |
| bossData | Brute |
| bossStartDelay | 13.6 |
| bossDirection | North |

</details>

<details><summary><b>Night06</b> (NightData)</summary>

| Alan | Değer |
|---|---|
| nightNumber | 6 |
| groups | (4) |
| groups[0].enemy | Walker |
| groups[0].count | 50 |
| groups[0].spawnInterval | 0.7 |
| groups[0].startDelay | 0 |
| groups[0].directions | (2) |
| groups[0].directions[0] | North |
| groups[0].directions[1] | South |
| groups[1].enemy | Runner |
| groups[1].count | 20 |
| groups[1].spawnInterval | 0.9 |
| groups[1].startDelay | 0 |
| groups[1].directions | (2) |
| groups[1].directions[0] | North |
| groups[1].directions[1] | South |
| groups[2].enemy | Tank |
| groups[2].count | 4 |
| groups[2].spawnInterval | 1.2 |
| groups[2].startDelay | 15.3 |
| groups[2].directions | (2) |
| groups[2].directions[0] | North |
| groups[2].directions[1] | South |
| groups[3].enemy | Exploder |
| groups[3].count | 8 |
| groups[3].spawnInterval | 1 |
| groups[3].startDelay | 13.6 |
| groups[3].directions | (2) |
| groups[3].directions[0] | North |
| groups[3].directions[1] | South |
| modifier | — |
| bossData | — |
| bossStartDelay | 20 |
| bossDirection | North |

</details>

<details><summary><b>Night07</b> (NightData)</summary>

| Alan | Değer |
|---|---|
| nightNumber | 7 |
| groups | (5) |
| groups[0].enemy | Walker |
| groups[0].count | 55 |
| groups[0].spawnInterval | 0.7 |
| groups[0].startDelay | 0 |
| groups[0].directions | (3) |
| groups[0].directions[0] | East |
| groups[0].directions[1] | South |
| groups[0].directions[2] | West |
| groups[1].enemy | Runner |
| groups[1].count | 25 |
| groups[1].spawnInterval | 0.9 |
| groups[1].startDelay | 0 |
| groups[1].directions | (3) |
| groups[1].directions[0] | East |
| groups[1].directions[1] | South |
| groups[1].directions[2] | West |
| groups[2].enemy | Tank |
| groups[2].count | 5 |
| groups[2].spawnInterval | 1.2 |
| groups[2].startDelay | 16.5 |
| groups[2].directions | (3) |
| groups[2].directions[0] | East |
| groups[2].directions[1] | South |
| groups[2].directions[2] | West |
| groups[3].enemy | Spitter |
| groups[3].count | 10 |
| groups[3].spawnInterval | 1 |
| groups[3].startDelay | 14.4 |
| groups[3].directions | (3) |
| groups[3].directions[0] | East |
| groups[3].directions[1] | South |
| groups[3].directions[2] | West |
| groups[4].enemy | Exploder |
| groups[4].count | 8 |
| groups[4].spawnInterval | 1 |
| groups[4].startDelay | 15.4 |
| groups[4].directions | (3) |
| groups[4].directions[0] | East |
| groups[4].directions[1] | South |
| groups[4].directions[2] | West |
| modifier | Storm |
| bossData | — |
| bossStartDelay | 20 |
| bossDirection | North |

</details>

<details><summary><b>Night08</b> (NightData)</summary>

| Alan | Değer |
|---|---|
| nightNumber | 8 |
| groups | (5) |
| groups[0].enemy | Walker |
| groups[0].count | 70 |
| groups[0].spawnInterval | 0.7 |
| groups[0].startDelay | 0 |
| groups[0].directions | (4) |
| groups[0].directions[0] | North |
| groups[0].directions[1] | East |
| groups[0].directions[2] | South |
| groups[0].directions[3] | West |
| groups[1].enemy | Runner |
| groups[1].count | 30 |
| groups[1].spawnInterval | 0.9 |
| groups[1].startDelay | 0 |
| groups[1].directions | (4) |
| groups[1].directions[0] | North |
| groups[1].directions[1] | East |
| groups[1].directions[2] | South |
| groups[1].directions[3] | West |
| groups[2].enemy | Tank |
| groups[2].count | 6 |
| groups[2].spawnInterval | 1.2 |
| groups[2].startDelay | 21.2 |
| groups[2].directions | (4) |
| groups[2].directions[0] | North |
| groups[2].directions[1] | East |
| groups[2].directions[2] | South |
| groups[2].directions[3] | West |
| groups[3].enemy | Spitter |
| groups[3].count | 12 |
| groups[3].spawnInterval | 1 |
| groups[3].startDelay | 18.6 |
| groups[3].directions | (4) |
| groups[3].directions[0] | North |
| groups[3].directions[1] | East |
| groups[3].directions[2] | South |
| groups[3].directions[3] | West |
| groups[4].enemy | Exploder |
| groups[4].count | 12 |
| groups[4].spawnInterval | 1 |
| groups[4].startDelay | 18.6 |
| groups[4].directions | (4) |
| groups[4].directions[0] | North |
| groups[4].directions[1] | East |
| groups[4].directions[2] | South |
| groups[4].directions[3] | West |
| modifier | — |
| bossData | — |
| bossStartDelay | 20 |
| bossDirection | North |

</details>

<details><summary><b>Night09</b> (NightData)</summary>

| Alan | Değer |
|---|---|
| nightNumber | 9 |
| groups | (5) |
| groups[0].enemy | Walker |
| groups[0].count | 80 |
| groups[0].spawnInterval | 0.7 |
| groups[0].startDelay | 0 |
| groups[0].directions | (4) |
| groups[0].directions[0] | North |
| groups[0].directions[1] | East |
| groups[0].directions[2] | South |
| groups[0].directions[3] | West |
| groups[1].enemy | Runner |
| groups[1].count | 35 |
| groups[1].spawnInterval | 0.9 |
| groups[1].startDelay | 0 |
| groups[1].directions | (4) |
| groups[1].directions[0] | North |
| groups[1].directions[1] | East |
| groups[1].directions[2] | South |
| groups[1].directions[3] | West |
| groups[2].enemy | Tank |
| groups[2].count | 8 |
| groups[2].spawnInterval | 1.2 |
| groups[2].startDelay | 23.4 |
| groups[2].directions | (4) |
| groups[2].directions[0] | North |
| groups[2].directions[1] | East |
| groups[2].directions[2] | South |
| groups[2].directions[3] | West |
| groups[3].enemy | Spitter |
| groups[3].count | 15 |
| groups[3].spawnInterval | 1 |
| groups[3].startDelay | 20.6 |
| groups[3].directions | (4) |
| groups[3].directions[0] | North |
| groups[3].directions[1] | East |
| groups[3].directions[2] | South |
| groups[3].directions[3] | West |
| groups[4].enemy | Exploder |
| groups[4].count | 15 |
| groups[4].spawnInterval | 1 |
| groups[4].startDelay | 20.6 |
| groups[4].directions | (4) |
| groups[4].directions[0] | North |
| groups[4].directions[1] | East |
| groups[4].directions[2] | South |
| groups[4].directions[3] | West |
| modifier | Fog |
| bossData | — |
| bossStartDelay | 20 |
| bossDirection | North |

</details>

<details><summary><b>Night10</b> (NightData)</summary>

| Alan | Değer |
|---|---|
| nightNumber | 10 |
| groups | (4) |
| groups[0].enemy | Walker |
| groups[0].count | 60 |
| groups[0].spawnInterval | 0.7 |
| groups[0].startDelay | 0 |
| groups[0].directions | (4) |
| groups[0].directions[0] | North |
| groups[0].directions[1] | East |
| groups[0].directions[2] | South |
| groups[0].directions[3] | West |
| groups[1].enemy | Runner |
| groups[1].count | 30 |
| groups[1].spawnInterval | 0.9 |
| groups[1].startDelay | 0 |
| groups[1].directions | (4) |
| groups[1].directions[0] | North |
| groups[1].directions[1] | East |
| groups[1].directions[2] | South |
| groups[1].directions[3] | West |
| groups[2].enemy | Tank |
| groups[2].count | 6 |
| groups[2].spawnInterval | 1.2 |
| groups[2].startDelay | 17.6 |
| groups[2].directions | (4) |
| groups[2].directions[0] | North |
| groups[2].directions[1] | East |
| groups[2].directions[2] | South |
| groups[2].directions[3] | West |
| groups[3].enemy | Exploder |
| groups[3].count | 10 |
| groups[3].spawnInterval | 1 |
| groups[3].startDelay | 16.2 |
| groups[3].directions | (4) |
| groups[3].directions[0] | North |
| groups[3].directions[1] | East |
| groups[3].directions[2] | South |
| groups[3].directions[3] | West |
| modifier | BloodMoon |
| bossData | BruteFinal |
| bossStartDelay | 20.6 |
| bossDirection | North |

</details>

<details><summary><b>WaveConfig</b> (WaveConfig)</summary>

| Alan | Değer |
|---|---|
| nights | (10) |
| nights[0] | Night01 |
| nights[1] | Night02 |
| nights[2] | Night03 |
| nights[3] | Night04 |
| nights[4] | Night05 |
| nights[5] | Night06 |
| nights[6] | Night07 |
| nights[7] | Night08 |
| nights[8] | Night09 |
| nights[9] | Night10 |
| maxAliveEnemies | 120 |

</details>

### Base

<details><summary><b>BaseAreaConfig</b> (BaseAreaConfig)</summary>

| Alan | Değer |
|---|---|
| radius | 14 |

</details>

<details><summary><b>GeneratorData</b> (GeneratorData)</summary>

| Alan | Değer |
|---|---|
| maxHealth | 1500 |
| fuelCapacity | 150 |
| baseFuelConsumption | 0.4 |
| fuelConsumptionPerTurret | 0.05 |
| emergencyFuelPrice | 30 |
| emergencyFuelAmount | 10 |
| emergencyFuelHoldDuration | 1 |
| emergencyFuelRange | 2 |
| emergencyFuelPromptFormat | Acil yakıt +{1} ({0} para) |

</details>

### Building

<details><summary><b>Barricade</b> (BuildableData)</summary>

| Alan | Değer |
|---|---|
| id | barricade |
| displayName | Barikat |
| description | Zombilerin yolunu keser; etrafından dolaşırlar. U ile metale ve betona yükseltilir. |
| icon | — |
| footprint | (3, 1) |
| levels | (3) |
| levels[0].displayName | Ahşap |
| levels[0].cost | 50 |
| levels[0].maxHealth | 300 |
| levels[0].material | M_BarricadeWood |
| levels[0].stats | (0) |
| levels[1].displayName | Metal |
| levels[1].cost | 120 |
| levels[1].maxHealth | 800 |
| levels[1].material | M_BarricadeMetal |
| levels[1].stats | (0) |
| levels[2].displayName | Beton |
| levels[2].cost | 300 |
| levels[2].maxHealth | 2000 |
| levels[2].material | M_BarricadeConcrete |
| levels[2].stats | (0) |
| prefab | Barricade |
| carvesNavMesh | evet |
| targetType | Barricade |

</details>

<details><summary><b>BuildConfig</b> (BuildConfig)</summary>

| Alan | Değer |
|---|---|
| slots | (3) |
| slots[0] | Barricade |
| slots[1] | Turret |
| slots[2] | SpikeTrap |
| structurePrewarm | 12 |
| cellSize | 1 |
| generatorHalfSize | 1 |
| generatorClearance | 2 |
| ghostMaterial | M_BuildGhost |
| validColor | #40FF6673 |
| invalidColor | #FF403373 |
| gamepadPlacementDistance | 3 |
| highlightColor | #FFD94C59 |
| highlightScale | 1.04 |
| rangeColor | #59D9FF99 |
| rangeLineWidth | 0.1 |
| rangeSegments | 72 |
| blockingLayers | 2432 |
| blockingCheckHeight | 2 |

</details>

<details><summary><b>BuildingPerkConfig</b> (BuildingPerkConfig)</summary>

| Alan | Değer |
|---|---|
| thornsReflectFraction | 0.3 |
| shockSlowFraction | 0.25 |
| shockSlowDuration | 1 |

</details>

<details><summary><b>RepairConfig</b> (RepairConfig)</summary>

| Alan | Değer |
|---|---|
| range | 2 |
| healthPerSecond | 50 |
| healthPerMoney | 5 |
| promptFormat | Onar ({0}) |
| barLingerDuration | 1 |

</details>

<details><summary><b>SpikeTrap</b> (BuildableData)</summary>

| Alan | Değer |
|---|---|
| id | spike_trap |
| displayName | Diken Tuzağı |
| description | Üstünden geçen zombilere hasar verir ve onları %40 yavaşlatır. Her hasar tikinde aşınır, dayanıklılığı bitince kırılır. |
| icon | — |
| footprint | (2, 2) |
| levels | (1) |
| levels[0].displayName | Standart |
| levels[0].cost | 80 |
| levels[0].maxHealth | 40 |
| levels[0].material | M_SpikeTrap |
| levels[0].stats | (3) |
| levels[0].stats[0].stat | Damage |
| levels[0].stats[0].value | 15 |
| levels[0].stats[1].stat | SlowFraction |
| levels[0].stats[1].value | 0.4 |
| levels[0].stats[2].stat | TickInterval |
| levels[0].stats[2].value | 0.5 |
| prefab | SpikeTrap |
| carvesNavMesh | hayır |
| targetType | Trap |

</details>

<details><summary><b>SpikeTrapConfig</b> (SpikeTrapConfig)</summary>

| Alan | Değer |
|---|---|
| targetLayers | 256 |
| scanBufferSize | 32 |
| idleScanInterval | 0.1 |
| slowDuration | 1 |
| wearPerTick | 1 |

</details>

<details><summary><b>Turret</b> (BuildableData)</summary>

| Alan | Değer |
|---|---|
| id | turret |
| displayName | Taret |
| description | Menzildeki en yakın zombiye döner ve ateş eder; güç ister. U ile önce hasarı, sonra menzili ve atış hızı artar. |
| icon | — |
| footprint | (2, 2) |
| levels | (3) |
| levels[0].displayName | Standart |
| levels[0].cost | 250 |
| levels[0].maxHealth | 250 |
| levels[0].material | M_Turret |
| levels[0].stats | (3) |
| levels[0].stats[0].stat | Damage |
| levels[0].stats[0].value | 8 |
| levels[0].stats[1].stat | FireRate |
| levels[0].stats[1].value | 5 |
| levels[0].stats[2].stat | Range |
| levels[0].stats[2].value | 12 |
| levels[1].displayName | Ağır Namlu |
| levels[1].cost | 300 |
| levels[1].maxHealth | 250 |
| levels[1].material | M_Turret |
| levels[1].stats | (3) |
| levels[1].stats[0].stat | Damage |
| levels[1].stats[0].value | 12.8 |
| levels[1].stats[1].stat | FireRate |
| levels[1].stats[1].value | 5 |
| levels[1].stats[2].stat | Range |
| levels[1].stats[2].value | 12 |
| levels[2].displayName | Gelişmiş |
| levels[2].cost | 500 |
| levels[2].maxHealth | 250 |
| levels[2].material | M_Turret |
| levels[2].stats | (3) |
| levels[2].stats[0].stat | Damage |
| levels[2].stats[0].value | 12.8 |
| levels[2].stats[1].stat | FireRate |
| levels[2].stats[1].value | 6.5 |
| levels[2].stats[2].stat | Range |
| levels[2].stats[2].value | 15 |
| prefab | Turret |
| carvesNavMesh | evet |
| targetType | Turret |

</details>

<details><summary><b>TurretConfig</b> (TurretConfig)</summary>

| Alan | Değer |
|---|---|
| scanInterval | 0.2 |
| targetLayers | 256 |
| scanBufferSize | 64 |
| turnSpeed | 360 |
| aimTolerance | 5 |
| hitMask | 2368 |
| weaponId | turret |
| perks | BuildingPerkConfig |

</details>

### Economy

<details><summary><b>EconomyConfig</b> (EconomyConfig)</summary>

| Alan | Değer |
|---|---|
| startingMoney | 300 |
| startingFuel | 100 |
| interestRate | 0.1 |
| interestCap | 50 |
| coinLifetime | 12 |
| coinBlinkDuration | 3 |
| minCoinsPerDrop | 1 |
| maxCoinsPerDrop | 5 |
| targetCoinValue | 5 |
| coinPrefab | Coin |
| coinPrewarm | 200 |
| coinMotion.spawnHeight | 0.8 |
| coinMotion.restHeight | 0.3 |
| coinMotion.scatterMinDistance | 0.6 |
| coinMotion.scatterMaxDistance | 1.6 |
| coinMotion.arcHeight | 1.2 |
| coinMotion.arcDuration | 0.45 |
| coinMotion.spinSpeed | 180 |
| coinMotion.blinkFrequency | 6 |
| coinMotion.attractStartSpeed | 4 |
| coinMotion.attractAcceleration | 40 |
| coinMotion.attractMaxSpeed | 25 |
| coinMotion.collectDistance | 0.4 |
| coinMotion.collectHeight | 1 |
| comboWindow | 2.5 |
| comboTiers | (3) |
| comboTiers[0].kills | 5 |
| comboTiers[0].multiplier | 1.5 |
| comboTiers[1].kills | 15 |
| comboTiers[1].multiplier | 2 |
| comboTiers[2].kills | 30 |
| comboTiers[2].multiplier | 3 |
| sellRefundRate | 0.5 |

</details>

### Shop

<details><summary><b>AmmoRifle</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | ammo_rifle |
| displayName | Tüfek Mermisi |
| description | 60 yedek tüfek mermisi. |
| category | Supplies |
| prices | (1) |
| prices[0] | 50 |
| maxLevel | 0 |
| requiredItemId | unlock_rifle |
| effect | BuyAmmo |
| targetId | rifle |
| upgradeType | Damage |
| value | 60 |

</details>

<details><summary><b>AmmoShotgun</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | ammo_shotgun |
| displayName | Pompalı Mermisi |
| description | 12 yedek pompalı mermisi. |
| category | Supplies |
| prices | (1) |
| prices[0] | 40 |
| maxLevel | 0 |
| requiredItemId | unlock_shotgun |
| effect | BuyAmmo |
| targetId | shotgun |
| upgradeType | Damage |
| value | 12 |

</details>

<details><summary><b>Fuel</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | fuel |
| displayName | Yakıt |
| description | Jeneratöre 10 birim yakıt. |
| category | Supplies |
| prices | (1) |
| prices[0] | 15 |
| maxLevel | 0 |
| requiredItemId | — |
| effect | BuyFuel |
| targetId | — |
| upgradeType | Damage |
| value | 10 |

</details>

<details><summary><b>HealthKit</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | health_kit |
| displayName | Can Paketi |
| description | Anında 50 can yeniler. |
| category | Supplies |
| prices | (1) |
| prices[0] | 60 |
| maxLevel | 0 |
| requiredItemId | — |
| effect | HealthKit |
| targetId | — |
| upgradeType | Damage |
| value | 50 |

</details>

<details><summary><b>Magnet</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | magnet |
| displayName | Mıknatıs |
| description | Jeton toplama yarıçapı her seviyede +1 m. |
| category | Upgrades |
| prices | (3) |
| prices[0] | 100 |
| prices[1] | 200 |
| prices[2] | 350 |
| maxLevel | 3 |
| requiredItemId | — |
| effect | PickupRadius |
| targetId | — |
| upgradeType | Damage |
| value | 1 |

</details>

<details><summary><b>PistolDamage</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | pistol_damage |
| displayName | Tabanca Hasarı |
| description | Her seviye hasarı %10 artırır. |
| category | Upgrades |
| prices | (5) |
| prices[0] | 150 |
| prices[1] | 300 |
| prices[2] | 500 |
| prices[3] | 800 |
| prices[4] | 1200 |
| maxLevel | 5 |
| requiredItemId | — |
| effect | WeaponUpgrade |
| targetId | pistol |
| upgradeType | Damage |
| value | 0 |

</details>

<details><summary><b>PistolFireRate</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | pistol_fire_rate |
| displayName | Tabanca Atış Hızı |
| description | Her seviye atış hızını %8 artırır. |
| category | Upgrades |
| prices | (3) |
| prices[0] | 200 |
| prices[1] | 450 |
| prices[2] | 800 |
| maxLevel | 3 |
| requiredItemId | — |
| effect | WeaponUpgrade |
| targetId | pistol |
| upgradeType | FireRate |
| value | 0 |

</details>

<details><summary><b>PistolMagSize</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | pistol_mag_size |
| displayName | Tabanca Şarjörü |
| description | Her seviye şarjör kapasitesini %25 artırır. |
| category | Upgrades |
| prices | (3) |
| prices[0] | 150 |
| prices[1] | 350 |
| prices[2] | 600 |
| maxLevel | 3 |
| requiredItemId | — |
| effect | WeaponUpgrade |
| targetId | pistol |
| upgradeType | MagSize |
| value | 0 |

</details>

<details><summary><b>RifleDamage</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | rifle_damage |
| displayName | Tüfek Hasarı |
| description | Her seviye hasarı %10 artırır. |
| category | Upgrades |
| prices | (5) |
| prices[0] | 150 |
| prices[1] | 300 |
| prices[2] | 500 |
| prices[3] | 800 |
| prices[4] | 1200 |
| maxLevel | 5 |
| requiredItemId | unlock_rifle |
| effect | WeaponUpgrade |
| targetId | rifle |
| upgradeType | Damage |
| value | 0 |

</details>

<details><summary><b>RifleFireRate</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | rifle_fire_rate |
| displayName | Tüfek Atış Hızı |
| description | Her seviye atış hızını %8 artırır. |
| category | Upgrades |
| prices | (3) |
| prices[0] | 200 |
| prices[1] | 450 |
| prices[2] | 800 |
| maxLevel | 3 |
| requiredItemId | unlock_rifle |
| effect | WeaponUpgrade |
| targetId | rifle |
| upgradeType | FireRate |
| value | 0 |

</details>

<details><summary><b>RifleMagSize</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | rifle_mag_size |
| displayName | Tüfek Şarjörü |
| description | Her seviye şarjör kapasitesini %25 artırır. |
| category | Upgrades |
| prices | (3) |
| prices[0] | 150 |
| prices[1] | 350 |
| prices[2] | 600 |
| maxLevel | 3 |
| requiredItemId | unlock_rifle |
| effect | WeaponUpgrade |
| targetId | rifle |
| upgradeType | MagSize |
| value | 0 |

</details>

<details><summary><b>ShotgunDamage</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | shotgun_damage |
| displayName | Pompalı Hasarı |
| description | Her seviye hasarı %10 artırır. |
| category | Upgrades |
| prices | (5) |
| prices[0] | 150 |
| prices[1] | 300 |
| prices[2] | 500 |
| prices[3] | 800 |
| prices[4] | 1200 |
| maxLevel | 5 |
| requiredItemId | unlock_shotgun |
| effect | WeaponUpgrade |
| targetId | shotgun |
| upgradeType | Damage |
| value | 0 |

</details>

<details><summary><b>ShotgunFireRate</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | shotgun_fire_rate |
| displayName | Pompalı Atış Hızı |
| description | Her seviye atış hızını %8 artırır. |
| category | Upgrades |
| prices | (3) |
| prices[0] | 200 |
| prices[1] | 450 |
| prices[2] | 800 |
| maxLevel | 3 |
| requiredItemId | unlock_shotgun |
| effect | WeaponUpgrade |
| targetId | shotgun |
| upgradeType | FireRate |
| value | 0 |

</details>

<details><summary><b>ShotgunMagSize</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | shotgun_mag_size |
| displayName | Pompalı Şarjörü |
| description | Her seviye şarjör kapasitesini %25 artırır. |
| category | Upgrades |
| prices | (3) |
| prices[0] | 150 |
| prices[1] | 350 |
| prices[2] | 600 |
| maxLevel | 3 |
| requiredItemId | unlock_shotgun |
| effect | WeaponUpgrade |
| targetId | shotgun |
| upgradeType | MagSize |
| value | 0 |

</details>

<details><summary><b>UnlockRifle</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | unlock_rifle |
| displayName | Tüfek |
| description | Seri atış: saniyede 10 mermi × 18 hasar, 35 m menzil. |
| category | Weapons |
| prices | (1) |
| prices[0] | 900 |
| maxLevel | 1 |
| requiredItemId | — |
| effect | UnlockWeapon |
| targetId | rifle |
| upgradeType | Damage |
| value | 0 |

</details>

<details><summary><b>UnlockShotgun</b> (ShopItemData)</summary>

| Alan | Değer |
|---|---|
| id | unlock_shotgun |
| displayName | Pompalı |
| description | Yakın mesafede yıkıcı: 8 saçma × 12 hasar, 12 m menzil. |
| category | Weapons |
| prices | (1) |
| prices[0] | 400 |
| maxLevel | 1 |
| requiredItemId | — |
| effect | UnlockWeapon |
| targetId | shotgun |
| upgradeType | Damage |
| value | 0 |

</details>

<details><summary><b>ShopCatalog</b> (ShopCatalog)</summary>

| Alan | Değer |
|---|---|
| items | (16) |
| items[0] | UnlockShotgun |
| items[1] | UnlockRifle |
| items[2] | PistolDamage |
| items[3] | PistolFireRate |
| items[4] | PistolMagSize |
| items[5] | ShotgunDamage |
| items[6] | ShotgunFireRate |
| items[7] | ShotgunMagSize |
| items[8] | RifleDamage |
| items[9] | RifleFireRate |
| items[10] | RifleMagSize |
| items[11] | Magnet |
| items[12] | AmmoShotgun |
| items[13] | AmmoRifle |
| items[14] | HealthKit |
| items[15] | Fuel |

</details>

### Loot

<details><summary><b>LootConfig</b> (LootConfig)</summary>

| Alan | Değer |
|---|---|
| cratesPerDay | 6 |
| minDistance | 22 |
| maxDistance | 38 |
| openHoldDuration | 2 |
| interactionRange | 2 |
| openPrompt | Aç |
| lootTable | LootTable |
| placementAttempts | 30 |
| navMeshSampleDistance | 2 |
| obstacleClearance | 1.2 |
| minCrateSpacing | 6 |
| blockingMask | 2560 |
| cratePrefab | LootCrate |
| fuelPickupPrefab | FuelPickup |
| ammoPickupPrefab | AmmoPickup |
| cratePrewarm | 6 |
| pickupPrewarm | 3 |
| pickupLifetime | 30 |
| pickupBlinkDuration | 3 |
| pickupMotion.spawnHeight | 0.8 |
| pickupMotion.restHeight | 0.3 |
| pickupMotion.scatterMinDistance | 0.6 |
| pickupMotion.scatterMaxDistance | 1.6 |
| pickupMotion.arcHeight | 1.2 |
| pickupMotion.arcDuration | 0.45 |
| pickupMotion.spinSpeed | 180 |
| pickupMotion.blinkFrequency | 6 |
| pickupMotion.attractStartSpeed | 4 |
| pickupMotion.attractAcceleration | 40 |
| pickupMotion.attractMaxSpeed | 25 |
| pickupMotion.collectDistance | 0.4 |
| pickupMotion.collectHeight | 1 |

</details>

<details><summary><b>LootTable</b> (LootTableData)</summary>

| Alan | Değer |
|---|---|
| farDistance | 32 |
| nearRewards | (2) |
| nearRewards[0].type | Money |
| nearRewards[0].minAmount | 30 |
| nearRewards[0].maxAmount | 60 |
| nearRewards[0].weight | 1 |
| nearRewards[1].type | Fuel |
| nearRewards[1].minAmount | 15 |
| nearRewards[1].maxAmount | 15 |
| nearRewards[1].weight | 1 |
| farRewards | (3) |
| farRewards[0].type | Money |
| farRewards[0].minAmount | 80 |
| farRewards[0].maxAmount | 150 |
| farRewards[0].weight | 1 |
| farRewards[1].type | Fuel |
| farRewards[1].minAmount | 40 |
| farRewards[1].maxAmount | 40 |
| farRewards[1].weight | 1 |
| farRewards[2].type | AmmoPack |
| farRewards[2].minAmount | 1 |
| farRewards[2].maxAmount | 1 |
| farRewards[2].weight | 1 |
| ammoPack | (2) |
| ammoPack[0].weaponId | shotgun |
| ammoPack[0].amount | 12 |
| ammoPack[1].weaponId | rifle |
| ammoPack[1].amount | 60 |

</details>

### Progression

<details><summary><b>PerkCatalog</b> (PerkCatalog)</summary>

| Alan | Değer |
|---|---|
| perks | (14) |
| perks[0] | QuickTrigger |
| perks[1] | HeavyRounds |
| perks[2] | ThickSkin |
| perks[3] | Magnet |
| perks[4] | SturdyWalls |
| perks[5] | Greedy |
| perks[6] | Adrenaline |
| perks[7] | TurretMaster |
| perks[8] | Vampire |
| perks[9] | Hunter |
| perks[10] | Ricochet |
| perks[11] | ThornWalls |
| perks[12] | ShockTurret |
| perks[13] | ChainExplosion |

</details>

<details><summary><b>Adrenaline</b> (PerkData)</summary>

| Alan | Değer |
|---|---|
| id | adrenaline |
| displayName | Adrenalin |
| description | Atılma bekleme süresi −%40. |
| icon | — |
| rarity | Rare |
| maxStacks | 1 |
| effects | (1) |
| effects[0] | StatModifierEffect |
| effects[0].stat | DashCooldown |
| effects[0].type | PercentAdd |
| effects[0].value | -0.4 |

</details>

<details><summary><b>ChainExplosion</b> (PerkData)</summary>

| Alan | Değer |
|---|---|
| id | chain_explosion |
| displayName | Zincirleme Son |
| description | Öldürdüğün düşman %10 şansla patlar (3 m, 40 hasar). |
| icon | — |
| rarity | Epic |
| maxStacks | 1 |
| effects | (1) |
| effects[0] | FlagEffect |
| effects[0].flag | ChainExplosion |

</details>

<details><summary><b>Greedy</b> (PerkData)</summary>

| Alan | Değer |
|---|---|
| id | greedy |
| displayName | Açgözlü |
| description | Jeton değeri +%25. |
| icon | — |
| rarity | Rare |
| maxStacks | 2 |
| effects | (1) |
| effects[0] | StatModifierEffect |
| effects[0].stat | CoinValue |
| effects[0].type | PercentAdd |
| effects[0].value | 0.25 |

</details>

<details><summary><b>HeavyRounds</b> (PerkData)</summary>

| Alan | Değer |
|---|---|
| id | heavy_rounds |
| displayName | Ağır Mermi |
| description | Silah hasarı +%15. |
| icon | — |
| rarity | Common |
| maxStacks | 3 |
| effects | (1) |
| effects[0] | StatModifierEffect |
| effects[0].stat | PlayerDamage |
| effects[0].type | PercentAdd |
| effects[0].value | 0.15 |

</details>

<details><summary><b>Hunter</b> (PerkData)</summary>

| Alan | Değer |
|---|---|
| id | hunter |
| displayName | Avcı |
| description | Yavaşlamış düşmanlara +%50 hasar. |
| icon | — |
| rarity | Rare |
| maxStacks | 1 |
| effects | (1) |
| effects[0] | FlagEffect |
| effects[0].flag | Hunter |

</details>

<details><summary><b>Magnet</b> (PerkData)</summary>

| Alan | Değer |
|---|---|
| id | magnet |
| displayName | Mıknatıs |
| description | Jeton toplama yarıçapı +%50. |
| icon | — |
| rarity | Common |
| maxStacks | 2 |
| effects | (1) |
| effects[0] | StatModifierEffect |
| effects[0].stat | PickupRadius |
| effects[0].type | PercentAdd |
| effects[0].value | 0.5 |

</details>

<details><summary><b>QuickTrigger</b> (PerkData)</summary>

| Alan | Değer |
|---|---|
| id | quick_trigger |
| displayName | Hızlı Tetik |
| description | Atış hızı +%15. |
| icon | — |
| rarity | Common |
| maxStacks | 3 |
| effects | (1) |
| effects[0] | StatModifierEffect |
| effects[0].stat | PlayerFireRate |
| effects[0].type | PercentAdd |
| effects[0].value | 0.15 |

</details>

<details><summary><b>Ricochet</b> (PerkData)</summary>

| Alan | Değer |
|---|---|
| id | ricochet |
| displayName | Seken Mermi |
| description | Mermi vurduğu düşmandan en yakın ikinci düşmana seker (%50 hasar). |
| icon | — |
| rarity | Epic |
| maxStacks | 1 |
| effects | (1) |
| effects[0] | FlagEffect |
| effects[0].flag | Ricochet |

</details>

<details><summary><b>ShockTurret</b> (PerkData)</summary>

| Alan | Değer |
|---|---|
| id | shock_turret |
| displayName | Şok Taret |
| description | Taret vuruşları düşmanı 1 sn %25 yavaşlatır. |
| icon | — |
| rarity | Epic |
| maxStacks | 1 |
| effects | (1) |
| effects[0] | FlagEffect |
| effects[0].flag | ShockTurret |

</details>

<details><summary><b>SturdyWalls</b> (PerkData)</summary>

| Alan | Değer |
|---|---|
| id | sturdy_walls |
| displayName | Sağlam Duvar |
| description | Barikatların maks. canı +%30. |
| icon | — |
| rarity | Common |
| maxStacks | 2 |
| effects | (1) |
| effects[0] | StatModifierEffect |
| effects[0].stat | BarricadeMaxHealth |
| effects[0].type | PercentAdd |
| effects[0].value | 0.3 |

</details>

<details><summary><b>ThickSkin</b> (PerkData)</summary>

| Alan | Değer |
|---|---|
| id | thick_skin |
| displayName | Kalın Deri |
| description | Maks. can +25 ve 25 can yeniler. |
| icon | — |
| rarity | Common |
| maxStacks | 3 |
| effects | (2) |
| effects[0] | StatModifierEffect |
| effects[0].stat | PlayerMaxHealth |
| effects[0].type | Flat |
| effects[0].value | 25 |
| effects[1] | HealEffect |
| effects[1].amount | 25 |

</details>

<details><summary><b>ThornWalls</b> (PerkData)</summary>

| Alan | Değer |
|---|---|
| id | thorn_walls |
| displayName | Dikenli Duvar |
| description | Barikatlar aldıkları yakın dövüş hasarının %30'unu saldırana yansıtır. |
| icon | — |
| rarity | Epic |
| maxStacks | 1 |
| effects | (1) |
| effects[0] | FlagEffect |
| effects[0].flag | ThornWalls |

</details>

<details><summary><b>TurretMaster</b> (PerkData)</summary>

| Alan | Değer |
|---|---|
| id | turret_master |
| displayName | Taret Ustası |
| description | Taret atış hızı +%25. |
| icon | — |
| rarity | Rare |
| maxStacks | 2 |
| effects | (1) |
| effects[0] | StatModifierEffect |
| effects[0].stat | TurretFireRate |
| effects[0].type | PercentAdd |
| effects[0].value | 0.25 |

</details>

<details><summary><b>Vampire</b> (PerkData)</summary>

| Alan | Değer |
|---|---|
| id | vampire |
| displayName | Vampir |
| description | Her öldürme 2 can yeniler. |
| icon | — |
| rarity | Rare |
| maxStacks | 1 |
| effects | (1) |
| effects[0] | FlagEffect |
| effects[0].flag | Vampire |

</details>

<details><summary><b>ProgressionConfig</b> (ProgressionConfig)</summary>

| Alan | Değer |
|---|---|
| baseXp | 10 |
| xpPerLevel | 6 |
| startingLevel | 1 |
| playerKillsOnly | hayır |
| rarityWeights | (3) |
| rarityWeights[0].rarity | Common |
| rarityWeights[0].weight | 60 |
| rarityWeights[1].rarity | Rare |
| rarityWeights[1].weight | 30 |
| rarityWeights[2].rarity | Epic |
| rarityWeights[2].weight | 10 |
| offerSize | 3 |
| offerSeed | 0 |

</details>

### GameFlow

<details><summary><b>GameConfig</b> (GameConfig)</summary>

| Alan | Değer |
|---|---|
| dayDuration | 120 |
| finalWarningDuration | 15 |
| duskDuration | 4 |
| dawnLightTransitionDuration | 3 |
| earlyReadyBonusPerStep | 5 |
| earlyReadyStepSeconds | 10 |
| playerRespawnDelay | 10 |
| playerRespawnOffset | (0, 0, -2.5) |

</details>

<details><summary><b>SettingsConfig</b> (SettingsConfig)</summary>

| Alan | Değer |
|---|---|
| masterVolume | 1 |
| musicVolume | 1 |
| sfxVolume | 1 |
| cameraPreset | Isometric |
| screenShake | evet |
| fullscreen | evet |

</details>
<!-- balance:auto:end -->

## Analiz ve öneriler

> 15.1, 2026-10-22. Yukarıdaki tablolardan hesap; oyun içinde ölçülmedi. **Değerler değiştirilmedi**: aşağıdakiler öneridir, karar tasarımın.
> Debug paneli (F12: N. geceye git, zaman ölçeği, FPS) ile doğrulanabilir.

### Para ve savunma maliyeti

- **Erken oyun dengeli.** 1. gece öncesi eldeki para ≈ 300 + 177 (sandık) + 45 (gezgin) ≈ 520. Bu 1 taret (250) + 2 ahşap barikat (100) + yedek demek. 1. gecenin kendi öldürme parası 200.
- **Orta oyunda para bolluğu başlıyor.** Üst sınır gelir 2–4. gecelerde 730 → 1240. Kümülatif ≈ 3.800 olunca tüfek (900) ve ilk yükseltmeler 4. geceye kadar alınabiliyor.
- **Kan Ayı geceleri ekonominin yarısı.** 5. ve 10. gece öldürme parası 2.560 + 5.560: tüm öldürme parasının %47'si.
  - 5. gece tek başına 4. gecenin 2.8 katı. Bunun 1.000'i boss'un ×2'lenmiş ödülü.
  - 5. geceden sonra para kısıtı neredeyse kalkıyor.
  - 10. gecenin 5.560'ı ise run bittiği için harcanamıyor.
- **Toplam harcanabilir ↔ toplam gelir:**
  - Üç silah ve tüm yükseltmeleri: 17.800. 4 taret L3: 4.200. 6 barikat L3: 2.820.
  - Run boyunca üst sınır gelir ≈ 20.900. Oyuncu run sonunda neredeyse her şeyi alabiliyor; seçim baskısı geç oyunda zayıf.
- **Maliyet verimliliği:**
  - Taret L1 6.3 para/DPS. L2 12.5. L3 26 (+3 m menzil hariç). İkinci taret kurmak yükseltmekten 2–4 kat verimli; sadece yakıt +0.05/sn ve alan maliyeti var.
  - Pompalı ek DPS başına ≈ 20 para (tüm saçmalar isabet ederse), tüfek ≈ 16. 18° saçılmada gerçek pompalı DPS'i daha düşük, pompalı "tuzak alım" olabilir.

### Zorluk eğrisi

- **Toplam can 14 kat artıyor, gereken DPS 3 kat.** Can 1.200 → 17.325. Gereken DPS 90 → 278. Spawn süreleri de uzadığı için (13 → 62 sn) gece başı baskı yumuşak artıyor.
- **İki sıçrama var:**
  - 5. gece (Kan Ayı + Brute): 7.125 can. 6. geceden (5.700) zor; eğri burada geri düşüyor.
  - 10. gece: 9. geceye göre can +%57, gereken DPS +%40. Final için bilinçli olabilir.
- **1. gece:** Gereken 90 DPS, tabancanın sürekli DPS'inin (57) üstünde. Düşmanların 33 m yürümesi (≈ 15 sn) ve jeneratörün 1.500 canı farkı emiyor; inşa etmeyen yeni oyuncu yine de jeneratör hasarı yer.
- **10. gece:** Tam yükseltilmiş tüfek ≈ 234 sürekli DPS (hasar +%50, atış +%24, şarjör 52; perk'siz). Perk'ler ve 1–2 taretle 278 rahat aşılıyor. Son gece hesapta kolay görünüyor; asıl zorluk 4 yön + Brute (6.000) + Patlayan.
- **Seviye ve perk:**
  - Tüm öldürmeler XP verdiği için (`playerKillsOnly` kapalı) 10. gece sonunda en çok seviye 18 → 17 kart. Tüm perk yığını 24; geç gecelerde seçim daralıyor.
  - 3. geceden itibaren gece başına ~2 seviye: savaş ortasında sık duraklama.

### Riskli noktalar ve öneriler

1. **Kan Ayı jeton çarpanı (×2) + boss ödülü.**
   - Risk: 5. geceden sonra ekonomi gerilimi kayboluyor.
   - Öneri: boss ödülünü olay çarpanından muaf tut veya Kan Ayı jetonunu ×1.5'e indir.
2. **Yakıt baskısı yok.**
   - En uzun gecede bile taban tüketim ≈ 25 (+ taret başına ≈ 3). Gerçek gece 2 kat uzun sayılsa da başlangıç 100 + günlük sandık yakıtı ≈ 60 bunu fazlasıyla karşılıyor.
   - Güç riski sadece Fırtına kesintisi.
   - Öneri: önce panelin zaman ölçeğiyle gerçek gece süresini ölç; sonra taban tüketimi artır (0.4 → 0.8–1.0) veya sandık yakıtını azalt.
3. **Biriktirme ve erken "Hazırım" ödülsüz (GDD §2 sütun 1).**
   - Faiz tavanı 50; 6. gece gelirinin %3.4'ü.
   - Erken hazır en çok 60; gündüzü kullanınca beklenen sandık + gezgin geliri ≈ 220. Erken başlamak hep zarar.
   - Öneri: faiz tavanını geceyle büyüt veya erken hazır bonusunu 10 sn başına 5 → 10–15 yap.
4. **Havuz ön ısıtması gece boyutlarının altında** (duman testinde de uyarı verdi).
   - Yürüyen 40 / en çok 90 (10. gece), Koşucu 15 / 45, Tank 5 / 9.
   - Jeton 200: toplu öldürmede (F6 / geceyi atla) aşılıyor.
   - Oynanışta Instantiate takılma yapabilir. Öneri: ön ısıtmayı en kalabalık gecenin adedinden türet (Validate Project uyarabilir).
5. **Taret yükseltmeleri ve pompalı zayıf değer.**
   - Öneri: Taret L2 300 → 200 ve L3 500 → 350; ya da pompalı fiyatı 400 → 300 veya saçma hasarı 12 → 14.
   - Önce isabet oranını oyun içinde ölç.
6. **Perk doygunluğu.** Run sonunda 24 yığının ~17'si alınabiliyor. İstenirse XP eğrisinin eğimi 6 → 8 yapılabilir; ya da "hepsi alındı" durumunda kart yerine para/can seçeneği eklenebilir.
