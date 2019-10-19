using Out.Internal.Core;
using Out.Utility;
using System;

namespace WowAI
{
    internal partial class Host
    {
        public double MyDistance(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            return Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2) + Math.Pow((z1 - z2), 2));
        }

        public double MyDistance(Vector3F loc1, Vector3F loc2)
        {
            return Math.Sqrt(Math.Pow((loc1.X - loc2.X), 2) + Math.Pow((loc1.Y - loc2.Y), 2) + Math.Pow((loc1.Z - loc2.Z), 2));
        }

        public double MyDistanceGpcPoint(GpsPoint loc1, GpsPoint loc2)
        {
            return Math.Sqrt(Math.Pow((loc1.X - loc2.X), 2) + Math.Pow((loc1.Y - loc2.Y), 2) + Math.Pow((loc1.Z - loc2.Z), 2));
        }

        public double MyDistanceVectorGpsPoint(Vector3F loc1, GpsPoint loc2)
        {
            return Math.Sqrt(Math.Pow((loc1.X - loc2.X), 2) + Math.Pow((loc1.Y - loc2.Y), 2) + Math.Pow((loc1.Z - loc2.Z), 2));
        }

        public double MyDistanceNoZ(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
        }
    }
}