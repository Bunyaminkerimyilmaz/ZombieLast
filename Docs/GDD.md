# Last Haven: Day & Night — Mini GDD

> Sürüm 0.1 · Tasarımın tek doğru kaynağı. Buradaki sayılar başlangıç değerleridir; dengeleme oyun içinde ScriptableObject'ler üzerinden yapılır.

## 1. Özet

| | |
|---|---|
| **Tür** | Top-down shooter + üs savunma + ekonomi (roguelite dokunuşlu) |
| **Platform** | PC (Windows / macOS), klavye-fare + gamepad |
| **Kamera** | 3D; izometrik (varsayılan) veya tepeden, ayarlardan seçilir |
| **Oturum** | 10 gece ≈ 40–60 dk |
| **Motor** | Unity 6 LTS, URP |

**Tek cümle:** Gündüz dışarı çıkıp topla ve üssünü kur; gece dört yönden gelen sürülere karşı jeneratörünü koru. Jeneratör yıkılırsa her şey biter.

## 2. Tasarım Sütunları

1. **Gündüz açgözlülük, gece bedel.** Gündüz aldığın her risk (uzak sandık, harcamadan biriktirmek, erken "Hazırım") gece karşına çıkar.
2. **Kahraman mı, mühendis mi?** Oyuncu sürekli seçim yapar: dışarıda jeton toplamak mı, içeride duvar onarmak mı?
3. **Her gece farklı.** Saldırı yönü, düşman karışımı, gece olayı ve perk seçimleri her geceyi değiştirir.
4. **Okunaklı kaos.** Ekranda yüzlerce zombi olsa da oyuncu ne olduğunu anlar: net renkler, net sesler, net geri bildirim.

## 3. Oyun Döngüsü

```
┌─► GÜNDÜZ (120 sn) ─► ALACAKARANLIK (4 sn) ─► GECE (dalgalar bitene kadar) ─► ŞAFAK (sonuç) ─┐
│   market, inşa,                              savaş, jeton topla,               istatistik,    │
│   sandık, istihbarat                         onar, perk seç                    faiz           │
└─────────────────────────────────────────────────────────────────────────────────────────────┘
10. gece biterse → ZAFER (tahliye)        Jeneratör yıkılırsa → OYUN BİTTİ
```

## 4. Kamera ve Kontroller

**Kamera** (Cinemachine, perspektif). Oyuncuyu takip eder ve nişan yönüne doğru en fazla **3 m** kayar (look-ahead).

| Preset | Pitch | Yaw | Mesafe | FOV |
|---|---|---|---|---|
| İzometrik (varsayılan) | 50° | 45° | 22 m | 35 |
| Tepeden | 75° | 0° | 24 m | 40 |

Hareket kamera yönüne göredir: W her zaman ekranın yukarısıdır.

| Eylem | Klavye / Fare | Gamepad |
|---|---|---|
| Hareket | WASD | Sol çubuk |
| Nişan | Fare | Sağ çubuk |
| Ateş | Sol tık (basılı) | RT |
| Şarjör | R | X / West |
| Atılma (dash) | Space | A / South |
| Silah 1–3 | 1, 2, 3 / tekerlek | Y / North (sıradaki) |
| Etkileşim (basılı tut) | E | B / East |
| İnşa modu | B | LB |
| Döndür (inşa) | Q | RB |
| Yükselt / Sat (inşa) | U / X | D-pad yukarı / aşağı |
| Yapı seç (inşa) | 1–5 | D-pad sol / sağ |
| İptal (inşa) | Sağ tık | B / East |
| Market | Tab | View / Select |
| Hazırım (geceyi başlat) | Enter | Start (basılı tut) |
| Duraklat | Esc | Start |

## 5. Oyuncu

| Özellik | Değer |
|---|---|
| Can | 100 (doğal yenilenme yok) |
| Hız | 6 m/s |
| Dash | 5 m / 0.18 sn, bekleme 2 sn, dash süresince hasar almaz |
| Hasar sonrası dokunulmazlık | 0.3 sn |
| Toplama (mıknatıs) yarıçapı | 3 m |
| Ölüm | 10 sn sonra jeneratörün yanında tam canla doğar. Kombo sıfırlanır. Oyun bitmez. |

## 6. Silahlar

Hitscan (raycast) + görsel mermi izi. Hasar mesafeyle düşmez.

