using RimWorld;
using UnityEngine;

namespace RPEF
{
    /// <summary>
    /// 크기가 다른 Head 텍스쳐를 사용하고자 할때 사용.
    /// </summary>
    public class ScaleableHairDef : HairDef
    {
        public Vector2 scale = Vector2.one;
    }
}
