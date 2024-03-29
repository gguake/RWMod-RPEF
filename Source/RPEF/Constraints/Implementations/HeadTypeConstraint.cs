using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class HeadTypeConstraint : Constraint<HeadTypeDef>
    {
        public HashSet<HeadTypeDef> HeadTypeDefs
        {
            get
            {
                if (_headTypeDefCache == null)
                {
                    _headTypeDefCache = new HashSet<HeadTypeDef>();

                    if (headTypes != null)
                    {
                        _headTypeDefCache.AddRange(headTypes);
                    }
                }

                return _headTypeDefCache;
            }
        }
        private HashSet<HeadTypeDef> _headTypeDefCache;
        private List<HeadTypeDef> headTypes;

        protected override bool Match(HeadTypeDef def) => def != null && HeadTypeDefs.Contains(def);
        protected override bool Match(Pawn pawn) => Match(pawn?.story?.headType);
    }
}
