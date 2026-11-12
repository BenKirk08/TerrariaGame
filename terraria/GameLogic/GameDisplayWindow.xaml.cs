using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WPFTerraria.View;
using systemShapes = System.Windows.Shapes;

namespace WPFTerraria
{
    public partial class MainWindow : Window
    {
        //Total Map dimensions in tiles
        private int mapX_Width; //125
        private int mapY_Height; //80

       public int newMapWidth = 1000;
       public int newMapHeight = 500;
       private double minHeight; //for perlin noise map generation
       private int maxDiffernece;
       //Dimesions of tiles the player will see
       public static int CameraWidth { get; } = 40; //40
       public static int CameraHeight { get; } = 24; //24

       //2D array of tiles in map
       public byte[,] tileMapData;
       private string[,] connectionMap;
       int[] heights;
       int[] newHeights;
       int[] smoothCeiling;

       //Create a player
       public Player myPlayer;
       private Image playerImage;
       private Image playerImageArm;
       private bool playerIsWalking;
       private double playerCurrentX;
       private double playerCurrentY;
       private RotateTransform armRotateTransform;
       private const double ArmRotationMax = 165;
       private double currentArmRotation;
       private bool swingingForward;
       private double distanceFallen;

                                                                     4|Page
       //one image for swinging whatever item is selected
       private Image swingItemImage;
       private RotateTransform itemRotateTransform;
       private RotateTransform toolHitboxTransform;
       private double toolHitboxConstantWhenDirection;
       private Stopwatch PotionUsedLast;
       private const int TimeToNextPotionSecs = 25;
       private HealingTimer myHealingTimer;
       private bool NotUsedPotionYet = true;
       Rect playerHitbox;
       Rect playerHitboxCenter;


       //Only update player if different
       private double previousPlayerX; //-1 so will do once
       private double previousPlayerY;

       //Camera
       public Point cameraCenter; //Camera follows player

       //Calculate camera boundaries
       private double minCameraX, maxCameraX, minCameraY, maxCameraY;
       public double halfCameraWidth;
       public double halfCameraHeight;

       private bool aimLocksFacing;

        //gameloop frames
        private Stopwatch frameStopwatch;
        private const double TargetFrameTimeMs = 1000.0 / 60.0; //->60 FPS =
16.67ms per frame
        private double accumulatedTimeInMs;

       private int frameRateCount;
       private Stopwatch fpsStopwatch;
       private Stopwatch dayClockStopwatch;
       private DateTime dayClock;
       private const double secsToChangeTime = 1.0;
       private const int tickMinutes = 5; //how many minutes per tick
       TimeSpan renderTime = TimeSpan.Zero; //for smooth sun/moon
       TimeSpan lastTickTime = TimeSpan.Zero; //previous 5-minute tick
       private double currentFps;

        //For key events
        private HashSet<Key> pressedKeys; //Allows pressing of multiple tiles
at a time

       //Show hitboxes
       public bool showHitboxes;
       private bool hitboxesToggledOnceAlready; //so cant hold button (h) in


                                                                     5|Page
       //Hitbox pooling
       private HashSet<string> currentFrameActiveKeys; //Current used squares
       public systemShapes.Rectangle[,] hitboxMap;
       private Queue<systemShapes.Rectangle> availableHitboxes;

        //for jumping
        private double velocityY = 0; //tracks current vertical speed
        public static double gravity = 0.85; //gravity effect
        private double jumpStrength = -16; //jumping force
        private bool isGrounded; //check if player is on the ground -> so cant
repeatedly jump

       //animating player movement
       private Stopwatch animationStopwatch;
       private Stopwatch jumpingAnimationStopwatch;
       public Stopwatch RegainPlayerHealth;
       public Stopwatch PlayerRegainHealthNow;
       private const double TimeToJumpImage = 135;
       public const double WalkFrameInterval = 100;
       private int currentWalkFrame = 0;
       private bool facingRight; //origionally facing right
       private double xTransformOrigin;

