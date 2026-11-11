using RimWorld;
using System;
using UnityEngine;

namespace RPEF
{
    /// <summary>
    /// 더 이상 쓰지 말 것. HairDef + <see cref="HairScaleExtension"/>으로 대체.
    /// HairDef를 상속하면 Def.index가 다른 HairDef와 겹쳐 이념의 헤어 빈도·성별 표가 서로 덮어써진다.
    /// 이전 XML이 로드에 실패하지 않도록 클래스만 남겨둔다.
    /// </summary>
    [Obsolete("Use a plain HairDef with RPEF.HairScaleExtension. Subclassing HairDef makes Def.index collide with other hairs.")]
    public class ScaleableHairDef : HairDef
    {
        public Vector2 scale = Vector2.one;
    }
}
