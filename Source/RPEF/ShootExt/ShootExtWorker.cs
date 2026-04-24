using UnityEngine;
using Verse;

namespace RPEF
{
    public abstract class ShootExtWorker
    {
        public virtual void OnLaunchBullet(
            Verb_ShootExt verb,
            Projectile originalProjectile,
            Thing launcher,
            Vector3 origin,
            LocalTargetInfo usedTarget,
            LocalTargetInfo intendedTarget,
            ProjectileHitFlags hitFlags,
            bool preventFriendlyFire,
            Thing equipment,
            ThingDef targetCoverDef)
        {
        }

        public virtual bool HookShotsPerBurst(Verb_ShootExt verb, ref int count) { return true; }
        public virtual bool HookEffectiveRange(Verb_ShootExt verb, ref float range) { return true; }
        public virtual bool HookWarmupTime(Verb_ShootExt verb, ref float warmupTime) { return true; }
        public virtual bool HookProjectile(Verb_ShootExt verb, ref ThingDef thingDef) { return true; }
        public virtual bool HookAvailable(Verb_ShootExt verb, ref bool available) { return true; }
    }

}
