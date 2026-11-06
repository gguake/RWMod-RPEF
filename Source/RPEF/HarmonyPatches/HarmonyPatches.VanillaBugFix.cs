using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RPEF
{
    public static partial class HarmonyPatches
    {
        public static void PatchVanillaBugFix(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(JobDriver_LayDown), nameof(JobDriver_LayDown.TryMakePreToilReservations)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(JobDriver_LayDown_TryMakePreToilReservations_Postfix)));
        }

        // 바닐라 눕기 잡은 침대가 아닌 바닥 칸을 물리 예약만 하고 목적지 예약은 하지 않는다.
        // 아기 먹이기(JobDriver_FeedBaby.GoToChair)는 의자가 없으면 RCellFinder.SpotToStandDuringJob으로 설 자리를 고르는데,
        // 이 탐색은 목적지 예약만 보고 물리 예약은 보지 않아 바닥에 누운 폰의 칸을 골라 ReserveSittableOrSpot에서 예약 오류를 낸다.
        // 시작된 잡에 묶이도록 여기서 건다(잡 정리 때 바닐라가 잡 연결을 풀고, 다음 길 찾기 때 지운다).
        private static void JobDriver_LayDown_TryMakePreToilReservations_Postfix(JobDriver_LayDown __instance, bool __result)
        {
            if (!__result || __instance.Bed != null) { return; }

            var pawn = __instance.pawn;
            var job = __instance.job;
            // 바닐라가 바닥 칸을 물리 예약하는 경우와 같은 조건
            if (!job.targetA.Cell.IsValid || pawn.ageTracker.CurLifeStage.alwaysDowned || job.forceSleep) { return; }
            if (!pawn.Spawned) { return; }

            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
        }
    }
}
