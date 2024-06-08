using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Birds;

[StaticConstructorOnStartup]
public static class Core
{
    private static readonly Dictionary<Pawn, CompFlyingPawn> cachedComps = new Dictionary<Pawn, CompFlyingPawn>();

    static Core()
    {
        new Harmony("Birds.Mod").PatchAll(Assembly.GetExecutingAssembly());
    }

    public static bool IsFlyingPawn(this Pawn pawn, out CompFlyingPawn comp)
    {
        if (!cachedComps.TryGetValue(pawn, out comp))
        {
            cachedComps[pawn] = comp = pawn.TryGetComp<CompFlyingPawn>();
        }

        return comp != null;
    }
}