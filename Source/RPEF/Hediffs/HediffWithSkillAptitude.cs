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
}
