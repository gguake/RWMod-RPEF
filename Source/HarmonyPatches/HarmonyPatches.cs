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
            List<(PawnKindDef kindDef, float weight)> kindDefCandidates = null;

            var originalKindDef = request.KindDef;
            foreach (var kindDef in DefDatabase<PawnKindDef>.AllDefsListForReading)
            {
                if (kindDef.modExtensions != null)
                {
                    foreach (var extension in kindDef.modExtensions)
                    {
                        if (extension is PawnKindExtension pawnKindReplaceExtension && pawnKindReplaceExtension.pawnKindReplacer != null)
                        {
                            foreach (var pawnKindOverride in pawnKindReplaceExtension.pawnKindReplacer)
                            {
                                if (pawnKindOverride.targetKindDef == originalKindDef && pawnKindOverride.weight > 0f)
                                {
                                    if (kindDefCandidates == null)
                                    {
                                        kindDefCandidates = new List<(PawnKindDef, float)>()
                                    {
                                        (originalKindDef, 1f)
                                    };
                                    }

                                    kindDefCandidates.Add((kindDef, pawnKindOverride.weight));
                                }
                            }
                        }
                    }
                }
            }

            if (kindDefCandidates != null)
            {
                var result = kindDefCandidates.RandomElementByWeight(v => v.weight).kindDef;
                request.KindDef = result;
            }

            return true;
        }

        private static void PawnRelationWorker_GenerationChance_Postfix(ref float __result, PawnRelationDef ___def, Pawn generated)
        {
            var raceExtension = generated.def.GetModExtension<RaceExtension>();
            if (raceExtension != null)
            {
                if (raceExtension.RelationChanceMultiplier.TryGetValue(___def, out var multiplier))
                {
                    __result *= multiplier;
                }
            }
        }

        private static bool PawnGenerator_GenerateGenes_Prefix(Pawn pawn)
        {
            if (pawn.genes != null)
            {
                var raceExtension = pawn.def.GetModExtension<RaceExtension>();
                if (raceExtension != null)
                {
                    if (raceExtension.melaninGeneOverrides != null && raceExtension.melaninGeneOverrides.Count > 0)
                    {
                        var melaninGeneDef = raceExtension.melaninGeneOverrides
                            .Where(v => v.geneDef.endogeneCategory == EndogeneCategory.Melanin)
                            .RandomElementByWeight(v => v.weight).geneDef;

                        pawn.genes.AddGene(melaninGeneDef, xenogene: false);
                    }

                    if (raceExtension.hairColorGeneOverrides != null && raceExtension.hairColorGeneOverrides.Count > 0)
                    {
                        var hairGeneDef = raceExtension.hairColorGeneOverrides
                            .Where(v => v.geneDef.endogeneCategory == EndogeneCategory.HairColor)
                            .RandomElementByWeight(v => v.weight).geneDef;

                        pawn.genes.AddGene(hairGeneDef, xenogene: false);
                    }
                }
            }

            return true;
        }
    }
}
