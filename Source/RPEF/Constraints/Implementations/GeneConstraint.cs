using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class GeneConstraint : Constraint<GeneDef>
    {
        public HashSet<GeneDef> GeneDefs
        {
            get
            {
                if (_geneDefCache == null)
                {
                    _geneDefCache = new HashSet<GeneDef>();

                    if (genes != null)
                    {
                        _geneDefCache.AddRange(genes);
                    }
                }

                return _geneDefCache;
            }
        }
        private HashSet<GeneDef> _geneDefCache;
        private List<GeneDef> genes;

        protected override bool Match(GeneDef def) => def != null && GeneDefs.Contains(def);
        protected override bool Match(Pawn pawn)
        {
            var geneDefs = GeneDefs;
            if (pawn == null) { return false; }

            var pawnGenes = pawn.genes?.GenesListForReading;
            if (pawnGenes != null)
            {
                for (int i = 0; i < pawnGenes.Count; ++i)
                {
                    if (geneDefs.Contains(pawnGenes[i].def))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
