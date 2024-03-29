using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class RecipeConstraint : Constraint<RecipeDef>
    {
        public HashSet<RecipeDef> RecipeDefs
        {
            get
            {
                if (_recipeDefCache == null)
                {
                    _recipeDefCache = new HashSet<RecipeDef>();

                    if (recipes != null)
                    {
                        _recipeDefCache.AddRange(recipes);
                    }
                }

                return _recipeDefCache;
            }
        }
        private HashSet<RecipeDef> _recipeDefCache;
        private List<RecipeDef> recipes;

        protected override bool Match(RecipeDef def) => def != null && RecipeDefs.Contains(def);
        protected override bool Match(Pawn pawn) => true;
    }
}
