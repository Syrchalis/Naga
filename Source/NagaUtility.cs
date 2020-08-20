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

namespace SyrNaga
{
    public static class NagaUtility
    {
        public static Color AssignSecondColor(Color firstColor)
        {
            float roll = Rand.Value;
            if (firstColor == green1 && roll < 0.5f)
            {
                return aqua2;
            }
            else if (firstColor == orange1 && roll < 0.25f)
            {
                return red2;
            }
            else if (firstColor == orange1 && roll < 0.75f)
            {
                return black2;
            }
            else if (firstColor == blue1 && roll < 0.33f)
            {
                return blue3;
            }
            else if (firstColor == aqua1 && roll < 0.5f)
            {
                return blue2;
            }
            else 
            {
                return colorPairs.TryGetValue(firstColor);
            }
        }
        
        //Changing the colors1 here also requires changing the color in the Race_Naga.xml
        public static Color orange1 = new Color(0.875f, 0.75f, 0.625f);
        public static Color orange2 = new Color(0.875f, 0.5f, 0.125f);

        public static Color yellow1 = new Color(0.9f, 0.9f, 0.5f);
        public static Color lime2 = new Color(0.375f, 0.75f, 0.25f);

        public static Color green1 = new Color(0.625f, 0.875f, 0.625f);
        public static Color green2 = new Color(0.25f, 0.625f, 0.25f);

        public static Color black1 = new Color(0.5f, 0.5f, 0.5f);
        public static Color black2 = new Color(0.3f, 0.3f, 0.3f);

        public static Color red1 = new Color(0.75f, 0.375f, 0.375f);
        public static Color red2 = new Color(0.625f, 0.125f, 0.125f);

        public static Color white1 = new Color(1f, 1f, 0.9f);
        public static Color white2 = new Color(0.9f, 0.9f, 0.9f);

        public static Color blue1 = new Color(0.675f, 0.875f, 1f);
        public static Color blue2 = new Color(0.25f, 0.5f, 0.875f);
        public static Color blue3 = new Color(0.375f, 0.375f, 0.75f);

        public static Color purple1 = new Color(0.75f, 0.625f, 0.875f);
        public static Color purple2 = new Color(0.5f, 0.25f, 0.75f);

        public static Color aqua1 = new Color(0.625f, 0.875f, 0.75f);
        public static Color aqua2 = new Color(0.25f, 0.75f, 0.5f);

        public static Dictionary<Color, Color> colorPairs = new Dictionary<Color, Color>
        {
            { orange1, orange2 },
            { yellow1, lime2 },
            { green1, green2 },
            { black1, black2 },
            { red1, red2 },
            { white1, white2 },
            { blue1, blue2 },
            { purple1, purple2 },
            { aqua1, aqua2 }
            /*{ green1, aqua2 },
            { yellow1, green2 },
            { orange1, red2 }*/
        };
    }
}
