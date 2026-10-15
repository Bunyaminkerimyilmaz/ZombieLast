using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>
    /// Oyuncunun yeniden doğması (GDD §5): <see cref="PlayerDiedEvent"/>'ten <see cref="IGameRules.PlayerRespawnDelay"/> sn sonra
    /// üs merkezinin (<see cref="IBaseArea.Center"/> + <see cref="IGameRules.PlayerRespawnOffset"/>) yanında tam canla doğar:
    /// ışınlanır, can dolar, dokunulmazlık penceresi ve poz sıfırlanır, <see cref="PlayerRespawnedEvent"/> gönderilir.
    /// Kurallar <see cref="IGameRules"/>'tan gelir (GameFlow kaydeder); yoksa oyuncu yeniden doğmaz.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerCharacter))]
    public sealed class PlayerRespawn : MonoBehaviour
    {
        private const string LogPrefix = "[Player] ";

        private readonly Countdown _countdown = new Countdown();
        private PlayerCharacter _character;
        private Health _health;
        private CharacterController _controller;
        private AnimationDriver _driver;
        private IGameRules _rules;
        private IBaseArea _baseArea;

        /// <summary>Ölü ve yeniden doğmayı bekliyor.</summary>
        public bool IsWaiting => _countdown.IsRunning;

        /// <summary>Yeniden doğmaya kalan süre (sn); beklemiyorsa 0.</summary>
        public float TimeUntilRespawn => _countdown.Remaining;

        private void Awake()
        {
            _character = GetComponent<PlayerCharacter>();
            _health = GetComponent<Health>();
            _controller = GetComponent<CharacterController>();
            _driver = GetComponent<AnimationDriver>();
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _rules))
            {
                Debug.LogWarning("PlayerRespawn: IGameRules bulunamadı; oyuncu ölünce yeniden doğmayacak. Sahnede [Systems]/GameFlow var mı?", this);
            }

            if (!ServiceLocator.TryGet(out _baseArea))
            {
                Debug.LogWarning("PlayerRespawn: IBaseArea bulunamadı; oyuncu öldüğü yerde yeniden doğacak. Sahnede [Gameplay]/Base var mı?", this);
            }
        }

        private void OnEnable()
        {
            EventBus<PlayerDiedEvent>.Subscribe(OnPlayerDied);
        }

        private void OnDisable()
        {
            EventBus<PlayerDiedEvent>.Unsubscribe(OnPlayerDied);
        }

        private void Update()
        {
            if (_countdown.Tick(Time.deltaTime))
            {
                Respawn();
            }
        }

        private void OnPlayerDied(PlayerDiedEvent died)
        {
            if (died.Player != gameObject || _rules == null)
            {
                return;
            }

            _countdown.Start(_rules.PlayerRespawnDelay);
            Debug.Log($"{LogPrefix}Oyuncu öldü; {_rules.PlayerRespawnDelay:0.#} sn sonra üste yeniden doğacak.", this);
        }

        private void Respawn()
        {
            Vector3 position = _baseArea != null ? _baseArea.Center + _rules.PlayerRespawnOffset : transform.position;

            // CharacterController açıkken transform'a yazılan konum ezilir.
            bool controllerEnabled = _controller != null && _controller.enabled;
            if (controllerEnabled)
            {
                _controller.enabled = false;
            }

            transform.position = position;
            if (controllerEnabled)
            {
                _controller.enabled = true;
            }

            _character.Revive();
            if (_driver != null)
            {
                _driver.ResetPose();
            }

            Debug.Log($"{LogPrefix}PlayerRespawnedEvent: oyuncu {position} noktasında {_health.Current:0}/{_health.Max:0} canla doğdu.", this);
            EventBus<PlayerRespawnedEvent>.Raise(new PlayerRespawnedEvent(gameObject, position, _health.Current, _health.Max));
        }
    }
}
