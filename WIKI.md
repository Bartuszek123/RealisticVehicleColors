# CS2 Modding Wiki — Distilled Reference

Source: https://cs2.paradoxwikis.com/Modding (fetched 2026-05-05). This is a focused
extract for a code-only civilian-vehicle-color mod. Asset/map-pipeline pages are
omitted.

---

## 1. Modding Toolchain

**IDE:** Visual Studio 2022 (17.8+) or JetBrains Rider (2021.3.3+).
**Unity:** 2022.3.7f1 must be installed and licensed via Unity Hub (even if unused).
**Entities:** 1.0.14.
**.NET:** 8 recommended, 6 minimum (note: `ModPostProcessor.exe` itself targets .NET 6 — see memory note).
**Node.js:** 20.11 (only for UI mods).
**Template:** "CSII C# Mod Project Template" via NuGet (`dotnet new csiimod`, or VS/Rider New Project search "CSII").

### Environment variables (set by template)

| Variable | Purpose |
| --- | --- |
| `CSII_INSTALLATIONPATH` | Game install root |
| `CSII_USERDATAPATH` | `…\AppData\LocalLow\Colossal Order\Cities Skylines II` |
| `CSII_TOOLPATH` | `CSII_USERDATAPATH\.cache\Modding` |
| `CSII_LOCALMODSPATH` | `CSII_USERDATAPATH\Mods` (deploy target) |
| `CSII_UNITYMODPROJECTPATH` | `CSII_TOOLPATH\UnityModsProject` |
| `CSII_UNITYVERSION` | `Unity 2022.3.7f1` |
| `CSII_ENTITIESVERSION` | `1.0.14` |
| `CSII_MODPOSTPROCESSORPATH` | `…\StreamingAssets\~Tooling~\ModPostprocessor\ModPostProcessor.exe` |
| `CSII_MODPUBLISHERPATH` | `…\StreamingAssets\~Tooling~\ModPublisher\ModPublisher.exe` |
| `CSII_MANAGEDPATH` | `Cities2_Data\Managed` (game DLLs) |
| `CSII_ASSEMBLYSEARCHPATH` | Semicolon-separated extra search paths |
| `CSII_PATHSET` | Always `Build` |

### Build pipeline (`dotnet build`)
1. Roslyn compile (with source generation — required for Entities/Burst).
2. AfterBuild → `ModPostProcessor.exe` runs IL post-processors.
3. Burst compiles native DLLs (Win/Linux/macOS).
4. Output auto-copied to `CSII_LOCALMODSPATH\<ModName>\`.
5. Mod loads after world creation (delay-loaded — cannot inject during early init).

### Disable a deployed mod
Prefix folder with `.` (e.g. `.RealisticVehicleColors`), restart game.

### Publishing — `Properties\PublishConfiguration.xml`
- `LongDescription`: multi-line, supports markdown subset (headings, bullets, bold). **No leading whitespace** on multi-line entries.
- `ChangeLog`, `ModId` (GUID assigned after first publish — paste back into XML).

Three publish profiles (right-click in VS, or top-bar in Rider):
1. **PublishNewMod** — first upload; console returns the ModID.
2. **PublishNewVersion** — bumps version + changelog.
3. **UpdatePublishedConfiguration** — metadata-only update.

Prereq: launch game and log into PDX once; publisher inherits the auth.

### Gotchas
- Source-gen is **mandatory**.
- Cannot run code before world creation.
- Non-default install paths require manual env var edits.
- After installing the toolchain, restart Node processes so they pick up new env vars.

---

## 2. Options UI (Settings menu) — full attribute reference

### Skeleton

```csharp
[FileLocation($"ModsSettings/{nameof(MyModSetting)}/{nameof(MyModSetting)}")]
public class MyModSetting : ModSetting
{
    public MyModSetting(IMod mod) : base(mod) { }
}

// In Mod.OnLoad:
var settings = new MyModSetting(this);
settings.RegisterInOptionsUI();
```

### Controls

```csharp
// Toggle
public bool Toggle { get; set; }

// Slider (int)
[SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kDataMegabytes)]
public int IntSlider { get; set; }

// Slider (float)
[SettingsUISlider(min = 0f, max = 10000f, step = 0.0001f, scalarMultiplier = 10, unit = Unit.kCustom)]
[SettingsUICustomFormat(fractionDigits = 4, maxValueWithFraction = 5000, separateThousands = false)]
public float FloatSlider { get; set; }

