using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFTerraria
{
    public partial class MainWindow
    {
        private Stopwatch placeCooldownTimer; //timer for block placement
cooldown
        private const double PlaceCooldownTime = 150; //0.15 seconds -> 150ms
        private double accumulatedTimeInMs_Place = 0;

        //validate if you can do anything on the tile hovered over
        private (bool isValid, string actionType) ValidateInteraction()
//helper method for both UI and game code
        {
            if (!hoveredTile.HasValue || isInventoryOpen) { return (false,
null); } //if inventorys open or mouse not on tile, null values

            int selectedIndex = myInventory.SelectedInventoryIndex; //get the
index of the item currently selected
            if (selectedIndex == -1) { return (false, null); } //-1 means no
item selected CHECK

           var (tileX, tileY) = hoveredTile.Value;
           byte tileID = tileMapData[tileX, tileY]; //get tile of hovered
valie

           //calculate distance (scalar quantity) from player
           double playerTileX = myPlayer.playerX / Utilities.TileSize;
           double playerTileY = myPlayer.playerY / Utilities.TileSize;

            //straight line distance -> d = √((x2 - x1)² + (y2 - y1)²)
            double dx = tileX - playerTileX;
            double dy = tileY - playerTileY;
            double squaredDistance = dx * dx + dy * dy; //avoid using
Math.sqrt as proccessor intensive
            //double distance = Math.Sqrt(Math.Pow(tileX - playerTileX, 2) +
Math.Pow(tileY - playerTileY, 2));

            InventoryItem selectedItem = myInventory[selectedIndex]; //get
current selected item

           //placement check


            if (selectedItem?.Item?.ItemType == "Block" ||
selectedItem?.Item?.ItemType == "Crafting Station") //if block item selected,
and placing in a empty tile
            {
                //check if theres an adjacent tile
                bool hasAdjacent = false;
                if (tileID == 0 && selectedItem?.Item?.ItemType == "Block")
                {
                    if (tileX > 0 &&
!TileDictionary.TileData[tileMapData[tileX - 1, tileY]].IsWalkableThrough &&
TileDictionary.TileData[tileMapData[tileX - 1, tileY]].IsSolid) hasAdjacent =
true;
                    else if (tileX < mapX_Width - 1 &&
!TileDictionary.TileData[tileMapData[tileX + 1, tileY]].IsWalkableThrough &&
TileDictionary.TileData[tileMapData[tileX + 1, tileY]].IsSolid) hasAdjacent =
true;
                    else if (tileY > 0 &&
!TileDictionary.TileData[tileMapData[tileX, tileY - 1]].IsWalkableThrough &&
TileDictionary.TileData[tileMapData[tileX, tileY - 1]].IsSolid) hasAdjacent =
true;
                    else if (tileY < mapY_Height - 1 &&
!TileDictionary.TileData[tileMapData[tileX, tileY + 1]].IsWalkableThrough &&
TileDictionary.TileData[tileMapData[tileX, tileY + 1]].IsSolid) hasAdjacent =
true;
                }
                else if (tileID == 0)
                {
                    if (tileX < mapX_Width - 1 &&
!TileDictionary.TileData[tileMapData[tileX, tileY + 1]].IsWalkableThrough &&
TileDictionary.TileData[tileMapData[tileX, tileY + 1]].IsSolid) hasAdjacent =
true;
                }

                   try
                   {
                        if
(!TileDictionary.TileData[selectedItem.Item.CreateTile()].IsWalkableThrough)
//if placing a walkable through block soesnt matter if in enemy/player
                        {
                            bool enemyInside;
                            foreach (var enemy in enemies) //check if any
enemies inside the block
                            {
                                enemyInside = enemy.Hitbox.IntersectsWith(new
Rect(tileX * Utilities.TileSize + 0.05, tileY * Utilities.TileSize + 0.05,
Utilities.TileSize - 0.1, Utilities.TileSize - 0.1));
                                if (enemyInside) { return (false, "place"); }
//if enemy inside, cant place
                            }
                            bool playerInside =
myPlayer.Hitbox.IntersectsWith(new Rect(tileX * Utilities.TileSize + 0.05,


tileY * Utilities.TileSize + 0.05, Utilities.TileSize - 0.1,
Utilities.TileSize - 0.1)); //not inside the players hitbox
                            if (playerInside) { return (false, "place"); }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Error creating tile to place");
                    }

                bool isValid = hasAdjacent && squaredDistance <=
maxPlaceableDistance * maxPlaceableDistance; ///its an empty tile within
distance, and has an adjacent tile

                return (isValid, "place");
           }

            //check for valid mining (can update when other blocks and need
different tools) -> pickaxe
            if (selectedItem?.Item?.ItemType == "Tool" &&
selectedItem.Item.Name.Contains("Pickaxe")) //if tool item selected and name
has pickaxe
            {
                bool canMine = selectedItem.Item.MiningLevel >=
TileDictionary.TileData[tileID].TileLevel &&
TileDictionary.TileData[tileID].PickaxeBroken == true;
                bool isValid = squaredDistance <= maxMineableDistance *
maxMineableDistance && TileDictionary.TileData[tileID].IsSolid && canMine;
//and the tiles solid, within breaking distance
                return (isValid, "mine");
            }

            //for Axe
            if (selectedItem?.Item?.ItemType == "Tool" &&
selectedItem.Item.Name.Contains("Axe")) //if tool item selected and name has
pickaxe
            {
                bool canMine = selectedItem.Item.MiningLevel >=
TileDictionary.TileData[tileID].TileLevel &&
TileDictionary.TileData[tileID].PickaxeBroken == false;
                bool isValid = squaredDistance <= maxMineableDistance *
maxMineableDistance && TileDictionary.TileData[tileID].IsSolid && canMine;
//and the tiles solid, within breaking distance
                return (isValid, "mine");
            }
            return (false, null); //if none of these, just return null values
        }



        //interaction outline hover
        private SolidColorBrush lastStrokeColor = Brushes.Yellow;


        private SolidColorBrush lastFillColor = Brushes.Transparent;
        public void UpdateInteractionOutline()
        {
            if (!hoveredTile.HasValue || isInventoryOpen)
            {
                if (interactionOutline.Visibility != Visibility.Collapsed)
                {
                    interactionOutline.Visibility = Visibility.Collapsed;
                }
                return; //hovered tile collapse if it doesnt have value or
inventorys open
            }

            var (tileX, tileY) = hoveredTile.Value;

            //calculate screen position -> not relative to map, but the actual
screen
            double screenX = tileX * Utilities.TileSize - cameraCenter.X +
GameCanvas.Width / 2;
            double screenY = tileY * Utilities.TileSize - cameraCenter.Y +
GameCanvas.Height / 2;

            Canvas.SetLeft(interactionOutline, screenX); //place the outline
on the canvas
            Canvas.SetTop(interactionOutline, screenY);

           SolidColorBrush newStrokeColor = Brushes.Yellow; //default
           SolidColorBrush newFillColor = Brushes.Transparent; //default

            if (isLeftMouseHeldIn && activeMinings.TryGetValue((tileX, tileY),
out MiningProgress progress)) //if currently mining a block
            {
                double progressPercent = progress.CurrentProgress /
progress.RequiredTime; //gets percentage of progress
                newFillColor = new SolidColorBrush(Color.FromArgb((byte)(200 *
progressPercent), 255, 165, 0)); //becomes more solid as progress
                newStrokeColor = newFillColor;
            }
            else
            {
                //check if the placement/mining is valid only for ui
interaction
                var (isValid, actionType) = ValidateInteraction();
                if (isValid)
                {
                    newStrokeColor = Brushes.Lime; //set a green hover
outline, if valid
                }
                else if (actionType == "mine" || actionType == "place")
                {
                    newStrokeColor = Brushes.OrangeRed; //invalid spot and red
                }


              }

              //update only if the colors changed -> avoid unnecessary UI
updates
              if (newStrokeColor != lastStrokeColor)
              {
                  interactionOutline.Stroke = newStrokeColor;
                  lastStrokeColor = newStrokeColor;

                  if (newFillColor != lastFillColor)
                  {
                      interactionOutline.Fill = newFillColor;
                      lastFillColor = newFillColor;
                  }
              }

              //ensure visible
              if (interactionOutline.Visibility != Visibility.Visible)
              {
                  interactionOutline.Visibility = Visibility.Visible;
              }
          }



          //place and mine blocks
          private class MiningProgress
          {
              public (int, int) TilePosition { get; set; }
              public double CurrentProgress { get; set; }
              public double RequiredTime { get; set; }
          }

          private Dictionary<(int, int)?, MiningProgress> activeMinings;
          (int tileX, int tileY)? currentMiningTile = null;

          private void HandleBlockInteraction()
          {
              if (isInventoryOpen || !hoveredTile.HasValue) return;

              var (isValid, actionType) = ValidateInteraction();
              if (!isValid) { currentMiningTile = null; return; }

              var (tileX, tileY) = hoveredTile.Value;

            int selectedIndex = myInventory.SelectedInventoryIndex;
            InventoryItem selectedItem = myInventory[selectedIndex]; //isValid
true therefore, there is an item

              //placing logic
              if (actionType == "place")
              {
                  //time since last frame

                double timePassedInMs =
placeCooldownTimer.Elapsed.TotalMilliseconds;
                placeCooldownTimer.Restart();
                accumulatedTimeInMs_Place += timePassedInMs; //use incase 0.1
seconds doesnt pass before next frame

               //only place block if amount of time has passed
               if (accumulatedTimeInMs_Place >= PlaceCooldownTime)
               {
                   accumulatedTimeInMs_Place = 0;
                   PlaceBlock(selectedItem, (tileX, tileY));
               }
           }

            //mining logic
            else if (actionType == "mine")
            {
                //if switching to new tile reset progress on previous one
                if (currentMiningTile.HasValue && currentMiningTile.Value !=
(tileX, tileY)) //check hover still on same tile
                {
                    activeMinings.Remove(currentMiningTile.Value); //if not
remove from active minings

               }
               currentMiningTile = (tileX, tileY); //update currently mining
tile
               MineBlock(tileX, tileY, selectedItem);
           }
       }


        private void MineBlock(int tileX, int tileY, InventoryItem
selectedItem) //the tile you are mining
        {
            byte tileMiningID = tileMapData[tileX, tileY]; //the tile you are
mining
            var tool = selectedItem.Item; //the selected tool your using

            double miningTime =
(TileDictionary.TileData[tileMiningID].TileLevel * 1000) / tool.ToolPower;
//calculate time to mine, based on the toolpower

            if (!activeMinings.TryGetValue((tileX, tileY), out MiningProgress
progress)) //if not already in dictionary make new, make new miningprogress
and put it out
            {
                progress = new MiningProgress
                {
                    TilePosition = (tileX, tileY),
                    RequiredTime = miningTime
                };
                activeMinings[(tileX, tileY)] = progress;

           }

           progress.CurrentProgress += TargetFrameTimeMs; //add progress

           if (progress.CurrentProgress >= progress.RequiredTime)
           {
               //remove from active mining
               activeMinings.Remove((tileX, tileY));

                //mine the tile
                SpawnMiningParticles(tileX, tileY,
TileDictionary.TileData[tileMapData[tileX, tileY]].Colour, 0.65, 3);
                MineTile(tileMiningID, tileX, tileY);
                //updates interaction outline in minetile
            }
        }
        private bool NotMiningAllAbove = true;
        private void MineTile(byte tileMiningID, int tileX, int tileY)
        {
            string tileName = TileDictionary.TileData[tileMiningID].TileName;
            if (tileMiningID == 191 && NotMiningAllAbove) //special case ->
living wood
            {
                NotMiningAllAbove = false;
                MineAllTilesAbove(tileMiningID, tileX, tileY);
                CheckIfMinedTopTree(tileX, tileY);                   Recursion
                return; //prevents mining the same tile twice
            }
            else if (tileMapData[tileX, tileY - 1] == 191 &&
NotMiningAllAbove)
            {
                NotMiningAllAbove = false;
                MineTile(tileMiningID, tileX, tileY);
                MineAllTilesAbove(191, tileX, tileY - 1);
                return; //also prevents mining the same tile twice
            }

            if (TileDictionary.nameToItem.ContainsKey(tileName)) //add 1 to
item based on tile type
            {
                Item droppedItem = TileDictionary.nameToItem[tileName];
                myInventory.AddItem(droppedItem, 1); //add mined item to
inventory
            }
            else
            {
                //no item to drop
            }

           loadedTileImages[tileX, tileY] = null;   //create a new empty tile
           tileMapData[tileX, tileY] = 0;
           UpdateTileConnections(tileX, tileY);


             currentMiningTile = null;
             //Update new connections if changes
             UpdateVisibleTiles(); //update the tiles on screen

             CheckIfPlayerStillGrounded(); //check the players still on the
ground
             foreach (var enemy in enemies)
             {
                 enemy.CheckIfEnemyStillGrounded(this);
             }
             UpdateInteractionOutline();//check if valid after

         }
         private void MineAllTilesAbove(byte MiningTileID, int tileX, int
tileY)
         {
             while (true)
             {
                 if (tileMapData[tileX, tileY] == MiningTileID)
                 {
                     CheckIfMinedTopTree(tileX, tileY);
                     MineTile(MiningTileID, tileX, tileY);
                     SpawnMiningParticles(tileX, tileY, Colors.SaddleBrown,
1.1, 6);
                    tileY -= 1;
                }
                else
                {
                    NotMiningAllAbove = true;
                    break;
                }
             }
         }
         private void CheckIfMinedTopTree(int tileX, int tileY)
         {
             if (TreeTop_ToLeafs.ContainsKey((tileX, tileY)))
             {
                 //copy list and remove from dictionary to prevent recuring
                 var leaves = TreeTop_ToLeafs[(tileX, tileY)].ToList();
                 TreeTop_ToLeafs.Remove((tileX, tileY));

                foreach (var tile in leaves)
                {
                    if (tileMapData[tile.Item1, tile.Item2] == 192) //if leaf
block
                    {
                        MineTile(192, tile.Item1, tile.Item2); //is empty
                        SpawnMiningParticles(tile.Item1, tile.Item2,
Colors.GreenYellow, 1.1, 6);
                    }
                }
            }
        }

        private void PlaceBlock(InventoryItem selectedItem, (int tileX, int
tileY) tilePosition)
        {
            int tileX = tilePosition.tileX;
            int tileY = tilePosition.tileY;
            //create new tile based on item type
            if (selectedItem?.Item?.CreateTile() is byte tileDataID)
            {
                tileMapData[tileX, tileY] = tileDataID;
                UpdateTileConnections(tileX, tileY); //Update new connections
if changes


               selectedItem.Quantity--; //take away one from the items
quantity
               UpdateVisibleTiles(); //update the tiles on screen
               UpdateInteractionOutline();//check if valid after
           }
           else { Console.WriteLine("Error making tile to place"); }
       }


       private void UpdateTileConnections(int x, int y)
       {
           //update center tile
           UpdateSingleTile(x, y);

           //update neighbors
           UpdateSingleTile(x - 1, y);
           UpdateSingleTile(x + 1, y);
           UpdateSingleTile(x, y - 1);
           UpdateSingleTile(x, y + 1);

           tileRenderer.InvalidateVisual(); //update screen
       }


        private void UpdateSingleTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= mapX_Width || y >= mapY_Height) {
return; }

           UpdateSingleConnection(x, y);

            BitmapSource tileImage = null;
            if (tileMapData[x, y] != 0)
            {
                tileImage = TileDictionary.GetTileImage(tileMapData[x, y],
connectionMap[x, y]);
            }

            //update both cache and renderer

           loadedTileImages[x, y] = tileImage;
           tileRenderer.UpdateSingleTile(x, y, tileImage);
       }


        public void UpdateSingleConnection(int x, int y) //update connection
