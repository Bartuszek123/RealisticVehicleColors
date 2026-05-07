using System.Collections.Generic;
using Colossal;

namespace RealisticVehicleColors
{
    public class LocaleEN : IDictionarySource
    {
        public LocaleEN(Setting setting) { m_Setting = setting; }

        private readonly Setting m_Setting;

        // Shared text — sliders behave the same way across the Default and Custom tabs.
        private const string SliderHowItWorks =
            "These sliders are weights, not strict percentages — they don't have to add up to 100. The mod balances them for you.";
        private const string CustomHexDesc =
            "Six-digit hex code, like F58025 (with or without a leading #). Any online color picker can give you one. Leave the field empty or set Probability to 0 to turn this slot off.";
        private const string CustomHexWarning =
            "This hex code is empty or invalid. The slot will be skipped until you enter a valid six-digit hex (e.g. F58025), or set Probability back to 0 to silence this warning.";
        private const string CustomProbabilityDesc =
            "How often cars get this custom color. Works just like the sliders on the Default Colors tab and competes against them. Set to 0 to turn this slot off.";

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(),                                "Realistic Vehicle Colors" },

                { m_Setting.GetOptionTabLocaleID(Setting.GeneralTab),             "General" },
                { m_Setting.GetOptionTabLocaleID(Setting.DefaultColorsTab),       "Default Colors" },
                { m_Setting.GetOptionTabLocaleID(Setting.CustomColorsTab),        "Custom Colors" },
                { m_Setting.GetOptionTabLocaleID(Setting.DebugTab),               "Debug" },

                { m_Setting.GetOptionGroupLocaleID(Setting.MainGroup),            "Main" },
                { m_Setting.GetOptionGroupLocaleID(Setting.ColorsGroup),          "Color Distribution" },
                { m_Setting.GetOptionGroupLocaleID(Setting.Slot1Group),           "Custom Slot 1" },
                { m_Setting.GetOptionGroupLocaleID(Setting.Slot2Group),           "Custom Slot 2" },
                { m_Setting.GetOptionGroupLocaleID(Setting.Slot3Group),           "Custom Slot 3" },
                { m_Setting.GetOptionGroupLocaleID(Setting.DebugGroup),           "Debug Tools" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Enabled)),      "Enabled" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Enabled)),       "Main on/off switch for the whole mod. Turn off to leave vehicle colors at the game's stock palette. Restart the game for the change to fully apply." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.UseCustomColors)), "Use custom color values" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.UseCustomColors)),  "Off: the mod picks the color mix automatically using real-world car-color statistics. On: the sliders below appear so you can tune the mix yourself. " + SliderHowItWorks },
                { m_Setting.GetOptionWarningLocaleID(nameof(Setting.UseCustomColors)), "Every color slider is at 0, so the mod can't pick anything. Traffic will fall back to a uniform mix of stock colors. Move at least one slider above 0, or press Reset color sliders." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ApplySettings)),   "Apply settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ApplySettings)),    "Apply your sliders, custom colors and toggles to the game. Cars already on the map re-roll their color so the change is visible immediately. With the mod disabled, this restores the vanilla color mix on every car." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ResetToDefaults)), "Reset color sliders" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ResetToDefaults)),  "Put every color slider back to the mod's recommended real-world values. Custom-color slots and the other settings are left alone. Press Apply settings afterwards to apply the change to the game. Disable the mod and press apply settings to revert to vanilla" },
                { m_Setting.GetOptionWarningLocaleID(nameof(Setting.ResetToDefaults)),"All your slider tweaks will be replaced with the mod's recommended values. Continue?" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DumpColorVariations)),"Dump color variations" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DumpColorVariations)), "Developer tool — most users can ignore this. When on, the mod writes a CSV listing of every vehicle's stock colors to its data folder on the next save load. Turn it off after the file appears." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.White)),        "White" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.White)),         "How often white cars appear in traffic." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Black)),        "Black" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Black)),         "How often black cars appear in traffic." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Grey)),         "Grey" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Grey)),          "How often grey cars appear in traffic." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Silver)),       "Silver" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Silver)),        "How often silver cars appear in traffic." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Red)),          "Red" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Red)),           "How often red cars appear in traffic." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Blue)),         "Blue" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Blue)),          "How often blue cars appear in traffic." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Brown)),        "Brown / Beige" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Brown)),         "How often brown, beige and tan cars appear in traffic." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Green)),        "Green" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Green)),         "How often green cars appear in traffic." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Yellow)),       "Yellow" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Yellow)),        "How often yellow cars appear in traffic." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Other)),        "Other" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Other)),         "How often unusual colors appear — orange, purple, magenta, pink and anything else that doesn't fit the buckets above." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom1Hex)),   "Hex code" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Custom1Hex)),    CustomHexDesc },
                { m_Setting.GetOptionWarningLocaleID(nameof(Setting.Custom1Hex)), CustomHexWarning },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom1Probability)), "Probability" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Custom1Probability)),  CustomProbabilityDesc },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom2Hex)),   "Hex code" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Custom2Hex)),    CustomHexDesc },
                { m_Setting.GetOptionWarningLocaleID(nameof(Setting.Custom2Hex)), CustomHexWarning },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom2Probability)), "Probability" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Custom2Probability)),  CustomProbabilityDesc },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom3Hex)),   "Hex code" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Custom3Hex)),    CustomHexDesc },
                { m_Setting.GetOptionWarningLocaleID(nameof(Setting.Custom3Hex)), CustomHexWarning },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom3Probability)), "Probability" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Custom3Probability)),  CustomProbabilityDesc },
            };
        }

        public void Unload() { }
    }
}
