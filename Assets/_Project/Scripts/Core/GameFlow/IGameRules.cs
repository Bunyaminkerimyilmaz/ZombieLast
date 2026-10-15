using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Oyun akışı ayarlarından (GameFlow'daki <c>GameConfig</c>) diğer modüllerin okuduğu kurallar. GameFlow'u referans alamayan
    /// modüller (ör. Player'daki yeniden doğma) bunu ServiceLocator'dan alır; kaydı <c>GameFlowController</c> yapar.
    /// </summary>
    public interface IGameRules
    {
        /// <summary>Oyuncunun ölümden sonra yeniden doğma süresi (sn, GDD §5).</summary>
        float PlayerRespawnDelay { get; }

        /// <summary>Yeniden doğma noktası: <see cref="IBaseArea.Center"/>'a eklenen dünya uzayı konum (m).</summary>
        Vector3 PlayerRespawnOffset { get; }
    }
}
