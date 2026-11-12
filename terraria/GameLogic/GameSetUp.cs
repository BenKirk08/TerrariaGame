using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPFTerraria.View;
using systemShapes = System.Windows.Shapes;

namespace WPFTerraria
{
    public partial class MainWindow
    {
        TileRenderer tileRenderer;
        private void SetUpNewGame()
        {
            //Load enemy Assets
            _ = EnemyAssetLoader.GetAnimation("Zombie1"); //random call to
load assets

              InitiliseCanvasVisibilty();

              mapX_Width = newMapWidth;
              mapY_Height = newMapHeight;
              tileMapData = new byte[mapX_Width, mapY_Height];
              connectionMap = new string[mapX_Width, mapY_Height];
              loadedTileImages = new BitmapSource[mapX_Width, mapY_Height];
              hitboxMap = new systemShapes.Rectangle[mapX_Width, mapY_Height];
              heights = new int[mapX_Width];

              InitiliseVariables();
              dayClock = DateTime.ParseExact("08:30", "HH:mm", null);
              InitliseCameraVariables();

              InitiliseFPS();
              InitiliseDayClock();

              GenerateMap(); //generate all tiles
              InitiliseParallax(true);

              tileRenderer = new TileRenderer(mapX_Width, mapY_Height,
chunkSize);
              GameCanvas.Children.Add(tileRenderer);
              Canvas.SetZIndex(tileRenderer, 5); //behind everything else

              //initilise inventory
              myInventory = new Inventory();
              myInventoryHUDDisplay.Inventory = myInventory;
              myFullInventoryDisplay.Inventory = myInventory;

              InitilialInventory();

              InitiliseHUDInventoryCanvas();
              InitilisePauseMenu();

            //player
            //initialize player position (center of the map in pixels)
            (myPlayer.playerX, myPlayer.playerY) =
InitialisePlayerPos((mapX_Width / 2), 100);
            InitilisePlayerImage(); //initlise the players image, w/position

              //sets hitbox visibilty to whatever the variable showhitboxes is
              ToggleHitboxesVisibility();

              InitiliseMouseControls(); //for mouse movement & buttons


              AddEssentialUIElements();
              InitiliseCraftingMenu();
        }
        private void LoadUpPreviousGameVariables()
        {
            InitiliseVariables();
            //initilise inventory
            myInventory = new Inventory();
            myInventoryHUDDisplay.Inventory = myInventory;
            myFullInventoryDisplay.Inventory = myInventory;
        }
        private void LoadUpPreviousGameState()
        {
            //Load enemy Assets
            _ = EnemyAssetLoader.GetAnimation("Zombie1"); //random call to
load assets

              InitiliseCanvasVisibilty();

              InitliseCameraVariables();

              InitiliseFPS();
              InitiliseDayClock();

              InitiliseParallax(false);

              tileRenderer = new TileRenderer(mapX_Width, mapY_Height,
chunkSize);
              GameCanvas.Children.Add(tileRenderer);
              Canvas.SetZIndex(tileRenderer, 5); //behind everything else

              InitiliseHUDInventoryCanvas();
              InitilisePauseMenu();

              InitilisePlayerImage(); //initlise the players image, w/position


           //sets hitbox visibilty to whatever the variable showhitboxes is
           ToggleHitboxesVisibility();

           InitiliseMouseControls(); //for mouse movement & buttons
           AddEssentialUIElements();
           InitiliseCraftingMenu();

           for (int x = 0; x < mapX_Width; x++)
           {
               for (int y = 0; y < mapY_Height; y++)
               {
                   UpdateSingleConnection(x, y);
               }
           }
       }