       //initise inventory
       public InventoryHUDDisplay myInventoryHUDDisplay;
       public FullInventoryDisplay myFullInventoryDisplay;
       public PlayerHealthBar myPlayerHealthBar;

       private bool isShooting = false;
       private Stopwatch shootCooldownTimer;

       private Stopwatch deathCountdownTimer;
       private const int respawnTimeInSeconds = 10;

        public Inventory myInventory;
        public bool isInventoryOpen; //see if inventory is open
        private bool inventoryToggledOnceAlready;
        private bool itemslotToggledOnceAlready;
        private int logItemSelected = -1; //when returning from inventory,
selected item returns to what it previously was

       //using mouse
       private bool MouseOnScreen;
       private (int, int)? hoveredTile;
       private systemShapes.Rectangle interactionOutline;
       private int maxPlaceableDistance = 5;
       private int maxMineableDistance = 4;
       private bool isLeftMouseHeldIn;
       private bool mouseMoved;
       private bool playerMoved;

       //pausing game menu

                                                                     6|Page
          private bool isPaused;
          public PauseMenu myPauseMenu;
          private bool PauseMenuToggledOnceAlready;

          private string gameChoice;
          private string genderChoice;
          private string playerGender;
          private bool isSwinging;
          private string currentAnimationType; //run, jumping, standing

        //loaded tiles
        public HashSet<(int, int)> loadedChunks; //stores (chunkX, chunkY) of
loaded chunks
        private BitmapSource[,] loadedTileImages;
        private Point lastCameraPosition = new Point(-1, -1);
        private const int chunkUpdateThreshold = 3; //only update when moved 4
tiles

          //swinging variables
          private double angleRad;
          private bool isArmVisibile;
          private const int swingSpeed = 12; //degrees swung per frame
          private const int rTool = 20; //arm length in pixels (used for circle
radius)
          private const int rItem = 4;
          private double armleftoffset;

        //parallaxLayers
        private ParallaxUpdater parallaxUpdater;
        private CaveBackground caveBackground;
        private Image MoonImage;
        private Image SunImage;
        //Times that it gets to its max brightness (dark or bright)
        TimeSpan sunriseStart = new TimeSpan(6, 0, 0); //6:00 AM
        TimeSpan sunriseEnd = new TimeSpan(7, 30, 0); //7:30 AM
        TimeSpan sunsetStart = new TimeSpan(20, 0, 0); //8:00 PM
        TimeSpan sunsetEnd = new TimeSpan(21, 30, 0); //9:30 PM
        //midpoint of the sunrise/sets
        private TimeSpan SunriseMid => sunriseStart +
TimeSpan.FromMinutes((sunriseEnd - sunriseStart).TotalMinutes / 2);
        private TimeSpan SunsetMid => sunsetStart +
TimeSpan.FromMinutes((sunsetEnd - sunsetStart).TotalMinutes / 2);

          private Stopwatch ZombieSpawnTimer;
          private bool ZombieSpawnTimer_WasPaused;
          private bool ApplyingPlayerKnockback;
          private int KnockbackDirection;
          private int EnemyAmount;

          private Stopwatch playersBeenHitStopwatch;

          private CraftingMenuDisplay myCraftingMenu;

                                                                          7|Page
        //Store active notifications so can stack onto them
        private readonly Dictionary<string, (Grid grid, int currentAmount)>
activeItemGrids = new();
        private const int pixelGapofGrids = 30; //pixels between each
notification

         //restart timers if was paused
         private bool PotionStopped_Timer = false;
         private bool RegainHealthStopper_timer = false;
         private bool WasPaused = false;

         private bool PlayerIsDead = false;

