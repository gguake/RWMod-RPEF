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
            if (xmlRoot.FirstChild?.Value != null && xmlRoot.FirstChild.Value.Length > 0)
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "replacedThoughtDef", xmlRoot.FirstChild.Value);
            }
        }
    }

    public class RaceExtension : DefModExtension
    {
        /// <summary>
        /// 해당 Race에 적용되는 ThoughtDef를 치환
        /// </summary>
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

        /// <summary>
        /// otherPawn가 해당 Race인 경우에 적용되는 ThoughtDef를 치환
        /// </summary>
        public Dictionary<ThoughtDef, ThoughtDef> ThoughtReplacerInverse
        {
            get
            {
                if (_thoughtReplacerInvDict == null)
                {
                    _thoughtReplacerInvDict = thoughtReplacerInv?.ToDictionary(v => v.thoughtDef, v => v.replacedThoughtDef) ?? new Dictionary<ThoughtDef, ThoughtDef>();
                }

                return _thoughtReplacerInvDict;
            }
        }
        private Dictionary<ThoughtDef, ThoughtDef> _thoughtReplacerInvDict;
        public List<ThoughtReplacer> thoughtReplacerInv;

        public static bool HideGenes(Pawn pawn) => pawn?.def.GetModExtension<RaceExtension>()?.hideGenes == true;
        public bool hideGenes = false;

        /// <summary>
        /// 불임 여부
        /// </summary>
        public bool sterile = false;

        /// <summary>
        /// 성장 단계를 임의로 변경할때 사용
        /// </summary>
        public List<int> growthMomentAgeOverride;

        /// <summary>
        /// 성장 점수 획득 배율을 변경할때 사용
        /// </summary>
        public SimpleCurve growthPointFactorCurve;

        /// <summary>
        /// 해당 Race의 고백 시도 빈도 가중치
        /// </summary>
        public float romanceFrequencyWeight = 1f;

        /// <summary>
        /// 해당 Race가 고백 시도시 성공률 가중치
        /// </summary>
        public float romanceSuccessChanceMultiplierAsInitiator = 1f;

        /// <summary>
        /// 해당 Race가 고백을 받을때 성공률 가중치
        /// </summary>
        public float romanceSuccessChanceMultiplierAsRecipient = 1f;
    }
}
