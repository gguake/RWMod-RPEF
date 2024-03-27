using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

namespace RPEF
{
    /// <summary>
    /// PawnKindDef
    /// </summary>
    public class PawnGeneratorKindHook : DefModExtension
    {
        public class PawnKindReplaceHook
        {
            public PawnKindDef targetKindDef;
            public int weight;

            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "targetKindDef", xmlRoot.Name);
                weight = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
            }
        }

        public Dictionary<PawnKindDef, int> ReplaceHookInfos
        {
            get
            {
                if (_replacerHookInfos == null && replaceHooks != null)
                {
                    _replacerHookInfos = replaceHooks.ToDictionary(v => v.targetKindDef, v => v.weight);
                }

                return _replacerHookInfos;
            }
        }
        private Dictionary<PawnKindDef, int> _replacerHookInfos;
        private List<PawnKindReplaceHook> replaceHooks;
    }
}