        public void ShowItemChangeNotification(Item item, int changeAmount)
        {
            if (item == null || changeAmount == 0 || ItemNotificationCanvas ==
null) return; //check for invalid inputs or missing canvas - return early

            string key = item.Name; //item name is unique key for stacking
notifications
            Grid grid; //ui container for A SINGLE notification
            TextBlock itemChangeText; //text for showing item name+ quantity
            Image itemImage; //image for item icon

            //check if this item already has an active notification -> for
stacking
            if (activeItemGrids.TryGetValue(key, out var existing)) //if does
exist
            {
                grid = existing.grid; //get existing grid for that item
                existing.currentAmount += changeAmount; //add change amount to
total
                activeItemGrids[key] = (grid, existing.currentAmount);
//update stored amount

                //update the displayed text immediately with new total
                itemChangeText = (TextBlock)grid.Children[1];
                itemChangeText.Text = $"{(existing.currentAmount > 0 ? "+" :
"")}{existing.currentAmount} {item.Name}";

                //restart the fade out timer so updated notification stay
longer
                if (grid.Tag is DispatcherTimer timer)
                {
                    timer.Stop();
                    timer.Start();
                }
                return; //exit since no new grid created
            }

            //create new notification grid if item doesnt alr have one

                                                                     8|Page
            grid = new Grid { Opacity = 0, Margin = new Thickness(0, 2, 0, 2)
}; //start transparent and small margin
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width =
GridLength.Auto }); //column for image
            grid.ColumnDefinitions.Add(new ColumnDefinition()); //column for
text

           itemImage = new Image
           {
               Source = item.Image,
               Width = 22,
               Height = 22,
               Margin = new Thickness(0, 0, 6, 0) //add space between icon
and text
           };
           Grid.SetColumn(itemImage, 0); //place image in first column

            itemChangeText = new TextBlock
            {
                Text = $"{(changeAmount > 0 ? "+" : "")}{changeAmount}
{item.Name}", //ie like "+3 Stone"
                Foreground = new SolidColorBrush(changeAmount > 0 ?
Colors.LimeGreen : Colors.Red), //color based on add/remove
                FontWeight = FontWeights.Bold,
                FontSize = 15,
                VerticalAlignment = VerticalAlignment.Center, //center
vertically
                Effect = new System.Windows.Media.Effects.DropShadowEffect
//add outline for readability
                {
                    Color = Colors.Black, //outline color
                    BlurRadius = 6,
                    ShadowDepth = 3,
                    Opacity = 10,
                }
            };

           Grid.SetColumn(itemChangeText, 1); //place text in second column

           //add to grid
           grid.Children.Add(itemImage);
           grid.Children.Add(itemChangeText);

            //calculate position grids stack upwards from bottom
            double baseY = ItemNotificationCanvas.Children.Count *
pixelGapofGrids; //grid takes up 30 picels height each
            Canvas.SetRight(grid, 0); //align to right edge
            Canvas.SetBottom(grid, baseY); //set bottom position based on how
many grids already there

           ItemNotificationCanvas.Children.Add(grid); //add grid to canvas



                                                                     9|Page
            activeItemGrids[key] = (grid, changeAmount); //store new active
