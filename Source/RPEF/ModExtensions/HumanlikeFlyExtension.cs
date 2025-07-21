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
        public HumanlikeFlyAnimationData GetAnimationData(LifeStageDef def)
        {
            if (!_dictLifestageAnimationData.TryGetValue(def, out var data))
            {
                data = animationData.FirstOrDefault(v => v.lifeStage == def);
                _dictLifestageAnimationData.Add(def, data);
            }

            return data;
        }

        [Unsaved]
        private Dictionary<LifeStageDef, HumanlikeFlyAnimationData> _dictLifestageAnimationData = new System.Collections.Generic.Dictionary<LifeStageDef, HumanlikeFlyAnimationData>();

        public List<HumanlikeFlyAnimationData> animationData;
        public List<string> requireBodyParts;

        public bool alwaysFlyIfDrafted;

        public override void ResolveReferences(Def parentDef)
        {
            _dictLifestageAnimationData.Clear();
        }

        public bool CheckCanEverFly(Pawn pawn)
        {
            var lifeStageDef = pawn.ageTracker?.CurLifeStage;
            if (lifeStageDef == null) { return false; }

            if (GetAnimationData(lifeStageDef) == null) { return false; }

            if (requireBodyParts != null)
            {
                for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; ++i)
                {
                    var hediff = pawn.health.hediffSet.hediffs[i];
                    if (hediff is Hediff_MissingPart hediffMissingPart && hediffMissingPart.Part != null)
                    {
                        for (int j = 0; j < requireBodyParts.Count; ++j)
                        {
                            if (requireBodyParts[j] == hediffMissingPart.Part.untranslatedCustomLabel)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