        //intilise Positions
        private (double, double) InitialisePlayerPos(int CenterXSpawn, int
SearchRadius)
        {
            double posX;
            double posY;
            try
            {
                //search from top (y=0) downward (y increases downward)
                for (int y = 0; y < mapY_Height; y++)
                {
                    //check columns around center
                    for (int xOffset = 0; xOffset <= SearchRadius; xOffset++)
                    {
                        //dheck both left and right of center
                        foreach (int x in new[] { CenterXSpawn + xOffset,
CenterXSpawn - xOffset })
                        {
                            if (x < 0 || x + 1 >= mapX_Width) continue; //stay
in map bounds

                           //check 3x2 empty space for player body
                           bool hasAirSpace =
                               (y + 2 < mapY_Height) && // Ensure y+2 is
inside the map
                               (x + 1 < mapX_Width) &&   // Ensure x+1 is
inside the map
                               tileMapData[x, y] == 0 &&
                               tileMapData[x, y + 1] == 0 &&
                               tileMapData[x, y + 2] == 0 &&
                               tileMapData[x + 1, y] == 0 &&
                               tileMapData[x + 1, y + 1] == 0 &&
                               tileMapData[x + 1, y + 2] == 0;




                            //check solid ground below (Y+3) is first solid
block
                            bool hasSolidGround =
                                y + 3 < mapY_Height && x + 1 < mapX_Width
&&//ensure we dont go out map bounds
                                TileDictionary.TileData[tileMapData[x, y +
3]].IsSolid && !TileDictionary.TileData[tileMapData[x, y +
3]].IsWalkableThrough &&
                                TileDictionary.TileData[tileMapData[x + 1, y +
3]].IsSolid && !TileDictionary.TileData[tileMapData[x + 1, y +
3]].IsWalkableThrough &&
                                tileMapData[x + 1, y + 3] == 1 &&
tileMapData[x, y + 3] == 1; //and on dirt

                           if (hasAirSpace && hasSolidGround)
                           {
                               //position player at of the 3x2 air space
                               posX = (x + 1) * Utilities.TileSize;
                               posY = (y + 1.5) * Utilities.TileSize; //stand
on y+2
                                return (posX, posY);

                           }
                       }
                   }
               }
           }
           catch { }
           Console.WriteLine("Error in placing player");
           return (0, 0); //error
        }
        private (double, double) InitialiseEnemyPos(int CenterXSpawn, int
SearchRadius, int playerCurrentY)
        {
            double posX;
            double posY;
            int searchY = 28; //search 28 up and down from current y

            // Search y positions up and down from playerCurrentY
            // Check columns around center
            try
            {
                for (int xOffset = 0; xOffset <= SearchRadius; xOffset++)
                {
                    foreach (int x in new[] { CenterXSpawn + xOffset,
CenterXSpawn - xOffset })
                    {
                        if (x < 0 || x + 1 >= mapX_Width) continue; // stay in
map bounds

                       for (int yOffset = 0; yOffset <= searchY; yOffset++)
                       {

                            foreach (int y in new[] { playerCurrentY +
yOffset, playerCurrentY - yOffset })
                            {
                                if (y < 0 || y + 2 >= mapY_Height) continue;
// stay in map bounds

                                // Check 3x2 empty space for player body
                                bool hasAirSpace =
                                    (y + 2 < mapY_Height) &&
                                    (x + 1 < mapX_Width) &&
                                    (x - 1 > 0) &&
                                    (y - 2 > 0) &&
                                    (tileMapData[x, y] == 0 ||
TileDictionary.TileData[tileMapData[x, y]].IsWalkableThrough) &&
                                    (tileMapData[x, y + 1] == 0 ||
TileDictionary.TileData[tileMapData[x, y + 1]].IsWalkableThrough) &&
                                    (tileMapData[x, y + 2] == 0 ||
TileDictionary.TileData[tileMapData[x, y + 2]].IsWalkableThrough) &&
                                    (tileMapData[x + 1, y] == 0 ||
TileDictionary.TileData[tileMapData[x + 1, y]].IsWalkableThrough) &&
                                    (tileMapData[x + 1, y + 1] == 0 ||
TileDictionary.TileData[tileMapData[x + 1, y + 1]].IsWalkableThrough) &&
                                    (tileMapData[x + 1, y + 2] == 0 ||
TileDictionary.TileData[tileMapData[x + 1, y + 2]].IsWalkableThrough) &&
                                    (tileMapData[x, y - 1] == 0 ||
TileDictionary.TileData[tileMapData[x, y - 1]].IsWalkableThrough) &&
                                    (tileMapData[x + 1, y - 1] == 0 ||
TileDictionary.TileData[tileMapData[x + 1, y - 1]].IsWalkableThrough) &&
                                    (tileMapData[x, y - 2] == 0 ||
TileDictionary.TileData[tileMapData[x, y - 2]].IsWalkableThrough) &&
                                    (tileMapData[x + 1, y - 2] == 0 ||
TileDictionary.TileData[tileMapData[x + 1, y - 2]].IsWalkableThrough);


                                // Check solid ground below (Y+3) is first
solid block
                                bool hasSolidGround =
                                    y + 3 < mapY_Height && x + 1 < mapX_Width
&&
                                    (TileDictionary.TileData[tileMapData[x, y
+ 3]].IsSolid && !TileDictionary.TileData[tileMapData[x, y +
3]].IsWalkableThrough) ||
                                    (TileDictionary.TileData[tileMapData[x +
1, y + 3]].IsSolid && !TileDictionary.TileData[tileMapData[x + 1, y +
3]].IsWalkableThrough); //&&

//tileMapData[x + 1, y + 3] == 1 && tileMapData[x, y + 3] == 1;

                               if (hasAirSpace && hasSolidGround)
                               {
                                   posX = (x + 1) * Utilities.TileSize;
                                   posY = (y + 1) * Utilities.TileSize;

                            return (posX, posY);

                       }
                   }
               }
           }
       }
    }
    catch { }
    Console.WriteLine("Error in placing Enemy");
    return (0, 0); // error
}


//start -> initilising
private void InitiliseCanvasVisibilty()
{
    GameCanvas.Visibility = Visibility.Visible;
    FullInventoryCanvas.Visibility = Visibility.Visible;
    InventoryHudCanvas.Visibility = Visibility.Visible;
    PauseMenuCanvas.Visibility = Visibility.Visible;
    HealthTimerCanvas.Visibility = Visibility.Collapsed;
}
private void AddEssentialUIElements()
{
    //add FPS textblock
    if (!GameCanvas.Children.Contains(fpsTextBlock))
    {
        GameCanvas.Children.Add(fpsTextBlock);
    }

    //addtimetextblock
    if (!GameCanvas.Children.Contains(timeTextBlock))
    {
        GameCanvas.Children.Add(timeTextBlock);
    }

    //add player hitbox rectangles
    if (!GameCanvas.Children.Contains(PlayerHitboxRect))
    {
        GameCanvas.Children.Add(PlayerHitboxRect);
    }

    if (!GameCanvas.Children.Contains(PlayerHitboxCenterRect))
    {
        GameCanvas.Children.Add(PlayerHitboxCenterRect);
    }
    if (!GameCanvas.Children.Contains(ToolHitbox))
    {
        GameCanvas.Children.Add(ToolHitbox);
    }
}
private void InitiliseHUDInventoryCanvas()

