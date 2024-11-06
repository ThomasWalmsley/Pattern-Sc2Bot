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
        public MapData mapData;
        public bool camera = false;
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

            if (camera) { DrawGrid(Controller.obs.Observation.RawData.Player.Camera.ToVector3()); }
            

            if (Controller.frame > 1) 
            {
                DrawPaths();
            }

            //Draw Clusters
            //List<Unit> mineralFields = new List<Unit>();
            var mineralFields = Controller.GetUnits(Units.MineralFields,Alliance.Neutral);
            
            var geysers = Controller.GetUnits(Units.VESPENE_GEYSER, Alliance.Neutral);
            var purifiergeysers = Controller.GetUnits(Units.PURIFIER_VESPENE_GEYSER, Alliance.Neutral);
            var protossgeysers = Controller.GetUnits(Units.PROTOSS_VESPENE_GEYSER, Alliance.Neutral);
            var richgeysers = Controller.GetUnits(Units.RICH_VESPENE_GEYSER, Alliance.Neutral);
            var shakurasgeysers = Controller.GetUnits(Units.SHAKURAS_VESPENE_GEYSER, Alliance.Neutral);
            var spaceplatformgeysers = Controller.GetUnits(Units.SPACE_PLATFORM_GEYSER, Alliance.Neutral);

            geysers.AddRange(purifiergeysers);
            geysers.AddRange(protossgeysers);
            geysers.AddRange(richgeysers);
            geysers.AddRange(shakurasgeysers);
            geysers.AddRange(spaceplatformgeysers);

            List<Vector2> resources = new List<Vector2>();

            foreach (var mineral in mineralFields) 
            {
                resources.Add(new Vector2 { X = mineral.Position.X, Y = mineral.Position.Y });
            }
            foreach (var geyser in geysers) 
            {
                resources.Add(new Vector2 { X = geyser.Position.X, Y = geyser.Position.Y });
            }

            DrawClusters(DBSCAN.Cluster(resources, 10,5));



            ccS.onFrame();
            return Controller.CloseFrame();
        }


        public void DrawPaths() 
        {
            if (mapData.MapLastUpdate == 0)
            {
                mapData.GetMapGrid((int)Controller.frame);
            }

            List<List<Vector2>> listPath = new List<List<Vector2>>();

            var resourceCenters = Controller.GetUnits(Units.ResourceCenters);
            var rcPosition = resourceCenters[0].Position;
            Vector2 startPath = new Vector2 { X = rcPosition.X+4, Y = rcPosition.Y };
            Vector2 endPath = new Vector2();

            foreach (var location in Controller.gameInfo.StartRaw.StartLocations)
            {
                endPath = new Vector2 { X = location.X, Y = location.Y };
                List<Vector2> path = mapData.GetPath(startPath, endPath);
                listPath.Add(path);
            }

            foreach (var path in listPath) 
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Vector3 start = new Vector3 { X = path[i].X, Y = path[i].Y, Z = mapData.Map[(int)path[i].X][(int)path[i].Y].TerrainHeight + 1 };
                    Vector3 end = new Vector3 { X = path[i + 1].X, Y = path[i + 1].Y, Z = mapData.Map[(int)path[i + 1].X][(int)path[i + 1].Y].TerrainHeight + 1 };
                    Controller.gdebug.DrawLine(start, end, new Color { R = 255, G = 100, B = 100 });
                }
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
            var height = 13;

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
                        point.Z = mapData.Map[(int)camera.X][(int)camera.Y].TerrainHeight+0.05f;
                        Controller.gdebug.DrawCube(point, 1, color);
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
            List<Vector2> CentroidsAndGas = new List<Vector2>();
            //Console.WriteLine(clusters.Count());
            foreach (var cluster in clusters)
            {
                foreach (var point in cluster)
                {
                    Controller.gdebug.DrawCube(new Vector3 { X = point.X, Y = point.Y, Z = mapData.Map[(int)point.X][(int)point.Y].TerrainHeight + 1 }, 1, new Color { R = 255, G = 100, B = 100 });
                }
                Vector2 centroid = CalculateCentroid(cluster);

                // Round to nearest half position. bases are 5x5 and therefore always centered in the middle of a tile.
                float x = (int)centroid.X + 0.5f;
                float y = (int)centroid.Y + 0.5f;

                CentroidsAndGas.Add(centroid);

                //Controller.gdebug.DrawSphere(new Vector3 { X = x, Y = y, Z = mapData.Map[(int)centroid.X][(int)centroid.Y].TerrainHeight + 0.05f }, 1);
                //DetermineFinalLocation(new Vector2 {X=x, Y =y },cluster);
            }


            var geysers = Controller.GetUnits(Units.VESPENE_GEYSER, Alliance.Neutral);
            var purifiergeysers = Controller.GetUnits(Units.PURIFIER_VESPENE_GEYSER, Alliance.Neutral);
            var protossgeysers = Controller.GetUnits(Units.PROTOSS_VESPENE_GEYSER, Alliance.Neutral);
            var richgeysers = Controller.GetUnits(Units.RICH_VESPENE_GEYSER, Alliance.Neutral);
            var shakurasgeysers = Controller.GetUnits(Units.SHAKURAS_VESPENE_GEYSER, Alliance.Neutral);
            var spaceplatformgeysers = Controller.GetUnits(Units.SPACE_PLATFORM_GEYSER, Alliance.Neutral);

            geysers.AddRange(purifiergeysers);
            geysers.AddRange(protossgeysers);
            geysers.AddRange(richgeysers);
            geysers.AddRange(shakurasgeysers);
            geysers.AddRange(spaceplatformgeysers);

            foreach (var geyser in geysers)
            {
                CentroidsAndGas.Add(new Vector2 { X = geyser.Position.X, Y = geyser.Position.Y });
            }
            var newClusters = DBSCAN.Cluster(CentroidsAndGas, 10, 2);
            foreach (var cluster in newClusters) 
            {
                foreach (var point in cluster)
                {
                    Controller.gdebug.DrawCube(new Vector3 { X = point.X, Y = point.Y, Z = mapData.Map[(int)point.X][(int)point.Y].TerrainHeight + 1 }, 1, new Color { R = 255, G = 100, B = 100 });
                }
                Vector2 centroid = CalculateCentroid(cluster);

                // Round to nearest half position. bases are 5x5 and therefore always centered in the middle of a tile.
                float x = (int)centroid.X + 0.5f;
                float y = (int)centroid.Y + 0.5f;
                Controller.gdebug.DrawSphere(new Vector3 { X = x, Y = y, Z = mapData.Map[(int)centroid.X][(int)centroid.Y].TerrainHeight + 0.05f }, 1);
                DetermineFinalLocation(new Vector2 { X = x, Y = y }, cluster);
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

            // Move the estimated base position slightly away from the closest mineral.
            // This ensures that the base location will not end up on the far side of the minerals.
           // if (closest.X < baseLocation.X)
           // {
           //     baseLocation.X += 2;
           // }
           // else if (closest.X > baseLocation.X)
           // {
           //     baseLocation.X -= 2;
           // }
           // if (closest.Y < baseLocation.Y)
           // {
           //     baseLocation.Y += 2;
           // }
           // else if (closest.Y > baseLocation.Y)
           // {
           //     baseLocation.Y -= 2;
           // }

            var closestLocation = 1000000f;
            var approximateLocation = baseLocation;

            for (int i = -6; i < 6; i++) 
            {
                for (int j = -6; j < 6; j++)
                {
                    Vector2 newPos = new Vector2 { X = approximateLocation.X + i, Y = approximateLocation.Y + j };
                    Controller.gdebug.DrawCube(new Vector3 { X = newPos.X, Y = newPos.Y, Z = mapData.Map[(int)newPos.X][(int)newPos.Y].TerrainHeight + 0.05f }, 1);
                }
                // Point2D newPos;
                // newPos = new Point2D { X = approximateLocation.X + i, Y = approximateLocation.Y + i };
                // Controller.gdebug.DrawCube(new Vector3 {X = newPos.X, Y = newPos.Y,Z = mapData.Map[(int)newPos.X][(int)newPos.Y].TerrainHeight + 0.05f },1);
                // newPos = new Point2D { X = approximateLocation.X + i, Y = approximateLocation.Y - i };
                // Controller.gdebug.DrawCube(new Vector3 { X = newPos.X, Y = newPos.Y, Z = mapData.Map[(int)newPos.X][(int)newPos.Y].TerrainHeight + 0.05f }, 1);
                // newPos = new Point2D { X = approximateLocation.X - i, Y = approximateLocation.Y + i };
                // Controller.gdebug.DrawCube(new Vector3 { X = newPos.X, Y = newPos.Y, Z = mapData.Map[(int)newPos.X][(int)newPos.Y].TerrainHeight + 0.05f }, 1);
                // newPos = new Point2D { X = approximateLocation.X - i, Y = approximateLocation.Y - i };
                // Controller.gdebug.DrawCube(new Vector3 { X = newPos.X, Y = newPos.Y, Z = mapData.Map[(int)newPos.X][(int)newPos.Y].TerrainHeight + 0.05f }, 1);
                // newPos = new Point2D { X = approximateLocation.X - i, Y = approximateLocation.Y};
                // Controller.gdebug.DrawCube(new Vector3 { X = newPos.X, Y = newPos.Y, Z = mapData.Map[(int)newPos.X][(int)newPos.Y].TerrainHeight + 0.05f }, 1);
                // newPos = new Point2D { X = approximateLocation.X + i, Y = approximateLocation.Y};
                // Controller.gdebug.DrawCube(new Vector3 { X = newPos.X, Y = newPos.Y, Z = mapData.Map[(int)newPos.X][(int)newPos.Y].TerrainHeight + 0.05f }, 1);
                // newPos = new Point2D { X = approximateLocation.X, Y = approximateLocation.Y - i };
                // Controller.gdebug.DrawCube(new Vector3 { X = newPos.X, Y = newPos.Y, Z = mapData.Map[(int)newPos.X][(int)newPos.Y].TerrainHeight + 0.05f }, 1);
                // newPos = new Point2D { X = approximateLocation.X, Y = approximateLocation.Y + i };
                // Controller.gdebug.DrawCube(new Vector3 { X = newPos.X, Y = newPos.Y, Z = mapData.Map[(int)newPos.X][(int)newPos.Y].TerrainHeight + 0.05f }, 1);
                // 
                // 
                // //Offset Positions
                // newPos = new Point2D { X = approximateLocation.X+1, Y = approximateLocation.Y + i };
                // Controller.gdebug.DrawCube(new Vector3 { X = newPos.X, Y = newPos.Y, Z = mapData.Map[(int)newPos.X][(int)newPos.Y].TerrainHeight + 0.05f }, 1);
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