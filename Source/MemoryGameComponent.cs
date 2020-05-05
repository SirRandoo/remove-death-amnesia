using System.Collections.Generic;
using RimWorld;
using Verse;

namespace SirRandoo.RDA
{
    public class MemoryGameComponent : GameComponent
    {
        private readonly Dictionary<NameTriple, Pawn_WorkSettings> _workBank = new Dictionary<NameTriple, Pawn_WorkSettings>();
        private readonly Dictionary<NameTriple, Pawn_PlayerSettings> _playerBank = new Dictionary<NameTriple, Pawn_PlayerSettings>();
        private readonly Dictionary<NameTriple, Building_Bed> _beds = new Dictionary<NameTriple, Building_Bed>();
        private readonly Dictionary<NameTriple, Outfit> _outfits = new Dictionary<NameTriple, Outfit>();
        private readonly Dictionary<NameTriple, DrugPolicy> _policies = new Dictionary<NameTriple, DrugPolicy>();
        private readonly Dictionary<NameTriple, FoodRestriction> _restrictions = new Dictionary<NameTriple, FoodRestriction>();
        private readonly Dictionary<NameTriple, List<TimeAssignmentDef>> _times = new Dictionary<NameTriple, List<TimeAssignmentDef>>();

        public MemoryGameComponent(Game game)
        {
            
        }

        private static Pawn_PlayerSettings CopyPlayerSettings(Pawn pawn, Pawn_PlayerSettings settings = null)
        {
            if (settings == null)
            {
                settings = pawn.playerSettings;
            }

            var t = new Pawn_PlayerSettings(pawn)
            {
                AreaRestriction = settings.AreaRestriction,
                medCare = settings.medCare,
                selfTend = settings.selfTend,
                displayOrder = settings.displayOrder,
                joinTick = settings.joinTick,
                hostilityResponse = settings.hostilityResponse
            };

            return t;
        }

        private static Pawn_WorkSettings CopyWorkSettings(Pawn pawn, Pawn_WorkSettings settings = null)
        {
            if (settings == null)
            {
                settings = pawn.workSettings;
            }
            
            var t = new Pawn_WorkSettings(pawn);
            t.EnableAndInitializeIfNotAlreadyInitialized();

            foreach (var work in settings.WorkGiversInOrderNormal)
            {
                t.SetPriority(work.def.workType, settings.GetPriority(work.def.workType));
            }

            return t;
        }

        public void TryRestoreSettings(Pawn pawn)
        {
            if (!_playerBank.TryGetValue((NameTriple) pawn.Name, out var settings))
            {
                return;
            }
            
            Logger.Info($"Attempting to restore player settings for {pawn.LabelCap}...");
            pawn.playerSettings = CopyPlayerSettings(pawn, settings);
        }

        public void TryRestoreWork(Pawn pawn)
        {
            if (!_workBank.TryGetValue((NameTriple) pawn.Name, out var settings))
            {
                return;
            }
            
            Logger.Info($"Attempting to restore work settings for {pawn.LabelCap}...");
            pawn.workSettings = CopyWorkSettings(pawn, settings);
        }

        public void TryRestoreBed(Pawn pawn)
        {
            if (!_beds.TryGetValue((NameTriple) pawn.Name, out var bed))
            {
                return;
            }

            if (bed.OwnersForReading.Contains(pawn))
            {
                return;
            }

            if (bed.ForPrisoners)
            {
                Logger.Warn($"Could not assign bed @ {bed.Position.ToString()} to pawn {pawn.LabelCap} -- It's for prisoners.");
                return;
            }

            if (!bed.Spawned)
            {
                Logger.Warn($"Could not assign bed @ {bed.Position.ToString()} to pawn {pawn.LabelCap} -- It's not built.");
            }

            if (!pawn.ownership.ClaimBedIfNonMedical(bed))
            {
                Logger.Warn($"Could not assign bed @ {bed.Position.ToString()} to pawn {pawn.LabelCap} -- It's for medical use.");
            }
        }