       {
           //inventory hud
           InventoryHudCanvas.Width = CameraWidth * Utilities.TileSize;
           InventoryHudCanvas.Height = CameraHeight * Utilities.TileSize;

           InventoryHudCanvas.Children.Add(myInventoryHUDDisplay);

           Canvas.SetLeft(InventoryHudCanvas, 0); //set gamewindow canvas
           Canvas.SetTop(InventoryHudCanvas, 0);

            Canvas.SetLeft(myInventoryHUDDisplay, 10); //set the hud display
user control
            Canvas.SetTop(myInventoryHUDDisplay, 10);

            InventoryHudCanvas.Visibility = isInventoryOpen ?
Visibility.Collapsed : Visibility.Visible; //set to collapsed if full
inventory open

           //full inventory
           FullInventoryCanvas.Width = CameraWidth * Utilities.TileSize;
           FullInventoryCanvas.Height = CameraHeight * Utilities.TileSize;

           FullInventoryCanvas.Children.Add(myFullInventoryDisplay);

           Canvas.SetLeft(FullInventoryCanvas, 0);
           Canvas.SetTop(FullInventoryCanvas, 0);

            Canvas.SetLeft(myFullInventoryDisplay, myInventory.SlotImageSize);
//note uses canvas that element is inside
            Canvas.SetTop(myFullInventoryDisplay,70);

            FullInventoryCanvas.Visibility = isInventoryOpen ?
Visibility.Visible : Visibility.Collapsed;

           //health bar
           HealthBarCanvas.Width = CameraWidth * Utilities.TileSize;
           HealthBarCanvas.Height = CameraHeight * Utilities.TileSize;

           HealthBarCanvas.Children.Add(myPlayerHealthBar);

           Canvas.SetLeft(HealthBarCanvas, 0);
           Canvas.SetTop(HealthBarCanvas, 0);

            Canvas.SetLeft(myPlayerHealthBar, CameraWidth * Utilities.TileSize
/ 2 - myPlayerHealthBar.HealthBarWidth / 2 + 50); //note uses canvas that
element is inside
            Canvas.SetTop(myPlayerHealthBar, 20);

           //healing timer
           HealthTimerCanvas.Width = CameraWidth * Utilities.TileSize;
           HealthTimerCanvas.Height = CameraHeight * Utilities.TileSize;

           HealthTimerCanvas.Children.Add(myHealingTimer);

           Canvas.SetLeft(HealthTimerCanvas, 0);
           Canvas.SetTop(HealthTimerCanvas, 0);

            Canvas.SetLeft(myHealingTimer, 10); //note uses canvas that
element is inside
            Canvas.SetTop(myHealingTimer, 10 +
myInventoryHUDDisplay.Inventory.SlotImageSize + 5);

        }
        private void InitilialInventory()
        {
            //initilise whats in inventory
            myInventory[0] = new InventoryItem(myInventory) { Item = new
CopperBroadswordItem(), Quantity = 1 };
            myInventory[1] = new InventoryItem(myInventory) { Item = new
CopperPickaxeItem(), Quantity = 1 };
            myInventory[2] = new InventoryItem(myInventory) { Item = new
CopperAxeItem(), Quantity = 1 };
        }
        private void InitiliseCraftingMenu()
        {
            if (myCraftingMenu != null) return;

           myCraftingMenu = new CraftingMenuDisplay
           {
               Inventory = myInventory
           };


            double left = myFullInventoryDisplay.FullInventoryDisplayWidth +
myInventory.SlotImageSize * 2;
            double top = 70;

           Canvas.SetLeft(myCraftingMenu, left);
           Canvas.SetTop(myCraftingMenu, top);

           FullInventoryCanvas.Children.Add(myCraftingMenu);
           Canvas.SetZIndex(myCraftingMenu, 12);
           myCraftingMenu.Visibility = Visibility.Collapsed;
           myCraftingMenu.RefreshList();

       }
       private void InitilisePauseMenu()
       {
           PauseMenuCanvas.Width = CameraWidth * Utilities.TileSize;
           PauseMenuCanvas.Height = CameraHeight * Utilities.TileSize;

           PauseMenuCanvas.Children.Add(myPauseMenu);

           Canvas.SetLeft(PauseMenuCanvas, 0);
           Canvas.SetTop(PauseMenuCanvas, 0);


            Canvas.SetLeft(myPauseMenu, CameraWidth * Utilities.TileSize / 2 -
myPauseMenu.PauseMenuDisplayWidth / 2);
            Canvas.SetTop(myPauseMenu, CameraHeight * Utilities.TileSize / 2 -
myPauseMenu.PauseMenuDisplayHeight / 2);

            PauseMenuCanvas.Visibility = isPaused ? Visibility.Visible :
Visibility.Collapsed;
        }
        private void InitilisePlayerImage()
        {
            //add player
            xTransformOrigin = (myPlayer.ImageResizedWidth -
myPlayer.RightPadding) / myPlayer.ImageResizedWidth - myPlayer.HitboxWidth / 4
/ myPlayer.ImageResizedWidth; //will flip in accordance to the centre of the
hitbox and image
            playerImage = new Image
            {
                Source = myPlayer.GetPlayerImage($"{playerGender}Standing"),
//get the player image -> initilally set to still
                Width = myPlayer.ImageResizedWidth,
                Height = myPlayer.ImageResizedHeight,
                RenderTransformOrigin = new Point(1 - xTransformOrigin, 0.5)
            };
            Canvas.SetZIndex(playerImage, 10);
            GameCanvas.Children.Add(playerImage);

            //add arm
            playerImageArm = new Image
            {
                Source = myPlayer.GetPlayerImage($"{playerGender}Arm"),
                Width = myPlayer.ImageResizedWidth, //same as player as pixels
same size {resolution} js not actual picture
                Height = myPlayer.ImageResizedHeight,
                Visibility = Visibility.Hidden,
                RenderTransformOrigin = new Point(1 - xTransformOrigin - 0.03,
0.5) //flips at same point as player
            };
            Canvas.SetZIndex(playerImageArm, 12);
            armRotateTransform = new RotateTransform(0);
            playerImageArm.RenderTransform = armRotateTransform;
            GameCanvas.Children.Add(playerImageArm);


            //add swinging item
            swingItemImage = new Image
            {
                RenderTransformOrigin = new Point(1 - xTransformOrigin - 0.03,
0.5), //flips at same point as player
                Visibility = Visibility.Hidden
            };
            Canvas.SetZIndex(swingItemImage, 11);
            itemRotateTransform = new RotateTransform(0);
            swingItemImage.RenderTransform = itemRotateTransform;

           GameCanvas.Children.Add(swingItemImage);

           ToolHitbox.Width = 22;
           ToolHitbox.Height = 65;
           toolHitboxTransform = new RotateTransform(0);
           ToolHitbox.RenderTransform = toolHitboxTransform;
           ToolHitbox.Visibility = Visibility.Hidden;

           SunImage = new Image
           {

                Source =
Utilities.LoadImage((Path.Combine(Utilities.GetSolutionDirectory(),
"AllGameAssets", "Content", "Backgrounds", "Sun.png"))),
                Width = 130,
                Height = 130,
                Stretch = Stretch.Fill
            };
            Canvas.SetZIndex(SunImage, 0);
            GameCanvas.Children.Add(SunImage);

            MoonImage = new Image
            {
                Source =
Utilities.LoadImage((Path.Combine(Utilities.GetSolutionDirectory(),
"AllGameAssets", "Content", "Backgrounds", "Moon.png"))),
                Width = 42,
                Height = 42,
                Stretch = Stretch.Fill
            };
            Canvas.SetZIndex(MoonImage, 0);
            GameCanvas.Children.Add(MoonImage);
        }
        private void InitiliseVariables()
        {
            //classes

           frameStopwatch = new Stopwatch();
           fpsStopwatch = new Stopwatch();
           dayClockStopwatch = new Stopwatch();
           animationStopwatch = new Stopwatch();
           jumpingAnimationStopwatch = new Stopwatch();
           placeCooldownTimer = new Stopwatch();
           ZombieSpawnTimer = new Stopwatch();
           playersBeenHitStopwatch = new Stopwatch();
           RegainPlayerHealth = new Stopwatch();
           PotionUsedLast = new Stopwatch();
           PlayerRegainHealthNow = new Stopwatch();
           shootCooldownTimer = new Stopwatch();
           deathCountdownTimer = new Stopwatch();
           RegainPlayerHealth.Start();
           ZombieSpawnTimer.Start();
           playersBeenHitStopwatch.Start();

   PotionUsedLast.Start();
   shootCooldownTimer.Start();


   myInventoryHUDDisplay = new InventoryHUDDisplay();
   myFullInventoryDisplay = new FullInventoryDisplay();
   myPlayerHealthBar = new PlayerHealthBar();
   myPlayer = new Player();
   myHealingTimer = new HealingTimer();

   currentFrameActiveKeys = new HashSet<string>();
   availableHitboxes = new Queue<systemShapes.Rectangle>();
   pressedKeys = new HashSet<Key>();
   loadedChunks = new HashSet<(int, int)>();

   activeMinings = new Dictionary<(int, int)?, MiningProgress>();

   myPauseMenu = new PauseMenu();

   //things that may change -> for when returning and coming back
   isGrounded = false;
   isSwinging = false;
   isArmVisibile = false;
   currentArmRotation = 0;
   angleRad = 0;
   distanceFallen = 0;
   KnockbackDirection = 0;
   EnemyAmount = 0;
   swingingForward = true;
   ZombieSpawnTimer_WasPaused = false;
   ApplyingPlayerKnockback = false;
   showHitboxes = false;
   hitboxesToggledOnceAlready = false;
   facingRight = true;
   aimLocksFacing = false;
   isInventoryOpen = false;
   inventoryToggledOnceAlready = false;
   itemslotToggledOnceAlready = false;
   hoveredTile = null;
   isLeftMouseHeldIn = false;
   mouseMoved = false;
   playerMoved = false;
   isPaused = false;
   PauseMenuToggledOnceAlready = false;
   previousPlayerX = -1; //-1 so will do once
   previousPlayerY = -1;

   minHeight = mapY_Height * 0.7;
   maxDiffernece = 90;
}
private void InitliseCameraVariables()
{

           //for camera settings
           halfCameraWidth = (CameraWidth * Utilities.TileSize) / 2;
           halfCameraHeight = (CameraHeight * Utilities.TileSize) / 2;

            minCameraX = (CameraWidth * Utilities.TileSize) / 2;
            maxCameraX = (mapX_Width * Utilities.TileSize) - (CameraWidth *
Utilities.TileSize) / 2;
            minCameraY = (CameraHeight * Utilities.TileSize) / 2;
            maxCameraY = (mapY_Height * Utilities.TileSize) - (CameraHeight *
Utilities.TileSize) / 2;
        }
        private void InitiliseMouseControls()
        {
            //initialize interaction outline -> shows when mouse hovers over
            interactionOutline = new systemShapes.Rectangle
            {
                Stroke = Brushes.Yellow,
                StrokeThickness = 2,
                Fill = Brushes.Transparent,
                Visibility = Visibility.Collapsed, //initilise as collapses
                Width = Utilities.TileSize, //set dimesions to tilesize
                Height = Utilities.TileSize
            };
            Canvas.SetZIndex(interactionOutline, 8);
            GameCanvas.Children.Add(interactionOutline);

           MouseOnScreen = this.IsMouseOver;   //force update

       }
       private void InitiliseFPS()
       {
           animationStopwatch.Start();
           fpsStopwatch.Start(); //start FPS timer
           fpsTextBlock.Text = "0.0 FPS";
           Canvas.SetLeft(fpsTextBlock, CameraWidth * Utilities.TileSize -
90);
           Canvas.SetTop(fpsTextBlock, 10);
       }
       private void InitiliseDayClock()
       {
           timeTextBlock.Text = "07:00";
           dayClockStopwatch.Start(); //start dayclock
           Canvas.SetLeft(timeTextBlock, CameraWidth * Utilities.TileSize -
90);
           Canvas.SetTop(timeTextBlock, 30);
       }
       private void SetCameraSettings()
       {
           cameraCenter = new Point(myPlayer.playerX, myPlayer.playerY);

             //Clamp camera to map edges
            cameraCenter.X = Math.Clamp(cameraCenter.X, minCameraX,
maxCameraX);

            cameraCenter.Y = Math.Clamp(cameraCenter.Y, minCameraY,
maxCameraY);

            //move players image & hitbox
            Canvas.SetLeft(playerImage, myPlayer.playerX - cameraCenter.X +
halfCameraWidth - playerImage.Width / 2 - myPlayer.LeftPadding);
            Canvas.SetTop(playerImage, myPlayer.playerY - cameraCenter.Y +
halfCameraHeight - playerImage.Height / 2 - myPlayer.TopPadding); //offsets as
image isnt centred
            UpdatePlayerHitboxDisplay();

           //update previous position
           previousPlayerX = myPlayer.playerX;
           previousPlayerY = myPlayer.playerY;
       }

