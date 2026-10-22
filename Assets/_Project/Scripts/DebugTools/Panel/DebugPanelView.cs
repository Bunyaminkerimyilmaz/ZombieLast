using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UIElements;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Debug panelinin elementleri (<c>UI/Debug/DebugPanel.uxml</c>, <see cref="DebugPanelElementNames"/>): bulur, düğme metni
    /// şablonlarını doldurur, gece ve zaman ölçeği düğmelerini üretir. Durum satırları <see cref="Status"/>'ta. Olaylara
    /// <see cref="DebugPanelCommands"/> bağlanır.
    /// </summary>
    public sealed class DebugPanelView
    {
        private readonly List<Button> _timeScaleButtons = new List<Button>();
        private readonly List<float> _timeScaleValues = new List<float>();
        private readonly VisualElement _root;
        private readonly VisualElement _nightButtons;
        private readonly VisualElement _timeScaleButtonRow;
        private readonly Label _stressInfo;
        private bool? _stressRunning;

        public DebugPanelView(VisualElement root)
        {
            _root = root;
            Screen = Find<VisualElement>(DebugPanelElementNames.Screen);
            Status = new DebugPanelStatusView(Find<Label>);
            AddMoney = Find<Button>(DebugPanelElementNames.AddMoneyButton);
            AddFuel = Find<Button>(DebugPanelElementNames.AddFuelButton);
            DrainFuel = Find<Button>(DebugPanelElementNames.DrainFuelButton);
            Invulnerable = Find<Toggle>(DebugPanelElementNames.InvulnerableToggle);
            SkipNight = Find<Button>(DebugPanelElementNames.SkipNightButton);
            KillAll = Find<Button>(DebugPanelElementNames.KillAllButton);
            SpawnHorde = Find<Button>(DebugPanelElementNames.SpawnHordeButton);
            _nightButtons = Find<VisualElement>(DebugPanelElementNames.NightButtons);
            LevelUp = Find<Button>(DebugPanelElementNames.LevelUpButton);
            RandomPerk = Find<Button>(DebugPanelElementNames.RandomPerkButton);
            PerkDropdown = Find<DropdownField>(DebugPanelElementNames.PerkDropdown);
            GivePerk = Find<Button>(DebugPanelElementNames.GivePerkButton);
            _timeScaleButtonRow = Find<VisualElement>(DebugPanelElementNames.TimeScaleButtons);
            _stressInfo = Find<Label>(DebugPanelElementNames.StressInfo);
            StressStart = Find<Button>(DebugPanelElementNames.StressStartButton);
            StressStop = Find<Button>(DebugPanelElementNames.StressStopButton);
        }

        /// <summary>Bulunamayan ilk elementin adı; hepsi bulunduysa null.</summary>
        public string MissingElement { get; private set; }

        public VisualElement Screen { get; }

        public DebugPanelStatusView Status { get; }

        public Button AddMoney { get; }

        public Button AddFuel { get; }

        public Button DrainFuel { get; }

        public Toggle Invulnerable { get; }

        public Button SkipNight { get; }

        public Button KillAll { get; }

        public Button SpawnHorde { get; }

        public Button LevelUp { get; }

        public Button RandomPerk { get; }

        public DropdownField PerkDropdown { get; }

        public Button GivePerk { get; }

        public Button StressStart { get; }

        public Button StressStop { get; }

        public void SetShown(bool shown)
        {
            Screen.EnableInClassList(DebugPanelElementNames.HiddenClass, !shown);
        }

        /// <summary>UXML'deki "{0}" şablonlarını inspector'daki miktarlarla doldurur.</summary>
        public void FormatAmounts(int money, int fuel, int lowFuel, int hordeCount)
        {
            Fill(AddMoney, money);
            Fill(AddFuel, fuel);
            Fill(DrainFuel, lowFuel);
            Fill(SpawnHorde, hordeCount);
        }

        /// <summary>Stres testi açıklamasındaki "{0}" (düşman) ve "{1}" (taret) şablonlarını doldurur.</summary>
        public void FormatStress(int enemies, int turrets)
        {
            _stressInfo.text = string.Format(CultureInfo.InvariantCulture, _stressInfo.text, enemies, turrets);
        }

        /// <summary>Test çalışırken sadece Durdur, değilken sadece Başlat tıklanır (değişince yazılır).</summary>
        public void SetStressRunning(bool running)
        {
            if (_stressRunning == running)
            {
                return;
            }

            _stressRunning = running;
            StressStart.SetEnabled(!running);
            StressStop.SetEnabled(running);
        }

        public void SetPerkChoices(List<string> displayNames)
        {
            PerkDropdown.choices = displayNames;
            PerkDropdown.index = displayNames.Count > 0 ? 0 : -1;
        }

        /// <summary>1..<paramref name="totalNights"/> düğmeleri; tıklanınca gece numarasıyla (1 tabanlı) çağrılır.</summary>
        public void BuildNightButtons(int totalNights, Action<int> onClicked)
        {
            _nightButtons.Clear();
            for (int number = 1; number <= totalNights; number++)
            {
                int captured = number;
                _nightButtons.Add(CreateSmallButton(number.ToString(CultureInfo.InvariantCulture), () => onClicked(captured)));
            }
        }

        /// <summary>Ölçek düğmeleri (ör. 0.5× 1× 2× 4×); tıklanınca ölçekle çağrılır.</summary>
        public void BuildTimeScaleButtons(IReadOnlyList<float> scales, Action<float> onClicked)
        {
            _timeScaleButtonRow.Clear();
            _timeScaleButtons.Clear();
            _timeScaleValues.Clear();
            for (int i = 0; i < scales.Count; i++)
            {
                float scale = scales[i];
                Button button = CreateSmallButton(DebugPanelStatusView.FormatScale(scale), () => onClicked(scale));
                _timeScaleButtonRow.Add(button);
                _timeScaleButtons.Add(button);
                _timeScaleValues.Add(scale);
            }
        }

        /// <summary>Geçerli ölçeğin düğmesini seçili gösterir.</summary>
        public void SelectTimeScale(float scale)
        {
            for (int i = 0; i < _timeScaleButtons.Count; i++)
            {
                _timeScaleButtons[i].EnableInClassList(DebugPanelElementNames.SelectedButtonClass,
                    Math.Abs(_timeScaleValues[i] - scale) < 0.0001f);
            }
        }

        private static void Fill(TextElement element, int value)
        {
            element.text = string.Format(CultureInfo.InvariantCulture, element.text, value);
        }

        private static Button CreateSmallButton(string text, Action onClicked)
        {
            var button = new Button(onClicked) { text = text };
            button.AddToClassList(DebugPanelElementNames.ButtonClass);
            button.AddToClassList(DebugPanelElementNames.SmallButtonClass);
            return button;
        }

        private T Find<T>(string name) where T : VisualElement
        {
            T element = _root.Q<T>(name);
            if (element == null && MissingElement == null)
            {
                MissingElement = name;
            }

            return element;
        }
    }
}
