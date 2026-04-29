using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace RPEF
{
    public static partial class HarmonyPatches
    {
        private static void PatchRace(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new Type[] { typeof(PawnGenerationRequest) }),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnGenerator_GeneratePawn_Prefix)));

            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker), nameof(PawnRelationWorker.GenerationChance)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnRelationWorker_GenerationChance_Postfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Parent), nameof(PawnRelationWorker_Parent.GenerationChance)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnRelationWorker_GenerationChance_Postfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Lover), nameof(PawnRelationWorker_Lover.GenerationChance)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnRelationWorker_GenerationChance_Postfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Lover), nameof(PawnRelationWorker_Sibling.GenerationChance)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnRelationWorker_GenerationChance_Postfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Lover), nameof(PawnRelationWorker_Child.GenerationChance)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnRelationWorker_GenerationChance_Postfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Lover), nameof(PawnRelationWorker_Spouse.GenerationChance)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnRelationWorker_GenerationChance_Postfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Lover), nameof(PawnRelationWorker_Fiance.GenerationChance)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnRelationWorker_GenerationChance_Postfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Lover), nameof(PawnRelationWorker_ExLover.GenerationChance)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnRelationWorker_GenerationChance_Postfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnRelationWorker_Lover), nameof(PawnRelationWorker_ExSpouse.GenerationChance)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnRelationWorker_GenerationChance_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateGenes"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnGenerator_GenerateGenes_Prefix)));

            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GetBodyTypeFor)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnGenerator_GetBodyTypeFor_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(PawnBioAndNameGenerator), "GiveShuffledBioTo"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnBioAndNameGenerator_GiveShuffledBioTo_Transpiler)));
            harmony.Patch(
                original: AccessTools.Method(typeof(PawnBioAndNameGenerator), "TryGiveSolidBioTo"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnBioAndNameGenerator_TryGiveSolidBioTo_Transpiler)));

            harmony.Patch(
                original: AccessTools.Method(typeof(PawnBioAndNameGenerator), "GiveShuffledBioTo"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnBioAndNameGenerator_GiveShuffledBioTo_Transpiler)));
            harmony.Patch(
                original: AccessTools.Method(typeof(PawnBioAndNameGenerator), "TryGiveSolidBioTo"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnBioAndNameGenerator_TryGiveSolidBioTo_Transpiler)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Gizmo_GrowthTier), "GrowthTierTooltip"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(Gizmo_GrowthTier_GrowthTierTooltip_Transpiler)));
            harmony.Patch(
                original: AccessTools.Method(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.TryChildGrowthMoment)),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_AgeTracker_TryChildGrowthMoment_Transpiler)));

            harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), "GrowthPointsFactor"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_AgeTracker_GrowthPointsFactor_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Pawn), nameof(Pawn.Sterile)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_Sterile_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.RandomSelectionWeight)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(InteractionWorker_RomanceAttempt_RandomSelectionWeight_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.SuccessChance)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(InteractionWorker_RomanceAttempt_SuccessChance_Postfix)));

            #region 조각상 관련
            harmony.Patch(
                original: AccessTools.Method(typeof(CompStatue), "CreateSnapshotOfPawn_HookForMods"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompStatue_CreateSnapshotOfPawn_HookForMods_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(CompStatue), "InitFakePawn"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompStatue_InitFakePawn_Transpiler)));
            #endregion

            harmony.Patch(
                original: AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.AdjustXenotypeForFactionlessPawn)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnGenerator_AdjustXenotypeForFactionlessPawn_Prefix)));

            #region 유전자 가리기 옵션
            harmony.Patch(
                original: AccessTools.Method(typeof(ITab_Genes), nameof(ITab_Genes.CanShowGenesTab)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ITab_Genes_CanShowGenesTab_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(CharacterCardUtility), "DoTopStack"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CharacterCardUtility_DoTopStack_Transpiler)));
            #endregion
        }

        private static bool PawnGenerator_GeneratePawn_Prefix(ref PawnGenerationRequest request)
        {
            List<(PawnKindDef kindDef, int weight)> kindDefCandidates = null;

            var originalKindDef = request.KindDef;
            foreach (var kindDef in DefDatabase<PawnKindDef>.AllDefsListForReading)
            {
                if (kindDef.modExtensions != null)
                {
                    foreach (var extension in kindDef.modExtensions)
                    {
                        if (extension is PawnGeneratorKindHook hook &&
                            hook.ReplaceHookInfos != null &&
                            hook.ReplaceHookInfos.TryGetValue(originalKindDef, out var weight) &&
                            weight > 0f)
                        {
                            if (kindDefCandidates == null)
                            {
                                kindDefCandidates = new List<(PawnKindDef kindDef, int weight)>()
                                {
                                    (originalKindDef, 10000),
                                };
                            }

                            kindDefCandidates.Add((kindDef, weight));
                        }
                    }
                }
            }

            if (kindDefCandidates != null && kindDefCandidates.Count > 1)
            {
                var result = kindDefCandidates.RandomElementByWeight(v => v.weight).kindDef;
                request.KindDef = result;
            }

            var pawnGenHook = request.KindDef.race.GetModExtension<PawnGeneratorRaceHook>();
            if (pawnGenHook != null && pawnGenHook.forcedXenotype != null)
            {
                request.ForcedXenotype = pawnGenHook.forcedXenotype;
            }

            return true;
        }

        private static void PawnRelationWorker_GenerationChance_Postfix(ref float __result, PawnRelationDef ___def, Pawn generated)
        {
            var pawnGenHook = generated.def.GetModExtension<PawnGeneratorRaceHook>();
            if (pawnGenHook != null)
            {
                if (pawnGenHook.RelationChanceMultiplier.TryGetValue(___def, out var multiplier))
                {
                    __result *= multiplier;
                }
            }
        }

        private static bool PawnGenerator_GenerateGenes_Prefix(Pawn pawn)
        {
            if (pawn.genes != null)
            {
                var pawnGenHook = pawn.def.GetModExtension<PawnGeneratorRaceHook>();
                if (pawnGenHook != null)
                {
                    if (pawnGenHook.melaninGeneOverrides != null && pawnGenHook.melaninGeneOverrides.Count > 0)
                    {
                        var melaninGeneDef = pawnGenHook.melaninGeneOverrides
                            .Where(v => v.geneDef.endogeneCategory == EndogeneCategory.Melanin)
                            .RandomElementByWeight(v => v.weight).geneDef;

                        pawn.genes.AddGene(melaninGeneDef, xenogene: false);
                    }

                    if (pawnGenHook.hairColorGeneOverrides != null && pawnGenHook.hairColorGeneOverrides.Count > 0)
                    {
                        var hairGeneDef = pawnGenHook.hairColorGeneOverrides
                            .Where(v => v.geneDef.endogeneCategory == EndogeneCategory.HairColor)
                            .RandomElementByWeight(v => v.weight).geneDef;

                        pawn.genes.AddGene(hairGeneDef, xenogene: false);
                    }
                }
            }

            return true;
        }

        private static void PawnGenerator_GetBodyTypeFor_Postfix(ref BodyTypeDef __result, Pawn pawn)
        {
            var hook = pawn.def.GetModExtension<PawnGeneratorRaceHook>();
            if (hook == null) { return; }

            if (pawn.DevelopmentalStage.Adult())
            {
                switch (pawn.gender)
                {
                    case Gender.Male:
                        if (hook.fixedMaleBodyType != null)
                        {
                            __result = hook.fixedMaleBodyType;
                        }
                        break;

                    case Gender.Female:
                        if (hook.fixedFemaleBodyType != null)
                        {
                            __result = hook.fixedFemaleBodyType;
                        }
                        break;
                }
            }
        }

        private static float PawnBioAndNameGenerator_MinAdulthoodAge_Getter_Injection(float age, Pawn pawn)
        {
            var hook = pawn.def.GetModExtension<PawnGeneratorRaceHook>();
            if (hook != null && hook.minAgeForAdulthood >= 0f)
            {
                return hook.minAgeForAdulthood;
            }

            return age;
        }
        private static IEnumerable<CodeInstruction> PawnBioAndNameGenerator_GiveShuffledBioTo_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            var index = instructions.FindIndex(v => v.opcode == OpCodes.Ldc_R4 && v.OperandIs(20));
            if (index >= 0)
            {
                instructions.InsertRange(index + 1, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(PawnBioAndNameGenerator_MinAdulthoodAge_Getter_Injection))),
                });
            }
            else
            {
                Log.Error($"[RPEF] Failed to find injection point for PawnBioAndNameGenerator_GiveShuffledBioTo_Transpiler");
            }

            return instructions;
        }

        private static IEnumerable<CodeInstruction> PawnBioAndNameGenerator_TryGiveSolidBioTo_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            var index = instructions.FindIndex(v => v.opcode == OpCodes.Ldc_R4 && v.OperandIs(20));
            if (index >= 0)
            {
                instructions.InsertRange(index + 1, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(PawnBioAndNameGenerator_MinAdulthoodAge_Getter_Injection))),
                });
            }
            else
            {
                Log.Error($"[RPEF] Failed to find injection point for PawnBioAndNameGenerator_TryGiveSolidBioTo_Transpiler");
            }

            return instructions;
        }

        private static bool GrowthUtility_IsGrowthBirthday_Override(int age, Pawn pawn)
        {
            var extension = pawn.def.GetModExtension<RaceExtension>();
            if (extension != null && extension.growthMomentAgeOverride != null)
            {
                for (int i = 0; i < extension.growthMomentAgeOverride.Count; i++)
                {
                    if (age == extension.growthMomentAgeOverride[i])
                    {
                        return true;
                    }
                }

                return false;
            }

            return GrowthUtility.IsGrowthBirthday(age);
        }
        private static IEnumerable<CodeInstruction> Gizmo_GrowthTier_GrowthTierTooltip_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            var index = instructions.FindIndex(
                v =>
                v.opcode == OpCodes.Call &&
                v.OperandIs(AccessTools.Method(typeof(GrowthUtility), nameof(GrowthUtility.IsGrowthBirthday))));

            if (index >= 0)
            {
                instructions.InsertRange(index + 1, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Gizmo_GrowthTier), "child")),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(GrowthUtility_IsGrowthBirthday_Override))),
                });

                instructions.RemoveAt(index);
            }
            else
            {
                Log.Error($"[RPEF] Failed to find injection point for Gizmo_GrowthTier_GrowthTierTooltip_Transpiler");
            }

            return instructions;
        }

        private static IEnumerable<CodeInstruction> Pawn_AgeTracker_TryChildGrowthMoment_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            var index = instructions.FindIndex(
                v =>
                v.opcode == OpCodes.Call &&
                v.OperandIs(AccessTools.Method(typeof(GrowthUtility), nameof(GrowthUtility.IsGrowthBirthday))));

            if (index >= 0)
            {
                instructions.InsertRange(index + 1, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_AgeTracker), "pawn")),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(GrowthUtility_IsGrowthBirthday_Override))),
                });

                instructions.RemoveAt(index);
            }
            else
            {
                Log.Error($"[RPEF] Failed to find injection point for Pawn_AgeTracker_TryChildGrowthMoment_Transpiler");
            }

            return instructions;
        }

        private static bool Pawn_AgeTracker_GrowthPointsFactor_Prefix(ref float __result, Pawn ___pawn)
        {
            var extension = ___pawn.def.GetModExtension<RaceExtension>();
            if (extension != null && extension.growthPointFactorCurve != null)
            {
                __result = extension.growthPointFactorCurve.Evaluate(___pawn.ageTracker.AgeBiologicalYearsFloat);
                return false;
            }

            return true;
        }

        private static void Pawn_Sterile_Postfix(ref bool __result, Pawn __instance)
        {
            if (!__result)
            {
                var extension = __instance.def.GetModExtension<RaceExtension>();
                if (extension != null && extension.sterile)
                {
                    __result = true;
                }
            }
        }

        private static void InteractionWorker_RomanceAttempt_RandomSelectionWeight_Postfix(Pawn initiator, ref float __result)
        {
            if (__result > 0f)
            {
                var extension = initiator.def.GetModExtension<RaceExtension>();
                if (extension != null)
                {
                    __result *= extension.romanceFrequencyWeight;
                }
            }
        }

        private static void InteractionWorker_RomanceAttempt_SuccessChance_Postfix(Pawn initiator, Pawn recipient, ref float __result)
        {
            if (__result > 0f)
            {
                var initiatorExt = initiator.def.GetModExtension<RaceExtension>();
                if (initiatorExt != null)
                {
                    __result *= initiatorExt.romanceSuccessChanceMultiplierAsInitiator;
                }

                var recipientExt = recipient.def.GetModExtension<RaceExtension>();
                if (recipientExt != null)
                {
                    __result *= recipientExt.romanceSuccessChanceMultiplierAsRecipient;
                }
            }
        }

        public class CompStatueRPEFInfo : IExposable
        {
            public ThingDef thingDef;
            public PawnKindDef pawnKindDef;

            public void ExposeData()
            {
                Scribe_Defs.Look(ref thingDef, "thingDef");
                Scribe_Defs.Look(ref pawnKindDef, "pawnKindDef");
            }
        }

        private const string CompStatueDictKey = "_RPEF_StatueInfo";
        private static void CompStatue_CreateSnapshotOfPawn_HookForMods_Postfix(Pawn p, Dictionary<string, object> dictToStoreDataIn)
        {
            dictToStoreDataIn.Add(CompStatueDictKey, new CompStatueRPEFInfo()
            {
                thingDef = p.def,
                pawnKindDef = p.kindDef,
            });
        }

        private static IEnumerable<CodeInstruction> CompStatue_InitFakePawn_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilGenerator)
        {
            var instructions = codeInstructions.ToList();

            var thingDefInjectionIndex = instructions.FindIndex(
                v =>
                v.opcode == OpCodes.Ldsfld &&
                v.OperandIs(AccessTools.Field(typeof(ThingDefOf), nameof(ThingDefOf.Human))));

            if (thingDefInjectionIndex >= 0)
            {
                var injectionIndex = thingDefInjectionIndex + 1;

                var localObject = ilGenerator.DeclareLocal(typeof(object));
                var labelNullCheckJump = ilGenerator.DefineLabel();

                instructions[injectionIndex] = instructions[injectionIndex].WithLabels(labelNullCheckJump);
                instructions.InsertRange(injectionIndex, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CompStatue), "additionalSavedPawnDataForMods")),
                    new CodeInstruction(OpCodes.Brfalse_S, labelNullCheckJump),

                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CompStatue), "additionalSavedPawnDataForMods")),
                    new CodeInstruction(OpCodes.Ldstr, CompStatueDictKey),
                    new CodeInstruction(OpCodes.Ldloca_S, localObject.LocalIndex),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Dictionary<string, object>), "TryGetValue")),
                    new CodeInstruction(OpCodes.Brfalse_S, labelNullCheckJump),

                    new CodeInstruction(OpCodes.Ldloc_S, localObject.LocalIndex),
                    new CodeInstruction(OpCodes.Brfalse_S, labelNullCheckJump),

                    new CodeInstruction(OpCodes.Ldloc_S, localObject.LocalIndex),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CompStatueRPEFInfo), nameof(CompStatueRPEFInfo.thingDef))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(ThingDefOf), nameof(ThingDefOf.Human))),
                    new CodeInstruction(OpCodes.Beq_S, labelNullCheckJump),

                    new CodeInstruction(OpCodes.Pop),
                    new CodeInstruction(OpCodes.Ldloc_S, localObject.LocalIndex),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CompStatueRPEFInfo), nameof(CompStatueRPEFInfo.thingDef))),
                });
            }

            var pawnKindDefInjectionIndex = instructions.FindIndex(
                v =>
                v.opcode == OpCodes.Ldsfld &&
                v.OperandIs(AccessTools.Field(typeof(PawnKindDefOf), nameof(PawnKindDefOf.Colonist))));

            if (pawnKindDefInjectionIndex >= 0)
            {
                var injectionIndex = pawnKindDefInjectionIndex + 1;

                var localObject = ilGenerator.DeclareLocal(typeof(object));
                var labelNullCheckJump = ilGenerator.DefineLabel();

                instructions[injectionIndex] = instructions[injectionIndex].WithLabels(labelNullCheckJump);
                instructions.InsertRange(injectionIndex, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CompStatue), "additionalSavedPawnDataForMods")),
                    new CodeInstruction(OpCodes.Brfalse_S, labelNullCheckJump),

                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CompStatue), "additionalSavedPawnDataForMods")),
                    new CodeInstruction(OpCodes.Ldstr, CompStatueDictKey),
                    new CodeInstruction(OpCodes.Ldloca_S, localObject.LocalIndex),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Dictionary<string, object>), "TryGetValue")),
                    new CodeInstruction(OpCodes.Brfalse_S, labelNullCheckJump),

                    new CodeInstruction(OpCodes.Ldloc_S, localObject.LocalIndex),
                    new CodeInstruction(OpCodes.Brfalse_S, labelNullCheckJump),

                    new CodeInstruction(OpCodes.Ldloc_S, localObject.LocalIndex),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CompStatueRPEFInfo), nameof(CompStatueRPEFInfo.pawnKindDef))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(PawnKindDefOf), nameof(PawnKindDefOf.Colonist))),
                    new CodeInstruction(OpCodes.Beq_S, labelNullCheckJump),

                    new CodeInstruction(OpCodes.Pop),
                    new CodeInstruction(OpCodes.Ldloc_S, localObject.LocalIndex),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CompStatueRPEFInfo), nameof(CompStatueRPEFInfo.pawnKindDef))),
                });
            }

            return instructions;
        }

        private static bool PawnGenerator_AdjustXenotypeForFactionlessPawn_Prefix(Pawn pawn, ref PawnGenerationRequest request, ref XenotypeDef xenotype)
        {
            var pawnGenHook = request.KindDef.race.GetModExtension<PawnGeneratorRaceHook>();
            if (pawnGenHook != null && pawnGenHook.forcedXenotype != null && pawnGenHook.forcedXenotype == xenotype)
            {
                return false;
            }

            return true;
        }

        private static void ITab_Genes_CanShowGenesTab_Postfix(ref bool __result)
        {
            if (!__result)
                return;

            Thing selected = Find.Selector.SingleSelectedThing;
            Pawn pawn = selected as Pawn ?? (selected as Corpse)?.InnerPawn;

            if (pawn != null && pawn.def.GetModExtension<RaceExtension>()?.hideGenes == true)
            {
                __result = false;
            }
        }

        private static IEnumerable<CodeInstruction> CharacterCardUtility_DoTopStack_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = instructions.ToList();
            var propertyGetterGenesList = AccessTools.PropertyGetter(typeof(Pawn_GeneTracker), nameof(Pawn_GeneTracker.GenesListForReading));

            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];

                if (codes[i].Calls(propertyGetterGenesList))
                {
                    i++;
                    yield return codes[i];

                    if (i + 1 < codes.Count && (codes[i + 1].opcode == OpCodes.Brfalse || codes[i + 1].opcode == OpCodes.Brfalse_S))
                    {
                        var skipLabel = codes[i + 1].operand;

                        i++;
                        yield return codes[i];

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RaceExtension), nameof(RaceExtension.HideGenes)));
                        yield return new CodeInstruction(OpCodes.Brtrue, skipLabel);
                    }
                }
            }
        }
    }
}