// Button (action on set)
[SettingsUIButton]
public bool Button { set { /* action */ } }

[SettingsUIButton]
[SettingsUIConfirmation]
public bool ButtonWithConfirm { set { } }

[SettingsUIButton]
[SettingsUIButtonGroup("GroupName")]
public bool GroupedButton { set { } }

// Enum dropdown
public SomeEnum EnumDropdown { get; set; } = SomeEnum.Value1;

// Custom dropdown
[SettingsUIDropdown(typeof(MyModSetting), nameof(GetItems))]
[SettingsUIValueVersion(typeof(MyModSetting), nameof(itemsVersion))]
public string StringDropdown { get; set; } = "First";

public DropdownItem<string>[] GetItems() => new[] {
    new DropdownItem<string> { value = "First", displayName = GetOptionLabelLocaleID("First") },
};
public int itemsVersion { get; set; }

// Read-only text / multiline
public string Text => "shown text";

[SettingsUIMultilineText]
public string MultilineText => string.Empty;

// Text input
[SettingsUITextInput]
public string Input { get; set; } = "";

// Directory picker
[SettingsUIDirectoryPicker]
public string DirectoryPicker { get; set; } = "C:/";

// Key bindings
[SettingsUIKeyboardBinding(BindingKeyboard.W, "Action Name", shift: true)]
public ProxyBinding KbBinding { get; set; }

[SettingsUIGamepadBinding(BindingGamepad.Select, "Action Name", rightStick: true)]
public ProxyBinding PadBinding { get; set; }

[SettingsUIMouseBinding(BindingMouse.Forward, "Action Name", shift: true)]
public ProxyBinding MouseBinding { get; set; }

[SettingsUIBindingMimic(InputManager.kNavigationMap, "Mouse Primary Click")]
public ProxyBinding Mimic { get; set; }
```

### Modifier attributes

```csharp
[SettingsUIHidden]                                                           // never shown
[SettingsUISearchHidden]                                                     // hidden from search
[SettingsUIDeveloper]                                                        // dev-mode only
[SettingsUIAdvanced]                                                         // advanced-mode only
[SettingsUIHideByCondition(typeof(T), nameof(Method), invert: false)]       // dynamic hide
[SettingsUIDisableByCondition(typeof(T), nameof(Method), invert: false)]    // dynamic disable
[SettingsUISetter(typeof(T), nameof(SetterMethod))]                         // custom setter
[SettingsUIDisplayName(overrideValue: "Name")]                              // static label override
[SettingsUIDisplayName(typeof(T), nameof(GetName))]                         // dynamic label
[SettingsUIDescription(overrideValue: "...")]                               // description override
[SettingsUIWarning(typeof(T), nameof(Check))]                               // warning indicator
```

### Class-level organization

```csharp
[SettingsUISection("GroupName")]                                  // single group
[SettingsUISection("TabName", "GroupName")]                       // tab + group
[SettingsUISection("Tab", "SimpleGroup", "AdvancedGroup")]        // separate adv group
[SettingsUITabOrder("Tab1", "Tab2")]                              // tab order
[SettingsUIGroupOrder("Group1", "Group2")]                        // group order
[SettingsUIShowGroupName("GroupA", "GroupB")]                     // force-show group headers
[SettingsUITabWarningAttribute("Tab", typeof(T), nameof(Check))]
[SettingsUIPageWarningAttribute(typeof(T), nameof(Check))]
[SettingsUIKeyboardAction("Action", ActionType.Axis, usages: new[] {"Usage"})]
[SettingsUIMouseAction("Action", ActionType.Button)]
[SettingsUIGamepadAction("Action", ActionType.Vector2)]
```

---

## 3. Localization

The wiki page (`Localize_your_mod`) does not document the full IDictionarySource API.
Two practical paths:
- **Roll your own**: model after the `LocaleEN` class in the Vanilla mod template (referenced by the wiki as the canonical example).
- **Use the I18n EveryWhere mod** as a dependency.

`ModSetting` exposes helper methods for locale keys (use these, do not hardcode IDs):
`GetSettingsLocaleID()`, `GetOptionTabLocaleID(tab)`, `GetOptionGroupLocaleID(group)`,
`GetOptionLabelLocaleID(prop)`, `GetOptionDescLocaleID(prop)`. Custom dropdown items
should be labelled via `GetOptionLabelLocaleID(value)` (see Options UI section).

Note: localization API isn't fully documented on the wiki — confirm by reading the
template's `LocaleEN.cs` once toolchain is set up.

---

## 4. Logging — `Colossal.Logging`

```csharp
public static ILog Log { get; } = LogManager
    .GetLogger(nameof(MyMod))
    .SetShowsErrorsInUI(false);

