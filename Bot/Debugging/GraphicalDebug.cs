using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Bot.MapAnalysis;
using SC2APIProtocol;
//using Bot.Util;

namespace Bot
{
    public static class GraphicalDebug
    {
        private static Request DrawRequest;
        public static bool Debug = true;
        private static int TextLine = 0;
        public static GameConnection GameConnection = Program.gc;

        public static List<DebugSphere> Spheres = new List<DebugSphere>();
        public static List<DebugBox> Cubes = new List<DebugBox>();
        public static List<DebugText> Texts = new List<DebugText>();
        public static List<DebugLine> Lines = new List<DebugLine>();

        public static void OpenFrame()
        {
            DrawRequest = null;
            TextLine = 0;
        }
        public static void CloseFrame() 
        {
            
            InitializeDebugCommand();
            TestingStuff();
            DrawAll();

            if (DrawRequest != null)
                GameConnection.SendRequest(DrawRequest).Wait();
        }

        private static void InitializeDebugCommand()
        {
            if (DrawRequest == null)
            {
                DrawRequest = new Request();

                DrawRequest.Debug = new RequestDebug();
                DebugCommand debugCommand = new DebugCommand();
                debugCommand.Draw = new DebugDraw();
                DrawRequest.Debug.Debug.Add(debugCommand);
            }
        }

        public static void DrawAll() 
        {
            if (Cubes.Count()>0)
            {
                foreach (var cube in Cubes)
                {
                    DrawRequest.Debug.Debug[0].Draw.Boxes.Add(cube);
                    //Console.WriteLine($"Drawing cube with color: R={cube.Color.R}, G={cube.Color.G}, B={cube.Color.B}");
                }
            }
            if (Spheres.Count() > 0)
            {
                foreach (var sphere in Spheres)
                {
                    DrawRequest.Debug.Debug[0].Draw.Spheres.Add(sphere);
                }
            }
            if (Texts.Count() > 0)
            {
                foreach (var text in Texts)
                {
                    DrawRequest.Debug.Debug[0].Draw.Text.Add(text);
                }
            }
            if (Lines.Count() > 0)
            {
                foreach (var line in Lines)
                {
                    DrawRequest.Debug.Debug[0].Draw.Lines.Add(line);
                }
            }

            //Clear Lists
            Cubes.Clear();
            Spheres.Clear();
            Texts.Clear();
            Lines.Clear();
        }

        public static void DrawText(string text)
        {
            DrawScreen(text, 20, 0.0f, 0.1f + 0.02f * TextLine);
            TextLine++;
        }

        public static void DrawText(string text, Unit unit,uint size) 
        {
            InitializeDebugCommand();
            var color = new Color { R = 255, B = 255, G = 255 };
            Point pos = new Point() { X = unit.Position.X, Y = unit.Position.Y, Z = unit.Position.Z };
            DrawRequest.Debug.Debug[0].Draw.Text.Add(new DebugText() { Text = text, Color = color, Size = size, WorldPos = pos});
            Texts.Add(new DebugText() { Text = text, Color = color, Size = size, WorldPos = pos});
        }


        public static void DrawSphere(Unit unit,float radius) 
        {
            if (!Debug) { return; }
            Point pos = new Point { X = unit.Position.X, Y = unit.Position.Y, Z = unit.Position.Z };
            var color = new Color { R = 255, B = 0, G = 0 };
            //DrawRequest.Debug.Debug[0].Draw.Spheres.Add(new DebugSphere() { Color = color,P = pos, R = radius });
            Spheres.Add(new DebugSphere() { Color = color,P = pos, R = radius });
        }

        public static void DrawSphere(Vector3 position, float radius)
        {
            if (!Debug) { return; }
            Point pos = new Point { X = position.X, Y = position.Y, Z = position.Z };
            var color = new Color { R = 100, B = 100, G = 255 };
            //DrawRequest.Debug.Debug[0].Draw.Spheres.Add(new DebugSphere() { Color = color,P = pos, R = radius });
            Spheres.Add(new DebugSphere() { Color = color, P = pos, R = radius });
        }

        public static void DrawCube(Unit unit, float size)
        {
            if (!Debug) { return; }
            float offset = size / 2;
            //max
            Point max = new Point { X = unit.Position.X + offset, Y = unit.Position.Y + offset, Z = unit.Position.Z + size };
            //min
            Point min = new Point { X = unit.Position.X- offset, Y = unit.Position.Y - offset, Z = unit.Position.Z };
            //colour
            var color = new Color { R = 255, B = 0, G = 0 };
            //DrawRequest.Debug.Debug[0].Draw.Boxes.Add(new DebugBox() {Color = color, Max = max, Min = min });
            Cubes.Add(new DebugBox() { Color = color, Max = max, Min = min });
        }

