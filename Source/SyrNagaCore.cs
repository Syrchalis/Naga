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
    class SyrNagaCore : Mod
    {
        public static SyrNagaSettings settings;

        public SyrNagaCore(ModContentPack content) : base(content)
        {
            settings = GetSettings<SyrNagaSettings>();
        }

        public override string SettingsCategory() => "SyrNagaSettingsCategory".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            checked
            {
                Listing_Standard listing_Standard = new Listing_Standard();
                listing_Standard.Begin(inRect);
                //listing_Standard.CheckboxLabeled("SyrNaga_useUnsupportedHair".Translate(), ref SyrNagaSettings.useUnsupportedHair, ("SyrNaga_useUnsupportedHairTooltip".Translate()));
                listing_Standard.CheckboxLabeled("SyrNaga_useStandardAI".Translate(), ref SyrNagaSettings.useStandardAI, "SyrNaga_useStandardAITooltip".Translate());
                //listing_Standard.Gap(24f);
                if (listing_Standard.ButtonText("SyrNaga_defaultSettings".Translate(), "SyrNaga_defaultSettingsTooltip".Translate()))
                {
                    //SyrNagaSettings.useUnsupportedHair = false;
                    SyrNagaSettings.useStandardAI = false;
                }
                listing_Standard.End();
                settings.Write();
            }
        }
        public override void WriteSettings()
        {
            base.WriteSettings();
        }
    }
}
