using HarmonyLib;
using RimWorld;
using Verse;

namespace RPEF
{
    public static partial class HarmonyPatches
    {
        public static void PatchSkillAptitude(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(SkillRecord), nameof(SkillRecord.Aptitude)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SkillRecord_Aptitude_Prefix)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SkillRecord_Aptitude_Postfix)));
        }

        // 바닐라는 aptitudeCached가 비었을때만 적성을 다시 계산하므로, 그 호출에서만 헤디프 보너스를 더해 캐시에 포함시킨다.
        private static void SkillRecord_Aptitude_Prefix(int? ___aptitudeCached, out bool __state)
        {
            __state = !___aptitudeCached.HasValue;
        }

        private static void SkillRecord_Aptitude_Postfix(SkillRecord __instance, ref int __result, ref int? ___aptitudeCached, bool __state)
        {
            if (!__state || !___aptitudeCached.HasValue) { return; }

            var pawn = __instance.Pawn;
            if (pawn?.health?.hediffSet == null) { return; }

            int bonus = 0;
            var hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; ++i)
            {
                if (hediffs[i].CurStage is HediffStageWithSkillAptitude stage)
                {
                    bonus += stage.AptitudeFor(__instance.def);
                }
            }

            if (bonus != 0)
            {
                ___aptitudeCached += bonus;
                __result += bonus;
            }
        }
    }
}
