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
        private Texture2D _uiIconCached;
        public Texture2D UIIcon
        {
            get
            {
                if (_uiIconCached == null && !uiIconPath.NullOrEmpty())
                {
                    _uiIconCached = ContentFinder<Texture2D>.Get(uiIconPath);
                }
                return _uiIconCached;
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

        /// <summary>
        /// 무기가 3개 이상의 모드를 지원하는 경우 모드 변경 UI Gizmo의 Label
        /// 모드가 2개인 경우 변경할 모드의 정보가 출력됨
        /// </summary>
        public string changeModeGizmoLabel;

        /// <summary>
        /// 무기가 3개 이상의 모드를 지원하는 경우 모드 변경 UI Gizmo의 Description
        /// 모드가 2개인 경우 변경할 모드의 정보가 출력됨
        /// </summary>
        public string changeModeGizmoDescription;

        /// <summary>
        /// 무기가 3개 이상의 모드를 지원하는 경우 모드 변경 UI Gizmo의 아이콘 경로
        /// 모드가 2개인 경우 변경할 모드의 정보가 출력됨
        /// </summary>
        [NoTranslate]
        public string changeModeGizmoUIIconPath;

        [Unsaved(false)]
        private Texture2D _changeModeGizmoUIIconCached;
        public Texture2D ChangeModeGizmoUIIcon
        {
            get
            {
                if (_changeModeGizmoUIIconCached == null && !changeModeGizmoUIIconPath.NullOrEmpty())
                {
                    _changeModeGizmoUIIconCached = ContentFinder<Texture2D>.Get(changeModeGizmoUIIconPath);
                }
                return _changeModeGizmoUIIconCached;
            }
        }

        /// <summary>
        /// 무기의 Label 변경 기능을 장착한 상태에서만 적용할지 여부
        /// </summary>
        public bool transformLabelOnlyEquipped = true;

        public CompProperties_EquippableVerbMode()
        {
            compClass = typeof(CompEquippableVerbMode);
        }
    }

    public class CompEquippableVerbMode : CompEquippable
    {
        public CompProperties_EquippableVerbMode Props => (CompProperties_EquippableVerbMode)props;

        public int CurrentVerbModeIndex => _verbModeIndex;
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

            yield return new Command_EquippableVerbMode(this);
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

        public void ChangeMode(int modeIndex)
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

    public class Command_EquippableVerbMode : Command
    {
        private CompEquippableVerbMode PrimaryComp => _comps[0];
        private List<CompEquippableVerbMode> _comps;

        public Command_EquippableVerbMode(CompEquippableVerbMode comp)
        {
            _comps = new List<CompEquippableVerbMode>() { comp };

            groupable = true;
            groupKey = comp.parent.def.defNameHash;

            icon = PrimaryComp.Props.modes.Count > 2 ? PrimaryComp.Props.ChangeModeGizmoUIIcon : PrimaryComp.Props.modes[(PrimaryComp.CurrentVerbModeIndex + 1) % 2].UIIcon;
        }

        public override string Label
        {
            get
            {
                if (PrimaryComp.Props.modes.Count > 2)
                {
                    return PrimaryComp.Props.changeModeGizmoLabel;
                }
                else
                {
                    var mode = PrimaryComp.Props.modes[(PrimaryComp.CurrentVerbModeIndex + 1) % 2];
                    return mode.changeGizmoLabel ?? mode.label ?? base.Label;
                }
            }
        }

        public override string Desc
        {
            get
            {
                if (PrimaryComp.Props.modes.Count > 2)
                {
                    return PrimaryComp.Props.changeModeGizmoDescription;
                }
                else
                {
                    var mode = PrimaryComp.Props.modes[(PrimaryComp.CurrentVerbModeIndex + 1) % 2];
                    return mode.changeGizmoDescription ?? mode.description ?? base.Desc;
                }
            }
        }

        public override bool GroupsWith(Gizmo other)
        {
            if (_comps.Count == 0)
            {
                return false;
            }

            if (base.GroupsWith(other) && other is Command_EquippableVerbMode otherCommand)
            {
                if (otherCommand._comps.Count > 0 && PrimaryComp.parent.def == otherCommand.PrimaryComp.parent.def)
                {
                    // 모드가 2개면 동일한 모드끼리만 묶고, 3개 이상이면 전부 하나로 표시한다.
                    if (PrimaryComp.Props.modes.Count > 2 || PrimaryComp.CurrentVerbModeIndex == otherCommand.PrimaryComp.CurrentVerbModeIndex)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void MergeWith(Gizmo other)
        {
            if (other is Command_EquippableVerbMode otherCommand)
            {
                _comps.AddRange(otherCommand._comps);
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);

            var verbModeIndex = PrimaryComp.CurrentVerbModeIndex;
            if (PrimaryComp.Props.modes.Count == 2)
            {
                var nextModeIndex = (verbModeIndex + 1) % 2;
                for (int i = 0; i < _comps.Count; ++i)
                {
                    _comps[i].ChangeMode(nextModeIndex);
                }
            }
            else
            {
                var options = new List<FloatMenuOption>();
                for (int i = 0; i < PrimaryComp.Props.modes.Count; i++)
                {
                    if (_comps.Count == 1 && verbModeIndex == i) { continue; }

                    var mode = PrimaryComp.Props.modes[i];

                    options.Add(new FloatMenuOption(
                        label: mode.changeGizmoLabel ?? mode.label,
                        action: () => {
                            for (int j = 0; j < _comps.Count; ++j)
                            {
                                _comps[j].ChangeMode(i);
                            }
                        },
                        mouseoverGuiAction: mode.changeGizmoDescription != null ? (rect) => TooltipHandler.TipRegion(rect, mode.changeGizmoDescription) : null));
                }

                if (options.Count > 0)
                {
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }
        }
    }
}
