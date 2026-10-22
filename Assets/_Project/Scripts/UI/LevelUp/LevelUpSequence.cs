using System;
using System.Collections.Generic;
using LastHaven.Core;

namespace LastHaven.UI
{
    /// <summary>Seviye kartlarının aşaması.</summary>
    public enum LevelUpStage
    {
        /// <summary>Kart yok; oyun (bu akış yüzünden) duraklı değil.</summary>
        Closed,

        /// <summary>Kartlar açık, seçim bekleniyor.</summary>
        Choosing,

        /// <summary>Kart seçildi; seçim animasyonu oynuyor, sonra sıradaki seviye veya devam.</summary>
        Confirming,
    }

    /// <summary><see cref="LevelUpSequence.Tick"/>'in sonucu: görünümün ne yapması gerektiği.</summary>
    public enum LevelUpTickResult
    {
        /// <summary>Değişiklik yok.</summary>
        None,

        /// <summary>Kuyruktaki sıradaki seviyenin kartları çekildi; oyun duraklı kalır.</summary>
        NextOffer,

        /// <summary>Kartlar kapandı; oyun devam ediyor.</summary>
        Closed,
    }

    /// <summary>
    /// Seviye kartlarının akışı (saf C#; GDD §13). <see cref="TryOpen"/> bekleyen seviyeyi kuyruktan alır
    /// (<see cref="IExperienceService.TryDequeueLevelUp"/>), <see cref="IPerkService.GenerateOffer"/> ile kartları çeker ve oyunu
    /// <see cref="IPauseService"/> ile duraklatır (kaynak bu akış). Seçim (<see cref="Choose"/>) giriş gecikmesinden sonra kabul edilir:
    /// perk hemen alınır, seçim animasyonu süresince beklenir; sonra kuyrukta seviye varsa onun kartları gelir (duraklama sürer), yoksa
    /// kapanır ve oyun devam eder. Alınabilecek perk kalmamışsa seviye kartsız geçer (<see cref="LevelSkipped"/>). Zaman gerçek
    /// zamandır (<see cref="Tick"/>'e <c>Time.unscaledDeltaTime</c>). Ne zaman açılabileceği <see cref="LevelUpPanelRules"/>'ta.
    /// </summary>
    public sealed class LevelUpSequence
    {
        private readonly IExperienceService _experience;
        private readonly IPerkService _perks;
        private readonly IPauseService _pause;
        private readonly float _inputDelay;
        private readonly float _confirmDuration;
        private readonly List<string> _offer = new List<string>();
        private readonly Countdown _inputTimer = new Countdown();
        private readonly Countdown _confirmTimer = new Countdown();

        /// <param name="pause">Null ise oyun duraklatılmaz (kartlar yine açılır).</param>
        /// <param name="inputDelay">Kartlar açıldıktan sonra seçimin kabul edilmediği süre (sn).</param>
        /// <param name="confirmDuration">Seçimden sonra devam etmeden önceki bekleme (sn).</param>
        public LevelUpSequence(IExperienceService experience, IPerkService perks, IPauseService pause, float inputDelay,
            float confirmDuration)
        {
            _experience = experience ?? throw new ArgumentNullException(nameof(experience));
            _perks = perks ?? throw new ArgumentNullException(nameof(perks));
            _pause = pause;
            _inputDelay = Math.Max(0f, inputDelay);
            _confirmDuration = Math.Max(0f, confirmDuration);
        }

        /// <summary>Alınabilecek perk kalmadığı için kartsız geçilen seviye (bekleyen kuyruktan çıktı).</summary>
        public event Action<int> LevelSkipped;

        public LevelUpStage Stage { get; private set; }

        public bool IsOpen => Stage != LevelUpStage.Closed;

        /// <summary>Kartları açık olan seviye (1 tabanlı); kapalıyken son açılan.</summary>
        public int Level { get; private set; }

        /// <summary>Kartlardaki perk id'leri (soldan sağa); kapalıyken boş.</summary>
        public IReadOnlyList<string> Offer => _offer;

        /// <summary>Seçilen kartın indeksi; seçim yoksa -1.</summary>
        public int ChosenIndex { get; private set; } = -1;

        /// <summary>Seçim şu an kabul edilir mi (kartlar açık, giriş gecikmesi bitti, henüz seçilmedi).</summary>
        public bool AcceptsChoice => Stage == LevelUpStage.Choosing && !_inputTimer.IsRunning;

        /// <summary>
        /// Kapalıyken bekleyen en eski seviyenin kartlarını açar ve oyunu duraklatır. Kuyruk boşsa (veya hiçbir seviyeye perk
        /// kalmadıysa) false. Açık olup olmamaya karar vermek çağıranın işi (<see cref="LevelUpPanelRules.CanOpen"/>).
        /// </summary>
        public bool TryOpen()
        {
            if (IsOpen || !TryBeginNextLevel())
            {
                return false;
            }

            _pause?.Pause(this);
            return true;
        }

        /// <summary>
        /// <paramref name="index"/>'teki kartı seçer: perk alınır, seçim animasyonu başlar. Seçim kabul edilmiyorsa
        /// (<see cref="AcceptsChoice"/>), indeks geçersizse veya perk alınamazsa hiçbir şey değişmez ve false döner.
        /// </summary>
        public bool Choose(int index)
        {
            if (!AcceptsChoice || index < 0 || index >= _offer.Count || !_perks.ApplyPerk(_offer[index]))
            {
                return false;
            }

            ChosenIndex = index;
            Stage = LevelUpStage.Confirming;
            _confirmTimer.Start(_confirmDuration);
            return true;
        }

        /// <summary>Gerçek zamanla ilerletir. Seçim animasyonu bitince sıradaki seviyeye geçer veya kapanır (oyun devam eder).</summary>
        public LevelUpTickResult Tick(float unscaledDeltaTime)
        {
            switch (Stage)
            {
                case LevelUpStage.Choosing:
                    _inputTimer.Tick(unscaledDeltaTime);
                    return LevelUpTickResult.None;
                case LevelUpStage.Confirming when _confirmTimer.Tick(unscaledDeltaTime):
                    if (TryBeginNextLevel())
                    {
                        return LevelUpTickResult.NextOffer;
                    }

                    Close();
                    return LevelUpTickResult.Closed;
                default:
                    return LevelUpTickResult.None;
            }
        }

        /// <summary>
        /// Kartları kapatır ve bu akışın duraklatmasını kaldırır (seçim yapılmadıysa o seviyenin perk'i verilmez; run bittiğinde,
        /// bileşen kapanırken). Kapalıyken de güvenle çağrılır.
        /// </summary>
        public void Close()
        {
            Stage = LevelUpStage.Closed;
            ChosenIndex = -1;
            _offer.Clear();
            _inputTimer.Stop();
            _confirmTimer.Stop();
            _pause?.Resume(this);
        }

        private bool TryBeginNextLevel()
        {
            while (_experience.TryDequeueLevelUp(out int level))
            {
                if (_perks.GenerateOffer(_offer, _perks.OfferSize) > 0)
                {
                    Level = level;
                    ChosenIndex = -1;
                    Stage = LevelUpStage.Choosing;
                    _confirmTimer.Stop();
                    if (_inputDelay > 0f)
                    {
                        _inputTimer.Start(_inputDelay);
                    }
                    else
                    {
                        _inputTimer.Stop();
                    }

                    return true;
                }

                LevelSkipped?.Invoke(level);
            }

            _offer.Clear();
            return false;
        }
    }
}