array based on tiles beside
        {
            if (x < 0 || x >= mapX_Width || y < 0 || y >= mapY_Height)
            {
                Console.WriteLine("Connection check out of bounds");
                return;
            }

            if (tileMapData[x, y] == 0 ||
!TileDictionary.TileData.TryGetValue(tileMapData[x, y], out var tileData) )
            {
                connectionMap[x, y] = "Null_Connect"; //empty or non solid
tiles
                return;
            }

            bool left, right, up, down;
            if (!TileDictionary.TileData[tileMapData[x, y]].IsWalkableThrough)
//if not wlakable do this
            {
                //check neighbors to see connection type
                left = x > 0 && tileMapData[x - 1, y] != 0 &&
TileDictionary.TileData[tileMapData[x - 1, y]].IsSolid &&
!TileDictionary.TileData[tileMapData[x - 1, y]].IsWalkableThrough ;
                right = x < mapX_Width - 1 && tileMapData[x + 1, y] != 0 &&
TileDictionary.TileData[tileMapData[x + 1, y]].IsSolid &&
!TileDictionary.TileData[tileMapData[x + 1, y]].IsWalkableThrough;
                up = y > 0 && tileMapData[x, y - 1] != 0 &&
TileDictionary.TileData[tileMapData[x, y - 1]].IsSolid &&
(!TileDictionary.TileData[tileMapData[x, y - 1]].IsWalkableThrough ||
tileMapData[x,y - 1] == 191 || TileDictionary.TileData[tileMapData[x, y-
1]].IsWorkStation); //unless living wood or workstation-> is a connection
upwards
                down = y < mapY_Height - 1 && tileMapData[x, y + 1] != 0 &&
TileDictionary.TileData[tileMapData[x, y + 1]].IsSolid &&
!TileDictionary.TileData[tileMapData[x, y + 1]].IsWalkableThrough;

            }
            else if (tileMapData[x,y] == 191) //living wood -> special case
            {
                //check neighbors to see connection type
                left = x > 0 && tileMapData[x - 1, y] != 0 &&
TileDictionary.TileData[tileMapData[x - 1, y]].IsWalkableThrough &&
TileDictionary.TileData[tileMapData[x - 1, y]].IsSolid && tileMapData[x,y] ==
tileMapData[x - 1, y];



                right = x < mapX_Width - 1 && tileMapData[x + 1, y] != 0 &&
TileDictionary.TileData[tileMapData[x + 1, y]].IsWalkableThrough &&
TileDictionary.TileData[tileMapData[x + 1, y]].IsSolid && tileMapData[x, y] ==
tileMapData[x + 1, y];
                up = y > 0 && tileMapData[x, y - 1] != 0 &&
TileDictionary.TileData[tileMapData[x, y - 1]].IsWalkableThrough &&
TileDictionary.TileData[tileMapData[x, y - 1]].IsSolid;
                down = y < mapY_Height - 1 && tileMapData[x, y + 1] != 0 &&
(TileDictionary.TileData[tileMapData[x, y + 1]].IsWalkableThrough &&
TileDictionary.TileData[tileMapData[x, y + 1]].IsSolid) || (tileMapData[x, y +
1] == 1); //unless dirt -> is a connection downwards
            }
            else //somwthing walkable through but not solid
            {
                //check neighbors to see connection type
                left = x > 0 && tileMapData[x - 1, y] != 0 &&
TileDictionary.TileData[tileMapData[x - 1, y]].IsWalkableThrough &&
TileDictionary.TileData[tileMapData[x - 1, y]].IsSolid;
                right = x < mapX_Width - 1 && tileMapData[x + 1, y] != 0 &&
TileDictionary.TileData[tileMapData[x + 1, y]].IsWalkableThrough &&
TileDictionary.TileData[tileMapData[x + 1, y]].IsSolid;
                up = y > 0 && tileMapData[x, y - 1] != 0 &&
TileDictionary.TileData[tileMapData[x, y - 1]].IsWalkableThrough &&
TileDictionary.TileData[tileMapData[x, y - 1]].IsSolid;
                down = y < mapY_Height - 1 && tileMapData[x, y + 1] != 0 &&
(TileDictionary.TileData[tileMapData[x, y + 1]].IsWalkableThrough &&
TileDictionary.TileData[tileMapData[x, y + 1]].IsSolid) || (tileMapData[x, y +
1] == 1); //unless dirt -> is a connection downwards

           }

            //for edges of map
            if (x == 0) left = true;
            if (x == mapX_Width - 1) right = true;
            if (y == 0) up = true;
            if (y == mapY_Height - 1) down = true;

            //determine connection type
            if (left && right && up && down) connectionMap[x, y] =
"Full_Connect";

            else if (up && right && down) connectionMap[x, y] = "URD_Connect";
            else if (left && right && down) connectionMap[x, y] =
"LRD_Connect";
            else if (left && up && down) connectionMap[x, y] = "LUD_Connect";
            else if (left && up && right) connectionMap[x, y] = "LUR_Connect";

            else if (left && up) connectionMap[x, y] = "LU_Connect";
            else if (left && right) connectionMap[x, y] = "LR_Connect";
            else if (left && down) connectionMap[x, y] = "LD_Connect";
            else if (up && right) connectionMap[x, y] = "UR_Connect";
            else if (up && down) connectionMap[x, y] = "UD_Connect";

            else if (right && down) connectionMap[x, y] = "RD_Connect";

            else if (left) connectionMap[x, y] = "L_Connect";
            else if (up) connectionMap[x, y] = "U_Connect";
            else if (right) connectionMap[x, y] = "R_Connect";
            else if (down) connectionMap[x, y] = "D_Connect";

            else connectionMap[x, y] = "Null_Connect";
       }


        private readonly List<MiningParticle> activeMiningParticles = new
