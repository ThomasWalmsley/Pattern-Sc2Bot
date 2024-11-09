using Bot.MapAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public class Region
    {
        public List<Vector3> Cells { get; }

        public Dictionary<int, Dictionary<int, MapCell>> RegionMap { get; set; }

        public Region(List<Vector3> cells) 
        {
            Cells = cells;
        }

        private void getMapCells() 
        {
            //if x not in dictionary, add it

           // foreach (var cell in Cells) 
           // {
           //     if (!RegionMap.ContainsKey((int)cell.X)) 
           //         RegionMap.Add(new Dictionary<int, MapCell>());
           // }
            
        }
       

    }
}
