using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using systemShapes = System.Windows.Shapes;

namespace WPFTerraria
{

    public partial class MainWindow
    {
        public readonly int Tile_PreloadX = 4;
        public readonly int Tile_PreloadY = 4;

        private List<(int x, int y)> previousFrameActiveHitboxes = new
List<(int x, int y)>();
        private HashSet<(int x, int y)> currentFrameActiveHitboxes = new
HashSet<(int x, int y)>();

        //updating tiles within camera
        private void UpdateVisibleTiles()
        {
            int cameraTileX = (int)(cameraCenter.X / Utilities.TileSize);
            int cameraTileY = (int)(cameraCenter.Y / Utilities.TileSize);

             //calcukate chunk range
             int chunkStartX = (cameraTileX - (CameraWidth / 2) -
Tile_PreloadX) / chunkSize;
             int chunkEndX = (cameraTileX + (CameraWidth / 2) + Tile_PreloadX)
/ chunkSize;

             int chunkStartY = (cameraTileY - (CameraHeight / 2) -
Tile_PreloadY + 1) / chunkSize;
             int chunkEndY = (cameraTileY + (CameraHeight / 2) + Tile_PreloadY)
/ chunkSize;

            currentFrameActiveHitboxes.Clear(); //reset for next frame

            //clamp values to prevent off map
            chunkStartX = Math.Clamp(chunkStartX, 0, mapX_Width / chunkSize);
            chunkStartY = Math.Clamp(chunkStartY, 0, mapY_Height / chunkSize);
            chunkEndX = Math.Clamp(chunkEndX, 0, mapX_Width / chunkSize);
            chunkEndY = Math.Clamp(chunkEndY, 0, mapY_Height / chunkSize);

            bool chunksLoaded = false;

            for (int chunkX = chunkStartX; chunkX <= chunkEndX; chunkX++)
            {
                for (int chunkY = chunkStartY; chunkY <= chunkEndY; chunkY++)

                {
                    if (!loadedChunks.Contains((chunkX, chunkY))) //adds adn
if doesnt conatin will do inside if loop
                    {                                   4D Loops from
                        LoadChunk(chunkX, chunkY);      when called load
                        chunksLoaded = true;
                                                        chunk
                    }

                    int tileStartX = chunkX * chunkSize;
                    int tileStartY = chunkY * chunkSize;
                    int tileEndX = Math.Min((chunkX + 1) * chunkSize,
mapX_Width);
                    int tileEndY = Math.Min((chunkY + 1) * chunkSize,
mapY_Height);

                    //process each tile in chunk
                    for (int x = tileStartX; x < tileEndX; x++)     6D Loop when
                    {
                                                                    called from game
                        for (int y = tileStartY; y < tileEndY; y++)
                        {                                           loop
                            byte tileID = tileMapData[x, y];

                            if (!TileDictionary.TileData[tileID].IsSolid ||
TileDictionary.TileData[tileID].IsWalkableThrough) continue;

                            //skip iteration if tiles not solid
                            currentFrameActiveHitboxes.Add((x, y));


                            //get hitbox from pool
                            systemShapes.Rectangle hitbox =
GetHitboxFromPool(x, y);
                            hitboxMap[x, y] = hitbox; //store in array

                            if (hitbox != null)
                            {
                                //update hitbox position
                                Canvas.SetLeft(hitbox, (x *
Utilities.TileSize) - cameraCenter.X + GameCanvas.Width / 2); //sets the
rectangles
                                Canvas.SetTop(hitbox, (y * Utilities.TileSize)
- cameraCenter.Y + GameCanvas.Height / 2); //uses hitbox X and Y not x & y
incase hitboxes wrong -> for debuggin
                            }
                        }
                    }
                }
            }

            if (chunksLoaded)
            {
                tileRenderer.UpdateLoadedChunks(loadedChunks,
loadedTileImages);

               tileRenderer.InvalidateVisual();
           }

           ReturnHitboxesToPool();
       }

