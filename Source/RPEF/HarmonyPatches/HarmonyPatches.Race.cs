using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace RPEF
{
    public static partial class HarmonyPatches
    {
        private static void PatchRace(Harmony harmony)
        {
            #region 폰 종류 교체 허용 범위
            {
                harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new Type[] { typeof(PawnGenerationRequest) }),
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnGenerator_GeneratePawn_Prefix)),
                    finalizer: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnGenerator_GeneratePawn_Finalizer)));

                PatchPawnKindReplaceScope(harmony,
                    AccessTools.DeclaredMethod(
                        typeof(PrisonerWillingToJoinQuestUtility), 
                        nameof(PrisonerWillingToJoinQuestUtility.GeneratePrisoner), 
                        new Type[] { typeof(PlanetTile), typeof(Faction) }),
                    "PrisonerWillingToJoinQuestUtility.GeneratePrisoner");

                PatchPawnKindReplaceScope(harmony,
                    AccessTools.DeclaredMethod(typeof(RimWorld.QuestGen.QuestNode_Root_Hospitality_Refugee), "RunInt"),
                    "QuestNode_Root_Hospitality_Refugee.RunInt");

                PatchPawnKindReplaceScope(harmony,
                    AccessTools.DeclaredMethod(
                        typeof(RimWorld.QuestGen.QuestNode_Root_WandererJoinAbasia), 
                        nameof(RimWorld.QuestGen.QuestNode_Root_WandererJoinAbasia.GeneratePawn_NewTemp), 
                        new Type[] { typeof(Map) }),
                    "QuestNode_Root_WandererJoinAbasia.GeneratePawn_NewTemp");

                PatchPawnKindReplaceScope(harmony,
                    AccessTools.DeclaredMethod(
                        typeof(ThingSetMaker_MapGen_AncientPodContents), 
                        "GenerateSlave_NewTemp", 
                        new Type[] { typeof(ThingSetMakerParams) }),
                    "ThingSetMaker_MapGen_AncientPodContents.GenerateSlave_NewTemp");

                PatchPawnKindReplaceScope(harmony,
                    AccessTools.DeclaredMethod(
                        typeof(RimWorld.BaseGen.SymbolResolver_TerrorBuildings), 
                        "SpawnBuildings", 
                        new Type[]
                        {
                            typeof(List<IntVec3>), typeof(int), typeof(Map), typeof(RimWorld.BaseGen.ResolveParams), typeof(List<IntVec3>), typeof(List<ThingDef>),
                        }),
                    "SymbolResolver_TerrorBuildings.SpawnBuildings");

                // 지연 열거자라 호출 시점이 아니라 MoveNext 안에서 폰이 만들어진다. 결과를 감싸 MoveNext 동안만 허용한다.
                var slaveStockMethod = AccessTools.DeclaredMethod(typeof(StockGenerator_Slaves), nameof(StockGenerator_Slaves.GenerateThings), new Type[] { typeof(PlanetTile), typeof(Faction) });
                if (slaveStockMethod != null)
                {
                    harmony.Patch(slaveStockMethod,
                        postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(StockGenerator_Slaves_GenerateThings_Postfix)));
                }
                else
                {
                    Log.Warning("[RPEF] Failed to find StockGenerator_Slaves.GenerateThings for pawn kind replace scope");
                }
            }
            #endregion

            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "GenerateGenes"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnGenerator_GenerateGenes_Prefix)));

            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GetBodyTypeFor)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnGenerator_GetBodyTypeFor_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(PawnBioAndNameGenerator), "GiveShuffledBioTo"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnBioAndNameGenerator_GiveShuffledBioTo_Transpiler)));
            harmony.Patch(
                original: AccessTools.Method(typeof(PawnBioAndNameGenerator), "TryGiveSolidBioTo"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnBioAndNameGenerator_TryGiveSolidBioTo_Transpiler)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Gizmo_GrowthTier), "GrowthTierTooltip"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(Gizmo_GrowthTier_GrowthTierTooltip_Transpiler)));
            harmony.Patch(
                original: AccessTools.Method(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.TryChildGrowthMoment)),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_AgeTracker_TryChildGrowthMoment_Transpiler)));

            harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), "GrowthPointsFactor"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_AgeTracker_GrowthPointsFactor_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Pawn), nameof(Pawn.Sterile)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_Sterile_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.RandomSelectionWeight)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(InteractionWorker_RomanceAttempt_RandomSelectionWeight_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.SuccessChance)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(InteractionWorker_RomanceAttempt_SuccessChance_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(StunHandler), "CanBeStunnedByDamage"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(StunHandler_CanBeStunnedByDamage_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(StunHandler), "CanAdaptToDamage"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(StunHandler_CanAdaptToDamage_Postfix)));

            #region 조각상 관련
            harmony.Patch(
                original: AccessTools.Method(typeof(CompStatue), "CreateSnapshotOfPawn_HookForMods"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompStatue_CreateSnapshotOfPawn_HookForMods_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(CompStatue), "InitFakePawn"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompStatue_InitFakePawn_Transpiler)));
            #endregion

            harmony.Patch(
                original: AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.AdjustXenotypeForFactionlessPawn)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnGenerator_AdjustXenotypeForFactionlessPawn_Prefix)));

            #region 유전자 가리기 옵션
            harmony.Patch(
                original: AccessTools.Method(typeof(ITab_Genes), nameof(ITab_Genes.CanShowGenesTab)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ITab_Genes_CanShowGenesTab_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(CharacterCardUtility), "DoTopStack"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CharacterCardUtility_DoTopStack_Transpiler)));
            #endregion
        }

        #region 폰 종류 교체 허용 범위
        // 종류 교체(PawnGeneratorKindHook)는 노예·난민을 새로 만드는 바닐라 경로 안에서만 일어나게 한다.
        // 기존 폰의 kindDef로 다시 만드는 경로(복제, 출산, 친척 생성 등)에서는 교체하지 않는다.
        [ThreadStatic]
        private static int pawnKindReplaceAllowScope;

        // GeneratePawn 중첩 깊이. 친척 생성처럼 GeneratePawn 안에서 다시 부르는 생성은 교체하지 않는다.
        [ThreadStatic]
        private static int generatePawnDepth;

        private static void PatchPawnKindReplaceScope(Harmony harmony, MethodInfo method, string methodName)
        {
            if (method == null)
            {
                Log.Warning($"[RPEF] Failed to find {methodName} for pawn kind replace scope");
                return;
            }

            harmony.Patch(method,
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnKindReplaceScope_Prefix)),
                finalizer: new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnKindReplaceScope_Finalizer)));
        }

        private static void PawnKindReplaceScope_Prefix(out int __state)
        {
            __state = pawnKindReplaceAllowScope;
            pawnKindReplaceAllowScope = __state + 1;
        }

        private static void PawnKindReplaceScope_Finalizer(int __state)
        {
            pawnKindReplaceAllowScope = __state;
        }

        private static void StockGenerator_Slaves_GenerateThings_Postfix(ref IEnumerable<Thing> __result)
        {
            if (__result != null)
            {
                __result = new PawnKindReplaceScopedEnumerable<Thing>(__result);
            }
        }

        /// <summary>
        /// 원본 열거자의 MoveNext 동안만 종류 교체를 허용한다. 소비자가 항목 사이에 실행하는 코드는 범위 밖이다.
        /// </summary>
        private class PawnKindReplaceScopedEnumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> source;

            public PawnKindReplaceScopedEnumerable(IEnumerable<T> source)
            {
                this.source = source;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new Enumerator(source.GetEnumerator());
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private class Enumerator : IEnumerator<T>
            {
                private readonly IEnumerator<T> inner;

                public Enumerator(IEnumerator<T> inner)
                {
                    this.inner = inner;
                }

                public T Current => inner.Current;

                object System.Collections.IEnumerator.Current => inner.Current;

                public bool MoveNext()
                {
                    var prevScope = pawnKindReplaceAllowScope;
                    pawnKindReplaceAllowScope = prevScope + 1;
                    try
                    {
                        return inner.MoveNext();
                    }
                    finally
                    {
                        pawnKindReplaceAllowScope = prevScope;
                    }
                }

                public void Reset()
                {
                    inner.Reset();
                }

                public void Dispose()
                {
                    inner.Dispose();
                }
            }
        }
        #endregion

        private static bool PawnGenerator_GeneratePawn_Prefix(ref PawnGenerationRequest request, out int __state)
        {
            __state = generatePawnDepth;
            generatePawnDepth = __state + 1;

            // 허용 범위 안의 가장 바깥 생성에서만, 신생아 전용 요청이 아닐 때만 종류를 교체한다.
            if (pawnKindReplaceAllowScope > 0 && __state == 0 && request.AllowedDevelopmentalStages != DevelopmentalStage.Newborn)
            {
                List<(PawnKindDef kindDef, int weight)> kindDefCandidates = null;

                var originalKindDef = request.KindDef;
                foreach (var kindDef in DefDatabase<PawnKindDef>.AllDefsListForReading)
                {
                    if (kindDef.modExtensions != null)
                    {
                        foreach (var extension in kindDef.modExtensions)
                        {
                            if (extension is PawnGeneratorKindHook hook &&
                                hook.ReplaceHookInfos != null &&
                                hook.ReplaceHookInfos.TryGetValue(originalKindDef, out var weight) &&
                                weight > 0f)
                            {
                                if (kindDefCandidates == null)
                                {
                                    kindDefCandidates = new List<(PawnKindDef kindDef, int weight)>()
                                    {
                                        (originalKindDef, 10000),
                                    };
                                }

                                kindDefCandidates.Add((kindDef, weight));
                            }
                        }
                    }
                }

                if (kindDefCandidates != null && kindDefCandidates.Count > 1)
                {
                    var result = kindDefCandidates.RandomElementByWeight(v => v.weight).kindDef;
                    request.KindDef = result;
                }
            }

            var pawnGenHook = request.KindDef.race.GetModExtension<PawnGeneratorRaceHook>();
            if (pawnGenHook != null && pawnGenHook.forcedXenotype != null)
            {
                request.ForcedXenotype = pawnGenHook.forcedXenotype;
            }

            return true;
        }

        private static void PawnGenerator_GeneratePawn_Finalizer(int __state)
        {
            generatePawnDepth = __state;
        }

        public static void LazyPatchRace(Harmony harmony)
        {
            var postfix = new HarmonyMethod(typeof(HarmonyPatches), nameof(PawnRelationWorker_GenerationChance_Postfix));

            var baseMethod = AccessTools.DeclaredMethod(typeof(PawnRelationWorker), nameof(PawnRelationWorker.GenerationChance));
            if (baseMethod != null)
            {
                harmony.Patch(baseMethod, postfix: postfix);
            }

            foreach (var subClass in typeof(PawnRelationWorker).AllSubclasses())
            {
                var method = AccessTools.DeclaredMethod(subClass, nameof(PawnRelationWorker.GenerationChance));
                if (method != null)
                {
                    harmony.Patch(method, postfix: postfix);
                }
            }
        }

        private static void PawnRelationWorker_GenerationChance_Postfix(ref float __result, PawnRelationDef ___def, Pawn generated, Pawn other)
        {
            var pawnGenHook = generated.def.GetModExtension<PawnGeneratorRaceHook>();
            if (pawnGenHook != null)
            {
                if (pawnGenHook.RelationChanceMultiplier.TryGetValue(___def, out var multiplier))
                {
                    __result *= multiplier;
                }
            }
        }

        private static bool PawnGenerator_GenerateGenes_Prefix(Pawn pawn)
        {
            if (pawn.genes != null)
            {
                var pawnGenHook = pawn.def.GetModExtension<PawnGeneratorRaceHook>();
                if (pawnGenHook != null)
                {
                    if (pawnGenHook.melaninGeneOverrides != null && pawnGenHook.melaninGeneOverrides.Count > 0)
                    {
                        var melaninGeneDef = pawnGenHook.melaninGeneOverrides
                            .Where(v => v.geneDef.endogeneCategory == EndogeneCategory.Melanin)
                            .RandomElementByWeight(v => v.weight).geneDef;

                        pawn.genes.AddGene(melaninGeneDef, xenogene: false);
                    }

                    if (pawnGenHook.hairColorGeneOverrides != null && pawnGenHook.hairColorGeneOverrides.Count > 0)
                    {
                        var hairGeneDef = pawnGenHook.hairColorGeneOverrides
                            .Where(v => v.geneDef.endogeneCategory == EndogeneCategory.HairColor)
                            .RandomElementByWeight(v => v.weight).geneDef;

                        pawn.genes.AddGene(hairGeneDef, xenogene: false);
                    }
                }
            }

            return true;
        }

        private static void PawnGenerator_GetBodyTypeFor_Postfix(ref BodyTypeDef __result, Pawn pawn)
        {
            var hook = pawn.def.GetModExtension<PawnGeneratorRaceHook>();
            if (hook == null) { return; }

            if (pawn.DevelopmentalStage.Adult())
            {
                switch (pawn.gender)
                {
                    case Gender.Male:
                        if (hook.fixedMaleBodyType != null)
                        {
                            __result = hook.fixedMaleBodyType;
                        }
                        break;

                    case Gender.Female:
                        if (hook.fixedFemaleBodyType != null)
                        {
                            __result = hook.fixedFemaleBodyType;
                        }
                        break;
                }
            }
        }

        private static float PawnBioAndNameGenerator_MinAdulthoodAge_Getter_Injection(float age, Pawn pawn)
        {
            var hook = pawn.def.GetModExtension<PawnGeneratorRaceHook>();
            if (hook != null && hook.minAgeForAdulthood >= 0f)
            {
                return hook.minAgeForAdulthood;
            }

            return age;
        }
        private static IEnumerable<CodeInstruction> PawnBioAndNameGenerator_GiveShuffledBioTo_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            var index = instructions.FindIndex(v => v.opcode == OpCodes.Ldc_R4 && v.OperandIs(20));
            if (index >= 0)
            {
                instructions.InsertRange(index + 1, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(PawnBioAndNameGenerator_MinAdulthoodAge_Getter_Injection))),
                });
            }
            else
            {
                Log.Error($"[RPEF] Failed to find injection point for PawnBioAndNameGenerator_GiveShuffledBioTo_Transpiler");
            }

            return instructions;
        }

        private static IEnumerable<CodeInstruction> PawnBioAndNameGenerator_TryGiveSolidBioTo_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            var index = instructions.FindIndex(v => v.opcode == OpCodes.Ldc_R4 && v.OperandIs(20));
            if (index >= 0)
            {
                instructions.InsertRange(index + 1, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(PawnBioAndNameGenerator_MinAdulthoodAge_Getter_Injection))),
                });
            }
            else
            {
                Log.Error($"[RPEF] Failed to find injection point for PawnBioAndNameGenerator_TryGiveSolidBioTo_Transpiler");
            }

            return instructions;
        }

        private static bool GrowthUtility_IsGrowthBirthday_Override(int age, Pawn pawn)
        {
            var extension = pawn.def.GetModExtension<RaceExtension>();
            if (extension != null && extension.growthMomentAgeOverride != null)
            {
                for (int i = 0; i < extension.growthMomentAgeOverride.Count; i++)
                {
                    if (age == extension.growthMomentAgeOverride[i])
                    {
                        return true;
                    }
                }

                return false;
            }

            return GrowthUtility.IsGrowthBirthday(age);
        }
        private static IEnumerable<CodeInstruction> Gizmo_GrowthTier_GrowthTierTooltip_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            var index = instructions.FindIndex(
                v =>
                v.opcode == OpCodes.Call &&
                v.OperandIs(AccessTools.Method(typeof(GrowthUtility), nameof(GrowthUtility.IsGrowthBirthday))));

            if (index >= 0)
            {
                instructions.InsertRange(index + 1, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Gizmo_GrowthTier), "child")),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(GrowthUtility_IsGrowthBirthday_Override))),
                });

                instructions.RemoveAt(index);
            }
            else
            {
                Log.Error($"[RPEF] Failed to find injection point for Gizmo_GrowthTier_GrowthTierTooltip_Transpiler");
            }

            return instructions;
        }

        private static IEnumerable<CodeInstruction> Pawn_AgeTracker_TryChildGrowthMoment_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            var index = instructions.FindIndex(
                v =>
                v.opcode == OpCodes.Call &&
                v.OperandIs(AccessTools.Method(typeof(GrowthUtility), nameof(GrowthUtility.IsGrowthBirthday))));

            if (index >= 0)
            {
                instructions.InsertRange(index + 1, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_AgeTracker), "pawn")),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(GrowthUtility_IsGrowthBirthday_Override))),
                });

                instructions.RemoveAt(index);
            }
            else
            {
                Log.Error($"[RPEF] Failed to find injection point for Pawn_AgeTracker_TryChildGrowthMoment_Transpiler");
            }

            return instructions;
        }

        private static bool Pawn_AgeTracker_GrowthPointsFactor_Prefix(ref float __result, Pawn ___pawn)
        {
            var extension = ___pawn.def.GetModExtension<RaceExtension>();
            if (extension != null && extension.growthPointFactorCurve != null)
            {
                __result = extension.growthPointFactorCurve.Evaluate(___pawn.ageTracker.AgeBiologicalYearsFloat);
                return false;
            }

            return true;
        }

        private static void Pawn_Sterile_Postfix(ref bool __result, Pawn __instance)
        {
            if (!__result)
            {
                var extension = __instance.def.GetModExtension<RaceExtension>();
                if (extension != null && extension.sterile)
                {
                    __result = true;
                }
            }
        }

        private static void InteractionWorker_RomanceAttempt_RandomSelectionWeight_Postfix(Pawn initiator, ref float __result)
        {
            if (__result > 0f)
            {
                var extension = initiator.def.GetModExtension<RaceExtension>();
                if (extension != null)
                {
                    __result *= extension.romanceFrequencyWeight;
                }
            }
        }

        private static void InteractionWorker_RomanceAttempt_SuccessChance_Postfix(Pawn initiator, Pawn recipient, ref float __result)
        {
            if (__result > 0f)
            {
                var initiatorExt = initiator.def.GetModExtension<RaceExtension>();
                if (initiatorExt != null)
                {
                    __result *= initiatorExt.romanceSuccessChanceMultiplierAsInitiator;
                }

                var recipientExt = recipient.def.GetModExtension<RaceExtension>();
                if (recipientExt != null)
                {
                    __result *= recipientExt.romanceSuccessChanceMultiplierAsRecipient;
                }
            }
        }

        public static void StunHandler_CanBeStunnedByDamage_Postfix(StunHandler __instance, DamageDef def, ref bool __result)
        {
            if (__result) { return; }

            if (__instance.parent is Pawn pawn && def == DamageDefOf.EMP)
            {
                var extension = pawn.def.GetModExtension<RaceExtension>();
                if (extension != null && extension.canBeStunnedByEMP && !pawn.Downed && !pawn.Dead)
                {
                    __result = true;
                }
            }
        }

        public static void StunHandler_CanAdaptToDamage_Postfix(StunHandler __instance, DamageDef def, ref bool __result)
        {
            if (__result) { return; }

            if (__instance.parent is Pawn pawn && def == DamageDefOf.EMP)
            {
                var extension = pawn.def.GetModExtension<RaceExtension>();
                if (extension != null && extension.canBeStunnedByEMP)
                {
                    __result = true;
                }
            }
        }

        public class CompStatueRPEFInfo : IExposable
        {
            private static readonly AccessTools.FieldRef<CompStatue, Dictionary<string, object>> CompStatue_additionalSavedPawnDataForMods =
                AccessTools.FieldRefAccess<CompStatue, Dictionary<string, object>>("additionalSavedPawnDataForMods");
            
            public ThingDef thingDef;
            public PawnKindDef pawnKindDef;

            public void ExposeData()
            {
                Scribe_Defs.Look(ref thingDef, "thingDef");
                Scribe_Defs.Look(ref pawnKindDef, "pawnKindDef");
            }

            public static CompStatueRPEFInfo Get(CompStatue statue)
            {
                var data = CompStatue_additionalSavedPawnDataForMods(statue);
                if (data != null && data.TryGetValue(CompStatueDictKey, out var value))
                {
                    return value as CompStatueRPEFInfo;
                }

                return null;
            }
        }

        private const string CompStatueDictKey = "_RPEF_StatueInfo";

        private static void CompStatue_CreateSnapshotOfPawn_HookForMods_Postfix(Pawn p, Dictionary<string, object> dictToStoreDataIn)
        {
            if (p.def == ThingDefOf.Human) { return; }

            dictToStoreDataIn[CompStatueDictKey] = new CompStatueRPEFInfo()
            {
                thingDef = p.def,
                pawnKindDef = p.kindDef,
            };
        }

        private static ThingDef CompStatue_InitFakePawn_ThingDef_Injection(ThingDef thingDef, CompStatue statue)
        {
            return CompStatueRPEFInfo.Get(statue)?.thingDef ?? thingDef;
        }

        private static PawnKindDef CompStatue_InitFakePawn_PawnKindDef_Injection(PawnKindDef pawnKindDef, CompStatue statue)
        {
            return CompStatueRPEFInfo.Get(statue)?.pawnKindDef ?? pawnKindDef;
        }

        private static IEnumerable<CodeInstruction> CompStatue_InitFakePawn_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            var thingDefIndex = instructions.FindIndex(
                v =>
                v.opcode == OpCodes.Ldsfld &&
                v.OperandIs(AccessTools.Field(typeof(ThingDefOf), nameof(ThingDefOf.Human))));

            if (thingDefIndex >= 0)
            {
                instructions.InsertRange(thingDefIndex + 1, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(CompStatue_InitFakePawn_ThingDef_Injection))),
                });
            }
            else
            {
                Log.Error($"[RPEF] Failed to find ThingDef injection point for CompStatue_InitFakePawn_Transpiler");
            }

            var pawnKindDefIndex = instructions.FindIndex(
                v =>
                v.opcode == OpCodes.Ldsfld &&
                v.OperandIs(AccessTools.Field(typeof(PawnKindDefOf), nameof(PawnKindDefOf.Colonist))));

            if (pawnKindDefIndex >= 0)
            {
                instructions.InsertRange(pawnKindDefIndex + 1, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(CompStatue_InitFakePawn_PawnKindDef_Injection))),
                });
            }
            else
            {
                Log.Error($"[RPEF] Failed to find PawnKindDef injection point for CompStatue_InitFakePawn_Transpiler");
            }

            return instructions;
        }

        private static bool PawnGenerator_AdjustXenotypeForFactionlessPawn_Prefix(Pawn pawn, ref PawnGenerationRequest request, ref XenotypeDef xenotype)
        {
            var pawnGenHook = request.KindDef.race.GetModExtension<PawnGeneratorRaceHook>();
            if (pawnGenHook != null && pawnGenHook.forcedXenotype != null && pawnGenHook.forcedXenotype == xenotype)
            {
                return false;
            }

            return true;
        }

        private static void ITab_Genes_CanShowGenesTab_Postfix(ref bool __result)
        {
            if (!__result)
                return;

            Thing selected = Find.Selector.SingleSelectedThing;
            Pawn pawn = selected as Pawn ?? (selected as Corpse)?.InnerPawn;

            if (pawn != null && pawn.def.GetModExtension<RaceExtension>()?.hideGenes == true)
            {
                __result = false;
            }
        }

        private static IEnumerable<CodeInstruction> CharacterCardUtility_DoTopStack_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = instructions.ToList();
            var propertyGetterGenesList = AccessTools.PropertyGetter(typeof(Pawn_GeneTracker), nameof(Pawn_GeneTracker.GenesListForReading));

            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];

                if (codes[i].Calls(propertyGetterGenesList))
                {
                    i++;
                    yield return codes[i];

                    if (i + 1 < codes.Count && (codes[i + 1].opcode == OpCodes.Brfalse || codes[i + 1].opcode == OpCodes.Brfalse_S))
                    {
                        var skipLabel = codes[i + 1].operand;

                        i++;
                        yield return codes[i];

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RaceExtension), nameof(RaceExtension.HideGenes)));
                        yield return new CodeInstruction(OpCodes.Brtrue, skipLabel);
                    }
                }
            }
        }
    }
}