// In OnLoad:
#if VERBOSE
    Log.effectivenessLevel = Level.Verbose;
#elif DEBUG
    Log.effectivenessLevel = Level.Debug;
#endif

Log.Debug("plain");
Log.DebugFormat("fmt {0}", x);                  // avoids alloc when filtered
if (Log.isDebugEnabled) Log.Debug(Expensive()); // gate expensive calls
Log.logStackTrace = true;                       // include stacktrace
```

Levels: DISABLED, EMERGENCY, FATAL, CRITICAL, ERROR, WARN, INFO, DEBUG, TRACE, VERBOSE, ALL.
Override globally: `--logsEffectiveness=DEBUG`.
Logs land in `%AppData%\..\LocalLow\Colossal Order\Cities Skylines II\Logs\<LoggerName>.log`.

Logger name **must match** mod folder, settings filename, and namespace.

---

## 5. Persistent settings — `.coc` files

```csharp
[FileLocation($"ModsSettings/{nameof(MyMod)}/{nameof(MyMod)}")]
public class MySettings { public bool BoolSetting { get; set; } }

// In OnLoad:
Settings = new MySettings();
AssetDatabase.global.LoadSettings("Mod Settings Section", Settings, new MySettings());
```

- Format: `.coc` (JSON-ish with named sections).
- Path: `ModsSettings/<ModName>/<ModName>.coc` under user data path.
- Auto-saves on property change.
- Properties matching the third-arg defaults are not serialized.
- Filename **must match** mod folder + logger name.

Standard locations (use `Colossal.PSI.Environment.EnvPath.kUserDataPath`):
- Logs: `Logs/YourMod.log`
- Settings: `ModsSettings/YourMod/YourMod.coc`
- Data: `ModsData/YourMod/*`
- Cache: `EnvPath.kCacheDataPath/.cache`
- Temp: `EnvPath.kTempDataPath`

---

## 6. ECS basics

- Built on Unity DOTS / Entities 1.0.14.
- All components must be `struct`.
- Three component flavours: `IComponentData`, `ISharedComponentData`, `IBufferElementData` (dynamic arrays).
- Extend `GameSystemBase` (Colossal-specific) instead of vanilla `SystemBase`.
- Modifying a component = read struct, mutate, write back via `EntityManager.SetComponentData`.

```csharp
public partial class MySystem : GameSystemBase
{
    private EntityQuery m_Query;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_Query = GetEntityQuery(
            ComponentType.ReadOnly<Building>(),
            ComponentType.ReadWrite<ElectricityConsumer>(),
            ComponentType.Exclude<Deleted>());
        RequireForUpdate(m_Query);
    }

    protected override void OnUpdate() { /* ... */ }
}
```

Wiki does **not** detail `SystemUpdatePhase` enum values, `IJobChunk`/`IJobEntity`, `EntityCommandBuffer`, or `BurstCompile` rules — fall back to Unity DOTS docs and decompiled Game.dll.

---

## 7. Prefab system

**Three-layer model — keep them straight:**

| Layer | Purpose | Access |
| --- | --- | --- |
| `PrefabBase` (authoring) | Vanilla baseline values shipped with the game | `prefabSystem.TryGetPrefab(prefabEntity, out PrefabBase pb)` |
| Prefab-Entity (ECS) | Tagged with `Game.Prefabs.PrefabData`; holds `*Data` components | `EntityManager.GetComponentData<XData>(prefabEntity)` |
| Instance Entity | Live runtime entity in the city; references prefab via `PrefabRef.m_Prefab` | Normal entity queries |

**Authoring vs Data:**
- Authoring components (no `Data` suffix, e.g. `DeathcareFacility`, `Workplace`): read-only baseline. Get via `prefabBase.TryGetExactly(out T authoring)`.
- Data components (`*Data` suffix): writable runtime values; modifying them affects newly placed/spawned things.

```csharp
PrefabSystem prefabSystem = World.DefaultGameObjectInjectionWorld
    .GetOrCreateSystemManaged<PrefabSystem>();

if (!prefabSystem.TryGetPrefab(prefabEntity, out PrefabBase prefabBase)) return;

