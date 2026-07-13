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

        // When on, any prefab missing a stock color in a bucket the user weighted
        // above zero gets a synthetic stand-in variation injected, so e.g. the Red
        // slider still produces red cars on models that never shipped a red variant.
        // Only meaningful in custom-color mode (in default mode every bucket is
        // weighted, which would repaint every car in every color).
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        public bool SynthesizeMissingColors { get; set; }

        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        [SettingsUIButton]
        [SettingsUIConfirmation]
        public bool ResetToDefaults { set { SetDefaultColorWeights(); } }

        // ── Custom colors ──────────────────────────────────────────────────────
        // Each slot has an explicit Enable toggle. The hex + probability fields only
        // appear once the slot is enabled, and a slot contributes to traffic only
        // when enabled (probability=0 alone no longer silently disables it). The hex
        // field's label doubles as a textual preview of the parsed color.
        [SettingsUISection(CustomColorsTab, Slot1Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        public bool Custom1Enabled { get; set; }

        [SettingsUITextInput]
        [SettingsUISection(CustomColorsTab, Slot1Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsSlot1Hidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetCustom1HexLabel))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        [SettingsUIWarning(typeof(Setting), nameof(IsCustom1HexInvalid))]
        public string Custom1Hex { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(CustomColorsTab, Slot1Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsSlot1Hidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetCustom1Label))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        public int Custom1Probability { get; set; }

        [SettingsUISection(CustomColorsTab, Slot2Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        public bool Custom2Enabled { get; set; }

        [SettingsUITextInput]
        [SettingsUISection(CustomColorsTab, Slot2Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsSlot2Hidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetCustom2HexLabel))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        [SettingsUIWarning(typeof(Setting), nameof(IsCustom2HexInvalid))]
        public string Custom2Hex { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(CustomColorsTab, Slot2Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsSlot2Hidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetCustom2Label))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        public int Custom2Probability { get; set; }

        [SettingsUISection(CustomColorsTab, Slot3Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        public bool Custom3Enabled { get; set; }

        [SettingsUITextInput]
        [SettingsUISection(CustomColorsTab, Slot3Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsSlot3Hidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetCustom3HexLabel))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        [SettingsUIWarning(typeof(Setting), nameof(IsCustom3HexInvalid))]
        public string Custom3Hex { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(CustomColorsTab, Slot3Group)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsSlot3Hidden))]
        [SettingsUIDisplayName(typeof(Setting), nameof(GetCustom3Label))]
        [SettingsUIValueVersion(typeof(Setting), nameof(GetSliderVersion))]
        public int Custom3Probability { get; set; }

        // ── Helpers ────────────────────────────────────────────────────────────
        public bool IsMasterDisabled() => !Enabled;
        public bool AreColorSlidersHidden() => !Enabled || !UseCustomColors;

        // A custom slot's hex + probability fields are shown only when the master
        // switch is on AND that slot's Enable toggle is on.
        public bool IsSlot1Hidden() => !Enabled || !Custom1Enabled;
        public bool IsSlot2Hidden() => !Enabled || !Custom2Enabled;
        public bool IsSlot3Hidden() => !Enabled || !Custom3Enabled;

        // A slot actually contributes color to traffic only when enabled, given a
        // positive probability and a parseable hex. Single source of truth reused by
        // the label/percent/total helpers and (via the same three conditions) by the
        // rebalance pass's AppendCustom calls.
        public bool IsSlot1Active() => Custom1Enabled && Custom1Probability > 0 && ColorClassifier.TryParseHex(Custom1Hex, out _);
        public bool IsSlot2Active() => Custom2Enabled && Custom2Probability > 0 && ColorClassifier.TryParseHex(Custom2Hex, out _);
        public bool IsSlot3Active() => Custom3Enabled && Custom3Probability > 0 && ColorClassifier.TryParseHex(Custom3Hex, out _);

        // Warning fires only while UseCustomColors is on — when off, Rebalance
        // bypasses these sliders entirely and uses the hardcoded defaults instead.
        public bool AreColorSlidersAllZero()
        {
            if (!UseCustomColors) return false;
            return White + Black + Grey + Silver + Red + Blue + Brown + Green + Yellow + Other == 0;
        }

        // A custom slot warns when it is enabled but the hex code doesn't parse —
        // meaning the slot is switched on yet would be silently dropped at load time.
        public bool IsCustom1HexInvalid() => Custom1Enabled && !ColorClassifier.TryParseHex(Custom1Hex, out _);
        public bool IsCustom2HexInvalid() => Custom2Enabled && !ColorClassifier.TryParseHex(Custom2Hex, out _);
        public bool IsCustom3HexInvalid() => Custom3Enabled && !ColorClassifier.TryParseHex(Custom3Hex, out _);

        // Live "(~X%)" suffix on each color slider's label. Custom slots only count
        // toward the total when active (enabled + positive probability + parseable
        // hex) — inactive slots contribute nothing in traffic, so including them
        // would shrink every other slider's displayed share for nothing.
        private int GetTotalSliderWeight()
        {
            int total = White + Black + Grey + Silver + Red + Blue + Brown + Green + Yellow + Other;
            if (IsSlot1Active()) total += Custom1Probability;
            if (IsSlot2Active()) total += Custom2Probability;
            if (IsSlot3Active()) total += Custom3Probability;
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

        // Custom slot label: shows "(~X%)" only when the slot is actually going to
        // contribute (active). Otherwise it's just "Probability" so the user isn't
        // told a misconfigured or off slot has any share.
        private LocalizedString MakeCustomSlotLabel(bool active, int weight)
        {
            if (!active) return LocalizedString.Value("Probability");
            return MakeColorLabel("Probability", weight);
        }
        public LocalizedString GetCustom1Label() => MakeCustomSlotLabel(IsSlot1Active(), Custom1Probability);
        public LocalizedString GetCustom2Label() => MakeCustomSlotLabel(IsSlot2Active(), Custom2Probability);
        public LocalizedString GetCustom3Label() => MakeCustomSlotLabel(IsSlot3Active(), Custom3Probability);

        // Textual color preview: the hex field's label reports which named color the
        // entered code resolves to (best we can do — the settings UI has no visual
        // color swatch widget). Falls back to a plain label when the hex is empty or
        // malformed (the warning icon covers the invalid case).
        private static LocalizedString MakeHexLabel(string hex)
        {
            if (ColorClassifier.TryParseHex(hex, out var c))
                return LocalizedString.Value($"Hex code — looks {ColorClassifier.BucketName(ColorClassifier.Classify(c))}");
            return LocalizedString.Value("Hex code");
        }
        public LocalizedString GetCustom1HexLabel() => MakeHexLabel(Custom1Hex);
        public LocalizedString GetCustom2HexLabel() => MakeHexLabel(Custom2Hex);
        public LocalizedString GetCustom3HexLabel() => MakeHexLabel(Custom3Hex);

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
                h = h * 31 + (Custom1Enabled ? 1 : 0);
                h = h * 31 + (Custom2Enabled ? 1 : 0);
                h = h * 31 + (Custom3Enabled ? 1 : 0);
                h = h * 31 + (Custom1Hex?.GetHashCode() ?? 0);
                h = h * 31 + (Custom2Hex?.GetHashCode() ?? 0);
                h = h * 31 + (Custom3Hex?.GetHashCode() ?? 0);
                return h;
            }
        }

        // Real-world car-color distribution, rounded. Used both as slider defaults
        // and as the live values when UseCustomColors is OFF.
        private static int DefaultBucketWeight(ColorBucket b) => b switch
        {
            ColorBucket.White  => 33,
            ColorBucket.Black  => 23,
            ColorBucket.Grey   => 19,
            ColorBucket.Silver => 8,
            ColorBucket.Blue   => 6,
            ColorBucket.Red    => 3,
            ColorBucket.Brown  => 3,
            ColorBucket.Green  => 3,
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
            DumpColorVariations = false;
            UseCustomColors = false;
            SynthesizeMissingColors = false;

            SetDefaultColorWeights();

            // Default ON so upgrading users (whose .coc predates this key and thus
            // loads it as the default) keep their already-configured custom colors.
            // A fresh, unconfigured slot stays inert regardless — empty hex /
            // probability 0 means it contributes nothing even while enabled.
            Custom1Enabled = true; Custom1Hex = ""; Custom1Probability = 0;
            Custom2Enabled = true; Custom2Hex = ""; Custom2Probability = 0;
            Custom3Enabled = true; Custom3Hex = ""; Custom3Probability = 0;
        }
    }
}
