using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace SirRandoo.RDA;

[PublicAPI]
public class MemoryThingComp : ThingComp
{
    private Area? _lastAllowedArea;
    private Building_Bed? _lastBed;
    private int _lastDisplayOrder;
    private DrugPolicy? _lastDrugPolicy;
    private FoodPolicy? _lastFoodRestriction;
    private HostilityResponseMode _lastHostilityResponse;
    private MedicalCareCategory _lastMedCareCategory;
    private ApparelPolicy? _lastOutfit;
    private List<TimeAssignmentDef>? _lastSchedule;

    // Player settings
    private bool _lastSelfTendSetting;
    private Pawn? _parentAsPawn;
    private bool _parentCached;

    private Command_Action? _restoreMemory;
    private Command_Action? _storeMemory;

    // Work settings
    private DefMap<WorkTypeDef, int>? _workCache;

    private Pawn? Parent
    {
        get
        {
            if (_parentCached || _parentAsPawn != null)
            {
                return _parentAsPawn;
            }

            _parentAsPawn = parent as Pawn;
            _parentCached = true;

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
            action = () => TryRestoreMemory(),
            activateSound = RdaStatic.GizmoSound,
            defaultLabel = "RDA.Gizmos.Restore.Label".TranslateSimple(),
            defaultDesc = "RDA.Gizmos.Restore.Description".TranslateSimple()
        };

        _storeMemory ??= new Command_Action
        {
            action = TryStoreMemory,
            activateSound = RdaStatic.GizmoSound,
            defaultLabel = "RDA.Gizmos.Store.Label".TranslateSimple(),
            defaultDesc = "RDA.Gizmos.Store.Description".TranslateSimple()
        };


        yield return _storeMemory;

        if (!CanRestoreMemory())
        {
            yield break;
        }

        yield return _restoreMemory;
    }

    internal void TryRestoreMemory(bool work = true)
    {
        if (Parent == null)
        {
            Logger.Warn("Parent was either null or applied to a non-pawn. MemoryThingComp should only be applied to pawns.");

            return;
        }

        TryRestoreBed();
        TryRestoreOutfit();
        TryRestoreFoodRestriction();
        TryRestoreDrugPolicy();
        TryRestoreTimetable();

        if (work)
        {
            TryRestoreWorkPriorities();
        }

        if (Parent.playerSettings != null)
        {
            TryRestoreArea();
            TryRestoreDisplayOrder();
            TryRestoreHostilityResponse();
            TryRestoreMedicalCare();
            TryRestoreSelfTend();
        }
    }

    internal void TryStoreMemory()
    {
        if (Parent == null)
        {
            Logger.Warn("Parent was either null or applied to a non-pawn. MemoryThingComp should only be applied to pawns.");

            return;
        }

        TryStoreBed();
        TryStoreOutfit();
        TryStoreFoodRestriction();
        TryStoreDrugPolicy();
        TryStoreTimetable();
        TryStoreWorkPriorities();

        if (Parent.playerSettings != null)
        {
            TryStoreArea();
            TryStoreDisplayOrder();
            TryStoreHostilityResponse();
            TryStoreMedicalCare();
            TryStoreSelfTend();
        }
    }

    private bool CanRestoreMemory()
    {
        if (Parent == null)
        {
            return false;
        }

        if (_lastBed != null)
        {
            return true;
        }

        if (_lastDrugPolicy != null)
        {
            return true;
        }

        if (_lastFoodRestriction != null)
        {
            return true;
        }

        if (_lastOutfit != null)
        {
            return true;
        }

        if (!_lastSchedule.NullOrEmpty())
        {
            return true;
        }

        if (_lastAllowedArea != null)
        {
            return true;
        }

        return _workCache != null;
    }

    internal void TryRestoreBed()
    {
        if (!Settings.Beds)
        {
            return;
        }

        if (!_lastBed?.Spawned ?? true)
        {
            return;
        }

        if (Parent.CurrentBed() == _lastBed)
        {
            return;
        }

        Parent?.ownership.ClaimBedIfNonMedical(_lastBed);
    }

    internal void TryRestoreOutfit()
    {
        if (!Settings.Outfits)
        {
            return;
        }

        if (_lastOutfit == null)
        {
            return;
        }

        if (!Current.Game.outfitDatabase.AllOutfits.Any(f => _lastOutfit.id.Equals(f.id)))
        {
            _lastOutfit = null;

            return;
        }

        if (Parent?.outfits.CurrentApparelPolicy == _lastOutfit)
        {
            return;
        }

        if (Parent != null)
        {
            Parent.outfits.CurrentApparelPolicy = _lastOutfit;
        }
    }

    internal void TryRestoreFoodRestriction()
    {
        if (!Settings.FoodRestrictions)
        {
            return;
        }

        if (_lastFoodRestriction == null)
        {
            return;
        }

        if (!Current.Game.foodRestrictionDatabase.AllFoodRestrictions.Any(f => _lastFoodRestriction.id.Equals(f.id)))
        {
            _lastFoodRestriction = null;

            return;
        }

        if (Parent?.foodRestriction.CurrentFoodPolicy == _lastFoodRestriction)
        {
            return;
        }

        if (Parent != null)
        {
            Parent.foodRestriction.CurrentFoodPolicy = _lastFoodRestriction;
        }
    }

    internal void TryRestoreDrugPolicy()
    {
        if (!Settings.DrugPolicies)
        {
            return;
        }

        if (_lastDrugPolicy == null)
        {
            return;
        }

        if (!Current.Game.drugPolicyDatabase.AllPolicies.Any(p => _lastDrugPolicy.id.Equals(p.id)))
        {
            _lastDrugPolicy = null;

            return;
        }

        if (Parent?.drugs.CurrentPolicy == _lastDrugPolicy)
        {
            return;
        }

        if (Parent != null)
        {
            Parent.drugs.CurrentPolicy = _lastDrugPolicy;
        }
    }

