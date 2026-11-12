using System;

namespace WPFTerraria
{
    public class Vector2
    {
        public double X { get; }
        public double Y { get; }

       public Vector2(double x, double y)
       {
           X = x;
           Y = y;
       }

        public static double Dot(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y; //scalar measure of how two vectors
interact directionally and in magnitude
        }
    }
}
