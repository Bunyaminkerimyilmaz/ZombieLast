using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Progression;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Perk kısayolları ([Systems]/DebugPerks; debug panelinin "İlerleme" bölümü de bunları çağırır): F10 bir seviye atlatır (sonraki
    /// seviyeye yetecek XP; seviye kartları açılır, 13.3), Shift+F10 kartsız rastgele bir perk verir (13.2), panel seçilen perk'i verir
    /// (<see cref="GivePerk"/>). Rastgele perk normal kart çekilişiyle seçilir (<see cref="IPerkService.GenerateOffer"/>, nadirlik
    /// ağırlıklı, maks. yığındakiler çıkmaz); hepsi normal yoldan alınır (<see cref="IPerkService.ApplyPerk"/>). Sadece editörde ve
    /// development build'de çalışır (sınıf her build'de var; "missing script" olmasın).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DebugPerkControls : MonoBehaviour
    {
        [SerializeField, Tooltip("F10'un XP eklediği servis ([Systems]/Progression). Sahne adımı bağlar.")]
        private ExperienceService _experience;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string Prefix = "[Debug] ";

        private readonly List<string> _offer = new List<string>(1);
        private InputAction _action;
        private IPerkService _perks;

        private void Awake()
        {
            _action = DebugHotkeys.Create("<Keyboard>/f10", OnF10);
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _perks))
            {
                Debug.LogWarning("DebugPerkControls: IPerkService bulunamadı; Shift+F10 çalışmayacak. Sahnede [Systems]/Progression var mı?", this);
            }
        }

        private void OnEnable()
        {
            _action.Enable();
        }

        private void OnDisable()
        {
            _action.Disable();
        }

        private void OnDestroy()
        {
            _action.Dispose();
        }

        // F10 tek action; Shift basılıysa seviye yerine doğrudan perk verir (iki ayrı bağlama ikisini birden tetiklerdi).
        private void OnF10()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.shiftKey.isPressed)
            {
                GiveRandomPerk();
                return;
            }

            LevelUp();
        }

        /// <summary>Sonraki seviyeye yetecek XP ekler (F10): normal seviye akışı, kartlar açılır. Oyun bittiyse etkisiz.</summary>
        public void LevelUp()
        {
            if (_experience == null || _experience.Tracker == null)
            {
                Debug.LogWarning($"{Prefix}F10: ExperienceService yok; seviye atlanmadı.", this);
                return;
            }

            int amount = _experience.XpToNextLevel - _experience.Xp;
            _experience.AddXp(amount);
            Debug.Log($"{Prefix}F10: +{amount} XP (seviye {_experience.Level}).", this);
        }

        /// <summary>Ağırlıklı rastgele bir perk verir (Shift+F10), kartsız.</summary>
        public void GiveRandomPerk()
        {
            if (_perks == null)
            {
                Debug.LogWarning($"{Prefix}Shift+F10: IPerkService yok; perk verilmedi.", this);
                return;
            }

            if (_perks.GenerateOffer(_offer, 1) == 0)
            {
                Debug.Log($"{Prefix}Shift+F10: alınabilecek perk kalmadı (hepsi maks. yığında, toplam {_perks.TotalStacks}).", this);
                return;
            }

            GivePerk(_offer[0]);
        }

        /// <summary>Id'si verilen perk'i kartsız verir (maks. yığın kuralı geçerli). Alındıysa true.</summary>
        public bool GivePerk(string perkId)
        {
            if (_perks == null)
            {
                Debug.LogWarning($"{Prefix}Perk ver: IPerkService yok; perk verilmedi.", this);
                return false;
            }

            if (!_perks.ApplyPerk(perkId))
            {
                Debug.LogWarning($"{Prefix}Perk ver: '{perkId}' alınamadı (bilinmiyor veya maks. yığında: "
                                 + $"{_perks.GetStacks(perkId)}/{_perks.GetMaxStacks(perkId)}).", this);
                return false;
            }

            Debug.Log($"{Prefix}Perk verildi: {perkId} {_perks.GetStacks(perkId)}/{_perks.GetMaxStacks(perkId)} "
                      + $"(toplam {_perks.TotalStacks} yığın).", this);
            return true;
        }
#endif
    }
}
