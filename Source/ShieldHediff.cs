using RimWorld;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Text;

namespace SyrNaga
{
    [StaticConstructorOnStartup]
    public class ShieldHediff : HediffWithComps
    {
        public float shieldDecayPerSec = 0.033f;
        public float shieldLossPerDmg = 0.033f;
        public float shieldCurrent = 1f;
        public float shieldMax = 1f;
        public int cooldownTicks;
        public bool broken;
        public int lastAbsorbDamageTick = -9999;
        public Vector3 impactAngleVect;
        public static Color shieldColor = new Color(0.75f, 1f, 0.88f);
        private static readonly Material BubbleMat = MaterialPool.MatFrom("Things/Mote/NagaShieldBubble", ShaderDatabase.Transparent, shieldColor);
        private float savedAngle;
        private bool removedOnBroken = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref shieldDecayPerSec, "shieldDecayPerSec", 0.033f, false);
            Scribe_Values.Look<float>(ref shieldCurrent, "shieldCurrent", 1f, false);
            Scribe_Values.Look<float>(ref shieldMax, "shieldMax", 1f, false);
            Scribe_Values.Look<int>(ref cooldownTicks, "cooldownTicks", 0, false);
            Scribe_Values.Look<bool>(ref broken, "broken", false, false);
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            shieldCurrent = shieldMax;
        }

        public override bool ShouldRemove
        {
            get
            {
                if (removedOnBroken && broken)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (cooldownTicks > 0)
            {
                cooldownTicks--;
            }
            else
            {
                shieldCurrent -= (shieldDecayPerSec / 60);
                if (shieldCurrent <= 0)
                {
                    broken = true;
                    NagaDefOf.ShieldEmitterEnd.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
                }
                if (!removedOnBroken)
                {
                    Reset();
                }
            }
        }

        public void Reset()
        {
            if (pawn.Spawned)
            {
                SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
                FleckMaker.ThrowLightningGlow(pawn.TrueCenter(), pawn.Map, 3f);
            }
            cooldownTicks = 0;
            shieldCurrent = shieldMax * 0.2f;
            broken = false;
        }

        public bool AbsorbDamage(DamageInfo dinfo)
        {
            lastAbsorbDamageTick = Find.TickManager.TicksGame;
            impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            if (broken)
            {
                return false;
            }
            else
            {
                shieldCurrent -= dinfo.Amount * shieldLossPerDmg;
                if (shieldCurrent <= 0f)
                {
                    broken = true;
                }
                return true;
            }
        }

        public void DrawWornExtras()
        {
            if (!pawn.Dead)
            {
                float num = Mathf.Lerp(1.2f, 1.55f, shieldCurrent);
                Vector3 vector = pawn.Drawer.DrawPos;
                vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    vector += this.impactAngleVect * num3;
                    num -= num3;
                }
                float angle = savedAngle;
                savedAngle++;
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
            }
        }

        public override string DebugString()
        {
            StringBuilder stringBuilder = new StringBuilder(base.DebugString());
            stringBuilder.AppendLine("cooldownTicks: " + this.cooldownTicks);
            return stringBuilder.ToString();
        }
    }


    [StaticConstructorOnStartup]
    public class Gizmo_HediffComp_Shield : Gizmo
    {
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(942612547, overRect, WindowLayer.GameUI, delegate
            {
                Rect rect = overRect.AtZero().ContractedBy(6f);
                Rect rect2 = rect;
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect2, shield.def.LabelCap);
                Rect rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                float fillPercent = shield.shieldCurrent / shield.shieldMax;
                Widgets.FillableBar(rect3, fillPercent, FullShieldBarTex, EmptyShieldBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, (shield.shieldCurrent * 100f).ToString("F0") + " / " + (shield.shieldMax * 100f).ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f);
            return new GizmoResult(GizmoState.Clear);
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }
        
        public ShieldHediff shield;
        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.4f, 0.3f));
        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
    }
}