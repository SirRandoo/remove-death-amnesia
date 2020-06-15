using System.Reflection;
using HarmonyLib;
using RimWorld;
using SirRandoo.RDA.Patches;
using UnityEngine;
using Verse;

namespace SirRandoo.RDA
{
    public class RDA : Mod
    {
        internal static Harmony Harmony;

        public RDA(ModContentPack content) : base(content)
        {
            Harmony = new Harmony("com.sirrandoo.rda");

            GetSettings<Settings>();
        }

        public static string ID => "Remove Death Amnesia";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.Draw(inRect);
        }

        public override string SettingsCategory()
        {
            return ID;
        }
    }

    [StaticConstructorOnStartup]
    public class RuntimeChecker
    {
        internal static readonly MethodInfo RimBillNotify;
        internal static readonly MethodInfo RdaBillNotify;
        internal static readonly MethodInfo EnableAndInit;
        internal static readonly MethodInfo EnableAndInitIf;
        internal static readonly FieldInfo PawnMindState;
        internal static readonly FieldInfo PawnWorkSettings;
        
        static RuntimeChecker()
        {
            RimBillNotify = AccessTools.Method(typeof(BillUtility), nameof(BillUtility.Notify_ColonistUnavailable));
            RdaBillNotify = AccessTools.Method(typeof(Pawn__Kill), nameof(Pawn__Kill.Notify_ColonistUnavailable));
            EnableAndInit = AccessTools.Method(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.EnableAndInitialize));
            EnableAndInitIf = AccessTools.Method(
                typeof(Pawn_WorkSettings),
                nameof(Pawn_WorkSettings.EnableAndInitializeIfNotAlreadyInitialized)
            );
            PawnMindState = AccessTools.Field(typeof(Pawn), "mindState");
            PawnWorkSettings = AccessTools.Field(typeof(Pawn), "workSettings");
            
            RDA.Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
