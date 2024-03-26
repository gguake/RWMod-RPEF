using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class BeardConstraint : DefConstraint<BeardDef>
    {
        protected override IEnumerable<BeardDef> WhitelistInner => whitelist;
        protected override IEnumerable<BeardDef> BlacklistInner => blacklist;

        public List<BeardDef> whitelist;
        public List<BeardDef> blacklist;
    }
}