        public static void DrawCube(Vector3 position, float size)
        {
            if (!Debug) { return; }
            float offset = size / 2;
            //max
            Point max = new Point { X = position.X + offset, Y = position.Y + offset, Z = position.Z + 0 };
            //min
            Point min = new Point { X = position.X - offset, Y = position.Y - offset, Z = position.Z };
            //colour
            var color = new Color { R = 255, B = 0, G = 0 };
            //DrawRequest.Debug.Debug[0].Draw.Boxes.Add(new DebugBox() {Color = color, Max = max, Min = min });
            Cubes.Add(new DebugBox() { Color = color, Max = max, Min = min });
        }

        public static void DrawCube(Vector3 position, float size, Color colour)
        {
            if (!Debug) { return; }
            float offset = size / 2;
            //max
            Point max = new Point { X = position.X + offset, Y = position.Y + offset, Z = position.Z + 0 };
            //min
            Point min = new Point { X = position.X - offset, Y = position.Y - offset, Z = position.Z };
            //colour
            //DrawRequest.Debug.Debug[0].Draw.Boxes.Add(new DebugBox() {Color = color, Max = max, Min = min });
            Cubes.Add(new DebugBox() { Color = colour, Max = max, Min = min });
        }

        public static void DrawLine(Unit unit, Unit unit2) 
        {
            if (!Debug) { return; }

            Point p0 = new Point { X = unit.Position.X, Y = unit.Position.Y, Z = unit.Position.Z + 0.05f };
            Point p1 = new Point { X = unit2.Position.X, Y = unit2.Position.Y, Z = unit2.Position.Z+0.05f };
            var color = new Color { R = 255, B = 0, G = 0 };

            Line line = new Line() { P0 = p0, P1 = p1 };
            Lines.Add(new DebugLine() { Color = color, Line = line });
        }

        public static void DrawLine(Vector3 start, Vector3 end)
        {
            if (!Debug) { return; }

            Point p0 = new Point { X = start.X, Y = start.Y, Z = start.Z + 0.05f };
            Point p1 = new Point { X = end.X, Y = end.Y, Z = end.Z + 0.05f };
            var color = new Color { R = 0, B = 0, G = 255 };

            Line line = new Line() { P0 = p0, P1 = p1 };
            Lines.Add(new DebugLine() { Color = color, Line = line });
        }

        public static void DrawLine(Vector3 start, Vector3 end, Color colour)
        {
            if (!Debug) { return; }

            Point p0 = new Point { X = start.X, Y = start.Y, Z = start.Z + 0.05f };
            Point p1 = new Point { X = end.X, Y = end.Y, Z = end.Z + 0.05f };
            var color = new Color { R = 0, B = 0, G = 255 };

            Line line = new Line() { P0 = p0, P1 = p1 };
            Lines.Add(new DebugLine() { Color = colour, Line = line });
        }


        public static int MapHeight(int x, int y)
        {
            //return 0;
            //return SC2Util.GetDataValue(RaxBot.Main.GameInfo.StartRaw.TerrainHeight, x, y);
            var terrainHeightData = Controller.gameInfo.StartRaw.TerrainHeight.Data;

            // Calculate the index in the byte array
            int index = y * Controller.gameInfo.StartRaw.TerrainHeight.Size.X + x;

            // Return the height value at the specified coordinates
            return terrainHeightData[index];
        }

        public static void TestingStuff() 
        {
            //summary - the code i used to test (draw boxes around minerals etc) goes here for now

            DrawText("Minerals: " + Controller.minerals);
            DrawText("Gas: " + Controller.vespene);

            var cc = Controller.GetUnits(Units.ResourceCenters, onlyCompleted: false).First();
            //get cc location
            var position = cc.Position;
            //DrawText("CC workers: " + cc.assignedWorkers);
            DrawText("cc position: " + position);
            DrawSphere(cc, 3);

            //draw mineral patches near cc
            var mineralFields = Controller.GetUnits(Units.MineralFields, onlyVisible: true, alliance: Alliance.Neutral);
            List<Unit> mfs = Controller.GetUnitsInRange(cc.Position, Units.MineralFields, 10, onlyVisible: true, alliance: Alliance.Neutral);
            DrawText("mineral fields in range: " + mfs.Count);
           // foreach (var mineralfield in mfs)
           // {
           //     DrawSphere(mineralfield, 1);
           // }

            //draw gas near cc
            var geysers = Controller.GetUnitsInRange(cc.Position, Units.GasGeysers, 10, onlyVisible: true, alliance: Alliance.Neutral);
            foreach (var gas in geysers)
            {
                DrawSphere(gas, 2);
            }

            //draw cube around scvs?
            List<Unit> workers = Controller.GetUnitsInRange(cc.Position, Units.Workers, 10, onlyVisible: true, alliance: Alliance.Self);
            DrawText("workers near cc: " + workers.Count);
            foreach (var worker in workers)
            {
                DrawCube(worker, 1);
                //DrawText("worker", worker, 20);
            }
        }

           

