using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Bir düşman tipinin değerleri (GDD §7). Asset'leri üretici oluşturur: <c>Data/Enemies/</c>.
    /// Prefab'daki <see cref="Enemy"/> bu asset'e bağlıdır; her havuzdan çıkışta değerler buradan yeniden okunur.
    /// </summary>
    public sealed class EnemyData : ScriptableObject
    {
        [Header("Kimlik")]
        [SerializeField, Tooltip("Modüller arası referans için sabit id (ör. dalga verisi, istatistik). Değiştirme.")]
        private string _id = "enemy";

        [SerializeField, Tooltip("Arayüzde görünen ad (Türkçe).")]
        private string _displayName = "Düşman";

        [Header("Can ve hareket")]
        [SerializeField, Min(1f), Tooltip("Maksimum can.")]
        private float _maxHealth = 60f;

        [SerializeField, Min(0f), Tooltip("Yürüme hızı (m/sn). NavMeshAgent.speed'e yazılır.")]
        private float _moveSpeed = 2.2f;

        [Header("Saldırı")]
        [SerializeField, Min(0f), Tooltip("Vuruş başına hasar.")]
        private float _damage = 10f;

        [SerializeField, Min(0.01f), Tooltip("Saniyedeki saldırı sayısı.")]
        private float _attackRate = 1f;

        [SerializeField, Min(0f), Tooltip("Saldırı menzili (m): düşmanın merkezinden hedefin yüzeyine yatay uzaklık.")]
        private float _attackRange = 1.5f;

        [SerializeField, Min(0f), Tooltip("Yapılara (jeneratör, barikat, taret, tuzak) verilen hasarın çarpanı. Tank ve Patlayan 2.")]
        private float _structureDamageMultiplier = 1f;

        [SerializeReference, Tooltip("Saldırı stratejisi: yakın dövüş, menzilli mermi (Tükürücü) veya kendini patlatma (Patlayan). Boşsa yakın dövüş.")]
        private IEnemyAttack _attack = new MeleeAttack();

        [Header("Hedef seçimi")]
        [SerializeField, Min(0f), Tooltip("Oyuncu bu yarıçap içindeyse oyuncuya saldırır (m). Tank 3, diğerleri 6.")]
        private float _aggroRadius = 6f;

        [SerializeField, Tooltip("Açıksa tipin tercih ettiği hedef türü yakındaysa jeneratörden önce ona gider (Tank: barikat, Tükürücü: taret).")]
        private bool _hasPreferredTarget;

        [SerializeField, Tooltip("Tercih edilen hedef türü. Sadece yukarıdaki kutu açıksa kullanılır.")]
        private TargetType _preferredTarget = TargetType.Barricade;

        [SerializeField, Min(0f), Tooltip("Tercih edilen hedef bu yarıçap içindeyse seçilir (m).")]
        private float _preferredTargetRadius = 15f;

        [Header("Ödül")]
        [SerializeField, Min(0), Tooltip("Ölünce düşen taban para.")]
        private int _moneyReward = 10;

        [SerializeField, Min(0), Tooltip("Ölünce kazanılan XP.")]
        private int _xpReward = 1;

        [Header("Fizik ve görünüm")]
        [SerializeField, Range(0f, 1f), Tooltip("Geri itilme direnci: 0 = tam itilir, 1 = hiç itilmez (Tank, Brute).")]
        private float _knockbackResistance;

        [SerializeField, Min(0f), Tooltip("Vuruş başına geri itilme mesafesi (m), direnç uygulanmadan. GDD §7: 0.3.")]
        private float _knockbackDistance = 0.3f;

        [SerializeField, Min(0f), Tooltip("Geri itilmenin süresi (sn). Bu sürede ajan yürümez.")]
        private float _knockbackDuration = 0.1f;

        [SerializeField, Min(0.01f), Tooltip("Spawn'da kökün ölçeği bununla çarpılır (prefab'ın kendi ölçeğinin üstüne).")]
        private float _scale = 1f;

        [SerializeField, Tooltip("Bu tipin prefab'ı (kökünde Enemy). Spawner havuzdan bunu çeker.")]
        private Enemy _prefab;

        [Header("Zamanlama")]
        [SerializeField, Min(0f), Tooltip("Yerden çıkma süresi (sn). Bu sürede hareket etmez ve saldırmaz.")]
        private float _spawnDuration = 0.5f;

        [SerializeField, Min(0.05f), Tooltip("Hedef seçiminin yeniden değerlendirilme aralığı (sn). Başlangıç her düşmanda rastgele kaydırılır.")]
        private float _retargetInterval = 0.5f;

        [SerializeField, Min(0f), Tooltip("Ölümden sonra ceset bu kadar bekler, sonra yere batar (sn).")]
        private float _corpseDuration = 2f;

        [SerializeField, Min(0f), Tooltip("Yere batma süresi (sn); bitince havuza döner.")]
        private float _sinkDuration = 1f;

        [SerializeField, Min(0f), Tooltip("Yerden çıkarken ve batarken gömülme derinliği (m). Ölçekle çarpılır.")]
        private float _burrowDepth = 2.2f;

        [Header("Gezinme")]
        [SerializeField, Min(0f), Tooltip("Saldırırken hedef menzil + bu kadar uzaklaşınca kovalamaya döner (m). Sınırda titremeyi önler.")]
        private float _disengageMargin = 0.3f;

        [SerializeField, Min(0.05f), Tooltip("Hedef son yol hedefinden bu kadar uzaklaşınca yol yeniden hesaplanır (m).")]
        private float _repathDistance = 0.5f;

        [SerializeField, Range(AvoidancePriority.Min, AvoidancePriority.Max), Tooltip("Kaçınma önceliği alt sınırı (NavMeshAgent.avoidancePriority; düşük = diğerleri yol verir). Her doğuşta [alt, üst] aralığından rastgele: aynı öncelikli ajanlar barikat önünde birbirini kilitlemesin.")]
        private int _minAvoidancePriority = 30;

        [SerializeField, Range(AvoidancePriority.Min, AvoidancePriority.Max), Tooltip("Kaçınma önceliği üst sınırı (dahil).")]
        private int _maxAvoidancePriority = 70;

        [Header("Ses ve efekt (opsiyonel)")]
        [SerializeField, Tooltip("Opsiyonel saldırı sesi. Boşsa sessiz.")]
        private AudioClip _attackSound;

        [SerializeField, Tooltip("Opsiyonel ölüm sesi. Boşsa sessiz.")]
        private AudioClip _deathSound;

        [SerializeField, Tooltip("Opsiyonel ölüm efekti prefab'ı. Boşsa atlanır.")]
        private GameObject _deathVfx;

        public string Id => _id;

        public string DisplayName => _displayName;

        public float MaxHealth => _maxHealth;

        public float MoveSpeed => _moveSpeed;

        public float Damage => _damage;

        public float AttackRate => _attackRate;

        /// <summary>İki saldırı arası süre (sn).</summary>
        public float AttackInterval => 1f / _attackRate;

        public float AttackRange => _attackRange;

        public float StructureDamageMultiplier => _structureDamageMultiplier;

        /// <summary>Saldırı stratejisi; atanmamışsa yakın dövüş.</summary>
        public IEnemyAttack Attack => _attack ?? MeleeAttack.Default;

        public float AggroRadius => _aggroRadius;

        /// <summary>Tipin tercih ettiği hedef türü; yoksa null.</summary>
        public TargetType? PreferredTarget => _hasPreferredTarget ? _preferredTarget : (TargetType?)null;

        public float PreferredTargetRadius => _preferredTargetRadius;

        public int MoneyReward => _moneyReward;

        public int XpReward => _xpReward;

        public float KnockbackResistance => _knockbackResistance;

        public float KnockbackDistance => _knockbackDistance;

        public float KnockbackDuration => _knockbackDuration;

        public float Scale => _scale;

        /// <summary>Null olabilir (üretici bağlamadıysa).</summary>
        public Enemy Prefab => _prefab;

        public float SpawnDuration => _spawnDuration;

        public float RetargetInterval => _retargetInterval;

        public float CorpseDuration => _corpseDuration;

        public float SinkDuration => _sinkDuration;

        public float BurrowDepth => _burrowDepth;

        public float DisengageMargin => _disengageMargin;

        public float RepathDistance => _repathDistance;

        public int MinAvoidancePriority => _minAvoidancePriority;

        public int MaxAvoidancePriority => _maxAvoidancePriority;

        /// <summary>Doğuş için rastgele kaçınma önceliği (<see cref="AvoidancePriority.Pick"/>).</summary>
        public int PickAvoidancePriority()
        {
            return AvoidancePriority.Pick(_minAvoidancePriority, _maxAvoidancePriority, Random.value);
        }

        /// <summary>Null olabilir.</summary>
        public AudioClip AttackSound => _attackSound;

        /// <summary>Null olabilir.</summary>
        public AudioClip DeathSound => _deathSound;

        /// <summary>Null olabilir.</summary>
        public GameObject DeathVfx => _deathVfx;

        /// <summary>Hedef seçimi ayarları (GDD §7 kural 1–3).</summary>
        public TargetSelectorSettings CreateTargetSelectorSettings()
        {
            return new TargetSelectorSettings(_aggroRadius, PreferredTarget, _preferredTargetRadius, _retargetInterval);
        }

        /// <summary>Durum makinesinin süre ve menzil ayarları.</summary>
        public EnemyBehaviourSettings CreateBehaviourSettings()
        {
            return new EnemyBehaviourSettings(_spawnDuration, _attackRange, _disengageMargin, AttackInterval, _corpseDuration, _sinkDuration);
        }
    }
}
