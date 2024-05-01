using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Xml;
using Verse;
using Verse.AI;

namespace RPEF
{
    public static class HarmonyPatches
    {
        private static Harmony harmony;

        public static void Patch()
        {
            harmony = new Harmony("rimworld.gguake.rpef");
            try
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
                    original: AccessTools.Method(typeof(ApparelGraphicRecordGetter), nameof(ApparelGraphicRecordGetter.TryGetGraphicApparel)),
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ApparelGraphicRecordGetter_TryGetGraphicApparel_Prefix)));

                harmony.Patch(
                    original: AccessTools.Method(typeof(BillUtility), "MakeNewBill"),
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(BillUtility_MakeNewBill_Prefix)));

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

                harmony.Patch(
                    original: AccessTools.PropertyGetter(typeof(Settlement), nameof(Settlement.MapGeneratorDef)),
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Settlement_MapGeneratorDef_getter_Postfix)));

                harmony.Patch(
                    original: AccessTools.PropertyGetter(typeof(Dialog_ChooseNewWanderers), "DefaultStartingPawnRequest"),
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Dialog_ChooseNewWanderers_DefaultStartingPawnRequest_getter_Postfix)));

                harmony.Patch(
                    original: AccessTools.Method(typeof(XmlInheritance), "ResolveXmlNodesRecursively"),
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(XmlInheritance_ResolveXmlNodesRecursively_Postfix)));

                RestrictionPatches.Patch(harmony);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                Log.Error("[RaceExt] Some error occured in patch process..");
                throw;
            }
        }

        public static void LazyPatch()
        {
            try
            {
                RestrictionPatches.LazyPatch(harmony);

                harmony.PatchAll();

                Log.Message($"[RaceExt] Patch Completed");
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                Log.Error("[RaceExt] Some error occured in lazy patch process..");
            }
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

        private static bool ApparelGraphicRecordGetter_TryGetGraphicApparel_Prefix(Apparel apparel, ref BodyTypeDef bodyType)
        {
            var hook = apparel.def.GetModExtension<ApparelGraphicHook>();
            if (hook == null) { return true; }

            if (hook.bodyTypeGraphicOverride != null)
            {
                foreach (var v in hook.bodyTypeGraphicOverride)
                {
                    if (v.from == bodyType)
                    {
                        bodyType = v.to;
                        return true;
                    }
                }
            }

            if (hook.defaultBodyTypeGraphicOverride != null)
            {
                bodyType = hook.defaultBodyTypeGraphicOverride;
            }

            return true;
        }


        private static bool BillUtility_MakeNewBill_Prefix(ref Bill __result, RecipeDef recipe, Precept_ThingStyle precept)
        {
            var extension = recipe.GetModExtension<RecipeBillHook>();
            if (extension == null) { return true; }

            if (extension.billOverrideType != null)
            {
                if (!typeof(Bill).IsAssignableFrom(extension.billOverrideType))
                {
                    Log.Error($"invalid billtype for recipe mod extension");
                    return true;
                }

                var bill = Activator.CreateInstance(extension.billOverrideType, new object[] { recipe, precept }) as Bill;
                if (bill != null)
                {
                    __result = bill;
                    return false;
                }
            }

            return true;
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

        private static void Settlement_MapGeneratorDef_getter_Postfix(ref MapGeneratorDef __result, Settlement __instance)
        {
            if (__instance.Faction != null)
            {
                var extension = __instance.Faction.def.GetModExtension<FactionSettlementHook>();
                if (extension != null && extension.mapGeneratorDef != null)
                {
                    __result = extension.mapGeneratorDef;
                }
            }
        }

        private static void Dialog_ChooseNewWanderers_DefaultStartingPawnRequest_getter_Postfix(ref PawnGenerationRequest __result)
        {
            var extension = Faction.OfPlayer.def.GetModExtension<FactionContinuePawnKindHook>();
            if (extension != null && extension.continueMemberKindOverride != null)
            {
                __result.KindDef = extension.continueMemberKindOverride;
            }
        }

        private static void XmlInheritance_ResolveXmlNodesRecursively_Postfix(object node)
        {
            var type = node.GetType();
            var xmlNode = type.GetField("xmlNode").GetValue(node) as XmlNode;
            var resolvedXmlNode = type.GetField("resolvedXmlNode").GetValue(node) as XmlNode;

            if (xmlNode.Name.EndsWith("Def")) { return; }

            Log.Message($"{xmlNode.Name} {resolvedXmlNode.Name}");
        }
    }
}
