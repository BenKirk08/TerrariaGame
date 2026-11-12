using System;

namespace WPFTerraria
{
    internal static class PerlinNoise2D
    {
        private static readonly Random rand = new Random();

        private static readonly Dictionary<(int, int), Vector2[]>
gradientCache = new Dictionary<(int, int), Vector2[]>();

          //generate 2D gradient vectors for grid
          private static Vector2[] GetGradients(int gridWidth, int gridHeight)
          {
              if (!gradientCache.TryGetValue((gridWidth, gridHeight), out var
grads))
              {
                  grads = new Vector2[gridWidth * gridHeight];
                  for (int i = 0; i < gridWidth * gridHeight; i++)
                  {
                      //generate random 2D unit vectors
                      double angle = rand.NextDouble() * 2 * Math.PI; //*2pi ie
in radians
                     grads[i] = new Vector2(Math.Cos(angle), Math.Sin(angle));
//uses radians
                  }
                  gradientCache[(gridWidth, gridHeight)] = grads;
              }
              return grads;
          }

        //generate 2D Perlin noise value at (x, y)
        public static double GeneratePerlinNoise(double x, double y, int
baseGridSize, int octaves, double persistence)
        {
            double total = 0; //final perlin value (unnormalised) -> all
values of octaves added together
            double frequency = 1; //amplify gradinets
            double amplitude = 1; //decrease amplitude
            double maxValue = 0; //total maximum value you can have, if every
value gives max -> get by adding ampltudes of all octaves
                                                               3D loops from when called
              for (int i = 0; i < octaves; i++)
              {
                  //Scale grid dimensions exponentially to prevent repetition
                  int currentGridWidth = (int)(baseGridSize * frequency);
                  int currentGridHeight = (int)(baseGridSize * frequency);

                total += GenerateSinglePerlin(x, y, currentGridWidth,
currentGridHeight) * amplitude; //add to total


                 maxValue += amplitude; //maxmium posible value if all profuce
1
                 frequency *= 2; //multiply frequency of points by 2 each
octave
                 amplitude *= persistence;
             }

             return (total / maxValue); //normalised value between -1 and 1
         }

        //2D noise calculation
        private static double GenerateSinglePerlin(double x, double y, int
gridWidth, int gridHeight)
        {
            Vector2[] gradients = GetGradients(gridWidth, gridHeight);

             //scale coordinates to the larger grid
             double scaledX = x * (gridWidth - 1);
             double scaledY = y * (gridHeight - 1);

             //calc integer grid indices
             int pointX0 = (int)Math.Floor(scaledX) % gridWidth;
             int pointY0 = (int)Math.Floor(scaledY) % gridHeight;
             int pointX1 = (pointX0 + 1) % gridWidth;
             int pointY1 = (pointY0 + 1) % gridHeight;

             //fractional distances
             double distanceFromX0 = (scaledX) - Math.Floor(scaledX);
             double distanceFromY0 = (scaledY) - Math.Floor(scaledY);

             //gradient indices for the 4 corners -> 0,0 is bottom left, 1,1 is
top right
            int gradIndex00 = ((pointY0 * gridWidth) + (pointX0)) %
gradients.Length;
            int gradIndex01 = ((pointY0 * gridWidth) + (pointX1)) %
gradients.Length;
            int gradIndex10 = ((pointY1 * gridWidth) + (pointX0)) %
gradients.Length;
            int gradIndex11 = ((pointY1 * gridWidth) + (pointX1)) %
gradients.Length;

            //dot products between gradients and distance vectors
            double dot00 = Vector2.Dot(gradients[gradIndex00], new
Vector2(distanceFromX0, distanceFromY0));
            double dot01 = Vector2.Dot(gradients[gradIndex01], new
Vector2(distanceFromX0 - 1, distanceFromY0));
            double dot10 = Vector2.Dot(gradients[gradIndex10], new
Vector2(distanceFromX0, distanceFromY0 - 1));
            double dot11 = Vector2.Dot(gradients[gradIndex11], new
Vector2(distanceFromX0 - 1, distanceFromY0 - 1));



            //fade and interpolate
            double u = Fade(distanceFromX0); //smoothed horizontal
interpolation factor
            double v = Fade(distanceFromY0); //smoothed vertical interpolation
factor


            return LinearInterpolate(
                LinearInterpolate(dot00, dot01, u), //interpolate between
bottom - left(dot00) and bottom - right(dot01)
                LinearInterpolate(dot10, dot11, u), //interpolate between top
- left(dot10) and top - right(dot11)
                v //vertically interpolate between them both
            );
        }

        //Fade and linear interpolation
        private static double Fade(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }
        private static double LinearInterpolate(double a, double b, double t)
        {
            return a + t * (b - a);
        }
        public static void Clear2DGradientValues() //for memory & reuse
        {
            gradientCache.Clear();
        }
    }
}