        public void TryRestoreOutfit(Pawn pawn)
        {
            if (!_outfits.TryGetValue((NameTriple) pawn.Name, out var outfit))
            {
                return;
            }

            if (!Current.Game.outfitDatabase.AllOutfits.Contains(outfit))
            {
                Logger.Warn($"Attempted to assign a non-existent outfit to {pawn.LabelCap}");

                if (!_outfits.Remove((NameTriple) pawn.Name))
                {
                    Logger.Warn("Could not remove non-existent outfit.");
                }

                return;
            }
            
            Logger.Info($"Attempting to restore outfit for {pawn.LabelCap}...");
            pawn.outfits.CurrentOutfit = outfit;
        }

        public void TryRestorePolicy(Pawn pawn)
        {
            if (!_policies.TryGetValue((NameTriple) pawn.Name, out var policy))
            {
                return;
            }

            if (!Current.Game.drugPolicyDatabase.AllPolicies.Contains(policy))
            {
                Logger.Warn($"Attempted to assign a non-existent policy to {pawn.LabelCap}!");
                
                if(!_policies.Remove((NameTriple) pawn.Name))
                {
                    Logger.Warn("Could not remove non-existent policy.");
                }
                
                return;
            }

            pawn.drugs.CurrentPolicy = policy;
        }

        public void TryRestoreRestriction(Pawn pawn)
        {
            if (!_restrictions.TryGetValue((NameTriple) pawn.Name, out var restriction))
            {
                return;
            }

            if (!Current.Game.foodRestrictionDatabase.AllFoodRestrictions.Contains(restriction))
            {
                Logger.Warn($"Attempted to assign a non-existent food restriction to {pawn.LabelCap}!");

                if (!_restrictions.Remove((NameTriple) pawn.Name))
                {
                    Logger.Warn("Could not remove non-existent food restriction.");
                }

                return;
            }

            pawn.foodRestriction.CurrentFoodRestriction = restriction;
        }

        public void TryRestoreTimetable(Pawn pawn)
        {
            if (!_times.TryGetValue((NameTriple) pawn.Name, out var timetable))
            {
                return;
            }

            pawn.timetable.times = timetable.ListFullCopy();
        }

        public void SaveBed(Pawn pawn, Building_Bed bed = null)
        {
            _beds[(NameTriple) pawn.Name] = bed ?? pawn.ownership.OwnedBed;
        }

        public void SaveWorkSettings(Pawn pawn, Pawn_WorkSettings settings = null)
        {
            _workBank[(NameTriple) pawn.Name] = CopyWorkSettings(pawn, settings);
        }

        public void SavePlayerSettings(Pawn pawn, Pawn_PlayerSettings settings = null)
        {
            _playerBank[(NameTriple) pawn.Name] = CopyPlayerSettings(pawn, settings);
        }

        public void SaveOutfit(Pawn pawn, Outfit outfit = null)
        {
            _outfits[(NameTriple) pawn.Name] = outfit ?? pawn.outfits.CurrentOutfit;
        }

        public void SaveDrugPolicy(Pawn pawn, DrugPolicy policy = null)
        {
            _policies[(NameTriple) pawn.Name] = policy ?? pawn.drugs.CurrentPolicy;
        }

        public void SaveFoodRestriction(Pawn pawn, FoodRestriction restriction = null)
        {
            _restrictions[(NameTriple) pawn.Name] = restriction ?? pawn.foodRestriction.CurrentFoodRestriction;
        }

        public void SaveTimeRestriction(Pawn pawn, List<TimeAssignmentDef> timetable = null)
        {
            _times[(NameTriple) pawn.Name] = timetable ?? pawn.timetable.times.ListFullCopy();
        }

        public bool DeleteBed(Pawn pawn)
        {
            return _beds.Remove((NameTriple) pawn.Name);
        }

        public bool DeleteWorkSettings(Pawn pawn)
        {
            return _workBank.Remove((NameTriple) pawn.Name);
        }

        public bool DeletePlayerSettings(Pawn pawn)
        {
            return _playerBank.Remove((NameTriple) pawn.Name);
        }

        public bool DeleteOutfit(Pawn pawn)
        {
            return _outfits.Remove((NameTriple) pawn.Name);
        }

        public bool DeleteDrugPolicy(Pawn pawn)
        {
            return _policies.Remove((NameTriple) pawn.Name);
        }

        public bool DeleteFoodRestriction(Pawn pawn)
        {
            return _restrictions.Remove((NameTriple) pawn.Name);
        }

        public bool DeleteTimeRestriction(Pawn pawn)
        {
            return _times.Remove((NameTriple) pawn.Name);
        }
    }
}
