using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace SirRandoo.RDA
{
    public class MemoryThingComp : ThingComp
    {
        private Pawn _parentAsPawn;
        private Building_Bed _lastBed;
        private DrugPolicy _lastDrugPolicy;
        private FoodRestriction _lastFoodRestriction;
        private Outfit _lastOutfit;
        private List<TimeAssignmentDef> _lastSchedule;

        // Player settings
        private bool _lastSelfTendSetting;
        private int _lastDisplayOrder;
        private MedicalCareCategory _lastMedCareCategory;
        private HostilityResponseMode _lastHostilityResponse;
        private Area _lastAllowedArea;
        
        // Work settings
        private DefMap<WorkTypeDef, int> _workCache;

        private Command_Action _restoreMemory;
        private Command_Action _storeMemory;

        private Pawn Parent
        {
            get
            {
                _parentAsPawn ??= parent as Pawn;

                return _parentAsPawn;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!Prefs.DevMode || Parent == null)
            {
                yield break;
            }

            _restoreMemory ??= new Command_Action
            {
                action = TryRestoreMemory,
                activateSound = RdaStatic.GizmoSound,
                defaultLabel = "RDA.Gizmos.Restore.Label".Translate(),
                defaultDesc = "RDA.Gizmos.Restore.Description".Translate()
            };

            _storeMemory ??= new Command_Action
            {
                action = TryStoreMemory,
                activateSound = RdaStatic.GizmoSound,
                defaultLabel = "RDA.Gizmos.Store.Label".Translate(),
                defaultDesc = "RDA.Gizmos.Store.Description".Translate()
            };


            yield return _storeMemory;

            if (!CanRestoreMemory())
            {
                yield break;
            }

            yield return _restoreMemory;
        }

        internal void TryRestoreMemory()
        {
            if (parent == null)
            {
                Logger.Warn("Could not restore memory; parent was null!");
                return;
            }

            if (Parent == null)
            {
                Logger.Warn("MemoryComp applied to a non-pawn! Only pawns should have this comp.");
                return;
            }

            if (Settings.Beds && (_lastBed?.Spawned ?? false) && Parent.CurrentBed() != _lastBed)
            {
                Parent.ownership.ClaimBedIfNonMedical(_lastBed);
            }

            if (Settings.Outfits && _lastOutfit != null && Parent.outfits.CurrentOutfit != _lastOutfit && Current.Game.outfitDatabase.AllOutfits.Any(o => _lastOutfit.uniqueId.Equals(o.uniqueId)))
            {
                Parent.outfits.CurrentOutfit = _lastOutfit;
            }

            if (Settings.FoodRestrictions && _lastFoodRestriction != null && Parent.foodRestriction.CurrentFoodRestriction != _lastFoodRestriction && Current.Game.foodRestrictionDatabase.AllFoodRestrictions.Any(f => _lastFoodRestriction.id.Equals(f.id)))
            {
                Parent.foodRestriction.CurrentFoodRestriction = _lastFoodRestriction;
            }

            if (Settings.DrugPolicies && _lastDrugPolicy != null && Parent.drugs.CurrentPolicy != _lastDrugPolicy && Current.Game.drugPolicyDatabase.AllPolicies.Any(p => _lastDrugPolicy.uniqueId.Equals(p.uniqueId)))
            {
                Parent.drugs.CurrentPolicy = _lastDrugPolicy;
            }

            if (Settings.Timetables && !_lastSchedule.NullOrEmpty() && Parent.timetable.times != _lastSchedule)
            {
                Parent.timetable.times = _lastSchedule;
            }

            if (Parent.playerSettings != null)
            {
                if(Settings.Area && _lastAllowedArea.Map.areaManager.AllAreas.Any(a => _lastAllowedArea.ID.Equals(a.ID)))
                {
                    Parent.playerSettings.AreaRestriction = _lastAllowedArea;
                }

                if(Settings.DisplayOrder)
                {
                    Parent.playerSettings.displayOrder = _lastDisplayOrder;
                }

                if(Settings.HostilityResponse)
                {
                    Parent.playerSettings.hostilityResponse = _lastHostilityResponse;
                }

                if(Settings.MedicalCare)
                {
                    Parent.playerSettings.medCare = _lastMedCareCategory;
                }

                if(Settings.SelfTend)
                {
                    Parent.playerSettings.selfTend = _lastSelfTendSetting;
                }
            }

            if (Settings.Priorities && _workCache != null)
            {
                Parent.workSettings.EnableAndInitialize();
                
                foreach (var work in _workCache)
                {
                    Parent.workSettings.SetPriority(work.Key, work.Value);
                }
            }
        }

        internal void TryStoreMemory()
        {
            if (parent == null)
            {
                Logger.Warn("Could not store memory; parent was null!");
                return;
            }

            if (Parent == null)
            {
                Logger.Warn("MemoryComp applied to a non-pawn! Only pawns should have this comp.");
                return;
            }

            if (Settings.Beds && (Parent.CurrentBed()?.Spawned ?? false))
            {
                _lastBed = Parent.CurrentBed();
            }

            if (Settings.Outfits && Parent.outfits.CurrentOutfit != null)
            {
                _lastOutfit = Parent.outfits.CurrentOutfit;
            }

            if (Settings.FoodRestrictions && Parent.foodRestriction.CurrentFoodRestriction != null)
            {
                _lastFoodRestriction = Parent.foodRestriction.CurrentFoodRestriction;
            }

            if (Settings.DrugPolicies && Parent.drugs.CurrentPolicy != null)
            {
                _lastDrugPolicy = Parent.drugs.CurrentPolicy;
            }

            if (Settings.Timetables && !Parent.timetable.times.NullOrEmpty())
            {
                _lastSchedule = Parent.timetable.times.ToList();
            }

            if (Parent.playerSettings != null)
            {
                if(Settings.Area)
                {
                    _lastAllowedArea = Parent.playerSettings.AreaRestriction;
                }

                if(Settings.DisplayOrder)
                {
                    _lastDisplayOrder = Parent.playerSettings.displayOrder;
                }

                if(Settings.HostilityResponse)
                {
                    _lastHostilityResponse = Parent.playerSettings.hostilityResponse;
                }

                if(Settings.MedicalCare)
                {
                    _lastMedCareCategory = Parent.playerSettings.medCare;
                }

                if(Settings.SelfTend)
                {
                    _lastSelfTendSetting = Parent.playerSettings.selfTend;
                }
            }

            if (Settings.Priorities && Parent.workSettings != null)
            {
                _workCache = (MemoryHelper.WorkSettingsMap.GetValue(Parent.workSettings) as DefMap<WorkTypeDef, int>)?.Copy();
            }
        }

        private bool CanRestoreMemory()
        {
            if (Parent == null)
            {
                return false;
            }

            return _lastBed != null
                   || _lastDrugPolicy != null
                   || _lastFoodRestriction != null
                   || _lastOutfit != null
                   || !_lastSchedule.NullOrEmpty()
                   || _lastAllowedArea != null
                   || _workCache != null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look(ref _lastBed, "lastKnownBed");
            Scribe_References.Look(ref _lastOutfit, "lastKnownOutfit");
            Scribe_Collections.Look(ref _lastSchedule, "lastKnownSchedule", LookMode.Def);
            Scribe_Deep.Look(ref _workCache, "lastKnownWorkSchedule");
            Scribe_References.Look(ref _lastDrugPolicy, "lastKnownDrugPolicy");
            Scribe_Values.Look(ref _lastSelfTendSetting, "lastKnownSelfTendSetting");
            Scribe_Values.Look(ref _lastDisplayOrder, "lastKnownDisplayOrder");
            Scribe_References.Look(ref _lastAllowedArea, "lastKnownAllowedArea");
            Scribe_Values.Look(ref _lastMedCareCategory, "lastKnownMedicalCategory");
            Scribe_Values.Look(ref _lastHostilityResponse, "lastKnownHostilityResponse");
            Scribe_References.Look(ref _lastFoodRestriction, "lastKnownFoodRestriction");
        }

        internal DefMap<WorkTypeDef, int> GetWorkSettings()
        {
            return _workCache;
        }

        public static bool ShouldRemember(Pawn pawn)
        {
            return pawn?.IsColonistPlayerControlled ?? false;
        }
    }
}
