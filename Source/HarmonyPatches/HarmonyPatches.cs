using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RPEF
{
    public static class HarmonyPatches
    {
        public static void Patch()
        {
            var harmony = new Harmony("rimworld.gguake.rpef");

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
                    postfix: new HarmonyMethod(typeof(RestrictionPatches), nameof(PawnGenerator_GetBodyTypeFor_Postfix)));

                RestrictionPatches.Patch(harmony);

                harmony.PatchAll();

                Log.Message($"[RaceExt] Patch Completed");
            }
            catch (Exception e)
            {
                Log.Error("[RaceExt] Some error occured in patch process..");
                throw;
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

    }
}
