namespace LastHaven.Core
{
    /// <summary>
    /// Havuzlanan objenin bileşenleri (kökte ve çocuklarda) havuz olaylarını bu arayüzle alır.
    /// Durum Instantiate/Destroy yerine burada sıfırlanır.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Havuzdan çıkınca çağrılır: obje konumlandırılıp etkinleştirildikten sonra.</summary>
        void OnSpawned();

        /// <summary>Havuza dönerken çağrılır: obje kapatılmadan önce.</summary>
        void OnDespawned();
    }
}
