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
        private bool? randomChosen;

        protected override bool Match(HeadTypeDef def)
        {
            if (def != null)
            {
                if (HeadTypeDefs.Contains(def)) { return true; }

                if (randomChosen.HasValue && randomChosen == def.randomChosen)
                {
                    return true;
                }
            }

            return false;
        }

        protected override bool Match(Pawn pawn) => Match(pawn?.story?.headType);
    }
}
