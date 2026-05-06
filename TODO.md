# TODO

Planned features and follow-ups. Top section is a "pick this up first next session" cue.

## Next session — priority list

User decided 2026-05-06: finish remaining v1.x items first, release on PDX Mods last.

1. **Better custom colors UX.** Today: 3 slots, each = name (text) + hex code (text input) + probability slider. Hex validation is in place (warning icon + locale text when probability > 0 but hex doesn't parse). Remaining pain points: no color preview swatch, no "enable this slot" toggle separate from probability=0. User to confirm scope.
2. **Release on PDX Mods.** First publish: edit `Properties\PublishConfiguration.xml` (long description, changelog, ModId left blank), then VS2022 right-click → Publish → `PublishNewMod` profile. Console returns a ModID GUID; paste it back into XML for future updates. Requires being logged into PDX in-game once. See WIKI.md §1 for the publish-flow details.

## v1.x — small additions

- [x] **"Reset to defaults" button** in the Options UI. Implemented via `[SettingsUIButton]` + `[SettingsUIConfirmation]` on `Setting.ResetToDefaults`; setter calls `SetDefaults()`. Confirmation prompt localized via `GetOptionWarningLocaleID`. (2026-05-06: re-scoped to reset only the color sliders and moved to the Default Colors tab.)
- [x] **Debug tab.** Dump toggle moved out of the main General tab into a dedicated Debug tab.
- [x] **Refined option descriptions.** Mention "restart the game" where relevant, drop jargon, clarify that trailers/farm equipment keep stock paintwork.
- [x] **"Use custom color values" toggle** on the Default Colors tab. Default OFF: sliders hidden, `GetBucketWeight` returns hardcoded real-world weights. ON: sliders visible, rebalance reads their values. Toggling never touches slider values — Reset button is the way to restore the recommended mix.
- [x] **All-zero slider warning.** `[SettingsUIWarning]` on `UseCustomColors`: fires when toggle is on and every default-color slider sums to 0 (mod would otherwise silently fall back to vanilla-uniform via the post-rebalance failsafe).
- [x] **Hex validation warnings** on the three Custom*Hex inputs. Fires when probability > 0 but hex fails `ColorClassifier.TryParseHex` — flags both empty and malformed values.

## v2 — bigger work

- [x] **Live slider updates (newly spawned cars).** "Apply settings" button on Default Colors tab → `Mod.RequestLiveRebalance()` sets `volatile bool m_NeedsLiveRefresh` on the system → next `OnUpdate` (PrefabUpdate phase) bulk-strips `RebalancedTag` via `EntityManager.RemoveComponent<T>(query)` → existing rebalance pass re-runs with current slider values. Implemented on the `live-update` branch.
- [ ] **Live refresh of in-world car instances.** Today's "Apply settings" only affects newly spawned vehicles. Cars already on the map keep their old colors until they despawn. WIKI.md §7 caveat: would need to dirty cached `MeshColor` state on each existing instance (probably `BatchesUpdated` flag).
    - **Disabled-master case (when implementing the above):** if the master toggle is OFF and the user presses Apply / Refresh, every in-world car must revert to the vanilla stock distribution — not stay on whatever was last rebalanced. Practically: restore `m_Probability` on each `ColorVariation` entry from the `m_OrigProbabilities` snapshot, then re-roll instance colors. Without this, master-off + Refresh would leave cars frozen in the previous custom mix.
- [ ] **Empty-bucket synthesis.** Today only custom slots add new ColorVariation entries. If a prefab has zero red variations and the user pushes Red slider high, that slider has nothing to weight. Could synthesize a neutral red entry per prefab to give every slider some traction.
- [ ] **HSV cutoff tuning.** Thresholds in `ColorBuckets.cs` were guessed. Revisit after eyeballing in-traffic results — especially the brown / yellow / orange / red boundaries.

## Nice-to-have

- [x] **Quiet logs.** Heartbeat removed; system now logs OnCreate, dump path (when applicable), pass results, and errors only.
- [ ] **Per-vehicle-type overrides.** Today the same slider set applies to every prefab. Could let users tune cars vs trucks separately (e.g., trucks more grey/silver).
- [ ] **Dedicated orange bucket.** Currently orange falls into `Other`. The mod's tagline mentions "rare colors (orange) beyond the base palette" — make it a real first-class bucket once HSV cutoffs are tuned.
