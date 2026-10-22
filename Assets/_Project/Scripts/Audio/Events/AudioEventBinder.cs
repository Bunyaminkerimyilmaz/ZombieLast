using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Audio
{
    /// <summary>
    /// Oyun olaylarını seslere bağlar ([Systems]/Audio): olay gelince <see cref="AudioEventMap"/>'teki sesi <see cref="IAudioService"/>
    /// ile çalar. Konumu olan olaylar <see cref="IAudioService.PlaySfx"/> (ses 3D ise oradan duyulur), konumsuzlar
    /// <see cref="IAudioService.PlayUi"/>. Boş eşleme veya klipsiz ses sessizce atlanır. Arayüz tıklaması <see cref="UiClickSounds"/>'ta,
    /// müzik <see cref="MusicController"/>'da.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AudioEventBinder : MonoBehaviour
    {
        [SerializeField, Tooltip("Olay → ses eşlemesi.")]
        private AudioEventMap _map;

        private IAudioService _audio;
        private bool _audioMissing;

        private void Awake()
        {
            if (_map == null)
            {
                Debug.LogError("AudioEventBinder: AudioEventMap atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            EventBus<WeaponFiredEvent>.Subscribe(OnWeaponFired);
            EventBus<ShotHitEvent>.Subscribe(OnShotHit);
            EventBus<EnemyKilledEvent>.Subscribe(OnEnemyKilled);
            EventBus<BossSpawnedEvent>.Subscribe(OnBossSpawned);
            EventBus<CoinCollectedEvent>.Subscribe(OnCoinCollected);
            EventBus<PurchaseCompletedEvent>.Subscribe(OnPurchaseCompleted);
            EventBus<StructurePlacedEvent>.Subscribe(OnStructurePlaced);
            EventBus<StructureDestroyedEvent>.Subscribe(OnStructureDestroyed);
            EventBus<PowerLostEvent>.Subscribe(OnPowerLost);
            EventBus<DayEndingSoonEvent>.Subscribe(OnDayEndingSoon);
            EventBus<LevelUpEvent>.Subscribe(OnLevelUp);
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
        }

        private void OnDisable()
        {
            EventBus<WeaponFiredEvent>.Unsubscribe(OnWeaponFired);
            EventBus<ShotHitEvent>.Unsubscribe(OnShotHit);
            EventBus<EnemyKilledEvent>.Unsubscribe(OnEnemyKilled);
            EventBus<BossSpawnedEvent>.Unsubscribe(OnBossSpawned);
            EventBus<CoinCollectedEvent>.Unsubscribe(OnCoinCollected);
            EventBus<PurchaseCompletedEvent>.Unsubscribe(OnPurchaseCompleted);
            EventBus<StructurePlacedEvent>.Unsubscribe(OnStructurePlaced);
            EventBus<StructureDestroyedEvent>.Unsubscribe(OnStructureDestroyed);
            EventBus<PowerLostEvent>.Unsubscribe(OnPowerLost);
            EventBus<DayEndingSoonEvent>.Unsubscribe(OnDayEndingSoon);
            EventBus<LevelUpEvent>.Unsubscribe(OnLevelUp);
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
        }

        private void OnWeaponFired(WeaponFiredEvent fired)
        {
            PlayAt(_map.GetWeaponFired(fired.WeaponId), fired.Origin);
        }

        private void OnShotHit(ShotHitEvent hit)
        {
            PlayAt(_map.GetShotHit(hit.TargetType), hit.Point);
        }

        private void OnEnemyKilled(EnemyKilledEvent killed)
        {
            PlayAt(_map.EnemyKilled, killed.Position);
        }

        private void OnBossSpawned(BossSpawnedEvent spawned)
        {
            if (spawned.Boss != null)
            {
                PlayAt(_map.BossSpawned, spawned.Boss.transform.position);
            }
            else
            {
                PlayFlat(_map.BossSpawned);
            }
        }

        private void OnCoinCollected(CoinCollectedEvent collected)
        {
            PlayAt(_map.CoinCollected, collected.Position);
        }

        private void OnPurchaseCompleted(PurchaseCompletedEvent purchased)
        {
            PlayFlat(_map.PurchaseCompleted);
        }

        private void OnStructurePlaced(StructurePlacedEvent placed)
        {
            PlayAt(_map.StructurePlaced, placed.Position);
        }

        private void OnStructureDestroyed(StructureDestroyedEvent destroyed)
        {
            PlayAt(_map.StructureDestroyed, destroyed.Position);
        }

        private void OnPowerLost(PowerLostEvent lost)
        {
            PlayAt(_map.PowerLost, lost.Position);
        }

        private void OnDayEndingSoon(DayEndingSoonEvent ending)
        {
            PlayFlat(_map.DayEndingSoon);
        }

        private void OnLevelUp(LevelUpEvent levelUp)
        {
            PlayFlat(_map.LevelUp);
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            PlayFlat(_map.GetPhaseChanged(changed.Phase));
        }

        private void PlayAt(SoundData sound, Vector3 position)
        {
            if (sound != null && TryGetAudio())
            {
                _audio.PlaySfx(sound, position);
            }
        }

        private void PlayFlat(SoundData sound)
        {
            if (sound != null && TryGetAudio())
            {
                _audio.PlayUi(sound);
            }
        }

        private bool TryGetAudio()
        {
            if (_audio != null)
            {
                return true;
            }

            if (_audioMissing || !enabled)
            {
                return false;
            }

            if (!ServiceLocator.TryGet(out _audio))
            {
                _audioMissing = true;
                Debug.LogWarning("AudioEventBinder: IAudioService bulunamadı; olay sesleri çalmayacak.", this);
                return false;
            }

            return true;
        }
    }
}
