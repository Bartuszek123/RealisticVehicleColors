using Game;
using Game.Prefabs;
using Game.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Scripting;

namespace RealisticVehicleColors
{
    // Marker tag added to a parent vehicle prefab once we have rebalanced its SubMesh
    // children. Gates the rebalance pass (so we don't keep re-mutating buffers).
    public struct RebalancedTag : IComponentData { }

    // Marker tag added once we've dumped a prefab's SubMesh ColorVariations to CSV.
    // Separate from RebalancedTag so the dump can run independently and works even
    // if rebalance already happened earlier in the session.
    public struct DumpedTag : IComponentData { }

    // Stored on each SubMesh (child) entity, recording the buffer length before any
    // custom-color appends. Lets the rebalance pass trim previously-appended customs
    // back off when the same SubMesh is reached via a different parent (a SubMesh can
    // be referenced by multiple parents — e.g. a shared mesh on car + trailer).
    public struct OriginalColorVariationCount : IComponentData
    {
        public int Value;
    }

    [UpdateAfter(typeof(PrefabInitializeSystem))]
    public partial class RealisticVehicleColorsSystem : GameSystemBase
    {
        // Parent civilian-vehicle prefabs that haven't been rebalanced yet.
        // ColorVariation is NOT here — that buffer lives on child SubMesh entities,
        // reached via the parent's SubMesh buffer.
        private EntityQuery m_RebalanceQuery;
        // Parent civilian-vehicle prefabs that haven't been dumped yet.
        private EntityQuery m_DumpQuery;
        // Civilian-vehicle prefabs that already carry RebalancedTag — used to bulk-strip
        // the tag when the user hits Apply settings, so the next OnUpdate's rebalance
        // pass picks them up again with fresh slider values.
        private EntityQuery m_StripTagQuery;
        private PrefabSystem m_PrefabSystem;

        // Set from the UI thread by Mod.RequestLiveRebalance(); read and cleared
        // on the next OnUpdate (PrefabUpdate phase, where structural ECS changes
        // are safe). Bool writes are atomic on x86/x64 — volatile is enough.
        private volatile bool m_NeedsLiveRefresh;
        public void RequestLiveRefresh() => m_NeedsLiveRefresh = true;

        // Snapshot of each SubMesh entity's original m_Probability values, captured
        // before Rebalance ever mutates the buffer. Lets the dump emit pristine
        // probabilities even if rebalance ran earlier in the session.
        private readonly Dictionary<Entity, byte[]> m_OrigProbabilities = new Dictionary<Entity, byte[]>();

