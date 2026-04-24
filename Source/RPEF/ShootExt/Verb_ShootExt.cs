using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace RPEF
{
    public class VerbProperties_ShootExt : VerbProperties
    {
        public List<ShootExtWorker> workers;
    }

    public class Verb_ShootExt : Verb_Shoot
    {
        public VerbProperties_ShootExt VerbProps => (VerbProperties_ShootExt)verbProps;

        protected override int ShotsPerBurst
        {
            get
            {
                var count = base.ShotsPerBurst;
                for (int i = 0; i <  VerbProps.workers.Count; ++i)
                {
                    if (VerbProps.workers[i].HookShotsPerBurst(this, ref count))
                    {
                        break;
                    }
                }

                return count;
            }
        }

        public override float EffectiveRange
        {
            get
            {
                var range = base.EffectiveRange;
                for (int i = 0; i < VerbProps.workers.Count; ++i)
                {
                    if (VerbProps.workers[i].HookEffectiveRange(this, ref range))
                    {
                        break;
                    }
                }

                return range;
            }
        }

        public override float WarmupTime
        {
            get
            {
                var warmupTime = base.WarmupTime;
                for (int i = 0; i < VerbProps.workers.Count; ++i)
                {
                    if (VerbProps.workers[i].HookWarmupTime(this, ref warmupTime))
                    {
                        break;
                    }
                }

                return warmupTime;
            }
        }

        public override ThingDef Projectile
        {
            get
            {
                var def = base.Projectile;
                for (int i = 0; i < VerbProps.workers.Count; ++i)
                {
                    if (VerbProps.workers[i].HookProjectile(this, ref def))
                    {
                        break;
                    }
                }

                return def;
            }
        }

        public override bool Available()
        {
            var available = base.Available();
            for (int i = 0; i < VerbProps.workers.Count; ++i)
            {
                if (VerbProps.workers[i].HookAvailable(this, ref available))
                {
                    break;
                }
            }

            return available;
        }

        protected override bool TryCastShot() => Verb_ShootExt_Patch.TryCastShot_LaunchProjectile(this);
    }

    [HarmonyPatch]
    internal static class Verb_ShootExt_Patch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
        public static bool TryCastShot_LaunchProjectile(Verb_LaunchProjectile instance)
        {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var methodInfoLaunch = AccessTools.Method(
                    typeof(Projectile),
                    nameof(Projectile.Launch),
                    new Type[]
                    {
                        typeof(Thing),
                        typeof(Vector3),
                        typeof(LocalTargetInfo),
                        typeof(LocalTargetInfo),
                        typeof(ProjectileHitFlags),
                        typeof(bool),
                        typeof(Thing),
                        typeof(ThingDef)
                    });

                var methodInfoLaunchInterceptor = AccessTools.Method(typeof(Verb_ShootExt_Patch), nameof(LaunchInterceptor));

                foreach (var inst in instructions)
                {
                    if (inst.Calls(methodInfoLaunch))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, methodInfoLaunchInterceptor);
                    }
                    else
                    {
                        yield return inst;
                    }
                }
            }

            _ = Transpiler(null);
            throw new NotImplementedException("stub");
        }

        public static void LaunchInterceptor(
            Projectile projectile,
            Thing launcher,
            Vector3 origin,
            LocalTargetInfo usedTarget,
            LocalTargetInfo intendedTarget,
            ProjectileHitFlags hitFlags,
            bool preventFriendlyFire,
            Thing equipment,
            ThingDef targetCoverDef,
            Verb_LaunchProjectile verb)
        {
            if (verb is Verb_ShootExt verbShootExt)
            {
                var props = verbShootExt.VerbProps;

                projectile.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);

                for (int i = 0; i < props.workers.Count; ++i)
                {
                    props.workers[i].OnLaunchBullet(verbShootExt, projectile, launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
                }
            }
            else
            {
                projectile.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
                return;
            }
        }
    }

}
