using System.Reflection;

using HarmonyLib;

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

        public static void Debug(string message)
        {
            if(Prefs.DevMode)
            {
                Log("DEBUG", message);
            }
        }

        public static void Info(string message) => Log("INFO", message);

        public static void Log(string level, string message) => Verse.Log.Message($"{level.ToUpper()} {ID} :: {message}");

        public static void Severe(string message) => Log("SEVERE", message);

        public static void Warn(string message) => Log("WARN", message);

        public override void DoSettingsWindowContents(Rect inRect) => Settings.Draw(inRect);

        public override string SettingsCategory() => ID;
    }

    [StaticConstructorOnStartup]
    public class RuntimeChecker
    {
        static RuntimeChecker()
        {
            RDA.Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
