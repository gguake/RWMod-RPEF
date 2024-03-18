using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class HeadTypeConstraint : DefConstraint<HeadTypeDef>
    {
        protected override IEnumerable<HeadTypeDef> WhitelistInner => whitelist;
        protected override IEnumerable<HeadTypeDef> BlacklistInner => blacklist;

        public List<HeadTypeDef> whitelist;
        public List<HeadTypeDef> blacklist;
    }
}
