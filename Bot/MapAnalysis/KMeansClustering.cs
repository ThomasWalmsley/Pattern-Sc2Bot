using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public static class KMeansClustering
    {
        public static List<Vector2> KMeansCluster(List<Unit> units, int k, int maxIterations = 100)
        {
            Random rand = new Random();

            List<Vector2> centroids = new List<Vector2>();

            // Initialize k random centroids from resource positions
            for (int i = 0; i < k; i++)
            {
                Unit randomUnit = units[rand.Next(units.Count)];
                centroids.Add(randomUnit.Position.ToVector2());

            }

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                // Step 1: Assign units to the nearest centroid
                var clusters = new List<List<Unit>>();
                for (int i = 0; i < k; i++) clusters.Add(new List<Unit>());

                foreach (var unit in units)
                {
                    int closestCentroidIndex = GetClosestCentroidIndex(unit.Position.ToVector2(), centroids);
                    clusters[closestCentroidIndex].Add(unit);
                }

                // Step 2: Recalculate centroids as the mean of the assigned resources
                bool centroidsChanged = false;
                for (int i = 0; i < k; i++) 
                {
                    if (clusters[i].Count > 0) 
                    {
                        var newCentroid = CalculateCentroid(clusters[i]);
                        if (GetDistance(centroids[i], newCentroid) > 0.1) // Threshold for detecting change
                        {
                            centroids[i] = newCentroid;
                            centroidsChanged = true;
                        }
                    }
                    // Stop if centroids have not changed
                    if (!centroidsChanged) break;
                }

            }
            return centroids;
        }


        private static int GetClosestCentroidIndex(Vector2 pos, List<Vector2> centroids) 
        {
            int closestIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < centroids.Count; i++)
            {
                float distance = GetDistance(pos, centroids[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }
            return closestIndex;
        }

        private static Vector2 CalculateCentroid(List<Unit> units)
        {
            float xSum = 0, ySum = 0;
            foreach (var unit in units)
            {
                xSum += unit.Position.X;
                ySum += unit.Position.Y;
            }

            return new Vector2
            {
                X = xSum / units.Count,
                Y = ySum / units.Count,
            };
        }

        private static float GetDistance(Vector2 pos1, Vector2 pos2)
        {
            return (float)Math.Sqrt(Math.Pow(pos1.X - pos2.X, 2) + Math.Pow(pos1.Y - pos2.Y, 2));
        }

    }
}
