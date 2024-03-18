using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class HairConstraint : DefConstraint<HairDef>
    {
        protected override IEnumerable<HairDef> WhitelistInner => whitelist;
        protected override IEnumerable<HairDef> BlacklistInner => blacklist;

        public List<HairDef> whitelist;
        public List<HairDef> blacklist;
    }
}
