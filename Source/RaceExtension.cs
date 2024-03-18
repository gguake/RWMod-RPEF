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

        public BodyTypeDef fixedMaleBodyType;
        public BodyTypeDef fixedFemaleBodyType;

        public List<ThoughtReplacer> thoughtReplacer;
        public List<RelationChanceMultiplier> relationChanceMultiplier;
    }
}
