using System.Collections.Generic;
using Colossal;

namespace RealisticVehicleColors
{
    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleEN(Setting setting) { m_Setting = setting; }

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
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Enabled)),       "Master switch. When off, the mod leaves vehicle colors untouched. Restart the game for changes to take effect." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.IncludeTrucks)),"Include civilian trucks" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.IncludeTrucks)), "Also rebalance delivery vans, cargo trucks and food-delivery scooters. Service vehicles (police, fire, ambulance, garbage, post, taxi, transit) are never touched. Cargo trailers and farm equipment keep their stock paintwork." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.UseCustomColors)), "Use custom color values" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.UseCustomColors)),  "When off, the mod uses its built-in real-world color distribution. Turn on to reveal the per-color sliders and apply your own values. Toggling this on or off never changes the slider values — to restore the recommended mix, press Reset color sliders." },
                { m_Setting.GetOptionWarningLocaleID(nameof(Setting.UseCustomColors)), "Every color slider is at 0. The mod will fall back to a uniform stock distribution. Adjust at least one slider above 0, or press Reset color sliders." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ResetToDefaults)), "Reset color sliders" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ResetToDefaults)),  "Restore the color sliders below to the mod's recommended real-world values. Custom-color slots and other settings are not affected. Restart the game to repaint vehicles." },
                { m_Setting.GetOptionWarningLocaleID(nameof(Setting.ResetToDefaults)),"All your slider tweaks will be replaced with the mod's recommended values. Continue?" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.DumpColorVariations)),"Dump color variations" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.DumpColorVariations)), "Debug tool. When on, the mod writes the game's stock vehicle colors to a CSV file in its data folder on the next save load. Useful for inspecting what the game ships with. Disable after capturing." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.White)),        "White" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Black)),        "Black" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Grey)),         "Grey" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Silver)),       "Silver" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Red)),          "Red" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Blue)),         "Blue" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Brown)),        "Brown / Beige" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Green)),        "Green" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Yellow)),       "Yellow" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Other)),        "Other" },

                { m_Setting.GetOptionDescLocaleID(nameof(Setting.White)),         "Relative weight for white vehicles. Sliders normalize against each other — they don't have to sum to 100. Restart the game to apply." },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Other)),         "Catch-all for colors that don't fit the named buckets (orange, purple, magenta, pink, etc.)." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom1Name)),  "Name" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom1Hex)),   "Hex code" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom1Probability)), "Probability" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Custom1Hex)),    "Six-digit hex like F58025 (with or without leading #). Leave empty or set Probability to 0 to disable this slot." },
                { m_Setting.GetOptionWarningLocaleID(nameof(Setting.Custom1Hex)), "This hex code is empty or invalid. The slot will be skipped until you enter a valid six-digit hex (e.g. F58025), or set Probability back to 0 to silence this warning." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom2Name)),  "Name" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom2Hex)),   "Hex code" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom2Probability)), "Probability" },
                { m_Setting.GetOptionWarningLocaleID(nameof(Setting.Custom2Hex)), "This hex code is empty or invalid. The slot will be skipped until you enter a valid six-digit hex (e.g. F58025), or set Probability back to 0 to silence this warning." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom3Name)),  "Name" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom3Hex)),   "Hex code" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Custom3Probability)), "Probability" },
                { m_Setting.GetOptionWarningLocaleID(nameof(Setting.Custom3Hex)), "This hex code is empty or invalid. The slot will be skipped until you enter a valid six-digit hex (e.g. F58025), or set Probability back to 0 to silence this warning." },
            };
        }

        public void Unload() { }
    }
}
