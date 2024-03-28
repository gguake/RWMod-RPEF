using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RPEF
{
    public static class AppliedConstraintCache
    {
        private static Dictionary<string, List<Constraint>> _cache = 
            new Dictionary<string, List<Constraint>>();

        private static int _lastCacheRefreshTicks;

        public static List<Constraint> Get(Pawn pawn)
        {
            var key = pawn.ThingID;
            if (GenTicks.TicksGame > _lastCacheRefreshTicks)
            {
                _cache.Clear();
            }

            if (!_cache.TryGetValue(key, out var result))
            {
                result = GetAllAppliedConstraintsOfPawn(pawn).ToList();
                _cache.Add(key, result);
            }

            return result;
        }

        private static IEnumerable<Constraint> GetAllAppliedConstraintsOfPawn(Pawn pawn)
        {
            if (pawn == null) { yield break; }

            // Race
            foreach (var constraint in pawn.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
            {
                yield return constraint;
            }

            // Equipments
            var equipments = pawn.equipment?.AllEquipmentListForReading;
            if (equipments != null && equipments.Count > 0)
            {
                foreach (var equipment in equipments)
                {
                    foreach (var constraint in equipment.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
                    {
                        yield return constraint;
                    }
                }
            }

            // Apparels
            var apparels = pawn.apparel?.WornApparel;
            if (apparels != null && apparels.Count > 0)
            {
                foreach (var apparel in apparels)
                {
                    foreach (var constraint in apparel.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
                    {
                        yield return constraint;
                    }
                }
            }

            if (pawn.story != null)
            {
                // Backstory
                foreach (var constraint in pawn.story.Childhood.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
                {
                    yield return constraint;
                }
                foreach (var constraint in pawn.story.Adulthood.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
                {
                    yield return constraint;
                }

                // BodyType
                foreach (var constraint in pawn.story.bodyType.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
                {
                    yield return constraint;
                }

                // HeadType
                foreach (var constraint in pawn.story.headType.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
                {
                    yield return constraint;
                }

                // Hair
                foreach (var constraint in pawn.story.hairDef.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
                {
                    yield return constraint;
                }

                // Trait
                var traits = pawn.story.traits?.allTraits;
                if (traits != null && traits.Count > 0)
                {
                    foreach (var trait in traits)
                    {
                        foreach (var constraint in trait.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
                        {
                            yield return constraint;
                        }
                    }
                }
            }

            // Hediff
            var hediffs = pawn.health?.hediffSet?.hediffs;
            if (hediffs != null && hediffs.Count > 0)
            {
                foreach (var hediff in hediffs)
                {
                    foreach (var constraint in hediff.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
                    {
                        yield return constraint;
                    }
                }
            }

            if (pawn.style != null)
            {
                // Beard
                foreach (var constraint in pawn.style.beardDef.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
                {
                    yield return constraint;
                }

                // Tattoo
                foreach (var constraint in pawn.style.FaceTattoo.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
                {
                    yield return constraint;
                }
                foreach (var constraint in pawn.style.BodyTattoo.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
                {
                    yield return constraint;
                }
            }

            // Thought
            var memories = pawn.needs?.mood?.thoughts?.memories?.Memories;
            if (memories != null && memories.Count > 0)
            {
                foreach (var memory in memories)
                {
                    foreach (var constraint in memory.def.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
                    {
                        yield return constraint;
                    }
                }
            }


            // Xenotype
            if (pawn.genes != null)
            {
                foreach (var constraint in pawn.genes.Xenotype.GetAllConstraintsOfDef(ConstraintRuleFlag.OnAppliedPawn))
                {
                    yield return constraint;
                }
            }
        }

    }
}
