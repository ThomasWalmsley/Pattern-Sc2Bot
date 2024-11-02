using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SC2APIProtocol;

namespace Bot
{
    class UnitsTracker
    {
        public static readonly UnitsTracker Instance = new UnitsTracker();

        private bool _isInitialized = false;

        public static Dictionary<ulong, Unit> UnitsByTag { get; private set; } = new Dictionary<ulong, Unit>();

        public static readonly List<Unit> NewOwnedUnits = new List<Unit>();

        public static List<Unit> NeutralUnits { get; private set; } = new List<Unit>();
        public static List<Unit> OwnedUnits { get; private set; } = new List<Unit>();
        public static List<Unit> EnemyUnits { get; private set; } = new List<Unit>();

        private const int EnemyDeathDelaySeconds = 4 * 60;

        private UnitsTracker() { }

        public static List<Unit> GetUnits(Alliance alliance)
        {
            //get units of each alliance

            List<Unit> list_units_in_alliance= new List<Unit>();

            switch (alliance) 
            {
                case Alliance.Self:
                    list_units_in_alliance = OwnedUnits;
                    break;
                case Alliance.Enemy:
                    list_units_in_alliance = EnemyUnits;
                    break;
                case Alliance.Neutral:
                    list_units_in_alliance = NeutralUnits;
                    break;
            }
            ;
            return list_units_in_alliance;
        }

        public void Reset()
        {
            _isInitialized = false;

            UnitsByTag.Clear();

            NewOwnedUnits.Clear();

            NeutralUnits.Clear();
            OwnedUnits.Clear();
            EnemyUnits.Clear();
        }

        public void Update(ResponseObservation observation) 
        {
            var unitsAsReportedByTheApi = observation.Observation.RawData.Units.ToList();
            var currentFrame = observation.Observation.GameLoop;

            if (!_isInitialized)
            {
                Init(unitsAsReportedByTheApi, currentFrame);
                //LogUnknownNeutralUnits();

                return;
            }
            NewOwnedUnits.Clear();
            // Find new units and update existing ones
            unitsAsReportedByTheApi.ForEach(newRawUnit => 
            {
                if (UnitsByTag.ContainsKey(newRawUnit.Tag))
                {
                    UnitsByTag[newRawUnit.Tag].Update(newRawUnit, currentFrame);
                }
                else
                {
                    HandleNewUnit(newRawUnit, currentFrame);
                }
            });

            // Handle dead units
            var deadUnitTags = observation.Observation.RawData.Event?.DeadUnits?.ToHashSet() ?? new HashSet<ulong>();
            //HandleDeadUnits(deadUnitTags, unitsAsReportedByTheApi, currentFrame);
            //
            // RememberEnemyUnitsOutOfSight(unitsAsReportedByTheApi);
            // EraseGhosts();
            //
            UpdateUnitLists();
        }

        private void Init(IEnumerable<SC2APIProtocol.Unit> rawUnits, ulong frame) 
        {
            var units = rawUnits.Select(rawUnit => new Unit(rawUnit, frame)).ToList();

            UnitsByTag = units.ToDictionary(unit => unit.Tag);

            OwnedUnits = units.Where(unit => unit.Alliance == Alliance.Self).ToList();
            NeutralUnits = units.Where(unit => unit.Alliance == Alliance.Neutral).ToList();
            EnemyUnits = units.Where(unit => unit.Alliance == Alliance.Enemy).ToList();
            _isInitialized = true;
            Console.WriteLine("Initialising");
        }

        private static void HandleNewUnit(SC2APIProtocol.Unit newRawUnit, ulong currentFrame)
        {
            var newUnit = new Unit(newRawUnit, currentFrame);
            if (newUnit.Alliance == Alliance.Self)
            {
                Logger.Info("{0} was born", newUnit.Name);
                NewOwnedUnits.Add(newUnit);
            }
            else
            {
                var equivalentUnit = UnitsByTag
                   .Select(kv => kv.Value)
                   .Where(unit => unit.UnitType == newUnit.UnitType)
                   .Where(unit => unit.Alliance == newUnit.Alliance)
                   .FirstOrDefault(unit => unit.Position.DistanceTo(newUnit.Position) <= 0.0001f); // Refinery snapshot can have a slightly different Z value   
            }
            //some more stuff goes here (to do with equivalent units)
            UnitsByTag[newUnit.Tag] = newUnit;
        }

      // private void HandleDeadUnits(IReadOnlySet<ulong> deadUnitTags, List<SC2APIProtocol.Unit> currentlyVisibleUnits, uint currentFrame)
      // {
      //    
      //     if (deadUnitTags == null) 
      //     {
      //         return;
      //     }
      //
      // }

        private static void UpdateUnitLists()
        {
            OwnedUnits = UnitsByTag.Where(unit => unit.Value.Alliance == Alliance.Self).Select(unit => unit.Value).ToList();
            NeutralUnits = UnitsByTag.Where(unit => unit.Value.Alliance == Alliance.Neutral).Select(unit => unit.Value).ToList();
            EnemyUnits = UnitsByTag.Where(unit => unit.Value.Alliance == Alliance.Enemy).Select(unit => unit.Value).ToList();
        }

    }
}
