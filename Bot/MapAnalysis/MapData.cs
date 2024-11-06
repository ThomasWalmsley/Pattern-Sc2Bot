using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Roy_T.AStar.Graphs;
//using Roy_T.AStar.Primitives;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Primitives;
using Roy_T.AStar.Paths;
using System.Numerics;

namespace Bot.MapAnalysis
{
    public static class MapData
    {
        public static int MapWidth { get; set; }
        public static int MapHeight { get; set; }
        public static Dictionary<int, Dictionary<int, MapCell>> Map { get; set; }
        public static string MapName { get; set; }
        public static int MapLastUpdate { get; set; }

        //public List<WallData> WallData { get; set; }
        //public List<PathData> PathData { get; set; }

        static Grid WalkGrid;

        public static void generateMapData()
        {
            //Code taken from Sharky Framework
            var placementGrid = Controller.gameInfo.StartRaw.PlacementGrid;
            var heightGrid = Controller.gameInfo.StartRaw.TerrainHeight;
            var pathingGrid = Controller.gameInfo.StartRaw.PathingGrid;
            MapWidth = pathingGrid.Size.X;
            MapHeight = pathingGrid.Size.Y;
            Map = new Dictionary<int, Dictionary<int, MapCell>>();
            for (var x = 0; x < pathingGrid.Size.X; x++)
            {
                var row = new Dictionary<int, MapCell>();
                for (var y = 0; y < pathingGrid.Size.Y; y++)
                {
                    var walkable = GetDataValueBit(pathingGrid, x, y);
                    var height = GetDataValueByte(heightGrid, x, y);
                    var placeable = GetDataValueBit(placementGrid, x, y);
                    row[y] = new MapCell { X = x, Y = y, Walkable = walkable, TerrainHeight = height, Buildable = placeable, HasCreep = false, CurrentlyBuildable = placeable, InEnemyVision = false, InSelfVision = false, Visibility = 0, LastFrameVisibility = 0, PoweredBySelfPylon = false, LastFrameAlliesTouched = 0, PathBlocked = false };
                }
                Map[x] = row;
            }
            MapName = Controller.gameInfo.MapName;
        }


        static bool GetDataValueBit(ImageData data, int x, int y)
        {
            int pixelID = x + y * data.Size.X;
            int byteLocation = pixelID / 8;
            int bitLocation = pixelID % 8;
            return ((data.Data[byteLocation] & 1 << (7 - bitLocation)) == 0) ? false : true;
        }
        static int GetDataValueByte(ImageData data, int x, int y)
        {
            int pixelID = x + y * data.Size.X;
            int rawValue = data.Data[pixelID];
            int heightValue = (rawValue - 128) / 8 + 1;
            //return data.Data[pixelID];
            return heightValue;
        }

        public static Grid GetMapGrid(int frame)
        {
            if (MapLastUpdate < frame)
            {
                var gridSize = new GridSize(columns: MapWidth, rows: MapHeight);
                var cellSize = new Size(Distance.FromMeters(1), Distance.FromMeters(1));
                var traversalVelocity = Velocity.FromMetersPerSecond(1);
                WalkGrid = Grid.CreateGridWithLateralAndDiagonalConnections(gridSize, cellSize, traversalVelocity);
                for (var x = 0; x < MapWidth; x++)
                {
                    for (var y = 0; y < MapHeight; y++)
                    {
                        if (!Map[x][y].Walkable)
                        {
                            WalkGrid.DisconnectNode(new GridPosition(x, y));
                            continue;
                        }

                        //var vector = new Vector2(x, y);
                        //if (ActiveUnitData.NeutralUnits.Values.Any(u => u.Attributes.Contains(SC2APIProtocol.Attribute.Structure) && !u.UnitTypeData.Name.Contains("Unbuildable") && Vector2.DistanceSquared(u.Position, vector) <= u.Unit.Radius * u.Unit.Radius))
                        //{
                        //    WalkGrid.DisconnectNode(new GridPosition(x, y));
                        //}
                    }
                }
                MapLastUpdate = frame;
            }
            return WalkGrid;
        }

       public static List<Vector2> GetPath(Vector2 start, Vector2 end) 
       {
           List<Vector2> listOfEdges = new List<Vector2>();
           var pathFinder = new PathFinder();
           var path = pathFinder.FindPath(new GridPosition((int)start.X, (int)start.Y), new GridPosition((int)end.X,(int)end.Y), WalkGrid);
           //Console.WriteLine($"type: {path.Type}, distance: {path.Distance}, duration {path.Duration}");
            //return path.Edges.Select(e => new Vector2(e.X, e.Y)).ToList();
            return path.Edges.Select(e => new Vector2(e.End.Position.X, e.End.Position.Y)).ToList();
        }


        public static List<Vector2> GenerateBaseLocations() 
        {
            return new List<Vector2>();
        }


    }
}
