using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace RPEF
{
    /// <summary>
    /// CompEquippableVerbMode가 있어야 정상 동작
    /// </summary>
    public class Verb_ShootWithMode : Verb_Shoot
    {
        public CompEquippableVerbMode EquipmentCompVerbModeSource => DirectOwner as CompEquippableVerbMode;

        public override Texture2D UIIcon =>
            EquipmentCompVerbModeSource?.CurrentVerbMode.UIIcon ??
            base.UIIcon;

        protected override int ShotsPerBurst => 
            EquipmentCompVerbModeSource?.CurrentVerbMode.shotsPerBurstOverride ??
            base.ShotsPerBurst;

        public override float EffectiveRange
        {
            get
            {
                var overrideRange = EquipmentCompVerbModeSource?.CurrentVerbMode.effectiveRangeOverride;
                if (overrideRange.HasValue)
                {
                    return overrideRange.Value * (EquipmentSource?.GetStatValue(StatDefOf.RangedWeapon_RangeMultiplier) ?? 1f);
                }

                return base.EffectiveRange;
            }
        }

        public override float WarmupTime
        {
            get
            {
                var overrideWarmupTime = EquipmentCompVerbModeSource?.CurrentVerbMode.warmupTimeOverride;
                if (overrideWarmupTime.HasValue)
                {
                    return overrideWarmupTime.Value * (EquipmentSource?.GetStatValue(StatDefOf.RangedWeapon_WarmupMultiplier) ?? 1f);
                }

                return base.WarmupTime;
            }
        }

        public override ThingDef Projectile =>
            EquipmentCompVerbModeSource?.CurrentVerbMode.projectileDefOverride ?? 
            base.Projectile;

        protected override bool TryCastShot() => Verb_ShootWithMode_Patch.TryCastShot_LaunchProjectile(this);
    }

    [HarmonyPatch]
    internal static class Verb_ShootWithMode_Patch
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

                var methodInfoLaunchInterceptor = AccessTools.Method(typeof(Verb_ShootWithMode_Patch), nameof(LaunchInterceptor));

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
            if (verb is Verb_ShootWithMode verbShootExt)
            {
                projectile.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);

                var projectileCount = verbShootExt.EquipmentCompVerbModeSource?.CurrentVerbMode.projectilesPerShot;
                for (int i = 1; i < projectileCount; ++i)
                {
                    var subProjectile = (Projectile)GenSpawn.Spawn(projectile.def, projectile.Position, projectile.Map);
                    subProjectile.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
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
