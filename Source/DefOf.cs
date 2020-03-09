using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;
using AlienRace;

namespace SyrNaga
{
    [DefOf]
    public static class NagaDefOf
    {
        static NagaDefOf()
        {
        }
        public static ThingDef Naga;
        public static ThingDef Mote_ShieldEmitter;
        public static ThingDef Mote_ShieldEmitterBurst;
        public static StatDef ShieldEmitterRadius;
        public static StatDef ShieldEmitterDecayRate;
        public static BodyDef Naga_Body;
        public static BodyPartDef NagaTail;
        public static BodyPartGroupDef TailAttackTool;
        public static BodyPartGroupDef Feet;
        public static ThoughtDef NagaThought;
        public static HediffDef HiddenNagaHediff;
        public static HediffDef NagaShieldEmitter;
        public static SoundDef ShieldEmitterPop;
        public static SoundDef ShieldEmitterEnd;
    }
}
