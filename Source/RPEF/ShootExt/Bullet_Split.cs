using RimWorld;
using UnityEngine;
using Verse;

namespace RPEF
{
    /// <summary>
    /// SplitProjectileExtension이 있어야 정상 동작
    /// 발사되는 즉시 자신은 제거되고 분열된 탄환들이 동일한 인자로 발사된다.
    /// </summary>
    public class Bullet_Split : Bullet
    {
        public override void Launch(
            Thing launcher,
            Vector3 origin,
            LocalTargetInfo usedTarget,
            LocalTargetInfo intendedTarget,
            ProjectileHitFlags hitFlags,
            bool preventFriendlyFire = false,
            Thing equipment = null,
            ThingDef targetCoverDef = null)
        {
            var extension = def.GetModExtension<SplitProjectileExtension>();
            if (extension?.projectileDef == null || !Spawned)
            {
                base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
                return;
            }

            var position = Position;
            var map = Map;
            Destroy();

            for (int i = 0; i < extension.count; ++i)
            {
                var subProjectile = (Projectile)GenSpawn.Spawn(extension.projectileDef, position, map);

                // 무기 특성으로 부여된 데미지 정보는 발사 전에 설정되므로 분열 탄환에도 넘겨준다.
                subProjectile.damageDefOverride = damageDefOverride;
                if (!extraDamages.NullOrEmpty())
                {
                    subProjectile.extraDamages.AddRange(extraDamages);
                }

                subProjectile.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            }
        }
    }
}
