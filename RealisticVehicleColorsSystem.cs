using Game;
using Game.Common;
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
        // the tag when the user hits Apply settings (master ON), so the next OnUpdate's
        // rebalance pass picks them up again with fresh slider values; also iterated by
        // the restore pass when Apply is pressed with master OFF.
        private EntityQuery m_StripTagQuery;
        // In-world vehicle instances. After rebalance / restore touches the prefab
        // buffer, we add BatchesUpdated to these so MeshColorSystem re-rolls each
        // instance's MeshColor from the updated ColorVariation buffer.
        private EntityQuery m_VehicleInstanceQuery;
        private PrefabSystem m_PrefabSystem;

        // Set from the UI thread by Mod.RequestLiveApply(); read and cleared on
        // the next OnUpdate (PrefabUpdate phase, where structural ECS changes are
        // safe). Bool writes are atomic on x86/x64 — volatile is enough.
        private volatile bool m_NeedsLiveApply;
        public void RequestLiveApply() => m_NeedsLiveApply = true;

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
            // Vehicle = Game.Vehicles.Vehicle, the "this entity is a vehicle instance" tag.
            // MeshColor lives on the instance and is what MeshColorSystem refreshes.
            m_VehicleInstanceQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<Game.Vehicles.Vehicle>(),
                    ComponentType.ReadOnly<MeshColor>(),
                    ComponentType.ReadOnly<PrefabRef>(),
                },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<Deleted>(),
                },
            });
        }

        [Preserve]
        protected override void OnUpdate()
        {
            var settings = Mod.Settings;
            if (settings == null) return;

            var em = EntityManager;

            bool liveApply = m_NeedsLiveApply;
            if (liveApply) m_NeedsLiveApply = false;
            bool dump = settings.DumpColorVariations;
            bool enabled = settings.Enabled;

            // Bulk remove = one structural change per archetype, much cheaper
            // than per-entity. Rebalance pass below picks the prefabs back up.
            if (liveApply && enabled)
            {
                int n = m_StripTagQuery.CalculateEntityCount();
                if (n > 0)
                {
                    em.RemoveComponent<RebalancedTag>(m_StripTagQuery);
                    Mod.log.Info($"Apply (master ON): cleared RebalancedTag from {n} civilian-vehicle prefabs.");
                }
                else
                {
                    Mod.log.Info("Apply (master ON): no rebalanced prefabs to refresh.");
                }
            }
            else if (liveApply)
            {
                try { RunRestorePass(em); }
                catch (Exception ex) { Mod.log.Error($"Restore pass failed: {ex}"); }
            }

            // Run dump BEFORE rebalance so any prefab that hasn't been rebalanced
            // yet is captured in its original state.
            if (dump && !m_DumpQuery.IsEmptyIgnoreFilter)
            {
                try { RunDumpPass(em); }
                catch (Exception ex) { Mod.log.Error($"Dump pass failed: {ex}"); }
            }

            if (enabled && m_RebalanceQuery.CalculateEntityCount() > 0)
            {
                try { RunRebalancePass(em, settings); }
                catch (Exception ex) { Mod.log.Error($"Rebalance pass failed: {ex}"); }
            }

            // MUST run after the prefab buffers above are at their final state.
            if (liveApply)
            {
                try { MarkInstancesDirty(em); }
                catch (Exception ex) { Mod.log.Error($"MarkInstancesDirty failed: {ex}"); }
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

        // Single source of truth for "is this a prefab the mod should rebalance?".
        // Drift between the rebalance pass and the dirty-mark pass would silently
        // de-sync prefab edits from instance re-rolls — keep both call-sites here.
        // `isCamper`: CarTrailerData + PersonalCarData → restrict to white/brown.
        private static bool ShouldRebalancePrefab(EntityManager em, Entity prefab, out bool isCamper)
        {
            isCamper = false;
            if (!em.HasComponent<CarData>(prefab)) return false;
            bool isCar = em.HasComponent<PersonalCarData>(prefab);
            bool isDelivery = em.HasComponent<DeliveryTruckData>(prefab);
            bool isCargo = em.HasComponent<CargoTransportVehicleData>(prefab);
            bool isTrailer = em.HasComponent<CarTrailerData>(prefab);
            if (!(isCar || isDelivery || isCargo || isTrailer)) return false;
            // Standalone trailers (semi-trailers, agri/forestry): semi-trailer paint
            // reflects the cargo company, not the driver, and agri trailers ship
            // with a single variation anyway.
            if (isTrailer && !isCar) return false;
            isCamper = isCar && isTrailer;
            return true;
        }

        private void RunRebalancePass(EntityManager em, Setting settings)
        {
            var entities = m_RebalanceQuery.ToEntityArray(Allocator.Temp);
            int prefabCount = entities.Length;
            int touchedSubMeshes = 0;
            int synthesizedEntries = 0;
            int submeshesWithSynth = 0;
            try
            {
                for (int i = 0; i < prefabCount; i++)
                {
                    Entity prefab = entities[i];
                    if (ShouldRebalancePrefab(em, prefab, out bool isCamper))
                    {
                        var subMeshes = em.GetBuffer<SubMesh>(prefab, isReadOnly: true);
                        for (int s = 0; s < subMeshes.Length; s++)
                        {
                            Entity sub = subMeshes[s].m_SubMesh;
                            if (sub == Entity.Null) continue;
                            if (!em.HasBuffer<ColorVariation>(sub)) continue;
                            SnapshotOriginalProbabilities(em, sub);
                            m_OrigProbabilities.TryGetValue(sub, out var origProbs);
                            int synth = Rebalance(em, sub, settings, origProbs, isCamper);
                            synthesizedEntries += synth;
                            if (synth > 0) submeshesWithSynth++;
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
            if (prefabCount > 0)
                Mod.log.Info($"Rebalance pass: {prefabCount} prefabs, {touchedSubMeshes} submeshes touched, " +
                             $"synthesized {synthesizedEntries} stand-in entries across {submeshesWithSynth} submeshes");
        }

        // Without this, in-world cars stay frozen on the previous custom mix
        // even after re-roll: MeshColor caches the picked variation per instance,
        // and re-rolling against a still-rebalanced ColorVariation buffer would
        // just pick from the same custom weights.
        private void RunRestorePass(EntityManager em)
        {
            var entities = m_StripTagQuery.ToEntityArray(Allocator.Temp);
            int prefabCount = entities.Length;
            int restoredSubMeshes = 0;
            try
            {
                for (int i = 0; i < prefabCount; i++)
                {
                    Entity prefab = entities[i];
                    var subMeshes = em.GetBuffer<SubMesh>(prefab, isReadOnly: true);
                    for (int s = 0; s < subMeshes.Length; s++)
                    {
                        Entity sub = subMeshes[s].m_SubMesh;
                        if (sub == Entity.Null) continue;
                        if (!em.HasBuffer<ColorVariation>(sub)) continue;
                        if (RestoreSubMesh(em, sub)) restoredSubMeshes++;
                    }
                    em.RemoveComponent<RebalancedTag>(prefab);
                }
            }
            finally
            {
                entities.Dispose();
            }
            Mod.log.Info($"Apply (master OFF): restored {prefabCount} prefabs, {restoredSubMeshes} submeshes.");
        }

        private bool RestoreSubMesh(EntityManager em, Entity sub)
        {
            var buffer = em.GetBuffer<ColorVariation>(sub);
            int origCount = em.HasComponent<OriginalColorVariationCount>(sub)
                ? em.GetComponentData<OriginalColorVariationCount>(sub).Value
                : buffer.Length;
            if (!m_OrigProbabilities.TryGetValue(sub, out var snap)) return false;
            RestoreFromSnapshot(buffer, origCount, snap);
            return true;
        }

        // Trim any custom-color appends past the stock layout, then copy the
        // snapshotted m_Probability values back over the surviving entries.
        // Shared between live restore (master OFF Apply) and the rebalance
        // failsafe (totalSum==0 — submesh has no entries in any user-enabled
        // bucket, so we replay vanilla probabilities instead of crashing the
        // weighted sampler).
        private static void RestoreFromSnapshot(DynamicBuffer<ColorVariation> buffer, int origCount, byte[] snap)
        {
            if (buffer.Length > origCount)
                buffer.RemoveRange(origCount, buffer.Length - origCount);
            int n = Math.Min(buffer.Length, snap.Length);
            for (int i = 0; i < n; i++)
            {
                var entry = buffer[i];
                entry.m_Probability = snap[i];
                buffer[i] = entry;
            }
        }

        // BatchesUpdated puts the instance on MeshColorSystem.m_UpdateQuery, which
        // re-runs SetMeshColorsJob and re-picks from the updated ColorVariation
        // buffer. Without it, instances stay on the MeshColor cached at spawn.
        private void MarkInstancesDirty(EntityManager em)
        {
            var entities = m_VehicleInstanceQuery.ToEntityArray(Allocator.Temp);
            var dirty = new NativeList<Entity>(entities.Length, Allocator.Temp);
            try
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    Entity inst = entities[i];
                    Entity prefab = em.GetComponentData<PrefabRef>(inst).m_Prefab;
                    if (prefab == Entity.Null) continue;
                    if (!ShouldRebalancePrefab(em, prefab, out _)) continue;
                    dirty.Add(inst);
                }
                if (dirty.Length > 0)
                    em.AddComponent<BatchesUpdated>(dirty.AsArray());
                Mod.log.Info($"Marked {dirty.Length}/{entities.Length} vehicle instances dirty.");
            }
            finally
            {
                entities.Dispose();
                dirty.Dispose();
            }
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

        // Returns how many synthetic stand-in entries were injected (0 unless
        // empty-bucket synthesis is on and this submesh was missing a weighted
        // bucket) — surfaced in the pass log so synthesis is observable.
        private static int Rebalance(EntityManager em, Entity sub, Setting settings, byte[] origProbs, bool restrictToWhiteBrown = false)
        {
            var buffer = em.GetBuffer<ColorVariation>(sub);
            if (buffer.Length == 0) return 0;

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

            // Only synthesize missing colors in custom-color mode: in default mode
            // every bucket is weighted, so synthesis would repaint every car in every
            // color and destroy the realistic distribution.
            bool synthesizeMissing = settings.SynthesizeMissingColors && settings.UseCustomColors;
            // Only allocated when synthesis is on — the common (off) path adds nothing.
            List<KeyValuePair<Color, int>> synthQueue = synthesizeMissing
                ? new List<KeyValuePair<Color, int>>()
                : null;

            const int BucketEnumCount = 10;
            var bucketCount = new NativeArray<int>(BucketEnumCount, Allocator.Temp);
            var assigned = new NativeArray<int>(originalCount, Allocator.Temp);
            // Per-bucket base share + leftover after integer division. Naive
            // weight/count truncates to 0 whenever weight < count (e.g. Blue
            // slider=8 with ~10 blue variations per submesh → every blue entry
            // gets 0 → bucket vanishes). Distribute the remainder across the
            // first `weight % count` entries so the bucket total stays ≈ weight.
            var bucketBase = new NativeArray<int>(BucketEnumCount, Allocator.Temp);
            var bucketRem = new NativeArray<int>(BucketEnumCount, Allocator.Temp);
            var bucketSeen = new NativeArray<int>(BucketEnumCount, Allocator.Temp);
            try
            {
                for (int i = 0; i < originalCount; i++)
                {
                    ColorBucket b = ColorClassifier.Classify(buffer[i].m_ColorSet.m_Channel0);
                    assigned[i] = (int)b;
                    bucketCount[(int)b]++;
                }

                // Camper restriction is unconditional. If the user zeroed both white and
                // brown sliders, force each to a minimum (uniform pick across white+brown
                // stock variations) so totalSum stays >0 and the failsafe doesn't restore
                // the full vanilla mix — that would let any color render and defeat the
                // restriction.
                bool forceMinWeight = restrictToWhiteBrown
                    && settings.GetBucketWeight(ColorBucket.White) == 0
                    && settings.GetBucketWeight(ColorBucket.Brown) == 0;

                for (int b = 0; b < BucketEnumCount; b++)
                {
                    var bucket = (ColorBucket)b;
                    int weight = settings.GetBucketWeight(bucket);
                    if (restrictToWhiteBrown && bucket != ColorBucket.White && bucket != ColorBucket.Brown)
                        weight = 0;
                    else if (forceMinWeight && (bucket == ColorBucket.White || bucket == ColorBucket.Brown))
                        weight = 1;
                    int count = bucketCount[b];
                    if (count > 0)
                    {
                        bucketBase[b] = weight / count;
                        bucketRem[b] = weight % count;
                    }
                    else if (synthesizeMissing && weight > 0
                             && ColorClassifier.TryRepresentativeColor(bucket, out var repr))
                    {
                        // No stock variation in this bucket, but the user weighted it
                        // above zero — queue a synthetic stand-in carrying the whole
                        // bucket weight so the slider has something to act on. Appended
                        // after the assignment loop below (Add may realloc the buffer).
                        synthQueue.Add(new KeyValuePair<Color, int>(repr, weight));
                    }
                }

                for (int i = 0; i < originalCount; i++)
                {
                    int b = assigned[i];
                    int idx = bucketSeen[b]++;
                    int per = bucketBase[b] + (idx < bucketRem[b] ? 1 : 0);

                    var entry = buffer[i];
                    entry.m_Probability = (byte)per;
                    buffer[i] = entry;
                }
            }
            finally
            {
                bucketCount.Dispose();
                assigned.Dispose();
                bucketBase.Dispose();
                bucketRem.Dispose();
                bucketSeen.Dispose();
            }

            // Inject synthetic stand-ins for buckets the prefab lacks (empty-bucket
            // synthesis). Done here, after the per-entry loop, since Add may realloc.
            if (synthQueue != null)
                for (int i = 0; i < synthQueue.Count; i++)
                    AppendColorEntry(buffer, synthQueue[i].Key, synthQueue[i].Value);

            // Custom slots contribute only when active (enabled + probability > 0 +
            // parseable hex) — same predicate the UI uses for its percent labels.
            if (settings.IsSlot1Active()) AppendCustom(buffer, settings.Custom1Hex, settings.Custom1Probability);
            if (settings.IsSlot2Active()) AppendCustom(buffer, settings.Custom2Hex, settings.Custom2Probability);
            if (settings.IsSlot3Active()) AppendCustom(buffer, settings.Custom3Hex, settings.Custom3Probability);

            // Post-failsafe: MeshColorSystem's weighted reservoir sampler crashes if a
            // group's total probability is 0 (random.NextInt(0)). If every entry ended
            // up at 0 — typically because this submesh has no stock variations in any
            // of the user's enabled buckets (e.g. a moped/accent submesh whose colors
            // all classify into buckets the user set to 0) — restore stock probabilities
            // so the submesh shows its vanilla mix instead of a uniform random pick
            // across off-bucket colors.
            int totalSum = 0;
            for (int i = 0; i < buffer.Length; i++) totalSum += buffer[i].m_Probability;
            if (totalSum == 0)
            {
                if (origProbs != null)
                {
                    RestoreFromSnapshot(buffer, originalCount, origProbs);
                }
                else
                {
                    // Snapshot missing (shouldn't happen — we always snapshot before
                    // rebalance). Last-resort uniform to avoid the NextInt(0) crash.
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        var e = buffer[i];
                        e.m_Probability = 100;
                        buffer[i] = e;
                    }
                }
            }

            return synthQueue?.Count ?? 0;
        }

        private static void AppendCustom(DynamicBuffer<ColorVariation> buffer, string hex, int probability)
        {
            if (probability <= 0) return;
            if (!ColorClassifier.TryParseHex(hex, out var rgb)) return;
            AppendColorEntry(buffer, rgb, probability);
        }

        // Append a new ColorVariation with the given RGB and probability, cloning the
        // source-type / group / sync flags from the first stock entry so the injected
        // color renders the same way the stock ones do (this is how custom colors have
        // worked since v1). Shared by custom slots and empty-bucket synthesis.
        private static void AppendColorEntry(DynamicBuffer<ColorVariation> buffer, Color rgb, int probability)
        {
            ColorVariation template = buffer.Length > 0 ? buffer[0] : default;
            template.m_ColorSet = new ColorSet(rgb);
            template.m_Probability = (byte)Mathf.Clamp(probability, 0, 100);
            buffer.Add(template);
        }
    }
}
