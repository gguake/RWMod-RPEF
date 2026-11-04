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

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            // ThinkNode_Subtree가 트리를 복사할때 races가 빠지면 모든 종족을 통과시키게 된다.
            var obj = (ThinkNode_ConditionalRace)base.DeepCopy(resolve);
            obj.races = races;
            return obj;
        }

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
