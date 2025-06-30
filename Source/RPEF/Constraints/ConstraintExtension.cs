using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RPEF
{
    public static class ConstraintExtension
    {
        private static Dictionary<Def, List<Constraint>> _defConstraintCache = new Dictionary<Def, List<Constraint>>();
        private static Dictionary<int, List<Constraint>> _pawnConstraintCache = new Dictionary<int, List<Constraint>>();
        private static GameInfo _curGameInfo;
        private static int _lastPawnCacheRefreshTicks;

        public static void ClearCache()
        {
            _defConstraintCache.Clear();
            _pawnConstraintCache.Clear();
            _curGameInfo = Find.GameInfo;
            _lastPawnCacheRefreshTicks = 0;
        }

        public static IEnumerable<Constraint> GetAllConstraintsOfDef(this Def def, ConstraintRuleFlag rule)
        {
            if (def == null) { yield break; }

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
                        yield return constraints[i];
                    }
                }
            }
        }

        public static IEnumerable<Constraint> GetAllConstraintsOfPawn(Pawn pawn)
        {
            if (pawn == null) { yield break; }

            if (Find.GameInfo != _curGameInfo || GenTicks.TicksGame != _lastPawnCacheRefreshTicks)
            {
                _pawnConstraintCache.Clear();

                _curGameInfo = Find.GameInfo;
                _lastPawnCacheRefreshTicks = GenTicks.TicksGame;
            }

            if (!_pawnConstraintCache.TryGetValue(pawn.thingIDNumber, out var result))
            {
                result = new List<Constraint>();

                // Race
                result.AddRange(pawn.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));

                // Equipments
                var equipments = pawn.equipment?.AllEquipmentListForReading;
                if (equipments != null && equipments.Count > 0)
                {
                    foreach (var equipment in equipments)
                    {
                        result.AddRange(equipment.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));
                    }
                }

                // Apparels
                var apparels = pawn.apparel?.WornApparel;
                if (apparels != null && apparels.Count > 0)
                {
                    foreach (var apparel in apparels)
                    {
                        result.AddRange(apparel.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));
                    }
                }

                if (pawn.story != null)
                {
                    // Backstory
                    if (pawn.story.Childhood != null)
                    {
                        result.AddRange(pawn.story.Childhood.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));
                    }
                    if (pawn.story.Adulthood != null)
                    {
                        result.AddRange(pawn.story.Adulthood.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));
                    }

                    // BodyType
                    result.AddRange(pawn.story.bodyType.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));

                    // HeadType
                    result.AddRange(pawn.story.headType.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));

                    // Hair
                    if (pawn.story.hairDef != null)
                    {
                        result.AddRange(pawn.story.hairDef.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));
                    }

                    // Trait
                    var traits = pawn.story.traits?.allTraits;
                    if (traits != null && traits.Count > 0)
                    {
                        foreach (var trait in traits)
                        {
                            result.AddRange(trait.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));
                        }
                    }
                }

                // Hediff
                var hediffs = pawn.health?.hediffSet?.hediffs;
                if (hediffs != null && hediffs.Count > 0)
                {
                    foreach (var hediff in hediffs)
                    {
                        result.AddRange(hediff.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));
                    }
                }

                if (pawn.style != null)
                {
                    // Beard
                    if (pawn.style.beardDef != null)
                    {
                        result.AddRange(pawn.style.beardDef.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));
                    }

                    // Tattoo
                    if (pawn.style.FaceTattoo != null)
                    {
                        result.AddRange(pawn.style.FaceTattoo.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));
                    }
                    if (pawn.style.BodyTattoo != null)
                    {
                        result.AddRange(pawn.style.BodyTattoo.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));
                    }
                }

                // Thought
                var memories = pawn.needs?.mood?.thoughts?.memories?.Memories;
                if (memories != null && memories.Count > 0)
                {
                    foreach (var memory in memories)
                    {
                        result.AddRange(memory.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));
                    }
                }

                // Xenotype
                if (pawn.genes?.Xenotype != null)
                {
                    result.AddRange(pawn.genes.Xenotype.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn));
                }

                if (result.Count == 0) { result = null; }

                _pawnConstraintCache.Add(pawn.thingIDNumber, result);
            }

            if (result == null) { yield break; }

            foreach (var constarint in result)
            {
                yield return constarint;
            }
        }

        public static bool CheckAllConstraints(this Def def, Pawn pawn, out Constraint failedConstraint)
        {
            foreach (var constraint in GetAllConstraintsOfDef(def, ConstraintRuleFlag.OnApplyPawn))
            {
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

            foreach (var constraint in GetAllConstraintsOfPawn(pawn))
            {
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
            foreach (var constraint in GetAllConstraintsOfDef(def, ConstraintRuleFlag.OnApplyPawn))
            {
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
            foreach (var constraint in pawnDef.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
            {
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
