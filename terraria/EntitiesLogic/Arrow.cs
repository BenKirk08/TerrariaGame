using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using systemShapes = System.Windows.Shapes;


namespace WPFTerraria
{
    internal class Arrow
    {
        //arrow Image
        public Image arrowImage;
        public double arrowWidth = (double)Utilities.TileSize * (double)1.2;
        public double arrowHeight = (double)Utilities.TileSize * (double)0.6;
        public int totalArrowDamage;

       //positions
       public double arrowPosX;
       public double arrowPosY;

       //velocities
       public double arrowVelocityX;
       public double arrowVelocityY;

       //constants
       private const double gravity = 0.4;
       private const double arrowSpeed = 18;

       public Rect Hitbox
       {
           get
           {
               return new Rect(
                   arrowPosX - (arrowWidth / 2),
                   arrowPosY - (arrowHeight / 2),
                   arrowImage.Width,
                   arrowImage.Height
               );
           }
       }
       public Rect arrowHitboxCenter
       {
           get
           {
               return new Rect(
                   arrowPosX - 2,
                   arrowPosY - 2,

                    4,
                    4
               );
           }
       }

       public systemShapes.Rectangle thisArrowsHitboxRectangle;
       public systemShapes.Rectangle thisArrowsCenterHitboxRectangle;
       public RotateTransform ArrowHitboxTransform;


        public Arrow(double ArrowPosX, double ArrowPosY, double AngleFired,
bool FiredRight, BitmapSource ArrowImage, int TotalArrowDamage)
        {
            totalArrowDamage = TotalArrowDamage;
            arrowImage = new Image
            {
                Source = ArrowImage,
                Width = arrowWidth,
                Height = arrowHeight,
                Stretch = Stretch.Fill
            };

           var main = Application.Current.MainWindow as MainWindow;
           main.GameCanvas.Children.Add(arrowImage);
           Canvas.SetZIndex(arrowImage, 10);

           arrowPosX = ArrowPosX;
           arrowPosY = ArrowPosY;

            CalcAngleOfProjection(AngleFired, FiredRight);
        }
        private void CalcAngleOfProjection(double AngleFired, bool FiredRight)
        {
            double RadAngleFired = AngleFired / 180 * Math.PI;
            arrowVelocityX = arrowSpeed * Math.Cos(RadAngleFired);
            arrowVelocityY = arrowSpeed * Math.Sin(RadAngleFired) * -1; //*-1
as up is negative, down is positve

           if (!FiredRight) //going left therfore negative velocity
           {
               arrowVelocityX *= -1;
           }
       }

       public void UpdateArrow(MainWindow game)
       {
           //calculate rotation angle
           double angleRadians = Math.Atan2(arrowVelocityY, arrowVelocityX);
           double angleDegrees = angleRadians * 180 / Math.PI;

           //update rotation if transform exists -> not remaking rotation
           if (ArrowHitboxTransform != null)

           {
               ArrowHitboxTransform.Angle = angleDegrees;
               ArrowHitboxTransform.CenterX = arrowWidth / 2;
               ArrowHitboxTransform.CenterY = arrowHeight / 2;
           }

           //move arrow
           arrowPosX += arrowVelocityX;
           arrowPosY += arrowVelocityY;

           //apply gravity
           arrowVelocityY += gravity;

            //apply rotation to image
            arrowImage.RenderTransform = new RotateTransform(angleDegrees,
arrowWidth / 2, arrowHeight / 2);

            //position image on canvas
            Canvas.SetLeft(arrowImage, arrowPosX - game.cameraCenter.X +
game.halfCameraWidth - (arrowWidth / 2));
            Canvas.SetTop(arrowImage, arrowPosY - game.cameraCenter.Y +
game.halfCameraHeight - (arrowHeight / 2));
        }

   }

   public partial class MainWindow
   {
       private List<Arrow> currentArrowList = new List<Arrow>();
       private systemShapes.Rectangle ArrowHitboxRectangle;
       private systemShapes.Rectangle ArrowCenterHitboxRectangle;

