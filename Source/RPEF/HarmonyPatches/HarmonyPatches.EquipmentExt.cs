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
        private static void PatchEquipmentExtension(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(JobGiver_ReactToCloseMeleeThreat), "TryGiveJob"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(JobGiver_ReactToCloseMeleeThreat_TryGiveJob_Transpiler)));
        }

        private static IEnumerable<CodeInstruction> JobGiver_ReactToCloseMeleeThreat_TryGiveJob_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
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
    }
}
