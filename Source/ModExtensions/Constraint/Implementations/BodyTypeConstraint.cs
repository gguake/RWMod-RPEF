using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class BodyTypeConstraint : Constraint<BodyTypeDef>
    {
        public HashSet<BodyTypeDef> BodyTypeDefs
        {
            get
            {
                if (_bodyTypeDefCache == null)
                {
                    _bodyTypeDefCache = new HashSet<BodyTypeDef>();

                    if (bodyTypes != null)
                    {
                        _bodyTypeDefCache.AddRange(bodyTypes);
                    }
                }

                return _bodyTypeDefCache;
            }
        }
        private HashSet<BodyTypeDef> _bodyTypeDefCache;
        private List<BodyTypeDef> bodyTypes;

        protected override bool Match(BodyTypeDef def) => def != null && BodyTypeDefs.Contains(def);
        protected override bool Match(Pawn pawn) => Match(pawn.story?.bodyType);
    }
}
