using RimWorld;
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
    }
}
