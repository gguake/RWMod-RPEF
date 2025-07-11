using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

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
                original: AccessTools.PropertyGetter(typeof(Pawn_FlightTracker), nameof(Pawn_FlightTracker.CanFlyNow)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_FlightTracker_CanFlyNow_getter_Postfix)));
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

        private static void Pawn_FlightTracker_CanFlyNow_getter_Postfix(Pawn ___pawn, ref bool __result)
        {
            if (__result && !___pawn.flight.Flying)
            {
                var modExtension = ___pawn.def.GetModExtension<HumanlikeFlyExtension>();
                if (modExtension != null)
                {
                    var data = modExtension.animationData.FirstOrDefault(v => v.lifeStage == ___pawn.ageTracker.CurLifeStage);
                    if (data != null)
                    {
                        if (modExtension.requireBodyParts != null)
                        {
                            for (int i = 0; i < ___pawn.health.hediffSet.hediffs.Count; ++i)
                            {
                                var hediff = ___pawn.health.hediffSet.hediffs[i];
                                if (hediff is Hediff_MissingPart hediffMissingPart && hediffMissingPart.Part != null)
                                {
                                    for (int j = 0; j < modExtension.requireBodyParts.Count; ++j)
                                    {
                                        if (modExtension.requireBodyParts[j] == hediffMissingPart.Part.untranslatedCustomLabel)
                                        {
                                            __result = false;
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


            }
        }
    }
}
