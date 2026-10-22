using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// [Systems]/EnemySpawner: Zincirleme Son (GDD §13). <see cref="PerkFlag.ChainExplosion"/> açıkken oyuncunun öldürdüğü düşman
    /// (<see cref="EnemyKilledEvent.KilledByPlayer"/>) <see cref="EnemyPerkConfig.ChainExplosionChance"/> olasılıkla ölüm noktasında
    /// patlar (<see cref="AreaDamage"/>; sadece düşmanlar, 3 m, 40 hasar). Patlamanın kaynağı oyuncudur: öldürdükleri yine oyuncunun
    /// sayılır (kombo, Vampir) ve onlar da patlayabilir (kalabalıkta zincir). Patlamalar bir sonraki karede işlenir: zincir kare kare
    /// ilerler, olay içinde iç içe hasar/olay zinciri kurulmaz.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ChainExplosionPerk : MonoBehaviour
    {
        private const string LogPrefix = "[Enemies] ";

        [SerializeField, Tooltip("Olasılık, yarıçap, hasar, efekt. Üretici bağlar.")]
        private EnemyPerkConfig _config;

        private List<Vector3> _pending = new List<Vector3>();
        private List<Vector3> _processing = new List<Vector3>();
        private Collider[] _buffer;
        private IRunModifiers _modifiers;
        private IPlayer _player;
        private IPoolService _pool;

        /// <summary>Sıradaki karede patlayacak nokta sayısı (debug, testler).</summary>
        public int PendingCount => _pending.Count;

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("ChainExplosionPerk: EnemyPerkConfig atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            _buffer = new Collider[_config.ChainExplosionBufferSize];
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _modifiers))
            {
                Debug.LogWarning("ChainExplosionPerk: IRunModifiers bulunamadı; Zincirleme Son çalışmayacak.", this);
            }

            ServiceLocator.TryGet(out _player);
            if (ServiceLocator.TryGet(out _pool) && _config.ExplosionVfx != null && _config.VfxPrewarm > 0)
            {
                _pool.Prewarm(_config.ExplosionVfx, _config.VfxPrewarm);
            }
        }

        private void OnEnable()
        {
            EventBus<EnemyKilledEvent>.Subscribe(OnEnemyKilled);
        }

        private void OnDisable()
        {
            EventBus<EnemyKilledEvent>.Unsubscribe(OnEnemyKilled);
            _pending.Clear();
        }

        private void Update()
        {
            if (_pending.Count == 0)
            {
                return;
            }

            // Bu karede doğan patlamalar (zincir) sonraki kareye kalır.
            (_pending, _processing) = (_processing, _pending);
            for (int i = 0; i < _processing.Count; i++)
            {
                Explode(_processing[i]);
            }

            _processing.Clear();
        }

        private void OnEnemyKilled(EnemyKilledEvent killed)
        {
            bool active = _modifiers != null && _modifiers.HasFlag(PerkFlag.ChainExplosion);
            if (ChainExplosionRule.ShouldExplode(active, killed.KilledByPlayer, UnityEngine.Random.value, _config.ChainExplosionChance))
            {
                _pending.Add(killed.Position);
            }
        }

        private void Explode(Vector3 groundPosition)
        {
            Vector3 center = groundPosition + Vector3.up * _config.ChainExplosionHeight;
            GameObject source = _player != null ? _player.Transform.gameObject : null;
            int damaged = AreaDamage.Apply(center, _config.CreateChainExplosionSpec(), source, _buffer);
            if (_pool != null && _config.ExplosionVfx != null)
            {
                _pool.Get(_config.ExplosionVfx, center, Quaternion.identity);
            }

            EventBus<ExplosionEvent>.Raise(new ExplosionEvent(center, _config.ChainExplosionRadius, source));
            Debug.Log(FormattableString.Invariant(
                $"{LogPrefix}Zincirleme Son: patlama ({groundPosition.x:0.#}, {groundPosition.z:0.#}), {damaged} düşman."), this);
        }
    }
}
