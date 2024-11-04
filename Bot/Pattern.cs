using System.Collections.Generic;
using SC2APIProtocol;
using System.Linq;
using System.Numerics;
using Bot.MapAnalysis;
using System;
using System.Net.Security;

namespace Bot {
    public class Pattern : Bot {

        TownHallSupervisor ccS;
        public MapData mapData;
        public bool camera = true;
        //Tracker unitTracker;
        //UnitsTracker unitTracker;

        //the following will be called every frame
        //you can increase the amount of frames that get processed for each step at once in Wrapper/GameConnection.cs: stepSize  
        public IEnumerable<SC2APIProtocol.Action> OnFrame() {
            Controller.OpenFrame();
            if (Controller.frame == 0) {
                Logger.Info("Pattern");
                Logger.Info("--------------------------------------");
                Logger.Info("Map: {0}", Controller.gameInfo.MapName);
                Logger.Info("--------------------------------------");

                Unit cc = Controller.GetUnits(Units.ResourceCenters)[0];//get first townhall
                ccS = new TownHallSupervisor(cc);
                mapData = new MapData();
                mapData.generateMapData();
            }

            if (Controller.frame == Controller.SecsToFrames(1)) 
                Controller.Chat("gl hf");

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
            if ((Controller.GetUnits(Units.SUPPLY_DEPOT).Count >= 2)&&(Controller.GetTotalCount(Units.SUPPLY_DEPOT)>=2))
            {
                if (Controller.maxSupply - Controller.currentSupply <= 5)
                    if (Controller.CanConstruct(Units.SUPPLY_DEPOT))
                        if (Controller.GetPendingCount(Units.SUPPLY_DEPOT) == 0)
                            Controller.Construct(Units.SUPPLY_DEPOT);
            }

            if (Controller.CanConstruct(Units.SUPPLY_DEPOT) && (Controller.GetTotalCount(Units.SUPPLY_DEPOT) == 0)) 
            {
                Controller.ConstructOnLocation(Units.SUPPLY_DEPOT,new Vector3 { X = 55, Y = 40});
            }
            if (Controller.CanConstruct(Units.SUPPLY_DEPOT) && (Controller.GetTotalCount(Units.SUPPLY_DEPOT) == 1))
            {
                Controller.ConstructOnLocation(Units.SUPPLY_DEPOT, new Vector3 { X = 58, Y = 43 });
            }



            //distribute workers optimally every 10 frames
            if (Controller.frame % 50 == 0)
                Controller.DistributeWorkers();

            if (Controller.CanConstruct(Units.BARRACKS) && (Controller.GetTotalCount(Units.BARRACKS) <1)) 
            {
                Controller.ConstructOnLocation(Units.BARRACKS, new Vector3 { X = 55, Y = 42 });
            }

            //build up to 4 barracks at once
           // if (Controller.CanConstruct(Units.BARRACKS)) 
           // if (Controller.GetTotalCount(Units.BARRACKS) < 4)                
           //     Controller.Construct(Units.BARRACKS);
           //
            foreach (var barracks in Controller.GetUnits(Units.BARRACKS, onlyCompleted:true)) {
                if (Controller.CanConstruct(Units.MARINE))
                    barracks.Train(Units.MARINE);
            }

            var army = Controller.GetUnits(Units.ArmyUnits);
            var enemyarmy = Controller.GetUnits(Units.ArmyUnits,alliance:Alliance.Enemy,onlyVisible: true);
            if (army.Count > 15)
            {
                if (enemyarmy.Count != 0 && Controller.frame % 50 == 0)
                {
                    foreach (var marine in army)
                    {
                        var closestEnemy = enemyarmy.OrderBy(unit => Vector3.Distance(marine.Position,unit.Position)).First();
                        Vector3 target = closestEnemy.Position;
                        List<Unit> marine_army = new List<Unit>() { marine };
                        Controller.Attack(marine_army, target);
                    }
                }
                else if (Controller.enemyLocations.Count > 0 && Controller.frame % 50 == 0) 
                {
                    Controller.Attack(army, Controller.enemyLocations[0]);
                }
            }


            //DrawGrid(Controller.obs.Observation.RawData.Player.Camera.ToVector3());

            if (Controller.frame > 1) 
            {
                DrawPaths();
            }
            

            ccS.onFrame();
            return Controller.CloseFrame();
        }


