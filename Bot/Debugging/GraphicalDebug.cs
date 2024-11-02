using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SC2APIProtocol;
//using Bot.Util;

namespace Bot
{
    public class GraphicalDebug
    {
        private Request DrawRequest;
        public static bool Debug = true;
        private int TextLine = 0;
        public GameConnection GameConnection = Program.gc;

        public List<DebugSphere> Spheres = new List<DebugSphere>();
        public List<DebugBox> Cubes = new List<DebugBox>();
        public List<DebugText> Texts = new List<DebugText>();
        public List<DebugLine> Lines = new List<DebugLine>();

        public void OpenFrame()
        {
            DrawRequest = null;
            TextLine = 0;
        }
        public void CloseFrame() 
        {
            
            InitializeDebugCommand();
            TestingStuff();
            DrawAll();

            if (DrawRequest != null)
                GameConnection.SendRequest(DrawRequest).Wait();
        }

        private void InitializeDebugCommand()
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

        public void DrawAll() 
        {
            if (Cubes.Count()>0)
            {
                foreach (var cube in Cubes)
                {
                    DrawRequest.Debug.Debug[0].Draw.Boxes.Add(cube);
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

        public void DrawText(string text)
        {
            DrawScreen(text, 20, 0.0f, 0.1f + 0.02f * TextLine);
            TextLine++;
        }

        public void DrawText(string text, Unit unit,uint size) 
        {
            InitializeDebugCommand();
            var color = new Color { R = 255, B = 255, G = 255 };
            Point pos = new Point() { X = unit.Position.X, Y = unit.Position.Y, Z = unit.Position.Z };
            DrawRequest.Debug.Debug[0].Draw.Text.Add(new DebugText() { Text = text, Color = color, Size = size, WorldPos = pos});
            Texts.Add(new DebugText() { Text = text, Color = color, Size = size, WorldPos = pos});
        }


        public void DrawSphere(Unit unit,float radius) 
        {
            if (!Debug) { return; }
            Point pos = new Point { X = unit.Position.X, Y = unit.Position.Y, Z = unit.Position.Z };
            var color = new Color { R = 255, B = 0, G = 0 };
            //DrawRequest.Debug.Debug[0].Draw.Spheres.Add(new DebugSphere() { Color = color,P = pos, R = radius });
            Spheres.Add(new DebugSphere() { Color = color,P = pos, R = radius });
        }

        public void DrawCube(Unit unit, float size)
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

        public void DrawLine(Unit unit, Unit unit2) 
        {
            if (!Debug) { return; }

            Point p0 = new Point { X = unit.Position.X, Y = unit.Position.Y, Z = unit.Position.Z + 0.05f };
            Point p1 = new Point { X = unit2.Position.X, Y = unit2.Position.Y, Z = unit2.Position.Z+0.05f };
            var color = new Color { R = 255, B = 0, G = 0 };

            Line line = new Line() { P0 = p0, P1 = p1 };
            Lines.Add(new DebugLine() { Color = color, Line = line });
        }

        public void DrawLine(Vector3 start, Vector3 end)
        {
            if (!Debug) { return; }

            Point p0 = new Point { X = start.X, Y = start.Y, Z = start.Z + 0.05f };
            Point p1 = new Point { X = end.X, Y = end.Y, Z = end.Z + 0.05f };
            var color = new Color { R = 0, B = 0, G = 255 };

            Line line = new Line() { P0 = p0, P1 = p1 };
            Lines.Add(new DebugLine() { Color = color, Line = line });
        }


        public int MapHeight(int x, int y)
        {
            return 0;
            //return SC2Util.GetDataValue(RaxBot.Main.GameInfo.StartRaw.TerrainHeight, x, y);
        }

        public void TestingStuff() 
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


            // Draw Lines Between Expansions
            // Assuming you have methods to get the self and enemy start locations
            var resourceCenters = Controller.GetUnits(Units.ResourceCenters);
            if (resourceCenters.Count > 0) {
                var rcPosition = resourceCenters[0].Position;
                var enemyStartLocation = Controller.enemyLocations[0];
                Vector3 start = new Vector3(rcPosition.X,rcPosition.Y,rcPosition.Z + 0.05f);
                Vector3 end = new Vector3(enemyStartLocation.X, enemyStartLocation.Y, rcPosition.Z + 0.05f);//use same height as your townhall
                if (Controller.frame %22 == 0){
                    //Logger.Info($"Drawing line between expansions:{start.ToString()} , {end.ToString()}");
                }
           
                DrawLine(start, end);
            }
            
        }




        public void DrawScreen(string text, uint size, float x, float y)
        {
            if (Debug)
            {
                InitializeDebugCommand();
                DrawRequest.Debug.Debug[0].Draw.Text.Add(new DebugText() { Text = text, Size = size, VirtualPos = new Point() { X = x, Y = y } });
            }
        }
    }
}