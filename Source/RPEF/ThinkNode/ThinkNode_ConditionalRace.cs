using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RPEF
{
    /// <summary>
    /// 특정 ThingDef에서만 동작하는 ThinkNode를 추가하고자 할때 사용
    /// </summary>
    public class ThinkNode_ConditionalRace : ThinkNode_Conditional
    {
        public List<ThingDef> races;

        public override float GetPriority(Pawn pawn)
        {
            if (races != null && !races.Contains(pawn.def))
            {
                return 0f;
            }

            return base.GetPriority(pawn);
        }

        protected override bool Satisfied(Pawn pawn)
        {
            if (races != null && !races.Contains(pawn.def))
            {
                return false;
            }

            return true;
        }
    }
}
