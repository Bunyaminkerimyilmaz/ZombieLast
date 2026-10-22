using System.Collections.Generic;
using LastHaven.Audio;
using LastHaven.Core;

namespace LastHaven.Editor
{
    /// <summary>
    /// Üretilen ses verileri (<see cref="AudioGenerator"/>): her olay sesi ve müzik parçası için bir <see cref="SoundData"/> tanımı
    /// (boş klip listesi; çalma ayarları ilk oluşturmada yazılır) ve <see cref="AudioEventMap"/>'teki yeri. Değerler GDD'de yok;
    /// okunaklı kaos (GDD §2) için sık sesler kısa bekleme + az kopya, duyurular 2D ve tek kopya.
    /// </summary>
    public static class AudioDefinitions
    {
        public const string DataFolder = "Assets/_Project/Data/Audio";
        public const string SfxFolder = DataFolder + "/Sfx";
        public const string MusicFolder = DataFolder + "/Music";

        /// <summary>Taretin atış id'si (<c>TurretConfig._weaponId</c> varsayılanı).</summary>
        public const string TurretWeaponId = "turret";

        // Haritanın yarısından biraz fazlası (80 m harita, 40 m'deki spawn kenarı); azalma doğrusal (ses kaynağı prefab'ı).
        private const float NearDistance = 2f;
        private const float FarDistance = 45f;

        /// <summary>Haritadaki yeri: alan adı (tekil) veya liste anahtarı.</summary>
        public enum Slot
        {
            WeaponFired,
            WeaponFiredFallback,
            ShotHitFlesh,
            ShotHitSurface,
            EnemyKilled,
            BossSpawned,
            CoinCollected,
            PurchaseCompleted,
            StructurePlaced,
            StructureDestroyed,
            PowerLost,
            DayEndingSoon,
            LevelUp,
            PhaseChanged,
            UiClick,
            Music,
        }

        public static readonly IReadOnlyList<Definition> Sfx = new[]
        {
            Spatial("Sfx_WeaponFired_Pistol", Slot.WeaponFired, 0.8f, 0.95f, 1.05f, 4, 0.03f).ForWeapon(WeaponsGenerator.PistolId),
            Spatial("Sfx_WeaponFired_Shotgun", Slot.WeaponFired, 1f, 0.95f, 1.05f, 3, 0.05f).ForWeapon(WeaponsGenerator.ShotgunId),
            Spatial("Sfx_WeaponFired_Rifle", Slot.WeaponFired, 0.7f, 0.95f, 1.05f, 4, 0.03f).ForWeapon(WeaponsGenerator.RifleId),
            Spatial("Sfx_WeaponFired_Turret", Slot.WeaponFired, 0.6f, 0.95f, 1.05f, 6, 0.03f).ForWeapon(TurretWeaponId),
            Spatial("Sfx_WeaponFired_Default", Slot.WeaponFiredFallback, 0.7f, 0.95f, 1.05f, 4, 0.03f),
            Spatial("Sfx_ShotHit_Flesh", Slot.ShotHitFlesh, 0.6f, 0.9f, 1.1f, 6, 0.03f),
            Spatial("Sfx_ShotHit_Surface", Slot.ShotHitSurface, 0.5f, 0.9f, 1.1f, 4, 0.04f),
            Spatial("Sfx_EnemyKilled", Slot.EnemyKilled, 0.8f, 0.9f, 1.1f, 5, 0.05f),
            Flat("Sfx_BossSpawned", Slot.BossSpawned, 1f, 1, 1f),
            Flat("Sfx_CoinCollected", Slot.CoinCollected, 0.6f, 3, 0.04f, 0.95f, 1.15f),
            Flat("Sfx_PurchaseCompleted", Slot.PurchaseCompleted, 0.9f, 2, 0.05f),
            Spatial("Sfx_StructurePlaced", Slot.StructurePlaced, 0.9f, 0.95f, 1.05f, 2, 0.05f),
            Spatial("Sfx_StructureDestroyed", Slot.StructureDestroyed, 1f, 0.9f, 1.1f, 3, 0.1f),
            Flat("Sfx_PowerLost", Slot.PowerLost, 1f, 1, 1f),
            Flat("Sfx_DayEndingSoon", Slot.DayEndingSoon, 0.9f, 1, 1f),
            Flat("Sfx_LevelUp", Slot.LevelUp, 1f, 1, 0.1f),
            Flat("Sfx_Phase_Day", Slot.PhaseChanged, 0.9f, 1, 0.5f).ForPhase(GamePhase.Day),
            Flat("Sfx_Phase_Dusk", Slot.PhaseChanged, 0.9f, 1, 0.5f).ForPhase(GamePhase.Dusk),
            Flat("Sfx_Phase_Night", Slot.PhaseChanged, 1f, 1, 0.5f).ForPhase(GamePhase.Night),
            Flat("Sfx_Phase_Dawn", Slot.PhaseChanged, 0.9f, 1, 0.5f).ForPhase(GamePhase.Dawn),
            Flat("Ui_Click", Slot.UiClick, 0.7f, 2, 0.03f),
        };

