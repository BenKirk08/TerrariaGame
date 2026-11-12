using System;

namespace WPFTerraria
{
    internal static class PerlinNoise1D
    {
        private static Random rand = new Random(); //can later fix for fixed
seeds -> new Random(number)

        //pre-cached gradients so when called all the same
        private static Dictionary<int, double[]> gradientCache = new
Dictionary<int, double[]>();
        private static double[] GetGradients(int gridSize) //initilise random
gradients to each gridpoint on grid
        {
            if (!gradientCache.TryGetValue(gridSize, out var grads)) //if not
alr in dictionary make cache
            {
                grads = new double[gridSize]; //double array of gradients,
with gridsize amount
                for (int i = 0; i < gridSize; i++)
                {
                    grads[i] = (rand.NextDouble() * 2) - 1; //Give Random
double value between -1 and 1
                    //Console.WriteLine(grads[i]);
                }
                gradientCache[gridSize] = grads; //store in dictionary, with
that gridsize (certain octave)
            }
            return grads;
        }

        //NOTE: x represents tile horizontal position in map
        public static double GeneratePerlinNoise(double x, int baseGridSize,
int octaves, double persistence) //returns value -1 to 1 which can amplify to
give height
        {
            //if (gradients == null) throw new Exception("Call
InitializeGradients() first!"); //if gradients not initised, do first

            double total = 0; //final perlin value (unnormalised) -> all
values of octaves added together
            double frequency = 1; //amplify gradinets
            double amplitude = 1; //decrease amplitude
            double maxValue = 0; //total maximum value you can have, if every
value gives max -> get by adding ampltudes of all octaves

           for (int i = 0; i < octaves; i++)

           {
                int currentGridSize = (int)(baseGridSize * frequency); //give
grid size (amount of points) for each octave -> each gridpoint has value of 0
                total += GenerateSinglePerlin(x, currentGridSize) * amplitude;
//add together, to add each perlin noise line
                maxValue += amplitude;

               //each ocatve increase frequency and decrase amplitude
               frequency *= 2.5; //increase frequency (more waves) -> by 2.5n
               amplitude *= persistence; //decrease amplitude (less effect)
           }

            return total / maxValue; //normalized based on amount of octaves
by /maxvalue -> [-1 to 1]
        }

        private static double GenerateSinglePerlin(double x, int gridSize)
//generate new random perlin noise line
        {
            //generate a new gradient for this octave
            double[] gradients = GetGradients(gridSize);

            //1 -> calc integer grid indices
            int pointX0 = (int)Math.Floor(x * gridSize); //where (x *
gridsize) is in terms of gridsize -> therfore same length nomatter current
gridsize, returns rounded down number
            double distanceFromX0 = (x * gridSize) - pointX0; //distance from
left point

            //2 -> use integer mod to get gradient indices -> the
gradients/random numbers around the point x ie that tile
            int gradientIndex0 = pointX0 % gridSize; //left gradient
            int gradientIndex1 = (pointX0 + 1) % gridSize; //right gradient

            //3 -> get gradients
            double gradient0 = gradients[gradientIndex0];
            double gradient1 = gradients[gradientIndex1];

            //4 -> Interpolate
            double dot0 = gradient0 * distanceFromX0; //influence of left
            double dot1 = gradient1 * (distanceFromX0 - 1); //influence of
right -> becomes negative from -1. used to interpolate. height of line
interpolating

            double u = Fade(distanceFromX0); //fade
            return LinearInterpolate(dot0, dot1, u); //returns interpolated
point form -1 to 1

       }

       private static double Fade(double t) //smoothing equation
       {

            return t * t * t * (t * (t * 6 - 15) + 10); //fade around
specified point -> 6t⁵ -15t⁴ +10t³
        }

        private static double LinearInterpolate(double a, double b, double t)
//linear interpolation, a,b -> 2 points, t -> point distance from a to b 0-1
        {
            return a + t * (b - a); //blends 'a' and b' based on 't'
        }

        public static void Clear1DGradientValues() //for memory & reuse
        {
            gradientCache.Clear();
        }

    }
}
