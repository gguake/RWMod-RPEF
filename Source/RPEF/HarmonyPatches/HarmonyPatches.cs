using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RPEF
{
    public static partial class HarmonyPatches
    {
        private static Harmony harmony;

        public static void Patch()
        {
            harmony = new Harmony("rimworld.gguake.rpef");
            try
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(ApparelGraphicRecordGetter), nameof(ApparelGraphicRecordGetter.TryGetGraphicApparel)),
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ApparelGraphicRecordGetter_TryGetGraphicApparel_Prefix)));

                harmony.Patch(
                    original: AccessTools.Method(typeof(BillUtility), "MakeNewBill"),
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(BillUtility_MakeNewBill_Prefix)));

                harmony.Patch(
                    original: AccessTools.PropertyGetter(typeof(Settlement), nameof(Settlement.MapGeneratorDef)),
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Settlement_MapGeneratorDef_getter_Postfix)));

                harmony.Patch(
                    original: AccessTools.PropertyGetter(typeof(Dialog_ChooseNewWanderers), "DefaultStartingPawnRequest"),
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Dialog_ChooseNewWanderers_DefaultStartingPawnRequest_getter_Postfix)));

                harmony.Patch(
                    original: AccessTools.Method(typeof(DynamicPawnRenderNodeSetup_Apparel), "ProcessApparel"),
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DynamicPawnRenderNodeSetup_Apparel_ProcessApparel_Postfix)));

                //harmony.Patch(
                //    original: AccessTools.Method(typeof(PawnRenderNode_Hair), nameof(PawnRenderNode_Hair.MeshSetFor)),
                //    transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnRenderNode_Hair_MeshSetFor_Transpiler)));

                PatchRace(harmony);
                PatchXML(harmony);
                PatchHumanlikeFlyExtension(harmony);
                PatchRestrictions(harmony);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                Log.Error("[RPEF] Some error occured in patch process..");
                throw;
            }
        }

        public static void LazyPatch()
        {
            try
            {
                LazyPatchRestrictions(harmony);

                harmony.PatchAll();
                Log.Message($"[RPEF] Patch Completed");
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                Log.Error("[RPEF] Some error occured in lazy patch process..");
            }
        }

        private static bool ApparelGraphicRecordGetter_TryGetGraphicApparel_Prefix(Apparel apparel, ref BodyTypeDef bodyType)
        {
            var hook = apparel.def.GetModExtension<ApparelGraphicHook>();
            if (hook == null) { return true; }

            if (hook.bodyTypeGraphicOverride != null)
            {
                foreach (var v in hook.bodyTypeGraphicOverride)
                {
                    if (v.from == bodyType)
                    {
                        bodyType = v.to;
                        return true;
                    }
                }
            }

            if (hook.defaultBodyTypeGraphicOverride != null)
            {
                bodyType = hook.defaultBodyTypeGraphicOverride;
            }

            return true;
        }


        private static bool BillUtility_MakeNewBill_Prefix(ref Bill __result, RecipeDef recipe, Precept_ThingStyle precept)
        {
            var extension = recipe.GetModExtension<RecipeBillHook>();
            if (extension == null) { return true; }

            if (extension.billOverrideType != null)
            {
                if (!typeof(Bill).IsAssignableFrom(extension.billOverrideType))
                {
                    Log.Error($"invalid billtype for recipe mod extension");
                    return true;
                }

                var bill = Activator.CreateInstance(extension.billOverrideType, new object[] { recipe, precept }) as Bill;
                if (bill != null)
                {
                    __result = bill;
                    return false;
                }
            }

            return true;
        }

        private static void Settlement_MapGeneratorDef_getter_Postfix(ref MapGeneratorDef __result, Settlement __instance)
        {
            if (__instance.Faction != null)
            {
                var extension = __instance.Faction.def.GetModExtension<FactionSettlementHook>();
                if (extension != null && extension.mapGeneratorDef != null)
                {
                    __result = extension.mapGeneratorDef;
                }
            }
        }

        private static void Dialog_ChooseNewWanderers_DefaultStartingPawnRequest_getter_Postfix(ref PawnGenerationRequest __result)
        {
            var extension = Faction.OfPlayer.def.GetModExtension<FactionContinuePawnKindHook>();
            if (extension != null && extension.continueMemberKindOverride != null)
            {
                __result.KindDef = extension.continueMemberKindOverride;
            }
        }

        private static FieldInfo _field_PawnRenderTree_nodesByTag = AccessTools.Field(typeof(PawnRenderTree), "nodesByTag");
        private static void DynamicPawnRenderNodeSetup_Apparel_ProcessApparel_Postfix(
            ref IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> __result, 
            Pawn pawn, 
            PawnRenderTree tree)
        {
            var result = new List<(PawnRenderNode node, PawnRenderNode parent)>();
            foreach (var tuple in __result)
            {
                if (tuple.node is PawnRenderNode_ApparelBase)
                {
                    var nodesByTag = _field_PawnRenderTree_nodesByTag.GetValue(tree) as Dictionary<PawnRenderNodeTagDef, PawnRenderNode>;
                    var apparelWornIndex = pawn.apparel.WornApparel.IndexOf(tuple.node.apparel);

                    var abstractParentApparelTagDef = tuple.node.Props.parentTagDef;
                    if (tuple.node.Props.parentTagDef == null)
                    {
                        abstractParentApparelTagDef = (tuple.node.apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || tuple.node.apparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover) ?
                            PawnRenderNodeTagDefOf.ApparelHead :
                            PawnRenderNodeTagDefOf.ApparelBody;
                    }

                    PawnRenderNode parentNode = null;
                    if (abstractParentApparelTagDef != null)
                    {
                        if (!tree.TryGetNodeByTag(abstractParentApparelTagDef, out parentNode))
                        {
                            parentNode = nodesByTag[abstractParentApparelTagDef];
                        }
                    }

                    int layerOffset = 0;
                    for (int i = 0; i < apparelWornIndex; ++i)
                    {
                        var wornApparel = pawn.apparel.WornApparel[i];
                        if (wornApparel.def.IsApparel)
                        {
                            var parentTagDef = wornApparel.def.apparel.parentTagDef;
                            if (parentTagDef == null)
                            {
                                parentTagDef = (wornApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || wornApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover) ?
                                    PawnRenderNodeTagDefOf.ApparelHead :
                                    PawnRenderNodeTagDefOf.ApparelBody;
                            }

                            if (abstractParentApparelTagDef == parentTagDef)
                            {
                                layerOffset++;
                            }
                        }
                    }

                    tuple.node.Props.baseLayer = (parentNode?.Props.baseLayer ?? 0) + layerOffset;
                    result.Add((tuple.node, parentNode));
                }
                else
                {
                    result.Add(tuple);
                }
            }

            __result = result;
        }

        //private static IEnumerable<CodeInstruction> PawnRenderNode_Hair_MeshSetFor_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilGenerator)
        //{
        //    var instructions = codeInstructions.ToList();

        //    var index = instructions.FirstIndexOf(v => v.opcode == OpCodes.Call && v.OperandIs(AccessTools.Method(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn))));
        //    if (index >= 0)
        //    {
        //        var localScale = ilGenerator.DeclareLocal(typeof(Vector2));
        //        var labelJump = ilGenerator.DefineLabel();
        //        var labelSkip = ilGenerator.DefineLabel();
        //        var injectionIndex = index - 2;

        //        instructions[injectionIndex] = instructions[injectionIndex].WithLabels(labelSkip);
        //        instructions[index] = instructions[index].WithLabels(labelJump);
        //        instructions.InsertRange(injectionIndex, new CodeInstruction[]
        //        {
        //            new CodeInstruction(OpCodes.Ldarg_1),
        //            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), nameof(Pawn.story))),
        //            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.hairDef))),
        //            new CodeInstruction(OpCodes.Isinst, typeof(ScaleableHairDef)),
        //            new CodeInstruction(OpCodes.Brfalse_S, labelSkip),

        //            new CodeInstruction(OpCodes.Ldarg_1),
        //            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn), nameof(Pawn.story))),
        //            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.hairDef))),
        //            new CodeInstruction(OpCodes.Castclass, typeof(ScaleableHairDef)),
        //            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ScaleableHairDef), nameof(ScaleableHairDef.scale))),
        //            new CodeInstruction(OpCodes.Stloc_S, localScale.LocalIndex),

        //            new CodeInstruction(OpCodes.Ldloc_S, localScale.LocalIndex),
        //            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Vector2), nameof(Vector2.x))),

        //            new CodeInstruction(OpCodes.Ldloc_S, localScale.LocalIndex),
        //            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Vector2), nameof(Vector2.y))),
        //            new CodeInstruction(OpCodes.Br_S, labelJump),
        //        });
        //    }

        //    return instructions;
        //}
    }
}
