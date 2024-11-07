using System.Collections.Generic;
using System.Numerics;
using Google.Protobuf.Collections;
using SC2APIProtocol;
using System;
using System.Linq;

namespace Bot {
    public class Unit {
        private SC2APIProtocol.Unit original;
        private UnitTypeData UnitTypeData;
        public SC2APIProtocol.Unit RawUnitData;

        public Vector3 Position { get; private set; }

        public string Name;
        public uint UnitType;
        public float Integrity;
        public ulong Tag;
        float FoodRequired;
        float Radius;
        public Alliance Alliance;
        public float _buildProgress;
        public UnitOrder Order;
        public RepeatedField<UnitOrder> Orders;
        public int Supply;
        public bool IsVisible;
        public ulong LastSeen;
        public HashSet<uint> Buffs;

        public int IdealWorkers;
        public int AssignedWorkers;
        public int MineralContents;
        public uint LastAttackFrame;

        //toms changes
        public Unit AssignedMineralPatch;
        public List<Unit> Workers_assigned = new List<Unit>();

        //SAJUUKS VARIABLES
        //there's more in the list above but idk
        public int InitialMineralCount = int.MaxValue;
        public int InitialVespeneCount = int.MaxValue;

        public Unit(SC2APIProtocol.Unit unit) {
            this.original = unit;
            this.UnitTypeData = Controller.gameData.Units[(int) unit.UnitType];

            this.Name = UnitTypeData.Name;
            this.Tag = unit.Tag;
            this.UnitType = unit.UnitType;
            this.Position = new Vector3(unit.Pos.X, unit.Pos.Y, unit.Pos.Z);
            this.Integrity = (unit.Health + unit.Shield) / (unit.HealthMax + unit.ShieldMax);
            this._buildProgress = unit.BuildProgress;
            this.IdealWorkers = unit.IdealHarvesters;
            this.AssignedWorkers = unit.AssignedHarvesters;
            
            this.Order = unit.Orders.Count > 0 ? unit.Orders[0] : new UnitOrder();
            this.Orders = unit.Orders;
            this.IsVisible = (unit.DisplayType == DisplayType.Visible);

            this.Supply = (int) UnitTypeData.FoodRequired;
             
        }

        public Unit(SC2APIProtocol.Unit unit, ulong frame)
        {
            Update(unit, frame);
        }

        public void Update(SC2APIProtocol.Unit unit, ulong frame) 
        {
            var unitTypeChanged = unit.UnitType != UnitType;
        
            RawUnitData = unit;

             if (unitTypeChanged)
             {
                UnitTypeData = Controller.gameData.Units[(int)unit.UnitType];
                //UnitTypeData = KnowledgeBase.GetUnitTypeData(unit.UnitType);
                //AliasUnitTypeData = UnitTypeData.HasUnitAlias ? KnowledgeBase.GetUnitTypeData(UnitTypeData.UnitAlias) : null;

                //var weapons = UnitTypeData.Weapons.Concat(AliasUnitTypeData?.Weapons ?? Enumerable.Empty<Weapon>()).ToList();
                //MaxRange = weapons.Count <= 0 ? 0 : weapons.Max(weapon => weapon.Range);
            }

            Name = UnitTypeData.Name;
            Tag = unit.Tag;
            UnitType = unit.UnitType;
            FoodRequired = UnitTypeData.FoodRequired;
            Radius = unit.Radius;
            Alliance = unit.Alliance;
            Position = unit.Pos.ToVector3();
            _buildProgress = unit.BuildProgress;
            Orders = unit.Orders;
            Order = unit.Orders.Count > 0 ? unit.Orders[0] : new UnitOrder();
            IsVisible = unit.DisplayType == DisplayType.Visible; // TODO GD This is not actually visible as in cloaked
            LastSeen = frame;
            Buffs = new HashSet<uint>(unit.BuffIds);
        
            // Snapshot minerals/gas don't have contents
            if (IsVisible && InitialMineralCount == int.MaxValue)
            {
                InitialMineralCount = RawUnitData.MineralContents;
                InitialVespeneCount = RawUnitData.VespeneContents;
            }
        }

        public void Update()
        {
            //Console.WriteLine("Updating");
            //Update this unit's data to coincide with the sc2apiUnit's new data

            //find unit in raw data(by tag?)
           //var unit = Controller.obs.Observation.RawData.Units.Where(i => i.Tag == tag).First();
           //
           //position = new Vector3(unit.Pos.X, unit.Pos.Y, unit.Pos.Z);
           //integrity = (unit.Health + unit.Shield) / (unit.HealthMax + unit.ShieldMax);
           //_buildProgress = unit.BuildProgress;
           //idealWorkers = unit.IdealHarvesters;
           //assignedWorkers = unit.AssignedHarvesters;
           //
           //order = unit.Orders.Count > 0 ? unit.Orders[0] : new UnitOrder();
           //IsVisible = (unit.DisplayType == DisplayType.Visible);
           //
           //supply = (int)unitTypeData.FoodRequired;
           //
           ////mineralContents = unit.MineralContents;
           ////CargoSpaceTaken = unit.CargoSpaceTaken;
           ////CargoSpaceMax = unit.CargoSpaceMax;
           //
           ////var test = UnitTypeData.HasMineralsFieldNumber;
        }
        
        public double GetDistance(Unit otherUnit) {
            return Vector3.Distance(Position, otherUnit.Position);
        }

        public double GetDistance(Vector3 location) {
            return Vector3.Distance(Position, location);
        }
        
        public void Train(uint unitType, bool queue=false) {            
            if (!queue && Orders.Count > 0)
                return;            

            var abilityID = Abilities.GetID(unitType);            
            var action = Controller.CreateRawUnitCommand(abilityID);
            action.ActionRaw.UnitCommand.UnitTags.Add(Tag);
            Controller.AddAction(action);

            var targetName = Controller.GetUnitName(unitType);
            Logger.Info("Started training: {0}", targetName);
        }
           
        public void Move(Vector3 target) {
            var action = Controller.CreateRawUnitCommand(Abilities.MOVE);
            action.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.X = target.X;
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = target.Y;
            action.ActionRaw.UnitCommand.UnitTags.Add(Tag);
            Controller.AddAction(action);
        }
        
        public void Smart(Unit unit) {
            var action = Controller.CreateRawUnitCommand(Abilities.SMART);
            action.ActionRaw.UnitCommand.TargetUnitTag = unit.Tag;
            action.ActionRaw.UnitCommand.UnitTags.Add(Tag);
            Controller.AddAction(action);
        }


        
    }
}