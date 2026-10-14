using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>Düşmanların hedef olarak seçebileceği nesne.</summary>
    public interface ITargetable
    {
        Transform Transform { get; }

        TargetType TargetType { get; }

        /// <summary>Şu an hedeflenebilir mi (ör. ölü değil, inşa önizlemesi değil).</summary>
        bool IsTargetable { get; }
    }
}
