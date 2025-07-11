using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class EquipmentExtension : DefModExtension
    {
        private static Dictionary<ThingDef, EquipmentExtension> _extensionCache = new Dictionary<ThingDef, EquipmentExtension>();

        public bool canUseOnMeleeThreat;

        public override void ResolveReferences(Def parentDef)
        {
            if (parentDef is ThingDef thingDef)
            {
                _extensionCache[thingDef] = this;
            }
        }

    }
}
