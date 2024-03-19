using Verse;

namespace RPEF
{
    public class RaceExtMod : Mod
    {
        public RaceExtMod(ModContentPack content) : base(content)
        {
            HarmonyPatches.Patch();
        }
    }
}