grid with amount in dictionary

            //fade in animation for smooth appearance
            var fadeIn = new DoubleAnimation(0, 1,
TimeSpan.FromMilliseconds(200)); //time to fade in
            grid.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            //set timer to remove notification after short delay
            var removeTimer = new DispatcherTimer { Interval =
TimeSpan.FromSeconds(2.5) }; //2.5 seocnds display time
            grid.Tag = removeTimer; //store timer reference in tag
            removeTimer.Tick += (s, e) => //on timer tick
            {
                removeTimer.Stop(); //stop timer so it only fires once

                //create fade out animation
                var fadeOut = new DoubleAnimation(1, 0,
TimeSpan.FromMilliseconds(300)); //300ms fade out
                fadeOut.Completed += (_, __) =>
                {
                    ItemNotificationCanvas.Children.Remove(grid); //remove
from canvas after fading out
                    activeItemGrids.Remove(key); //remove from active list

                     //reposition all remaining toasts so they shift down
neatly
                    int i = 0;
                    foreach (UIElement child in
ItemNotificationCanvas.Children)
                    {
                        Canvas.SetBottom(child, i * pixelGapofGrids);
                        i++;
                    }
                };
                grid.BeginAnimation(UIElement.OpacityProperty, fadeOut);
//start fade out
            };
            removeTimer.Start(); //begin timer countdown
        }

         private void InitiliseGameCanvas()
         {
             //set dimensions to camera
             GameCanvas.Width = CameraWidth * Utilities.TileSize;
             GameCanvas.Height = CameraHeight * Utilities.TileSize;

            //starts at (0,0)
            Canvas.SetLeft(GameCanvas, 0);
            Canvas.SetTop(GameCanvas, 0);

            //set window dimensions
            GameWindow.Width = CameraWidth * Utilities.TileSize;

            GameWindow.Height = CameraHeight * Utilities.TileSize;
            GameWindow.WindowStartupLocation =
WindowStartupLocation.CenterScreen;
        }

       public MainWindow()
       {
           InitializeComponent();
           Application.Current.MainWindow = this; //set mainwindow to this

            //enable VSync (matches monitor refresh rate)
            RenderOptions.ProcessRenderMode =
System.Windows.Interop.RenderMode.Default;

           CreateAllDatabases();
           //LoadDataFromTable("Items");

           ChooseGame();
       }

       public void ChooseGame()
       {
           InitiliseGameCanvas(); //make a game canvas
           InitililiseMainMenu();

           //wait for UI to load before calling menu logic
           gameChoice = null;

           if (!this.IsLoaded) //WAIT TO LOAD
           {
               this.Loaded += (s, e) => Dispatcher.BeginInvoke(new Action(()
=>
               {
                   ProcessGameStart();
               }));
           }
           else
           {
               ProcessGameStart();
           }
       }
       private void ProcessGameStart()
       {
           gameChoice = WaitForMenuChoice();
           if (gameChoice != null)
           {
               if (gameChoice == "New Game")
               {
                   StartNewGame();
               }
               if (gameChoice == "Load Game")
               {
                   LoadingAGame();

             }
         }
     }

     private string MapSizeSelected;
     //start games
     private void StartNewGame()
     {
         InitililisGenderSelectionMenu();

         //wait for UI to load before calling menu logic
         genderChoice = null;

         if (!this.IsLoaded) //WAIT TO LOAD
         {
             this.Loaded += (s, e) => Dispatcher.BeginInvoke(new Action(()
=>
             {
                 ProcessGenderSelection();
             }));
         }
         else
         {
             ProcessGenderSelection();
         }

         InitililiseMapSelectionMenu();
         MapSizeSelected = null;
         //wait for UI to load before calling menu logic
         if (!this.IsLoaded) //WAIT TO LOAD
         {
             this.Loaded += (s, e) => Dispatcher.BeginInvoke(new Action(()
=>
             {
                 ProcessMapSelection();
             }));
         }
         else
         {
             ProcessMapSelection();
         }
         Console.WriteLine("Starting a New Game");

         SetUpNewGame();

         SetCameraSettings();
         UpdateVisibleTiles();

         SubscribeToGameEvents();
         tileRenderer.UpdateCamera(cameraCenter);
     }
     private void ProcessGenderSelection()
     {

   genderChoice = WaitForGenderChoice();
   if (genderChoice != null)
   {
       if (genderChoice == "Girl Character")
       {
           playerGender = "Girl";
       }
       if (genderChoice == "Boy Character")
       {
           playerGender = "Boy";
       }
   }
}
private void ProcessMapSelection()
{
    MapSizeSelected = WaitForMapSizeChoice();
    if (MapSizeSelected != null)
    {
        if (MapSizeSelected == "Small Map")
        {
            newMapWidth = 1500;
            newMapHeight = 500;
        }
        if (MapSizeSelected == "Medium Map")
        {
            newMapWidth = 3000;
            newMapHeight = 1000;
        }
        if (MapSizeSelected == "Large Map")
        {
            newMapWidth = 6000;
            newMapHeight = 2000;
        }
    }
}
private void LoadingAGame()
{
    Console.WriteLine("Loading a Game");

   LoadUpPreviousGameVariables();
   LoadGame();
   LoadUpPreviousGameState();

   SetCameraSettings();
   UpdateVisibleTiles();

   SubscribeToGameEvents();
   tileRenderer.UpdateCamera(cameraCenter);
}
private void SubscribeToGameEvents()
{
    //event subscriptions -> keydown and up alr in xaml file
    CompositionTarget.Rendering += GameLoop;

            this.KeyDown += GameWindow_KeyDown;
            this.KeyUp += GameWindow_KeyUp;

            this.MouseMove += GameCanvas_MouseMove;
            this.MouseEnter += GameCanvas_MouseEnter;
            this.MouseLeave += GameCanvas_MouseLeave;

            this.MouseLeftButtonDown += GameCanvas_MouseLeftButtonDown;
            this.MouseLeftButtonUp += GameCanvas_MouseLeftButtonUp;
       }

        //key events
        private void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.H && !hitboxesToggledOnceAlready) //H -> toggle
