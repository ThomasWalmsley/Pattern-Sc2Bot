using System.Collections.Generic;
using SC2APIProtocol;
using System.Linq;
using System.Numerics;

namespace Bot {
    internal class Pattern : Bot {

        TownHallSupervisor ccS;
        //Tracker unitTracker;
        //UnitsTracker unitTracker;

        //the following will be called every frame
        //you can increase the amount of frames that get processed for each step at once in Wrapper/GameConnection.cs: stepSize  
        public IEnumerable<Action> OnFrame() {
            
            
            Controller.OpenFrame();
            if (Controller.frame == 0) {
                Logger.Info("Pattern");
                Logger.Info("--------------------------------------");
                Logger.Info("Map: {0}", Controller.gameInfo.MapName);
                Logger.Info("--------------------------------------");

                Unit cc = Controller.GetUnits(Units.ResourceCenters)[0];//get first townhall
                ccS = new TownHallSupervisor(cc);
            }

            if (Controller.frame == Controller.SecsToFrames(1)) 
                Controller.Chat("gl hf");

            //unitTracker.Update(Controller.obs);
            UnitsTracker.Instance.Update(Controller.obs);

            var structures = Controller.GetUnits(Units.Structures);
            if (structures.Count == 1) {
                //last building                
                if (structures[0].Integrity < 0.4) //being attacked or burning down                 
                    if (!Controller.chatLog.Contains("gg"))
                        Controller.Chat("gg");                
            }

            var resourceCenters = Controller.GetUnits(Units.ResourceCenters);
            foreach (var rc in resourceCenters) {
                if (Controller.GetUnits(Units.Workers,alliance:Alliance.Self).Count()>=16) { continue; }
                if (Controller.CanConstruct(Units.SCV))
                    rc.Train(Units.SCV);
            }
            
            
            //keep on buildings depots if supply is tight
            if (Controller.maxSupply - Controller.currentSupply <= 5)
                if (Controller.CanConstruct(Units.SUPPLY_DEPOT))
                    if (Controller.GetPendingCount(Units.SUPPLY_DEPOT) == 0)                    
                        Controller.Construct(Units.SUPPLY_DEPOT);

            
            //distribute workers optimally every 10 frames
            if (Controller.frame % 10 == 0)
                Controller.DistributeWorkers();
                //Logger.Info("Barracks: " + Controller.GetTotalCount(Units.BARRACKS).ToString());



            //build up to 4 barracks at once
            if (Controller.CanConstruct(Units.BARRACKS)) 
                if (Controller.GetTotalCount(Units.BARRACKS) < 4)                
                    Controller.Construct(Units.BARRACKS);
                   //foreach (var item in UnitsTracker.OwnedUnits) 
                   //{
                   //    Logger.Info(item.Name);
                   //}

            //train marine
            foreach (var barracks in Controller.GetUnits(Units.BARRACKS, onlyCompleted:true)) {
                if (Controller.CanConstruct(Units.MARINE))
                    barracks.Train(Units.MARINE);
            }



            //attack when we have enough units
            //List<SC2APIProtocol.Unit> enemyunits = Controller.obs.Observation.RawData.Units.Where(unit => unit.Alliance == Alliance.Enemy).ToList();
            var army = Controller.GetUnits(Units.ArmyUnits);
            var enemyarmy = Controller.GetUnits(Units.ArmyUnits,alliance:Alliance.Enemy,onlyVisible: true);
            // if (army.Count > 20) {
            //     if (Controller.enemyLocations.Count > 0)
            //
            //         Controller.Attack(army, Controller.enemyLocations[0]);
            // }
            if (army.Count > 15)
            {
                if (enemyarmy.Count != 0)
                {
                    foreach (var marine in army)
                    {
                        //var closestEnemy = enemyarmy.OrderBy(unit => Vector3.Distance(marine.position, new Vector3 { X = unit.Pos.X, Y = unit.Pos.Y, Z = unit.Pos.Z })).First();
                        var closestEnemy = enemyarmy.OrderBy(unit => Vector3.Distance(marine.Position,unit.Position)).First();
                        //Vector3 target = new Vector3 { X = closestEnemy.Pos.X, Y = closestEnemy.Pos.Y, Z = closestEnemy.Pos.Z };
                        Vector3 target = closestEnemy.Position;
                        List<Unit> marine_army = new List<Unit>() { marine };
                        Controller.Attack(marine_army, target);
                    }
                    //enemyarmy = enemyunits.OrderBy(unit => Vector3.Distance(townhall.position, unit.position)).


                    //Point enemyUnit = enemyunits.First().Pos;
                    //Vector3 target = new Vector3 { X = enemyUnit.X, Y = enemyUnit.Y, Z = enemyUnit.Z };
                    //Controller.Attack(army, target);
                }
                else if (Controller.enemyLocations.Count > 0) 
                {
                    Controller.Attack(army, Controller.enemyLocations[0]);
                }
            }



            ccS.onFrame();
            
            Controller.gdebug.CloseFrame();
            return Controller.CloseFrame();
        }
    }
}