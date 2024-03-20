using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace RPEF
{
    public static class RestrictionPatches
    {
        public static void Patch(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(Pawn_StoryTracker), "TryGetRandomHeadFromSet"),
                transpiler: new HarmonyMethod(typeof(RestrictionPatches), nameof(Pawn_StoryTracker_TryGetRandomHeadFromSet_Transpiler)));
            harmony.Patch(__Pawn_StoryTracker_TryGetRandomHeadFromSet_CanUseHeadType,
                postfix: new HarmonyMethod(typeof(RestrictionPatches), nameof(Pawn_StoryTracker_TryGetRandomHeadFromSet_CanUseHeadType_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnStyleItemChooser), nameof(PawnStyleItemChooser.WantsToUseStyle)),
                postfix: new HarmonyMethod(typeof(RestrictionPatches), nameof(PawnStyleItemChooser_WantsToUseStyle_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GetBodyTypeFor)),
                postfix: new HarmonyMethod(typeof(RestrictionPatches), nameof(PawnGenerator_GetBodyTypeFor_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(ApparelProperties), nameof(ApparelProperties.PawnCanWear), new Type[] { typeof(Pawn), typeof(bool) }),
                postfix: new HarmonyMethod(typeof(RestrictionPatches), nameof(ApparelProperties_PawnCanWear_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), nameof(EquipmentUtility.CanEquip), new Type[] { typeof(Thing), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool) }),
                postfix: new HarmonyMethod(typeof(RestrictionPatches), nameof(EquipmentUtility_CanEquip_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.XenotypesAvailableFor)),
                postfix: new HarmonyMethod(typeof(RestrictionPatches), nameof(PawnGenerator_XenotypesAvailableFor_Postfix)));

            harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.FillBackstorySlotShuffled)),
                transpiler: new HarmonyMethod(typeof(RestrictionPatches), nameof(PawnBioAndNameGenerator_FillBackstorySlotShuffled_Transpiler)));

            harmony.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) }),
                prefix: new HarmonyMethod(typeof(RestrictionPatches), nameof(Pawn_HealthTracker_AddHediff_Prefix)));

            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemoryFast), new Type[] { typeof(ThoughtDef), typeof(Precept) }),
                prefix: new HarmonyMethod(typeof(RestrictionPatches), nameof(MemoryThoughtHandler_TryGainMemoryFast_Prefix_1)));

            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemoryFast), new Type[] { typeof(ThoughtDef), typeof(int), typeof(Precept) }),
                prefix: new HarmonyMethod(typeof(RestrictionPatches), nameof(MemoryThoughtHandler_TryGainMemoryFast_Prefix_2)));

            harmony.Patch(AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new Type[] { typeof(Thought_Memory), typeof(Pawn) }),
                prefix: new HarmonyMethod(typeof(RestrictionPatches), nameof(MemoryThoughtHandler_TryGainMemory_Prefix)));

            Log.Message($"[RaceExt] Restriction Patch Succeeded");
        }

        private static MethodInfo __Pawn_StoryTracker_TryGetRandomHeadFromSet_CanUseHeadType;
        private static IEnumerable<CodeInstruction> Pawn_StoryTracker_TryGetRandomHeadFromSet_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeInstructions = instructions.ToList();
            foreach (var inst in codeInstructions)
            {
                if (inst.opcode == OpCodes.Ldftn)
                {
                    __Pawn_StoryTracker_TryGetRandomHeadFromSet_CanUseHeadType = inst.operand as MethodInfo;
                    break;
                }
            }

            return codeInstructions;
        }

        private static void Pawn_StoryTracker_TryGetRandomHeadFromSet_CanUseHeadType_Postfix(ref bool __result, HeadTypeDef head, Pawn ___pawn)
        {
            if (__result)
            {
                if (head == null || ___pawn == null) { return; }

                if (!head.IsAllowedFor(___pawn) || !___pawn.IsAllowedFor(head))
                {
                    __result = false;
                }
            }
        }

        private static void PawnStyleItemChooser_WantsToUseStyle_Postfix(ref bool __result, Pawn pawn, StyleItemDef styleItemDef)
        {
            if (__result)
            {
                switch (styleItemDef)
                {
                    case HairDef hairDef:
                        if (!hairDef.IsAllowedFor(pawn) || !pawn.IsAllowedFor(hairDef))
                        {
                            __result = false;
                        }
                        break;

                    case TattooDef tattooDef:
                        if (!tattooDef.IsAllowedFor(pawn) || !pawn.IsAllowedFor(tattooDef))
                        {
                            __result = false;
                        }
                        break;

                    case BeardDef beardDef:
                        if (!beardDef.IsAllowedFor(pawn) || !pawn.IsAllowedFor(beardDef))
                        {
                            __result = false;
                        }
                        break;
                }
            }
        }

        private static void PawnGenerator_GetBodyTypeFor_Postfix(ref BodyTypeDef __result, Pawn pawn)
        {
            var extension = pawn.def.GetModExtension<RaceExtension>();
            if (extension == null) { return; }

            if (pawn.DevelopmentalStage.Adult())
            {
                switch (pawn.gender)
                {
                    case Gender.Male:
                        if (extension.fixedMaleBodyType != null)
                        {
                            __result = extension.fixedMaleBodyType;
                        }
                        break;

                    case Gender.Female:
                        if (extension.fixedFemaleBodyType != null)
                        {
                            __result = extension.fixedFemaleBodyType;
                        }
                        break;
                }
            }
        }

        private static Dictionary<ApparelProperties, ThingDef> _apparelPropertiesThingDefCache = new Dictionary<ApparelProperties, ThingDef>();
        private static void ApparelProperties_PawnCanWear_Postfix(ref bool __result, ApparelProperties __instance, Pawn pawn)
        {
            if (__result)
            {
                if (!_apparelPropertiesThingDefCache.TryGetValue(__instance, out var thingDef))
                {
                    foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        if (def.apparel == __instance)
                        {
                            thingDef = def;
                            _apparelPropertiesThingDefCache.Add(__instance, def);
                            break;
                        }
                    }
                }

                if (thingDef != null && (!thingDef.IsAllowedFor(pawn) || !pawn.IsAllowedFor(thingDef)))
                {
                    __result = false;
                    return;
                }
            }
        }

        private static void EquipmentUtility_CanEquip_Postfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason)
        {
            if (__result)
            {
                if (thing == null || pawn == null) { return; }

                if (!thing.def.IsAllowedFor(pawn) || !pawn.IsAllowedFor(thing.def))
                {
                    __result = false;
                }
            }
        }

        private static void PawnGenerator_XenotypesAvailableFor_Postfix(ref Dictionary<XenotypeDef, float> __result, PawnKindDef kind)
        {
            var keys = __result.Keys.ToList();
            foreach (var xenotype in keys)
            {
                if (!xenotype.IsAllowedFor(kind.race))
                {
                    __result.Remove(xenotype);
                    continue;
                }

                if (kind.race.modExtensions != null)
                {
                    foreach (var extension in kind.race.modExtensions)
                    {
                        if (extension is XenotypeConstraint constraint && !constraint.IsAllowedFor(xenotype))
                        {
                            __result.Remove(xenotype);
                            break;
                        }
                    }
                }
            }
        }

        private static List<BackstoryDef> RemoveNotCompatibleBackstories(List<BackstoryDef> backstoryDefs, Pawn pawn)
        {
            backstoryDefs.RemoveAll(def => !def.IsAllowedFor(pawn) || !pawn.IsAllowedFor(def));
            return backstoryDefs;
        }

        private static IEnumerable<CodeInstruction> PawnBioAndNameGenerator_FillBackstorySlotShuffled_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();
            var index = instructions.FindLastIndex(
                v => v.opcode == OpCodes.Call &&
                v.OperandIs(AccessTools.Method(typeof(GenCollection), nameof(GenCollection.TakeRandom)).MakeGenericMethod(typeof(BackstoryDef))));

            if (index < 0) { throw new NotImplementedException(); }

            var injectionIndex = index - 1;
            instructions.InsertRange(injectionIndex, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RestrictionPatches), nameof(RemoveNotCompatibleBackstories))),
            });

            return instructions;
        }

        private static bool Pawn_HealthTracker_AddHediff_Prefix(Pawn ___pawn, Hediff hediff)
        {
            if (hediff != null)
            {
                if (!hediff.def.IsAllowedFor(___pawn) || !___pawn.IsAllowedFor(hediff.def))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool MemoryThoughtHandler_TryGainMemoryFast_Prefix_1(Pawn ___pawn, ref ThoughtDef mem)
        {
            if (!mem.IsAllowedFor(___pawn) || !___pawn.IsAllowedFor(mem))
            {
                return false;
            }

            var extension = ___pawn.def.GetModExtension<RaceExtension>();
            if (extension != null && extension.ThoughtReplacer.TryGetValue(mem, out var replacedThoughtDef))
            {
                mem = replacedThoughtDef;
            }

            return true;
        }

        private static bool MemoryThoughtHandler_TryGainMemoryFast_Prefix_2(Pawn ___pawn, ref ThoughtDef mem)
        {
            if (!mem.IsAllowedFor(___pawn) || !___pawn.IsAllowedFor(mem))
            {
                return false;
            }

            var extension = ___pawn.def.GetModExtension<RaceExtension>();
            if (extension != null && extension.ThoughtReplacer.TryGetValue(mem, out var replacedThoughtDef))
            {
                mem = replacedThoughtDef;
            }

            return true;
        }

        private static bool MemoryThoughtHandler_TryGainMemory_Prefix(MemoryThoughtHandler __instance, Pawn ___pawn, ref Thought_Memory newThought, Pawn otherPawn)
        {
            if (!newThought.def.IsAllowedFor(___pawn) || !___pawn.IsAllowedFor(newThought.def))
            {
                return false;
            }

            var extension = ___pawn.def.GetModExtension<RaceExtension>();
            if (extension != null && extension.ThoughtReplacer.TryGetValue(newThought.def, out var replacedThoughtDef))
            {
                var replacedThought = ThoughtMaker.MakeThought(replacedThoughtDef, newThought.sourcePrecept);
                __instance.TryGainMemory(replacedThought, otherPawn);
                return false;
            }

            return true;
        }
    }
}
