using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace SirRandoo.RDA
{
    [StaticConstructorOnStartup]
    public static class MemoryHelper
    {
        internal static readonly FieldInfo WorkSettingsMap;

        static MemoryHelper()
        {
            WorkSettingsMap = typeof(Pawn_WorkSettings).GetField(
                "priorities",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
        }

        public static DefMap<WorkTypeDef, int> Copy(this DefMap<WorkTypeDef, int> m)
        {
            var map = new DefMap<WorkTypeDef, int>();

            foreach (KeyValuePair<WorkTypeDef, int> pair in m)
            {
                map[pair.Key] = pair.Value;
            }

            return map;
        }
    }
}
