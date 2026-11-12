using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFTerraria
{
    internal class CaveBackground
    {
        private readonly Image[,] tiles = new Image[3, 3];
        private readonly double tileWidth;
        private readonly double tileHeight;

        private readonly int[] mapHeight;
        private readonly Canvas caveLayer; //conatins the background images
and outline

        private System.Windows.Shapes.Path outlinePath;
        public CaveBackground(BitmapSource source, double sizeMultiplier,
Canvas mainCanvas, int zIndex, int[] mapHeight)
        {
            this.mapHeight = mapHeight;

           tileWidth = source.PixelWidth * sizeMultiplier;
           tileHeight = source.PixelHeight * sizeMultiplier;

           //Create a veiwing sized canvas for the cave tiles
           caveLayer = new Canvas
           {
               Width = MainWindow.CameraWidth * Utilities.TileSize,
               Height = MainWindow.CameraHeight * Utilities.TileSize,
               IsHitTestVisible = false
           };
           mainCanvas.Children.Add(caveLayer);
           Canvas.SetZIndex(caveLayer, zIndex);

           //Create 3x3 tile grid inside canvas
           for (int y = 0; y < 3; y++)
           {
               for (int x = 0; x < 3; x++)
               {
                   var img = new Image
                   {
                       Source = source,
                       Width = tileWidth,
                       Height = tileHeight,
                       Stretch = Stretch.Fill


                   };
                   caveLayer.Children.Add(img);
                   tiles[x, y] = img;
               }
           }
           outlinePath = new System.Windows.Shapes.Path
           {
               Stroke = Brushes.Black,
               StrokeThickness = 6,
               IsHitTestVisible = false
           };
           caveLayer.Children.Add(outlinePath);
           Canvas.SetZIndex(outlinePath, 2);
       }

       public void Update(Point cameraCenter)
       {
           //tiles move w player
           double offsetX = -(cameraCenter.X % tileWidth);
           if (offsetX > 0) offsetX -= tileWidth;

            double offsetY = -(cameraCenter.Y % tileHeight);
            if (offsetY > 0) offsetY -= tileHeight;

           //Position the 3x3 tiles in the container
           for (int y = 0; y < 3; y++)
           {                                             3D loop when called in main
               for (int x = 0; x < 3; x++)
                                                         loop
               {
                   var img = tiles[x, y];
                   Canvas.SetLeft(img, offsetX + x * tileWidth);
                   Canvas.SetTop(img, offsetY + y * tileHeight);
               }
           }
       }


       public void UpdateClip(Point cameraCenter)
       {
           double screenW = caveLayer.Width;
           double screenH = caveLayer.Height;

            double worldLeftPx = cameraCenter.X - screenW / 2.0;
            double worldTopPx = cameraCenter.Y - screenH / 2.0;

            int firstTileX = (int)Math.Floor(worldLeftPx /
Utilities.TileSize);
            int lastTileX = (int)Math.Ceiling((worldLeftPx + screenW) /
Utilities.TileSize);

            //image
            var clipGeo = new StreamGeometry();
            using (var ctx = clipGeo.Open())

            {
                ctx.BeginFigure(new Point(0, screenH), isFilled: true,
isClosed: true);

                double lastGroundY = mapHeight[Math.Clamp(firstTileX, 0,
mapHeight.Length - 1)] * Utilities.TileSize - worldTopPx;

                for (int tx = firstTileX; tx <= lastTileX; tx++)
                {
                    int clampedX = Math.Clamp(tx, 0, mapHeight.Length - 1);
                    double groundY = mapHeight[clampedX] * Utilities.TileSize
- worldTopPx;
                    double screenX = (tx * Utilities.TileSize) - worldLeftPx;

                    //Steping horizontal then vertical
                    ctx.LineTo(new Point(screenX, lastGroundY), true, false);
                    ctx.LineTo(new Point(screenX, groundY), true, false);

                    lastGroundY = groundY;
                }

                ctx.LineTo(new Point(screenW, screenH), true, false);
            }
            clipGeo.Freeze();
            caveLayer.Clip = clipGeo;

             // black outline alongg top
             var outlineGeo = new StreamGeometry();
             using (var ctx = outlineGeo.Open())
            {
                 double lastGroundY = mapHeight[Math.Clamp(firstTileX, 0,
mapHeight.Length - 1)] * Utilities.TileSize - worldTopPx;
                 double startX = (firstTileX * Utilities.TileSize) -
worldLeftPx;

                ctx.BeginFigure(new Point(startX, lastGroundY), isFilled:
false, isClosed: false);

                for (int tx = firstTileX; tx <= lastTileX; tx++)
                {
                    int clampedX = Math.Clamp(tx, 0, mapHeight.Length - 1);
                    double groundY = mapHeight[clampedX] * Utilities.TileSize
- worldTopPx;
                    double screenX = (tx * Utilities.TileSize) - worldLeftPx;

                    // Horizontal along previous Y
                    ctx.LineTo(new Point(screenX, lastGroundY), true, false);
                    //Vertical to current Y
                    ctx.LineTo(new Point(screenX, groundY), true, false);

                    lastGroundY = groundY;
                }
            }

            outlineGeo.Freeze();

            outlinePath.Data = outlineGeo;
            //outline ontop of tiles
            Canvas.SetZIndex(outlinePath, 999);
        }
    }
}
