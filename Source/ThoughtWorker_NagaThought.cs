using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;

namespace SyrNaga
{
    public class ThoughtWorker_NagaThought : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                return false;
            }
            if (pawn.def != ThingDefOf.Human || other.def != NagaDefOf.Naga)
            {
                return false;
            }
            return true;
        }
    }
}
