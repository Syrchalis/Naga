using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.AI;
using AlienRace;
using Verse.Sound;
using Verse.Grammar;

namespace SyrNaga
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("Syrchalis.Rimworld.Naga");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            femaleVariants = TextureVariants(true);
            maleVariants = TextureVariants(false);
            if (!ThrumkinActive)
            {
                harmony.Patch(AccessTools.Method(typeof(GrammarUtility), nameof(GrammarUtility.RulesForPawn), new Type[]
                { typeof(string), typeof(Name), typeof(string), typeof(PawnKindDef), typeof(Gender), typeof(Faction), typeof(int), typeof(int), typeof(string),
                typeof(bool), typeof(bool), typeof(bool), typeof(List<RoyalTitle>), typeof(Dictionary<string, string>), typeof(bool) }),
                null, new HarmonyMethod(AccessTools.Method(typeof(RulesForPawnPatch), nameof(RulesForPawnPatch.RulesForPawn_Postfix))), null, null);
            }
        }
        public static bool ThrumkinActive => ModsConfig.ActiveModsInLoadOrder.Any(m => m.PackageId == "syrchalis.thrumkin");

        public static int TextureVariants(bool female)
        {
            int count = 0;
            ThingDef_AlienRace thingDef_AlienRace = (NagaDefOf.Naga as ThingDef_AlienRace);
            string path;
            if (female)
            {
                path = "Things/Naga/Body/Naked_Female";
            }
            else
            {
                path = "Things/Naga/Body/Naked_Male";
            }
            Texture2D texture = ContentFinder<Texture2D>.Get(path + "_east", false);
            while (texture != null)
            {
                count++;
                texture = ContentFinder<Texture2D>.Get(path + count + "_east", false);
            }
            return count;
        }
        public static int femaleVariants;
        public static int maleVariants;
    }

    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), new Type[] { typeof(Pawn), typeof(SkillDef) })]
    public class GenerateQualityCreatedByPawnPatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void GenerateQualityCreatedByPawn_Postfix(ref QualityCategory __result, Pawn pawn, SkillDef relevantSkill)
        {
            if (pawn?.def != null && pawn.def == NagaDefOf.Naga)
            {
                float roll = Rand.Value;
                if (roll < 0.2f)
                {
                    __result += 1;
                    if (__result > QualityCategory.Legendary)
                    {
                        __result = QualityCategory.Legendary;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Thing), "IngestedCalculateAmounts")]
    public class IngestedCalculateAmountsPatch
    {
        [HarmonyPostfix]
        public static void IngestedCalculateAmounts_Postfix(Thing __instance, Pawn ingester, float nutritionWanted, ref float nutritionIngested)
        {
            
            if (ingester?.def != null && __instance?.def?.ingestible != null && ingester.def == NagaDefOf.Naga)
            {
                if (__instance.def == ThingDefOf.MealSurvivalPack || __instance.def == ThingDefOf.Pemmican || __instance.def == NagaDefOf.MealLavish)
                {
                    return;
                }
                CompIngredients compIngr = __instance.TryGetComp<CompIngredients>();
                if (compIngr?.ingredients != null)
                {
                    bool meat = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.Meat) != FoodTypeFlags.None);
                    bool animalP = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.AnimalProduct) != FoodTypeFlags.None);
                    bool nonMeat = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.Meat) == FoodTypeFlags.None);
                    bool nonAnimalP = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.AnimalProduct) == FoodTypeFlags.None);
                    if (meat && !nonMeat)
                    {
                        nutritionIngested *= 1.0f;
                    }
                    else if (animalP && !nonAnimalP)
                    {
                        nutritionIngested *= 1.0f;
                    }
                    else if (animalP && nonMeat)
                    {
                        nutritionIngested *= 0.7f;
                    }
                    else if (meat && animalP)
                    {
                        nutritionIngested *= 1.2f;
                    }
                    else if (meat && nonMeat)
                    {
                        nutritionIngested *= 0.7f;
                    }
                    else
                    {
                        nutritionIngested *= 0.35f;
                    }
                }
                else
                {
                    if ((__instance.def.ingestible.foodType & FoodTypeFlags.Meat) != FoodTypeFlags.None)
                    {
                        nutritionIngested *= 1.0f;
                    }
                    else if ((__instance.def.ingestible.foodType & FoodTypeFlags.AnimalProduct) != FoodTypeFlags.None)
                    {
                        nutritionIngested *= 1.2f;
                    }
                    else if ((__instance.def.ingestible.foodType & FoodTypeFlags.Corpse) != FoodTypeFlags.None)
                    {
                        nutritionIngested *= 1.0f;
                    }
                    else
                    {
                        nutritionIngested *= 0.35f;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.FoodOptimality))]
    public static class FoodOptimalityPatch
    {
        [HarmonyPriority(Priority.First)]
        [HarmonyPostfix]
        public static void FoodOptimality_Postfix(ref float __result, Pawn eater, Thing foodSource)
        {
            if (SyrNagaSettings.useStandardAI)
            {
                return;
            }
            else
            {
                __result = FoodOptimality_Method(__result, eater, foodSource);
            }
        }

        public static float FoodOptimality_Method(float __result, Pawn eater, Thing foodSource)
        {
            float foodValue = __result;
            if (eater?.def != null && foodSource?.def?.ingestible?.foodType != null && eater.def == NagaDefOf.Naga)
            {
                if (foodSource.def == ThingDefOf.MealSurvivalPack || foodSource.def == ThingDefOf.Pemmican || foodSource.def == NagaDefOf.MealLavish)
                {
                    return foodValue;
                }
                CompIngredients compIngr = foodSource.TryGetComp<CompIngredients>();
                if (compIngr?.ingredients != null)
                {
                    bool meat = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.Meat) != FoodTypeFlags.None);
                    bool animalP = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.AnimalProduct) != FoodTypeFlags.None);
                    bool nonMeat = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.Meat) == FoodTypeFlags.None);
                    bool nonAnimalP = compIngr.ingredients.Exists(i => ((i.ingestible?.foodType ?? FoodTypeFlags.None) & FoodTypeFlags.AnimalProduct) == FoodTypeFlags.None);
                    if (meat && !nonMeat)
                    {
                        foodValue += 20f;
                    }
                    else if (animalP && !nonAnimalP)
                    {
                        foodValue += 20f;
                    }
                    else if (meat && animalP)
                    {
                        foodValue += 40f;
                    }
                    else if (meat && nonMeat)
                    {
                        foodValue += 0f;
                    }
                    else if (animalP && nonMeat)
                    {
                        foodValue += 10f;
                    }
                    else
                    {
                        foodValue -= 20f;
                    }
                }
                else
                {
                    if ((foodSource.def.ingestible.foodType & FoodTypeFlags.Meat) != FoodTypeFlags.None)
                    {
                        foodValue += 20f;
                    }
                    else if ((foodSource.def.ingestible.foodType & FoodTypeFlags.AnimalProduct) != FoodTypeFlags.None)
                    {
                        foodValue += 40f;
                    }
                    else if ((foodSource.def.ingestible.foodType & FoodTypeFlags.Corpse) != FoodTypeFlags.None)
                    {
                        foodValue += 20f;
                    }
                    else
                    {
                        foodValue -= 20f;
                    }
                }
            }
            return foodValue;
        }
    }
    [HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
    public static class GenerateNewPawnInternalPatch
    {
        [HarmonyPostfix]
        public static void GenerateNewPawnInternal_Postfix(ref PawnGenerationRequest request, Pawn __result)
        {
            if (__result != null && __result.def == NagaDefOf.Naga)
            {
                __result.health.AddHediff(HediffMaker.MakeHediff(NagaDefOf.HiddenNagaHediff, __result));

                __result.GetComp<AlienPartGenerator.AlienComp>().GetChannel("skin").second = NagaUtility.AssignSecondColor(__result.story.SkinColor);
            }
        }
    }


    [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics))]
    public static class ResolveAllGraphicsPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        public static void ResolveAllGraphics_Prefix(PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;
            ThingDef_AlienRace thingDef_AlienRace = pawn.def as ThingDef_AlienRace;
            if (pawn?.def != null && pawn.def == NagaDefOf.Naga && thingDef_AlienRace != null)
            {
                //Changing body to random pattern variation and changing second color to appropiate pair
                AlienPartGenerator alienPartGenerator = thingDef_AlienRace.alienRace.generalSettings.alienPartGenerator;
                GraphicPaths currentGraphicPath = thingDef_AlienRace.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage);
                //Fixes randomly wrong gender/bodytype combination - cause still unknown!
                if (pawn.gender == Gender.Female && pawn.story.bodyType == BodyTypeDefOf.Male || pawn.gender == Gender.Male && pawn.story.bodyType == BodyTypeDefOf.Female)
                {
                    pawn.story.bodyType = pawn.gender == Gender.Female ? BodyTypeDefOf.Female : BodyTypeDefOf.Male;
                }
                string nakedPath = AlienPartGenerator.GetNakedPath(pawn.story.bodyType, currentGraphicPath.body, thingDef_AlienRace.alienRace.generalSettings.alienPartGenerator.useGenderedBodies ? pawn.gender.ToString() : "");
                int variantIndex = pawn.thingIDNumber % ((pawn.gender == Gender.Female) ? HarmonyPatches.femaleVariants : HarmonyPatches.maleVariants);
                __instance.nakedGraphic = GraphicDatabase.Get(typeof(Graphic_Naga), variantIndex == 0 ? nakedPath : nakedPath + variantIndex.ToString(), ShaderDatabase.CutoutComplex, Vector2.one, 
                    pawn.story.SkinColor, alienPartGenerator.SkinColor(pawn, false), null, null);
            }
        }
        public static GraphicPaths GetCurrentGraphicPath(this List<GraphicPaths> list, LifeStageDef lifeStageDef)
        {
            return list.FirstOrDefault(delegate (GraphicPaths gp)
            {
                List<LifeStageDef> lifeStageDefs = gp.lifeStageDefs;
                return lifeStageDefs != null && lifeStageDefs.Contains(lifeStageDef);
            }) ?? list.First<GraphicPaths>();
        }
    }


    [HarmonyPatch(typeof(ApparelUtility), nameof(ApparelUtility.HasPartsToWear))]
    public static class HasPartsToWearPatch
    {
        [HarmonyPostfix]
        public static void HasPartsToWear_Postfix(ref bool __result, Pawn p, ThingDef apparel)
        {
            if (apparel?.apparel?.bodyPartGroups != null && p?.def != null)
            {
                if (p.def == NagaDefOf.Naga && apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) && !apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
                {
                    __result = false;
                }
                if (p.def == NagaDefOf.Naga && apparel.apparel.bodyPartGroups.Contains(NagaDefOf.Feet) && !apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
                {
                    __result = false;
                }
                if (p.def != NagaDefOf.Naga && apparel.apparel.bodyPartGroups.Contains(NagaDefOf.TailAttackTool))
                {
                    __result = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class PreApplyDamagePatch
    {
        [HarmonyPrefix]
        public static bool PreApplyDamage_Prefix(ref Pawn __instance, ref DamageInfo dinfo, out bool absorbed)
        {
            bool exludedDamageDefs = dinfo.Def == DamageDefOf.SurgicalCut || dinfo.Def == DamageDefOf.ExecutionCut || dinfo.Def == DamageDefOf.EMP || dinfo.Def == DamageDefOf.Stun;
            if (!exludedDamageDefs && __instance.health != null && __instance.health.hediffSet.HasHediff(NagaDefOf.NagaShieldEmitter, false))
            {
                ShieldHediff shieldHediff = __instance.health.hediffSet.GetFirstHediffOfDef(NagaDefOf.NagaShieldEmitter, false) as ShieldHediff;
                if (shieldHediff != null)
                {
                    if (shieldHediff.broken)
                    {
                    }
                    else
                    {
                        if (shieldHediff.AbsorbDamage(dinfo))
                        {
                            if (shieldHediff.broken)
                            {
                                SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(__instance.Position, __instance.Map, false));
                                MoteMaker.MakeStaticMote(__instance.TrueCenter(), __instance.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
                            }
                            else
                            {
                                SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(__instance.Position, __instance.Map, false));
                                shieldHediff.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
                                Vector3 loc = __instance.TrueCenter() + shieldHediff.impactAngleVect.RotatedBy(180f) * 0.5f;
                                float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
                                MoteMaker.MakeStaticMote(loc, __instance.Map, ThingDefOf.Mote_ExplosionFlash, num);
                                int num2 = (int)num;
                                for (int i = 0; i < num2; i++)
                                {
                                    MoteMaker.ThrowDustPuff(loc, __instance.Map, Rand.Range(0.8f, 1.2f));
                                }
                            }
                            absorbed = true;
                            return false;
                        }
                    }
                }
            }
            absorbed = false;
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
    public static class GetGizmosPatch
    {
        [HarmonyPostfix]
        public static void GetGizmos_Postfix(ref Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            bool flag = __instance.health != null && __instance.health.hediffSet.HasHediff(NagaDefOf.NagaShieldEmitter, false);
            if (flag)
            {
                bool flag2 = Find.Selector.NumSelected == 1;
                if (flag2)
                {
                    ShieldHediff shield = __instance.health.hediffSet.GetFirstHediffOfDef(NagaDefOf.NagaShieldEmitter, false) as ShieldHediff;
                    __result = __result.AddItem(new Gizmo_HediffComp_Shield
                    {
                        shield = shield
                    });
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) })]
    public static class RenderPawnInternalPatch
    {
        [HarmonyPostfix]
        public static void RenderPawnInternal_Postfix(PawnRenderer __instance, Pawn ___pawn, Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump, bool invisible)
        {
            if (___pawn.health != null && ___pawn.health.hediffSet.HasHediff(NagaDefOf.NagaShieldEmitter, false))
            {
                ShieldHediff shield = ___pawn.health.hediffSet.GetFirstHediffOfDef(NagaDefOf.NagaShieldEmitter, false) as ShieldHediff;
                shield.DrawWornExtras();
            }
        }
    }

    public static class RulesForPawnPatch
    {
        public static IEnumerable<Rule> RulesForPawn_Postfix(IEnumerable<Rule> __result, string pawnSymbol, string title, Gender gender, PawnKindDef kind)
        {
            List<Rule> ruleList = __result.ToList();
            string prefix = "";
            if (!pawnSymbol.NullOrEmpty())
            {
                prefix = prefix + pawnSymbol + "_";
            }
            for (int i = 0; i < ruleList.Count; i++)
            {
                Rule_String r = ruleList[i] as Rule_String;
                if (r != null && r.keyword == (prefix + "titleIndef"))
                {
                    ruleList[i] = new Rule_String(prefix + "titleIndef", Find.ActiveLanguageWorker.WithIndefiniteArticle(kind.race.label + " " + title, gender, false, false));
                }
                else if (r != null && r.keyword == (prefix + "titleDef"))
                {
                    ruleList[i] = new Rule_String(prefix + "titleDef", Find.ActiveLanguageWorker.WithDefiniteArticle(kind.race.label + " " + title, gender, false, false));
                }
                else if (r != null && r.keyword == (prefix + "title"))
                {
                    ruleList[i] = new Rule_String(prefix + "title", kind.race.label + " " + title);
                }
            }
            __result = ruleList;
            foreach (Rule rule in __result)
            {
                yield return rule;
            }
        }
    }
}