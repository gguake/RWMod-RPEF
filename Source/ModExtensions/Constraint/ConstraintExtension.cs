using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public static class ConstraintExtension
    {
        public static IEnumerable<Constraint> GetAllConstraintsOfDef(this Def def, ConstraintRuleFlag rule)
        {
            if (def == null) { yield break; }
            if (def.modExtensions != null)
            {
                for (int i = 0; i < def.modExtensions.Count; ++i)
                {
                    if (def.modExtensions[i] is Constraint constraint && (constraint.rule & rule) != ConstraintRuleFlag.None)
                    {
                        yield return constraint;
                    }
                }
            }
        }

        public static bool CheckAllConstraints(this Def def, Pawn pawn, out Constraint failedConstraint)
        {
            foreach (var constraint in GetAllConstraintsOfDef(def, ConstraintRuleFlag.OnApplyPawn))
            {
                if (!constraint.Check(pawn)) { failedConstraint = constraint; return false; }
            }

            foreach (var constraint in AppliedConstraintCache.Get(pawn))
            {
                if (!constraint.Check(def)) { failedConstraint = constraint; return false; }
            }

            failedConstraint = null;
            return true;
        }

        public static bool CheckAllConstraints(this Def def, ThingDef pawnDef, out Constraint failedConstraint)
        {
            foreach (var constraint in GetAllConstraintsOfDef(def, ConstraintRuleFlag.OnApplyPawn))
            {
                if (!constraint.Check(pawnDef)) { failedConstraint = constraint; return false; }
            }

            // Race
            foreach (var constraint in pawnDef.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
            {
                if (!constraint.Check(def)) { failedConstraint = constraint; return false; }
            }

            failedConstraint = null;
            return true;
        }
    }

}
