using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bot.MapAnalysis
{
    public static class DBSCAN
    {
        public static List<List<Vector2>> Cluster(List<Vector2> points, float epsilon, int minPoints)
        {
            var clusters = new List<List<Vector2>>();
            var visited = new HashSet<Vector2>();
            var noise = new List<Vector2>();

            foreach (var point in points)
            {
                if (visited.Contains(point))
                    continue;

                visited.Add(point);
                var neighbors = GetNeighbors(point, points, epsilon);

                if (neighbors.Count < minPoints)
                {
                    noise.Add(point);
                }
                else
                {
                    var cluster = new List<Vector2>();
                    clusters.Add(cluster);
                    ExpandCluster(point, neighbors, cluster, points, visited, epsilon, minPoints);
                }
            }

            return clusters;
        }

        private static void ExpandCluster(Vector2 point, List<Vector2> neighbors, List<Vector2> cluster, List<Vector2> points, HashSet<Vector2> visited, float epsilon, int minPoints)
        {
            cluster.Add(point);

            for (int i = 0; i < neighbors.Count; i++)
            {
                var neighbor = neighbors[i];

                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    var neighborNeighbors = GetNeighbors(neighbor, points, epsilon);

                    if (neighborNeighbors.Count >= minPoints)
                    {
                        neighbors.AddRange(neighborNeighbors.Where(n => !neighbors.Contains(n)));
                    }
                }

                if (!cluster.Contains(neighbor))
                {
                    cluster.Add(neighbor);
                }
            }
        }

        private static List<Vector2> GetNeighbors(Vector2 point, List<Vector2> points, float epsilon)
        {
            var neighbors = new List<Vector2>();

            foreach (var p in points)
            {
                if (Vector2.Distance(point, p) <= epsilon)
                {
                    neighbors.Add(p);
                }
            }

            return neighbors;
        }
    }
}
