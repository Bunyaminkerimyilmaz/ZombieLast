using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Taretin hedef takibi (<see cref="Turret"/>'in parçası): <see cref="TurretConfig.ScanInterval"/>'da bir menzildeki en yakın
    /// canlı düşmanı seçer (<see cref="TurretTargetScanner"/>), aradaki karelerde hedefin canlılığını denetler ve kafayı ona çevirir
    /// (<see cref="TurretAim"/>). Hedef ölür veya havuza dönerse bir sonraki taramaya kadar hedef yoktur.
    /// </summary>
    public sealed class TurretTracker
    {
        private readonly TurretConfig _config;
        private readonly TurretAim _aim = new TurretAim();
        private readonly TurretTargetScanner _scanner;
        private TurretTarget _target;
        private float _scanTimer;

        public TurretTracker(TurretConfig config)
        {
            _config = config;
            _scanner = new TurretTargetScanner(config.ScanBufferSize, config.TargetLayers);
        }

        /// <summary>Kafanın dünya yaw'ı (derece).</summary>
        public float Yaw => _aim.Yaw;

        /// <summary>Seçili hedef (geçersiz olabilir: <see cref="TurretTarget.IsValid"/>).</summary>
        public TurretTarget Target => _target;

        /// <summary>Yönü anında ayarlar, hedefi bırakır; ilk <see cref="Tick"/> hemen tarar.</summary>
        public void Reset(float yaw)
        {
            _aim.Reset(yaw);
            _target = default;
            _scanTimer = 0f;
        }

        public void ClearTarget()
        {
            _target = default;
        }

        /// <summary>
        /// Gerekirse tarar ve kafayı hedefe çevirir. Hedef varsa ve kafa hizalandıysa true döner; <paramref name="direction"/> =
        /// eksenden hedefe yatay yön.
        /// </summary>
        /// <param name="axis">Taretin ekseni, namlu yüksekliğinde (tarama merkezi).</param>
        public bool Tick(Vector3 axis, float range, float deltaTime, out Vector3 direction)
        {
            direction = Vector3.zero;
            _scanTimer -= deltaTime;
            if (_scanTimer <= 0f)
            {
                _scanTimer = Mathf.Max(_scanTimer + _config.ScanInterval, 0f);
                _scanner.TryFindNearest(axis, range, out _target);
            }

            if (!_target.IsValid)
            {
                _target = default;
                return false;
            }

            float targetYaw = TurretAim.YawTowards(axis, _target.Transform.position);
            _aim.Tick(targetYaw, _config.TurnSpeed, deltaTime);
            direction = TurretAim.Direction(targetYaw);
            return _aim.IsAlignedWith(targetYaw, _config.AimTolerance);
        }
    }
}
