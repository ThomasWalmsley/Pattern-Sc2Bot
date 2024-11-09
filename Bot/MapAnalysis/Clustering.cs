using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bot.MapAnalysis
{
    public static class Clustering
    {
        private enum DBSCANLabels
        {
            Noise,
            BorderPoint,
            CorePoint
        }

        public static (List<List<Vector2>> clusters, List<Vector2> noise) DBSCAN(List<Vector2> positions, float epsilon, int minPoints)
        {
            var clusters = new List<List<Vector2>>();
            var labels = new Dictionary<Vector2, DBSCANLabels>();
        
            var currentCluster = new List<Vector2>();
            foreach (var position in positions)
            {
                if (labels.ContainsKey(position))
                {
                    continue;
                }
        
                var neighbors = positions.Where(otherItem => position != otherItem && position.DistanceTo(otherItem) <= epsilon).ToList();
                if (neighbors.Count < minPoints)
                {
                    labels[position] = DBSCANLabels.Noise;
                    continue;
                }
        
                labels[position] = DBSCANLabels.CorePoint;
                currentCluster.Add(position);
        
                for (var i = 0; i < neighbors.Count; i++)
                {
                    var neighbor = neighbors[i];
        
                    if (labels.TryGetValue(neighbor, out var label))
                    {
                        if (label == DBSCANLabels.Noise)
                        {
                            labels[neighbor] = DBSCANLabels.BorderPoint;
                            currentCluster.Add(neighbor);
                        }
        
                        continue;
                    }
        
                    currentCluster.Add(neighbor);
        
                    var neighborsOfNeighbor = positions.Where(otherPosition => neighbor != otherPosition && neighbor.DistanceTo(otherPosition) <= epsilon).ToList();
                    if (neighborsOfNeighbor.Count >= minPoints)
                    {
                        labels[neighbor] = DBSCANLabels.CorePoint;
                        neighbors.AddRange(neighborsOfNeighbor);
                    }
                    else
                    {
                        labels[neighbor] = DBSCANLabels.BorderPoint;
                    }
                }
        
                clusters.Add(currentCluster);
                currentCluster = new List<Vector2>();
            }
        
            //clusters.ForEach(DrawBoundingBox);
        
            var noise = new List<Vector2>();
            foreach (var kvp in labels)
            {
                var position=kvp.Key;
                var label=kvp.Value;

                if (label == DBSCANLabels.Noise)
                {
                    noise.Add(position);
                }
            }
        
            return (clusters, noise);
        }


        public static (List<List<MapCell>> clusters, List<Vector2> noise) DBSCAN(List<MapCell> positions, float epsilon, int minPoints)
        {
            var clusters = new List<List<MapCell>>();
            var labels = new Dictionary<MapCell, DBSCANLabels>();

            var currentCluster = new List<Vector2>();
            foreach (var position in positions)
            {
                if (labels.ContainsKey(position))
                {
                    continue;
                }

                var neighbors = positions.Where(otherItem => position != otherItem && position.DistanceTo(otherItem) <= epsilon).ToList();
                if (neighbors.Count < minPoints)
                {
                    labels[position] = DBSCANLabels.Noise;
                    continue;
                }

                labels[position] = DBSCANLabels.CorePoint;
                currentCluster.Add(position);

                for (var i = 0; i < neighbors.Count; i++)
                {
                    var neighbor = neighbors[i];

                    if (labels.TryGetValue(neighbor, out var label))
                    {
                        if (label == DBSCANLabels.Noise)
                        {
                            labels[neighbor] = DBSCANLabels.BorderPoint;
                            currentCluster.Add(neighbor);
                        }

                        continue;
                    }

                    currentCluster.Add(neighbor);

                    var neighborsOfNeighbor = positions.Where(otherPosition => neighbor != otherPosition && neighbor.DistanceTo(otherPosition) <= epsilon).ToList();
                    if (neighborsOfNeighbor.Count >= minPoints)
                    {
                        labels[neighbor] = DBSCANLabels.CorePoint;
                        neighbors.AddRange(neighborsOfNeighbor);
                    }
                    else
                    {
                        labels[neighbor] = DBSCANLabels.BorderPoint;
                    }
                }

                clusters.Add(currentCluster);
                currentCluster = new List<Vector2>();
            }

            //clusters.ForEach(DrawBoundingBox);

            var noise = new List<Vector2>();
            foreach (var kvp in labels)
            {
                var position = kvp.Key;
                var label = kvp.Value;

                if (label == DBSCANLabels.Noise)
                {
                    noise.Add(position);
                }
            }

            return (clusters, noise);
        }
    }
}
