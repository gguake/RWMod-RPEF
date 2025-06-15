using RimWorld;
using System.Collections.Generic;
using System.Xml;
using Verse;

namespace RPEF
{
    public class HumanlikeFlyAnimationData
    {
        public LifeStageDef lifeStage;
        public AnimationDef animationSouth;
        public AnimationDef animationNorth;
        public AnimationDef animationEast;
        public AnimationDef animationWest;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            XmlHelper.ParseElements(this, xmlRoot, "lifeStage");
        }
    }

    public class HumanlikeFlyExtension : DefModExtension
    {
        public List<HumanlikeFlyAnimationData> animationData;

        public List<string> requireBodyParts;
    }
}
