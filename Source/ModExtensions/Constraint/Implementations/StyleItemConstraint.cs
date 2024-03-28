using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RPEF
{
    public class StyleItemConstraint : Constraint<StyleItemDef>
    {
        public HashSet<StyleItemDef> StyleItemDefs
        {
            get
            {
                if (_styleItemCache == null)
                {
                    _styleItemCache = new HashSet<StyleItemDef>();

                    if (hairs != null)
                    {
                        _styleItemCache.AddRange(hairs);
                    }

                    if (hairTags != null)
                    {
                        foreach (var def in DefDatabase<HairDef>.AllDefsListForReading)
                        {
                            if (def.styleTags.Intersect(hairTags).Any())
                            {
                                _styleItemCache.Add(def);
                            }
                        }
                    }

                    if (beards != null)
                    {
                        _styleItemCache.AddRange(beards);
                    }

                    if (beardTags != null)
                    {
                        foreach (var def in DefDatabase<BeardDef>.AllDefsListForReading)
                        {
                            if (def.styleTags.Intersect(beardTags).Any())
                            {
                                _styleItemCache.Add(def);
                            }
                        }
                    }

                    if (tattoos != null)
                    {
                        _styleItemCache.AddRange(tattoos);
                    }

                    if (tattooTags != null)
                    {
                        foreach (var def in DefDatabase<TattooDef>.AllDefsListForReading)
                        {
                            if (def.styleTags.Intersect(tattooTags).Any())
                            {
                                _styleItemCache.Add(def);
                            }
                        }
                    }
                }

                return _styleItemCache;
            }
        }
        private HashSet<StyleItemDef> _styleItemCache;

        private List<HairDef> hairs;
        private List<BeardDef> beards;
        private List<TattooDef> tattoos;

        private List<string> hairTags;
        private List<string> beardTags;
        private List<string> tattooTags;

        protected override bool Match(StyleItemDef def) => def != null && StyleItemDefs.Contains(def);
        protected override bool Match(Pawn pawn)
        {
            if (pawn == null) { return false; }

            var styleItemDefs = StyleItemDefs;
            if (styleItemDefs.Contains(pawn.story.hairDef)) { return true; }
            if (styleItemDefs.Contains(pawn.style.beardDef)) { return true; }
            if (styleItemDefs.Contains(pawn.style.BodyTattoo)) { return true; }
            if (styleItemDefs.Contains(pawn.style.FaceTattoo)) { return true; }

            return false;
        }
    }
}