hitboxes
            {
                showHitboxes = !showHitboxes;
                ToggleHitboxesVisibility();
                hitboxesToggledOnceAlready = true;
            }
            else if (e.Key == Key.I && !inventoryToggledOnceAlready) //I ->
inventory
            {
                if (!isInventoryOpen)
                {
                    logItemSelected = myInventory.SelectedInventoryIndex; //if
opening inventory store current selected item index (slot)
                    interactionOutline.Visibility = Visibility.Collapsed;
//also hide the interaction/hover outline of the mouse
                }
                else
                {
                    //restore selection to the same slot index
                    if (logItemSelected >= 0 && logItemSelected <
myInventory.HudSlotAmount) //if its not -1 (unselecetd)
                    {
                        myInventory.SelectedInventoryIndex = logItemSelected;
                    }
                    UpdateInteractionOutline(); //instantly show interaction
outline again
                    interactionOutline.Visibility = Visibility.Visible;
                }

                isInventoryOpen = !isInventoryOpen; //change the state of
inventory opened

                //change visibility -> only 1 open
                FullInventoryCanvas.Visibility = isInventoryOpen ?
Visibility.Visible : Visibility.Collapsed;
                myCraftingMenu.Visibility = isInventoryOpen ?
Visibility.Visible : Visibility.Collapsed;
                if (myCraftingMenu.Visibility == Visibility.Visible)

               {
                    //get crafting stations near player only when opening
                    List<int> nearbyStationIds =
myCraftingMenu.GetNearbyCraftingStationIDs(); // your existing method

myCraftingMenu.UpdateCraftingStationsIfNeeded(nearbyStationIds);
                }


                InventoryHudCanvas.Visibility = isInventoryOpen ?
Visibility.Collapsed : Visibility.Visible;
                pressedKeys.Clear(); //clear pressed keys to prevent movement
stuck
                inventoryToggledOnceAlready = true;
            }
            else if (e.Key >= Key.D1 && e.Key <= Key.D8 &&
!itemslotToggledOnceAlready) //1-8 -> inventory slot select
            {
                int index = e.Key - Key.D1; //-1 as index starts from 0

               //if pressing the already selected slot deselect
               if (myInventory.SelectedInventoryIndex == index)
               {
                   myInventory.SelectedInventoryIndex = -1; //deselect
               }
               else
               {
                   myInventory.SelectedInventoryIndex = index; //else select
               }
               itemslotToggledOnceAlready = true;
           }
           else if (e.Key == Key.Escape && !PauseMenuToggledOnceAlready)
           {
               TogglePauseMenu();
               PauseMenuToggledOnceAlready = true;
           }
           else //others -> movement (A,D,Space)
           {
               //movement Keys
               pressedKeys.Add(e.Key);
           }
           e.Handled = true;
       }

        public void TogglePauseMenu()
        {
            isPaused = !isPaused;
            PauseMenuCanvas.Visibility = isPaused ? Visibility.Visible :
Visibility.Collapsed;

            //if (isPaused) { CompositionTarget.Rendering -= GameLoop; }
//stop game updates when paused


              //else { CompositionTarget.Rendering += GameLoop; } //resume game
updates
          }
          public void CleanUpGame() //for clearing memory when returning to main
