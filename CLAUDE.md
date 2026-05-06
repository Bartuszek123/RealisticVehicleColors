# RealisticVehicleColors

CS2 mod that redistributes civilian vehicle colors to match real-world statistics, adds rare colors (orange) beyond the base palette, and exposes per-color sliders in the in-game Options menu. Targets PDX Mods.

See `WIKI.md` for distilled CS2 modding-wiki reference (toolchain, Options UI attribute reference, ECS/Prefab patterns, vehicle component catalog, logging, settings persistence, debugging, dev mode).
See `TODO.md` for the current priority list (top section is "next session — start here").
See `_research/default_color_distribution.md` for the captured vanilla fleet color distribution.

## Current state (2026-05-06)

**v1 verified working in-game.** User set Red and Blue sliders to 0 and confirmed no red or blue cars appeared in traffic. End-to-end pipeline (prefab walk → ColorVariation buffer rewrite → MeshColorSystem picks new probabilities at spawn) is functional.

What's done:
- Civilian-vehicle prefab walk (parent → `SubMesh` → child render prefab → `ColorVariation` buffer).
- Per-bucket slider weights with HSV-based color classification (`ColorBuckets.cs`).
- Custom color slots (3, hex input + probability).
- `CamperTrailer01` restricted to white/brown.
- Standalone trailers (semi-trailers, agri/forestry) skipped from rebalance.
- Trains/ships/airplanes excluded via `CarData` filter.
- Settings persistence (`.coc` at `ModsSettings/RealisticVehicleColors/`).
- Reset-to-defaults button in Options.
- Debug tab with one-shot CSV dump tool.
- Failsafes: camper white=brown=0 drops the restriction; total probability=0 falls back to uniform 100.
- Snapshot of original `m_Probability` so the dump always emits stock data even if rebalance ran first.

Known limitations (NOT bugs — accepted for v1):
- **Existing in-world cars keep their old color**; only newly spawned vehicles get the new distribution. Fleet rotates over a few minutes of in-game time at 3× speed. (Refreshing in-world instances would need explicit dirty-state on cached MeshColor — see WIKI.md §7.)

**Live update (post-v1, on `live-update` branch — verified in-game 2026-05-06):** instead of relying on `onSettingsApplied` (which would fire per-keystroke and risks running structural ECS changes off the PrefabUpdate phase), there's an explicit "Apply settings" button on the Default Colors tab. Click → `Mod.RequestLiveApply()` flips a `volatile bool` (`m_NeedsLiveApply`) in the system → next `OnUpdate` (PrefabUpdate phase, safe) handles both the prefab-side update and the in-world refresh. No event hooks, no debouncing. Confirmed working: live refresh of in-world cars on Apply (master ON and OFF), and clean uninstall — disabling/removing the mod returns every in-world car to vanilla colors on next launch (we only mutate runtime prefab buffers, never save data).

- **Master ON + Apply:** bulk-remove `RebalancedTag` from every civilian-vehicle prefab via `EntityManager.RemoveComponent<T>(query)` → rebalance pass picks them back up with current slider values.
- **Master OFF + Apply:** walk every still-tagged prefab, trim custom-color appends back to `OriginalColorVariationCount.Value`, restore each entry's `m_Probability` from the `m_OrigProbabilities` snapshot, then strip the tag. The Apply button is visible regardless of the master toggle so this path is reachable.
- **In-world refresh (both paths):** after the prefab buffers are at their final state, query every entity with `Game.Vehicles.Vehicle + MeshColor + PrefabRef`, filter to civilian-vehicle prefabs (same predicate as rebalance: `CarData` + any of `PersonalCarData/DeliveryTruckData/CargoTransportVehicleData/CarTrailerData`, minus standalone trailers and trucks-when-disabled), and bulk-add `BatchesUpdated`. That puts each instance on `MeshColorSystem.m_UpdateQuery`, which re-runs `SetMeshColorsJob` next frame — the per-entity RNG seed is fixed so the new pick is deterministic from the new probabilities.

Next: see `TODO.md` "Next session" section. Tomorrow's first task is **release on PDX Mods**.

## Scope

