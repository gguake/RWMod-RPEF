using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RPEF
{
    public class HediffStageWithSkillAptitude : HediffStage
    {
        public List<Aptitude> skillAptitudes;

        public int AptitudeFor(SkillDef skill)
        {
            if (skillAptitudes.NullOrEmpty()) { return 0; }

            int sum = 0;
            for (int i = 0; i < skillAptitudes.Count; ++i)
            {
                if (skillAptitudes[i].skill == skill)
                {
                    sum += skillAptitudes[i].level;
                }
            }
            return sum;
        }
    }
}
