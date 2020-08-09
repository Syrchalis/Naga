using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;

namespace SyrNaga
{
    public class NagaShieldEmitter : Apparel
    {
        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            if (dinfo.Def.harmsHealth && dinfo.Def.ExternalViolenceFor(this) && dinfo.Weapon != null)
            {
                float radius = this.GetStatValue(NagaDefOf.ShieldEmitterRadius, true);
                float maxShield = this.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true);
                float shieldDecay = this.GetStatValue(NagaDefOf.ShieldEmitterDecayRate, true);
                //Log.Message("ShieldEmitter Radius: " + radius + " / MaxShield: " + maxShield + " / ShieldDecayRate: " + shieldDecay);
                foreach (Thing t in GenRadial.RadialDistinctThingsAround(Wearer.Position, Wearer.Map, radius, true))
                {
                    Pawn pawn = t as Pawn;
                    if (pawn != null && pawn.Faction != null && !pawn.HostileTo(Wearer))
                    {
                        if (pawn.health.hediffSet.HasHediff(NagaDefOf.NagaShieldEmitter, false))
                        {
                            ShieldHediff oldShield = pawn.health.hediffSet.GetFirstHediffOfDef(NagaDefOf.NagaShieldEmitter, false) as ShieldHediff;
                            if (oldShield.shieldCurrent <= maxShield)
                            {
                                pawn.health.RemoveHediff(oldShield);
                            }
                        }
                        Hediff hediff = pawn.health.AddHediff(NagaDefOf.NagaShieldEmitter);
                        ShieldHediff shieldHediff = hediff as ShieldHediff;
                        shieldHediff.shieldMax *= maxShield;
                        shieldHediff.shieldCurrent = shieldHediff.shieldMax;
                        shieldHediff.shieldDecayPerSec = shieldDecay;
                        if (pawn != Wearer)
                        {
                            ShieldEmitterBurst(Wearer, pawn.DrawPos, NagaDefOf.Mote_ShieldEmitterBurst);
                        }
                    }
                }
                MoteMaker.ThrowLightningGlow(Wearer.TrueCenter(), Wearer.Map, 4f);
                ShieldEmitter(Wearer, Wearer.Map, radius, NagaDefOf.Mote_ShieldEmitter);
                NagaDefOf.ShieldEmitterPop.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map, false));
                Destroy(DestroyMode.Vanish);
            }
            return true;
        }

        public static Mote ShieldEmitter(Pawn caster, Map map, float radius, ThingDef moteDef)
        {
            if (!caster.Position.ShouldSpawnMotesAt(map) || map.moteCounter.Saturated)
            {
                return null;
            }
            Mote mote = (Mote)ThingMaker.MakeThing(moteDef, null);
            mote.Scale = radius / 3f;
            mote.rotationRate = 0f;
            mote.exactPosition = caster.DrawPos;
            mote.exactRotation = Rand.Range(0, 360);
            GenSpawn.Spawn(mote, caster.Position, map, WipeMode.Vanish);
            return mote;
        }

        private static void ShieldEmitterBurst(Pawn thrower, Vector3 targetCell, ThingDef mote)
        {
            if (!thrower.Position.ShouldSpawnMotesAt(thrower.Map) || thrower.Map.moteCounter.Saturated)
            {
                return;
            }
            float num = Rand.Range(20f, 30f);
            EmitterMote emitterMote = (EmitterMote)ThingMaker.MakeThing(mote, null);
            emitterMote.Scale = 1f;
            emitterMote.exactRotation = (targetCell - thrower.DrawPos).AngleFlat();
            emitterMote.rotationRate = 0f;
            emitterMote.exactPosition = thrower.DrawPos;
            emitterMote.SetVelocity((targetCell - emitterMote.exactPosition).AngleFlat(), num);
            emitterMote.targetVector = targetCell;
            //moteThrown.airTimeLeft = (float)Mathf.RoundToInt((moteThrown.exactPosition - vector).magnitude / num);
            GenSpawn.Spawn(emitterMote, thrower.Position, thrower.Map, WipeMode.Vanish);
        }
    }

    public class EmitterMote : MoteThrown
    {
        public Vector3 targetVector;
        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            if (exactPosition != null && targetVector != null)
            {
                if (exactPosition.ToIntVec3() == targetVector.ToIntVec3() && !Destroyed)
                {
                    Destroy(DestroyMode.Vanish);
                    return;
                }
            }
        }
    }
}
