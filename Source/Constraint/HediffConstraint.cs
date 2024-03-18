using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class HediffConstraint : DefConstraint<HediffDef>
    {
        protected override IEnumerable<HediffDef> WhitelistInner => whitelist;
        protected override IEnumerable<HediffDef> BlacklistInner => blacklist;

        public List<HediffDef> whitelist;
        public List<HediffDef> blacklist;
    }
}
