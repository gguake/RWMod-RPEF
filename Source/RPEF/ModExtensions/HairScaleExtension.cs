using UnityEngine;
using Verse;

namespace RPEF
{
    /// <summary>
    /// HairDef
    /// 크기가 다른 Hair 텍스쳐를 사용하고자 할때 사용. (PawnRenderNodeWorker_CustomHair 필요)
    ///
    /// HairDef를 상속한 Def 클래스로 만들면 안 된다. 상속한 Def는 DefDatabase&lt;HairDef&gt;와
    /// 자기 타입의 DefDatabase 양쪽에 들어가고, Def.index는 자기 타입 쪽 값으로 덮어써진다.
    /// 그러면 DefMap&lt;HairDef, ...&gt;(이념의 헤어 빈도·성별 표)에서 다른 헤어와 같은 칸을 쓰게 된다.
    /// </summary>
    public class HairScaleExtension : DefModExtension
    {
        public Vector2 scale = Vector2.one;
    }
}
