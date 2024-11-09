using Bot.MapAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace Bot
{
    public class RegionAnalyser
    {
        private static readonly float DiagonalDistance = (float)Math.Sqrt(2);
        private const int RegionMinPoints = 6;
        private const float RegionZMultiplier = 8;

        public RegionAnalyser() { }

        public List<Region> GenerateRegions() 
        {
            //Use DBSCAN to cluster cells into regions
            //First, cluster cells by height
            //Second, cluster cells by distance

            List<Region> regions = new List<Region>();

            //First, get all MapCells and add them to a list?

            List<MapCell> mapCells = new List<MapCell>();
            foreach (var row in MapData.Map.Values) 
            {
                mapCells.AddRange(row.Values.ToList());
            }

            mapCells = mapCells.Where(cell => cell.Walkable).ToList();

            List<Vector3> cells = new List<Vector3>();
            foreach (var mapCell in mapCells) 
            {
                cells.Add(new Vector3 {X = mapCell.X,Y=mapCell.Y,Z=mapCell.TerrainHeight * 50 });
            }

            float epsilon = 1.5f;//diagonal distance between squares

            //cluster cells by height
            var clusters = DBSCAN.Cluster(cells,epsilon,6);

            foreach (var cluster in clusters) 
            {
                //fix heights back to normal after doing the dbscan
                var updatedCluster = cluster.Select(cell => new Vector3((int)cell.X, (int)cell.Y, MapData.Map[(int)cell.X][(int)cell.Y].TerrainHeight)).ToList();
                regions.Add(new Region(updatedCluster));
            }

            return regions;
        }

        public List<Region> generatePotentialRegions(List<MapCell> cells) 
        {
            //Use DBSCAN to cluster cells into regions
            //Using the DBSCAN from Sajuuk's bot instead of mine (it's way cleaner
            List<Region> potentialRegions = new List<Region>();

            cells.ForEach(mapCell =>
            {
                // Highly penalize height differences
                mapCell.TerrainHeight = mapCell.TerrainHeight * 10;
            });

            var noise = new HashSet<MapCell>();
            var clusteringResult = Clustering.DBSCAN(cells, epsilon: DiagonalDistance + 0.04f, minPoints: RegionMinPoints);

            return potentialRegions;
        }



































        public void DrawRegions(List<Region> regions) 
        {
            foreach (var region in regions)
            {
                //find center
                Vector3 center = new Vector3();
                center.X = (int)region.Cells.Average(c => c.X);
                center.Y = (int)region.Cells.Average(c => c.Y);
                center.Z = 12;
                GraphicalDebug.DrawSphere(center, 2, new Color {R=100,G=255,B=255 });
                GraphicalDebug.DrawText($"Region : {center.X},{center.Y}",center,15);
                Vector3 camera = Controller.obs.Observation.RawData.Player.Camera.ToVector3();
                if (region.Cells.Contains(new Vector3 {X = (int)camera.X,Y = (int)camera.Y,Z= MapData.Map[(int)camera.X][(int)camera.Y].TerrainHeight }))
                    DrawRegionUnderCamera(region);
            }
        }

        public void DrawRegionUnderCamera(Region region) 
        {
            Vector3 camera = Controller.obs.Observation.RawData.Player.Camera.ToVector3();

            //TODO: Grid should be the size of the regino

            

            foreach (var cell in region.Cells) 
            {
                GraphicalDebug.DrawCube(new Vector3 { X = cell.X, Y = cell.Y,Z = cell.Z },1);
                
            }
        }




    }
}
