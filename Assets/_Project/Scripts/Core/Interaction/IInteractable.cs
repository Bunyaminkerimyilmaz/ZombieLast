using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Oyuncunun etkileşim tuşunu (E / B-East) basılı tutarak kullandığı nesne: yapı onarımı (GDD §11), acil yakıt (§12), ganimet
    /// sandığı (§9). Kendini <see cref="IInteractableRegistry"/>'ye kaydeder; oyuncunun <c>Interactor</c>'ı menzildeki en yakın
    /// <see cref="CanInteract"/> olanı seçer. Basılı tutma <see cref="HoldDuration"/> kadar sürünce <see cref="OnInteract"/> çağrılır;
    /// tuş basılı kalırsa süre baştan sayılır ve etkileşim tekrarlanır (onarım böyle sürer).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>İpucunun dünyadaki konumu (UI ekrana yansıtır), ör. yapının tepesi.</summary>
        Vector3 PromptPosition { get; }

        /// <summary>
        /// İpucu metni, ör. "Onar (12)". Tuş adını UI ekler. Değişmedikçe aynı string örneğini döndür: dinleyiciler değişimi
        /// referansla anlar, her kare yeni string oluşturulmaz.
        /// </summary>
        string Prompt { get; }

        /// <summary>Şu an etkileşim sunuyor mu (ör. yapı hasarlı). Değilse seçilmez ve ipucu çıkmaz.</summary>
        bool CanInteract { get; }

        /// <summary>Bir etkileşim için basılı tutma süresi (sn). 0 veya altı: her karede bir etkileşim.</summary>
        float HoldDuration { get; }

        /// <summary>Oyuncu bu yatay mesafeden (m, sınır dahil) yakınsa etkileşebilir.</summary>
        float InteractionRange { get; }

        /// <summary>Noktadan nesneye yatay (XZ) uzaklık (m); yapılarda ayak izinin kenarına.</summary>
        float GetDistance(Vector3 point);

        /// <summary>
        /// Basılı tutulurken her kare o turun ilerlemesi (0..1); tur bitince 1. Basılı tutma kesilince (tuş bırakıldı, hedef değişti,
        /// <see cref="OnInteract"/> false döndü) bir kez 0.
        /// </summary>
        void OnHoldProgress(float progress);

        /// <summary>
        /// Basılı tutma süresi doldu: etkileşimi uygula. False dönerse (ör. para yetmedi) basılı tutma durur; tekrar başlamak için
        /// tuş bırakılıp yeniden basılmalı.
        /// </summary>
        bool OnInteract();
    }
}
