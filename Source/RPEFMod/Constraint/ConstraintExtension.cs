using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RPEF
{
    public static class ConstraintExtension
    {
        public static IEnumerable<Constraint> GetAllConstraintsOfDef(this Def def, ConstraintRuleFlag rule)
        {
            if (def == null) { yield break; }

            var extension = def.GetModExtension<ConstraintModExtension>();
            if (extension != null)
            {
                var constraints = extension.def.constraints;
                for (int i = 0; i < constraints.Count; ++i)
                {
                    if ((constraints[i].rule & rule) != ConstraintRuleFlag.None)
                    {
                        yield return constraints[i];
                    }
                }
            }
        }

        public static bool CheckAllConstraints(this Def def, Pawn pawn, out Constraint failedConstraint)
        {
            foreach (var constraint in GetAllConstraintsOfDef(def, ConstraintRuleFlag.OnApplyPawn))
            {
                if (!constraint.Check(pawn))
                {
                    failedConstraint = constraint;
                    if (failedConstraint.failReason != null)
                    {
                        JobFailReason.Is(failedConstraint.failReason);
                    }

                    return false;
                }
            }

            foreach (var constraint in AppliedConstraintCache.Get(pawn))
            {
                if (!constraint.Check(def)) 
                {
                    failedConstraint = constraint;
                    if (failedConstraint.failReason != null)
                    {
                        JobFailReason.Is(failedConstraint.failReason);
                    }

                    return false;
                }
            }

            failedConstraint = null;
            return true;
        }

        public static bool CheckAllConstraints(this Def def, ThingDef pawnDef, out Constraint failedConstraint)
        {
            foreach (var constraint in GetAllConstraintsOfDef(def, ConstraintRuleFlag.OnApplyPawn))
            {
                if (!constraint.Check(pawnDef))
                {
                    failedConstraint = constraint;
                    if (failedConstraint.failReason != null)
                    {
                        JobFailReason.Is(failedConstraint.failReason);
                    }

                    return false;
                }
            }

            // Race
            foreach (var constraint in pawnDef.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
            {
                if (!constraint.Check(def)) 
                {
                    failedConstraint = constraint;
                    if (failedConstraint.failReason != null)
                    {
                        JobFailReason.Is(failedConstraint.failReason);
                    }

                    return false;
                }
            }

            failedConstraint = null;
            return true;
        }
    }

}