| Silah | Hasar | Atış/sn | Şarjör | Yedek | Doldurma | Saçılma | Menzil | Fiyat |
|---|---|---|---|---|---|---|---|---|
| Tabanca | 20 | 4 | 12 | ∞ | 1.2 sn | 2° | 25 m | Başlangıç |
| Pompalı | 12 × 8 saçma | 1.1 | 6 | 36 | 2.0 sn | 18° | 12 m | 400 |
| Tüfek | 18 | 10 | 30 | 180 | 1.8 sn | 4° | 35 m | 900 |

**Yükseltmeler** (her silah için ayrı):

| Yükseltme | Etki / seviye | Seviye fiyatları |
|---|---|---|
| Hasar | +10% | 150 / 300 / 500 / 800 / 1200 |
| Atış hızı | +8% | 200 / 450 / 800 |
| Şarjör | +25% | 150 / 350 / 600 |

## 7. Düşmanlar

| Düşman | Can | Hız | Hasar | Saldırı/sn | Menzil | Para | XP | İlk gece | Özel |
|---|---|---|---|---|---|---|---|---|---|
| Yürüyen (Walker) | 60 | 2.2 | 10 | 1.0 | 1.5 m | 10 | 1 | 1 | — |
| Koşucu (Runner) | 35 | 5.0 | 6 | 1.5 | 1.3 m | 12 | 1 | 2 | — |
| Tank | 400 | 1.6 | 35 | 0.6 | 2.0 m | 60 | 5 | 3 | Yapılara ×2 hasar, önce barikatı hedefler, geri itilmez |
| Tükürücü (Spitter) | 70 | 2.5 | 12 | 0.5 | 10 m (mermi) | 25 | 2 | 4 | Menzilli, önce taretleri hedefler |
| Patlayan (Exploder) | 50 | 3.5 | 60 (3 m alan) | — | 1.2 m | 20 | 2 | 6 | Yapıya/oyuncuya değince veya öldürülünce patlar, yakındakileri zincirleme tetikler, yapılara ×2 |
| **Brute (Boss)** | 3000 / 6000 | 1.8 | 40 (4 m alan) | 1 / 6 sn | 4 m | 500 | 30 | 5, 10 | Vuruştan 1 sn önce yerde uyarı dairesi; %50 can altında öfke (hız ×1.4); geri itilmez |

