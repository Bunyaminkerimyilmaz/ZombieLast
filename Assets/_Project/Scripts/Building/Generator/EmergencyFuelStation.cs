using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Acil yakıt (GDD §12): gece jeneratörün kenarına <see cref="GeneratorData.EmergencyFuelRange"/> (2 m) yakınken E'yi
    /// <see cref="GeneratorData.EmergencyFuelHoldDuration"/> (1 sn) basılı tut → 30 para karşılığı 10 yakıt
    /// (<see cref="EmergencyFuelRule"/>). Jeneratörün kökünde <see cref="IInteractable"/>; sadece gece, jeneratör ayaktayken ve
    /// depoda yer varken sunulur. Her basışta bir alım: alımdan sonra tutma durur, tekrar almak için tuş bırakılıp yeniden basılır.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GeneratorPower))]
    public sealed class EmergencyFuelStation : MonoBehaviour, IInteractable
    {
        private const string LogPrefix = "[Base] ";

        private Generator _generator;
        private GeneratorPower _power;
        private Collider _collider;
        private IWallet _wallet;
        private IInteractableRegistry _registry;
        private string _prompt;
        private bool _started;

        public Vector3 PromptPosition
        {
            get
            {
                Bounds bounds = Bounds;
                return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            }
        }

        /// <summary>"Acil yakıt +10 (30 para)"; fiyat ve miktar sabit olduğu için bir kez kurulur.</summary>
        public string Prompt => _prompt ??= Data != null ? Data.FormatEmergencyFuelPrompt() : string.Empty;

        public bool CanInteract =>
            Data != null
            && _wallet != null
            && isActiveAndEnabled
            && Power.IsConsuming
            && EmergencyFuelRule.HasRoom(_wallet.Fuel, _wallet.GetCapacity(WalletResource.Fuel), Data.EmergencyFuelAmount);

        public float HoldDuration => Data != null ? Data.EmergencyFuelHoldDuration : 0f;

        public float InteractionRange => Data != null ? Data.EmergencyFuelRange : 0f;

        private Generator Owner => _generator != null ? _generator : _generator = GetComponent<Generator>();

        private GeneratorPower Power => _power != null ? _power : _power = GetComponent<GeneratorPower>();

        private GeneratorData Data => Owner != null ? Owner.Data : null;

        private Bounds Bounds
        {
            get
            {
                if (_collider == null)
                {
                    _collider = GetComponent<Collider>();
                }

                return _collider != null ? _collider.bounds : new Bounds(transform.position, Vector3.zero);
            }
        }

        public float GetDistance(Vector3 point)
        {
            return PlanarDistance.ToBounds(point, Bounds);
        }

        public void OnHoldProgress(float progress)
        {
            // İlerleme halkası UI'da (10.3; InteractionPromptEvent).
        }

        /// <summary>Alımı dener; her durumda false döner: bir basışta en fazla bir alım (tutma durur).</summary>
        public bool OnInteract()
        {
            if (Data == null || _wallet == null)
            {
                return false;
            }

            int price = Data.EmergencyFuelPrice;
            int amount = Data.EmergencyFuelAmount;
            switch (EmergencyFuelRule.TryBuy(_wallet, price, amount))
            {
                case EmergencyFuelResult.Purchased:
                    Debug.Log($"{LogPrefix}Acil yakıt: +{amount} yakıt, -{price} para (yakıt {_wallet.Fuel}, para {_wallet.Money}).", this);
                    break;
                case EmergencyFuelResult.NotEnoughMoney:
                    Debug.Log($"{LogPrefix}Acil yakıt alınamadı: para yetmiyor ({_wallet.Money}/{price}).", this);
                    break;
                case EmergencyFuelResult.TankFull:
                    Debug.Log($"{LogPrefix}Acil yakıt alınamadı: depoda yer yok ({_wallet.Fuel}/{_wallet.GetCapacity(WalletResource.Fuel)}).", this);
                    break;
            }

            return false;
        }

        private void Start()
        {
            ServiceLocator.TryGet(out _wallet);
            if (!ServiceLocator.TryGet(out _registry))
            {
                Debug.LogWarning("EmergencyFuelStation: IInteractableRegistry bulunamadı; acil yakıt alınamayacak. Sahnede [Systems]/Interaction var mı?", this);
            }

            // Servisler Awake'te kaydolur; sahnedeki jeneratörün OnEnable'ı onlardan önce çalışabilir. İlk kaydı Start yapar.
            _started = true;
            _registry?.Register(this);
        }

        private void OnEnable()
        {
            if (_started)
            {
                _registry?.Register(this);
            }
        }

        private void OnDisable()
        {
            _registry?.Unregister(this);
        }
    }
}
