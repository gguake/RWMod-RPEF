using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class XenotypeConstraint : DefConstraint<XenotypeDef>
    {
        protected override IEnumerable<XenotypeDef> WhitelistInner => whitelist;
        protected override IEnumerable<XenotypeDef> BlacklistInner => blacklist;

        public List<XenotypeDef> whitelist;
        public List<XenotypeDef> blacklist;
    }
}
