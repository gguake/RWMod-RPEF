using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class HediffStageWithSkillAptitude : HediffStage
    {
        public List<Aptitude> skillAptitudes;

        public int AptitudeFor(SkillDef skill)
        {
            if (skillAptitudes.NullOrEmpty()) { return 0; }

            int sum = 0;
            for (int i = 0; i < skillAptitudes.Count; ++i)
            {
                if (skillAptitudes[i].skill == skill)
                {
                    sum += skillAptitudes[i].level;
                }
            }
            return sum;
        }
    }

    public static partial class HarmonyPatches
    {
        public static void PatchSkillAptitude(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(SkillRecord), nameof(SkillRecord.Aptitude)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SkillRecord_Aptitude_Postfix)));
        }

        private static void SkillRecord_Aptitude_Postfix(SkillRecord __instance, ref int __result)
        {
            var pawn = __instance.Pawn;
            if (pawn?.health?.hediffSet == null) { return; }

            var hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; ++i)
            {
                if (hediffs[i].CurStage is HediffStageWithSkillAptitude stage)
                {
                    __result += stage.AptitudeFor(__instance.def);
                }
            }
        }
    }
}
