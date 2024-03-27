using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class HediffConstraint : Constraint<HediffDef>
    {
        public HashSet<HediffDef> HediffDefs
        {
            get
            {
                if (_hediffDefCache == null)
                {
                    _hediffDefCache = new HashSet<HediffDef>();

                    if (hediffs != null)
                    {
                        _hediffDefCache.AddRange(hediffs);
                    }
                }

                return _hediffDefCache;
            }
        }
        private HashSet<HediffDef> _hediffDefCache;
        private List<HediffDef> hediffs;

        protected override bool Match(HediffDef def) => def != null && HediffDefs.Contains(def);
        protected override bool Match(Pawn pawn)
        {
            var hediffDefs = HediffDefs;
            if (pawn == null) { return false; }

            var hediffs = pawn.health?.hediffSet?.hediffs;
            if (hediffs != null)
            {
                for (int i = 0; i < hediffs.Count; ++i)
                {
                    var hediff = hediffs[i];
                    if (hediffDefs.Contains(hediff.def))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
