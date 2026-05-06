# Default Civilian Vehicle Color Distribution (CS2 vanilla)

Captured 2026-05-05 / refined 2026-05-06 via the in-mod `DumpColorVariations` debug toggle.
Source data: `ModsData/RealisticVehicleColors/color_dump.csv`.

## How the data was captured

1. `RealisticVehicleColorsSystem` queries parent civilian-vehicle prefabs:
   - `All`: `PrefabData`, `SubMesh`, **`CarData`** (excludes trains, ships, airplanes that share `CargoTransportVehicleData`).
   - `Any`: `PersonalCarData`, `DeliveryTruckData`, `CargoTransportVehicleData`, `CarTrailerData`.
2. For each parent, walks the `SubMesh` buffer to reach child render-prefab entities, which hold the `ColorVariation` buffer.
3. Snapshots each entry's `m_Probability` byte before any rebalance touches it.
4. Records prefab classification flags (`is_car`, `is_delivery_truck`, `is_cargo_truck`, `is_bicycle`, `is_trailer`) and `PersonalCarData.m_Probability` (cars only — trucks have no spawn-weight equivalent).

## Findings

### `m_Probability` is uniform in stock data

Every `ColorVariation.m_Probability` = 100 in vanilla. Wiki §8: *"Probability is uniform across Color Sets. To bias one color, duplicate its entry."*  Modelers bias colors via duplicate entries.

### Spawn probability (`PersonalCarData.m_Probability`) is mostly uniform

19 of 24 personal-car prefabs (cars + bikes/scooters + camper trailer) have `m_Probability = 100`. Only the 5 MuscleCar prefabs are at 50. Trucks have no equivalent — they're picked by cargo type/capacity in `*SelectData` systems.

Result: fleet-weighted ≈ unweighted, within 0.1pp.

### Distribution — cars only (ex-bikes, ex-trailers), fleet-weighted

n = 1450 variations across 19 car/bike/scooter prefabs (`is_car=True, is_bicycle=False, is_trailer=False`).

| Bucket | Vanilla CS2 | Mod default (real-world) | Delta |
| --- | --- | --- | --- |
| Grey | 21.4% | 13 | overrepresented |
| Silver | 16.8% | 12 | overrepresented |
| Blue | 13.3% | 10 | slight over |
| White | 12.3% | 26 | **massively under** |
| Brown | 9.6% | 4 | overrepresented |
| Black | 9.2% | 22 | **massively under** |
| Green | 5.5% | 2 | overrepresented |
| Yellow | 5.0% | 1 | overrepresented |
| Red | 4.0% | 9 | underrepresented |
| Other | 2.9% | 1 | slight over |

### Distribution — all civilian road vehicles, unweighted

Cars + bikes + trailers + civilian trucks, each variation equally weighted. n = 1947.

| Bucket | % |
| --- | --- |
| Grey | 21.1% |
| Silver | 16.7% |
| Blue | 13.8% |
| White | 12.0% |
| Brown | 9.8% |
| Black | 9.1% |
| Green | 5.3% |
| Yellow | 4.7% |
| Red | 4.5% |
| Other | 3.0% |

## Civilian-vehicle prefab inventory (vanilla)

### Cars + motorbikes + scooters (`PersonalCarData`, no `BicycleData`, no `CarTrailerData`) — 19 prefabs

| Prefab | Variations | `m_Probability` |
| --- | --- | --- |
| Car01 | 89 | 100 |
| Car02 | 74 | 100 |
| Car03 | 74 | 100 |
| Car04 | 74 | 100 |
| Car05 | 74 | 100 |
| Car06 | 74 | 100 |
| Car07 | 74 | 100 |
| Car08 | 74 | 100 |
| Car09 | 74 | 100 |
| Car10 | 89 | 100 |
| Van01 | 74 | 100 |
| Motorbike01 | 51 | 100 |
| Motorbike02 | 89 | 100 |
| Scooter01 | 51 | 100 |
| MuscleCar01 | 74 | 50 |
| MuscleCar02 | 74 | 50 |
| MuscleCar03 | 89 | 50 |
| MuscleCar04 | 89 | 50 |
| MuscleCar05 | 89 | 50 |

### Bicycles (`PersonalCarData` + `BicycleData`) — 4 prefabs, 292 variations

`Bicycle01`, `Bicycle02`, `Bicycle03`, `ElectricScooter01` (73 each).

### Civilian trucks (`DeliveryTruckData` or `CargoTransportVehicleData`, on road via `CarData`) — 7 prefabs

| Prefab | Variations | Notes |
| --- | --- | --- |
| EU_DeliveryVan01 | 6 | DeliveryTruck |
| NA_DeliveryVan01 | 6 | DeliveryTruck |
| EU_TruckTractor01 | 9 | DeliveryTruck (semi-tractor cab) |
| NA_TruckTractor01 | 9 | DeliveryTruck (semi-tractor cab) |
| MotorbikeDelivery01 | 51 | DeliveryTruck (food-delivery scooter) |
| CoalTruck01 | 6 | DeliveryTruck |
| OilTruck01 | 6 | DeliveryTruck |

### Trailers (`CarTrailerData`) — 16 prefabs

- `CamperTrailer01` (89 vars) — also has `PersonalCarData`. Rebalances to civilian distribution.
- `TruckTrailer01–04` (3 vars each) — also `DeliveryTruckData`. Rebalances.
- `TractorTrailer01–03`, `TractorPlough01`, `TractorSowingMachine01`, `TractorSprayer01`, `TractorFertilizerSpreader01`, `ForestForwarderTrailer01`, `ForestHarvesterTrailer01`, `FrontendLoaderTrailer01`, `OreMiningTractorTrailer01` (1 var each) — agricultural / forestry trailers, single-color, rebalance is a no-op.

### Excluded — non-road vehicles caught by `CargoTransportVehicleData` until the `CarData` filter was added

| Prefab | Variations |
| --- | --- |
| AirplaneCargo01 | 1 |
| ShipCargo01 | 5 |
| ShipCargo02 | 5 |
| TrainAgricultureCar01 | 6 |
| TrainOilCar01 | 6 |
| TrainCargoCar01 | 6 |
| TrainOreCar01 | 6 |
| TrainForestryCar01 | 6 |

## Takeaways

- Stock CS2 traffic over-represents **grey + silver + brown + green + yellow**, under-represents **white + black + red** vs real-world.
- Combined grey + silver in vanilla = ~38%, vs ~25% real-world.
- Combined white + black in vanilla = ~22%, vs ~48% real-world.
- The mod's default sliders shift the fleet — more white cars, far fewer grey/silver/brown/yellow.
- Per-prefab spawn weights are essentially uniform; only MuscleCars are genuinely rare.
- Bicycles inflate white/grey/silver. Filter via `is_bicycle` for "real" car analysis.
- Agricultural trailers have 1 color each — rebalancing them is a no-op but keeps the query simple.

## Caveats

- Color classification follows `ColorClassifier.Classify` (HSV thresholds in `ColorBuckets.cs`).
- Trucks aren't fleet-weighted (no `m_Probability`). Treated as flat counts.
- Modded prefabs would shift these numbers. Capture done with no asset mods loaded.
- A SubMesh shared between a car and a truck is rebalanced via the car's pass; if the user disables `IncludeTrucks`, the shared SubMesh still inherits car colors.
