using System.Collections.Generic;
using Verse;

namespace RPEF
{
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

        protected override bool Match(ThingDef def)
        {
            if (!def.IsWeapon)
            {
                return true;
            }

            return base.Match(def);
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

        protected override bool Match(ThingDef def)
        {
            if (!def.IsApparel)
            {
                return true;
            }

            return base.Match(def);
        }
    }
}
