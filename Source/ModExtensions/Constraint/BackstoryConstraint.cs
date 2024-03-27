using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RPEF
{
    public class BackstoryConstraint : Constraint<BackstoryDef>
    {
        public HashSet<BackstoryDef> BackstoryDefs
        {
            get
            {
                if (_backstoryDefCache == null)
                {
                    _backstoryDefCache = new HashSet<BackstoryDef>();

                    if (backstories != null)
                    {
                        _backstoryDefCache.AddRange(backstories);
                    }

                    if (backstoryCategories != null)
                    {
                        foreach (var def in DefDatabase<BackstoryDef>.AllDefsListForReading)
                        {
                            if (def.spawnCategories.Intersect(backstoryCategories).Any())
                            {
                                _backstoryDefCache.Add(def);
                            }
                        }
                    }
                }

                return _backstoryDefCache;
            }
        }
        private HashSet<BackstoryDef> _backstoryDefCache;

        private List<BackstoryDef> backstories;
        private List<string> backstoryCategories;

        protected override bool Match(BackstoryDef def) => def != null && BackstoryDefs.Contains(def);
        protected override bool Match(Pawn pawn)
        {
            var backstoryDefs = BackstoryDefs;
            if (backstoryDefs == null || pawn == null) { return false; }

            var childhood = pawn.story?.Childhood;
            if (childhood != null && backstoryDefs.Contains(childhood)) { return true; }

            var adulthood = pawn.story?.Adulthood;
            if (adulthood != null && backstoryDefs.Contains(adulthood)) { return true; }

            return false;
        }
    }
}
