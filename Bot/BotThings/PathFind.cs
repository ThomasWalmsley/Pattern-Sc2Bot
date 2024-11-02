using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public static class PathFind
    {
        public static List<Vector3> FindPath(Vector3 start, Vector3 end, StartRaw startRaw) 
        {
            return AStar(start, end, startRaw.PathingGrid);
        }

        private static float Heuristic(Vector3 a, Vector3 b)
        {
            // Using Euclidean distance as the heuristic
            return Vector3.Distance(a, b);
        }

        private static List<Vector3> ReconstructPath(Dictionary<Vector3, Vector3> cameFrom, Vector3 current)
        {
            List<Vector3> totalPath = new List<Vector3> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Add(current);
            }
            totalPath.Reverse();
            return totalPath;
        }

        private static List<Vector3> GetNeighbors(Vector3 node, global::SC2APIProtocol.ImageData pathingGrid)
        {
            // This method should be implemented to return the neighbors of the given node
            // For simplicity, let's assume a grid-based map with 8 possible movements (N, S, E, W, NE, NW, SE, SW)
            List<Vector3> neighbors = new List<Vector3>
            {
                new Vector3(node.X + 1, node.Y, node.Z),
                new Vector3(node.X - 1, node.Y, node.Z),
                new Vector3(node.X, node.Y + 1, node.Z),
                new Vector3(node.X, node.Y - 1, node.Z),
                new Vector3(node.X + 1, node.Y + 1, node.Z),
                new Vector3(node.X - 1, node.Y + 1, node.Z),
                new Vector3(node.X + 1, node.Y - 1, node.Z),
                new Vector3(node.X - 1, node.Y - 1, node.Z)
            };

            // Filter out neighbors that are not walkable based on the pathingGrid
            neighbors.RemoveAll(neighbor => !IsWalkable(neighbor, pathingGrid));

            return neighbors;
        }

        private static bool IsWalkable(Vector3 position, global::SC2APIProtocol.ImageData pathingGrid)
        {
            // Implement logic to check if the position is walkable based on the pathingGrid
            // This is a placeholder implementation and should be replaced with actual logic
            int x = (int)position.X;
            int y = (int)position.Y;
            return pathingGrid.Data[y * pathingGrid.Size.X + x] == 1;
        }

        private static List<Vector3> AStar(Vector3 start, Vector3 end, global::SC2APIProtocol.ImageData pathingGrid)
        {
            HashSet<Vector3> closedSet = new HashSet<Vector3>();
            HashSet<Vector3> openSet = new HashSet<Vector3> { start };
            Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();

            Dictionary<Vector3, float> gScore = new Dictionary<Vector3, float>
            {
                [start] = 0
            };

            Dictionary<Vector3, float> fScore = new Dictionary<Vector3, float>
            {
                [start] = Heuristic(start, end)
            };

            while (openSet.Count > 0)
            {
                Vector3 current = default(Vector3);
                float minFScore = float.MaxValue;
                foreach (var node in openSet)
                {
                    if (fScore.TryGetValue(node, out float score) && score < minFScore)
                    {
                        minFScore = score;
                        current = node;
                    }
                }

                if (current.Equals(end))
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (var neighbor in GetNeighbors(current, pathingGrid))
                {
                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    float tentativeGScore = gScore[current] + Vector3.Distance(current, neighbor);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    else if (tentativeGScore >= gScore[neighbor])
                    {
                        continue;
                    }

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, end);
                }
            }

            return new List<Vector3>(); // Return an empty path if no path is found
        }
    }
}
