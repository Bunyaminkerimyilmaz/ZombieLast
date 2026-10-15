using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Üs alanı (GDD §11): merkez bu objenin konumu, yarıçap <see cref="BaseAreaConfig"/>'ten.
    /// Awake'te <see cref="IBaseArea"/> servisi olarak kaydolur. Jeneratör bu objenin çocuğu olarak merkezde durur.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BaseArea : MonoBehaviour, IBaseArea
    {
        [SerializeField, Tooltip("Üs alanı ayarları (yarıçap).")]
        private BaseAreaConfig _config;

        public Vector3 Center => transform.position;

        public float Radius => _config != null ? _config.Radius : 0f;

        public bool Contains(Vector3 position)
        {
            return BaseAreaMath.Contains(Center, Radius, position);
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("BaseArea: BaseAreaConfig atanmamış; yarıçap 0 sayılacak. Tools/Last Haven/Build Game Scene çalıştır.", this);
            }

            ServiceLocator.Register<IBaseArea>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IBaseArea>(this);
        }
    }
}
