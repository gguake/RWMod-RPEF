using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
    /// <summary>
    /// 비행하는 Humanlike Pawn을 구현하기 위한 Extension
    /// 비행 가능 여부와 애니메이션은 생애 단계가 아니라 생물학적 나이로 판단
    /// (다른 모드가 생애 단계를 바꿔 넣어도 영향을 받지 않도록)
    /// </summary>
    public class HumanlikeFlyExtension : DefModExtension
    {
        /// <summary>
        /// 비행 가능한 최소 생물학적 나이
        /// </summary>
        public float minAge;

        /// <summary>
        /// 비행시 재생할 애니메이션
        /// </summary>
        public AnimationDef animationSouth;
        public AnimationDef animationNorth;
        public AnimationDef animationEast;
        public AnimationDef animationWest;

        /// <summary>
        /// Bodypart가 요구되는지
        /// </summary>
        public List<string> requireBodyParts;

        /// <summary>
        /// 소집시 항상 비행을 시도하는지
        /// </summary>
        public bool alwaysFlyIfDrafted;

        public AnimationDef GetAnimation(Rot4 rot)
        {
            if (rot == Rot4.South) { return animationSouth; }
            if (rot == Rot4.North) { return animationNorth; }
            if (rot == Rot4.East) { return animationEast; }
            return animationWest;
        }

        public bool CheckCanEverFly(Pawn pawn)
        {
            if (pawn.ageTracker == null || pawn.ageTracker.AgeBiologicalYearsFloat < minAge) { return false; }

            if (requireBodyParts != null)
            {
                for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; ++i)
                {
                    var hediff = pawn.health.hediffSet.hediffs[i];
                    if (hediff is Hediff_MissingPart hediffMissingPart && hediffMissingPart.Part != null)
                    {
                        for (int j = 0; j < requireBodyParts.Count; ++j)
                        {
                            if (requireBodyParts[j] == hediffMissingPart.Part.untranslatedCustomLabel)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
