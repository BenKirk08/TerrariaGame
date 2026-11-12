using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
namespace WPFTerraria
{
    public class ParallaxUpdater
    {
        private readonly List<ParallaxLayer> layers = new();

        public void AddLayer(BitmapSource source, double speedX, double
speedY, Canvas canvas, int zIndex, double sizeMultiplier, int originY, int
yStart)
        {
            var layer = new ParallaxLayer(source, speedX, speedY, canvas,
zIndex, sizeMultiplier, originY, yStart);
            layers.Add(layer);
        }

        public void UpdateAll(Point cameraCenter)
        {
            foreach (var layer in layers)
            {
                layer.Update(cameraCenter);
            }
        }
    }
}




Cra;ing
