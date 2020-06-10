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
        
        // Wild man
        private bool _recentlyTamed;

        private Command_Action _restoreMemory;
        private Command_Action _storeMemory;

        public Pawn Parent
        {
            get
            {
                _parentAsPawn ??= parent as Pawn;

                return _parentAsPawn;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!Prefs.DevMode && Parent == null)
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

            if ((_lastBed?.Spawned ?? false) && Parent.CurrentBed() != _lastBed)
            {
                Parent.ownership.ClaimBedIfNonMedical(_lastBed);
            }

            if (_lastOutfit != null && Parent.outfits.CurrentOutfit != _lastOutfit)
            {
                Parent.outfits.CurrentOutfit = _lastOutfit;
            }

            if (_lastFoodRestriction != null && Parent.foodRestriction.CurrentFoodRestriction != _lastFoodRestriction)
            {
                Parent.foodRestriction.CurrentFoodRestriction = _lastFoodRestriction;
            }

            if (_lastDrugPolicy != null && Parent.drugs.CurrentPolicy != _lastDrugPolicy)
            {
                Parent.drugs.CurrentPolicy = _lastDrugPolicy;
            }

            if (!_lastSchedule.NullOrEmpty() && Parent.timetable.times != _lastSchedule)
            {
                Parent.timetable.times = _lastSchedule;
            }

            if (Parent.playerSettings != null)
            {
                Parent.playerSettings.AreaRestriction = _lastAllowedArea;
                Parent.playerSettings.displayOrder = _lastDisplayOrder;
                Parent.playerSettings.hostilityResponse = _lastHostilityResponse;
                Parent.playerSettings.medCare = _lastMedCareCategory;
                Parent.playerSettings.selfTend = _lastSelfTendSetting;
            }

            if (_workCache != null)
            {
                Parent.workSettings.EnableAndInitialize();
                
                foreach (var work in _workCache)
                {
                    Parent.workSettings.SetPriority(work.Key, work.Value);
                }

                Logger.Info(_workCache.ToStringSafeEnumerable());
                Logger.Info(Parent.workSettings.DebugString());
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

            if (Parent.CurrentBed()?.Spawned ?? false)
            {
                _lastBed = Parent.CurrentBed();
            }

            if (Parent.outfits.CurrentOutfit != null)
            {
                _lastOutfit = Parent.outfits.CurrentOutfit;
            }

            if (Parent.foodRestriction.CurrentFoodRestriction != null)
            {
                _lastFoodRestriction = Parent.foodRestriction.CurrentFoodRestriction;
            }

            if (Parent.drugs.CurrentPolicy != null)
            {
                _lastDrugPolicy = Parent.drugs.CurrentPolicy;
            }

            if (!Parent.timetable.times.NullOrEmpty())
            {
                _lastSchedule = Parent.timetable.times.ToList();
            }

            if (Parent.playerSettings != null)
            {
                _lastAllowedArea = Parent.playerSettings.AreaRestriction;
                _lastDisplayOrder = Parent.playerSettings.displayOrder;
                _lastHostilityResponse = Parent.playerSettings.hostilityResponse;
                _lastMedCareCategory = Parent.playerSettings.medCare;
                _lastSelfTendSetting = Parent.playerSettings.selfTend;
            }

            if (Parent.workSettings != null)
            {
                _workCache = (MemoryHelper.WorkSettingsMap.GetValue(Parent.workSettings) as DefMap<WorkTypeDef, int>)?.Copy();
                
                Logger.Info(_workCache.ToStringSafeEnumerable());
                Logger.Info(Parent.workSettings.DebugString());
            }
        }

        internal bool CanRestoreMemory()
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
            Scribe_Values.Look(ref _recentlyTamed, "wasRecentlyTamed");
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
