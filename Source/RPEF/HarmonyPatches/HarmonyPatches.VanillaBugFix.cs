using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;

namespace RPEF
{
    public static partial class HarmonyPatches
    {
        public static void PatchVanillaBugFix(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(ChoiceLetter_BabyToChild), nameof(ChoiceLetter_BabyToChild.CanShowInLetterStack)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ChoiceLetter_BabyToChild_CanShowInLetterStack_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Pawn_FlightTracker), nameof(Pawn_FlightTracker.Notify_JobStarted)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_FlightTracker_Notify_JobStarted_Prefix)));

            var goToChairInitAction = typeof(JobDriver_FeedBaby)
                .GetNestedTypes(BindingFlags.NonPublic)
                .SelectMany(v => v.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                .FirstOrDefault(v => v.Name.Contains("<GoToChair>"));
            if (goToChairInitAction == null)
            {
                Log.Error($"[RPEF] Failed to find JobDriver_FeedBaby.GoToChair init action");
                return;
            }

            harmony.Patch(
                original: goToChairInitAction,
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(JobDriver_FeedBaby_GoToChair_Transpiler)));
        }

        // 아기→어린이 편지의 폰이 사라진 경우(파괴·세이브 참조 소실) 편지 목록 표시에서 null 참조 오류가 난다.
        private static bool ChoiceLetter_BabyToChild_CanShowInLetterStack_Prefix(ref bool __result, Pawn ___pawn)
        {
            if (___pawn == null)
            {
                __result = false;
                return false;
            }

            return true;
        }

        // 작업 정의가 없는 작업으로 Notify_JobStarted가 불리면 비행 추적기가 job.def에서 null 참조 오류를 낸다.
        private static bool Pawn_FlightTracker_Notify_JobStarted_Prefix(Job job)
        {
            return job?.def != null;
        }

        // 바닐라 아기 먹이기는 의자가 없으면 RCellFinder.SpotToStandDuringJob으로 설 자리를 고르고 바로 ReserveSittableOrSpot으로 예약한다.
        // 이 탐색은 목적지 예약만 보고 물리 예약은 보지 않아서, 다른 폰이 물리 예약만 한 칸(바닥에 누운 칸, 작업대 상호작용 칸 등)을 골라 예약 오류를 낸다.
        // 먹기(Toils_Ingest.TryFindChairOrSpot)처럼 그 자리를 예약할 수 있는 칸만 고르게 한다.
        private static IEnumerable<CodeInstruction> JobDriver_FeedBaby_GoToChair_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            var original = AccessTools.Method(typeof(RCellFinder), nameof(RCellFinder.SpotToStandDuringJob));
            var index = instructions.FindIndex(v => v.Calls(original));
            if (index >= 0)
            {
                instructions[index].operand = AccessTools.Method(typeof(HarmonyPatches), nameof(JobDriver_FeedBaby_SpotToStandDuringJob_Injection));
            }
            else
            {
                Log.Error($"[RPEF] Failed to find injection point for JobDriver_FeedBaby_GoToChair_Transpiler");
            }

            return instructions;
        }

        private static IntVec3 JobDriver_FeedBaby_SpotToStandDuringJob_Injection(Pawn pawn, Predicate<IntVec3> extraValidator, Region targetRegion)
        {
            return RCellFinder.SpotToStandDuringJob(pawn, c => pawn.CanReserveSittableOrSpot(c) && (extraValidator == null || extraValidator(c)), targetRegion);
        }
    }
}
