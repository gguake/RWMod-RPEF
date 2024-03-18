using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class RaceConstraint : DefConstraint<ThingDef>
    {
        protected override IEnumerable<ThingDef> WhitelistInner => whitelist;
        protected override IEnumerable<ThingDef> BlacklistInner => blacklist;

        public List<ThingDef> whitelist;
        public List<ThingDef> blacklist;
    }
}
