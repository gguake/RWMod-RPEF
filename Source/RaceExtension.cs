using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
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

    public class RelationChanceMultiplier
    {
        public PawnRelationDef relationDef;
        public float multiplier;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "relationDef", xmlRoot.Name);
            multiplier = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
        }
    }

    public class GeneOverride
    {
        public GeneDef geneDef;
        public float weight;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "geneDef", xmlRoot.Name);
            weight = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
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

        public Dictionary<PawnRelationDef, float> RelationChanceMultiplier
        {
            get
            {
                if (_relationChanceMultiplierDict == null)
                {
                    _relationChanceMultiplierDict = relationChanceMultiplier?.ToDictionary(v => v.relationDef, v => v.multiplier) ?? new Dictionary<PawnRelationDef, float>();
                }

                return _relationChanceMultiplierDict;
            }
        }
        private Dictionary<PawnRelationDef, float> _relationChanceMultiplierDict;
        public List<RelationChanceMultiplier> relationChanceMultiplier;

        public BodyTypeDef fixedMaleBodyType;
        public BodyTypeDef fixedFemaleBodyType;

        public List<GeneOverride> melaninGeneOverrides;
        public List<GeneOverride> hairColorGeneOverrides;
    }
}
