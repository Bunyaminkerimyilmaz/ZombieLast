using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Etkileşilebilir nesnelerin kaydı ([Systems]/Interaction). Nesneler etkinleşince kaydolur, devre dışı kalınca silinir;
    /// oyuncu adayları buradan sorgular, sahnede arama yapılmaz. Mantık saf <see cref="InteractableSet"/>'te.
    /// </summary>
    public interface IInteractableRegistry
    {
        /// <summary>Kayıtlı nesne sayısı (o an etkileşim sunmayanlar dahil).</summary>
        int Count { get; }

        /// <summary>Ekler. Zaten kayıtlıysa false döner.</summary>
        bool Register(IInteractable interactable);

        /// <summary>Siler. Kayıtlı değilse false döner.</summary>
        bool Unregister(IInteractable interactable);

        /// <summary>
        /// <see cref="IInteractable.CanInteract"/> olan ve kendi <see cref="IInteractable.InteractionRange"/>'i içinde (dahil) kalan
        /// en yakın nesne; yoksa null. Eşitlikte önce kaydolan.
        /// </summary>
        IInteractable FindNearest(Vector3 position);
    }
}