        public void DrawPaths() 
        {
            if (mapData.MapLastUpdate == 0)
            {
                mapData.GetMapGrid((int)Controller.frame);
            }
            var resourceCenters = Controller.GetUnits(Units.ResourceCenters);
            var rcPosition = resourceCenters[0].Position;
            Vector2 startPath = new Vector2 { X = rcPosition.X+4, Y = rcPosition.Y };
            Vector2 endPath = new Vector2 { X = Controller.enemyLocations[0].X, Y = Controller.enemyLocations[0].Y };
            //List<Vector2> path = mapData.GetPath(rcPosition.ToVector2(), Controller.enemyLocations[0].ToVector2());
            List<Vector2> path = mapData.GetPath(startPath, endPath);
            //List<Vector2> path = mapData.GetPath(new Vector2 {X = 73, Y = 37 }, new Vector2 {X = 77, Y = 67 });
            for (int i = 0; i < path.Count - 1; i++) 
            {
                Vector3 start = new Vector3 { X = path[i].X, Y = path[i].Y, Z = mapData.Map[(int)path[i].X][(int)path[i].Y].TerrainHeight + 1 };
                Vector3 end = new Vector3 { X = path[i+1].X, Y = path[i+1].Y, Z = mapData.Map[(int)path[i+1].X][(int)path[i+1].Y].TerrainHeight + 1};
                Controller.gdebug.DrawLine(start, end, new Color { R = 255, G = 100, B = 100 });
            }
        }


        public void DrawGrid()
        {
            if(!camera) { return; }
            //loop through the Map Dictionary and draw a square for each cell
            foreach (var row in mapData.Map)
            {
                foreach (var cell in row.Value.Values)
                {
                    if (Controller.frame % 100 < 50)
                    {
                        if (cell.Walkable)
                        {
                            Controller.gdebug.DrawCube(new Vector3(cell.X + 0.5f, cell.Y + 0.5f, cell.TerrainHeight), 1, new Color { R = 100, G = 255, B = 100 });
                            //Console.WriteLine($"Cell: X:{cell.X} Y:{cell.Y} Z:{cell.TerrainHeight}");
                        }
                    }
                    else 
                    {
                        if (cell.Buildable)
                        {
                            Controller.gdebug.DrawCube(new Vector3(cell.X + 0.5f, cell.Y + 0.5f, cell.TerrainHeight), 1, new Color { R = 100, G = 100, B = 255 });
                            //Console.WriteLine($"Cell: X:{cell.X} Y:{cell.Y} Z:{cell.TerrainHeight}");
                        }
                    }

                }
            }

        }

        private void DrawGrid(Vector3 camera)
        {
            var height = 12;

            Controller.gdebug.DrawText($"Camera: {(int)camera.X},{(int)camera.Y} : Walkable");
            Controller.gdebug.DrawSphere(new Vector3 { X = camera.X, Y = camera.Y, Z = height }, .25f);
            Controller.gdebug.DrawLine(new Vector3 { X = camera.X, Y = camera.Y, Z = height }, new Vector3 { X = camera.X, Y = camera.Y, Z = 0 }, new Color { R = 255, G = 255, B = 255 });

            for (int x = -5; x <= 5; x++)
            {
                for (int y = -5; y <= 5; y++)
                {
                    var point = new Vector3 { X = (int)camera.X + x, Y = (int)camera.Y + y, Z = height + 1 };
                    var color = new Color { R = 255, G = 100, B = 100 };
                    if (point.X + 1 < mapData.MapWidth && point.Y + 1 < mapData.MapHeight && point.X > 0 && point.Y > 0)
                    {
                        if (mapData.Map[(int)point.X][(int)point.Y].Walkable)
                        {
                            color = new Color { R =100, G = 255, B = 100 };
                        }
                        point.X = point.X + 0.5f;
                        point.Y = point.Y + 0.5f;
                        point.Z = mapData.Map[(int)camera.X][(int)camera.Y].TerrainHeight;
                        Controller.gdebug.DrawCube(point, 1, color);
                        //Controller.gdebug.DrawLine(point, new Vector3 { X = point.X + 1, Y = point.Y, Z = height + 1 }, color);
                        //Controller.gdebug.DrawLine(point, new Vector3 { X = point.X, Y = point.Y + 1, Z = height + 1 }, color);
                        //Controller.gdebug.DrawLine(point, new Vector3 { X = point.X, Y = point.Y + 1, Z = 1 }, color);
                    }
                }
            }
        }
    }
}