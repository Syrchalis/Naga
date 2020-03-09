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
    [StaticConstructorOnStartup]
    public class NagaShieldBelt : Apparel
    {
        public override bool AllowVerbCast(IntVec3 root, Map map, LocalTargetInfo targ, Verb verb)
        {
            return true;
        }
        private float EnergyMax
        {
            get
            {
                return this.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true);
            }
        }

        private float EnergyGainPerTick
        {
            get
            {
                return this.GetStatValue(StatDefOf.EnergyShieldRechargeRate, true) / 60f;
            }
        }

        public float Energy
        {
            get
            {
                return this.energy;
            }
        }

        public ShieldState ShieldState
        {
            get
            {
                if (this.ticksToReset > 0)
                {
                    return ShieldState.Resetting;
                }
                return ShieldState.Active;
            }
        }

        private bool ShouldDisplay
        {
            get
            {
                Pawn wearer = base.Wearer;
                return wearer.Spawned && !wearer.Dead && !wearer.Downed && (wearer.InAggroMentalState || wearer.Drafted || (wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner) || Find.TickManager.TicksGame < this.lastKeepDisplayTick + this.KeepDisplayingTicks);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref energy, "energy", 0f, false);
            Scribe_Values.Look<int>(ref ticksToReset, "ticksToReset", -1, false);
            Scribe_Values.Look<int>(ref lastKeepDisplayTick, "lastKeepDisplayTick", 0, false);
        }

        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            if (Find.Selector.SingleSelectedThing == base.Wearer)
            {
                yield return new Gizmo_NagaEnergyShieldStatus
                {
                    shield = this
                };
            }
            yield break;
        }

        public override float GetSpecialApparelScoreOffset()
        {
            return this.EnergyMax * this.ApparelScorePerEnergyMax;
        }

        public override void Tick()
        {
            base.Tick();
            if (base.Wearer == null)
            {
                this.energy = 0f;
                return;
            }
            if (this.ShieldState == ShieldState.Resetting)
            {
                this.ticksToReset--;
                if (this.ticksToReset <= 0)
                {
                    this.Reset();
                }
            }
            else if (this.ShieldState == ShieldState.Active)
            {
                this.energy += this.EnergyGainPerTick;
                if (this.energy > this.EnergyMax)
                {
                    this.energy = this.EnergyMax;
                }
            }
        }

        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            if (this.ShieldState == ShieldState.Active && ((dinfo.Instigator != null && !dinfo.Instigator.Position.AdjacentTo8WayOrInside(base.Wearer.Position)) || dinfo.Def.isExplosive))
            {
                if (dinfo.Instigator != null)
                {
                    AttachableThing attachableThing = dinfo.Instigator as AttachableThing;
                    if (attachableThing != null && attachableThing.parent == base.Wearer)
                    {
                        return false;
                    }
                }
                this.energy -= dinfo.Amount * EnergyLossPerDamage;
                if (dinfo.Def == DamageDefOf.EMP)
                {
                    this.energy = -1f;
                }
                if (this.energy < 0f)
                {
                    this.Break();
                }
                else
                {
                    this.AbsorbedDamage(dinfo);
                }
                return true;
            }
            return false;
        }
        
        public void KeepDisplaying()
        {
            this.lastKeepDisplayTick = Find.TickManager.TicksGame;
        }
        
        private void AbsorbedDamage(DamageInfo dinfo)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = base.Wearer.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            MoteMaker.MakeStaticMote(loc, base.Wearer.Map, ThingDefOf.Mote_ExplosionFlash, num);
            int num2 = (int)num;
            for (int i = 0; i < num2; i++)
            {
                MoteMaker.ThrowDustPuff(loc, base.Wearer.Map, Rand.Range(0.8f, 1.2f));
            }
            this.lastAbsorbDamageTick = Find.TickManager.TicksGame;
            this.KeepDisplaying();
        }
        
        private void Break()
        {
            SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
            MoteMaker.MakeStaticMote(base.Wearer.TrueCenter(), base.Wearer.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
            for (int i = 0; i < 6; i++)
            {
                Vector3 loc = base.Wearer.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f);
                MoteMaker.ThrowDustPuff(loc, base.Wearer.Map, Rand.Range(0.8f, 1.2f));
                MoteMaker.ThrowLightningGlow(base.Wearer.TrueCenter(), base.Wearer.Map, 2f);
            }
            this.energy = 0f;
            this.ticksToReset = this.StartingTicksToReset;
        }
        
        private void Reset()
        {
            if (base.Wearer.Spawned)
            {
                SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(base.Wearer.Position, base.Wearer.Map, false));
                MoteMaker.ThrowLightningGlow(base.Wearer.TrueCenter(), base.Wearer.Map, 3f);
            }
            this.ticksToReset = -1;
            this.energy = this.EnergyOnReset;
        }
        
        public override void DrawWornExtras()
        {
            if (this.ShieldState == ShieldState.Active && this.ShouldDisplay)
            {
                float num = Mathf.Lerp(1.2f, 1.55f, this.energy);
                Vector3 vector = base.Wearer.Drawer.DrawPos;
                vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    vector += this.impactAngleVect * num3;
                    num -= num3;
                }
                float angle = (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, NagaShieldBelt.BubbleMat, 0);
            }
        }

        private float energy;
        private int ticksToReset = -1;
        private int lastKeepDisplayTick = -9999;
        private Vector3 impactAngleVect;
        private int lastAbsorbDamageTick = -9999;
        private const float MinDrawSize = 1.2f;
        private const float MaxDrawSize = 1.55f;
        private const float MaxDamagedJitterDist = 0.05f;
        private const int JitterDurationTicks = 8;
        private int StartingTicksToReset = 300;
        private float EnergyOnReset = 0f;
        private float EnergyLossPerDamage = 0.033f;
        private int KeepDisplayingTicks = 1000;
        private float ApparelScorePerEnergyMax = 0.5f;
        private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);
    }
    [StaticConstructorOnStartup]
    public class Gizmo_NagaEnergyShieldStatus : Gizmo
    {
        public Gizmo_NagaEnergyShieldStatus()
        {
            this.order = -100f;
        }
        
        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }
        
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(984688, overRect, WindowLayer.GameUI, delegate
            {
                Rect rect = overRect.AtZero().ContractedBy(6f);
                Rect rect2 = rect;
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect2, this.shield.LabelCap);
                Rect rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                float fillPercent = this.shield.Energy / this.shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true);
                Widgets.FillableBar(rect3, fillPercent, FullShieldBarTex, EmptyShieldBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, (this.shield.Energy * 100f).ToString("F0") + " / " + (this.shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true) * 100f).ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f);
            return new GizmoResult(GizmoState.Clear);
        }

        public NagaShieldBelt shield;
        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
    }
}
