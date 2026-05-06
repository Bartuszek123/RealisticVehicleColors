using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

namespace RealisticVehicleColors
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(RealisticVehicleColors)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        public static Setting Settings { get; private set; }
        public static RealisticVehicleColorsSystem System { get; private set; }

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

            Settings = new Setting(this);
            Settings.SetDefaults();
            Settings.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Settings));
            // Third arg must be a fresh defaults instance — properties matching it are NOT
            // serialized, so passing Settings itself would suppress every save.
            var defaults = new Setting(this);
            defaults.SetDefaults();
            AssetDatabase.global.LoadSettings(nameof(RealisticVehicleColors), Settings, defaults);

            updateSystem.UpdateAt<RealisticVehicleColorsSystem>(SystemUpdatePhase.PrefabUpdate);
            System = updateSystem.World.GetExistingSystemManaged<RealisticVehicleColorsSystem>();
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }
            System = null;
        }

        // Called from the Apply settings button in Options. Setter runs on the
        // main thread (UI click), but ECS structural changes must wait for our
        // PrefabUpdate phase — so we just flag the system and let OnUpdate do it.
        public static void RequestLiveApply()
        {
            if (System == null)
            {
                log.Warn("RequestLiveApply: system not yet registered, ignoring.");
                return;
            }
            System.RequestLiveApply();
        }
    }
}
