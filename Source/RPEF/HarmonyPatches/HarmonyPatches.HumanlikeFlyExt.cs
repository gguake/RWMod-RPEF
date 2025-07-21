using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace RPEF
{
    public static partial class HarmonyPatches
    {
        private static void PatchHumanlikeFlyExtension(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Pawn_FlightTracker), nameof(Pawn_FlightTracker.GetBestFlyAnimation)),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_FlightTracker_GetBestFlyAnimation_Transpiler)));

            harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(Pawn_FlightTracker), nameof(Pawn_FlightTracker.CanEverFly)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_FlightTracker_CanEverFly_getter_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Pawn_FlightTracker), nameof(Pawn_FlightTracker.Notify_JobStarted)),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_FlightTracker_Notify_JobStarted_Transpiler)));
        }

        private static AnimationDef Pawn_FlightTracker_GetBestFlyAnimation_Injection(Pawn pawn)
        {
            var modExtension = pawn.def.GetModExtension<HumanlikeFlyExtension>();
            if (modExtension != null)
            {
                var data = modExtension.GetAnimationData(pawn.ageTracker.CurLifeStage);
                if (data != null)
                {
                    var rot = pawn.Rotation;
                    if (rot == Rot4.South)
                    {
                        return data.animationSouth;
                    }
                    else if (rot == Rot4.North)
                    {
                        return data.animationNorth;
                    }
                    else if (rot == Rot4.East)
                    {
                        return data.animationEast;
                    }
                    else
                    {
                        return data.animationWest;
                    }
                }
            }

            return null;
        }

        private static IEnumerable<CodeInstruction> Pawn_FlightTracker_GetBestFlyAnimation_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            var index = instructions.FindIndex(
                v =>
                v.opcode == OpCodes.Callvirt &&
                v.OperandIs(AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.Humanlike))));

            if (index >= 0)
            {
                instructions.RemoveAt(index + 2);
                instructions.RemoveAt(index + 2);
                instructions.InsertRange(index + 2, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(Pawn_FlightTracker_GetBestFlyAnimation_Injection))),
                    new CodeInstruction(OpCodes.Ret),
                });

            }
            else
            {
                Log.Error($"[RPEF] Failed to find injection point for Pawn_FlightTracker_GetBestFlyAnimation_Transpiler");
            }

            return instructions;
        }

        private static void Pawn_FlightTracker_CanEverFly_getter_Postfix(Pawn ___pawn, ref bool __result)
        {
            if (__result)
            {
                var modExtension = ___pawn.def.GetModExtension<HumanlikeFlyExtension>();
                if (modExtension != null && !modExtension.CheckCanEverFly(___pawn))
                {
                    __result = false;
                }
            }
        }

        private static IEnumerable<CodeInstruction> Pawn_FlightTracker_Notify_JobStarted_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilGenerator)
        {
            var instructions = codeInstructions.ToList();
            var labelInjection = ilGenerator.DefineLabel();
            var labelSkip = ilGenerator.DefineLabel();
            var labelNullableSkip = ilGenerator.DefineLabel();
            var labelNullableNull = ilGenerator.DefineLabel();
            var labelFlyingJump = ilGenerator.DefineLabel();

            {
                var index = instructions.FindIndex(
                    v => v.opcode == OpCodes.Call &&
                    v.OperandIs(AccessTools.PropertyGetter(typeof(Pawn_FlightTracker), nameof(Pawn_FlightTracker.CanEverFly))));

                if (index >= 0)
                {
                    instructions[index + 1].operand = labelInjection;
                }
                else
                {
                    Log.Error($"[RPEF] Failed to find injection point for Pawn_FlightTracker_GetBestFlyAnimation_Transpiler");
                }
            }

            {
                var index = instructions.FindIndex(v => v.opcode == OpCodes.Ret);
                if (index >= 0)
                {
                    var injectionIndex = index + 1;

                    instructions[injectionIndex] = instructions[injectionIndex].WithLabels(labelSkip);
                    instructions.InsertRange(injectionIndex, new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0).WithLabels(labelInjection),
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_FlightTracker), "flightCooldownTicks")),

                        new CodeInstruction(OpCodes.Ldc_I4_0),
                        new CodeInstruction(OpCodes.Bgt_Un_S, labelSkip),

                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_FlightTracker), "pawn")),
                        new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.Drafted))),
                        new CodeInstruction(OpCodes.Brfalse_S, labelSkip),

                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_FlightTracker), "pawn")),
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Thing), nameof(Thing.def))),
                        new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Def), nameof(Def.GetModExtension), generics: new Type[] { typeof(HumanlikeFlyExtension) })),
                        new CodeInstruction(OpCodes.Dup),
                        
                        new CodeInstruction(OpCodes.Brtrue_S, labelNullableSkip),

                        new CodeInstruction(OpCodes.Pop),
                        new CodeInstruction(OpCodes.Ldc_I4_0),

                        new CodeInstruction(OpCodes.Br_S, labelNullableNull),

                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(HumanlikeFlyExtension), nameof(HumanlikeFlyExtension.alwaysFlyIfDrafted))).WithLabels(labelNullableSkip),
                        new CodeInstruction(OpCodes.Brfalse_S, labelSkip).WithLabels(labelNullableNull),

                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Pawn_FlightTracker), nameof(Pawn_FlightTracker.Flying))),
                        new CodeInstruction(OpCodes.Brtrue_S, labelFlyingJump),

                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Pawn_FlightTracker), "StartFlyingInternal")),

                        new CodeInstruction(OpCodes.Ldarg_1).WithLabels(labelFlyingJump),
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(Job), nameof(Job.flying))),
                        new CodeInstruction(OpCodes.Ret),
                    });
                }
                else
                {
                    Log.Error($"[RPEF] Failed to find injection point for Pawn_FlightTracker_GetBestFlyAnimation_Transpiler");
                }
            }

            return instructions;
        }

    }
}
