# QA Kontrol Listesi — Uçtan Uca Run (1. → 10. gece)

> Sürüm build'inde 1. geceden 10. geceye tam bir run'da kontrol edilecekler. Sayılar GDD'den (bölüm numaraları parantezde).
> Bir madde başarısızsa: gece numarası, ne yaptığın, ne gördüğün ve `Player.log`'u (aşağıda) ekleyerek kaydet.

## Hazırlık

- **Build:** Unity'de `Tools/Last Haven/Build/Windows` → `Builds/Windows/LastHaven.exe`. macOS için `.../Build/macOS` (Unity'de "Mac Build Support (Mono)" modülü kurulu olmalı).
- **Dağıtırken** `Last Haven_BurstDebugInformation_DoNotShip` klasörünü kopyalama.
- **Sürüm build'i:** debug paneli (F12) ve F1–F10 kısayolları kapalı. Hızlı test için (belirli geceye atlama, para, ölümsüzlük) editörü veya development build'i kullan; bu liste ise gerçek akışı test eder.
- **Log:** `%USERPROFILE%\AppData\LocalLow\Last Haven Team\Last Haven\Player.log` (macOS: `~/Library/Logs/Last Haven Team/Last Haven/Player.log`). Her oturumdan sonra içinde `Exception` ve `Error` ara.
- **Temiz başlangıç:** Ayarları sıfırlamak için kayıt defterinde `HKCU\Software\Last Haven Team\Last Haven` anahtarını sil.
- Run'ın bir kısmını **gamepad** ile oyna (madde 25).

## Açılış ve menüler

