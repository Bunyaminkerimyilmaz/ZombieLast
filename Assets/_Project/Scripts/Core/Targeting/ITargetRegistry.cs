using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Hedeflenebilir nesnelerin tür bazlı kaydı. Hedefler kendini etkinleşince kaydeder, devre dışı kalınca siler.
    /// Düşmanlar adayları buradan sorgular; sahnede arama yapılmaz.
    /// </summary>
    public interface ITargetRegistry
    {
        /// <summary>Hedefi kendi türünün listesine ekler. Zaten kayıtlıysa false döner.</summary>
        bool Register(ITargetable target);

        /// <summary>Hedefi siler. Kayıtlı değilse false döner.</summary>
        bool Unregister(ITargetable target);

        /// <summary>
        /// Bu türün <see cref="ITargetable.IsTargetable"/> olan en yakın hedefi; yoksa null.
        /// Mesafe yatay düzlemde (XZ) ölçülür ve <paramref name="maxDistance"/> dahildir.
        /// </summary>
        ITargetable FindNearest(TargetType type, Vector3 position, float maxDistance);

        /// <summary>Bu türün kayıtlı tüm hedefleri (o an hedeflenemeyenler dahil). Liste bellek ayırmaz; saklama, değiştirme.</summary>
        IReadOnlyList<ITargetable> GetAll(TargetType type);
    }
}