       private void UpdateArrows()
       {
           foreach (var arrow in currentArrowList)
           {
               arrow.UpdateArrow(this);
           }
           UpdateArrowHitboxDisplay();
           DetectArrowHitsEnemyOrBoss();
           DetectArrowHitsTile();
       }

        private void FireArrow(double angleFired, bool firedRight,
BitmapSource imageSource, int TotalArrowDamage)
        {
            Arrow thisArrow = new Arrow(myPlayer.playerX, myPlayer.playerY,
angleFired, firedRight, imageSource, TotalArrowDamage);
            currentArrowList.Add(thisArrow);
            ArrowHitboxRectangle = new systemShapes.Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,

               Width = thisArrow.Hitbox.Width,
               Height = thisArrow.Hitbox.Height,
               Fill = Brushes.Transparent,
               Tag = "Hitbox"
            };
            thisArrow.thisArrowsHitboxRectangle = ArrowHitboxRectangle;
            GameCanvas.Children.Add(thisArrow.thisArrowsHitboxRectangle);
            Canvas.SetZIndex(thisArrow.thisArrowsHitboxRectangle, 9);
            thisArrow.thisArrowsHitboxRectangle.Visibility = showHitboxes ?
Visibility.Visible : Visibility.Collapsed;
            thisArrow.ArrowHitboxTransform = new RotateTransform(0);
            thisArrow.thisArrowsHitboxRectangle.RenderTransform =
thisArrow.ArrowHitboxTransform;

            ArrowCenterHitboxRectangle = new systemShapes.Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 4,
                Width = thisArrow.arrowHitboxCenter.Width,
                Height = thisArrow.arrowHitboxCenter.Height,
                Tag = "Hitbox"
            };
            thisArrow.thisArrowsCenterHitboxRectangle =
ArrowCenterHitboxRectangle;

GameCanvas.Children.Add(thisArrow.thisArrowsCenterHitboxRectangle);
            Canvas.SetZIndex(thisArrow.thisArrowsCenterHitboxRectangle, 9);
            thisArrow.thisArrowsCenterHitboxRectangle.Visibility =
showHitboxes ? Visibility.Visible : Visibility.Collapsed;
        }
        private void DetectArrowHitsEnemyOrBoss()
        {
            foreach (var arrow in currentArrowList.ToList()) // ToList() lets
us remove safely
            {
                if (arrow.thisArrowsHitboxRectangle == null) continue;

                //get world corners of arrows rotated hitbox
                Point[] arrowCorners = GetArrowHitboxCornersWorld(arrow);
                if (currentEyeBoss != null)
                {
                    //check eye boss hit
                    if (currentEyeBoss.eyeBossBeenHit.ElapsedMilliseconds >
500) //short cooldown between hits
                    {
                        if (RotatedRectIntersectsAABB(arrowCorners,
currentEyeBoss.Hitbox))
                        {
                            //eye boss hit
                            currentEyeBoss.eyeBossBeenHit.Restart();
                            currentEyeBoss.ChangeBossHealth(-
arrow.totalArrowDamage); //arrow.Damage if you add that field


                           //no boss knowckback
                           //remove the arrow on hit
                           RemoveArrow(arrow);
                           break; //exit loop for this arrow -> destroyed
                       }
                   }

                }
                foreach (var enemy in enemies)
                {
                    if (Math.Abs(arrow.arrowPosX - enemy.enemyX) < 150 &&
Math.Abs(arrow.arrowPosY - enemy.enemyY) < 150) //shorten checls
                    {
                        if (enemy.enemyBeenHit.ElapsedMilliseconds > 200 &&
!enemy.despawnEnemyStopwatch.IsRunning) //short cooldown between hits & loaded
in
                        {
                            if (RotatedRectIntersectsAABB(arrowCorners,
enemy.Hitbox))
                            {
                                //enemy hit
                                enemy.enemyBeenHit.Restart();
                                enemy.ChangeEnemyHealth(-
arrow.totalArrowDamage); //arrow.Damage if you add that field

                                //knockback
                                if (myPlayer.playerX < enemy.enemyX) //use
player not arrow as often arrow hit enemy at other side (corner)
                                {
                                    enemy.ApplyEnemyKnockback(1);
                                }
                                else
                                {
                                    enemy.ApplyEnemyKnockback(-1);
                                }

                               //remove the arrow on hit
                               RemoveArrow(arrow);
                               break; //exit enemy loop for this arrow
                           }
                       }
                   }
               }
           }
       }
       private Point[] GetArrowHitboxCornersWorld(Arrow arrow)
       {
           double arrowWorldX = arrow.arrowPosX;
           double arrowWorldY = arrow.arrowPosY;
           double arrowWidth = arrow.arrowWidth;
           double arrowHeight = arrow.arrowHeight;

            //rotation (in radians)

            double angle = Math.Atan2(arrow.arrowVelocityY,
arrow.arrowVelocityX);

           //local corner offsets from center
           Vector[] corners =
           {
               new Vector(-arrowWidth/2, -arrowHeight/2),
               new Vector(arrowWidth/2, -arrowHeight/2),
               new Vector(arrowWidth/2, arrowHeight/2),
               new Vector(-arrowWidth/2, arrowHeight/2)
           };

            Point[] worldCorners = new Point[4];
            for (int i = 0; i < 4; i++)
            {
                double rotatedX = corners[i].X * Math.Cos(angle) -
corners[i].Y * Math.Sin(angle);
                double rotatedY = corners[i].X * Math.Sin(angle) +
corners[i].Y * Math.Cos(angle);
                worldCorners[i] = new Point(arrowWorldX + rotatedX,
arrowWorldY + rotatedY);
            }

            return worldCorners;
        }
        private void DetectArrowHitsTile() //checks rotated and doesnt have to