        public static void drawGrids() 
        {
            //draw a grid of where buildings can be placed
            //draw a grid of where buildings can be placed
            var cc = Controller.GetUnits(Units.ResourceCenters, onlyCompleted: false).First();
            for (int x = 200; x > 0; x -= 1)
            {
                for (int y = 200; y > 0; y -= 1)
                {
                    if (Controller.frame % 100 < 50)
                    {
                        if (Controller.GetTilePlacable(x, y))
                        {
                            Color colour = new Color { R = 255, G = 100, B = 100 };
                            DrawCube(new Vector3(x + 0.5f, y + 0.5f, cc.Position.Z + 0.05f), 1, colour);
                        }
                    }
                    else 
                    {
                        if (Controller.GetTileWalkable(x, y))
                        {
                            Color colour = new Color { R = 100, G = 100, B = 255};
                            //DrawText($"Color : R:{colour.R} G:{colour.G} B:{colour.B}");
                            DrawCube(new Vector3(x, y, cc.Position.Z + 0.05f), 1, colour);
                        }
                    }
                }
            }
        }


        public static void DrawScreen(string text, uint size, float x, float y)
        {
            if (Debug)
            {
                InitializeDebugCommand();
                DrawRequest.Debug.Debug[0].Draw.Text.Add(new DebugText() { Text = text, Size = size, VirtualPos = new Point() { X = x, Y = y } });
            }
        }


        public static void DrawCameraGrid()
        {
            var height = 13;
            Vector3 camera = Controller.obs.Observation.RawData.Player.Camera.ToVector3();
            DrawText($"Camera: {(int)camera.X},{(int)camera.Y} : Walkable");
            DrawSphere(new Vector3 { X = camera.X, Y = camera.Y, Z = height }, .25f);
            DrawLine(new Vector3 { X = camera.X, Y = camera.Y, Z = height }, new Vector3 { X = camera.X, Y = camera.Y, Z = 0 }, new Color { R = 255, G = 255, B = 255 });

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
                            color = new Color { R = 100, G = 255, B = 100 };
                        }
                        point.X = point.X + 0.5f;
                        point.Y = point.Y + 0.5f;
                        point.Z = MapData.Map[(int)camera.X][(int)camera.Y].TerrainHeight + 0.05f;
                        DrawCube(point, 1, color);
                    }
                }
            }
        }


        public static void DrawPaths()
        {
            //ALL THIS LOGIC SHOULD NOT BE IN GRAPHICAL DEBUG
            if (MapData.MapLastUpdate == 0)
            {
                MapData.GetMapGrid((int)Controller.frame);
            }

            List<List<Vector2>> listPath = new List<List<Vector2>>();

            var resourceCenters = Controller.GetUnits(Units.ResourceCenters);
            var rcPosition = resourceCenters[0].Position;
            Vector2 startPath = new Vector2 { X = rcPosition.X + 4, Y = rcPosition.Y };
            Vector2 endPath = new Vector2();

            foreach (var location in Controller.gameInfo.StartRaw.StartLocations)
            {
                endPath = new Vector2 { X = location.X, Y = location.Y };
                List<Vector2> path = MapData.GetPath(startPath, endPath);
                listPath.Add(path);
            }

            foreach (var path in listPath)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Vector3 start = new Vector3 { X = path[i].X, Y = path[i].Y, Z = MapData.Map[(int)path[i].X][(int)path[i].Y].TerrainHeight + 1 };
                    Vector3 end = new Vector3 { X = path[i + 1].X, Y = path[i + 1].Y, Z = MapData.Map[(int)path[i + 1].X][(int)path[i + 1].Y].TerrainHeight + 1 };
                    GraphicalDebug.DrawLine(start, end, new Color { R = 255, G = 100, B = 100 });
                }
            }

        }

    }
}