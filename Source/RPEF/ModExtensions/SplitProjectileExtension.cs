using System.Collections.Generic;
using Verse;

namespace RPEF
{
    /// <summary>
    /// Bullet_Split과 함께 사용
    /// 발사 즉시 projectileDef 탄환 count개로 분열된다.
    /// </summary>
    public class SplitProjectileExtension : DefModExtension
    {
        public ThingDef projectileDef;
        public int count = 1;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors())
            {
                yield return error;
            }

            if (projectileDef == null)
            {
                yield return "projectileDef is null";
            }
            else if (projectileDef.projectile == null)
            {
                yield return $"projectileDef {projectileDef.defName} is not a projectile";
            }

            if (count < 1)
            {
                yield return "count must be at least 1";
            }
        }
    }
}
