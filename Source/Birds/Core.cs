using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Birds;

[StaticConstructorOnStartup]
public static class Core
{
    private static Dictionary<Pawn, CompFlyingPawn> cachedComps = new Dictionary<Pawn, CompFlyingPawn>();

    static Core()
    {
        new Harmony("Birds.Mod").PatchAll(Assembly.GetExecutingAssembly());
    }

    public static bool IsFlyingPawn(this Pawn pawn, out CompFlyingPawn comp)
    {
        comp = null;
        if (pawn == null)
        {
            return false;
        }

        if (cachedComps == null)
        {
            cachedComps = [];
        }

        if (!cachedComps.TryGetValue(pawn, out comp))
        {
            cachedComps[pawn] = pawn.TryGetComp<CompFlyingPawn>();
        }

        comp = cachedComps[pawn];
        return comp != null;
    }
}