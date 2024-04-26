using Verse;

namespace RPEF
{
    public class RPEFMod : Mod
    {
        public RPEFMod(ModContentPack content) : base(content)
        {
            HarmonyPatches.Patch();
        }
    }

    [StaticConstructorOnStartup]
    public static class RPEFLazyPatcher
    {
        static RPEFLazyPatcher()
        {
            HarmonyPatches.LazyPatch();
        }
    }
}
