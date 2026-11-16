using RimWorld;
using System.Linq;
using Verse;

namespace RPEF
{
    public class CompProperties_AbilityRequiresBodyPart : AbilityCompProperties
    {
        public BodyPartDef bodyPartDef;
        public int requiredCount;

        /// <summary>
        /// 사용 불가 사유의 번역 키. 인자는 {0}=폰, {1}=신체 부위 이름, {2}=모자란 개수. 없으면 영어 기본 문구를 쓴다.
        /// </summary>
        public string disabledReasonKey;

        public CompProperties_AbilityRequiresBodyPart()
        {
            compClass = typeof(CompAbility_RequiresBodyPart);
        }
    }

    public class CompAbility_RequiresBodyPart : AbilityComp
    {
        public CompProperties_AbilityRequiresBodyPart Props => (CompProperties_AbilityRequiresBodyPart)props;

        public override bool GizmoDisabled(out string reason)
        {
            var bodyPartCount = parent.pawn.health.hediffSet.GetNotMissingParts().Count(v => v.def == Props.bodyPartDef);
            if (bodyPartCount < Props.requiredCount)
            {
                var missingCount = Props.requiredCount - bodyPartCount;
                if (!Props.disabledReasonKey.NullOrEmpty())
                {
                    reason = Props.disabledReasonKey.Translate(parent.pawn, Props.bodyPartDef.LabelCap, missingCount);
                }
                else
                {
                    reason = $"{parent.pawn.LabelShort} lacks {missingCount} {Props.bodyPartDef.LabelCap} body parts";
                }

                return true;
            }

            reason = null;
            return false;
        }
    }
}
