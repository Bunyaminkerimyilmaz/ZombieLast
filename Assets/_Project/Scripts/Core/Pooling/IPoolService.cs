using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Prefab başına obje havuzu. Oynanış sırasında Instantiate/Destroy yerine kullanılır (mermi izi, düşman, jeton, VFX...).
    /// Havuzdan çıkan objenin <see cref="IPoolable"/> bileşenleri <see cref="IPoolable.OnSpawned"/> /
    /// <see cref="IPoolable.OnDespawned"/> alır.
    /// </summary>
    public interface IPoolService
    {
        /// <summary>
        /// Prefab'ın havuzundan bir örnek alır (boşsa oluşturur), konumlandırır ve etkinleştirir.
        /// <paramref name="prefab"/> null ise hata loglar ve null döner.
        /// </summary>
        GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation);

        /// <summary>
        /// <see cref="Get(GameObject, Vector3, Quaternion)"/> + örneğin <typeparamref name="T"/> bileşeni.
        /// Bileşen örnek başına önbelleklenir; her çağrıda GetComponent yapılmaz.
        /// </summary>
        T Get<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component;

        /// <summary>
        /// Örneği havuza iade eder: <see cref="IPoolable.OnDespawned"/>, sonra obje kapanır.
        /// Zaten havuzdaysa uyarı verir ve yok sayar. Bu havuzdan gelmeyen obje uyarıyla yok edilir.
        /// </summary>
        void Release(GameObject instance);

        /// <summary>
        /// Havuzda en az <paramref name="count"/> kapalı örnek hazır olacak şekilde eksikleri önceden oluşturur.
        /// Yükleme sırasında çağrılır; böylece oynanışta Instantiate yapılmaz.
        /// </summary>
        void Prewarm(GameObject prefab, int count);

        /// <summary>Prefab'ın havuz sayıları. Havuzu yoksa hepsi 0.</summary>
        PoolStats GetStats(GameObject prefab);
    }
}
