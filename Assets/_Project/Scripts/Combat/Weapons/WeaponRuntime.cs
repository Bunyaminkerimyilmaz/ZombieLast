using System;
using LastHaven.Core;

namespace LastHaven.Combat
{
    /// <summary>
    /// Bir oyuncu silahının çalışma anı durumu (saf C#): şarjör, yedek mermi, atış zamanlayıcısı, doldurma ve yükseltme seviyeleri.
    /// Etkili değer = <see cref="WeaponData"/> × yükseltme çarpanı, sonra <see cref="IRunModifiers"/> (Player* statları).
    /// </summary>
    /// <remarks>
    /// Sadece <see cref="Tick"/> ile ilerler: elde olmayan silah Tick edilmezse atış bekleme süresi de durur.
    /// Şarjör boşken bir sonraki Tick doldurmayı kendiliğinden başlatır (yedek varsa).
    /// </remarks>
    public sealed class WeaponRuntime
    {
        // WeaponData'daki alt sınırla aynı: perk/gece etkileri atış hızını 0'a indirse de silah kilitlenmesin.
        private const float MinFireRate = 0.01f;

        private static readonly int UpgradeTypeCount = Enum.GetValues(typeof(WeaponUpgradeType)).Length;

        private readonly WeaponUpgradeConfig _upgrades;
        private readonly IRunModifiers _modifiers;
        private readonly int[] _upgradeLevels = new int[UpgradeTypeCount];
        private readonly Countdown _reload = new Countdown();
        private float _fireCooldown;

        /// <param name="upgrades">Null ise yükseltme yapılamaz.</param>
        /// <param name="modifiers">Null ise run modifier'ları uygulanmaz.</param>
        public WeaponRuntime(WeaponData data, WeaponUpgradeConfig upgrades = null, IRunModifiers modifiers = null)
        {
            Data = data != null ? data : throw new ArgumentNullException(nameof(data));
            _upgrades = upgrades;
            _modifiers = modifiers;
            Ammo = MagSize;
            Reserve = data.HasInfiniteReserve ? 0 : data.ReserveAmmo;
        }

        /// <summary>Şarjördeki/yedek mermi veya şarjör kapasitesi değişti.</summary>
        public event Action AmmoChanged;

        /// <summary>Doldurma başladı; parametre süre (sn).</summary>
        public event Action<float> ReloadStarted;

        /// <summary>Doldurma bitti (true) veya iptal edildi (false).</summary>
        public event Action<bool> ReloadFinished;

        public WeaponData Data { get; }

        public string Id => Data.Id;

        /// <summary>Şarjördeki mermi.</summary>
        public int Ammo { get; private set; }

        /// <summary>Yedek mermi. <see cref="HasInfiniteReserve"/> ise her zaman 0 ve kullanılmaz.</summary>
        public int Reserve { get; private set; }

        public bool HasInfiniteReserve => Data.HasInfiniteReserve;

        public bool IsReloading => _reload.IsRunning;

        /// <summary>Doldurma ilerlemesi 0..1; doldurmuyorsa 0.</summary>
        public float ReloadProgress => _reload.IsRunning ? _reload.Progress : 0f;

        /// <summary>Şu an ateşlenebilir mi (doldurmuyor, mermi var, bekleme bitti).</summary>
        public bool CanFire => !IsReloading && Ammo > 0 && _fireCooldown <= 0f;

        public float Damage => ApplyStat(StatType.PlayerDamage, Data.Damage * GetUpgradeMultiplier(WeaponUpgradeType.Damage));

        public float FireRate =>
            Math.Max(MinFireRate, ApplyStat(StatType.PlayerFireRate, Data.FireRate * GetUpgradeMultiplier(WeaponUpgradeType.FireRate)));

        /// <summary>İki atış arası süre (sn).</summary>
        public float FireInterval => 1f / FireRate;

