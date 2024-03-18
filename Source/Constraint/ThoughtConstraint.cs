using RimWorld;
using System.Collections.Generic;

namespace RPEF
{
    public class ThoughtConstraint : DefConstraint<ThoughtDef>
    {
        protected override IEnumerable<ThoughtDef> WhitelistInner => whitelist;
        protected override IEnumerable<ThoughtDef> BlacklistInner => blacklist;

        public List<ThoughtDef> whitelist;
        public List<ThoughtDef> blacklist;
    }
}
