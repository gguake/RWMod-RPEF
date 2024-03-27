using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class XenotypeConstraint : Constraint<XenotypeDef>
    {
        public HashSet<XenotypeDef> XenotypeDefs
        {
            get
            {
                if (_xenotypeDefCache == null)
                {
                    _xenotypeDefCache = new HashSet<XenotypeDef>();

                    if (xenotypes != null)
                    {
                        _xenotypeDefCache.AddRange(xenotypes);
                    }
                }

                return _xenotypeDefCache;
            }
        }
        private HashSet<XenotypeDef> _xenotypeDefCache;
        private List<XenotypeDef> xenotypes;

        protected override bool Match(XenotypeDef def) => def != null && XenotypeDefs.Contains(def);
        protected override bool Match(Pawn pawn)
        {
            if (pawn == null) { return false; }

            var xenotypeDefs = XenotypeDefs;
            if (xenotypeDefs.Contains(pawn.genes?.Xenotype)) { return true; }

            return false;
        }
    }
}
