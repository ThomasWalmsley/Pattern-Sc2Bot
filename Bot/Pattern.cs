using System.Collections.Generic;
using SC2APIProtocol;
using System.Linq;
using System.Numerics;
using Bot.MapAnalysis;
using System;
using System.Net.Security;
using System.Dynamic;
using System.Resources;

namespace Bot {
    public class Pattern : Bot {

        TownHallSupervisor ccS;
        public bool camera = false;
        public RegionAnalyser regionAnalyser = new RegionAnalyser();


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
                MapData.generateMapData();
                MapData.GetMapGrid(0);
            }

            UnitsTracker.Instance.Update(Controller.obs);

            if (Controller.frame == 0)
            {
                MapData.GenerateBaseLocations();
                MapData.OrderBaseLocations();
                MapData.GeneratePathsToBases();
                MapData.Regions = regionAnalyser.GenerateRegions();
            }

            var structures = Controller.GetUnits(Units.Structures);

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


            //MapData.GenerateBaseLocations();

            if (camera) { GraphicalDebug.DrawCameraGrid(5); }


            foreach (var baseLocation in MapData.BaseLocations) 
            {
                GraphicalDebug.DrawSphere(new Vector3 { X = baseLocation.X+0.5f, Y = baseLocation.Y + 0.5f, Z = MapData.Map[(int)baseLocation.X][(int)baseLocation.Y].TerrainHeight + 0.05f }, 2.5f,new Color {R=100,G=255,B=100 });
                GraphicalDebug.DrawText($"{baseLocation.X},{baseLocation.Y}", new Vector3(baseLocation.X + 0.5f, baseLocation.Y + 0.5f, MapData.Map[(int)baseLocation.X][(int)baseLocation.Y].TerrainHeight+1),25);
            }

            regionAnalyser.DrawRegions(MapData.Regions);

            if (camera) { GraphicalDebug.DrawCameraGrid(5); }

