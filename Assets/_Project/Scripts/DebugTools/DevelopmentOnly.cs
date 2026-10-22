using UnityEngine;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Geliştirme objesi işareti (15.3): editörde ve development build'de hiçbir şey yapmaz; sürüm build'inde objeyi (ve altını) Awake'te
    /// kapatır. Test kuklaları gibi oyuncuya gitmemesi gereken sahne objelerinin köküne sahne adımı ekler. Sınıf her build'de var
    /// (sahnede "missing script" olmasın diye).
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10000)]
    public sealed class DevelopmentOnly : MonoBehaviour
    {
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
        private void Awake()
        {
            gameObject.SetActive(false);
        }
#endif
    }
}