var data = EntityManager.GetComponentData<DeathcareFacilityData>(prefabEntity);
data.m_ProcessingRate = baseRate * scalar;
EntityManager.SetComponentData(prefabEntity, data);
```

**Critical caveat:** existing instances do not hot-reload from prefab edits. Most
runtime values cache instance-side and only refresh on rebuild/extension/etc. Color
changes likely apply only to **newly spawned** vehicles unless an explicit refresh
is triggered.

---

## 8. Color Variations — what the asset-pipeline page says

The wiki names the *concept* but not the C# types. Field-level details we have:

- A mesh's **Render Prefab** carries a **Color Properties** component.
- That component holds a **list of Color Sets** (one per variation).
- Each Color Set has **3 channels** (R/G/B → element 0/1/2). These map to the asset's masked color regions (body, trim, accent — not literal RGB).
- **Probability is uniform across Color Sets.** To bias one color, **duplicate its entry**.
- HSB/Alpha randomness sliders apply per-set jitter to all variations.
- Each channel has a **"Can Be Modified By External"** flag — when set, an external system (company colors, transit line colors) may override that channel at runtime.

Wiki does **not** name the C# types (`ColorVariation`, `ColorVariationSet`) or describe
the spawn-time selection system. That remains a decompile target — search `Game.dll`
for `ColorVariation`, `ColorVariationSet`, `ColorProperties`, `Recolor`. The "Can Be
Modified By External" hook is the most promising integration point: it implies a
runtime side-channel exists for overriding picked colors.

---

## 9. Vehicles — components and systems catalog

Confirmed entries from `Systems_and_Components_catalog`:

### Components
| Type | Namespace | Purpose |
| --- | --- | --- |
| `Car` | `Game.Vehicles` | Car flags (emergency, queueing, PT-lane) |
| `CarCurrentLane` | `Game.Vehicles` | Lane reference + change data |
| `CarNavigation` | `Game.Vehicles` | Target pos/rot, max speed |
| `CarNavigationLane` | `Game.Vehicles` | Buffer of upcoming lanes |
| `Odometer` | `Game.Vehicles` | Cumulative mileage |
| `OwnedVehicle` | `Game.Vehicles` | Buffer on household: vehicles owned |
| `ParkedCar` | `Game.Vehicles` | Marks parked cars + lane/pos |
| `PersonalCar` | `Game.Vehicles` | Properties of a personally owned car (keeper + state flags) |
| `Vehicles` | `Game.Vehicles` | Tag — "this entity is a vehicle" |
| `Passenger` | `Game.Vehicles` | Buffer of passenger entities |
| `Taxi` | `Game.Vehicles` | Taxi-specific data |
| `Blocker` | `Game.Vehicles` | What/why this vehicle is blocked |
| `CurrentVehicle` | `Game.Creatures` | Creature → vehicle reference (driver/passenger flags) |
| `CarData` | `Game.Prefabs` | Prefab capabilities: max speed, braking, turning, energy type, size class |

### Systems
- `Game.Simulation.CarNavigationSystem` — `UpdateNavigationJob` for entities with `Car`, `CarCurrentLane`, `UpdateFrame`.
- `Game.Simulation.ResourceBuyerSystem` — pathfinding to sellers; spawns vehicles for resource hauling.

**Civilian-truck identifier — still TBD.** `PersonalCar` covers cars but the catalog
excerpt above does not name the civilian-truck variant. Open question — search the
catalog page or `Game.dll` for `DeliveryTruck`, `CargoTransport`, or similar.

---

## 10. Memory / Native disposal

| Allocator | Disposal |
| --- | --- |
| `Allocator.Temp` | Auto, end of frame — no action needed |
| `Allocator.TempJob` | Manual `Dispose()` at end of `OnUpdate`, OR `Dispose(jobHandle)` to defer until job completes |
| `Allocator.Persistent` | Manual `Dispose()` in `OnDestroy` |

Pattern:

```csharp
protected override void OnCreate() {
    m_Counts = new NativeArray<int>(N, Allocator.Persistent);
}

protected override void OnUpdate() {
    var queue = new NativeQueue<Action>(Allocator.TempJob);
    JobHandle h = /* ... */;
    queue.Dispose(h);          // free after job
}

