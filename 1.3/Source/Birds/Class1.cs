using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Birds
{
    public class CompProperties_FlyingPawn : CompProperties
    {
        public bool flyWhenFleeing;

        public float flyingMoveSpeedMultiplier;

        public List<GraphicData> flyingBodyGraphicData;

        public List<GraphicData> flyingFemaleBodyGraphicData;

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            if (flyingBodyGraphicData != null)
            {
                foreach (var graphicData in flyingBodyGraphicData)
                {
                    graphicData.graphicClass = typeof(Graphic_Multi);
                }
            }
            if (flyingFemaleBodyGraphicData != null)
            {
                foreach (var graphicData in flyingFemaleBodyGraphicData)
                {
                    graphicData.graphicClass = typeof(Graphic_Multi);
                }
            }
        }
        public CompProperties_FlyingPawn()
        {
            compClass = typeof(CompFlyingPawn);
        }
    }

    public class CompFlyingPawn : ThingComp
    {
        public CompProperties_FlyingPawn Props => base.props as CompProperties_FlyingPawn;
    }
    [StaticConstructorOnStartup]
    public static class Core
    {
        static Core()
        {
            var harmony = new Harmony("Birds.Mod");
            harmony.PatchAll();
        }

        private static Dictionary<Pawn, CompFlyingPawn> cachedComps = new Dictionary<Pawn, CompFlyingPawn>();
        public static bool IsFlyingPawn(this Pawn pawn, out CompFlyingPawn comp)
        {
            if (!cachedComps.TryGetValue(pawn, out comp))
            {
                cachedComps[pawn] = comp = pawn.TryGetComp<CompFlyingPawn>();
            }
            return comp != null;
        }
        public static void SetGraphic(this Pawn pawn, GraphicData graphicData)
        {
            Log.Message("pawn.Drawer.renderer.graphics.nakedGraphic: " + pawn.Drawer.renderer.graphics.nakedGraphic);
            Log.Message("Shader: " + pawn.Drawer.renderer.graphics.nakedGraphic?.Shader);
            pawn.Drawer.renderer.graphics.nakedGraphic = GraphicDatabase.Get(graphicData.graphicClass, graphicData.texPath, graphicData.shaderType?.Shader ?? ShaderDatabase.CutoutComplex, graphicData.drawSize, graphicData.color, graphicData.colorTwo);
            PortraitsCache.SetDirty(pawn);
            PortraitsCache.PortraitsCacheUpdate();
            GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
    public class Pawn_JobTracker_StartJob_Patch
    {
        private static void Postfix(Pawn_JobTracker __instance, Pawn ___pawn, Job newJob, JobTag? tag)
        {
            if (___pawn.IsFlyingPawn(out var comp))
            {
                var curJob = ___pawn.CurJobDef;
                if (comp.Props.flyWhenFleeing && (curJob == JobDefOf.Flee || curJob == JobDefOf.FleeAndCower))
                {
                    var curKindLifeStage = ___pawn.ageTracker.CurKindLifeStage;
                    var curKindLifeStageInd = ___pawn.ageTracker.CurLifeStageIndex;
                    if (___pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null)
                    {
                        ___pawn.SetGraphic(comp.Props.flyingBodyGraphicData[curKindLifeStageInd]);
                    }
                    else
                    {
                        ___pawn.SetGraphic(comp.Props.flyingFemaleBodyGraphicData[curKindLifeStageInd]);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "CleanupCurrentJob")]
    public class Pawn_JobTracker_CleanupCurrentJob_Patch
    {
        public static void Prefix(Pawn_JobTracker __instance, Pawn ___pawn)
        {
            if (___pawn.IsFlyingPawn(out var comp))
            {
                var curJob = ___pawn.CurJobDef;
                if (comp.Props.flyWhenFleeing && (curJob == JobDefOf.Flee || curJob == JobDefOf.FleeAndCower))
                {
                    PawnKindLifeStage curKindLifeStage = ___pawn.ageTracker.CurKindLifeStage;
                    if (___pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null)
                    {
                        ___pawn.SetGraphic(curKindLifeStage.bodyGraphicData);
                    }
                    else
                    {
                        ___pawn.SetGraphic(curKindLifeStage.femaleGraphicData);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue))]
    public static class GetStatValue_Patch
    {
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(Thing thing, StatDef stat, bool applyPostProcess, ref float __result)
        {
            if (stat == StatDefOf.MoveSpeed && thing is Pawn pawn && pawn.IsFlyingPawn(out var comp))
            {
                var curJob = pawn.CurJobDef;
                if (comp.Props.flyWhenFleeing && (curJob == JobDefOf.Flee || curJob == JobDefOf.FleeAndCower))
                {
                    __result *= comp.Props.flyingMoveSpeedMultiplier;
                }
            }
        }
    }
}
