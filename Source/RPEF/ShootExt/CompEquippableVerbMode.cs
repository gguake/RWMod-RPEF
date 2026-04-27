using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RPEF
{
    public class VerbMode
    {
        public string label;
        public string description;

        public string changeGizmoLabel;
        public string changeGizmoDescription;

        public string customLabel;

        [NoTranslate]
        public string uiIconPath;

        [Unsaved(false)]
        private Texture2D uiIconCached;

        public Texture2D UIIcon
        {
            get
            {
                if (uiIconCached == null && !uiIconPath.NullOrEmpty())
                {
                    uiIconCached = ContentFinder<Texture2D>.Get(uiIconPath);
                }
                return uiIconCached;
            }
        }

        public int swapTicks = 300;

        public int? shotsPerBurstOverride;
        public float? effectiveRangeOverride;
        public float? warmupTimeOverride;
        public ThingDef projectileDefOverride;

        public int projectilesPerShot = 1;

        public List<StatModifier> statOffsets;
        public List<StatModifier> statFactors;

        private Dictionary<StatDef, StatModifier> _statOffsetDict;
        public float GetStatOffset(StatDef stat)
        {
            if (_statOffsetDict == null)
            {
                _statOffsetDict = statOffsets?.ToDictionary(e => e.stat) ?? new Dictionary<StatDef, StatModifier>();
            }

            return _statOffsetDict.TryGetValue(stat, out var modifier) ? modifier.value : 0f;
        }


        private Dictionary<StatDef, StatModifier> _statFactorDict;
        public float GetStatFactor(StatDef stat)
        {
            if (_statFactorDict == null)
            {
                _statFactorDict = statFactors?.ToDictionary(e => e.stat) ?? new Dictionary<StatDef, StatModifier>();
            }

            return _statFactorDict.TryGetValue(stat, out var modifier) ? modifier.value : 1f;
        }
    }

    public class CompProperties_EquippableVerbMode : CompProperties
    {
        public List<VerbMode> modes;
        public bool transformLabelOnlyEquipped = true;

        public CompProperties_EquippableVerbMode()
        {
            compClass = typeof(CompEquippableVerbMode);
        }
    }

    public class CompEquippableVerbMode : CompEquippable
    {
        public CompProperties_EquippableVerbMode Props => (CompProperties_EquippableVerbMode)props;

        public VerbMode CurrentVerbMode => Props.modes[_verbModeIndex];

        private int _verbModeIndex;

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref _verbModeIndex, "verbModeIndex");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (_verbModeIndex < 0 || _verbModeIndex >= Props.modes.Count)
                {
                    _verbModeIndex = 0;
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetEquippedGizmosExtra()
        {
            if (Holder?.Drafted != true || Props.modes.NullOrEmpty() || Props.modes.Count < 2)
            {
                yield break;
            }

            var current = CurrentVerbMode;
            yield return new Command_Action
            {
                defaultLabel = current.label,
                defaultDesc = current.description,
                icon = current.UIIcon,
                action = () =>
                {
                    if (Props.modes.Count == 2)
                    {
                        ChangeMode((_verbModeIndex + 1) % 2);
                    }
                    else
                    {
                        var options = new List<FloatMenuOption>();
                        for (int i = 0; i < Props.modes.Count; i++)
                        {
                            if (i == _verbModeIndex)
                            {
                                continue;
                            }

                            int targetIndex = i;
                            var mode = Props.modes[i];
                            options.Add(new FloatMenuOption(
                                label: mode.changeGizmoLabel ?? mode.label,
                                action: () => ChangeMode(targetIndex),
                                iconTex: mode.UIIcon,
                                iconColor: Color.white)); ;
                        }

                        if (options.Count > 0)
                        {
                            Find.WindowStack.Add(new FloatMenu(options));
                        }
                    }
                },
            };
        }

        public override string TransformLabel(string label)
        {
            if (CurrentVerbMode.customLabel == null || 
                (Props.transformLabelOnlyEquipped && Holder == null))
            {
                return base.TransformLabel(label);
            }

            return string.Format(CurrentVerbMode.customLabel, base.TransformLabel(label));
        }

        public override float GetStatOffset(StatDef stat)
            => CurrentVerbMode.GetStatOffset(stat);

        public override float GetStatFactor(StatDef stat)
            => CurrentVerbMode.GetStatFactor(stat);

        private void ChangeMode(int modeIndex)
        {
            _verbModeIndex = modeIndex;

            var ticks = Props.modes[modeIndex].swapTicks;
            if (ticks <= 0)
            {
                return;
            }

            var pawn = Holder;
            if (pawn?.jobs != null)
            {
                var waitJob = JobMaker.MakeJob(JobDefOf.Wait, ticks, true);
                pawn.jobs.StartJob(waitJob, JobCondition.InterruptForced);
            }
        }
    }
}
