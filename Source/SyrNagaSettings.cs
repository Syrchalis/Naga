using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;

namespace SyrNaga
{
    class SyrNagaSettings : ModSettings
    {
        //public static bool useUnsupportedHair = false;
        public static bool useStandardAI = false;

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look<bool>(ref useUnsupportedHair, "SyrNaga_useUnsupportedHair", false, true);
            Scribe_Values.Look<bool>(ref useStandardAI, "SyrNaga_usestandardAI", false, true);
        }
    }
}
