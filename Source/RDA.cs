using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SirRandoo.RDA
{
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

        public override string SettingsCategory()
        {
            return Id;
        }
    }

    [StaticConstructorOnStartup]
    public class RdaStatic
    {
        internal static readonly SoundDef GizmoSound;
        internal static readonly MethodInfo EnableAndInit;
        internal static readonly MethodInfo EnableAndInitIf;
        internal static readonly FieldInfo PawnMindState;
        internal static readonly FieldInfo PawnWorkSettings;

        static RdaStatic()
        {
            GizmoSound = SoundDef.Named("Click");
            EnableAndInit = AccessTools.Method(
                typeof(Pawn_WorkSettings),
                nameof(Pawn_WorkSettings.EnableAndInitialize)
            );
            EnableAndInitIf = AccessTools.Method(
                typeof(Pawn_WorkSettings),
                nameof(Pawn_WorkSettings.EnableAndInitializeIfNotAlreadyInitialized)
            );
            PawnMindState = AccessTools.Field(typeof(Pawn), "mindState");
            PawnWorkSettings = AccessTools.Field(typeof(Pawn), "workSettings");

            Rda.Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
