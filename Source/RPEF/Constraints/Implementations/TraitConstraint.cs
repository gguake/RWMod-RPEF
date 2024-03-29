using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class TraitConstraint : Constraint<TraitDef>
    {
        public HashSet<TraitDef> TraitDefs
        {
            get
            {
                if (_traitDefCache == null)
                {
                    _traitDefCache = new HashSet<TraitDef>();

                    if (traits != null)
                    {
                        _traitDefCache.AddRange(traits);
                    }
                }

                return _traitDefCache;
            }
        }
        private HashSet<TraitDef> _traitDefCache;
        private List<TraitDef> traits;

        protected override bool Match(Pawn pawn)
        {
            if (pawn == null) { return false; }

            var traitDefs = TraitDefs;
            foreach (var trait in pawn.story.traits.allTraits)
            {
                if (traitDefs.Contains(trait.def))
                {
                    return true;
                }
            }

            return false;
        }

        protected override bool Match(TraitDef def) => def != null && TraitDefs.Contains(def);
    }
}
