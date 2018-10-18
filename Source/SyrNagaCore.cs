using Harmony;
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
            var harmony = HarmonyInstance.Create("Syrchalis.Rimworld.Naga");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override string SettingsCategory() => "SyrNagaSettingsCategory".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            checked
            {
                Listing_Standard listing_Standard = new Listing_Standard();
                listing_Standard.Begin(inRect);
                //listing_Standard.CheckboxLabeled("SyrThrumkin_useUnsupportedHair".Translate(), ref SyrNagaSettings.useUnsupportedHair, ("SyrThrumkin_useUnsupportedHairTooltip".Translate()));
                //listing_Standard.Gap(24f);
                if (listing_Standard.ButtonText("SyrThrumkin_defaultSettings".Translate(), "SyrThrumkin_defaultSettingsTooltip".Translate()))
                {
                    //SyrNagaSettings.useUnsupportedHair = false;
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