- **In:** civilian road vehicles. Query: `All { PrefabData, SubMesh, CarData }` + `Any { PersonalCarData, DeliveryTruckData, CargoTransportVehicleData, CarTrailerData }`.
- **Out:** police, fire, ambulance, garbage, taxis, transit, any service vehicle (filtered by their distinct `*Data` markers — they're not in the `Any[]` set).
- **`CarData` is the road-vehicle gate.** Without it, `CargoTransportVehicleData` would also match `TrainXxxCar01`, `ShipCargo01/02`, `AirplaneCargo01`. `CarPrefab`/`CarTrailerPrefab` add `CarData`; trains/ships/airplanes don't.
- **Bicycles** carry `PersonalCarData` — distinguish via the `BicycleData` tag.
- **Trailers** are caught via `CarTrailerData` for the dump, but rebalance **skips standalone trailers** (`CarTrailerData` without `PersonalCarData`). Rationale: semi-trailer paint reflects the cargo company, not the driver, and agri/forestry trailers ship with a single variation anyway. `CamperTrailer01` has `PersonalCarData` so it IS rebalanced — but with weights **restricted to white + brown only** (campers IRL are typically beige/white).
- **Color model:** named buckets (white, black, grey, silver, red, blue, ..., orange-rare). Each bucket gets a slider 0-100 representing weight. Sliders normalize at runtime — they do not need to sum to 100. On vehicle spawn → weighted random → pick a `ColorVariation`.
- **New colors:** inject extra `ColorVariation` entries at load (orange + any other rares).

## Toolchain & build

- IDE: Visual Studio 2022.
- Template: `dotnet new csiimod` (already scaffolded).
- Entry point: `class Mod : IMod` in `Mod.cs` with `OnLoad(UpdateSystem)` / `OnDispose()`.
- Build: `dotnet build` — auto-deploys DLL to `%CSII_LOCALMODSPATH%` (`C:\Users\xxbar\AppData\LocalLow\Colossal Order\Cities Skylines II\Mods`).
- **Do not edit `Mod.props` or `Mod.targets`** — auto-generated by the toolchain.
- Disable a deployed mod for testing by prefixing its folder with `.` (e.g. `.RealisticVehicleColors`), then restart the game.

## Settings pattern

- Subclass `ModSetting`, decorate with `[FileLocation("ModsSettings/<ModName>/<ModName>")]` (explicit path — the bare-name form does not produce a `.coc` file).
- `LoadSettings(name, settings, defaults)` — third arg MUST be a fresh defaults instance, not `settings` itself. Properties matching the third-arg defaults are NOT serialized; passing `settings` as third arg silently disables persistence.
- Call `RegisterInOptionsUI()` from `OnLoad`.
- Sliders: `[SettingsUISlider(min, max, step)]` on int/float properties.
- Toggles: bare bool properties.
- Grouping: `[SettingsUISection("Tab", "Group")]`. Tab/group order via `[SettingsUITabOrder]` / `[SettingsUIGroupOrder]` on the class.
- Master-toggle gating: `[SettingsUIHideByCondition(typeof(MyModSetting), nameof(IsMasterDisabled))]`.

## Settings layout (current)

- Tab `General`: master on/off, "include civilian trucks" toggle.
- Tab `Default Colors`: "Use custom color values" toggle (default OFF — `GetBucketWeight` returns hardcoded real-world weights, sliders hidden); ON exposes the sliders + a "Reset color sliders" button. Toggling never modifies slider values; user presses Reset to restore the recommended mix. Above all of that sits the "Apply settings" button (hidden only when master off) which triggers a live re-rebalance without a game restart.
- Tab `Custom Colors`: 3 slots, each = name + hex code + probability slider, hidden when master off. Hex inputs carry a `[SettingsUIWarning]` that fires when probability > 0 but the hex fails `ColorClassifier.TryParseHex`.
- Tab `Debug`: one-shot CSV dump toggle.

## Localization

English-only at launch. Structure via `IDictionarySource` so adding a language is drop-in. No translator workflow needed yet.

## Vehicle color architecture (resolved via decompile)

Two-entity layout — easy to get wrong:

- **Parent vehicle prefab entity** carries `PrefabData` + the civilian-vehicle `*Data` IComponentData (`PersonalCarData` / `DeliveryTruckData` / `CargoTransportVehicleData`) + a `SubMesh` IBufferElementData buffer.
- **Child render-prefab entity**, reached via each `SubMesh` entry's `m_SubMesh` field, holds the `ColorVariation` IBufferElementData buffer.
- **A query that requires both `PersonalCarData` and `ColorVariation` on one entity matches nothing** — the previous v1 code did this and silently no-op'd. Always walk parent → SubMesh → child.

Selection at spawn: `Game.Rendering.MeshColorSystem` does weighted reservoir sampling per `ColorGroupID`, with `ColorVariation.m_Probability` as the weight (proportional, not absolute).

In stock data `m_Probability` is uniform = 100 across all entries — modelers bias colors by **duplicating entries**, not by tweaking the byte. This means rebalance can equivalently work in either of two ways: rewrite per-entry `m_Probability`, or duplicate/remove entries. v1 rewrites the byte.

Per-prefab spawn weight: `PersonalCarData.m_Probability` (consumed by `PersonalCarSelectData`). 100 for most cars, 50 for the 5 vanilla MuscleCars. Trucks have no equivalent. See `_research/default_color_distribution.md` for the captured fleet distribution.

## ECS gotchas to remember

- `EntityManager.AddComponentData(e, ...)` triggers an archetype move on `e`. Any `DynamicBuffer<T>` you grabbed via `GetBuffer<T>(e)` BEFORE the structural change is **invalidated** — re-fetch immediately after. Symptom: crash inside `Buffer.Memcpy` from `DynamicBuffer.get_Item`.
- `SystemUpdatePhase.PrefabUpdate` is invoked from `PrefabSystem.OnUpdate` (which runs every frame in `MainLoop`). Safe phase for one-shot prefab edits.
- `PrefabSystem.GetPrefabName(Entity)` resolves a prefab entity's authoring name — useful for logging/CSV.
- **Main menu → load is NOT a clean reset.** Verified empirically: prefab entities + their `RebalancedTag` persist across main-menu transitions. `OnDispose` fires on game quit, not on main-menu navigation. The `live-update` branch addresses this by exposing an "Apply settings" button that bulk-removes `RebalancedTag` from every civilian-vehicle prefab so the rebalance query picks them up again — no need to restart for newly spawned cars to use new sliders.

## Diagnostic dump

`Setting.DumpColorVariations` toggle writes `ModsData/RealisticVehicleColors/color_dump.csv` with one row per `ColorVariation` entry: prefab name, vehicle-type flags, `is_bicycle`, `prefab_probability`, RGB, original `m_Probability` (snapshotted before any rebalance touches it). Re-toggle on a fresh launch to recapture; settings now persist correctly.

## PDX Mods publish

- Edit `Properties\PublishConfiguration.xml` before publishing.
- First publish: VS2022 right-click → Publish → `PublishNewMod` profile. Returns a ModID GUID.
- Paste returned ModID into `PublishConfiguration.xml` `<ModId Value="..." />` for subsequent `PublishNewVersion` / `UpdatePublishedConfiguration` runs.
- Requires PDX login in-game first.
