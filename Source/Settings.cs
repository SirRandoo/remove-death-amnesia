using UnityEngine;


using Verse;

namespace SirRandoo.RDA
{
    public class Settings : ModSettings
    {
        private static Vector2 ScrollPos = Vector2.zero;

        public static bool Priorities = true;
        public static bool Bills = false;

        public static void Draw(Rect canvas)
        {
            var panel = new Listing_Standard();

            var view = new Rect(0f, 0f, canvas.width, 36f * 26f);
            view.xMax *= 0.9f;

            panel.BeginScrollView(canvas, ref ScrollPos, ref view);

            panel.Label("RDA.Groups.General.Label".Translate(), tooltip: TranslateIf("RDA.Groups.General.Tooltip"));
            panel.GapLine();

            panel.CheckboxLabeled("RDA.Settings.Priorities.Label".Translate(), ref Priorities, TranslateIf("RDA.Settings.Priorities.Tooltip"));
            panel.Gap(24);

            panel.Label("RDA.Groups.Experimental.Label".Translate(), tooltip: TranslateIf("RDA.Groups.Experimental.Tooltip"));
            panel.GapLine();

            panel.CheckboxLabeled("RDA.Settings.Bills.Label".Translate(), ref Bills, TranslateIf("RDA.Settings.Bills.Tooltip"));

            panel.EndScrollView(ref view);
        }

        private static string TranslateIf(string s, params object[] args)
        {
            if(!s.CanTranslate()) return null;

            return s.Translate(args);
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Priorities, "workPriorities", true);

            Scribe_Values.Look(ref Bills, "bills", false);
        }
    }
}