have hitboxes visible
        {
            foreach (var arrow in currentArrowList.ToList()) //copy of list
for safe removal
            {
                //get rotated hitbox corners
                Point[] arrowCorners = GetArrowHitboxCornersWorld(arrow);

                //only check nearby tiles
                int startTileX = Math.Max(0, (int)((arrow.arrowPosX -
arrow.arrowWidth) / Utilities.TileSize));
                int endTileX = Math.Min(mapX_Width - 1, (int)((arrow.arrowPosX
+ arrow.arrowWidth) / Utilities.TileSize));
                int startTileY = Math.Max(0, (int)((arrow.arrowPosY -
arrow.arrowHeight) / Utilities.TileSize));
                int endTileY = Math.Min(mapY_Height - 1,
(int)((arrow.arrowPosY + arrow.arrowHeight) / Utilities.TileSize));

                bool hitTile = false;

               for (int x = startTileX; x <= endTileX && !hitTile; x++)
               {
                   for (int y = startTileY; y <= endTileY && !hitTile; y++)
                   {
                       byte tileID = tileMapData[x, y];
                       if (tileID == 0) continue; //skip empty air tiles


                         if (TileDictionary.TileData.TryGetValue(tileID, out
var tileInfo))
                         {
                             //only collide with solid & not walkable tiles
                             if (tileInfo.IsSolid &&
!tileInfo.IsWalkableThrough)
                            {
                                 //tiles world rectangle
                                 Rect tileRect = new Rect(
                                     x * Utilities.TileSize,
                                     y * Utilities.TileSize,
                                     Utilities.TileSize,
                                     Utilities.TileSize
                                 );

                                 //check rotated arrow vs tile AABB -> uses
arrow corners
                                 if (RotatedRectIntersectsAABB(arrowCorners,
tileRect))
                                 {
                                     hitTile = true;
                                     break;              5D loop
                                 }
                             }
                         }
                     }
                 }

                 if (hitTile)
                 {
                     RemoveArrow(arrow); //remove arrow on impact
                 }
             }
        }

        private void RemoveArrow(Arrow arrow)
        {
            GameCanvas.Children.Remove(arrow.thisArrowsHitboxRectangle);
            GameCanvas.Children.Remove(arrow.thisArrowsCenterHitboxRectangle);
            GameCanvas.Children.Remove(arrow.arrowImage);
            currentArrowList.Remove(arrow);
        }
    }
}
