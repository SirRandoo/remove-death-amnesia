using System.Reflection;

using Harmony;

using Verse;

namespace SirRandoo.RDA
{
    public class RDA : Mod
    {
        public static string ID => "Remove Death Amnesia";
        internal static HarmonyInstance Harmony;

        public RDA(ModContentPack content) : base(content)
        {
            Harmony = HarmonyInstance.Create("com.sirrandoo.rda");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Message(string.Format("{0} :: Initialized!", ID));
        }
    }
}
