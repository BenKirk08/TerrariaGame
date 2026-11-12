using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace WPFTerraria
{
    internal class ParallaxImageLoader
    {
        private string[] ParallaxImageNames = { "ForestBackground",
"ForestTrees", "CaveBackground" };
        protected Dictionary<string, BitmapSource> ParallaxImage_Dict = new
Dictionary<string, BitmapSource>();
        public ParallaxImageLoader()
        {
            LoadAllParallaxImages();
        }
        private void LoadAllParallaxImages()
        {
            foreach (string parallaxImageName in ParallaxImageNames)
            {
                string PicturePath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "Content",
"Backgrounds", parallaxImageName + ".png");
                BitmapSource ThisParallaxImage =
Utilities.LoadImage(PicturePath);
                ParallaxImage_Dict.Add(parallaxImageName, ThisParallaxImage);
            }
        }
        public BitmapSource GetParallaxImage(string parallaxImageName)
//returns image of that name from dictionary
        {
            return ParallaxImage_Dict[parallaxImageName];
        }
    }
}
