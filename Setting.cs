using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI.Localization;
using UnityEngine;

namespace RealisticVehicleColors
{
    [FileLocation("ModsSettings/" + nameof(RealisticVehicleColors) + "/" + nameof(RealisticVehicleColors))]
    [SettingsUITabOrder(GeneralTab, DefaultColorsTab, CustomColorsTab, DebugTab)]
    [SettingsUIGroupOrder(MainGroup, ColorsGroup, Slot1Group, Slot2Group, Slot3Group, DebugGroup)]
    [SettingsUIShowGroupName(MainGroup, ColorsGroup, Slot1Group, Slot2Group, Slot3Group, DebugGroup)]
    public class Setting : ModSetting
    {
        public const string GeneralTab = "General";
        public const string DefaultColorsTab = "DefaultColors";
        public const string CustomColorsTab = "CustomColors";
        public const string DebugTab = "Debug";

        public const string MainGroup = "Main";
        public const string ColorsGroup = "Colors";
        public const string Slot1Group = "Slot1";
        public const string Slot2Group = "Slot2";
        public const string Slot3Group = "Slot3";
        public const string DebugGroup = "Debug";

        public Setting(IMod mod) : base(mod) { }

        // ── General ────────────────────────────────────────────────────────────
        [SettingsUISection(GeneralTab, MainGroup)]
        public bool Enabled { get; set; }

