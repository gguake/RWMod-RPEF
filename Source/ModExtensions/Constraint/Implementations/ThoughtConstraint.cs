using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class ThoughtConstraint : Constraint<ThoughtDef>
    {
        public HashSet<ThoughtDef> ThoughtDefs
        {
            get
            {
                if (_thoughtDefCache == null)
                {
                    _thoughtDefCache = new HashSet<ThoughtDef>();

                    if (thoughts != null)
                    {
                        _thoughtDefCache.AddRange(thoughts);
                    }
                }

                return _thoughtDefCache;
            }
        }
        private HashSet<ThoughtDef> _thoughtDefCache;
        private List<ThoughtDef> thoughts;

        protected override bool Match(ThoughtDef def) => def != null && ThoughtDefs.Contains(def);
        protected override bool Match(Pawn pawn)
        {
            var thoughtDefs = ThoughtDefs;
            if (pawn == null) { return false; }

            var memories = pawn.needs?.mood?.thoughts?.memories?.Memories;
            if (memories != null)
            {
                for (int i = 0; i < memories.Count; ++i)
                {
                    var memory = memories[i];
                    if (thoughtDefs.Contains(memory.def))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
