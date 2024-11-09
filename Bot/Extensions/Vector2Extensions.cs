using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace Bot
{
    public static class Vector2Extensions
    {
        public static float DistanceTo(this Vector2 origin, Vector2 destination)
        {
            return Vector2.Distance(origin, destination);
        }
    }
}