        public static readonly IReadOnlyList<Definition> Music = new[]
        {
            Track("Music_Day", MusicCue.Day, 0.6f, loop: true),
            Track("Music_Night", MusicCue.Night, 0.7f, loop: true),
            Track("Music_Boss", MusicCue.Boss, 0.8f, loop: true),
            Track("Music_GameOver", MusicCue.GameOver, 0.7f, loop: false),
            Track("Music_Victory", MusicCue.Victory, 0.7f, loop: false),
        };

        public static string GetSfxPath(string assetName)
        {
            return $"{SfxFolder}/{assetName}.asset";
        }

        public static string GetMusicPath(string assetName)
        {
            return $"{MusicFolder}/{assetName}.asset";
        }

        private static Definition Spatial(
            string assetName, Slot slot, float volume, float minPitch, float maxPitch, int maxSimultaneous, float cooldown)
        {
            return new Definition(assetName, slot, SoundSpatial.ThreeD, volume, minPitch, maxPitch, maxSimultaneous, cooldown);
        }

        private static Definition Flat(
            string assetName, Slot slot, float volume, int maxSimultaneous, float cooldown, float minPitch = 1f, float maxPitch = 1f)
        {
            return new Definition(assetName, slot, SoundSpatial.TwoD, volume, minPitch, maxPitch, maxSimultaneous, cooldown);
        }

        private static Definition Track(string assetName, MusicCue cue, float volume, bool loop)
        {
            return new Definition(assetName, Slot.Music, SoundSpatial.TwoD, volume, 1f, 1f, 1, 0f).ForMusic(cue, loop);
        }

        /// <summary>Bir <see cref="SoundData"/> asset'inin tanımı.</summary>
        public sealed class Definition
        {
            public Definition(
                string assetName,
                Slot slot,
                SoundSpatial spatial,
                float volume,
                float minPitch,
                float maxPitch,
                int maxSimultaneous,
                float cooldown)
            {
                AssetName = assetName;
                Slot = slot;
                Spatial = spatial;
                Volume = volume;
                MinPitch = minPitch;
                MaxPitch = maxPitch;
                MaxSimultaneous = maxSimultaneous;
                Cooldown = cooldown;
            }

            public string AssetName { get; }

            public Slot Slot { get; }

            public SoundSpatial Spatial { get; }

            public float Volume { get; }

            public float MinPitch { get; }

            public float MaxPitch { get; }

            public int MaxSimultaneous { get; }

            public float Cooldown { get; }

            public float MinDistance => NearDistance;

            public float MaxDistance => FarDistance;

            /// <summary><see cref="Slot.WeaponFired"/> için silah id'si.</summary>
            public string WeaponId { get; private set; } = string.Empty;

            /// <summary><see cref="Slot.PhaseChanged"/> için evre.</summary>
            public GamePhase Phase { get; private set; }

            /// <summary><see cref="Slot.Music"/> için durum.</summary>
            public MusicCue Cue { get; private set; }

            /// <summary><see cref="Slot.Music"/>: bitince başa sarar mı.</summary>
            public bool Loop { get; private set; }

            public Definition ForWeapon(string weaponId)
            {
                WeaponId = weaponId;
                return this;
            }

            public Definition ForMusic(MusicCue cue, bool loop)
            {
                Cue = cue;
                Loop = loop;
                return this;
            }

            public Definition ForPhase(GamePhase phase)
            {
                Phase = phase;
                return this;
            }
        }
    }
}
