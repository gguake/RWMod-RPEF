using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using static RimWorld.FoodUtility;

namespace RPEF
{
    public static partial class HarmonyPatches
    {
        public static void PatchRestrictions(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(PlayDataLoader), "ResetStaticDataPost"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PlayDataLoader_ResetStaticDataPost_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(Pawn_StoryTracker), "TryGetRandomHeadFromSet"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_StoryTracker_TryGetRandomHeadFromSet_Transpiler)));
            harmony.Patch(__Pawn_StoryTracker_TryGetRandomHeadFromSet_CanUseHeadType,
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_StoryTracker_TryGetRandomHeadFromSet_CanUseHeadType_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnStyleItemChooser), nameof(PawnStyleItemChooser.WantsToUseStyle)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnStyleItemChooser_WantsToUseStyle_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(ApparelProperties), nameof(ApparelProperties.PawnCanWear), new Type[] { typeof(Pawn), typeof(bool) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ApparelProperties_PawnCanWear_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), nameof(EquipmentUtility.CanEquip), new Type[] { typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(EquipmentUtility_CanEquip_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.XenotypesAvailableFor)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnGenerator_XenotypesAvailableFor_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.FillBackstorySlotShuffled)),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnBioAndNameGenerator_FillBackstorySlotShuffled_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) }),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_HealthTracker_AddHediff_Prefix)));

            harmony.Patch(AccessTools.Method(typeof(HediffGiverUtility), nameof(HediffGiverUtility.TryApply)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HediffGiverUtility_TryApply_Prefix)));

            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemoryFast), new Type[] { typeof(ThoughtDef), typeof(Precept) }),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(MemoryThoughtHandler_TryGainMemoryFast_Prefix_1)));

            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemoryFast), new Type[] { typeof(ThoughtDef), typeof(int), typeof(Precept) }),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(MemoryThoughtHandler_TryGainMemoryFast_Prefix_2)));

            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new Type[] { typeof(Thought_Memory), typeof(Pawn) }),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(MemoryThoughtHandler_TryGainMemory_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Bill), nameof(Bill.PawnAllowedToStartAnew)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Bill_PawnAllowedToStartAnew_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GenerateTraitsFor)),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnGenerator_GenerateTraitsFor_Transpiler)));

            harmony.Patch(
                original: AccessTools.Method(typeof(TraitSet), nameof(TraitSet.GainTrait)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(TraitSet_GainTrait_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(FoodUtility), nameof(FoodUtility.ThoughtsFromIngesting)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(FoodUtility_ThoughtsFromIngesting_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(CharacterCardUtility), "SetupGenerationRequest"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CharacterCardUtility_SetupGenerationRequest_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(CharacterCardUtility), "LifestageAndXenotypeOptions"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CharacterCardUtility_LifestageAndXenotypeOptions_Transpiler)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Pawn_GeneTracker), "Notify_GenesChanged"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_GeneTracker_Notify_GenesChanged_Postfix)));
        }

        public static void LazyPatchRestrictions(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(PawnApparelGenerator), "CanUsePair"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnApparelGenerator_CanUsePair_Postfix)));

            {
                foreach (var subClass in typeof(PawnApparelGenerator).GetNestedType("PossibleApparelSet", BindingFlags.NonPublic).GetNestedTypes(BindingFlags.NonPublic))
                {
                    foreach (var method in subClass.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).AsParallel())
                    {
                        var name = method.Name;
                        if (name.Contains("HatPairValidator"))
                        {
                            harmony.Patch(original: method, postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnApparelGenerator_PossibleApparelSet_Validator_Postfix)));
                        }
                        else if (name.Contains("ParkaPairValidator"))
                        {
                            harmony.Patch(original: method, postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnApparelGenerator_PossibleApparelSet_Validator_Postfix)));
                        }
                    }
                }
            }

            Log.Message($"[RaceExt] Restriction Patch Succeeded");
        }

        private static void PlayDataLoader_ResetStaticDataPost_Postfix()
        {
            ConstraintExtension.ClearCache();
        }

        #region EarlyPatch
        private static MethodInfo __Pawn_StoryTracker_TryGetRandomHeadFromSet_CanUseHeadType;
        private static IEnumerable<CodeInstruction> Pawn_StoryTracker_TryGetRandomHeadFromSet_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            foreach (var inst in codeInstructions)
            {
                if (inst.opcode == OpCodes.Ldftn)
                {
                    __Pawn_StoryTracker_TryGetRandomHeadFromSet_CanUseHeadType = inst.operand as MethodInfo;
                    break;
                }
            }

            return codeInstructions;
        }

        private static void Pawn_StoryTracker_TryGetRandomHeadFromSet_CanUseHeadType_Postfix(ref bool __result, HeadTypeDef head, Pawn ___pawn)
        {
            if (__result)
            {
                if (head == null || ___pawn == null) { return; }

                __result = head.CheckAllConstraints(___pawn, out _);
            }
        }

        private static void PawnStyleItemChooser_WantsToUseStyle_Postfix(ref bool __result, Pawn pawn, StyleItemDef styleItemDef)
        {
            if (__result)
            {
                __result = styleItemDef.CheckAllConstraints(pawn, out _);
            }
        }

        private static Dictionary<ApparelProperties, ThingDef> _apparelPropertiesThingDefCache = new Dictionary<ApparelProperties, ThingDef>();
        private static void ApparelProperties_PawnCanWear_Postfix(ref bool __result, ApparelProperties __instance, Pawn pawn)
        {
            if (__result)
            {
                if (!_apparelPropertiesThingDefCache.TryGetValue(__instance, out var thingDef))
                {
                    foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        if (def.apparel == __instance)
                        {
                            thingDef = def;
                            _apparelPropertiesThingDefCache.Add(__instance, def);
                            break;
                        }
                    }
                }

                if (thingDef != null && !thingDef.CheckAllConstraints(pawn, out _))
                {
                    __result = false;
                    return;
                }
            }
        }

        private static void EquipmentUtility_CanEquip_Postfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason)
        {
            if (__result)
            {
                if (thing == null || pawn == null) { return; }

                if (!thing.def.CheckAllConstraints(pawn, out var failedConstraint))
                {
                    if (failedConstraint.failReason?.Length > 0)
                    {
                        cantReason = failedConstraint.failReason;
                    }

                    __result = false;
                }
            }
        }

        private static void PawnGenerator_XenotypesAvailableFor_Postfix(ref Dictionary<XenotypeDef, float> __result, PawnKindDef kind)
        {
            var keys = __result.Keys.ToList();
            foreach (var xenotype in keys)
            {
                if (!xenotype.CheckAllConstraints(kind.race, out _))
                {
                    __result.Remove(xenotype);
                    continue;
                }
            }
        }

        private static List<BackstoryDef> RemoveNotCompatibleBackstories(List<BackstoryDef> backstoryDefs, Pawn pawn)
        {
            backstoryDefs.RemoveAll(def => !def.CheckAllConstraints(pawn, out _));
            return backstoryDefs;
        }

        private static IEnumerable<CodeInstruction> PawnBioAndNameGenerator_FillBackstorySlotShuffled_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();
            var index = instructions.FindLastIndex(
                v => v.opcode == OpCodes.Call &&
                v.OperandIs(AccessTools.Method(typeof(GenCollection), nameof(GenCollection.TakeRandom)).MakeGenericMethod(typeof(BackstoryDef))));

            if (index < 0) { throw new NotImplementedException(); }

            var injectionIndex = index - 1;
            instructions.InsertRange(injectionIndex, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(RemoveNotCompatibleBackstories))),
            });

            return instructions;
        }

        private static bool Pawn_HealthTracker_AddHediff_Prefix(Pawn ___pawn, Hediff hediff)
        {
            if (hediff != null)
            {
                return hediff.def.CheckAllConstraints(___pawn, out _);
            }

            return true;
        }

        private static bool HediffGiverUtility_TryApply_Prefix(ref bool __result, Pawn pawn, HediffDef hediff)
        {
            if (pawn != null && hediff != null)
            {
                if (!hediff.CheckAllConstraints(pawn, out _))
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }

        private static bool MemoryThoughtHandler_TryGainMemoryFast_Prefix_1(Pawn ___pawn, ref ThoughtDef mem)
        {
            if (!mem.CheckAllConstraints(___pawn, out _))
            {
                return false;
            }

            var extension = ___pawn.def.GetModExtension<RaceExtension>();
            if (extension != null && extension.ThoughtReplacer.TryGetValue(mem, out var replacedThoughtDef))
            {
                if (replacedThoughtDef == null) { return false; }

                mem = replacedThoughtDef;
            }

            return true;
        }

        private static bool MemoryThoughtHandler_TryGainMemoryFast_Prefix_2(Pawn ___pawn, ref ThoughtDef mem)
        {
            if (!mem.CheckAllConstraints(___pawn, out _))
            {
                return false;
            }

            var extension = ___pawn.def.GetModExtension<RaceExtension>();
            if (extension != null && extension.ThoughtReplacer.TryGetValue(mem, out var replacedThoughtDef))
            {
                if (replacedThoughtDef == null) { return false; }

                mem = replacedThoughtDef;
            }

            return true;
        }

        private static bool MemoryThoughtHandler_TryGainMemory_Prefix(MemoryThoughtHandler __instance, Pawn ___pawn, ref Thought_Memory newThought, Pawn otherPawn)
        {
            if (!newThought.def.CheckAllConstraints(___pawn, out _))
            {
                return false;
            }

            var pawnRaceExtension = ___pawn.def.GetModExtension<RaceExtension>();
            if (pawnRaceExtension != null)
            {
                if (pawnRaceExtension.ThoughtReplacer.TryGetValue(newThought.def, out var replacedThoughtDef))
                {
                    if (replacedThoughtDef == null) { return false; }

                    var replacedThought = ThoughtMaker.MakeThought(replacedThoughtDef, newThought.sourcePrecept);
                    replacedThought.SetForcedStage(newThought.CurStageIndex);

                    __instance.TryGainMemory(replacedThought, otherPawn);
                    return false;
                }
            }


            var otherPawnRaceExtension = otherPawn?.def.GetModExtension<RaceExtension>();
            if (otherPawnRaceExtension != null)
            {
                if (otherPawnRaceExtension.ThoughtReplacerInverse.TryGetValue(newThought.def, out var replacedThoughtDef))
                {
                    if (replacedThoughtDef == null) { return false; }

                    var replacedThought = ThoughtMaker.MakeThought(replacedThoughtDef, newThought.sourcePrecept);
                    replacedThought.SetForcedStage(newThought.CurStageIndex);

                    __instance.TryGainMemory(replacedThought, otherPawn);
                    return false;
                }
            }

            return true;
        }

        private static void Bill_PawnAllowedToStartAnew_Postfix(ref bool __result, Bill __instance, Pawn p, RecipeDef ___recipe)
        {
            if (__result && p != null && ___recipe != null)
            {
                if (!___recipe.CheckAllConstraints(p, out var constraint))
                {
                    __result = false;
                }
            }
        }

        private static IEnumerable<TraitDef> FilterTraitCandidates(IEnumerable<TraitDef> traits, Pawn pawn)
        {
            return traits.Where(trait => trait.CheckAllConstraints(pawn, out _));
        }
        private static IEnumerable<CodeInstruction> PawnGenerator_GenerateTraitsFor_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            var index = instructions.FindIndex(
                v =>
                v.opcode == OpCodes.Call &&
                v.OperandIs(AccessTools.PropertyGetter(typeof(DefDatabase<TraitDef>), "AllDefsListForReading")));

            if (index >= 0)
            {
                instructions.InsertRange(index + 1, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.FilterTraitCandidates))),
                });
            }
            else
            {
                Log.Error($"[RPEF] Failed to find injection point for PawnGenerator_GenerateTraitsFor_Transpiler");
            }


            return instructions;
        }

        private static bool TraitSet_GainTrait_Prefix(Pawn ___pawn, Trait trait)
        {
            if (!trait.def.CheckAllConstraints(___pawn, out _))
            {
                return false;
            }

            return true;
        }

        private static void FoodUtility_ThoughtsFromIngesting_Postfix(ref List<ThoughtFromIngesting> __result, Pawn ingester)
        {
            for (int i = 0; i < __result.Count; ++i)
            {
                var thought = __result[i].thought;
                if (!thought.CheckAllConstraints(ingester, out _))
                {
                    __result.RemoveAt(i);
                    i--;
                }
            }
        }

        private static bool CharacterCardUtility_SetupGenerationRequest_Prefix(int index, List<XenotypeDef> allowedXenotypes)
        {
            if (allowedXenotypes != null && allowedXenotypes.Count > 0)
            {
                allowedXenotypes.RemoveAll(xenotype => !xenotype.CheckAllConstraints(StartingPawnUtility.GetGenerationRequest(index).KindDef.race, out _));

                if (allowedXenotypes.Empty())
                {
                    allowedXenotypes.Add(XenotypeDefOf.Baseliner);
                }
            }

            return true;
        }

        private static IEnumerable<XenotypeDef> CharacterCardUtility_LifestageAndXenotypeOptions_Injection(IEnumerable<XenotypeDef> xenotypeDefs, Pawn pawn)
        {
            foreach (var xenotypeDef in xenotypeDefs)
            {
                if (pawn != null)
                {
                    if (!xenotypeDef.CheckAllConstraints(pawn.kindDef.race, out _))
                    {
                        continue;
                    }
                }

                yield return xenotypeDef;
            }
        }
        private static IEnumerable<CodeInstruction> CharacterCardUtility_LifestageAndXenotypeOptions_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();

            var index = codeInstructions.FindIndex(v => v.opcode == OpCodes.Call && v.OperandIs(AccessTools.PropertyGetter(typeof(DefDatabase<XenotypeDef>), "AllDefs"))) + 1;
            codeInstructions.InsertRange(index, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.CharacterCardUtility_LifestageAndXenotypeOptions_Injection)))
            });

            return codeInstructions;
        }

        private static void Pawn_GeneTracker_Notify_GenesChanged_Postfix(Pawn ___pawn)
        {
            ___pawn.apparel?.DropAllOrMoveAllToInventory((Apparel apparel) => !apparel.PawnCanWear(___pawn));
        }
        #endregion

        #region LazyPatch
        private static void PawnApparelGenerator_CanUsePair_Postfix(ref bool __result, ThingStuffPair pair, Pawn pawn)
        {
            if (__result)
            {
                if (!pair.thing.CheckAllConstraints(pawn, out _))
                {
                    __result = false;
                }
            }
        }

        private static void PawnApparelGenerator_PossibleApparelSet_Validator_Postfix(ref bool __result, Pawn ___pawn, ThingStuffPair pa)
        {
            if (__result)
            {
                if (!pa.thing.CheckAllConstraints(___pawn, out _))
                {
                    __result = false;
                }
            }
        }
        #endregion
    }
}