       //Parallax backgrounds
       private void InitiliseParallax(bool NewMap)
       {
           var parallaxLoader = new ParallaxImageLoader();
           parallaxUpdater = new ParallaxUpdater();

            //forest background (moves very slow, far away)

parallaxUpdater.AddLayer(parallaxLoader.GetParallaxImage("ForestBackground"),
0.2, 0.075, GameCanvas, 1, 1, 0, (mapY_Height - ((int)Math.Round(minHeight) +
(maxDiffernece / 2))) * Utilities.TileSize);

            //trees (closer, moves faster)

parallaxUpdater.AddLayer(parallaxLoader.GetParallaxImage("ForestTrees"), 0.6,
0.15, GameCanvas, 2, 1.1, 320, (mapY_Height - ((int)Math.Round(minHeight) +
(maxDiffernece / 2))) * Utilities.TileSize);

            if (NewMap == true)
            {
                FindNewWorldHeights();
            }
            caveBackground = new
CaveBackground(parallaxLoader.GetParallaxImage("CaveBackground"), 3,
GameCanvas, 3, smoothCeiling);

       }
       private void FindNewWorldHeights()
       {
           newHeights = new int[mapX_Width];

            for (int x = 0; x < mapX_Width; x++)
            {
                for (int y = 0; y < mapY_Height; y++)
                {
                    if (tileMapData[x, y] != 0 &&
!TileDictionary.TileData[tileMapData[x, y]].IsWalkableThrough)

                   {
                       newHeights[x] = y + 2;
                       break;
                   }
               }
           }
           int smoothRadius = 6;
           smoothCeiling = new int[mapX_Width];

           for (int x = 0; x < mapX_Width; x++)
           {
               int sum = 0;
               int count = 0;
               for (int dx = -smoothRadius; dx <= smoothRadius; dx++)
               {
                   int nx = x + dx;
                   if (nx >= 0 && nx < mapX_Width)
                   {
                       sum += newHeights[nx];
                       count++;
                   }
               }
               int averaged = sum / count;


                //if big diffenece in average in negative way (big drop ie
tunnel) will take averaged for no big drops
                if ((newHeights[x] - averaged) > 5)
                {
                    smoothCeiling[x] = averaged;
                }
                else
                {
                    smoothCeiling[x] = newHeights[x];
                }
            }
        }