| # | Adım | Beklenen |
|---|---|---|
| 1 | `LastHaven.exe`'yi aç. | Kenarlıksız tam ekranda ana menü açılır (LAST HAVEN, GÜNDÜZ & GECE; arka planda gece sahnesi). Pencere başlığı "Last Haven". Oyna / Ayarlar / Çıkış. |
| 2 | Ayarlar: genel/müzik/efekt seslerini değiştir, kamerayı Tepeden yap, sarsıntıyı ve tam ekranı kapat. Oyunu kapatıp yeniden aç. | Değişiklikler anında uygulanır. Tam ekran kapanınca pencereye geçer. Yeniden açılışta tüm ayarlar korunur (§16). |
| 3 | Oyna. Oyunda F12, F1–F10 tuşlarına bas; üssün doğu-güneydoğusuna bak. | Debug paneli açılmaz, kısayollar bir şey yapmaz, üs dışında gri test kuklası yoktur (sürüm build'i). |

## 1. gündüz

| # | Adım | Beklenen |
|---|---|---|
| 4 | Oyun başlar başlamaz HUD'a bak. | GÜNDÜZ 2:00 geri sayımı, Gece 1/10, 100 can, jeneratör 1500, tabanca 12 mermi, 300 para / 100 yakıt, XP şeridi Sv 1 (§10, §16). |
| 5 | İstihbarata bak. | Kuzey yönünde saldırı oku, Kuzey bölgesinde kırmızı ışık sütunu, düşman listesinde "20 Yürüyen", olay yok (§8, §9). |
| 6 | Dış halkaya git; gezginlere yaklaş. | 3–6 gezgin dolaşır, üsse saldırmaz; yaklaşınca sana saldırır. Öldürülünce jeton düşer (§9). |
| 7 | 6 sandıktan birkaçını aç (E basılı 2 sn; bir kez yarıda bırak). | Yarıda bırakınca ilerleme sıfırlanır. Yakın (< 32 m): 30–60 para veya 15 yakıt; uzak: 80–150 para, 40 yakıt veya mermi paketi (§9). |
| 8 | Tab ile marketi aç; parası yetmeyen ve yeten birer öğe almayı dene. | Silahlar / Yükseltmeler / Malzeme sekmeleri. Yetmeyen öğede ret animasyonu, yeten öğede para düşer, kart güncellenir (§10). |
| 9 | B ile inşa modu: barikat (50) koy, Q ile döndür, U ile yükselt, X ile sat; taret (250) ve diken tuzağı (80) koy. Üs dışına ve jeneratörün dibine koymayı dene. | Geçerli yerde yeşil, geçersiz yerde/para yetmezken kırmızı önizleme. Satışta harcananın %50'si geri gelir. Alt çubukta seçili yapı ve fiyat görünür (§11). |
| 10 | Gündüz bir gezgine kendini öldürt; ölüyken B'ye bas. | İnşa modu açılmaz (açıksa kapanır). 10 sn sonra jeneratörün yanında tam canla doğarsın; kombo sıfırlanır, oyun bitmez (§5). |
| 11 | Enter ile geceyi erken başlat (sayaçta ~60 sn kalmışken). | Kalan her 10 sn için +5 para (~+30). 4 sn alacakaranlık, ışık maviye döner, inşa modu ve market kapanır (§3, §9). |

## Gece 1–3

| # | Adım | Beklenen |
|---|---|---|
| 12 | Gece 1'i savun. | 20 Yürüyen Kuzeyden gelir; kalan düşman sayacı azalır. Jetonlar saçılır, yanına gidince sana uçar, 12 sn sonra kaybolur (son 3 sn yanıp söner). Ardışık 5 öldürmede kombo ×1.5 (§8, §10). |
| 13 | Taretin çalıştığını izle; acil yakıtı dene (gece jeneratörün yanında E 1 sn). | Taret menzildeki en yakın düşmana döner ve ateş eder. Yakıt gece azalır; acil yakıt 30 para karşılığı +10 yakıt verir (§11, §12). |
| 14 | Hasarlı barikata 2 m yaklaşıp E'yi basılı tut. | Saniyede ~50 can dolar, her 5 can 1 para düşer; para bitince durur. Gece de çalışır (§11). |
| 15 | Seviye atla. | Oyun durur, çerçeve rengi nadirliği gösteren 3 kart çıkar; seçilen perk HUD'da görünür ve etkisi hissedilir (ör. Kalın Deri can barını büyütür) (§13). |
| 16 | Gece 1 bitince şafak panelini oku; Devam. | ŞAFAK: bu gece öldürülen, toplanan para, şafak faizi (paranın %10'u, en fazla 50), en yüksek kombo. Devam → GÜNDÜZ, Gece 2/10, yeni sandıklar (§10, §16). |
| 17 | Gece 2 ve 3'ü oyna. | Gece 2: Kuzey+Doğu, Koşucular hızlı. Gece 3: Kuzey, Doğu, Güney; Tank'lar barikatı hedefleyip kırar. Üs tamamen çevriliyse zombiler bir barikatı kırıp boşluktan girer (§7, §8). |

## Gece 4–7 (olaylar ve boss)

| # | Adım | Beklenen |
|---|---|---|
| 18 | Gece 4 öncesi istihbarat; sonra Gece 4. | İstihbaratta "Sis" önceden görünür. Gece yoğun sis, düşmanlar biraz daha hızlı; Tükürücüler menzilden asit atar, asit barikatın üstünden geçer (§7, §14). |
| 19 | Gece 5: Brute + Kan Ayı. | Kırmızı ay ışığı; düşman sayısı ×1.5, jetonlar ×2. Brute doğunca boss can barı (3000) çıkar; darbe öncesi yerde uyarı, darbede kamera sarsılır; canı azalınca öfkelenip hızlanır. Ölünce bar kaybolur (§7, §14). |
| 20 | Gece 6: Patlayanlar. | Patlayan barikatın/oyuncunun yanında patlar, alan hasarı verir, yakındaki Patlayanları zincirleme patlatır; kamera sarsılır (§7). |
| 21 | Gece 7: Fırtına. Yakıtı bilerek bitir (acil yakıt alma). | Her 25 sn'de 5 sn güç kesintisi: şimşek flaşı, taretler durur, üs ışıkları söner, sonra geri gelir. Yakıt 0'da "GÜÇ YOK" yanıp söner; yakıt gelince güç döner (§12, §14). |

## Gece 8–10 ve sonuç

| # | Adım | Beklenen |
|---|---|---|
| 22 | Gece 8 ve 9 (Gece 9'da Sis). Kalabalıkta oyna. | Dört yönden saldırı, ekranda çok düşman; belirgin takılma/donma yok. Kalan düşman sayacı tutarlı azalır ve 0'da şafak gelir (aynı anda en fazla 120 canlı; fazlası kuyrukta bekler) (§8). |
| 23 | Esc ile duraklat; Ayarlar'a girip çık; Devam. | DURAKLATILDI: oyun tamamen durur (düşmanlar, sayaçlar). Ayarlar açılıp kapanır, Devam ile kaldığı yerden sürer (§16). |
| 24 | Gece 10: Final Brute (6000) + Kan Ayı; geceyi bitir. | ZAFER: "Tahliye geldi!", atlatılan gece 10/10, toplam öldürme, kazanılan para, en yüksek kombo, alınan perk'ler. Tekrar Oyna → yeni run (Gece 1, 300 para / 100 yakıt); Ana Menü → ana menü (§15). |
| 25 | Yeni bir run'da jeneratörü bilerek yıktır (ör. Gece 2'de savunma yapma). | OYUN BİTTİ: "Jeneratör yıkıldı.", ulaşılan gece, öldürme, para, kombo, perk'ler. Tekrar Oyna yeni run başlatır; önceki run'ın perk ve yapıları gelmez (§15). |
| 26 | Bir gece boyunca yalnızca gamepad ile oyna. | Sol çubuk hareket, sağ çubuk nişan, RT ateş, A atılma, LB inşa modu (D-pad seçim/yükselt/sat), Select market, Start basılı = Hazırım, Start = duraklat; kartlar ve menüler D-pad + A ile seçilir (§4). |
| 27 | Oyundan Çıkış ile çık; `Player.log`'u aç. | Oyun temiz kapanır. Logda `Exception` / `NullReference` / kırmızı hata yoktur. |
