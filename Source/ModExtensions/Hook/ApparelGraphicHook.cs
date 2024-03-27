using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RPEF
{
    public class BodyTypeGraphicOverride
    {
        public BodyTypeDef from;
        public BodyTypeDef to;
    }

    public class ApparelGraphicHook : DefModExtension
    {
        public BodyTypeDef defaultBodyTypeGraphicOverride;
        public List<BodyTypeGraphicOverride> bodyTypeGraphicOverride;
    }
}
