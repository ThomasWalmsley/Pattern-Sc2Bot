using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Roy_T.AStar.Graphs;
//using Roy_T.AStar.Primitives;

namespace Bot.MapAnalysis
{
    internal class MapData
    {
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }
        public Dictionary<int, Dictionary<int, MapCell>> Map { get; set; }
        public string MapName { get; set; }
        //public List<WallData> WallData { get; set; }
       //public List<PathData> PathData { get; set; }
    }
}
