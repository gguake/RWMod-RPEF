using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class TattooConstraint : DefConstraint<TattooDef>
    {
        protected override IEnumerable<TattooDef> WhitelistInner => whitelist;
        protected override IEnumerable<TattooDef> BlacklistInner => blacklist;

        public List<TattooDef> whitelist;
        public List<TattooDef> blacklist;
    }
}