        /// <summary>Etkili şarjör kapasitesi; en yakın tam sayıya yuvarlanır (x.5 yukarı), en az 1.</summary>
        public int MagSize
        {
            get
            {
                float size = ApplyStat(StatType.PlayerMagSize, Data.MagSize * GetUpgradeMultiplier(WeaponUpgradeType.MagSize));
                return Math.Max(1, (int)Math.Round(size, MidpointRounding.AwayFromZero));
            }
        }

        public int PelletCount => Data.PelletCount;

        public float SpreadAngle => Data.SpreadAngle;

        public float Range => Data.Range;

        public float ReloadTime => Data.ReloadTime;

        /// <summary>Zamanlayıcıları ilerletir: atış bekleme süresi, doldurma; şarjör boşsa doldurmayı başlatır.</summary>
        public void Tick(float deltaTime)
        {
            // Bekleme süresi 0'ın altına en fazla bir kare iner; artan kısım TryFire'da sonraki atışa devreder.
            if (_fireCooldown > 0f)
            {
                _fireCooldown -= deltaTime;
            }

            if (_reload.Tick(deltaTime))
            {
                FinishReload();
            }

            if (!IsReloading && Ammo <= 0)
            {
                TryStartReload();
            }
        }

        /// <summary>Ateşlenebiliyorsa bir mermi harcar (saçma sayısından bağımsız) ve true döner.</summary>
        public bool TryFire()
        {
            if (!CanFire)
            {
                return false;
            }

            Ammo--;

            // Devreden süre atış hızını kare süresinden bağımsız tutar; takılmadan sonra seri atış olmasın diye en fazla bir aralık.
            float interval = FireInterval;
            _fireCooldown = Math.Max(_fireCooldown, -interval) + interval;
            AmmoChanged?.Invoke();
            return true;
        }

        /// <summary>Doldurmayı başlatır. Zaten dolduruyorsa, şarjör doluysa veya yedek bittiyse false.</summary>
        public bool TryStartReload()
        {
            if (IsReloading || Ammo >= MagSize || (!HasInfiniteReserve && Reserve <= 0))
            {
                return false;
            }

            _reload.Start(ReloadTime);
            ReloadStarted?.Invoke(ReloadTime);
            return true;
        }

        /// <summary>Süren doldurmayı iptal eder (ör. silah değişti); mermi değişmez.</summary>
        public void CancelReload()
        {
            if (!IsReloading)
            {
                return;
            }

            _reload.Stop();
            ReloadFinished?.Invoke(false);
        }

        public int GetUpgradeLevel(WeaponUpgradeType type)
        {
            return _upgradeLevels[(int)type];
        }

        /// <summary>Bu yükseltmenin en yüksek seviyesi; yükseltme verisi yoksa 0.</summary>
        public int GetMaxUpgradeLevel(WeaponUpgradeType type)
        {
            return _upgrades != null ? _upgrades.GetMaxLevel(type) : 0;
        }

        /// <summary>Yükseltmeyi bir seviye artırır; en yüksek seviyedeyse false. Ödeme çağıranın işi.</summary>
        public bool TryUpgrade(WeaponUpgradeType type)
        {
            if (GetUpgradeLevel(type) >= GetMaxUpgradeLevel(type))
            {
                return false;
            }

            _upgradeLevels[(int)type]++;
            if (type == WeaponUpgradeType.MagSize)
            {
                AmmoChanged?.Invoke();
            }

            return true;
        }

        private void FinishReload()
        {
            int needed = Math.Max(0, MagSize - Ammo);
            int loaded = HasInfiniteReserve ? needed : Math.Min(needed, Reserve);
            Ammo += loaded;
            if (!HasInfiniteReserve)
            {
                Reserve -= loaded;
            }

            AmmoChanged?.Invoke();
            ReloadFinished?.Invoke(true);
        }

        private float GetUpgradeMultiplier(WeaponUpgradeType type)
        {
            return _upgrades != null ? _upgrades.GetMultiplier(type, GetUpgradeLevel(type)) : 1f;
        }

        private float ApplyStat(StatType stat, float value)
        {
            return _modifiers != null ? _modifiers.Get(stat, value) : value;
        }
    }
}
