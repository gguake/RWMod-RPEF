using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RPEF
{
    public static class ConstraintExtension
    {
        private static Dictionary<Def, List<Constraint>> _defConstraintCache = new Dictionary<Def, List<Constraint>>();
        private static Dictionary<int, List<Constraint>> _pawnConstraintCache = new Dictionary<int, List<Constraint>>();
        private static int _lastPawnCacheRefreshTicks;

        public static void ClearCache()
        {
            _defConstraintCache.Clear();
            _pawnConstraintCache.Clear();
            _lastPawnCacheRefreshTicks = 0;
        }

        public static void GetAllConstraintsOfDef(this Def def, ConstraintRuleFlag rule, List<Constraint> outList)
        {
            if (def == null) { return; }

            if (!_defConstraintCache.TryGetValue(def, out var constraints))
            {
                if (def.modExtensions != null)
                {
                    for (int i = 0; i < def.modExtensions.Count; ++i)
                    {
                        if (def.modExtensions[i] is ConstraintModExtension ext)
                        {
                            if (ext.def != null && ext.def.constraints != null && ext.def.constraints.Count > 0)
                            {
                                if (constraints == null) { constraints = new List<Constraint>(ext.def.constraints.Count); }
                                constraints.AddRange(ext.def.constraints);
                            }

                            if (ext.defs != null)
                            {
                                foreach (var constraintDef in ext.defs)
                                {
                                    if (constraintDef != null && constraintDef.constraints != null && constraintDef.constraints.Count > 0)
                                    {
                                        if (constraints == null) { constraints = new List<Constraint>(constraintDef.constraints.Count); }
                                        constraints.AddRange(constraintDef.constraints);
                                    }
                                }
                            }
                        }
                    }
                }

                _defConstraintCache.Add(def, constraints);
            }

            if (constraints != null)
            {
                for (int i = 0; i < constraints.Count; ++i)
                {
                    if ((constraints[i].rule & rule) != ConstraintRuleFlag.None)
                    {
                        outList.Add(constraints[i]);
                    }
                }
            }
        }

        public static void GetAllConstraintsOfPawn(this Pawn pawn, List<Constraint> outList)
        {
            if (pawn == null) { return; }

            if (GenTicks.TicksGame != _lastPawnCacheRefreshTicks)
            {
                _pawnConstraintCache.Clear();

                _lastPawnCacheRefreshTicks = GenTicks.TicksGame;
            }

            if (!_pawnConstraintCache.TryGetValue(pawn.thingIDNumber, out var result))
            {
                result = new List<Constraint>();

                // Race
                pawn.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);

                // Equipments
                var equipments = pawn.equipment?.AllEquipmentListForReading;
                if (equipments != null && equipments.Count > 0)
                {
                    foreach (var equipment in equipments)
                    {
                        equipment.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);
                    }
                }

                // Apparels
                var apparels = pawn.apparel?.WornApparel;
                if (apparels != null && apparels.Count > 0)
                {
                    foreach (var apparel in apparels)
                    {
                        apparel.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);
                    }
                }

                if (pawn.story != null)
                {
                    // Backstory
                    if (pawn.story.Childhood != null)
                    {
                        pawn.story.Childhood.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);
                    }
                    if (pawn.story.Adulthood != null)
                    {
                        pawn.story.Adulthood.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);
                    }

                    // BodyType
                    pawn.story.bodyType.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);

                    // HeadType
                    pawn.story.headType.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);

                    // Hair
                    if (pawn.story.hairDef != null)
                    {
                        pawn.story.hairDef.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);
                    }

                    // Trait
                    var traits = pawn.story.traits?.allTraits;
                    if (traits != null && traits.Count > 0)
                    {
                        foreach (var trait in traits)
                        {
                            trait.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);
                        }
                    }
                }

                // Hediff
                var hediffs = pawn.health?.hediffSet?.hediffs;
                if (hediffs != null && hediffs.Count > 0)
                {
                    foreach (var hediff in hediffs)
                    {
                        hediff.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);
                    }
                }

                if (pawn.style != null)
                {
                    // Beard
                    if (pawn.style.beardDef != null)
                    {
                        pawn.style.beardDef.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);
                    }

                    // Tattoo
                    if (pawn.style.FaceTattoo != null)
                    {
                        pawn.style.FaceTattoo.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);
                    }
                    if (pawn.style.BodyTattoo != null)
                    {
                        pawn.style.BodyTattoo.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);
                    }
                }

                // Thought
                var memories = pawn.needs?.mood?.thoughts?.memories?.Memories;
                if (memories != null && memories.Count > 0)
                {
                    foreach (var memory in memories)
                    {
                        memory.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);
                    }
                }

                // Xenotype
                if (pawn.genes?.Xenotype != null)
                {
                    pawn.genes.Xenotype.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, result);
                }

                _pawnConstraintCache.Add(pawn.thingIDNumber, result);
            }

            outList.AddRange(result);
        }

        private static List<Constraint> _tmpConstraints = new List<Constraint>();
        public static bool CheckAllConstraints(this Def def, Pawn pawn, out Constraint failedConstraint)
        {
            _tmpConstraints.Clear();
            def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnApplyPawn, _tmpConstraints);
            for (int i = 0; i < _tmpConstraints.Count; ++i)
            {
                var constraint = _tmpConstraints[i];
                if (!constraint.Check(pawn))
                {
                    failedConstraint = constraint;
                    if (failedConstraint.failReason != null)
                    {
                        JobFailReason.Is(failedConstraint.failReason);
                    }

                    return false;
                }
            }

            _tmpConstraints.Clear();
            pawn.GetAllConstraintsOfPawn(_tmpConstraints);
            for (int i = 0; i < _tmpConstraints.Count; ++i)
            {
                var constraint = _tmpConstraints[i];
                if (!constraint.Check(def)) 
                {
                    failedConstraint = constraint;
                    if (failedConstraint.failReason != null)
                    {
                        JobFailReason.Is(failedConstraint.failReason);
                    }

                    return false;
                }
            }

            failedConstraint = null;
            return true;
        }

        public static bool CheckAllConstraints(this Def def, ThingDef pawnDef, out Constraint failedConstraint)
        {
            _tmpConstraints.Clear();
            def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnApplyPawn, _tmpConstraints);
            for (int i = 0; i < _tmpConstraints.Count; ++i)
            {
                var constraint = _tmpConstraints[i];
                if (!constraint.Check(pawnDef))
                {
                    failedConstraint = constraint;
                    if (failedConstraint.failReason != null)
                    {
                        JobFailReason.Is(failedConstraint.failReason);
                    }

                    return false;
                }
            }

            // Race
            _tmpConstraints.Clear();
            pawnDef.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn, _tmpConstraints);
            for (int i = 0; i < _tmpConstraints.Count; ++i)
            {
                var constraint = _tmpConstraints[i];
                if (!constraint.Check(def)) 
                {
                    failedConstraint = constraint;
                    if (failedConstraint.failReason != null)
                    {
                        JobFailReason.Is(failedConstraint.failReason);
                    }

                    return false;
                }
            }

            failedConstraint = null;
            return true;
        }
    }

}
