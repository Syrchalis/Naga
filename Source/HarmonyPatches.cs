using Harmony;
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

namespace SyrNaga
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
        }
    }

    [HarmonyPatch(typeof(QualityUtility), nameof(QualityUtility.GenerateQualityCreatedByPawn), new Type[] { typeof(Pawn), typeof(SkillDef) })]
    public class GenerateQualityCreatedByPawnPatch
    {
        [HarmonyPostfix]
        public static void GenerateQualityCreatedByPawn_Postfix(ref QualityCategory __result, Pawn pawn, SkillDef relevantSkill)
        {
            if (pawn.def == NagaDefOf.Naga)
            {
                float roll = Rand.Value;
                if (roll < 0.5f)
                {
                    __result += 1;
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
            
            if (ingester.def == NagaDefOf.Naga)
            {
                CompIngredients compIngr = __instance.TryGetComp<CompIngredients>();
                if (compIngr != null)
                {
                    bool meat = compIngr.ingredients.Exists(i => (i.ingestible.foodType & FoodTypeFlags.Meat) != FoodTypeFlags.None);
                    bool nonMeat = compIngr.ingredients.Exists(i => (i.ingestible.foodType & FoodTypeFlags.Meat) == FoodTypeFlags.None);
                    if (meat && !nonMeat)
                    {
                        nutritionIngested *= 1f;
                    }
                    else if (meat && nonMeat)
                    {
                        nutritionIngested *= 0.6f;
                    }
                    else if (!meat && nonMeat)
                    {
                        nutritionIngested *= 0.2f;
                    }
                }
                else
                {
                    if ((__instance.def.ingestible.foodType & FoodTypeFlags.Meat) != FoodTypeFlags.None)
                    {
                        nutritionIngested *= 1f;
                    }
                    else if ((__instance.def.ingestible.foodType & FoodTypeFlags.Corpse) != FoodTypeFlags.None)
                    {
                        nutritionIngested *= 1f;
                    }
                    else if ((__instance.def.ingestible.foodType & FoodTypeFlags.Meat) == FoodTypeFlags.None)
                    {
                        nutritionIngested *= 0.2f;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.FoodOptimality))]
    public static class FoodOptimalityPatch
    {
        [HarmonyPostfix]
        public static void FoodOptimality_Postfix(ref float __result, Pawn eater, Thing foodSource)
        {
            if (eater?.def != null && foodSource?.def != null && eater.def == NagaDefOf.Naga)
            {
                CompIngredients compIngr = foodSource.TryGetComp<CompIngredients>();
                if (compIngr != null)
                {
                    bool meat = compIngr.ingredients.Exists(i => (i.ingestible.foodType & FoodTypeFlags.Meat) != FoodTypeFlags.None);
                    bool nonMeat = compIngr.ingredients.Exists(i => (i.ingestible.foodType & FoodTypeFlags.Meat) == FoodTypeFlags.None);
                    if (meat && !nonMeat)
                    {
                        __result += 10f;
                    }
                    else if (meat && nonMeat)
                    {
                        __result -= 10f;
                    }
                    else if (!meat && nonMeat)
                    {
                        __result -= 40f;
                    }
                }
                else
                {
                    if ((foodSource.def.ingestible.foodType & FoodTypeFlags.Meat) != FoodTypeFlags.None)
                    {
                        __result += 10f;
                    }
                    else if ((foodSource.def.ingestible.foodType & FoodTypeFlags.Corpse) != FoodTypeFlags.None)
                    {
                        __result += 10f;
                    }
                    else if ((foodSource.def.ingestible.foodType & FoodTypeFlags.Meat) == FoodTypeFlags.None)
                    {
                        __result -= 40f;
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(PawnGenerator), "TryGenerateNewPawnInternal")]
    public static class TryGenerateNewPawnInternalPatch
    {
        [HarmonyPostfix]
        public static void TryGenerateNewPawnInternal_Postfix(ref Pawn __result)
        {
            if (__result?.def != null && __result.def == NagaDefOf.Naga)
            {
                __result.health.AddHediff(HediffMaker.MakeHediff(NagaDefOf.HiddenNagaHediff, __result));
                if (__result?.gender != null && __result.gender == Gender.Male)
                {
                    __result.story.bodyType = BodyTypeDefOf.Male;
                }
                if (__result?.gender != null && __result.gender == Gender.Female)
                {
                    __result.story.bodyType = BodyTypeDefOf.Female;
                }
                AlienPartGenerator.AlienComp alienComp = __result.TryGetComp<AlienPartGenerator.AlienComp>();
                if (alienComp?.skinColor != null && alienComp.skinColor == new Color(1.0f, 1.0f, 1.0f))
                {
                    if (Rand.Chance(0.5f))
                    {
                        alienComp.skinColor = orangeL;
                        alienComp.skinColorSecond = orange;
                    }
                    else
                    {
                        alienComp.skinColor = greenL;
                        alienComp.skinColorSecond = green;
                    }
                }
            }
        }
        public static Color orange = new Color(0.8f, 0.5f, 0.1f);
        public static Color orangeL = new Color(0.8f, 0.7f, 0.6f);
        public static Color green = new Color(0.25f, 0.5f, 0.25f);
        public static Color greenL = new Color(0.6f, 0.8f, 0.6f);
    }
    [HarmonyPatch(typeof(ApparelUtility), nameof(ApparelUtility.HasPartsToWear))]
    public static class HasPartsToWearPatch
    {
        [HarmonyPostfix]
        public static void HasPartsToWear_Postfix(ref bool __result, Pawn p, ThingDef apparel)
        {
            if (apparel?.apparel?.bodyPartGroups != null && p?.def != null)
            {
                if (apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) && !apparel.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) && p.def == NagaDefOf.Naga)
                {
                    __result = false;
                }
                if (apparel.apparel.bodyPartGroups.Contains(NagaDefOf.TailAttackTool) && p.def != NagaDefOf.Naga)
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
                    __result = __result.Add(new Gizmo_HediffComp_Shield
                    {
                        shield = shield
                    });
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) })]
    public static class RenderPawnInternalPatch
    {
        [HarmonyPostfix]
        public static void RenderPawnInternal_Postfix(PawnRenderer __instance, Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn.health != null && pawn.health.hediffSet.HasHediff(NagaDefOf.NagaShieldEmitter, false))
            {
                ShieldHediff shield = pawn.health.hediffSet.GetFirstHediffOfDef(NagaDefOf.NagaShieldEmitter, false) as ShieldHediff;
                shield.DrawWornExtras();
            }
        }
    }
}
