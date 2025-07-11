using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Xml;
using Verse;
using Verse.AI;

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
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DynamicPawnRenderNodeSetup_Apparel_ProcessApparel)));

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

        private static void DynamicPawnRenderNodeSetup_Apparel_ProcessApparel(ref IEnumerable<(PawnRenderNode node, PawnRenderNode parent)> __result, PawnRenderTree tree, Apparel ap)
        {
            var result = new List<(PawnRenderNode node, PawnRenderNode parent)>();
            foreach (var tuple in __result)
            {
                if (tuple.node is PawnRenderNode_ApparelBase)
                {
                    PawnRenderNodeTagDef abstractParentApparelTagDef = tuple.node.Props.parentTagDef;
                    if (tuple.node.Props.parentTagDef == null)
                    {
                        abstractParentApparelTagDef = (ap.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || ap.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover) ?
                        PawnRenderNodeTagDefOf.ApparelHead :
                        PawnRenderNodeTagDefOf.ApparelBody;
                    }

                    PawnRenderNode parentNode = null;
                    if (abstractParentApparelTagDef != null)
                    {
                        tree.TryGetNodeByTag(abstractParentApparelTagDef, out parentNode);
                    }

                    result.Add((tuple.node, parentNode));
                }
                else
                {
                    result.Add(tuple);
                }
            }

            __result = result;
        }
    }
}
