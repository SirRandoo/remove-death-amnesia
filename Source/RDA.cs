using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace SirRandoo.RDA;

[PublicAPI]
public class Rda : Mod
{
    internal static Harmony Harmony;
    public static string Id = "Remove Death Amnesia";

    public Rda(ModContentPack content) : base(content)
    {
        Harmony = new Harmony("com.sirrandoo.rda");

        GetSettings<Settings>();
        Id = content.Name;
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Settings.Draw(inRect);
    }

    public override string SettingsCategory() => Id;
}

[StaticConstructorOnStartup]
internal static class RdaStatic
{
    internal static readonly SoundDef GizmoSound = SoundDef.Named("Click");
    internal static readonly MethodInfo EnableAndInit = AccessTools.Method(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.EnableAndInitialize));

    internal static readonly MethodInfo EnableAndInitIf = AccessTools.Method(
        typeof(Pawn_WorkSettings),
        nameof(Pawn_WorkSettings.EnableAndInitializeIfNotAlreadyInitialized)
    );

    internal static readonly FieldInfo PawnMindState = AccessTools.Field(typeof(Pawn), "mindState");
    internal static readonly FieldInfo PawnWorkSettings = AccessTools.Field(typeof(Pawn), "workSettings");

    static RdaStatic()
    {
        Rda.Harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}
