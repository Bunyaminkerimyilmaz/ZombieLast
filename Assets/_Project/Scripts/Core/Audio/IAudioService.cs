using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Ses efekti servisi (Audio modülü uygular, [Systems]/Audio). Havuzlanmış ses kaynaklarıyla çalar, SFX mixer grubuna yollar.
    /// Ses null ise, klibi yoksa ya da sınırına (aynı anda en fazla / bekleme / toplam kaynak) takılırsa istek sessizce atlanır.
    /// Çalan ses oyun duraklasa da (<c>Time.timeScale</c> 0) sürer; süreler gerçek zamanla sayılır.
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Oyun sesi: <see cref="SoundData.Spatial"/> 3D ise <paramref name="position"/>'dan uzaklıkla zayıflar, 2D ise konum yok sayılır.
        /// </summary>
        void PlaySfx(SoundData sound, Vector3 position);

        /// <summary>Arayüz sesi: her zaman 2D (konumsuz), verideki 2D/3D ayarından bağımsız.</summary>
        void PlayUi(SoundData sound);
    }
}
