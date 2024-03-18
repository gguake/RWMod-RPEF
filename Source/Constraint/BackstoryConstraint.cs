using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RPEF
{
    public class BackstoryConstraint : DefConstraint<BackstoryDef>
    {
        protected override IEnumerable<BackstoryDef> WhitelistInner
        {
            get
            {
                if (whitelist == null && categoryWhitelist == null)
                {
                    return null;
                }

                var result = new HashSet<BackstoryDef>();
                if (whitelist != null)
                {
                    result.AddRange(whitelist);
                }

                if (categoryWhitelist != null)
                {
                    foreach (var def in DefDatabase<BackstoryDef>.AllDefsListForReading)
                    {
                        foreach (var category in def.spawnCategories)
                        {
                            if (categoryWhitelist.Contains(category))
                            {
                                result.Add(def);
                                break;
                            }
                        }
                    }
                }

                return result;
            }
        }

        protected override IEnumerable<BackstoryDef> BlacklistInner
        {
            get
            {
                if (blacklist == null && categoryBlacklist == null)
                {
                    return null;
                }

                var result = new HashSet<BackstoryDef>();
                if (blacklist != null)
                {
                    result.AddRange(blacklist);
                }

                if (categoryBlacklist != null)
                {
                    foreach (var def in DefDatabase<BackstoryDef>.AllDefsListForReading)
                    {
                        foreach (var category in def.spawnCategories)
                        {
                            if (categoryBlacklist.Contains(category))
                            {
                                result.Add(def);
                                break;
                            }
                        }
                    }
                }

                return result;
            }
        }

        public List<BackstoryDef> whitelist;
        public List<BackstoryDef> blacklist;

        public List<string> categoryWhitelist;
        public List<string> categoryBlacklist;
    }
}
