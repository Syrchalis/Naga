using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;

namespace SyrNaga
{
    public class SkillNeed_DirectNaga : SkillNeed
    {
        public override float ValueFor(Pawn pawn)
        {
            if (pawn.skills == null)
            {
                return 1f;
            }
            int level = pawn.skills.GetSkill(skill).Level;
            if (pawn.def == NagaDefOf.Naga)
            {
                if (valuesPerLevelNaga.Count > level)
                {
                    return valuesPerLevelNaga[level];
                }
                if (valuesPerLevelNaga.Count > 0)
                {
                    return valuesPerLevelNaga[valuesPerLevelNaga.Count - 1];
                }
            }
            else
            {
                if (valuesPerLevel.Count > level)
                {
                    return valuesPerLevel[level];
                }
                if (valuesPerLevel.Count > 0)
                {
                    return valuesPerLevel[valuesPerLevel.Count - 1];
                }
            }
            return 1f;
        }
        public List<float> valuesPerLevel = new List<float>();
        public List<float> valuesPerLevelNaga = new List<float>();
    }
}
