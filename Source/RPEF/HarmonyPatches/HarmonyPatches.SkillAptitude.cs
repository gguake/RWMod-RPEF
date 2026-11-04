using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class HediffWithSkillAptitude : HediffWithComps
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            if (CurStage is HediffStageWithSkillAptitude)
            {
                pawn.skills?.DirtyAptitudes();
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();

            if (CurStage is HediffStageWithSkillAptitude)
            {
                pawn.skills?.DirtyAptitudes();
            }
        }

        protected override void OnStageIndexChanged(int stageIndex)
        {
            base.OnStageIndexChanged(stageIndex);

            // 적성 단계에서 일반 단계로 바뀌는 경우에도 캐시에 남은 보너스를 지워야 하므로 조건 없이 갱신
            pawn?.skills?.DirtyAptitudes();
        }
    }

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
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SkillRecord_Aptitude_Prefix)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SkillRecord_Aptitude_Postfix)));
        }

        /// <summary>
        /// HediffWithSkillAptitude만 단계 변경시 적성 캐시를 갱신하므로 다른 hediffClass와 함께 쓰면 보너스가 갱신되지 않는다.
        /// </summary>
        private static void ValidateSkillAptitudeHediffs()
        {
            foreach (var def in DefDatabase<HediffDef>.AllDefsListForReading)
            {
                if (def.stages == null || typeof(HediffWithSkillAptitude).IsAssignableFrom(def.hediffClass)) { continue; }

                for (int i = 0; i < def.stages.Count; ++i)
                {
                    if (def.stages[i] is HediffStageWithSkillAptitude)
                    {
                        Log.Error($"[RPEF] {def.defName} uses HediffStageWithSkillAptitude but its hediffClass is not RPEF.HediffWithSkillAptitude.");
                        break;
                    }
                }
            }
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
