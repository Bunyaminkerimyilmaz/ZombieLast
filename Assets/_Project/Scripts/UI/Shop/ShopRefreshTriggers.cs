using System;
using LastHaven.Core;

namespace LastHaven.UI
{
    /// <summary>
    /// Market kartlarının durumunu değiştirebilen olaylar (<see cref="IShopService.GetStatus"/>'in girdileri): para, yakıt deposu
    /// (yakıt), oyuncunun canı (can paketi) ve satın alma (seviye). Market paneli açıkken dinlenir; kapalıyken panel açılışta
    /// zaten yeniler. Evre olayı panelin kendisinde (kapanma).
    /// </summary>
    public sealed class ShopRefreshTriggers
    {
        private readonly Action _refresh;
        private readonly Action<int> _moneyChanged;
        private readonly Action<string> _purchased;

        /// <param name="refresh">Kart durumlarını yenile.</param>
        /// <param name="moneyChanged">Yeni para (etiket); ardından <paramref name="refresh"/> da çağrılır.</param>
        /// <param name="purchased">Satın alınan öğenin id'si (animasyon); ardından <paramref name="refresh"/> da çağrılır.</param>
        public ShopRefreshTriggers(Action refresh, Action<int> moneyChanged, Action<string> purchased)
        {
            _refresh = refresh;
            _moneyChanged = moneyChanged;
            _purchased = purchased;
        }

        public void Subscribe()
        {
            EventBus<PurchaseCompletedEvent>.Subscribe(OnPurchaseCompleted);
            EventBus<MoneyChangedEvent>.Subscribe(OnMoneyChanged);
            EventBus<FuelChangedEvent>.Subscribe(OnFuelChanged);
            EventBus<PlayerDamagedEvent>.Subscribe(OnPlayerDamaged);
            EventBus<PlayerRespawnedEvent>.Subscribe(OnPlayerRespawned);
        }

        public void Unsubscribe()
        {
            EventBus<PurchaseCompletedEvent>.Unsubscribe(OnPurchaseCompleted);
            EventBus<MoneyChangedEvent>.Unsubscribe(OnMoneyChanged);
            EventBus<FuelChangedEvent>.Unsubscribe(OnFuelChanged);
            EventBus<PlayerDamagedEvent>.Unsubscribe(OnPlayerDamaged);
            EventBus<PlayerRespawnedEvent>.Unsubscribe(OnPlayerRespawned);
        }

        private void OnPurchaseCompleted(PurchaseCompletedEvent purchase)
        {
            _purchased(purchase.ItemId);
            _refresh();
        }

        private void OnMoneyChanged(MoneyChangedEvent changed)
        {
            _moneyChanged(changed.Money);
            _refresh();
        }

        private void OnFuelChanged(FuelChangedEvent changed) => _refresh();

        private void OnPlayerDamaged(PlayerDamagedEvent damaged) => _refresh();

        private void OnPlayerRespawned(PlayerRespawnedEvent respawned) => _refresh();
    }
}