            ccS.onFrame();
            return Controller.CloseFrame();
        }





        public void DrawGrid()
        {
            if(!camera) { return; }
            //loop through the Map Dictionary and draw a square for each cell
            foreach (var row in MapData.Map)
            {
                foreach (var cell in row.Value.Values)
                {
                    if (Controller.frame % 100 < 50)
                    {
                        if (cell.Walkable)
                        {
                            GraphicalDebug.DrawCube(new Vector3(cell.X + 0.5f, cell.Y + 0.5f, cell.TerrainHeight), 1, new Color { R = 100, G = 255, B = 100 });
                            //Console.WriteLine($"Cell: X:{cell.X} Y:{cell.Y} Z:{cell.TerrainHeight}");
                        }
                    }
                    else 
                    {
                        if (cell.Buildable)
                        {
                            GraphicalDebug.DrawCube(new Vector3(cell.X + 0.5f, cell.Y + 0.5f, cell.TerrainHeight), 1, new Color { R = 100, G = 100, B = 255 });
                            //Console.WriteLine($"Cell: X:{cell.X} Y:{cell.Y} Z:{cell.TerrainHeight}");
                        }
                    }

                }
            }

        }

        private void DrawGrid(Vector3 camera)
        {
            var height = 13;

            GraphicalDebug.DrawText($"Camera: {(int)camera.X},{(int)camera.Y} : Walkable");
            GraphicalDebug.DrawSphere(new Vector3 { X = camera.X, Y = camera.Y, Z = height }, .25f);
            GraphicalDebug.DrawLine(new Vector3 { X = camera.X, Y = camera.Y, Z = height }, new Vector3 { X = camera.X, Y = camera.Y, Z = 0 }, new Color { R = 255, G = 255, B = 255 });

            for (int x = -5; x <= 5; x++)
            {
                for (int y = -5; y <= 5; y++)
                {
                    var point = new Vector3 { X = (int)camera.X + x, Y = (int)camera.Y + y, Z = height + 1 };
                    var color = new Color { R = 255, G = 100, B = 100 };
                    if (point.X + 1 < MapData.MapWidth && point.Y + 1 < MapData.MapHeight && point.X > 0 && point.Y > 0)
                    {
                        if (MapData.Map[(int)point.X][(int)point.Y].Walkable)
                        {
                            color = new Color { R =100, G = 255, B = 100 };
                        }
                        point.X = point.X + 0.5f;
                        point.Y = point.Y + 0.5f;
                        point.Z = MapData.Map[(int)camera.X][(int)camera.Y].TerrainHeight+0.05f;
                        GraphicalDebug.DrawCube(point, 1, color);
                        //Controller.gdebug.DrawLine(point, new Vector3 { X = point.X + 1, Y = point.Y, Z = height + 1 }, color);
                        //Controller.gdebug.DrawLine(point, new Vector3 { X = point.X, Y = point.Y + 1, Z = height + 1 }, color);
                        //Controller.gdebug.DrawLine(point, new Vector3 { X = point.X, Y = point.Y + 1, Z = 1 }, color);
                    }
                }
            }
        }


        public void DrawClusters(List<List<Vector2>> clusters)
        {
            //Experiment to see if clustering the minerals and then the mineral centroid and the geysers works better
            //List<Vector2> CentroidsAndGas = new List<Vector2>();
            //Console.WriteLine(clusters.Count());
            foreach (var cluster in clusters)
            {
                foreach (var point in cluster)
                {
                    GraphicalDebug.DrawCube(new Vector3 { X = point.X, Y = point.Y, Z = MapData.Map[(int)point.X][(int)point.Y].TerrainHeight + 1 }, 1, new Color { R = 255, G = 100, B = 100 });
                }
                Vector2 centroid = CalculateCentroid(cluster);

                // Round to nearest half position. bases are 5x5 and therefore always centered in the middle of a tile.
                float x = (int)centroid.X + 0.5f;
                float y = (int)centroid.Y + 0.5f;

                //CentroidsAndGas.Add(centroid);

                //Controller.gdebug.DrawSphere(new Vector3 { X = x, Y = y, Z = mapData.Map[(int)centroid.X][(int)centroid.Y].TerrainHeight + 0.05f }, 1);
                DetermineFinalLocation(new Vector2 {X=x, Y =y },cluster);
            }
        }

        public void DetermineFinalLocation(Vector2 approxLocation, List<Vector2> cluster)
        {
            Vector2 baseLocation = approxLocation;


            Vector2 closest = new Vector2();
            var closestDistance = 10000f;
            foreach (var mineralField in cluster)
            {
                var distance = Math.Abs(mineralField.X - approxLocation.X) + Math.Abs(mineralField.Y - approxLocation.Y);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = mineralField;
                }
            }
            GraphicalDebug.DrawCube(new Vector3 { X = closest.X, Y = closest.Y, Z = MapData.Map[(int)closest.X][(int)closest.Y].TerrainHeight + 0.05f }, 1,new Color {R = 100,G=100,B=255 });
            GraphicalDebug.DrawSphere(new Vector3 { X = baseLocation.X, Y = baseLocation.Y, Z = MapData.Map[(int)baseLocation.X][(int)baseLocation.Y].TerrainHeight + 0.05f }, 1);

            var closestLocation = 1000000f;
            var approximateLocation = baseLocation;

            for (int i = -4; i < 4; i++) 
            {
                for (int j = -4; j < 4; j++)
                {
                    Vector2 newPos = new Vector2 { X = approximateLocation.X + i, Y = approximateLocation.Y + j };
                    GraphicalDebug.DrawCube(new Vector3 { X = newPos.X, Y = newPos.Y, Z = MapData.Map[(int)newPos.X][(int)newPos.Y].TerrainHeight + 0.05f }, 1);
                }
            }
        }


        private Vector2 CalculateCentroid(List<Vector2> units)
        {
            float xSum = 0, ySum = 0;
            foreach (var unit in units)
            {
                xSum += unit.X;
                ySum += unit.Y;
            }

            return new Vector2
            {
                X = xSum / units.Count,
                Y = ySum / units.Count,
            };
        }

        public List<Vector2> GetExpansionLocations() 
        {
            List<Vector2> ExpansionLocations = new List<Vector2>();

            //use dbscan????


            return ExpansionLocations;
        }

    }
}