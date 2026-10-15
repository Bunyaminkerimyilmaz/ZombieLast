using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Mantık kodunun görsele tek kapısı (CLAUDE.md "Görsel ayrımı"). <c>Visual</c> altında controller'lı bir Animator varsa
    /// <see cref="AnimParams"/> parametrelerini yazar (controller'da olmayan parametre atlanır); yoksa <see cref="ProceduralPose"/>
    /// ile yedek animasyon oynatır: saldırıda öne atılma, vuruşta sarsılma, ölümde rastgele yöne devrilme.
    /// Kökte durur; havuzdan her çıkışta pozu sıfırlar.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AnimationDriver : MonoBehaviour, IPoolable
    {
        [SerializeField, Tooltip("Görsel kök (mesh + Animator). Boşsa kökün 'Visual' adlı çocuğu kullanılır.")]
        private Transform _visual;

        [SerializeField, Tooltip("Animator yokken oynatılan prosedürel yedek animasyonun ayarları.")]
        private ProceduralPoseSettings _procedural = new ProceduralPoseSettings();

        private ProceduralPose _pose;
        private AnimatorParameters _animator;
        private Vector3 _restPosition;
        private Quaternion _restRotation = Quaternion.identity;
        private bool _initialized;

        /// <summary>Görsel kök; bulunamadıysa null.</summary>
        public Transform Visual
        {
            get
            {
                EnsureInitialized();
                return _visual;
            }
        }

        /// <summary>Controller'ı atanmış bir Animator var mı (yoksa prosedürel yedek oynar).</summary>
        public bool UsesAnimator
        {
            get
            {
                EnsureInitialized();
                return _animator.IsUsable;
            }
        }

        /// <summary>Prosedürel yedek animasyon (Animator varken boşta kalır). Testler ve hata ayıklama için.</summary>
        public ProceduralPose Pose
        {
            get
            {
                EnsureInitialized();
                return _pose;
            }
        }

        /// <summary>Yatay hız (m/sn). Sadece Animator'a yazılır.</summary>
        public void SetSpeed(float speed)
        {
            EnsureInitialized();
            _animator.SetFloat(AnimParams.SpeedId, speed);
        }

        /// <summary>Atılma durumu. Sadece Animator'a yazılır.</summary>
        public void SetDashing(bool dashing)
        {
            EnsureInitialized();
            _animator.SetBool(AnimParams.IsDashingId, dashing);
        }

        /// <summary>Yakın dövüş saldırısı; yedek: öne atılma.</summary>
        public void TriggerAttack()
        {
            if (!Trigger(AnimParams.AttackId))
            {
                _pose.PlayLunge();
            }
        }

        /// <summary>Hasar aldı (ölmeden); yedek: yana sarsılma.</summary>
        public void TriggerHit()
        {
            if (!Trigger(AnimParams.HitId))
            {
                _pose.PlayShake();
            }
        }

        /// <summary>Öldü; yedek: rastgele yöne devrilme (<see cref="ResetPose"/>'a kadar yatık kalır).</summary>
        public void TriggerDie()
        {
            if (!Trigger(AnimParams.DieId))
            {
                Vector3 direction = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * Vector3.forward;
                _pose.PlayTopple(direction);
            }
        }

        /// <summary>Ateş etti. Sadece Animator'a yazılır.</summary>
        public void TriggerFire()
        {
            Trigger(AnimParams.FireId);
        }

        /// <summary>Doldurmaya başladı. Sadece Animator'a yazılır.</summary>
        public void TriggerReload()
        {
            Trigger(AnimParams.ReloadId);
        }

        /// <summary>Görseli dinlenme pozuna döndürür ve Animator'ı baştan başlatır (havuzdan çıkış, yeniden doğma).</summary>
        public void ResetPose()
        {
            EnsureInitialized();
            _pose.Reset();
            ApplyPose();
            _animator.Rebind();
        }

        public void OnSpawned()
        {
            ResetPose();
        }

        public void OnDespawned()
        {
            EnsureInitialized();
            _pose.Reset();
            ApplyPose();
        }

        /// <summary>Yedek animasyonu ilerletip görsele uygular. LateUpdate'ten çağrılır; testler doğrudan çağırır.</summary>
        internal void Advance(float deltaTime)
        {
            EnsureInitialized();
            if (!_pose.IsActive)
            {
                return;
            }

            _pose.Tick(deltaTime);
            ApplyPose();
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void LateUpdate()
        {
            Advance(Time.deltaTime);
        }

        // Tembel kurulum: EditMode testlerinde ve başka bir bileşenin Awake'inden çağrılınca da çalışır.
        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            _pose = new ProceduralPose(_procedural);
            if (_visual == null)
            {
                _visual = VisualRoot.Find(transform);
            }

            if (_visual == null)
            {
                Debug.LogWarning($"AnimationDriver: '{name}' altında '{VisualRoot.Name}' bulunamadı; animasyon oynamayacak.", this);
                _animator = new AnimatorParameters(null);
                return;
            }

            _restPosition = _visual.localPosition;
            _restRotation = _visual.localRotation;
            _animator = new AnimatorParameters(_visual.GetComponentInChildren<Animator>(true));
        }

        /// <summary>Animator kullanılıyorsa tetikler (parametre yoksa atlar) ve true döner; değilse false (yedek oynatılsın).</summary>
        private bool Trigger(int id)
        {
            EnsureInitialized();
            if (!_animator.IsUsable)
            {
                return false;
            }

            _animator.SetTrigger(id);
            return true;
        }

        private void ApplyPose()
        {
            if (_visual == null)
            {
                return;
            }

            _visual.localPosition = _restPosition + _pose.Offset;
            _visual.localRotation = _pose.Rotation * _restRotation;
        }
    }
}