**Hedef seçimi** (0.5 sn'de bir yeniden değerlendirilir, her frame değil):

1. Oyuncu aggro yarıçapındaysa (6 m; Tank 3 m) → oyuncu.
2. Değilse ve tipin tercih ettiği hedef 15 m içindeyse → o hedef (Tank: barikat, Tükürücü: taret).
3. Değilse → jeneratör.
4. Yol tıkalıysa (`PathPartial`) → yolun sonuna en yakın yapı.

**Geri bildirim:**
- Vuruşta 0.08 sn beyaz flaş ve 0.3 m geri itilme (direnç düşman verisinden gelir).
- Ölünce devrilir, 2 sn sonra yere batarak kaybolur.

## 8. Gece Yapısı ve Dalgalar

- Her gece bir `NightData`'dır. İçindekiler:
  - spawn grupları (düşman tipi, adet, aralık, başlangıç gecikmesi, yönler)
  - gece olayı (opsiyonel)
  - boss (opsiyonel)
- 4 spawn bölgesi var: **Kuzey, Doğu, Güney, Batı**. Harita kenarındadırlar, merkezden ~33 m uzakta ve 6 m yarıçaplıdır (33 + 6 = 39 m; bölgeler 40 m'deki harita kenarının içinde kalır, §17).
- Spawn aralığı varsayılan olarak 0.6–1.2 sn'dir. Ağır düşmanlar grubun ortalarında gelir.
- Aynı anda en fazla **120** canlı düşman bulunur. Fazlası kuyrukta bekler.
- Gece, tüm gruplar spawn olduğunda ve canlı düşman sayısı 0'a indiğinde biter.

| Gece | Yönler | İçerik | Olay |
|---|---|---|---|
| 1 | K | 20 Yürüyen | — |
| 2 | K, D | 30 Yürüyen, 8 Koşucu | — |
| 3 | K, D, G | 35 Yürüyen, 15 Koşucu, 2 Tank | — |
| 4 | D, B | 40 Yürüyen, 15 Koşucu, 3 Tank, 6 Tükürücü | Sis |
| 5 | Hepsi | **Boss: Brute (3000)** + 40 Yürüyen, 10 Koşucu | Kan Ayı |
| 6 | K, G | 50 Yürüyen, 20 Koşucu, 4 Tank, 8 Patlayan | — |
| 7 | D, G, B | 55 Yürüyen, 25 Koşucu, 5 Tank, 10 Tükürücü, 8 Patlayan | Fırtına |
| 8 | Hepsi | 70 Yürüyen, 30 Koşucu, 6 Tank, 12 Tükürücü, 12 Patlayan | — |
| 9 | Hepsi | 80 Yürüyen, 35 Koşucu, 8 Tank, 15 Tükürücü, 15 Patlayan | Sis |
| 10 | Hepsi | **Final Boss: Brute (6000)** + 60 Yürüyen, 30 Koşucu, 6 Tank, 10 Patlayan | Kan Ayı |

## 9. Gündüz Evresi

- **Süre:** 120 sn. "Hazırım" ile erken bitirilebilir.
- **Erken hazır bonusu:** Kalan her 10 sn için +5 para.
- **Son 15 sn:** Büyük geri sayım ve ekran kenarında kırmızı nabız.
- **Yapılabilecekler:** market, inşa / yükseltme / satış, ganimet sandıkları, istihbaratı okuma.
- **Gündüz tehlikesi:** Dış halkada 3–6 Yürüyen "gezgin" olarak dolaşır. Üsse saldırmazlar, ama oyuncu aggro yarıçapına girerse ona saldırırlar. Gece başlayınca normal düşmana dönüşürler.
- **Ganimet sandıkları:**
  - Her gün 6 tane çıkar, merkezden 22–38 m uzakta rastgele noktalarda.
  - Açmak için E'yi 2 sn basılı tut. Bırakırsan ilerleme sıfırlanır.
  - Gece başlayınca açılmamış sandıklar kaybolur.

  | Uzaklık | Ödül (biri seçilir) |
  |---|---|
  | Yakın (< 32 m) | 30–60 para · 15 yakıt |
  | Uzak (≥ 32 m) | 80–150 para · 40 yakıt · mermi paketi |
- **İstihbarat:** HUD bu gecenin bilgilerini gösterir:
  - saldırı yönleri (ekran kenarında oklar)
  - düşman tipleri ve adetleri
  - gece olayı

  Aktif spawn bölgelerinde kırmızı ışık sütunu yanar.

## 10. Ekonomi

**Kaynaklar:** Para (her şey için) ve Yakıt (jeneratör için). Başlangıçta **300 para, 100 yakıt** vardır.

- **Jeton düşürme:**
  - Ölen düşmanın ödülü 1–5 jetona bölünüp etrafa saçılır.
  - Jetonlar 12 sn sonra kaybolur; son 3 sn yanıp söner.
  - Oyuncu toplama yarıçapına girince jetonlar ona doğru uçar.
- **Kombo:** Öldürmeler arasındaki pencere 2.5 sn'dir.
  - 5 öldürme → ×1.5, 15 → ×2, 30 → ×3.
  - Oyuncu hasar alınca veya pencere dolunca sıfırlanır.
  - Öldürme anındaki çarpan o düşmanın jeton değerine uygulanır.
- **Faiz:** Şafakta mevcut paranın %10'u eklenir (en fazla 50).
- **Satış:** Yapıya harcanan toplam paranın %50'si iade edilir.

| Market öğesi | Fiyat |
|---|---|
| Pompalı | 400 |
| Tüfek | 900 |
| Pompalı mermisi (12) | 40 |
| Tüfek mermisi (60) | 50 |
| Can paketi (+50 can) | 60 |
| Yakıt (10 birim) | 15 |
| Mıknatıs +1 m (3 seviye) | 100 / 200 / 350 |
| Silah yükseltmeleri | §6 |

Market sadece gündüz açıktır.

## 11. İnşa ve Savunma

- İnşa, yükseltme ve satış **sadece gündüz** yapılır. **Onarım her zaman** yapılabilir.
- **Izgara:** 1 m hücre. İnşa alanı jeneratör merkezli **14 m** yarıçaptır. Jeneratörün 2 m çevresine inşa edilemez.
- **Önizleme:** Yeşil = geçerli. Kırmızı = alan dışı, çakışma veya para yetersiz. Q ile 90° döner.
- **Onarım:** Yapıya 2 m yakınken E'yi basılı tut. Hız 50 can/sn, maliyet 5 can başına 1 para. Para biterse onarım durur.

| Yapı | Boyut | Seviye 1 | Seviye 2 | Seviye 3 | Not |
|---|---|---|---|---|---|
| Barikat | 3×1 | Ahşap: 300 can · **50** para | Metal: 800 can · **+120** | Beton: 2000 can · **+300** | NavMesh'i keser (carve) |
| Taret | 2×2 | 8 hasar, 5 atış/sn, 12 m, 250 can · **250** para | Hasar ×1.6 · **+300** | Menzil +3 m, atış hızı ×1.3 · **+500** | Güç gerektirir; menzildeki en yakın düşmanı hedefler |
| Diken Tuzağı | 2×2 | 15 hasar/sn, %40 yavaşlatma, dayanıklılık 40 · **80** para | — | — | Üzerinden geçilir, NavMesh'i kesmez. Hasar tiki 0.5 sn'de bir; her tik dayanıklılıktan 1 düşer, bitince kırılır. |

## 12. Jeneratör ve Güç

- Üssün merkezindedir, **1500 can**. Yıkılırsa oyun biter.
- **Yakıt kapasitesi:** 150.
- **Tüketim:** Sadece gece. Taban 0.4/sn, her taret için +0.05/sn.
- **Yakıt 0 olursa:** Güç kesilir. Taretler durur, üs ışıkları söner, HUD'da "GÜÇ YOK" uyarısı çıkar. Yakıt gelince güç geri döner.
- **Acil yakıt (gece):** Jeneratöre yakınken E'yi 1 sn basılı tut → 30 para karşılığı 10 yakıt (normal fiyatın 2 katı).
- Jeneratör çevresindeki 4 nokta ışık gece üssü aydınlatır.

## 13. Seviye ve Perk'ler

- **XP:** Düşman öldürünce otomatik kazanılır (§7). Sonraki seviye için gereken XP: `10 + 6 × mevcutSeviye`.
- **Seviye atlayınca:**
  - Oyun duraklar ve 3 farklı perk kartı sunulur.
  - Nadirlik ağırlıkları: Sıradan 60, Nadir 30, Epik 10.
  - Maks. yığına ulaşmış perk tekrar çıkmaz.
  - Seçilen perk run boyunca kalır.

| Perk | Nadirlik | Etki | Maks. yığın |
|---|---|---|---|
| Hızlı Tetik | Sıradan | Atış hızı +15% | 3 |
| Ağır Mermi | Sıradan | Hasar +15% | 3 |
| Kalın Deri | Sıradan | Maks. can +25 (ve o kadar iyileştirir) | 3 |
| Mıknatıs | Sıradan | Toplama yarıçapı +50% | 2 |
| Sağlam Duvar | Sıradan | Barikat maks. canı +30% | 2 |
| Açgözlü | Nadir | Jeton değeri +25% | 2 |
| Adrenalin | Nadir | Dash bekleme süresi −40% | 1 |
| Taret Ustası | Nadir | Taret atış hızı +25% | 2 |
| Vampir | Nadir | Her öldürme +2 can | 1 |
| Avcı | Nadir | Yavaşlamış düşmanlara +50% hasar | 1 |
| Seken Mermi | Epik | Mermi vurduktan sonra en yakın ikinci düşmana seker (%50 hasar) | 1 |
| Dikenli Duvar | Epik | Barikatlar aldığı yakın dövüş hasarının %30'unu yansıtır | 1 |
| Şok Taret | Epik | Taret vuruşları 1 sn %25 yavaşlatır | 1 |
| Zincirleme Son | Epik | Oyuncunun öldürdüğü düşman %10 şansla patlar (3 m, 40 hasar) | 1 |

**Planlanan sinerjiler:**
- Diken Tuzağı veya Şok Taret yavaşlatır, Avcı yavaşlamış düşmana ek hasar verir.
- Zincirleme Son kalabalıkta zincir patlamalar yaratır.

## 14. Gece Olayları

| Olay | Etki | Görsel |
|---|---|---|
| Kan Ayı | Düşman sayısı ×1.5, jeton değeri ×2 | Kırmızı ay ışığı |
| Sis | Görüş ~12 m, düşman hızı +10% | Yoğun sis |
| Fırtına | Her 25 sn'de 5 sn güç kesintisi | Şimşek flaşı + hafif sarsıntı |

Olay gündüz istihbaratta önceden görünür.

## 15. Kazanma / Kaybetme

- **Zafer:** 10. gece tamamlanır → "Tahliye" ekranı ve istatistikler.
- **Yenilgi:** Jeneratör yıkılır → "Oyun Bitti" ekranı. Gösterilenler: ulaşılan gece, öldürme sayısı, kazanılan para, en yüksek kombo, alınan perk'ler.
- **Oyuncunun ölümü** oyunu bitirmez (§5).

## 16. Arayüz

- **HUD:**
  - oyuncu canı
  - silah adı ve mermi (şarjör / yedek)
  - para
  - yakıt ve güç durumu
  - jeneratör canı
  - evre adı ve zamanlayıcı
  - Gece X/10
  - kalan düşman
  - kombo
  - XP barı ve seviye
  - istihbarat okları (gündüz)
  - boss can barı
  - alınan perk'ler
  - etkileşim ipucu ("E — Aç")
- **Market** (Tab, gündüz): sekmeler Silahlar / Yükseltmeler / Malzeme.
- **İnşa çubuğu:** İnşa modunda alt çubuk. Yapı üzerine gelince tooltip çıkar.
- **Seviye kartları:** 3 kart; çerçeve rengi nadirliği gösterir.
- **Şafak ekranı:** öldürme, kazanılan para, faiz, en yüksek kombo.
- **Diğer ekranlar:** Ana Menü, Duraklat, Ayarlar, Oyun Bitti, Zafer.
- **Ayarlar:** ses (genel / müzik / efekt), kamera preset'i, ekran sarsıntısı aç/kapa, tam ekran.
- **Hasar sayıları ve "+12" jeton yazıları:** dünyadan ekrana yansıtılır.

## 17. Harita (MVP)

- 80×80 m düz zemin. Merkezde jeneratör, çevresinde 14 m yarıçaplı üs alanı (zeminde farklı renk daire).
- Harita kenarında 4 spawn bölgesi (§8).
- Dış halkada (18–38 m) ~25 engel: araba (4×2×1.5 küp), kaya, duvar parçası. Sabit seed ile yerleşir.
- Harita sınırında görünmez duvarlar.

## 18. Görsel ve Ses Yönü (Placeholder dönemi)

Asset'ler en son elle eklenir. O zamana kadar her şey primitive şekil ve renk kodlu URP/Lit materyallerle çalışır.

| Varlık | Şekil | Renk |
|---|---|---|
| Oyuncu | Kapsül + öne bakan küçük küp (burun) | Mavi |
| Yürüyen | Kapsül | Yeşil |
| Koşucu | İnce kapsül (×0.8) | Sarı-yeşil |
| Tank | Büyük kapsül (×1.6) | Koyu kırmızı |
| Tükürücü | Kapsül | Mor |
| Patlayan | Küre (yürürken nabız gibi büyür-küçülür) | Turuncu |
| Brute | Dev kapsül (×2.5) | Siyah-kırmızı |
| Jeneratör | 2×2×2 küp + nokta ışık | Sarı |
| Barikat | 3×1×1.5 küp | Kahve → gri → beyaz (seviyeye göre) |
| Taret | Silindir gövde + küp kafa | Camgöbeği |
| Diken Tuzağı | Yassı küp | Koyu gri |
| Jeton | Küçük yassı silindir | Altın |
| Ganimet sandığı | Küp | Turuncu-kahve |
| Zemin / Üs alanı / Engel | Plane / daire / küpler | Koyu gri / biraz açık gri / kahve-gri |

- **Işık:** Gündüz sıcak beyaz. Gece koyu mavi ambiyans; oyuncunun feneri (spot ışık) ve üs ışıkları yanar.
- **Ses:** Tüm sesler `SoundData` ile oyun olaylarına bağlanır; klip yoksa sessiz kalır. Müzik: Gündüz (sakin), Gece (gerilim), Boss, Zafer, Yenilgi.

## 19. Kapsam Dışı (v2 fikirleri)

- NPC hayatta kalanlar (tamirci, nişancı, toplayıcı, doktor)
- Karakter sınıfları (Mühendis, Asker, Çöpçü, Saha Doktoru)
- Run'lar arası kalıcı ilerleme (madalyalar, kilit açma)
- Ses/gürültü sistemi (gürültü zombi çeker)
- Kazıcı ve Çığlıkçı zombiler
- Mutajen kaynağı
- Sonsuz mod
- Co-op