menu
          {
              //remove events
              CompositionTarget.Rendering -= GameLoop;
              GameCanvas.KeyDown -= GameWindow_KeyDown;
              GameCanvas.KeyUp -= GameWindow_KeyUp;

              GameCanvas.MouseMove -= GameCanvas_MouseMove;
              GameCanvas.MouseEnter -= GameCanvas_MouseEnter;
              GameCanvas.MouseLeave -= GameCanvas_MouseLeave;

              GameCanvas.MouseLeftButtonDown -= GameCanvas_MouseLeftButtonDown;
              GameCanvas.MouseLeftButtonUp -= GameCanvas_MouseLeftButtonUp;

              //clear inventory data
              myInventory?.PlayersItems.Clear();

              //clear the tile map
              for (int x = 0; x < tileMapData?.GetLength(0); x++)
              {
                  for (int y = 0; y < tileMapData?.GetLength(1); y++)
                  {
                      tileMapData[x, y] = 0;
                      loadedTileImages[x, y] = null;
                      hitboxMap[x,y] = null;
                      connectionMap[x,y] = null;
                  }
              }


              foreach (UIElement element in GameCanvas.Children)
              {
                  if (element is Image img)
                  {
                      img.Source = null; //Make sure release image memory
                  }
              }
              loadedChunks?.Clear();

              foreach (var enemy in enemies.ToList())
              {
                  RemoveEnemy(enemy);
              }
              foreach (var arrow in currentArrowList.ToList())
              {
                  RemoveArrow(arrow);
              }
              if (currentEyeBoss != null)
              {

               RemoveEyeBoss(currentEyeBoss);
           }

           //remove UI elements
           GameCanvas.Children.Clear();
           FullInventoryCanvas.Children.Clear();
           InventoryHudCanvas.Children.Clear();
           PauseMenuCanvas.Children.Clear();

           GameCanvas.Visibility = Visibility.Collapsed;
           FullInventoryCanvas.Visibility = Visibility.Collapsed;
           InventoryHudCanvas.Visibility = Visibility.Collapsed;
           PauseMenuCanvas.Visibility = Visibility.Collapsed;

           //garbage collection free unused memory
           GC.Collect();
           GC.WaitForPendingFinalizers();
       }

        private void GameWindow_KeyUp(object sender, KeyEventArgs e)
        {
            //if cant hold in
            switch (e.Key)
            {
                case Key.H: hitboxesToggledOnceAlready = false; break;
                case Key.I: inventoryToggledOnceAlready = false; break;
                case Key.Escape: PauseMenuToggledOnceAlready = false; break;
            }
            if (e.Key >= Key.D1 && e.Key <= Key.D8) {
itemslotToggledOnceAlready = false; }

            if (!isInventoryOpen) //Only remove keys if inventorys closed ->
as already all cleared from keydown 'I'
            {
                pressedKeys.Remove(e.Key);
            }
            e.Handled = true;
        }



       //mouse events
       //move mouse
       Point CurrentMousePos = new Point(0,0);
       private void GameCanvas_MouseMove(object sender, MouseEventArgs e)
       {

            //check mouse is currently on the screen else return, changes
hoveredtile to null
            if (!MouseOnScreen) { return; }

            //check if the mouse is directly any other canvas (not on any UI
elements like InverntoryHUD)

            if (IsMouseOverUI(InventoryHudCanvas)) //can add more canvas when