        //Pooling
        private systemShapes.Rectangle GetHitboxFromPool(int x, int y)
//returns a image control
        {
            if (TileDictionary.TileData[tileMapData[x, y]].IsSolid) //only
create hitboxes for solid tiles
            {
                if (hitboxMap[x, y] != null) //hitbox already exists
                {
                    return hitboxMap[x, y];
                }

               systemShapes.Rectangle hitbox;

               if (availableHitboxes.Count > 0) //reuse from pool
               {
                   hitbox = availableHitboxes.Dequeue();
               }
               else //no available hitboxes -> create new one
               {
                   hitbox = new systemShapes.Rectangle();
                   hitbox.Stroke = Brushes.Red;
                   hitbox.StrokeThickness = 1;
                   hitbox.Fill = Brushes.Transparent;
                   hitbox.Tag = "Hitbox";
                   hitbox.Width = Utilities.TileSize;
                   hitbox.Height = Utilities.TileSize;
                   Canvas.SetZIndex(hitbox, 8);
                   GameCanvas.Children.Add(hitbox);
               }

                hitbox.Visibility = showHitboxes ? Visibility.Visible :
Visibility.Collapsed;

                return hitbox;
           }
           return null;
       }

       private void ReturnHitboxesToPool()
       {
           foreach (var (x, y) in previousFrameActiveHitboxes)
           {
               if (!currentFrameActiveHitboxes.Contains((x, y)))
               {
                   systemShapes.Rectangle hitbox = hitboxMap[x, y];
                   if (hitbox != null)

                    {
                        hitbox.Visibility = Visibility.Collapsed;
                        availableHitboxes.Enqueue(hitbox);
                        hitboxMap[x, y] = null;
                    }
                }
            }

            previousFrameActiveHitboxes = currentFrameActiveHitboxes.ToList();
        }


        //hitboxes
        public void UpdatePlayerHitboxDisplay()
        {
            //hitbox
            playerHitbox = myPlayer.Hitbox; //get the player hitbox -> from
class
            Canvas.SetLeft(PlayerHitboxRect, myPlayer.playerX - cameraCenter.X
+ halfCameraWidth - playerHitbox.Width / 2); //update the rectangle's position
and size
            Canvas.SetTop(PlayerHitboxRect, myPlayer.playerY - cameraCenter.Y
+ halfCameraHeight - playerHitbox.Height / 2);

            PlayerHitboxRect.Width = playerHitbox.Width;
            PlayerHitboxRect.Height = playerHitbox.Height;
            Canvas.SetZIndex(PlayerHitboxRect, 10); //bring to front

            //centre hitbox
            playerHitboxCenter = myPlayer.playerHitboxCenter; //get the centre
of hitbox -> from class
            Canvas.SetLeft(PlayerHitboxCenterRect, myPlayer.playerX -
cameraCenter.X + halfCameraWidth);
            Canvas.SetTop(PlayerHitboxCenterRect, myPlayer.playerY -
cameraCenter.Y + halfCameraHeight);

            //update the rectangle's position and size
            PlayerHitboxCenterRect.Width = playerHitboxCenter.Width;
            PlayerHitboxCenterRect.Height = playerHitboxCenter.Height;
            Canvas.SetZIndex(PlayerHitboxCenterRect, 10); //bring to front
        }

