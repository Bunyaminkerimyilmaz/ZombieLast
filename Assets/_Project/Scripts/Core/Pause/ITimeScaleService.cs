namespace LastHaven.Core
{
    /// <summary>
    /// Oyunun normal hız çarpanı (<c>PauseService</c>, [Systems]/Pause, Awake'te kaydolur; debug paneli 0.5× / 1× / 2× / 4× için
    /// kullanır). Zaman ölçeğinin tek sahibi duraklatma servisi olduğu için <c>Time.timeScale</c>'e doğrudan yazılmaz: oyun
    /// duraklıyken veya donmuşken (<see cref="IPauseService"/>, <see cref="IHitStopService"/>) yeni ölçek saklanır ve oyun devam edince
    /// geçerli olur. Sahne kapanınca (duraklatma servisi yok olunca) ölçek 1'e döner.
    /// </summary>
    public interface ITimeScaleService
    {
        /// <summary>Oyun akarken geçerli ölçek (duraklıyken de devam edince dönülecek değer). Başlangıçta 1.</summary>
        float TimeScale { get; }

        /// <summary>Normal ölçeği değiştirir. 0 veya negatif değer yok sayılır (duraklatma <see cref="IPauseService"/>'in işi).</summary>
        void SetTimeScale(float scale);
    }
}
