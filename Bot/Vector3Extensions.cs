﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Numerics;
using SC2APIProtocol;

namespace Bot
{
    public static class Vector3Extensions
    {
        public static Point ToPoint(this Vector3 vector, float xOffset = 0, float yOffset = 0, float zOffset = 0)
        {
            return new Point
            {
                X = vector.X + xOffset,
                Y = vector.Y + yOffset,
                Z = vector.Z + zOffset,
            };
        }

        public static Point2D ToPoint2D(this Vector3 vector, float xOffset = 0, float yOffset = 0)
        {
            return new Point2D
            {
                X = vector.X + xOffset,
                Y = vector.Y + yOffset,
            };
        }

        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
        public static float DistanceTo(this Vector3 origin, Vector3 destination)
        {
            return Vector3.Distance(origin, destination);
        }

        public static Vector3 DirectionTo(this Vector3 origin, Vector3 destination, bool ignoreZAxis = true)
        {
            var direction = Vector3.Normalize(destination - origin);
            if (ignoreZAxis)
            {
                direction.Z = 0;
            }

            return direction;
        }

    }
}