        public void UpdateEnemyHitboxDisplay() //enemy hitboxes
        {
            foreach (var enemy in enemies)
            {
                Canvas.SetLeft(enemy.thisEnemiesHitboxRectangle, enemy.enemyX
- cameraCenter.X + halfCameraWidth - enemy.halfImageWidth); //update the
rectangle's position and size
                Canvas.SetTop(enemy.thisEnemiesHitboxRectangle, enemy.enemyY -
cameraCenter.Y + halfCameraHeight - enemy.halfImageHeight);

                //centre hitbox

                Canvas.SetLeft(enemy.thisEnemiesCenterHitboxRectangle,
enemy.enemyX - (enemy.enemyHitboxCenter.Width / 2) - cameraCenter.X +
halfCameraWidth);
                Canvas.SetTop(enemy.thisEnemiesCenterHitboxRectangle,
enemy.enemyY - (enemy.enemyHitboxCenter.Height / 2) - cameraCenter.Y +
halfCameraHeight);
            }
        }
        public void UpdateEyeBossHitboxDisplay() //eye boss hitboxes
        {
            //to set ofefest of utilties.tilesize * 1.1 in downawrds direction
            //get the current facing angle
            double angleDegrees =
currentEyeBoss.EyeBossRotationTransform.Angle;
            double angleRadians = angleDegrees * Math.PI / 180.0;

            double offsetX = 0;
            double offsetLY = Utilities.TileSize * 1.1;

            //rotate offset vector by current facing angle
            double offsetWorldX = offsetX * Math.Cos(angleRadians) - offsetLY
* Math.Sin(angleRadians);
            double offsetWorldY = offsetX * Math.Sin(angleRadians) + offsetLY
* Math.Cos(angleRadians);

            //position hitbox using rotated offset
            Canvas.SetLeft(currentEyeBoss.thisEyeBossHitboxRectangle,
currentEyeBoss.eyeBossX - cameraCenter.X + halfCameraWidth
                - currentEyeBoss.pivotRectX + offsetWorldX);

            Canvas.SetTop(currentEyeBoss.thisEyeBossHitboxRectangle,
currentEyeBoss.eyeBossY - cameraCenter.Y + halfCameraHeight
                - currentEyeBoss.pivotRectY + offsetWorldY);

            //centre hitbox
            Canvas.SetLeft(currentEyeBoss.thisEyeBossCenterHitboxRectangle,
currentEyeBoss.eyeBossX - (currentEyeBoss.eyeBossHitboxCenter.Width / 2) -
cameraCenter.X + halfCameraWidth);
            Canvas.SetTop(currentEyeBoss.thisEyeBossCenterHitboxRectangle,
currentEyeBoss.eyeBossY - (currentEyeBoss.eyeBossHitboxCenter.Height / 2) -
cameraCenter.Y + halfCameraHeight);
        }
        public void UpdateArrowHitboxDisplay()
        {
            foreach (var arrow in currentArrowList)
            {
                //hitbox
                Canvas.SetLeft(arrow.thisArrowsHitboxRectangle,
arrow.arrowPosX - cameraCenter.X + halfCameraWidth - (arrow.arrowWidth / 2));
//update the rectangle's position and size
                Canvas.SetTop(arrow.thisArrowsHitboxRectangle, arrow.arrowPosY
- cameraCenter.Y + halfCameraHeight - (arrow.arrowHeight / 2));


                //centre hitbox
                Canvas.SetLeft(arrow.thisArrowsCenterHitboxRectangle,
arrow.arrowPosX + (arrow.arrowHitboxCenter.Width / 2) - cameraCenter.X +
halfCameraWidth);
                Canvas.SetTop(arrow.thisArrowsCenterHitboxRectangle,
arrow.arrowPosY + (arrow.arrowHitboxCenter.Height / 2) - cameraCenter.Y +
halfCameraHeight);
            }
        }


        //toggle
        private void ToggleHitboxesVisibility()
        {
            for (int x = 0; x < hitboxMap.GetLength(0); x++)
            {
                for (int y = 0; y < hitboxMap.GetLength(1); y++)
                {
                    if (hitboxMap[x, y] != null) //check if hitbox exists
                    {
                        hitboxMap[x, y].Visibility = showHitboxes ?
Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            //update player hitboxes
            PlayerHitboxRect.Visibility = showHitboxes ? Visibility.Visible :
Visibility.Collapsed;
            PlayerHitboxCenterRect.Visibility = showHitboxes ?
Visibility.Visible : Visibility.Collapsed;
            ToolHitbox.Visibility = showHitboxes ? Visibility.Visible :
Visibility.Hidden;
            foreach (var enemy in enemies)
            {
                enemy.thisEnemiesHitboxRectangle.Visibility = showHitboxes ?
Visibility.Visible : Visibility.Collapsed;
                enemy.thisEnemiesCenterHitboxRectangle.Visibility =
showHitboxes ? Visibility.Visible : Visibility.Collapsed;
            }
            if (currentEyeBoss != null)
            {
                currentEyeBoss.thisEyeBossHitboxRectangle.Visibility =
showHitboxes ? Visibility.Visible : Visibility.Collapsed;
                currentEyeBoss.thisEyeBossCenterHitboxRectangle.Visibility =
showHitboxes ? Visibility.Visible : Visibility.Collapsed;
            }
        }

    }
}