        [SettingsUISection(GeneralTab, MainGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        public bool IncludeTrucks { get; set; }

        // ── Debug ──────────────────────────────────────────────────────────────
        [SettingsUISection(DebugTab, DebugGroup)]
        public bool DumpColorVariations { get; set; }

        // ── Default colors ─────────────────────────────────────────────────────
        // OFF (default): sliders hidden, rebalance uses the baked-in real-world
        // distribution. ON: sliders visible and rebalance reads their values.
        // Toggling ON/OFF never modifies slider values — to restore the recommended
        // mix the user presses the Reset button below.
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        [SettingsUIWarning(typeof(Setting), nameof(AreColorSlidersAllZero))]
        public bool UseCustomColors { get; set; }

        // Visible regardless of the master toggle: with master OFF, Apply restores
        // every in-world car to the stock color mix (otherwise they'd stay frozen on
        // whatever was rebalanced last).
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIButton]
        public bool ApplySettings { set { Mod.RequestLiveApply(); } }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetWhiteLabel))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        public int White { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetBlackLabel))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        public int Black { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetGreyLabel))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        public int Grey { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetSilverLabel))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        public int Silver { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetRedLabel))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        public int Red { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetBlueLabel))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        public int Blue { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetBrownLabel))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        public int Brown { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetGreenLabel))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        public int Green { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetYellowLabel))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        public int Yellow { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetOtherLabel))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        public int Other { get; set; }

        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        [SettingsUIButton]
        [SettingsUIConfirmation]
        public bool ResetToDefaults { set { SetDefaultColorWeights(); } }

        // ── Custom colors ──────────────────────────────────────────────────────
        [SettingsUITextInput]
        [SettingsUISection(CustomColorsTab, Slot1Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        public string Custom1Name { get; set; }

        [SettingsUITextInput]
        [SettingsUISection(CustomColorsTab, Slot1Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        [SettingsUIWarning(typeof(Setting), nameof(IsCustom1HexInvalid))]
        public string Custom1Hex { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(CustomColorsTab, Slot1Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        public int Custom1Probability { get; set; }

        [SettingsUITextInput]
        [SettingsUISection(CustomColorsTab, Slot2Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        public string Custom2Name { get; set; }

        [SettingsUITextInput]
        [SettingsUISection(CustomColorsTab, Slot2Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        [SettingsUIWarning(typeof(Setting), nameof(IsCustom2HexInvalid))]
        public string Custom2Hex { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(CustomColorsTab, Slot2Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        public int Custom2Probability { get; set; }

        [SettingsUITextInput]
        [SettingsUISection(CustomColorsTab, Slot3Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        public string Custom3Name { get; set; }

        [SettingsUITextInput]
        [SettingsUISection(CustomColorsTab, Slot3Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        [SettingsUIWarning(typeof(Setting), nameof(IsCustom3HexInvalid))]
        public string Custom3Hex { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(CustomColorsTab, Slot3Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        public int Custom3Probability { get; set; }

        // ── Helpers ────────────────────────────────────────────────────────────
        public bool IsMasterDisabled() => !Enabled;
        public bool AreColorSlidersHidden() => !Enabled || !UseCustomColors;

        // Warning fires only while UseCustomColors is on — when off, Rebalance
        // bypasses these sliders entirely and uses the hardcoded defaults instead.
        public bool AreColorSlidersAllZero()
        {
            if (!UseCustomColors) return false;
            return White + Black + Grey + Silver + Red + Blue + Brown + Green + Yellow + Other == 0;
        }

        // A custom slot warns only when the user has dialled probability above 0
        // (so the slot is "on") but the hex code doesn't parse — meaning the slot
        // would be silently dropped at load time.
        public bool IsCustom1HexInvalid() => Custom1Probability > 0 && !ColorClassifier.TryParseHex(Custom1Hex, out _);
        public bool IsCustom2HexInvalid() => Custom2Probability > 0 && !ColorClassifier.TryParseHex(Custom2Hex, out _);
        public bool IsCustom3HexInvalid() => Custom3Probability > 0 && !ColorClassifier.TryParseHex(Custom3Hex, out _);

        // Live "(~X%)" suffix on each color slider's label. Total denominator includes
        // both default-color sliders and any custom slot with probability > 0 — that
        // matches what AppendCustom does in Rebalance, so the displayed % stays close
        // to what the user actually sees in traffic. Hex validity isn't checked here:
        // if a slot has probability > 0 but a broken hex it still pulls share away in
        // intent — the [SettingsUIWarning] on the hex field handles that misconfig.
        private int GetTotalSliderWeight()
        {
            int total = White + Black + Grey + Silver + Red + Blue + Brown + Green + Yellow + Other;
            if (Custom1Probability > 0) total += Custom1Probability;
            if (Custom2Probability > 0) total += Custom2Probability;
            if (Custom3Probability > 0) total += Custom3Probability;
            return total;
        }

        private LocalizedString MakeColorLabel(string name, int weight)
        {
            int total = GetTotalSliderWeight();
            if (total <= 0 || weight <= 0) return LocalizedString.Value(name);
            float pct = weight * 100f / total;
            string suffix = pct < 1f ? "<1%" : $"~{Mathf.RoundToInt(pct)}%";
            return LocalizedString.Value($"{name} ({suffix})");
        }

        public LocalizedString GetWhiteLabel()  => MakeColorLabel("White",         White);
        public LocalizedString GetBlackLabel()  => MakeColorLabel("Black",         Black);
        public LocalizedString GetGreyLabel()   => MakeColorLabel("Grey",          Grey);
        public LocalizedString GetSilverLabel() => MakeColorLabel("Silver",        Silver);
        public LocalizedString GetRedLabel()    => MakeColorLabel("Red",           Red);
        public LocalizedString GetBlueLabel()   => MakeColorLabel("Blue",          Blue);
        public LocalizedString GetBrownLabel()  => MakeColorLabel("Brown / Beige", Brown);
        public LocalizedString GetGreenLabel()  => MakeColorLabel("Green",         Green);
        public LocalizedString GetYellowLabel() => MakeColorLabel("Yellow",        Yellow);
        public LocalizedString GetOtherLabel()  => MakeColorLabel("Other",         Other);

        // Version bumps any time something the labels depend on changes, so the
        // settings-UI widget re-evaluates the dynamic display name. Including custom
        // probabilities since they shift the denominator.
        public int GetSliderVersion()
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + White;
                h = h * 31 + Black;
                h = h * 31 + Grey;
                h = h * 31 + Silver;
                h = h * 31 + Red;
                h = h * 31 + Blue;
                h = h * 31 + Brown;
                h = h * 31 + Green;
                h = h * 31 + Yellow;
                h = h * 31 + Other;
                h = h * 31 + Custom1Probability;
                h = h * 31 + Custom2Probability;
                h = h * 31 + Custom3Probability;
                return h;
            }
        }

        // Real-world car-color distribution, rounded. Used both as slider defaults
        // and as the live values when UseCustomColors is OFF.
        private static int DefaultBucketWeight(ColorBucket b) => b switch
        {
            ColorBucket.White  => 26,
            ColorBucket.Black  => 22,
            ColorBucket.Grey   => 13,
            ColorBucket.Silver => 12,
            ColorBucket.Blue   => 10,
            ColorBucket.Red    => 9,
            ColorBucket.Brown  => 4,
            ColorBucket.Green  => 2,
            ColorBucket.Yellow => 1,
            ColorBucket.Other  => 1,
            _ => 0,
        };

        public int GetBucketWeight(ColorBucket b)
        {
            if (!UseCustomColors) return DefaultBucketWeight(b);
            return b switch
            {
                ColorBucket.White => White,
                ColorBucket.Black => Black,
                ColorBucket.Grey => Grey,
                ColorBucket.Silver => Silver,
                ColorBucket.Red => Red,
                ColorBucket.Blue => Blue,
                ColorBucket.Brown => Brown,
                ColorBucket.Green => Green,
                ColorBucket.Yellow => Yellow,
                ColorBucket.Other => Other,
                _ => 0,
            };
        }

        private void SetDefaultColorWeights()
        {
            White  = DefaultBucketWeight(ColorBucket.White);
            Black  = DefaultBucketWeight(ColorBucket.Black);
            Grey   = DefaultBucketWeight(ColorBucket.Grey);
            Silver = DefaultBucketWeight(ColorBucket.Silver);
            Blue   = DefaultBucketWeight(ColorBucket.Blue);
            Red    = DefaultBucketWeight(ColorBucket.Red);
            Brown  = DefaultBucketWeight(ColorBucket.Brown);
            Green  = DefaultBucketWeight(ColorBucket.Green);
            Yellow = DefaultBucketWeight(ColorBucket.Yellow);
            Other  = DefaultBucketWeight(ColorBucket.Other);
        }

        public override void SetDefaults()
        {
            Enabled = true;
            IncludeTrucks = true;
            DumpColorVariations = false;
            UseCustomColors = false;

            SetDefaultColorWeights();

            Custom1Name = ""; Custom1Hex = ""; Custom1Probability = 0;
            Custom2Name = ""; Custom2Hex = ""; Custom2Probability = 0;
            Custom3Name = ""; Custom3Hex = ""; Custom3Probability = 0;
        }
    }
}
