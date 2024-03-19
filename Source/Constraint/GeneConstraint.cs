using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class GeneConstraint : DefConstraint<GeneDef>
    {
        protected override IEnumerable<GeneDef> WhitelistInner => whitelist;

        protected override IEnumerable<GeneDef> BlacklistInner => blacklist;

        public List<GeneDef> whitelist;
        public List<GeneDef> blacklist;
    }
}