more added, cant js say directly over as doesnt count the images on canvas (e
is the mouse)
            {
                hoveredTile = null; // Stop interacting with tiles
                interactionOutline.Visibility = Visibility.Collapsed;
                return;
            }

            //convert mouse position to tile coordinates
            CurrentMousePos = e.GetPosition(GameCanvas); //returns point of
x&y pixels of mouse on the gamecanvas (e == mouse)
            var newHoveredTile = GetTileFromMousePosition(CurrentMousePos);

           if (hoveredTile != newHoveredTile) //update only when moving to
new tile
           {
               hoveredTile = newHoveredTile;
               mouseMoved = true;
           }
       }



        private (int, int) GetTileFromMousePosition(Point mousePos) //gets the
position of the mouse in game tiles
        {
            //camera centre is relative to map & - half camera size (in this
case gamecanvas as mousepos uses gamecanvas), then add mouse pos to get pos
relative to map
            double worldX = mousePos.X + cameraCenter.X - GameCanvas.Width /
2;
            double worldY = mousePos.Y + cameraCenter.Y - GameCanvas.Height /
2;

            int tileX = (int)Math.Floor(worldX / Utilities.TileSize); //always
round down for the tile (starts at 0)
            int tileY = (int)Math.Floor(worldY / Utilities.TileSize);

           //clamp the tile to the map boundaries
           tileX = Math.Clamp(tileX, 0, mapX_Width - 1);
           tileY = Math.Clamp(tileY, 0, mapY_Height - 1);

            return (tileX, tileY); //return 2 integers of X and Y in tile,
this is what the tile your hovered overs equal to, always called every time
mouse moves
        }



        private bool IsMouseOverUI(FrameworkElement thisParent) //check if
mouse over any UI element in certain canvas


       {
            DependencyObject hoveredElement = Mouse.DirectlyOver as
DependencyObject; //DependancyObject is base class any WPF UI elements
            //FrameworkElemt is step above DependencyObject ie a canvas, an
image

            while (hoveredElement != null)
            {
                if (hoveredElement is FrameworkElement frameworkElement &&
frameworkElement.Parent == thisParent) //detects only UI elements inside the
canvas && its parent so that its not the canvas itself
                { return true; } //mouse is over the UI and return true
                hoveredElement = VisualTreeHelper.GetParent(hoveredElement);
//moves up the UI hierarchy and finds the main canvas even if mouse is over
image inside it so if visual tree has thisParent, is on that canvas
            }
            return false; //mouse is not over any UI -> return false
        }

       private void GameCanvas_MouseLeave(object sender, MouseEventArgs e)
       {
           MouseOnScreen = false;
           hoveredTile = null;
       }

       private void GameCanvas_MouseEnter(object sender, MouseEventArgs e)
       {
           MouseOnScreen = true;
       }

        //press in left mouse button
        private void GameCanvas_MouseLeftButtonDown(object sender,
MouseButtonEventArgs e)
        {
            isSwinging = true; //for swinging animation
            isLeftMouseHeldIn = true; //start to break/place blocks
            HandleBlockInteraction(); //place/break immediately on click
            UpdateInteractionOutline(); //immediatly change interaction
outline
        }

        //let up left mouse button
        private void GameCanvas_MouseLeftButtonUp(object sender,
MouseButtonEventArgs e)
        {
            isSwinging = false; //for swinging animation
            isLeftMouseHeldIn = false; //stop breaking/placing blocks

            if (currentMiningTile.HasValue) //if let up mouse, remove the
current mining value
            {
                activeMinings.Remove(currentMiningTile.Value);


                currentMiningTile = null; //set current mining to null
            }
            UpdateInteractionOutline();
        }
    }
}
