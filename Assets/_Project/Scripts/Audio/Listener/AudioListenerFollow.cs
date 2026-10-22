using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Audio
{
    /// <summary>
    /// Sahnedeki tek <see cref="AudioListener"/>'ı ([Systems]/Audio/Listener) oyuncunun üstünde tutar ve kameranın yatay açısına
    /// çevirir: kamera ~20 m uzakta olduğu için 3D sesler oyuncuya göre zayıflar, sağ/sol kanal ekranın sağ/soluyla eşleşir.
    /// Oyuncu yoksa (ör. menü sahnesi) yerinde kalır.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioListener))]
    public sealed class AudioListenerFollow : MonoBehaviour
    {
        [SerializeField, Tooltip("Dinleyici yüksekliği buradan.")]
        private AudioConfig _config;

        private IPlayer _player;
        private ICameraService _camera;

        private void LateUpdate()
        {
            if (_player == null && !ServiceLocator.TryGet(out _player))
            {
                return;
            }

            float height = _config != null ? _config.ListenerHeight : 0f;
            Transform self = transform;
            self.position = _player.Position + Vector3.up * height;
            if (_camera != null || ServiceLocator.TryGet(out _camera))
            {
                self.rotation = Quaternion.Euler(0f, _camera.Yaw, 0f);
            }
        }
    }
}
