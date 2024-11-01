using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Numerics;
using SC2APIProtocol;

namespace Bot
{
    public static class PointExtensions
    {
        //Summary
        //adds ways to convert point to different vector types
        public static Vector2 ToVector2(this Point point, float xOffset = 0, float yOffset = 0)
        {
            return new Vector2
            {
                X = point.X + xOffset,
                Y = point.Y + yOffset,
            };
        }

        public static Vector3 ToVector3(this Point point)
        {
            return new Vector3
            {
                X = point.X,
                Y = point.Y,
                Z = point.Z,
            };
        }

        public static Vector2 ToVector2(this Point2D point)
        {
            return new Vector2
            {
                X = point.X,
                Y = point.Y,
            };
        }
    }
}
