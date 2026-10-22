using System.IO;
using System.Text.RegularExpressions;
using LastHaven.Core;
using LastHaven.Editor;
using LastHaven.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// UI asset'leri (6.3): UXML'ler kodun aradığı elementleri içerir (ad + tip), paneller gizli başlar, USS/TSS hatasız içe aktarılır,
    /// PanelSettings ayarları görevdeki gibidir. Asset oluşturmaz.
    /// </summary>
    public class UIGenerationTests
    {
        private const string KeyboardKeyClass = "key--keyboard";
        private const string GamepadKeyClass = "key--gamepad";

        [Test]
        public void Hud_ContainsAllElements()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);

            AssertScreenRoot(root, startsHidden: false);
            foreach (string phaseLabel in HudElementNames.PhaseLabels)
            {
                AssertHas<Label>(root, phaseLabel);
            }

            AssertHas<Label>(root, HudElementNames.DayTimer);
            AssertHas<Label>(root, HudElementNames.NightValue);
            AssertHas<VisualElement>(root, HudElementNames.RemainingGroup);
            AssertHas<Label>(root, HudElementNames.RemainingValue);
            AssertHas<VisualElement>(root, HudElementNames.PlayerHealthFill);
            AssertHas<Label>(root, HudElementNames.PlayerHealthValue);
            AssertHas<VisualElement>(root, HudElementNames.GeneratorHealthFill);
            AssertHas<Label>(root, HudElementNames.GeneratorHealthValue);
            AssertHas<Label>(root, HudElementNames.WeaponName);
            AssertHas<Label>(root, HudElementNames.AmmoValue);
            AssertHas<Label>(root, HudElementNames.MoneyValue);
            AssertHas<Label>(root, HudElementNames.FuelValue);
            AssertHas<VisualElement>(root, HudElementNames.FuelFill);
            AssertHas<Label>(root, HudElementNames.FuelEstimate);
            AssertHas<VisualElement>(root, HudElementNames.PowerWarning);
            AssertHas<VisualElement>(root, HudElementNames.ComboGroup);
            AssertHas<Label>(root, HudElementNames.ComboCount);
            AssertHas<Label>(root, HudElementNames.ComboMultiplier);
            AssertHas<VisualElement>(root, HudElementNames.ComboWindowFill);

            AssertHas<VisualElement>(root, HudElementNames.BuildBar);
            AssertHas<VisualElement>(root, HudElementNames.BuildCards);
            AssertHas<Label>(root, HudElementNames.BuildDescription);
            AssertHas<Label>(root, HudElementNames.BuildNoSelection);
            AssertHas<VisualElement>(root, HudElementNames.StructureTooltip);
            AssertHas<Label>(root, HudElementNames.TooltipName);
            AssertHas<Label>(root, HudElementNames.TooltipLevelName);
            AssertHas<Label>(root, HudElementNames.TooltipLevel);
            AssertHas<Label>(root, HudElementNames.TooltipHealthValue);
            AssertHas<VisualElement>(root, HudElementNames.TooltipHealthFill);
            AssertHas<VisualElement>(root, HudElementNames.TooltipUpgrade);
            AssertHas<Label>(root, HudElementNames.TooltipUpgradeCost);
            AssertHas<VisualElement>(root, HudElementNames.TooltipMaxLevel);
            AssertHas<Label>(root, HudElementNames.TooltipSellRefund);
            AssertHas<VisualElement>(root, HudElementNames.InteractionPrompt);
            AssertHas<VisualElement>(root, HudElementNames.InteractionRing);
            AssertHas<Label>(root, HudElementNames.InteractionText);
            AssertHas<VisualElement>(root, HudElementNames.LootIndicators);
            AssertHas<VisualElement>(root, HudElementNames.AttackIndicators);
            foreach (string indicator in HudElementNames.AttackIndicatorsByDirection)
            {
                AssertHas<VisualElement>(root, indicator);
            }

            AssertHas<VisualElement>(root, HudElementNames.IntelPanel);
            AssertHas<VisualElement>(root, HudElementNames.IntelEnemies);
            AssertHas<VisualElement>(root, HudElementNames.IntelEvent);
            AssertHas<Label>(root, HudElementNames.IntelEventName);
            AssertHas<Label>(root, HudElementNames.IntelEventDescription);
            AssertHas<VisualElement>(root, HudElementNames.DayWarning);
            AssertHas<VisualElement>(root, HudElementNames.DayWarningPulse);
            AssertHas<Label>(root, HudElementNames.DayCountdown);
            AssertHas<VisualElement>(root, HudElementNames.ReadyHint);
            AssertHas<VisualElement>(root, HudElementNames.ReadyBonus);
            AssertHas<Label>(root, HudElementNames.ReadyBonusValue);
            AssertHas<VisualElement>(root, HudElementNames.BossPanel);
            AssertHas<Label>(root, HudElementNames.BossName);
            AssertHas<VisualElement>(root, HudElementNames.BossHealthFill);
            AssertHas<Label>(root, HudElementNames.BossHealthValue);
            AssertHas<VisualElement>(root, HudElementNames.XpGroup);
            AssertHas<Label>(root, HudElementNames.XpLevel);
            AssertHas<VisualElement>(root, HudElementNames.XpFill);
            AssertHas<Label>(root, HudElementNames.XpValue);
            AssertHas<VisualElement>(root, HudElementNames.PerkList);
            AssertHas<VisualElement>(root, HudElementNames.PerkTooltip);
            AssertHas<Label>(root, HudElementNames.PerkTooltipName);
            AssertHas<Label>(root, HudElementNames.PerkTooltipStacks);
            AssertHas<Label>(root, HudElementNames.PerkTooltipDescription);
            AssertHas<VisualElement>(root, HudElementNames.FloatingTexts);
            AssertHas<VisualElement>(root, HudElementNames.Banner);
            AssertHas<VisualElement>(root, HudElementNames.BannerDay);
            AssertHas<Label>(root, HudElementNames.BannerDayNumber);
            AssertHas<VisualElement>(root, HudElementNames.BannerNight);
            AssertHas<Label>(root, HudElementNames.BannerNightNumber);
            AssertHas<VisualElement>(root, HudElementNames.BannerDawn);
            AssertHas<VisualElement>(root, HudElementNames.BannerBoss);
            AssertHas<Label>(root, HudElementNames.BannerSubtitle);
        }

        /// <summary>
        /// 14.1: uçan yazı katmanı ekranın doğrudan çocuğu, boş (etiketleri kod oluşturur) ve HUD kutularından önce (altta çizilir);
        /// evre başlığı ve tüm grupları gizli başlar, her grubun başlığı metnini UXML'den alır.
        /// </summary>
        [Test]
        public void Hud_FloatingTextsAndBanner_Layout()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);
            VisualElement screen = root.Q(className: UIClassNames.Screen);
            VisualElement layer = root.Q(HudElementNames.FloatingTexts);
            VisualElement banner = root.Q(HudElementNames.Banner);

            Assert.AreSame(screen, layer.parent);
            Assert.AreEqual(0, layer.childCount, "Etiketleri kod oluşturur.");
            Assert.AreEqual(PickingMode.Ignore, layer.pickingMode, "Katman fareyi yakalamaz.");
            Assert.AreSame(screen, banner.parent);
            Assert.Less(screen.IndexOf(layer), screen.IndexOf(root.Q(HudElementNames.PlayerHealthFill).parent.parent.parent),
                "HUD kutuları yazıların üstünde çizilir.");
            Assert.IsTrue(banner.ClassListContains(UIClassNames.Hidden), "Başlık gizli başlar.");
            foreach (string group in new[]
                     {
                         HudElementNames.BannerDay, HudElementNames.BannerNight, HudElementNames.BannerDawn, HudElementNames.BannerBoss,
                     })
            {
                VisualElement element = root.Q(group);
                Assert.IsTrue(element.ClassListContains(UIClassNames.Hidden), $"{group} gizli başlar.");
                Assert.IsTrue(banner.Contains(element));
                Assert.IsFalse(string.IsNullOrEmpty(element.Q<Label>().text), $"{group}: başlık metni UXML'de.");
            }

            Assert.IsTrue(root.Q(HudElementNames.BannerSubtitle).ClassListContains(UIClassNames.Hidden));
        }

        [Test]
        public void HudFeelConfigAsset_ExistsWithPositiveTimings()
        {
            var config = AssetDatabase.LoadAssetAtPath<HudFeelConfig>(UIGenerator.HudFeelConfigPath);
            if (config == null)
            {
                Assert.Inconclusive($"{UIGenerator.HudFeelConfigPath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            Assert.GreaterOrEqual(config.FloatingTextPoolSize, 1);
            Assert.Greater(config.DamageLifetime, 0f);
            Assert.Greater(config.CoinLifetime, 0f);
            Assert.Greater(config.CreateBannerTiming().Total, 0f);
            Assert.Greater(config.GetDamageScale(config.BigDamage), config.GetDamageScale(0f), "Yüksek hasar daha büyük.");
        }

        /// <summary>13.3: perk listesi ve ipucu gizli başlar; liste can kutusuyla aynı sol alt sütunda, onun üstünde.</summary>
        [Test]
        public void Hud_PerkList_StartsHidden_AboveHealthInBottomLeft()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);
            VisualElement list = root.Q(HudElementNames.PerkList);
            VisualElement health = root.Q(HudElementNames.PlayerHealthFill);

            Assert.IsTrue(list.ClassListContains(UIClassNames.Hidden), "Perk yokken gizli.");
            Assert.IsTrue(root.Q(HudElementNames.PerkTooltip).ClassListContains(UIClassNames.Hidden));
            Assert.AreSame(list.parent, root.Q(HudElementNames.PerkTooltip).parent);
            Assert.IsTrue(list.parent.Contains(health), "Can kutusuyla aynı sütun.");
            Assert.Less(list.parent.IndexOf(list), list.parent.IndexOf(health.parent.parent), "Liste can kutusunun üstünde.");
        }

        [Test]
        public void LevelUpPanel_ContainsAllElements_StartsHidden()
        {
            TemplateContainer root = Instantiate(UIGenerator.LevelUpPanelPath);

            AssertScreenRoot(root, startsHidden: true);
            AssertHas<Label>(root, LevelUpElementNames.LevelValue);
            AssertHas<VisualElement>(root, LevelUpElementNames.Cards);
            AssertHas<VisualElement>(root, LevelUpElementNames.Pending);
            AssertHas<Label>(root, LevelUpElementNames.PendingValue);
            Assert.IsTrue(root.Q(LevelUpElementNames.Pending).ClassListContains(UIClassNames.Hidden));
            Assert.AreEqual(root.Query(className: KeyboardKeyClass).ToList().Count > 0, root.Query(className: GamepadKeyClass).ToList().Count > 0,
                "Klavye ve gamepad ipuçları birlikte.");
        }

        [Test]
        public void PerkCard_IsButton_ContainsAllElements_OneLabelPerRarity()
        {
            TemplateContainer root = Instantiate(UIGenerator.PerkCardPath);

            Assert.IsNotNull(root.Q<Button>(className: PerkCardView.CardClass), $"Kart kökü '{PerkCardView.CardClass}' sınıflı Button olmalı.");
            AssertHas<Label>(root, LevelUpElementNames.CardKey);
            AssertHas<VisualElement>(root, LevelUpElementNames.CardIcon);
            AssertHas<Label>(root, LevelUpElementNames.CardInitials);
            AssertHas<Label>(root, LevelUpElementNames.CardName);
            AssertHas<Label>(root, LevelUpElementNames.CardDescription);
            AssertHas<Label>(root, LevelUpElementNames.CardStacks);
            foreach (PerkRarity rarity in System.Enum.GetValues(typeof(PerkRarity)))
            {
                Assert.AreEqual(1, root.Query(className: PerkStyle.RarityClass("perk-card__rarity", rarity)).ToList().Count, rarity.ToString());
            }
        }

        [Test]
        public void LevelUpConfigAsset_ExistsWithPositiveTimings()
        {
            var config = AssetDatabase.LoadAssetAtPath<LevelUpPanelConfig>(UIGenerator.LevelUpConfigPath);
            if (config == null)
            {
                Assert.Inconclusive($"{UIGenerator.LevelUpConfigPath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            Assert.Greater(config.InputDelay, 0f, "Kazara seçim koruması.");
            Assert.Greater(config.ConfirmDuration, 0f, "Kısa seçim animasyonu.");
        }

        /// <summary>11.2: her saldırı yönünün (K, D, G, B) kendi göstergesi var; ok taşıyıcısı ve ok ucu var, hepsi gizli başlar.</summary>
        [Test]
        public void Hud_AttackIndicators_OnePerDirection_HiddenWithArrow()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);
            VisualElement container = root.Q(HudElementNames.AttackIndicators);

            Assert.AreEqual(CompassDirections.All.Count, HudElementNames.AttackIndicatorsByDirection.Length, "İndeks = yön.");
            Assert.AreEqual(CompassDirections.All.Count, container.Query(className: EdgeIndicatorView.IndicatorClass).ToList().Count);
            foreach (string name in HudElementNames.AttackIndicatorsByDirection)
            {
                VisualElement indicator = container.Q(name);
                Assert.IsNotNull(indicator, name);
                Assert.IsTrue(indicator.ClassListContains(UIClassNames.Hidden), name);
                Assert.IsTrue(new EdgeIndicatorView(indicator).IsValid, $"{name}: ok taşıyıcısı ve ok ucu olmalı.");
            }

            Assert.AreSame(root.Q(className: UIClassNames.Screen), container.parent, "Kenar konumu ekrana göre yazılır.");
            Assert.AreEqual(0, root.Q(HudElementNames.LootIndicators).Query(name: HudElementNames.AttackIndicatorNorth).ToList().Count,
                "Sandık göstergeleri saldırı oklarını sayamaz.");
        }

        /// <summary>11.2: bir gecede en fazla 5 tip + boss; satırlar gizli başlar ve adet/ad/boss etiketi taşır.</summary>
        [Test]
        public void Hud_IntelPanel_SixHiddenRows_StartsHidden()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);
            var rows = root.Q(HudElementNames.IntelEnemies).Query(className: IntelRowView.RowClass).ToList();

            Assert.AreEqual(6, rows.Count);
            foreach (VisualElement row in rows)
            {
                Assert.IsTrue(row.ClassListContains(UIClassNames.Hidden));
                Assert.IsTrue(new IntelRowView(row).IsValid);
            }

            Assert.IsTrue(root.Q(HudElementNames.IntelPanel).ClassListContains(UIClassNames.Hidden), "Gündüz dışında gizli.");
            Assert.IsTrue(root.Q(HudElementNames.IntelEvent).ClassListContains(UIClassNames.Hidden), "Olay yoksa gizli.");
        }

        [Test]
        public void Hud_DayWarningAndReadyHint_StartHidden()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);
            VisualElement screen = root.Q(className: UIClassNames.Screen);

            Assert.IsTrue(root.Q(HudElementNames.DayWarning).ClassListContains(UIClassNames.Hidden));
            Assert.AreSame(screen, root.Q(HudElementNames.DayWarning).parent, "Kenar nabzı tüm ekranı kaplar.");
            Assert.AreEqual(0, screen.IndexOf(root.Q(HudElementNames.DayWarning)), "Nabız her şeyin altında çizilir.");
            Assert.IsTrue(root.Q(HudElementNames.ReadyHint).ClassListContains(UIClassNames.Hidden));
        }

        /// <summary>12.2: boss barı üst sütunda (evre kutusunun altında), gizli başlar; ad metni koddan (boss verisi).</summary>
        [Test]
        public void Hud_BossPanel_StartsHidden_InTopColumn()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);
            VisualElement panel = root.Q(HudElementNames.BossPanel);

            Assert.IsTrue(panel.ClassListContains(UIClassNames.Hidden));
            Assert.AreSame(root.Q(HudElementNames.ReadyHint).parent, panel.parent, "Evre kutusu ve Hazırım ipucuyla aynı sütunda.");
            Assert.AreEqual(string.Empty, root.Q<Label>(HudElementNames.BossName).text);
            Assert.IsTrue(panel.Contains(root.Q(HudElementNames.BossHealthFill)));
        }

        /// <summary>11.1: GDD §9 "en yakın 3 sandık"; her göstergede dönen ok taşıyıcısı ve ok ucu var, hepsi gizli başlar.</summary>
        [Test]
        public void Hud_LootIndicators_ThreeHiddenIndicatorsWithArrow()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);
            VisualElement container = root.Q(HudElementNames.LootIndicators);
            var indicators = container.Query(className: EdgeIndicatorView.IndicatorClass).ToList();

            Assert.AreEqual(3, indicators.Count);
            foreach (VisualElement indicator in indicators)
            {
                Assert.IsTrue(indicator.ClassListContains(UIClassNames.Hidden));
                Assert.IsTrue(new EdgeIndicatorView(indicator).IsValid, "Ok taşıyıcısı ve ok ucu olmalı.");
            }

            Assert.AreSame(root.Q(className: UIClassNames.Screen), container.parent, "Kenar konumu ekrana göre yazılır.");
        }

        [Test]
        public void Hud_BuildBarTooltipAndPromptStartHidden()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);

            Assert.IsTrue(root.Q(HudElementNames.BuildBar).ClassListContains(UIClassNames.Hidden), "İnşa modu dışında gizli.");
            Assert.IsTrue(root.Q(HudElementNames.StructureTooltip).ClassListContains(UIClassNames.Hidden));
            Assert.IsTrue(root.Q(HudElementNames.InteractionPrompt).ClassListContains(UIClassNames.Hidden));
        }

        [Test]
        public void Hud_WorldAnchoredBoxes_AreDirectChildrenOfScreen()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);
            VisualElement screen = root.Q(className: UIClassNames.Screen);

            // WorldAnchor ebeveynin boyutunu ekran sayar ve konumu ebeveyne göre yazar.
            Assert.AreSame(screen, root.Q(HudElementNames.StructureTooltip).parent);
            Assert.AreSame(screen, root.Q(HudElementNames.InteractionPrompt).parent);
        }

        [Test]
        public void Hud_KeyHints_HaveKeyboardAndGamepadVariants_NeverHiddenClass()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);
            var keyboard = root.Query(className: KeyboardKeyClass).ToList();
            var gamepad = root.Query(className: GamepadKeyClass).ToList();

            Assert.Greater(keyboard.Count, 0);
            Assert.AreEqual(keyboard.Count, gamepad.Count, "Her klavye tuşunun gamepad karşılığı olmalı.");
            foreach (VisualElement key in keyboard)
            {
                Assert.IsFalse(key.ClassListContains(UIClassNames.Hidden), "Tuş görünürlüğünü lh-gamepad yönetir.");
            }

            foreach (VisualElement key in gamepad)
            {
                Assert.IsFalse(key.ClassListContains(UIClassNames.Hidden), "Tuş görünürlüğünü lh-gamepad yönetir.");
            }
        }

        [Test]
        public void BuildCard_ContainsAllElements()
        {
            TemplateContainer root = Instantiate(UIGenerator.BuildCardPath);

            Assert.IsNotNull(root.Q(className: BuildCardView.CardClass), $"Kart kökünde '{BuildCardView.CardClass}' sınıfı yok.");
            AssertHas<Label>(root, HudElementNames.CardKey);
            AssertHas<Label>(root, HudElementNames.CardPrice);
            AssertHas<VisualElement>(root, HudElementNames.CardSwatch);
            AssertHas<Label>(root, HudElementNames.CardName);
        }

        /// <summary>
        /// 10.3: renkler ve yazı boyutları sadece Theme.uss'te tanımlı; ekran USS'leri değişkenle (<c>var(--...)</c>) kullanır.
        /// </summary>
        [TestCase(UIGenerator.HudStylePath)]
        [TestCase(UIGenerator.HudBuildStylePath)]
        [TestCase(UIGenerator.HudInteractionStylePath)]
        [TestCase(UIGenerator.HudLootStylePath)]
        [TestCase(UIGenerator.HudIntelStylePath)]
        [TestCase(UIGenerator.PanelsStylePath)]
        [TestCase(UIGenerator.ShopStylePath)]
        [TestCase(UIGenerator.HudPerksStylePath)]
        [TestCase(UIGenerator.LevelUpStylePath)]
        [TestCase(UIGenerator.HudFeelStylePath)]
        [TestCase(UIGenerator.MenusStylePath)]
        public void StyleSheets_TakeColorsAndFontSizesFromTheme(string path)
        {
            string text = Regex.Replace(File.ReadAllText(path), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

            Assert.IsFalse(Regex.IsMatch(text, @"rgba?\(|#[0-9a-fA-F]{3,8}\b"), $"{path}: renk değeri yazılmış; Theme.uss değişkeni kullan.");
            foreach (Match property in Regex.Matches(text, @"(?<name>[\w-]*color|font-size|text-shadow)\s*:\s*(?<value>[^;]+);"))
            {
                Assert.IsTrue(property.Groups["value"].Value.Trim().StartsWith("var(", System.StringComparison.Ordinal),
                    $"{path}: '{property.Value}' Theme.uss değişkeni kullanmalı.");
            }
        }

        [Test]
        public void Hud_ComboStartsHidden()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);

            Assert.IsTrue(root.Q(HudElementNames.ComboGroup).ClassListContains(UIClassNames.Hidden), "Kombo yokken gizli olmalı.");
            Assert.IsTrue(root.Q(HudElementNames.ComboMultiplier).ClassListContains(UIClassNames.Hidden));
        }

        [Test]
        public void Hud_XpStrip_AlwaysVisible_DrawnBelowBuildBar()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);
            VisualElement screen = root.Q(className: UIClassNames.Screen);
            VisualElement xp = root.Q(HudElementNames.XpGroup);

            Assert.IsFalse(xp.ClassListContains(UIClassNames.Hidden), "XP şeridi her evrede görünür.");
            Assert.AreSame(screen, xp.parent);
            Assert.Less(screen.IndexOf(xp), screen.IndexOf(root.Q(HudElementNames.BuildBar)), "İnşa çubuğu XP şeridinin üstünde çizilir.");
            Assert.IsTrue(xp.Contains(root.Q(HudElementNames.XpFill)));
        }

        [Test]
        public void Hud_PowerWarningStartsHidden()
        {
            TemplateContainer root = Instantiate(UIGenerator.HudPath);

            Assert.IsTrue(root.Q(HudElementNames.PowerWarning).ClassListContains(UIClassNames.Hidden), "Güç varken gizli olmalı.");
        }

        [Test]
        public void DawnPanel_ContainsAllElements_StartsHidden()
        {
            TemplateContainer root = Instantiate(UIGenerator.DawnPanelPath);

            AssertScreenRoot(root, startsHidden: true);
            AssertHas<Label>(root, PanelElementNames.DawnNightValue);
            AssertHas<Label>(root, PanelElementNames.NightKillsValue);
            AssertHas<Label>(root, PanelElementNames.NightMoneyValue);
            AssertHas<Label>(root, PanelElementNames.NightBestComboValue);
            AssertHas<Label>(root, PanelElementNames.InterestValue);
            AssertHas<Label>(root, PanelElementNames.TotalKillsValue);
            AssertHas<Button>(root, PanelElementNames.ContinueButton);
        }

        [TestCase(UIGenerator.GameOverPanelPath)]
        [TestCase(UIGenerator.VictoryPanelPath)]
        public void RunEndPanels_ContainAllElements_StartHidden(string path)
        {
            TemplateContainer root = Instantiate(path);

            AssertScreenRoot(root, startsHidden: true);
            AssertHas<Label>(root, PanelElementNames.RunEndNightValue);
            AssertHas<Label>(root, PanelElementNames.TotalKillsValue);
            AssertHas<Button>(root, PanelElementNames.RestartButton);
        }

        [Test]
        public void ShopPanel_ContainsAllElements_StartsHidden()
        {
            TemplateContainer root = Instantiate(UIGenerator.ShopPanelPath);

            AssertScreenRoot(root, startsHidden: true);
            AssertHas<Label>(root, ShopElementNames.MoneyValue);
            AssertHas<Button>(root, ShopElementNames.CloseButton);
            Assert.AreEqual(System.Enum.GetValues(typeof(ShopCategory)).Length, ShopElementNames.Tabs.Length,
                "Her market sekmesi için bir buton olmalı.");
            Assert.AreEqual(ShopElementNames.Tabs.Length, ShopElementNames.Pages.Length);
            for (int i = 0; i < ShopElementNames.Tabs.Length; i++)
            {
                AssertHas<Button>(root, ShopElementNames.Tabs[i]);
                AssertHas<VisualElement>(root, ShopElementNames.Pages[i]);
            }
        }

        [Test]
        public void ShopCard_ContainsAllElements_NoticesStartHidden()
        {
            TemplateContainer root = Instantiate(UIGenerator.ShopCardPath);

            Assert.IsNotNull(root.Q(className: ShopCardView.CardClass), $"Kart kökünde '{ShopCardView.CardClass}' sınıfı yok.");
            AssertHas<Label>(root, ShopElementNames.CardName);
            AssertHas<Label>(root, ShopElementNames.CardDescription);
            AssertHas<VisualElement>(root, ShopElementNames.CardDots);
            AssertHas<Label>(root, ShopElementNames.CardPrice);
            AssertHas<Label>(root, ShopElementNames.CardMax);
            AssertHas<Label>(root, ShopElementNames.CardOwned);
            AssertHas<VisualElement>(root, ShopElementNames.CardLocked);
            AssertHas<Label>(root, ShopElementNames.CardRequirement);
            AssertHas<Label>(root, ShopElementNames.CardUnavailable);
            AssertHas<Button>(root, ShopElementNames.CardBuyButton);
            Assert.IsTrue(root.Q(ShopElementNames.CardMax).ClassListContains(UIClassNames.Hidden));
            Assert.IsTrue(root.Q(ShopElementNames.CardOwned).ClassListContains(UIClassNames.Hidden));
            Assert.IsTrue(root.Q(ShopElementNames.CardLocked).ClassListContains(UIClassNames.Hidden));
            Assert.IsTrue(root.Q(ShopElementNames.CardUnavailable).ClassListContains(UIClassNames.Hidden));
        }

        [TestCase(UIGenerator.ThemeStylePath)]
        [TestCase(UIGenerator.HudStylePath)]
        [TestCase(UIGenerator.HudBuildStylePath)]
        [TestCase(UIGenerator.HudInteractionStylePath)]
        [TestCase(UIGenerator.HudLootStylePath)]
        [TestCase(UIGenerator.HudIntelStylePath)]
        [TestCase(UIGenerator.PanelsStylePath)]
        [TestCase(UIGenerator.ShopStylePath)]
        [TestCase(UIGenerator.HudPerksStylePath)]
        [TestCase(UIGenerator.LevelUpStylePath)]
        [TestCase(UIGenerator.HudFeelStylePath)]
        [TestCase(UIGenerator.MenusStylePath)]
        public void StyleSheets_ImportWithoutErrors(string path)
        {
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);

            Assert.IsNotNull(sheet, path);
            Assert.IsFalse(sheet.importedWithErrors, $"{path} hatalı içe aktarıldı; konsola bak.");
            Assert.IsFalse(sheet.importedWithWarnings, $"{path} uyarıyla içe aktarıldı; konsola bak.");
        }

        [Test]
        public void RuntimeTheme_IsThemeStyleSheet()
        {
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(UIGenerator.RuntimeThemePath);

            Assert.IsNotNull(theme, UIGenerator.RuntimeThemePath);
            Assert.IsFalse(theme.importedWithErrors);
        }

        [Test]
        public void Configure_WritesScaleWithScreenSize1080p()
        {
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            try
            {
                UIGenerator.Configure(settings);

                AssertScaling(settings);
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void PanelSettingsAsset_IsConfiguredWithRuntimeTheme()
        {
            var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(UIGenerator.PanelSettingsPath);
            if (settings == null)
            {
                Assert.Inconclusive($"{UIGenerator.PanelSettingsPath} yok; Tools/Last Haven/Generate Default Data çalıştır.");
            }

            AssertScaling(settings);
            Assert.AreSame(AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(UIGenerator.RuntimeThemePath), settings.themeStyleSheet);
        }

        private static void AssertScaling(PanelSettings settings)
        {
            Assert.AreEqual(PanelScaleMode.ScaleWithScreenSize, settings.scaleMode);
            Assert.AreEqual(new Vector2Int(1920, 1080), settings.referenceResolution);
            Assert.AreEqual(PanelScreenMatchMode.MatchWidthOrHeight, settings.screenMatchMode);
        }

        private static TemplateContainer Instantiate(string path)
        {
            var layout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            Assert.IsNotNull(layout, path);
            return layout.Instantiate();
        }

        private static void AssertScreenRoot(VisualElement root, bool startsHidden)
        {
            VisualElement screen = root.Q(className: UIClassNames.Screen);
            Assert.IsNotNull(screen, $"UXML kökünde '{UIClassNames.Screen}' sınıfı yok.");
            Assert.AreEqual(startsHidden, screen.ClassListContains(UIClassNames.Hidden),
                startsHidden ? "Panel gizli başlamalı (editörde Game view'ı kapatmasın)." : "HUD görünür başlamalı.");
        }

        private static void AssertHas<T>(VisualElement root, string name) where T : VisualElement
        {
            Assert.IsNotNull(root.Q<T>(name), $"UXML'de '{name}' ({typeof(T).Name}) yok.");
        }
    }
}
