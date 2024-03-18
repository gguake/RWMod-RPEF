using System.Collections.Generic;
using System.Xml;
using Verse;

namespace RPEF
{
    public class PawnKindReplacer
    {
        public PawnKindDef targetKindDef;
        public float weight;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "targetKindDef", xmlRoot.Name);
            weight = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
        }
    }

    public class PawnKindExtension : DefModExtension
    {
        public List<PawnKindReplacer> pawnKindReplacer;
    }
}
