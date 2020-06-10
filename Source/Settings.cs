using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#pragma warning disable 618

namespace SirRandoo.RDA
{
    internal enum Pages
    {
        General
        //Experimental
    }

    public class Settings : ModSettings
    {
        private static string _lastPage = RDA.Pages.General.ToString();
        private static readonly List<string> Pages = Enum.GetNames(typeof(Pages)).ToList();
        private static readonly int PageCount = Pages.Count;

        private static Vector2 _menuScrollPos = Vector2.zero;
        private static Vector2 _pageScrollPos = Vector2.zero;

        public static bool Priorities = true;
        public static bool Bills;
        public static bool Beds;
        public static bool DrugPolicies;
        public static bool FoodRestrictions;
        public static bool Outfits;
        public static bool Timetables;
        public static bool SelfTend;
        public static bool DisplayOrder;
        public static bool MedicalCare;
        public static bool HostilityResponse;
        public static bool Area;

        public static void Draw(Rect canvas)
        {
            GUI.BeginGroup(canvas);
            var menuRect = new Rect(0f, 0f, canvas.width * 0.25f, canvas.height);
            var menuInnerRect = new Rect(0f, 0f, menuRect.width - 8f, menuRect.height - 8f);
            var menuView = new Rect(0f, 0f, menuInnerRect.width - 16f, Text.LineHeight * PageCount);
            var pageRect = new Rect(
                menuRect.x + menuRect.width + 5f,
                0f,
                canvas.width - menuRect.width - 5f,
                canvas.height
            );
            var pageInnerRect = new Rect(0f, 0f, pageRect.width, pageRect.height);
            var pageView = new Rect(0f, 0f, pageRect.width - 16f, Text.LineHeight * 13);
            var listing = new Listing_Standard();

            Widgets.DrawMenuSection(menuRect);
            listing.Begin(menuRect.ContractedBy(4f));
            listing.BeginScrollView(menuInnerRect, ref _menuScrollPos, ref menuView);
            foreach (var page in Pages)
            {
                var lineRect = listing.GetRect(Text.LineHeight);

                if (_lastPage == page)
                {
                    Widgets.DrawHighlightSelected(lineRect);
                }

                Widgets.Label(lineRect, $"RDA.Settings.{page}".Translate());

                if (Widgets.ButtonInvisible(lineRect))
                {
                    _lastPage = page;
                }
            }

            listing.End();
            listing.EndScrollView(ref menuView);

            listing.Begin(pageRect);
            listing.BeginScrollView(pageInnerRect, ref _pageScrollPos, ref pageView);
            switch (_lastPage)
            {
                case "General":
                    DrawGeneralPage(listing);
                    break;
                case "Experimental":
                    DrawExperimentalPage(listing);
                    break;
            }

            listing.End();
            listing.EndScrollView(ref pageView);

            GUI.EndGroup();
        }

        private static void DrawGeneralPage(Listing_Standard listing)
        {
            listing.CheckboxLabeled("RDA.Settings.General.Bed".Translate(), ref Beds);
            listing.CheckboxLabeled("RDA.Settings.General.DrugPolicy".Translate(), ref DrugPolicies);
            listing.CheckboxLabeled("RDA.Settings.General.FoodRestriction".Translate(), ref FoodRestrictions);
            listing.CheckboxLabeled("RDA.Settings.General.Outfit".Translate(), ref Outfits);
            listing.CheckboxLabeled("RDA.Settings.General.TimeTable".Translate(), ref Timetables);
            listing.CheckboxLabeled("RDA.Settings.General.SelfTend".Translate(), ref SelfTend);
            listing.CheckboxLabeled("RDA.Settings.General.DisplayOrder".Translate(), ref DisplayOrder);
            listing.CheckboxLabeled(
                "RDA.Settings.General.MedicalCare".Translate(),
                ref MedicalCare,
                "RDA.Settings.General.MedicalCare.Tooltip".Translate()
            );
            listing.CheckboxLabeled(
                "RDA.Settings.General.HostilityResponse".Translate(),
                ref HostilityResponse,
                "RDA.Settings.General.HostilityResponse.Tooltip".Translate()
            );
            listing.CheckboxLabeled("RDA.Settings.General.Area".Translate(), ref Area);
            listing.CheckboxLabeled("RDA.Settings.General.Work".Translate(), ref Priorities);
        }

        private static void DrawExperimentalPage(Listing_Standard listing)
        {
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Beds, "beds");
            Scribe_Values.Look(ref DrugPolicies, "drugPolicies", true);
            Scribe_Values.Look(ref FoodRestrictions, "foodRestrictions", true);
            Scribe_Values.Look(ref Outfits, "outfits", true);
            Scribe_Values.Look(ref Timetables, "timetables", true);
            Scribe_Values.Look(ref SelfTend, "selfTend");
            Scribe_Values.Look(ref DisplayOrder, "displayOrder", true);
            Scribe_Values.Look(ref MedicalCare, "medicalCare", true);
            Scribe_Values.Look(ref HostilityResponse, "hostilityResponse", true);
            Scribe_Values.Look(ref Area, "area", true);
            Scribe_Values.Look(ref Priorities, "workPriorities", true);

            Scribe_Values.Look(ref Bills, "bills");
        }
    }
}