    internal void TryRestoreTimetable()
    {
        if (!Settings.Timetables)
        {
            return;
        }

        if (_lastSchedule.NullOrEmpty())
        {
            return;
        }

        if (Parent?.timetable.times?.SequenceEqual(_lastSchedule!) ?? false)
        {
            return;
        }

        if (Parent != null)
        {
            Parent.timetable.times = _lastSchedule.ListFullCopy();
        }
    }

    internal void TryRestoreArea()
    {
        if (!Settings.Area)
        {
            return;
        }

        if (_lastAllowedArea == null)
        {
            return;
        }

        if (!_lastAllowedArea.Map.areaManager.AllAreas.Any(a => _lastAllowedArea.ID.Equals(a.ID)))
        {
            _lastAllowedArea = null;

            return;
        }

        if (Parent?.playerSettings.AreaRestrictionInPawnCurrentMap == _lastAllowedArea)
        {
            return;
        }

        if (Parent != null)
        {
            Parent.playerSettings.AreaRestrictionInPawnCurrentMap = _lastAllowedArea;
        }
    }

    internal void TryRestoreDisplayOrder()
    {
        if (!Settings.DisplayOrder)
        {
            return;
        }

        if (Parent != null)
        {
            Parent.playerSettings.displayOrder = _lastDisplayOrder;
        }
    }

    internal void TryRestoreHostilityResponse()
    {
        if (!Settings.HostilityResponse)
        {
            return;
        }

        if (Parent != null)
        {
            Parent.playerSettings.hostilityResponse = _lastHostilityResponse;
        }
    }

    internal void TryRestoreMedicalCare()
    {
        if (!Settings.MedicalCare)
        {
            return;
        }

        if (Parent != null)
        {
            Parent.playerSettings.medCare = _lastMedCareCategory;
        }
    }

    internal void TryRestoreSelfTend()
    {
        if (!Settings.SelfTend)
        {
            return;
        }

        if (Parent != null)
        {
            Parent.playerSettings.selfTend = _lastSelfTendSetting;
        }
    }

    internal void TryRestoreWorkPriorities()
    {
        if (!Settings.Priorities)
        {
            return;
        }

        if (_workCache is not { Count: > 0 })
        {
            return;
        }

        MemoryHelper.WorkSettingsMap.SetValue(Parent?.workSettings, _workCache.Copy());
    }

    internal void TryStoreBed()
    {
        if (!Settings.Beds)
        {
            return;
        }

        if (!Parent.CurrentBed()?.Spawned ?? true)
        {
            return;
        }

        _lastBed = Parent.CurrentBed();
    }

    internal void TryStoreOutfit()
    {
        if (!Settings.Outfits)
        {
            return;
        }

        if (Parent?.outfits?.CurrentApparelPolicy == null)
        {
            return;
        }

        _lastOutfit = Parent.outfits.CurrentApparelPolicy;
    }

    internal void TryStoreFoodRestriction()
    {
        if (!Settings.FoodRestrictions)
        {
            return;
        }

        if (Parent?.foodRestriction?.CurrentFoodPolicy == null)
        {
            return;
        }

        _lastFoodRestriction = Parent.foodRestriction.CurrentFoodPolicy;
    }

    internal void TryStoreDrugPolicy()
    {
        if (!Settings.DrugPolicies)
        {
            return;
        }

        if (Parent?.drugs?.CurrentPolicy == null)
        {
            return;
        }

        _lastDrugPolicy = Parent.drugs.CurrentPolicy;
    }

    internal void TryStoreTimetable()
    {
        if (!Settings.Timetables)
        {
            return;
        }

        if (Parent?.timetable?.times?.NullOrEmpty() ?? true)
        {
            return;
        }

        _lastSchedule = Parent.timetable.times.ToList();
    }

    internal void TryStoreArea()
    {
        if (!Settings.Area)
        {
            return;
        }

        if (Parent?.playerSettings?.AreaRestrictionInPawnCurrentMap == null)
        {
            return;
        }

        _lastAllowedArea = Parent.playerSettings.AreaRestrictionInPawnCurrentMap;
    }

    internal void TryStoreDisplayOrder()
    {
        if (!Settings.DisplayOrder)
        {
            return;
        }

        if (Parent?.playerSettings == null)
        {
            return;
        }

        _lastDisplayOrder = Parent.playerSettings.displayOrder;
    }

    internal void TryStoreHostilityResponse()
    {
        if (!Settings.HostilityResponse)
        {
            return;
        }

        if (Parent?.playerSettings == null)
        {
            return;
        }

        _lastHostilityResponse = Parent.playerSettings.hostilityResponse;
    }

    internal void TryStoreMedicalCare()
    {
        if (!Settings.MedicalCare)
        {
            return;
        }

        if (Parent?.playerSettings == null)
        {
            return;
        }

        _lastMedCareCategory = Parent.playerSettings.medCare;
    }

    internal void TryStoreSelfTend()
    {
        if (!Settings.SelfTend)
        {
            return;
        }

        if (Parent?.playerSettings == null)
        {
            return;
        }

        _lastSelfTendSetting = Parent.playerSettings.selfTend;
    }

    internal void TryStoreWorkPriorities()
    {
        if (!Settings.Priorities)
        {
            return;
        }

        if (Parent?.workSettings == null)
        {
            return;
        }

        _workCache = (MemoryHelper.WorkSettingsMap.GetValue(Parent.workSettings) as DefMap<WorkTypeDef, int>)?.Copy();
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

    public static bool ShouldRemember(Pawn? pawn) => pawn?.IsColonistPlayerControlled ?? false;
}
