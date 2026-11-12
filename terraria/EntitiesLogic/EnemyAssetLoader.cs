using System;
using System.Windows.Media.Imaging;

namespace WPFTerraria
{
    public static class EnemyAssetLoader
    {
        private static readonly Dictionary<string, List<BitmapSource>>
allAnimations = new Dictionary<string, List<BitmapSource>>();

        //zombies
        private const int NumberOfZombies = 3;
        private const int ZombieAnimationFrames = 3;


        //eye boss
        private const int EyeBossAnimationFrames = 3;
        private const int EyeBossPhases = 2;


       static EnemyAssetLoader()
       {
           LoadAllEnemyAnimations();
           LoadEyeBossAnimations();
       }

        private static void LoadAllEnemyAnimations()
        {
            //loading Zombie animations
            for (int i = 1; i <= NumberOfZombies; i++)
            {
                string enemyType = $"Zombie{i}";
                var animations = new List<BitmapSource>();
                for (int frame = 1; frame <= ZombieAnimationFrames; frame++)
                {
                    string picturePath =
System.IO.Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets",
"PixelArt", "Enemy", "Zombie", $"{enemyType}Run{frame}.png");
                    BitmapSource image = Utilities.LoadImage(picturePath);
                    animations.Add(image);
                }
                allAnimations[enemyType] = animations;
            }
        }

       private static void LoadEyeBossAnimations()
       {
           //loading Eye Boss animations
           for (int i = 1; i <= EyeBossPhases; i++)
           {
               string BossType = $"EyeStage{i}";

                var animations = new List<BitmapSource>();
                for (int frame = 1; frame <= EyeBossAnimationFrames; frame++)
                {
                    string picturePath =
System.IO.Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets",
"PixelArt", "Enemy", "Boss", "Eye", $"{BossType}_{frame}.png");
                    BitmapSource image = Utilities.LoadImage(picturePath);
                    animations.Add(image);
                }
                allAnimations[BossType] = animations;
            }
        }


        //Retrieve animations for a specific enemy type
        public static List<BitmapSource> GetAnimation(string type)
        {
            return allAnimations.ContainsKey(type) ? allAnimations[type] :
null;
        }
    }
}
