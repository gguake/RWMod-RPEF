using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

namespace RPEF
{
    /// <summary>
    /// ThingDef
    /// </summary>
    public class PawnGeneratorRaceHook : DefModExtension
    {
        public class RelationChanceMultiplierData
        {
            public PawnRelationDef relationDef;
            public float multiplier;

            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "relationDef", xmlRoot.Name);
                multiplier = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
            }
        }
        public Dictionary<PawnRelationDef, float> RelationChanceMultiplier
        {
            get
            {
                if (_relationChanceMultiplierDict == null && relationChanceMultiplier != null)
                {
                    _relationChanceMultiplierDict = relationChanceMultiplier.ToDictionary(v => v.relationDef, v => v.multiplier) ?? new Dictionary<PawnRelationDef, float>();
                }

                return _relationChanceMultiplierDict;
            }
        }
        private Dictionary<PawnRelationDef, float> _relationChanceMultiplierDict;
        private List<RelationChanceMultiplierData> relationChanceMultiplier;

        public BodyTypeDef fixedMaleBodyType;
        public BodyTypeDef fixedFemaleBodyType;

        public class GeneOverrideData
        {
            public GeneDef geneDef;
            public float weight;

            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "geneDef", xmlRoot.Name);
                weight = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
            }
        }
        public List<GeneOverrideData> melaninGeneOverrides;
        public List<GeneOverrideData> hairColorGeneOverrides;


    }
}
