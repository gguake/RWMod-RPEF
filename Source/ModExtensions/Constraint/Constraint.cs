using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RPEF
{
    public enum ConstraintType : byte
    {
        NoOp,
        Required,
        Conflict,

        Whitelist,
        Blacklist,
    }

    public abstract class Constraint : DefModExtension
    {
        public ConstraintType type = ConstraintType.NoOp;

        public virtual void Validate()
        {
        }

        public bool Check(Pawn pawn)
        {
            var matched = Match(pawn);

            switch (type)
            {
                case ConstraintType.NoOp:
                    return true;

                case ConstraintType.Required:
                case ConstraintType.Whitelist:
                    return matched;

                case ConstraintType.Conflict:
                case ConstraintType.Blacklist:
                    return !matched;
            }

            return false;
        }

        protected abstract bool Match(Pawn pawn);
    }

    public abstract class Constraint<T> : Constraint
    {
        protected abstract bool Match(T def);
    }

    public static class ConstraintExtension
    {
        public static bool CheckAllConstraints(this Def def, Pawn pawn)
        {
            if (def.modExtensions == null) { return true; }

            for (int i = 0; i < def.modExtensions.Count; ++i)
            {
                if (def.modExtensions[i] is Constraint constraint && !constraint.Check(pawn))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CheckAllConstraints<T>(this Pawn pawn, T def) where T : Def
        {
            if (pawn.def.modExtensions == null) { return true; }

            for (int i = 0; i < def.modExtensions.Count; ++i)
            {
                if (def.modExtensions[i] is Constraint constraint && !constraint.Check(pawn))
                {
                    return false;
                }
            }

            return true;
        }
    }

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
                }

                return _backstoryDefCache;
            }
        }
        private HashSet<BackstoryDef> _backstoryDefCache;
        private List<BackstoryDef> backstories;

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

    public class HediffConstraint : Constraint<HediffDef>
    {
        public HashSet<HediffDef> HediffDefs
        {
            get
            {
                if (_hediffDefCache == null)
                {
                    _hediffDefCache = new HashSet<HediffDef>();

                    if (hediffs != null)
                    {
                        _hediffDefCache.AddRange(hediffs);
                    }
                }

                return _hediffDefCache;
            }
        }
        private HashSet<HediffDef> _hediffDefCache;
        private List<HediffDef> hediffs;

        protected override bool Match(HediffDef def) => def != null && HediffDefs.Contains(def);
        protected override bool Match(Pawn pawn)
        {
            var hediffDefs = HediffDefs;
            if (pawn == null) { return false; }

            var hediffs = pawn.health?.hediffSet?.hediffs;
            if (hediffs != null)
            {
                for (int i = 0; i < hediffs.Count; ++i)
                {
                    var hediff = hediffs[i];
                    if (hediffDefs.Contains(hediff.def))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

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

    public abstract class ThingConstraint : Constraint<ThingDef>
    {
        public HashSet<ThingDef> ThingDefs
        {
            get
            {
                if (_thingDefCache == null)
                {
                    _thingDefCache = new HashSet<ThingDef>();

                    if (things != null)
                    {
                        _thingDefCache.AddRange(things);
                    }
                }

                return _thingDefCache;
            }
        }
        private HashSet<ThingDef> _thingDefCache;
        private List<ThingDef> things;

        protected override bool Match(ThingDef def) => def != null && ThingDefs.Contains(def);
    }

    public class RaceConstraint : ThingConstraint
    {
        public override void Validate()
        {
            foreach (var thingDef in ThingDefs)
            {
                if (thingDef.category != ThingCategory.Pawn || thingDef.race == null)
                {
                    Log.Error($"[RPEF] {thingDef.defName} is not allowed for RaceConstraint.");
                }
            }
        }

        protected override bool Match(Pawn pawn)
        {
            if (pawn == null) { return false; }

            var thingDefs = ThingDefs;
            if (thingDefs.Contains(pawn.def)) { return true; }

            return false;
        }
    }

    public class EquipmentConstraint : ThingConstraint
    {
        public override void Validate()
        {
            foreach (var thingDef in ThingDefs)
            {
                if (!thingDef.IsWeapon)
                {
                    Log.Error($"[RPEF] {thingDef.defName} is not allowed for EquipmentConstraint.");
                }
            }
        }

        protected override bool Match(Pawn pawn)
        {
            if (pawn == null || pawn.equipment == null) { return false; }

            var thingDefs = ThingDefs;
            foreach (var equipment in pawn.equipment.AllEquipmentListForReading)
            {
                if (thingDefs.Contains(equipment.def))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class ApparelConstraint : ThingConstraint
    {
        public override void Validate()
        {
            foreach (var thingDef in ThingDefs)
            {
                if (!thingDef.IsApparel)
                {
                    Log.Error($"[RPEF] {thingDef.defName} is not allowed for ApparelConstraint.");
                }
            }
        }

        protected override bool Match(Pawn pawn)
        {
            if (pawn == null || pawn.apparel == null) { return false; }

            var thingDefs = ThingDefs;
            foreach (var apparel in pawn.apparel.WornApparel)
            {
                if (thingDefs.Contains(apparel.def))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
