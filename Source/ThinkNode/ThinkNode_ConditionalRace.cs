using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RPEF
{
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
