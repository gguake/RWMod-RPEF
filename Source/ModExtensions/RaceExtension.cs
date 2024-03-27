using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

namespace RPEF
{
    public class ThoughtReplacer
    {
        public ThoughtDef thoughtDef;
        public ThoughtDef replacedThoughtDef;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thoughtDef", xmlRoot.Name);
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "replacedThoughtDef", xmlRoot.FirstChild.Value);
        }
    }

    public class RaceExtension : DefModExtension
    {
        public Dictionary<ThoughtDef, ThoughtDef> ThoughtReplacer
        {
            get
            {
                if (_thoughtReplacerDict == null)
                {
                    _thoughtReplacerDict = thoughtReplacer?.ToDictionary(v => v.thoughtDef, v => v.replacedThoughtDef) ?? new Dictionary<ThoughtDef, ThoughtDef>();
                }

                return _thoughtReplacerDict;
            }
        }
        private Dictionary<ThoughtDef, ThoughtDef> _thoughtReplacerDict;
        public List<ThoughtReplacer> thoughtReplacer;
    }
}
