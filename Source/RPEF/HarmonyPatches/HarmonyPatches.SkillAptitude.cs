using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
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
