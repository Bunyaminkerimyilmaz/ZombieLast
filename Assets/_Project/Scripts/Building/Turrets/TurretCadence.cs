using System;

namespace LastHaven.Building
{
    /// <summary>
    /// Taretin atış zamanlayıcısı (saf). Silahlarla aynı kural (3.2): bekleme süresi sadece pozitifken azalır, 0'ın altına inen
    /// kısım (en fazla bir aralık) sonraki atışa devreder; atış hızı FPS'ten bağımsız kalır, takılmadan sonra seri atış olmaz.
    /// Karede en fazla bir atış.
    /// </summary>
    public sealed class TurretCadence
    {
        private float _cooldown;

        /// <summary>Şimdi ateş edilebilir mi.</summary>
        public bool IsReady => _cooldown <= 0f;

        public void Tick(float deltaTime)
        {
            if (_cooldown > 0f && deltaTime > 0f)
            {
                _cooldown -= deltaTime;
            }
        }

        /// <summary>Hazırsa atışı sayar, sonraki atışın bekleme süresini başlatır ve true döner.</summary>
        /// <param name="interval">İki atış arası süre (sn); negatifse 0.</param>
        public bool TryFire(float interval)
        {
            if (!IsReady)
            {
                return false;
            }

            interval = Math.Max(0f, interval);
            _cooldown = Math.Max(_cooldown, -interval) + interval;
            return true;
        }

        /// <summary>Hemen ateş edilebilir duruma döner (yerleşme, havuzdan çıkış).</summary>
        public void Reset()
        {
            _cooldown = 0f;
        }
    }
}
