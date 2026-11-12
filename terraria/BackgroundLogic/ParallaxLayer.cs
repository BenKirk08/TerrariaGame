using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFTerraria
{
    internal class ParallaxLayer
    {
        private Image image1;
        private Image image2; //repeat/loop over (same image)
        private Image image3;

        private double speedX;
        private double speedY; //speeds they move vertically and horizonatlly

        private int originY;
        private int yStart;

        public ParallaxLayer(BitmapSource source, double speedX, double
speedY, Canvas canvas, int zIndex, double sizeMultiplier, int originY, int
ystart)
        {
            this.speedX = speedX;
            this.speedY = speedY;
            this.originY = originY;
            yStart = ystart;

           image1 = new Image
           {
               Source = source,
               Width = source.PixelWidth * sizeMultiplier,
               Height = source.PixelHeight * sizeMultiplier,
               Stretch = Stretch.Fill
           };

           image2 = new Image
           {
               Source = source,
               Width = source.PixelWidth * sizeMultiplier,
               Height = source.PixelHeight * sizeMultiplier,
               Stretch = Stretch.Fill
           };

           image3 = new Image
           {
               Source = source,
               Width = source.PixelWidth * sizeMultiplier,
               Height = source.PixelHeight * sizeMultiplier,
               Stretch = Stretch.Fill

           };

           //insert behind game tiles
           canvas.Children.Add(image1);
           Canvas.SetZIndex(image1, zIndex);
           canvas.Children.Add(image2);
           Canvas.SetZIndex(image2, zIndex);
           canvas.Children.Add(image3);
           Canvas.SetZIndex(image3, zIndex);
       }

       public void Update(Point cameraCenter)
       {

            double offsetX = -((cameraCenter.X) * speedX) %
image1.ActualWidth;
            //Ensure offset is negative so image1 starts at the left
            if (offsetX > 0) offsetX -= image1.ActualWidth;

            //parallax from an origin
            double offsetY = originY - ((cameraCenter.Y - yStart) * speedY);

           //Position the two images side by side
           Canvas.SetLeft(image1, offsetX);
           Canvas.SetTop(image1, offsetY);

           Canvas.SetLeft(image2, offsetX + image1.ActualWidth);
           Canvas.SetTop(image2, offsetY);


            if (offsetX + image1.ActualWidth + image2.ActualWidth <
MainWindow.CameraWidth * Utilities.TileSize)
            {
                image3.Visibility = Visibility.Visible;
                Canvas.SetLeft(image3, offsetX + image1.ActualWidth * 2);
                Canvas.SetTop(image3, offsetY);
            }
            else
            {
                image3.Visibility = Visibility.Hidden;
            }
        }
    }
}
