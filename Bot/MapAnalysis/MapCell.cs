using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.MapAnalysis
{
    public class MapCell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int TerrainHeight { get; set; }
        public bool Walkable { get; set; }
        public bool Buildable { get; set; }
        public bool CurrentlyBuildable { get; set; }
        public bool PoweredBySelfPylon { get; set; }
        public bool HasCreep { get; set; }
        public bool InEnemyVision { get; set; }
        public bool InSelfVision { get; set; }
        public int Visibility { get; set; }
        public int LastFrameVisibility { get; set; }
        public int LastFrameAlliesTouched { get; set; }
        public bool PathBlocked { get; set; }
    }
}