protected override void OnDestroy() {
    m_Counts.Dispose();
}
```

Wiki does not cover Harmony unpatch, event unsubscription, or `EntityQuery` ownership. Treat those as standard .NET hygiene: store handles, undo in `OnDispose`/`OnDestroy`.

---

## 11. Debugging

**Attach Unity debugger from VS2022:** Debug → Attach Unity Debugger → pick game process.
**Rider:** Run → Attach to Unity Process.

**Prereqs (one-time):**
1. Read Unity version off `Cities2.exe` properties.
2. Replace `UnityPlayer.dll` in the game dir with the development build from your Unity Editor.
3. Edit `Cities2_Data/boot.config` and add `player-connection-debug=1`.
4. Confirm: "Development Build" label appears bottom-right in the running game.

The `CS2-ModdingTools` NuGet package can automate the above for Debug builds (recent
patches may break this — fall back to manual).

---

## 12. Developer mode

**Launch parameter:** `-developerMode` (per the Developer mode wiki page — single dash).
Note: the toolchain page elsewhere writes `--developerMode` (double dash). If one
form fails, try the other.

`--uiDeveloperMode` (separate flag) opens UI debugger at `http://localhost:9444`.

**In-game shortcuts:**
- `Tab` — developer menu
- `Home` — add-object panel (more objects than the default UI)

**Dev menu (Simulation tab) features:**
- Instant zone development, disable pollution, disable service requirements.
- Spawn events/disasters (pre-select a building first).
- "Bypass validation results" — skip placement checks for buildings/networks.

No documented vehicle-inspection or color-preview tooling.

---

## 13. Mod security / publishing

The wiki's `Mod_security` page covers **distribution hygiene only** (use Paradox
Mods, avoid BepInEx mods, check Skyve for conflicts). It does **not** publish a list
of forbidden APIs (network, reflection, file I/O outside allowed dirs). When in
doubt, stick to standard locations (§5) and avoid network calls.

---

## 14. Quick lookup — wiki URLs

| Topic | URL |
| --- | --- |
| Hub | https://cs2.paradoxwikis.com/Modding |
| Toolchain | https://cs2.paradoxwikis.com/Modding_Toolchain |
| Options UI | https://cs2.paradoxwikis.com/Options_UI |
| Color Variations (asset) | https://cs2.paradoxwikis.com/Assets:_Setting_Up_Color_Variations |
| ECS | https://cs2.paradoxwikis.com/ECS_-_Entity_Component_System |
| Prefab system | https://cs2.paradoxwikis.com/PrefabSystem |
| Prefab quick start | https://cs2.paradoxwikis.com/Prefab_-_Quick_Start |
| Systems | https://cs2.paradoxwikis.com/Systems |
| Components catalog | https://cs2.paradoxwikis.com/Systems_and_Components_catalog |
| Logging | https://cs2.paradoxwikis.com/Logging |
| Debugging | https://cs2.paradoxwikis.com/Debugging |
| Localization | https://cs2.paradoxwikis.com/Localize_your_mod |
| Settings file | https://cs2.paradoxwikis.com/Creating_a_Settings_File |
| Memory leaks | https://cs2.paradoxwikis.com/How_To_Avoid_Memory_Leaks |
| Naming standards | https://cs2.paradoxwikis.com/Naming_Folder_And_Files |
| Developer mode | https://cs2.paradoxwikis.com/Developer_mode |
| Code+UI walkthrough | https://cs2.paradoxwikis.com/Creating_UI_And_Code_Mods |
| Mod security | https://cs2.paradoxwikis.com/Mod_security |
| UI Modding | https://cs2.paradoxwikis.com/UI_Modding |
| Mod Key Binding | https://cs2.paradoxwikis.com/Mod_Key_Binding |
| Game units | https://cs2.paradoxwikis.com/Commonly_units_in_the_game |

---

## 15. Open items the wiki does not answer

These remain blockers / TBDs after a full read:
1. **Vehicle color application system** — wiki names `ColorVariation` only at the asset level. The C# type names, the spawn-time random-pick system, and the "Recolor" override path are undocumented. **Decompile `Game.dll`** to find them.
2. **Civilian truck tag** — `PersonalCar` is documented; the analogous truck component is not in the catalog excerpt. Check the full catalog page or grep decompiled assemblies.
3. **`SystemUpdatePhase` enum values** — wiki references them but doesn't list members. Decompile `Game.dll`.
4. **Localization (IDictionarySource)** — undocumented; mirror `LocaleEN` from the template.
5. **`-developerMode` vs `--developerMode`** — wiki contradicts itself; try both.
6. **Mod security technical rules** — not published; assume "no network, stick to standard dirs".
