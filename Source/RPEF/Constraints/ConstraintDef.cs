using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class ConstraintDef : Def
    {
        public List<Constraint> constraints;
    }

    public class ConstraintModExtension : DefModExtension
    {
        public ConstraintDef def;
        public List<ConstraintDef> defs;
    }
}