List<MiningParticle>();
        private class MiningParticle
        {
            public Ellipse Shape; //circle shape
            public Point Position;
            public Vector Velocity;
            public double Lifetime;
        }
        private void SpawnMiningParticles(int tileX, int tileY, Color color,
double timeOfParticles, int count)
        {
            Random rand = new Random();
            for (int i = 0; i < count; i++)
            {
                double size = rand.NextDouble() * 3 + 2; // 2 to 5 pixel
particles
                Ellipse particle = new Ellipse //new circle
                {
                    Width = size,
                    Height = size,
                    Fill = new SolidColorBrush(color),
                    Opacity = 1.0
                };

                double worldX = tileX * Utilities.TileSize +
Utilities.TileSize / 2; //center of tile
                double worldY = tileY * Utilities.TileSize +
Utilities.TileSize / 2;

                double canvasX = worldX - cameraCenter.X + GameCanvas.Width /
2; //convert to canvas position
                double canvasY = worldY - cameraCenter.Y + GameCanvas.Height /
2;

               Canvas.SetLeft(particle, canvasX);
               Canvas.SetTop(particle, canvasY);
               GameCanvas.Children.Add(particle);
               Canvas.SetZIndex(particle, 6); //above tiles, below player

                //random velocity


                Vector velocity = new Vector(rand.NextDouble() * 2 - 1,
rand.NextDouble() * 2 - 1);
                velocity.Normalize(); //only direction (length 1)
                velocity *= rand.NextDouble() * 50 + 25; //pixels per second

                  activeMiningParticles.Add(new MiningParticle
                  {
                      Shape = particle,
                      Position = new Point(worldX, worldY),
                      Velocity = velocity,
                      Lifetime = timeOfParticles // seconds
                  });
              }
          }
          private void UpdateMiningParticles()
          {
              double deltaTime = TargetFrameTimeMs / 1000.0; //convert fps ms ->
seconds

            for (int i = activeMiningParticles.Count - 1; i >= 0; i--) //for
loop backwards as removing from list
            {
                var thisParticle = activeMiningParticles[i];
                thisParticle.Lifetime -= deltaTime;

                  //move
                  thisParticle.Position += thisParticle.Velocity * deltaTime;

                  //fade
                  thisParticle.Shape.Opacity = thisParticle.Lifetime / 0.5;

                //update screen position
                double screenX = thisParticle.Position.X - cameraCenter.X +
GameCanvas.Width / 2;
                double screenY = thisParticle.Position.Y - cameraCenter.Y +
GameCanvas.Height / 2;
                Canvas.SetLeft(thisParticle.Shape, screenX);
                Canvas.SetTop(thisParticle.Shape, screenY);

                  //remove finsihed particles
                  if (thisParticle.Lifetime <= 0)
                  {
                      GameCanvas.Children.Remove(thisParticle.Shape);
                      activeMiningParticles.RemoveAt(i);
                  }
              }
          }
    }
}