       //map generation
       private void GenerateMap()
       {
           int[] heights = GenerateHeightsUsing1DPerlinNoise();
           GenerateCavesUsing2DPerlinNoise(heights);

           PerlinNoise1D.Clear1DGradientValues(); //cleanup
           PerlinNoise2D.Clear2DGradientValues();

           CarveInitialTunnels();

           int iterationsCA = 3;
           ApplyCellularAutomata(iterationsCA);


           GenerateOres();

           GenerateBottomFloorBedrock();

           PerlinNoise1D.Clear1DGradientValues();
           GenerateTrees();

           PerlinNoise1D.Clear1DGradientValues(); //cleanup
           PerlinNoise2D.Clear2DGradientValues();

           //get a 2D map of how blocks a connected
           for (int x = 0; x < mapX_Width; x++)
           {
               for (int y = 0; y < mapY_Height; y++)
               {
                   UpdateSingleConnection(x, y);
               }
           }

        }
        private int[] GenerateHeightsUsing1DPerlinNoise()
        {
            //height terrain
            int baseFrequency = Math.Max(1, mapX_Width / 120); //major
generation every 150 -> but atleast 1
            int octaves = 4; //starting 3 octaves
            double persistance = 0.3; //scales down by 0.3 every octave


            double amplification = 4; //due to Gaussian-like distribution,
tends towards 0

            //store heights in an array for later use -> relate to that x axis


            for (int x = 0; x < mapX_Width; x++)
            {
                double X_fraction = (double)x / (double)mapX_Width; //NOTE:
can only use fraction from 0 to 1, still works as actaul bigger number does
nothing -> mapY_height - () -> as then starts from bottom and goes up

                //handles rare outliers
                double noise = PerlinNoise1D.GeneratePerlinNoise(X_fraction,
baseFrequency, octaves, persistance); //noise gives range -1->1,
                double noiseAmplified = Math.Clamp(((((noise *
(double)amplification)) + 1) / 2.0), 0.0, 1.0); //range 0 to 1. make sure not
over 1 or under 0 after amplification
                //Console.WriteLine(noiseAmplified);

                heights[x] = (int)Math.Round(mapY_Height - ((noiseAmplified *
maxDiffernece) + minHeight));
                //Console.WriteLine($"Column {x} has terrain height
{heights[x]}");

           }
           return heights;
       } //perlin Noise
       private void GenerateCavesUsing2DPerlinNoise(int[] heights)
       {
           //unique offsets
           Random rand = new Random();
           double offsetX = rand.NextDouble() * 500;
           double offsetY = rand.NextDouble() * 500;

              int caveBaseFrequency = mapX_Width / 225; //larger caves at lower
frequencies
            int caveOctaves = 3; //sdds fine details
            double cavePersistence = 0.35; //balances detail & smoothness,
lower = smoother
            double cavebaseThreshold = 0; //lower = less caves, higher = fewer
            double depthFactor1 = 0.3; //controls how much caves widen as you
get deeper -> higher = more
            double depthFactor2 = 0.15;
            int amplification = 3; //amplifies noise

              //noise repeats every mapheight
              int noiseRepeatCount = Math.Max(1, mapX_Width / mapY_Height);
              double sectionWidth = (double)mapX_Width / 100;

              //sections scale with map size
              double sectionStep = mapY_Height / 2.0; //can adjust

              double divionFactor = mapX_Width / 10;

              for (int x = 0; x < mapX_Width; x++)
              {
                  int sectionIndex = x / (int)sectionWidth;
                  double sectionOffsetX = offsetX + sectionIndex * sectionStep;

                int surfaceY = heights[x];
                for (int y = 0; y < mapY_Height; y++)
                {
                    if (y < surfaceY)
                    {
                        //air above the surface
                        tileMapData[x, y] = 0;
                    }
                    else
                    {
                        double sectionVariation = sectionIndex * 0.137;
//small, irregular offset per section
                        double nx = (x + sectionOffsetX + sectionVariation) /
divionFactor;
                        double ny = (y + offsetY + sectionVariation) /
divionFactor;



                        double caveNoise =
PerlinNoise2D.GeneratePerlinNoise(nx, ny, caveBaseFrequency, caveOctaves,
cavePersistence);
                        double amplifiedCaveNoise = Math.Clamp(((((caveNoise *
(double)amplification)) + 1) / 2.0), 0.0, 1.0);

                        double multiplierToMaxDepth = 4;
                        double timeToMaxDepthFactor1 = Math.Min(1, ((double)(y
- surfaceY - 5) / (double)(mapY_Height - surfaceY - 5)) *
multiplierToMaxDepth); //quater way down (*2)
                        double timeToMaxDepthFactor2 = 0; //intilise
                        if (timeToMaxDepthFactor1 == 1) //when 1st done (fast
depthfatctor)
                        {
                            timeToMaxDepthFactor2 = Math.Min(1, ((double)(y -
surfaceY - ((1 / multiplierToMaxDepth) * mapY_Height) / (double)(mapY_Height -
surfaceY - ((1 / multiplierToMaxDepth) * mapY_Height)) * 2)));
                        }
                        double caveThreshold = cavebaseThreshold +
(depthFactor1 * (timeToMaxDepthFactor1) + depthFactor2 *
timeToMaxDepthFactor2); //get wider caves as deeper

                       if (amplifiedCaveNoise < caveThreshold)
                       {
                           tileMapData[x, y] = 0; //cave
                       }
                       else
                       {
                           //layer dirt near surface stone deeper down
                           if (y < surfaceY + 9)
                           {
                               tileMapData[x, y] = 1; //dirt layer
                           }
                           else
                           {
                               tileMapData[x, y] = 2; //stone layer
                           }
                       }
                   }
               }
           }
       }
       private void CarveInitialTunnels()
       {
           int numTunnels = mapX_Width / 300; //number of tunnels across the
map
            List<int> tunnelStartPositions = new List<int>(); //store valid
start positions of tunnels
            Console.Write("Tunnels: ");

           for (int t = 0; t < numTunnels; t++)
           {


               int startX;
               bool validPosition;
               do
               {
                   Random rand = new Random();
                   startX = rand.Next(5, mapX_Width - 5); //pick a random X
position
                   validPosition = true;

                    //check if its too close to an existing tunnel
                    foreach (int existingX in tunnelStartPositions)
                    {
                        if (Math.Abs(startX - existingX) < 50) //50 block
minimum spacing -> abs make number alw positive
                        {
                            validPosition = false;
                            break;
                        }
                    }
                }
                while (!validPosition); //keep regenerating until a valid
position is found
                Console.Write(startX + " ");

               tunnelStartPositions.Add(startX); //store the valid start
position
               int startY = 10; //start near the surface

               //carve the tunnel
               CarveSpiralTunnel(startX, startY);
               PerlinNoise1D.Clear1DGradientValues();
            }
            Console.WriteLine();
        } //carving tunnels
        private void CarveSpiralTunnel(int x, int y)
        {
            //unique offsets
            int tunnelWidth = 8; //width of average tunnel at start (+1) as
also uses 0 so ie if 8 is 9 width
            double wideningFactor = 0.05; //small chance to widen as depth
increases
            double unWideningFactor = 0.0175; //smaller chance to decrease in
width;
            //make the Perlin noise space square & prevent repetition
            double xCount = 0;
            double xAdd = (double)1 / (double)mapX_Width;
            double yCount = 0;
            double yAdd = (double)1 / (double)mapY_Height;
            int yAmplification = 4;
            int xAmplification = 4;
            int yChange = 0;
            int perlinFreq = mapX_Width / 67;

            while (y < mapY_Height - 2)
            {
                if (x >= mapX_Width || x < 0) return; //stop if out of bounds
                //perlin Noise for up/down movement
                double yNoise = PerlinNoise1D.GeneratePerlinNoise(yCount,
perlinFreq + 1, 4, 0.5);
                double yNoiseAmplified = Math.Clamp(((((yNoise *
(double)yAmplification)) + 1) / 2.0), 0.0, 1.0);
                yCount += yAdd;
                if (yNoiseAmplified > 0.95) { //rarely up 3
                    y -= 3;
                    yChange = -3; }
                else if (yNoiseAmplified > 0.85) { //somtimes up 2
                    y -= 2;
                    yChange = -2; }
                else if (yNoiseAmplified > 0.72) { //somtimes up 1
                    y -= 1;
                    yChange = -1; }
                else if (yNoiseAmplified > 0.59) { yChange = 0; }//sometimes
same
                else if (yNoiseAmplified > 0.33) { //mostly down 1
                    y += 1;
                    yChange = 1; }
                else if (yNoiseAmplified > 0.15) { //somtimes down 2
                    y += 2;
                    yChange = 2; }
                else if (yNoiseAmplified >= 0) { //rarely down 3
                    y += 3;
                    yChange = 3; }
                int yChangeAbs = Math.Abs(yChange);
                bool yPositive;
                if (yChange >= 0) { yPositive = true; }
                else { yPositive = false; }
                for (int i = 0; i < yChangeAbs; i++)
                {
                    if (yPositive == true)
                    {
                        for (int w = -tunnelWidth / 2; w <= tunnelWidth / 2;
w++)
                        {
                            int carveX = Math.Clamp(x + w, 0, mapX_Width - 1);
                            int carveY = Math.Clamp(y - i, 0, mapY_Height -
1);
                            tileMapData[carveX, carveY] = 0;
                        }
                    }
                    else
                    {
                        for (int w = -tunnelWidth / 2; w <= tunnelWidth / 2;
w++)
                        {

                           int carveX = Math.Clamp(x + w, 0, mapX_Width - 1);
                           int carveY = Math.Clamp(y + i, 0, mapY_Height -
1);
                           tileMapData[carveX, carveY] = 0;
                       }
                    }
                }
                //perlin noise for left/right movement
                double xNoise = PerlinNoise1D.GeneratePerlinNoise(xCount,
perlinFreq, 4, 0.5);
                double xNoiseAmplified = Math.Clamp(((((xNoise *
(double)xAmplification)) + 1) / 2.0), 0.0, 1.0);
                xCount += xAdd;
                if (xNoiseAmplified > 0.9) { x -= 3; } //rarey left 3
                else if (xNoiseAmplified > 0.8) { x -= 2; } //somtimes left 2
                else if (xNoiseAmplified > 0.56) { x -= 1; }//moslty left 1
                else if (xNoiseAmplified > 0.44) { }//sometimes same
                else if (xNoiseAmplified > 0.2) { x += 1; }//mostly right 1
                else if (xNoiseAmplified > 0.1) { x += 2; }//somtimes right 2
                else if (xNoiseAmplified >= 0) { x += 3; }//rarely right 3
                if (xNoiseAmplified == 0 || yNoiseAmplified == 0)
                {
                    PerlinNoise1D.Clear1DGradientValues();
                }
                //slowly widen tunnel deeper in the world
                if (y > mapY_Height * 0.9 && Random.Shared.NextDouble() <
wideningFactor) { tunnelWidth = Math.Min(tunnelWidth + 1, 14); } //max width =
15
                else if (y > mapY_Height * 0.7 && Random.Shared.NextDouble() <
wideningFactor) { tunnelWidth = Math.Min(tunnelWidth + 1, 12); } //max width =
13
                else if (y > mapY_Height * 0.5 && Random.Shared.NextDouble() <
wideningFactor) { tunnelWidth = Math.Min(tunnelWidth + 1, 10); } // max width
= 11
                //but small chance to decrease after starts to mayb widen
                if (y > mapY_Height * 0.5 && Random.Shared.NextDouble() <
unWideningFactor)
                {
                    tunnelWidth = Math.Max(tunnelWidth - 1, 0); //max width =
15
                    if (tunnelWidth == 0)
                    {
                        break; //stop if width becomes 0, (very unlikely)
                    }
                }
            }
        }
        private void ApplyCellularAutomata(int iterations)
        {
            int ThresholdNeighbours = 5;
            for (int i = 0; i < iterations; i++) //run multiple times for
better smoothing

            {
                 byte[,] newMap = (byte[,])tileMapData.Clone(); //copy the
existing map

                for (int x = 1; x <= mapX_Width - 2; x++) //dont do edges
                {
                    for (int y = 1; y <= mapY_Height - 2; y++)   5D loop from when called
                    {
                        (int, byte) solidCount = CountSolidNeighbors(x, y,
ThresholdNeighbours); //int for count (max == 8), uses byte for tilemapdata of
a random tile

                        if (solidCount.Item1 >= ThresholdNeighbours)
                        {
                            if (solidCount.Item2 != 0)
                            {
                                newMap[x, y] = solidCount.Item2; //becomes
solids
                            }
                            else
                            {
                                newMap[x, y] = 2; //if glitch then js set to
stone
                                Console.WriteLine("Error in returning solid
tile in Cellular Automata");
                            }
                        }
                        else
                        {
                            newMap[x, y] = 0; //become airs
                        }
                    }
                }

                 tileMapData = newMap; //update map after each iteration
             }
         } //cellular automata

        private (int, byte) CountSolidNeighbors(int x, int y, int
ThresholdNeighbours)
        {
            int count = 0;
            Random rand = new Random();
            int blockReturn = rand.Next(1, ThresholdNeighbours + 1); //gets
random number -> 1 to threshold neighbouts
            byte blockByteData = 0; //initilise

            for (int offsetX = -1; offsetX <= 1; offsetX++) //one to left to
middle to one to right
            {
                for (int offsetY = -1; offsetY <= 1; offsetY++)
                {


                    if (!(offsetX == 0 && offsetY == 0) &&
TileDictionary.TileData[tileMapData[x + offsetX, y + offsetY]].IsSolid)
//ignore center tile
                    {
                        count++;
                        if (count == blockReturn) //returns the data of a
random tile that is solid
                        {
                            blockByteData = tileMapData[x + offsetX, y +
offsetY];
                        }
                    }
                }
            }
            return (count, blockByteData);
        }
        private void GenerateOres()
        {
            //Each ore gets its own noise field so they don't overlap too
predictably
            GenerateSingleOre(
                oreId: 7,                  //Copper (common)
                baseFrequency: mapX_Width / 21, //bigger divisor = lower
frequency = less common, bigger
                octaves: 3,
                persistence: 0.4,
                amplification: 2,
                threshold: 0.82,           //lower threshold = more common
                minDepthRatio: 0,       //Allow near surface
                maxDepthRatio: 0.7
            );

            GenerateSingleOre(
                oreId: 167,                   //Lead (less common) -> bigger
blobs tho
                 baseFrequency: mapX_Width / 25,
                 octaves: 3,
                 persistence: 0.4,
                 amplification: 2,
                 threshold: 0.84,
                 minDepthRatio: 0,
                 maxDepthRatio: 0.8
            );

            GenerateSingleOre(
                oreId: 6,                  //Iron (kind of rare)
                baseFrequency: mapX_Width / 21,
                octaves: 3,
                persistence: 0.4,
                amplification: 2,
                threshold: 0.86,
                minDepthRatio: 0.3,       //Starts a bit below surface


                maxDepthRatio: 0.95
           );

           GenerateSingleOre(
               oreId: 8,                 //Gold (rare)
               baseFrequency: mapX_Width / 20, //smaller blobs
               octaves: 3,
               persistence: 0.4,
               amplification: 2,
               threshold: 0.86,
               minDepthRatio: 0.5,       //Mostly mid/deep
               maxDepthRatio: 1
           );

           GenerateSingleOre(
               oreId: 169,                 //Platinum (very rare)
               baseFrequency: mapX_Width / 18, //smaller blobs
               octaves: 3,
               persistence: 0.4,
               amplification: 2,
               threshold: 0.87,
               minDepthRatio: 0.6,        //Only deep underground
               maxDepthRatio: 1.0
           );
        }
        private void GenerateSingleOre(byte oreId, int baseFrequency, int
octaves, double persistence, int amplification, double threshold, double
minDepthRatio, double maxDepthRatio)
        {
            Random rand = new Random();
            double offsetX = rand.NextDouble() * 500;
            double offsetY = rand.NextDouble() * 500;

            for (int x = 0; x < mapX_Width; x++)
            {
                for (int y = 0; y < mapY_Height; y++)
                {
                    if (tileMapData[x, y] == 2) // stone only
                    {
                        double depthRatio = (double)y / mapY_Height;
                        if (depthRatio < minDepthRatio || depthRatio >
maxDepthRatio)
                        {
                            continue;
                        }

                        double nx = (x + offsetX) / (double)mapX_Width;
                        double ny = (y + offsetY) / (double)mapY_Height;

                        double noise = PerlinNoise2D.GeneratePerlinNoise(nx,
ny, baseFrequency, octaves, persistence);



                        double amplified = Math.Clamp((((noise *
amplification) + 1) / 2.0), 0.0, 1.0);
                        //Console.WriteLine($"Ore {oreId} at ({x},{y}) has
noise {amplified}");
                        if (amplified > threshold)
                            tileMapData[x, y] = oreId;
                    }
                }
            }

            PerlinNoise2D.Clear2DGradientValues();
        }
        private void GenerateBottomFloorBedrock()
        {
            for (int x = 0; x < mapX_Width; x++)
            {
                for (int y = mapY_Height - 2; y <= mapY_Height - 1; y++) //2
layers of bedrock
                {
                    tileMapData[x, y] = 56; //bedrock (obsidian) ->
unbreakable
                }
            }
        }
        private void GenerateTrees()
        {
            FindNewWorldHeights();
            int baseFrequency = Math.Max(1, mapX_Width / 75); //major
generation every 80 -> but atleast 1
            int octaves = 3; //starting 3 octaves
            double persistance = 0.3; //scales down by 0.3 every octave

            int amplification = 3; //due to Gaussian-like distribution, tends
towards 0
            int lastTreeSpaces = 0;
            int treeNextToRadiusLeft = 0;
            Random rand = new Random();
            for (int x = 0; x < mapX_Width; x++)
            {
                lastTreeSpaces += 1;
                double X_fraction = (double)x / (double)mapX_Width;

                double noise = PerlinNoise1D.GeneratePerlinNoise(X_fraction,
baseFrequency, octaves, persistance); //noise gives range -1->1,
                double noiseAmplified = Math.Clamp(((((noise *
(double)amplification)) + 1) / 2.0), 0.0, 1.0); //range 0 to 1. make sure not
over 1 or under 0 after amplification

                if (treeNextToRadiusLeft == 0)
                {
                    treeNextToRadiusLeft = rand.Next(5, 11); //trees not right
next to eachother


               }

               if (treeNextToRadiusLeft <= lastTreeSpaces)
               {
                   if (noiseAmplified > 0.56)
                   {
                       GenerateATree(x, rand);
                       treeNextToRadiusLeft = 0;
                       lastTreeSpaces = 0;
                   }
               }
            }
        }
        private void GenerateATree(int x, Random rand)
        {
            int treeHeight = rand.Next(9, 19); //generates tree size 9 to 18
blocks high
            if (tileMapData[x, newHeights[x] - 2] == 1) //only on dirt //-2
cus how newmapheights works
            {
                for (int y = 0; y < treeHeight; y++)
                {
                    tileMapData[x, newHeights[x] - y - 3] = 191; //living wood
-> -3 as -2 in newheight -1 as above ground
                    if (y == (treeHeight - 1))
                    {
                        GenerateTreeLeaves((x, newHeights[x] - y - 3));
                    }
                }
            }
        }

        Dictionary<(int, int), List<(int, int)>> TreeTop_ToLeafs = new
Dictionary<(int, int), List<(int, int)>>();
        private void GenerateTreeLeaves((int, int) TreeTopCoordinate)
        {
            List<(int, int)> LeafCoordinates = new List<(int, int)>(); //list
of all leaf tile positions

           int centerX = TreeTopCoordinate.Item1; //x position of tree top
           int centerY = TreeTopCoordinate.Item2; //y position of tree top

            int leafRadiusX = 4; //horizontal leaf radius -> cretes
oval/circle shape
            int leafRadiusY = 4; //vertical leaf radius
            Random rand = new Random();

            HashSet<(int, int)> coreLeaves = new HashSet<(int, int)>();
//stores solid inner leaf tiles

            //first part: create solid inner leaves (of radius -1)
            for (int changeX = -(leafRadiusX - 1); changeX <= (leafRadiusX -
1); changeX++)

           {
                for (int changeY = -(leafRadiusY - 1); changeY <= (leafRadiusY
- 1); changeY++)
                {
                    //ELLISPSIES FORMULA
                    double normalized = Math.Pow(changeX /
(double)(leafRadiusX - 1), 2)
                                      + Math.Pow(changeY /
(double)(leafRadiusY - 1), 2); //check if within circle shape

                    if (normalized <= 1.0) //inside ellipse
                    {
                        int worldX = centerX + changeX;
                        int worldY = centerY + changeY;
                        if (worldX >= 0 && worldX < mapX_Width && worldY >= 0
&& worldY < mapY_Height &&
                            tileMapData[worldX, worldY] == 0) //check bounds
and is empty
                        {
                            tileMapData[worldX, worldY] = 192; //place leaf
tile
                            LeafCoordinates.Add((worldX, worldY)); //add to
leaf list
                            coreLeaves.Add((worldX, worldY)); //mark as part
of core
                        }
                    }
                }
            }

            //second part: randomly grow from core leafs (25% chance per side)
            (int, int)[] fourDirections = { (1, 0), (-1, 0), (0, 1), (0, -1)
}; //4-way directions

            foreach (var (leafCoreX, leafCoreY) in coreLeaves) //loop through
each core leaf
            {
                foreach (var (checkDirX, checkDirY) in fourDirections) //check
around each side
                {
                    if (rand.NextDouble() < 0.25) //25% chance to grow outward
                    {
                        int tileX = leafCoreX + checkDirX; //neighbor x
                        int tileY = leafCoreY + checkDirY; //neighbor y

                        if (tileX < 0 || tileX >= mapX_Width || tileY < 0 ||
tileY >= mapY_Height)
                        {
                            continue; //skip if out of bounds
                        }




                         if (tileMapData[tileX, tileY] != 0) //skip if not
empty tile
                         {
                             continue;
                         }

                        //ellipsies eqn again
                        double normalizedOuter = Math.Pow((tileX - centerX) /
(double)(leafRadiusX - 1), 2)
                                               + Math.Pow((tileY - centerY) /
(double)(leafRadiusY - 1), 2); //distance from center

                         if (normalizedOuter > 1.0 && normalizedOuter <= 1.6)
//only add near edge
                         {
                             tileMapData[tileX, tileY] = 192; //add extra leaf
tile
                             LeafCoordinates.Add((tileX, tileY)); //record
position
                         }
                     }
                 }
             }

            TreeTop_ToLeafs.Add(TreeTopCoordinate, LeafCoordinates); //store
generated leaves for this tree
        }
    }
}
