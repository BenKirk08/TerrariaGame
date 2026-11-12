using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using systemShapes = System.Windows.Shapes;

namespace WPFTerraria
{
    public partial class MainWindow
    {
        private readonly List<systemShapes.Rectangle> collisionBuffer = new
List<systemShapes.Rectangle>(2); //3 as 2 is max amount used, but want to be
safe, (pre-allocated, reusable list)
        public systemShapes.Rectangle[] CheckCollisonWithTiles(double newX,
double newY, int maxAmountToCheck, double hitboxWidth, double hitboxHeight)
//returns tile collided with
        {
            //create virtual new hitbox when moved to see if collision
detected
            Rect newPlayerHitbox = new Rect(
                newX - (hitboxWidth / 2),
                newY - (hitboxHeight / 2),
                hitboxWidth,
                hitboxHeight
            );
            collisionBuffer.Clear();

            //get the position in tiles that the player is covering
            int startX = Math.Max(0, (int)(newPlayerHitbox.X /
Utilities.TileSize)); //make sure in tiles and not pixels
            int startY = Math.Max(0, (int)(newPlayerHitbox.Y /
Utilities.TileSize));
            int endX = Math.Min(mapX_Width - 1, (int)(newPlayerHitbox.X +
newPlayerHitbox.Width - 0.001) / Utilities.TileSize); //-1 as integer division
will round down and map width is eg 250 (0 -> 249)
            int endY = Math.Min(mapY_Height - 1, (int)(newPlayerHitbox.Y +
newPlayerHitbox.Height - 0.001) / Utilities.TileSize);

            //loop through every tiles in player bounderies -> small so
efficient
            for (int x = startX; x <= endX; x++)
            {                                             5D loop connected within
                for (int y = startY; y <= endY; y++)      Enemy & main loop
                {
                    if (hitboxMap[x, y] != null) //if hitbox exists, tile is
solid
                    {
                        Rect hitboxbitsmaller = new Rect(
                            Canvas.GetLeft(hitboxMap[x, y]) + cameraCenter.X -
GameCanvas.Width / 2,

                              Canvas.GetTop(hitboxMap[x, y]) + cameraCenter.Y -
GameCanvas.Height / 2,
                              hitboxMap[x, y].Width - 0.02,
                              hitboxMap[x, y].Height - 0.02
                         );

                        if (newPlayerHitbox.IntersectsWith(hitboxbitsmaller))
//if playerhitbox is in tile hitbox
                        {
                            collisionBuffer.Add(hitboxMap[x, y]);
                            if (collisionBuffer.Count >= maxAmountToCheck)
//if amount of collisons in array
                            {
                                return collisionBuffer.ToArray(); //return all
collisons detected
                            }
                        }
                    }
                }
            }
            if (collisionBuffer.Count > 0)
            {
                return collisionBuffer.ToArray(); //return less than max
            }
            return null; //no collision

        }
        private void CheckIfPlayerStillGrounded() //after horizontal movement
-> dont call all time as waste of memory, only when move
        {
            //check if the player still grounded
            if (isGrounded)
            {
                //check for ground 1 pixel below to see if still grounded
                systemShapes.Rectangle[] groundCheck =
CheckCollisonWithTiles(myPlayer.playerX, myPlayer.playerY + 1, 1,
myPlayer.HitboxWidth, myPlayer.HitboxHeight); //if not 1 pixel below, no
collision due to collision logic, only need to check 1 collsion
                if (groundCheck == null)
                {
                    isGrounded = false;
                    jumpingAnimationStopwatch.Restart();
                    velocityY = 0; //start falling immediately -> no upward
force
                }
            }
        }
    }
}
