using Verse;

namespace RPEF
{
    public static class ConstraintExtension
    {
        public static bool CheckAllConstraints(this Def def, Pawn pawn)
        {
            if (def?.modExtensions != null)
            {
                for (int i = 0; i < def.modExtensions.Count; ++i)
                {
                    if (def.modExtensions[i] is Constraint constraint && !constraint.Check(pawn))
                    {
                        return false;
                    }
                }
            }

            if (pawn.def.modExtensions != null)
            {
                for (int i = 0; i < pawn.def.modExtensions.Count; ++i)
                {
                    if (pawn.def.modExtensions[i] is Constraint constraint && !constraint.Check(def))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        public static bool CheckAllConstraints(this Def def, ThingDef pawnDef)
        {
            if (def.modExtensions == null) { return true; }

            for (int i = 0; i < def.modExtensions.Count; ++i)
            {
                if (def.modExtensions[i] is Constraint constraint && !constraint.Check(pawnDef))
                {
                    return false;
                }
            }

            for (int i = 0; i < pawnDef.modExtensions.Count; ++i)
            {
                if (pawnDef.modExtensions[i] is Constraint constraint && !constraint.Check(def))
                {
                    return false;
                }
            }

            return true;
        }
    }

}