        [Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            Mod.log.Info($"{nameof(RealisticVehicleColorsSystem)}.OnCreate");
            m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            var allCommon = new ComponentType[]
            {
                ComponentType.ReadOnly<PrefabData>(),
                ComponentType.ReadOnly<SubMesh>(),
                // CarData is on every road vehicle (CarPrefab/CarTrailerPrefab) and excludes
                // trains/ships/airplanes that may share other markers like CargoTransportVehicleData.
                ComponentType.ReadOnly<CarData>(),
            };
            var anyCivilian = new ComponentType[]
            {
                ComponentType.ReadOnly<PersonalCarData>(),
                ComponentType.ReadOnly<DeliveryTruckData>(),
                ComponentType.ReadOnly<CargoTransportVehicleData>(),
                ComponentType.ReadOnly<CarTrailerData>(),
            };
            m_RebalanceQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = allCommon,
                Any = anyCivilian,
                None = new ComponentType[] { ComponentType.ReadOnly<RebalancedTag>() },
            });
            m_DumpQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = allCommon,
                Any = anyCivilian,
                None = new ComponentType[] { ComponentType.ReadOnly<DumpedTag>() },
            });
            var allCommonWithTag = new ComponentType[allCommon.Length + 1];
            for (int i = 0; i < allCommon.Length; i++) allCommonWithTag[i] = allCommon[i];
            allCommonWithTag[allCommon.Length] = ComponentType.ReadOnly<RebalancedTag>();
            m_StripTagQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = allCommonWithTag,
                Any = anyCivilian,
            });
        }

        [Preserve]
        protected override void OnUpdate()
        {
            var settings = Mod.Settings;
            if (settings == null) return;

            var em = EntityManager;

            // Live refresh: user clicked Apply in Options. Strip RebalancedTag from
            // every civilian-vehicle prefab so the rebalance query below picks them
            // up again with the current slider values. Bulk remove is one structural
            // change for the whole archetype — much cheaper than per-entity.
            if (m_NeedsLiveRefresh)
            {
                m_NeedsLiveRefresh = false;
                int n = m_StripTagQuery.CalculateEntityCount();
                if (n > 0)
                {
                    em.RemoveComponent<RebalancedTag>(m_StripTagQuery);
                    Mod.log.Info($"Apply settings: cleared RebalancedTag from {n} civilian-vehicle prefabs.");
                }
                else
                {
                    Mod.log.Info("Apply settings: no rebalanced prefabs to refresh.");
                }
            }

            bool dump = settings.DumpColorVariations;
            bool enabled = settings.Enabled;

            int dumpQueryCount = m_DumpQuery.CalculateEntityCount();
            int rebalanceQueryCount = m_RebalanceQuery.CalculateEntityCount();

            // Pass 1: dump (independent from rebalance — runs over its own query).
            // Run dump BEFORE rebalance on this frame so any prefab that hasn't been
            // rebalanced yet is captured in its original state.
            if (dump && dumpQueryCount > 0)
            {
                try { RunDumpPass(em); }
                catch (Exception ex) { Mod.log.Error($"Dump pass failed: {ex}"); }
            }

            // Pass 2: rebalance — only when master switch is on.
            if (enabled && rebalanceQueryCount > 0)
            {
                try { RunRebalancePass(em, settings); }
                catch (Exception ex) { Mod.log.Error($"Rebalance pass failed: {ex}"); }
            }
        }

        private void RunDumpPass(EntityManager em)
        {
            var entities = m_DumpQuery.ToEntityArray(Allocator.Temp);
            int prefabCount = entities.Length;
            StreamWriter writer = null;
            int variationsWritten = 0;
            int submeshesDumped = 0;
            try
            {
                writer = OpenDumpWriter();
                for (int i = 0; i < prefabCount; i++)
                {
                    Entity prefab = entities[i];
                    bool isCar = em.HasComponent<PersonalCarData>(prefab);
                    bool isDelivery = em.HasComponent<DeliveryTruckData>(prefab);
                    bool isCargo = em.HasComponent<CargoTransportVehicleData>(prefab);
                    bool isBicycle = em.HasComponent<BicycleData>(prefab);
                    bool isTrailer = em.HasComponent<CarTrailerData>(prefab);
                    int prefabProbability = isCar
                        ? em.GetComponentData<PersonalCarData>(prefab).m_Probability
                        : -1;
                    string name = m_PrefabSystem.GetPrefabName(prefab);

                    var subMeshes = em.GetBuffer<SubMesh>(prefab, isReadOnly: true);
                    for (int s = 0; s < subMeshes.Length; s++)
                    {
                        Entity sub = subMeshes[s].m_SubMesh;
                        if (sub == Entity.Null) continue;
                        if (!em.HasBuffer<ColorVariation>(sub)) continue;
                        variationsWritten += DumpSubMesh(em, name, isCar, isDelivery, isCargo, isBicycle, isTrailer, prefabProbability, s, sub, writer);
                        submeshesDumped++;
                    }

                    em.AddComponent<DumpedTag>(prefab);
                }
            }
            finally
            {
                writer?.Dispose();
                entities.Dispose();
            }
            Mod.log.Info($"Dump pass: {prefabCount} prefabs, {submeshesDumped} submeshes, {variationsWritten} variations written");
        }

        private void RunRebalancePass(EntityManager em, Setting settings)
        {
            var entities = m_RebalanceQuery.ToEntityArray(Allocator.Temp);
            int prefabCount = entities.Length;
            int touchedSubMeshes = 0;
            try
            {
                for (int i = 0; i < prefabCount; i++)
                {
                    Entity prefab = entities[i];
                    bool isCar = em.HasComponent<PersonalCarData>(prefab);
                    bool isDelivery = em.HasComponent<DeliveryTruckData>(prefab);
                    bool isCargo = em.HasComponent<CargoTransportVehicleData>(prefab);
                    bool isTrailer = em.HasComponent<CarTrailerData>(prefab);
                    bool isTruck = isDelivery || isCargo;
                    // Trailers without PersonalCarData (semi-trailers, agri/forestry trailers) keep
                    // their stock colors — semi-trailer paint reflects the cargo company, not the
                    // driver, and agri trailers ship with a single variation anyway.
                    bool isStandaloneTrailer = isTrailer && !isCar;
                    bool skip = isStandaloneTrailer || (isTruck && !isCar && !settings.IncludeTrucks);

                    if (!skip)
                    {
                        var subMeshes = em.GetBuffer<SubMesh>(prefab, isReadOnly: true);
                        for (int s = 0; s < subMeshes.Length; s++)
                        {
                            Entity sub = subMeshes[s].m_SubMesh;
                            if (sub == Entity.Null) continue;
                            if (!em.HasBuffer<ColorVariation>(sub)) continue;
                            SnapshotOriginalProbabilities(em, sub);
                            // Camper trailer (CarTrailerData + PersonalCarData): restrict to
                            // white/brown buckets — campers are typically beige or white IRL.
                            bool isCamper = isCar && isTrailer;
                            Rebalance(em, sub, settings, isCamper);
                            touchedSubMeshes++;
                        }
                    }

                    em.AddComponent<RebalancedTag>(prefab);
                }
            }
            finally
            {
                entities.Dispose();
            }
            Mod.log.Info($"Rebalance pass: {prefabCount} prefabs, {touchedSubMeshes} submeshes touched");
        }

        private static StreamWriter OpenDumpWriter()
        {
            string dir = Path.Combine(Application.persistentDataPath, "ModsData", nameof(RealisticVehicleColors));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "color_dump.csv");
            Mod.log.Info($"Dump path: {path}");
            bool needHeader = !File.Exists(path);
            var writer = new StreamWriter(path, append: true, Encoding.UTF8);
            if (needHeader)
                writer.WriteLine("prefab,is_car,is_delivery_truck,is_cargo_truck,is_bicycle,is_trailer,prefab_probability,submesh_index,variation_index,group_id,probability,source_type,sync_flags,r,g,b,hex");
            return writer;
        }

        private int DumpSubMesh(EntityManager em, string prefabName, bool isCar, bool isDelivery, bool isCargo, bool isBicycle, bool isTrailer, int prefabProbability, int subMeshIndex, Entity sub, StreamWriter writer)
        {
            var buffer = em.GetBuffer<ColorVariation>(sub, isReadOnly: true);
            m_OrigProbabilities.TryGetValue(sub, out var origProbs);
            int written = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                var v = buffer[i];
                var c = v.m_ColorSet.m_Channel0;
                int r = Mathf.Clamp(Mathf.RoundToInt(c.r * 255f), 0, 255);
                int g = Mathf.Clamp(Mathf.RoundToInt(c.g * 255f), 0, 255);
                int b = Mathf.Clamp(Mathf.RoundToInt(c.b * 255f), 0, 255);
                string hex = $"{r:X2}{g:X2}{b:X2}";
                // Prefer the snapshot taken before any rebalance touched the buffer.
                byte prob = origProbs != null && i < origProbs.Length ? origProbs[i] : v.m_Probability;
                writer.WriteLine(
                    $"{Csv(prefabName)},{isCar},{isDelivery},{isCargo},{isBicycle},{isTrailer},{prefabProbability}," +
                    $"{subMeshIndex},{i},{v.m_GroupID.GetHashCode()},{prob}," +
                    $"{(int)v.m_ColorSourceType},{(int)v.m_SyncFlags}," +
                    $"{r},{g},{b},{hex}");
                written++;
            }
            return written;
        }

        private void SnapshotOriginalProbabilities(EntityManager em, Entity sub)
        {
            if (m_OrigProbabilities.ContainsKey(sub)) return;
            var buffer = em.GetBuffer<ColorVariation>(sub, isReadOnly: true);
            var snap = new byte[buffer.Length];
            for (int i = 0; i < buffer.Length; i++) snap[i] = buffer[i].m_Probability;
            m_OrigProbabilities[sub] = snap;
        }

        private static string Csv(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0) return s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        private static void Rebalance(EntityManager em, Entity sub, Setting settings, bool restrictToWhiteBrown = false)
        {
            var buffer = em.GetBuffer<ColorVariation>(sub);
            if (buffer.Length == 0) return;

            // Pre-failsafe: camper restriction would zero everything if both whitelist
            // sliders are 0 — drop the restriction so the prefab still gets picked.
            if (restrictToWhiteBrown
                && settings.GetBucketWeight(ColorBucket.White) == 0
                && settings.GetBucketWeight(ColorBucket.Brown) == 0)
            {
                restrictToWhiteBrown = false;
            }

            int originalCount;
            if (em.HasComponent<OriginalColorVariationCount>(sub))
            {
                originalCount = em.GetComponentData<OriginalColorVariationCount>(sub).Value;
                if (buffer.Length > originalCount)
                    buffer.RemoveRange(originalCount, buffer.Length - originalCount);
            }
            else
            {
                originalCount = buffer.Length;
                // AddComponentData triggers an archetype move on `sub`, which invalidates
                // the DynamicBuffer<ColorVariation> handle obtained above. Re-fetch after.
                em.AddComponentData(sub, new OriginalColorVariationCount { Value = originalCount });
                buffer = em.GetBuffer<ColorVariation>(sub);
            }

            var bucketCount = new NativeArray<int>(10, Allocator.Temp);
            var assigned = new NativeArray<int>(originalCount, Allocator.Temp);
            try
            {
                for (int i = 0; i < originalCount; i++)
                {
                    ColorBucket b = ColorClassifier.Classify(buffer[i].m_ColorSet.m_Channel0);
                    assigned[i] = (int)b;
                    bucketCount[(int)b]++;
                }

                for (int i = 0; i < originalCount; i++)
                {
                    var b = (ColorBucket)assigned[i];
                    int weight = settings.GetBucketWeight(b);
                    // Camper-style trailer: mask everything except white + brown.
                    if (restrictToWhiteBrown && b != ColorBucket.White && b != ColorBucket.Brown)
                        weight = 0;
                    int count = bucketCount[(int)b];
                    int per = count > 0 ? Mathf.Clamp(weight / count, 0, 100) : 0;

                    var entry = buffer[i];
                    entry.m_Probability = (byte)per;
                    buffer[i] = entry;
                }
            }
            finally
            {
                bucketCount.Dispose();
                assigned.Dispose();
            }

            AppendCustom(buffer, settings.Custom1Hex, settings.Custom1Probability);
            AppendCustom(buffer, settings.Custom2Hex, settings.Custom2Probability);
            AppendCustom(buffer, settings.Custom3Hex, settings.Custom3Probability);

            // Post-failsafe: MeshColorSystem's weighted reservoir sampler crashes if a
            // group's total probability is 0 (random.NextInt(0)). If every entry ended
            // up at 0, fall back to vanilla-uniform 100 across the whole buffer.
            int totalSum = 0;
            for (int i = 0; i < buffer.Length; i++) totalSum += buffer[i].m_Probability;
            if (totalSum == 0)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    var e = buffer[i];
                    e.m_Probability = 100;
                    buffer[i] = e;
                }
            }
        }

        private static void AppendCustom(DynamicBuffer<ColorVariation> buffer, string hex, int probability)
        {
            if (probability <= 0) return;
            if (!ColorClassifier.TryParseHex(hex, out var rgb)) return;

            ColorVariation template = buffer.Length > 0 ? buffer[0] : default;
            template.m_ColorSet = new ColorSet(rgb);
            template.m_Probability = (byte)Mathf.Clamp(probability, 0, 100);
            buffer.Add(template);
        }
    }
}
