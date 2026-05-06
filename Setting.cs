using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;

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

        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(IsMasterDisabled))]
        [SettingsUIButton]
        public bool ApplySettings { set { Mod.RequestLiveRebalance(); } }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        public int White { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        public int Black { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        public int Grey { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        public int Silver { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        public int Red { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        public int Blue { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        public int Brown { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        public int Green { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
        public int Yellow { get; set; }

        [SettingsUISlider(min = 0, max = 100, step = 1, unit = "integer")]
        [SettingsUISection(DefaultColorsTab, ColorsGroup)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(AreColorSlidersHidden))]
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
