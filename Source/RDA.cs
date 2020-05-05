using System.Reflection;

using HarmonyLib;

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
        static RdaStatic()
        {
            Rda.Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
