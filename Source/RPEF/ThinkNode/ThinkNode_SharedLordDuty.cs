using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RPEF
{
    /// <summary>
    /// 듀티 행동을 복제 없이 실행하는 노드.
    /// 바닐라 ThinkNode_Duty는 해석될 때 모든 DutyDef의 행동 트리를 자기 밑에 복사한다. 그걸 MentalStateCritical처럼
    /// 여러 종족 트리가 가져다 쓰는 트리에 넣으면 트리마다 전체 듀티가 한 벌씩 더 복제되어 시작 로딩이 크게 늘어난다.
    /// 이 노드는 LordDuty 트리에 이미 있는 ThinkNode_Duty를 그대로 불러 쓴다.
    /// </summary>
    public class ThinkNode_SharedLordDuty : ThinkNode
    {
        private static ThinkNode_Duty _shared;

        public static ThinkNode_Duty Shared
        {
            get
            {
                if (_shared == null)
                {
                    _shared = DefDatabase<ThinkTreeDef>.GetNamed("LordDuty").thinkRoot.FirstNodeOfType<ThinkNode_Duty>(includeThis: true);
                }

                return _shared;
            }
        }

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            return Shared.TryIssueJobPackage(pawn, jobParams);
        }
    }

    /// <summary>
    /// 모든 트리 해석이 끝난 뒤 공유 듀티 노드를 ThinkNode_SharedLordDuty의 하위 노드로 걸어 둔다.
    /// 저장된 작업은 폰의 주 트리에서 저장 키로 작업 출처 노드를 다시 찾으므로(Job.ExposeData), 걸어 두지 않으면 불러올 때 못 찾는다.
    /// 해석 중에 걸면 바닐라가 공유 노드를 복사하거나 키를 다시 매기므로 해석이 끝난 뒤에 건다.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class SharedLordDutyLinker
    {
        static SharedLordDutyLinker()
        {
            var nodes = new List<ThinkNode_SharedLordDuty>();
            foreach (var def in DefDatabase<ThinkTreeDef>.AllDefsListForReading)
            {
                foreach (var node in def.thinkRoot.ThisAndChildrenRecursive)
                {
                    if (node is ThinkNode_SharedLordDuty sharedNode && sharedNode.subNodes.Count == 0)
                    {
                        nodes.Add(sharedNode);
                    }
                }
            }

            if (nodes.Count == 0)
            {
                return;
            }

            var shared = ThinkNode_SharedLordDuty.Shared;
            if (shared == null)
            {
                Log.Error("RPEF: LordDuty think tree has no ThinkNode_Duty. ThinkNode_SharedLordDuty will not work.");
                return;
            }

            foreach (var node in nodes)
            {
                node.subNodes.Add(shared);
            }
        }
    }
}
