using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// İnşa yerleşim noktası. Fare: imleç ışınının ızgara düzlemiyle (yer seviyesi) kesişimi. Gamepad: oyuncunun baktığı
    /// yönde sabit mesafe (<see cref="BuildConfig.GamepadPlacementDistance"/>).
    /// </summary>
    public sealed class BuildPointer
    {
        private readonly IInputReader _input;
        private readonly ICameraService _camera;
        private readonly IPlayer _player;
        private readonly float _gamepadDistance;

        /// <param name="camera">Yoksa fareyle nokta yok.</param>
        /// <param name="player">Yoksa gamepad'le nokta yok.</param>
        public BuildPointer(IInputReader input, ICameraService camera, IPlayer player, float gamepadDistance)
        {
            _input = input;
            _camera = camera;
            _player = player;
            _gamepadDistance = gamepadDistance;
        }

        /// <summary>Işının <paramref name="height"/> yüksekliğindeki yatay düzlemle kesişimi; ışın düzleme gitmiyorsa false.</summary>
        public static bool TryIntersectGround(Ray ray, float height, out Vector3 point)
        {
            var plane = new Plane(Vector3.up, new Vector3(0f, height, 0f));
            if (plane.Raycast(ray, out float distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }

            point = default;
            return false;
        }

        /// <summary>Son kullanılan cihaza göre yerleşim noktası, <paramref name="groundHeight"/> yüksekliğinde.</summary>
        public bool TryGetPoint(float groundHeight, out Vector3 point)
        {
            if (_input.IsUsingGamepad)
            {
                if (_player == null)
                {
                    point = default;
                    return false;
                }

                point = _player.Position + _player.AimDirection * _gamepadDistance;
                point.y = groundHeight;
                return true;
            }

            if (!TryGetScreenRay(out Ray ray))
            {
                point = default;
                return false;
            }

            return TryIntersectGround(ray, groundHeight, out point);
        }

        /// <summary>Fare kullanılıyorsa kameradan imlecin içinden geçen ışın; gamepad'de (imleç yok) veya kamera yoksa false.</summary>
        public bool TryGetScreenRay(out Ray ray)
        {
            Camera camera = _camera != null ? _camera.Camera : null;
            if (_input.IsUsingGamepad || camera == null)
            {
                ray = default;
                return false;
            }

            ray = camera.ScreenPointToRay(_input.AimScreenPosition);
            return true;
        }
    }
}
