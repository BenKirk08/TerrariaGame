using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using systemShapes = System.Windows.Shapes;

namespace WPFTerraria
{
    internal class EyeBoss
    {
        //core fields
        public double eyeBossX { get; set; } //world x
        public double eyeBossY { get; set; } //world y
        public int _eyeBossMaxHealth { get; set; } //max hp
        public int _eyeBossHealth { get; set; } //current hp
        public double currentSpeed { get; set; } //move speed
        public int Damage { get; set; } = 16; //damage dealt per hit

       //image and transforms
       public BitmapSource CurrentImage { get; set; } //current frame
       public Image eyeBossImageControl { get; set; } //image control
       public RotateTransform EyeBossRotationTransform; //rotation for hitbox

       public double halfImageWidth; //half width cached
       public double halfImageHeight; //half height cached
       private double pivotImageX; //image pivot x
       private double pivotImageY; //image pivot y
       public double pivotRectX; //hitbox pivot x
       public double pivotRectY; //hitbox pivot y


       //hitboxes

        public systemShapes.Rectangle thisEyeBossHitboxRectangle; //main
hitbox rect
        public systemShapes.Rectangle thisEyeBossCenterHitboxRectangle;
//center hitbox rect

       public Rect Hitbox
       {
           get
           {
               return new Rect(
                   eyeBossX - halfImageWidth,
                   eyeBossY - halfImageHeight,
                   eyeBossImageControl.Width,
                   eyeBossImageControl.Height - Utilities.TileSize * 2.2
               );

              }
          }

          public Rect eyeBossHitboxCenter
          {
              get { return new Rect(eyeBossX - 3.75, eyeBossY - 3.75, 7.5,
7.5); }
          }

          //animation and health bar
          private Stopwatch eyeBossAnimationStopwatch = new Stopwatch(); //anim
timer
          public Stopwatch hitPlayerStopwatch = new Stopwatch(); //player hit
delay
          public Stopwatch eyeBossBeenHit = new Stopwatch(); //boss hit delay

          public BossHealthBar eyeBossHealthBar; //ui element

          private bool isInStageOne = true; //stage check
          private int currentFrame = 0; //frame index
          private List<BitmapSource> stageOneFrames; //frames stage 1
          private List<BitmapSource> stageTwoFrames; //frames stage 2

        public EyeBoss(double x, double y, List<BitmapSource> stage1Frames,
List<BitmapSource> stage2Frames)
        {
            eyeBossX = x;
            eyeBossY = y;
            currentSpeed = 7.5;

              stageOneFrames = stage1Frames;
              stageTwoFrames = stage2Frames;

              CurrentImage = stage1Frames.FirstOrDefault();
              eyeBossImageControl = new Image
              {
                  Source = CurrentImage,
                  Width = CurrentImage.Width * 1.2,
                  Height = CurrentImage.Height * 1.2,
              };

              //timers
              eyeBossAnimationStopwatch.Start();
              hitPlayerStopwatch.Start();
              eyeBossBeenHit.Start();

              //cache half imagesize
              halfImageWidth = eyeBossImageControl.Width / 2;
              halfImageHeight = eyeBossImageControl.Height / 2;

              //create health bar
              eyeBossHealthBar = new BossHealthBar();
              var main = Application.Current.MainWindow as MainWindow;

           main.GameCanvas.Children.Add(eyeBossHealthBar);
           Canvas.SetZIndex(eyeBossHealthBar, 9);

            //place health bar
            Canvas.SetLeft(eyeBossHealthBar, (MainWindow.CameraWidth *
Utilities.TileSize / 2) - (eyeBossHealthBar.HealthBarWidth / 2));
            Canvas.SetTop(eyeBossHealthBar, MainWindow.CameraHeight *
Utilities.TileSize - eyeBossHealthBar.HealthBarHeight - (Utilities.TileSize *
2));

           //set hp
           _eyeBossMaxHealth = 750;
           ChangeBossMaxHealth(0);
           _eyeBossHealth = 750;
           ChangeBossHealth(0);

            //set pivots
            pivotImageX = halfImageWidth;
            pivotImageY = halfImageHeight + Utilities.TileSize * 1.1;
//adjusted vertically
            pivotRectX = halfImageWidth;
            pivotRectY = halfImageHeight;

           //hover settings for phase 1
           framesForArc = 60 * 5; //5s arc
           idleRotateSpeedHover = (double)360 / (double)framesForArc;
           hoverSpeed = 1.0 / framesForArc;
       }

        //health handling
        public void ChangeBossHealth(int healthChange)
        {
            int newHealth = Math.Min(_eyeBossMaxHealth, Math.Max(0,
_eyeBossHealth + healthChange));
            eyeBossHealthBar.BossHealth = newHealth; //update healthbar
            _eyeBossHealth = newHealth;
            if (isInStageOne && newHealth < (_eyeBossMaxHealth / 2)) //if half
health -> transform
            {
                currentState = EyeBossState.Transforming;
            }
        }

        protected void ChangeBossMaxHealth(int maxHealthChange)
        {
            eyeBossHealthBar.BossMaxHealth = _eyeBossMaxHealth +
maxHealthChange;
            _eyeBossMaxHealth += maxHealthChange;
        }

        //states and ai update
        public enum EyeBossState { Idle, Hover, Dash, Transforming, OrbPhase,
Dead } //all the states

        public EyeBossState currentState = EyeBossState.Hover; //satrt state

       public void UpdateAI(MainWindow game)
       {
           if (_eyeBossHealth <= 0) //die if no health
           {
               currentState = EyeBossState.Dead;
           }

           switch (currentState) //only dp updates for current state
           {
               case EyeBossState.Idle:
                   DoIdleBehaviour(game);
                   break;
                                                      Finite state machine
               case EyeBossState.Hover:
                   DoHoverMechanic(game);
                   break;
               case EyeBossState.Dash:
                   DoDash(game);
                   break;
               case EyeBossState.Transforming:
                   TransformToStageTwo();
                   break;
               case EyeBossState.OrbPhase:
                   DoOrbPhase(game);
                   break;
               case EyeBossState.Dead:
                   game.EyeBossIsDead = true;
                   CleanupOrbs(game);
                   break;
           }

           UpdateAnimation();

            //place image so local pivot aligns w/world eye position
            Canvas.SetLeft(eyeBossImageControl, eyeBossX - game.cameraCenter.X
+ game.halfCameraWidth - pivotImageX);
            Canvas.SetTop(eyeBossImageControl, eyeBossY - game.cameraCenter.Y
+ game.halfCameraHeight - pivotImageY);
        }

        //idle and look mechanics
        private double facingAngle = 0.0; //0 deg = right
        private double idleRotateSpeed; //deg per frame
        private const double idleRotateSpeedDashPhase1 = 2.5; //degrees per
frame when dashing phase 1
        private const double idleRotateSpeedDashPhase2 = 5; //degrees per
frame when dashing phase 2
        private double idleRotateSpeedHover; //hover rotation speed
        private bool facingInitialized = false;

       private void DoIdleBehaviour(MainWindow game)
       {

            //check stage for rotation speed
            idleRotateSpeed = isInStageOne ? idleRotateSpeedDashPhase1 :
idleRotateSpeedDashPhase2;

             if (LookAtPlayerMechanic(game))
             {
                 currentState = EyeBossState.Dash; //switch to dash if facing
player
                 facingInitialized = false;
             }
         }

        private bool LookAtPlayerMechanic(MainWindow game)
        {
            if (!facingInitialized)
            {
                facingAngle = EyeBossRotationTransform.Angle + 90; //get
current facing
                facingInitialized = true;
            }

             //vector to player
             double dx = game.myPlayer.playerX - eyeBossX;
             double dy = game.myPlayer.playerY - eyeBossY;       Vectors & angles

             //angle to player (0 deg=right and positive y down)
             double targetAngle = Math.Atan2(dy, dx) * 180 / Math.PI;
             if (targetAngle < 0)
             {
                 targetAngle += 360;
             }

             //find shortest signed difference
             double diff = (targetAngle - facingAngle + 540) % 360 - 180;

             bool isFacing = false;
             if (Math.Abs(diff) > idleRotateSpeed)
             {
                 facingAngle += Math.Sign(diff) * idleRotateSpeed;
             }
             else
             {
                 facingAngle = targetAngle;
                 isFacing = true;
             }

             facingAngle = (facingAngle + 360) % 360;
             UpdateImageRotation(facingAngle);
             return isFacing;
         }

         //hover behaviour
         private static readonly Random rand = new Random();

       private bool firstTimeMovement = true;
       private double offsetX = 0, offsetY = 0;
       private bool arcUp = true;
       private double startX = 0, startY = 0;
       private double hoverT = 0.0; //progress 0 to 1
       private int framesForArc;
       private double hoverSpeed; //speed over arc
       private double arcHeight; //arc height
       private void DoHoverMechanic(MainWindow game)
       {
           idleRotateSpeed = idleRotateSpeedHover;
           LookAtPlayerMechanic(game); //look at player while hovering
           DoHoverMovement(game);
       }

       private void DoHoverMovement(MainWindow game)
       {
           if (firstTimeMovement)
           {
               firstTimeMovement = false;
               startX = eyeBossX;
               startY = eyeBossY;

                //choose relative offset to player
                offsetX = (eyeBossX < game.myPlayer.playerX) ? rand.Next(200,
301) : -rand.Next(200, 301);

                if (eyeBossY < game.myPlayer.playerY)
                {
                    offsetY = -rand.Next((int)Math.Round(game.halfCameraHeight
- 105), (int)Math.Round(game.halfCameraHeight) - 50);
                    arcUp = true;
                }
                else
                {
                    offsetY = rand.Next((int)Math.Round(game.halfCameraHeight
- 105), (int)Math.Round(game.halfCameraHeight) - 50);
                    arcUp = false;
                }

               arcHeight = rand.Next(80, 115); //height of arc up/down
               hoverT = 0.0;
           }

            //follow players movement
            double targetX = game.myPlayer.playerX + offsetX;
            double targetY = game.myPlayer.playerY + offsetY;

           //progress interpolation
           hoverT += hoverSpeed;
           if (hoverT > 1.0)
           {


               hoverT = 1.0; //clamp
           }

            //linear interpolate
            double x = startX + (targetX - startX) * hoverT;
            double y = startY + (targetY - startY) * hoverT;

           //add sin arc
           double curveOffset = Math.Sin(hoverT * Math.PI) * arcHeight;
           y += arcUp ? -curveOffset : curveOffset;

           eyeBossX = x;
           eyeBossY = y;

            //finished arc
            if (hoverT >= 1.0) //finished hovering
            {
                firstTimeMovement = true;
                hoverT = 0.0;
                currentState = EyeBossState.Idle; //change to idle state after
hover to make sure looking at player
            }
        }
        //dash behaviouer
        private bool startedPhase1Dash = false;
        private bool startedNewDashPhase1 = true;
        private int totalPhase1Dashes = 0;
        private int maxPhase1Dashes;
        private double distancePastPlayer;
        private double totalDistanceMoved = 0;
        private double totalDistanceToMove = 0;
        private void DoDash(MainWindow game)
        {
            if (startedNewDashPhase1)
            {
                startedNewDashPhase1 = false;
                maxPhase1Dashes = rand.Next(3, 6); //3 to 5 dashes
            }

            if (!startedPhase1Dash)
            {
                startedPhase1Dash = true;
                totalPhase1Dashes += 1;
                distancePastPlayer = rand.Next(450, 551);
                totalDistanceToMove += distancePastPlayer;
                totalDistanceToMove +=
Math.Sqrt(Math.Pow(game.myPlayer.playerX - eyeBossX, 2) +

Math.Pow(game.myPlayer.playerY - eyeBossY, 2));
            }

            //direction and move


            facingAngle = EyeBossRotationTransform.Angle + 90;
            double angle = facingAngle * Math.PI / 180;
            eyeBossX += currentSpeed * Math.Cos(angle);
            eyeBossY += currentSpeed * Math.Sin(angle);
            totalDistanceMoved += Math.Abs(currentSpeed * Math.Cos(angle)) +
                                  Math.Abs(currentSpeed * Math.Sin(angle));

            //finished dash
            if (totalDistanceToMove <= totalDistanceMoved)
            {
                totalDistanceMoved = 0;
                totalDistanceToMove = 0;
                startedPhase1Dash = false;
                currentState = EyeBossState.Idle; //idle in between dashes

                if (totalPhase1Dashes >= maxPhase1Dashes)
                {
                    totalPhase1Dashes = 0;
                    currentState = EyeBossState.Hover; //after all dashes
hover
                    startedNewDashPhase1 = true;
                }
            }
        }

        //transform animation
        private int currentTransformingFrame = 0;
        private const int totalTransformingFrames = 60 * 4; //4 seconds
        private const double totalTransformingAngles = 360.0 * 3.0; //3 full
spins
        private double startingTransfromingAngle;
        private bool JustStartedTransforming = true;
        private double EaseInOutExponetially(double perctangeComplete)
        {
            if (perctangeComplete < 0.5)
            {
                return 0.5 * Math.Pow(2, (20 * perctangeComplete) - 10);
            }
            else
            {
                isInStageOne = false;
                return 1 - 0.5 * Math.Pow(2, (-20 * perctangeComplete) + 10);
            }
        }

        private void TransformToStageTwo()
        {
            if (JustStartedTransforming)
            {
                JustStartedTransforming = false;
                startingTransfromingAngle = EyeBossRotationTransform.Angle +
90;
            }
            currentTransformingFrame++;
            double percentageComplete = (double)currentTransformingFrame /
totalTransformingFrames;
            double eased = EaseInOutExponetially(percentageComplete);
            double angle = eased * totalTransformingAngles;

              UpdateImageRotation(startingTransfromingAngle + angle);

              if (currentTransformingFrame >= totalTransformingFrames)
              {
                  //reset load of variables
                  totalDistanceMoved = 0;
                  totalDistanceToMove = 0;
                  totalPhase1Dashes = 0;
                  startedPhase1Dash = false;
                  startedNewDashPhase1 = true;
                  facingInitialized = false;

                  firstTimeMovement = true;
                  hoverT = 0.0;
                  currentTransformingFrame = 0;
                  JustStartedTransforming = true;
                  framesForArc /= 2;
                  idleRotateSpeedHover = 360.0 / framesForArc;
                  hoverSpeed = 1.0 / framesForArc;
                  currentSpeed *= 1.7;
                  currentState = EyeBossState.OrbPhase;
              }
          }

          //orb phase
          private class EyeOrb
          {
              public double x, y;              //world pos
              public double localX, localY;    //final ring offset (radius
vector)
              public double targetX, targetY; //ring world target
              public bool arrived = false;     //reached ring slot
              public bool isFired = false;
              public bool isCharging = false;
              public double velX, velY;
              public double chargeTimer = 0;
              public Ellipse orbShape;
              public int OrbDamage = 4;
              public bool OrbsHitPlayer = false;
          }

          //fields
          private List<EyeOrb> orbitingOrbs = new List<EyeOrb>();
          private const int orbCount = 10;



        private readonly double orbRadius = MainWindow.CameraHeight / 2 *
Utilities.TileSize - 100;
        private double orbRotateSpeed = 3 ;
        private double orbFireInterval = 60 * 1.1;
        private const double orbSpeedMultiplier = 1.2; //speed relative to
boss speed
        private bool orbsSpawned = false;
        private bool ringFormed = false; //rotation starts only after all
arrived
        private int orbChargeDelay = 0;
        private readonly Random orbRand = new Random();

        //boss orbit + approach
        private double bossOrbitAngleDeg = 0;
        private double bossOrbitSpeedDeg = 1.2;
        private double bossOrbitMargin = 20;
        private bool bossMovingToStart = false;
        private double bossTargetStartX, bossTargetStartY;
        private double bossStartAngleDeg = 0; //lock start side
        private bool bossMovingToHover = false;

        private double storedPlayerX, storedPlayerY; //player position when
phase started
        private double storedPlayerXOrb, storedPlayerYOrb; //player position
when phase started
        private double orbChargeSpeedMultiplier = 1.0; //starts normal



        //helpers
        private static double Clamp(double PlayerWorldX, double WorldOrbMin,
double WorldOrbMax) => PlayerWorldX < WorldOrbMin ? WorldOrbMin :
(PlayerWorldX > WorldOrbMax ? WorldOrbMax : PlayerWorldX);

        private bool IsWorldPointOffscreen(MainWindow game, double worldX,
double worldY, double margin = 100)
        {
            double left = game.cameraCenter.X - game.halfCameraWidth - margin;
            double right = game.cameraCenter.X + game.halfCameraWidth +
margin;
            double top = game.cameraCenter.Y - game.halfCameraHeight - margin;
            double bottom = game.cameraCenter.Y + game.halfCameraHeight +
margin;
            return (worldX < left || worldX > right || worldY < top || worldY
> bottom);
        }

       private void CleanupOrbs(MainWindow game)
       {
           foreach (var orb in orbitingOrbs)
           {
               game.GameCanvas.Children.Remove(orb.orbShape);

           }
           orbitingOrbs.Clear();
           orbsSpawned = false;
           ringFormed = false;
       }

       //main phase
       private void DoOrbPhase(MainWindow game)
       {
           if (_eyeBossHealth <= 0)
           {
               CleanupOrbs(game);
               currentState = EyeBossState.Dead;
               return;
           }

            //ring center near player, clamped to screen
            double ringCenterX = Clamp(game.myPlayer.playerX,
                game.cameraCenter.X - game.halfCameraWidth + orbRadius,
                game.cameraCenter.X + game.halfCameraWidth - orbRadius);
            double ringCenterY = Clamp(game.myPlayer.playerY,
                game.cameraCenter.Y - game.halfCameraHeight + orbRadius,
                game.cameraCenter.Y + game.halfCameraHeight - orbRadius);

            double bossR = orbRadius + bossOrbitMargin +
Math.Max(halfImageWidth, halfImageHeight);

           //spawn once
           if (!orbsSpawned)
           {
               CleanupOrbs(game);
               orbitingOrbs.Clear();

                //decide start side: if boss is left of center -> start at
leftmost, else rightmost
                bossStartAngleDeg = (eyeBossX < ringCenterX) ? 180.0 : 0.0;
                bossTargetStartX = ringCenterX + bossR *
Math.Cos(bossStartAngleDeg * Math.PI / 180.0);
                bossTargetStartY = ringCenterY + bossR *
Math.Sin(bossStartAngleDeg * Math.PI / 180.0);
                bossMovingToStart = true;
                bossMovingToHover = false;
                bossOrbitAngleDeg = bossStartAngleDeg; //when we begin
orbiting, start from here

                //orbs spawn off-screen along their *own* radial then fly
inward to ring slot
                double spawnExtra = Math.Max(game.halfCameraWidth,
game.halfCameraHeight) + 60; //off-screen
                for (int i = 0; i < orbCount; i++)
                {
                    double angle = i * (360.0 / orbCount);
                    double radians = angle * Math.PI / 180.0;

                     //final ring offset and world target
                     double localx = orbRadius * Math.Cos(radians);
                     double localy = orbRadius * Math.Sin(radians);
                     double tileX = ringCenterX + localx;
                     double tileY = ringCenterY + localy;

                     //spawn point further along same radial, outside screen
bounds
                     double spawnX = ringCenterX + (orbRadius + spawnExtra) *
Math.Cos(radians);
                     double spawnY = ringCenterY + (orbRadius + spawnExtra) *
Math.Sin(radians);

                     var orb = new EyeOrb
                     {
                         localX = localx,
                         localY = localy,
                         targetX = tileX,
                         targetY = tileY,
                         x = spawnX,
                         y = spawnY,
                         arrived = false,
                         orbShape = new Ellipse
                         {
                             Width = 18,
                             Height = 18,
                             Fill = Brushes.Cyan,
                             Stroke = Brushes.White,
                             StrokeThickness = 2
                         }
                     };
                     game.GameCanvas.Children.Add(orb.orbShape);
                     Canvas.SetZIndex(orb.orbShape, 6);
                     orbitingOrbs.Add(orb);
                 }

                 orbsSpawned = true;
                 ringFormed = false;
                 orbChargeDelay = 30; //wait before first charge
                                      //do not orbit yet; let boss and orbs
approach first
                                      //update stored player position for next
frame

                 storedPlayerX = game.myPlayer.playerX;
                 storedPlayerY = game.myPlayer.playerY;
                 storedPlayerXOrb = game.myPlayer.playerX;
                 storedPlayerYOrb = game.myPlayer.playerY;
                 return;
           }

            //boss glides to start point (no teleport)

           if (bossMovingToStart)
           {
               //adjust boss target if player moved since start
               double playerChangeX = game.myPlayer.playerX - storedPlayerX;
               double playerChnageY = game.myPlayer.playerY - storedPlayerY;
               bossTargetStartX += playerChangeX;
               bossTargetStartY += playerChnageY;

               //update stored player position for next frame
               storedPlayerX = game.myPlayer.playerX;
               storedPlayerY = game.myPlayer.playerY;

                double bossChangeX = bossTargetStartX - eyeBossX;
                double bossChangeY = bossTargetStartY - eyeBossY;   Pythagoras theorem

                double distance = Math.Sqrt(bossChangeX * bossChangeX +
bossChangeY * bossChangeY);
                double step = game.myPlayer.playerSpeed * 1.15; //slower
approach but faster than player so player cant run away
                if (distance <= step)
                {
                    eyeBossX = bossTargetStartX;
                    eyeBossY = bossTargetStartY;
                    bossMovingToStart = false; //ready to orbit once ring
formed
                }
                else
                {
                    eyeBossX += (bossChangeX / distance) * step;
                    eyeBossY += (bossChangeY / distance) * step;
                }

                //face player while approaching
                double angle = Math.Atan2(ringCenterY - eyeBossY, ringCenterX
- eyeBossX) * 180 / Math.PI;
                if (angle < 0) angle += 360;
                UpdateImageRotation(angle);
            }

            //orbs fly straight in to their ring slots (no rotation yet)
            bool allArrived = true;
            double approachSpeed = currentSpeed * 0.9;

            //compute player movement once
            double playerDx = game.myPlayer.playerX - storedPlayerXOrb;
            double playerDy = game.myPlayer.playerY - storedPlayerYOrb;

            //shift targets and current positions together so paths stay
locked to player
            if (Math.Abs(playerDx) > 0.001 || Math.Abs(playerDy) > 0.001)
            {
                foreach (var orb in orbitingOrbs)
                {

                   if (!orb.arrived)
                   {
                       orb.targetX += playerDx;   //move where it's heading
                       orb.targetY += playerDy;
                       orb.x += playerDx;         //move where it is now
                       orb.y += playerDy;
                   }
               }
           }

            //now advance each orb toward its (possibly shifted) target
            foreach (var orb in orbitingOrbs)
            {
                if (!orb.arrived)
                {
                    double changeInX = orb.targetX - orb.x;
                    double changeInY = orb.targetY - orb.y;
                    double distance = Math.Sqrt(changeInX * changeInX +
changeInY * changeInY);
                                                         Pythagoras theorem
                   if (distance <= approachSpeed)
                   {
                       orb.x = orb.targetX;
                       orb.y = orb.targetY;
                       orb.arrived = true;
                   }
                   else
                   {
                       double inv = approachSpeed / distance;
                       orb.x += changeInX * inv;
                       orb.y += changeInY * inv;
                   }
               }
               allArrived &= orb.arrived;
           }

           //update stored player position once per frame (after shifts)
           storedPlayerXOrb = game.myPlayer.playerX;
           storedPlayerYOrb = game.myPlayer.playerY;


            //update orb visuals during approach (cyan pulse)
            double time = Environment.TickCount * 0.01;
            foreach (var orb in orbitingOrbs)
            {
                if (!orb.arrived)
                {
                    double glow = 0.5 + 0.5 * Math.Sin(time + orb.x * 0.002 +
orb.y * 0.002);
                    byte blue = (byte)(200 + 55 * glow);
                    byte green = (byte)(220 + 35 * glow);
                    orb.orbShape.Fill = new
SolidColorBrush(Color.FromArgb(200, 0, green, blue));

                    orb.orbShape.Stroke = Brushes.White;
                    Canvas.SetLeft(orb.orbShape, orb.x - game.cameraCenter.X +
game.halfCameraWidth - orb.orbShape.Width / 2);
                    Canvas.SetTop(orb.orbShape, orb.y - game.cameraCenter.Y +
game.halfCameraHeight - orb.orbShape.Height / 2);
                }
            }

             //if all orbs have arrived AND boss is at start -> begin rotation
+ charging
             if (!ringFormed && allArrived && !bossMovingToStart)
             {
                 ringFormed = true;
                 //lock orbit angle to the side we chose earlier
                 bossOrbitAngleDeg = bossStartAngleDeg;
             }

             //if ring not formed yet, stop here
             if (!ringFormed)
                 return;

            //boss orbit (now allowed)
            bossOrbitAngleDeg = (bossOrbitAngleDeg + bossOrbitSpeedDeg) % 360;
            double bossRad = bossOrbitAngleDeg * Math.PI / 180.0;
            eyeBossX = ringCenterX + bossR * Math.Cos(bossRad);
            eyeBossY = ringCenterY + bossR * Math.Sin(bossRad);
            double face = Math.Atan2(ringCenterY - eyeBossY, ringCenterX -
eyeBossX) * 180 / Math.PI;
            if (face < 0) face += 360;
            UpdateImageRotation(face);

            //rotate orbs around ring center
            double stepRad = orbRotateSpeed * Math.PI / 180.0;
            foreach (var orb in orbitingOrbs)
            {
                if (!orb.isFired)
                {
                    //rotate their local offset
                    double lx = orb.localX * Math.Cos(stepRad) - orb.localY *
Math.Sin(stepRad);
                    double ly = orb.localX * Math.Sin(stepRad) + orb.localY *
Math.Cos(stepRad);
                    orb.localX = lx;
                    orb.localY = ly;
                    orb.x = ringCenterX + lx;
                    orb.y = ringCenterY + ly;
                }
                else
                {
                    orb.x += orb.velX;
                    orb.y += orb.velY;
                    if (!orb.OrbsHitPlayer)
                    {

                          Rect playerRect = game.myPlayer.Hitbox;

                          //find closest point on rect to circle center
                          double closestX = Math.Clamp(orb.x, playerRect.Left,
playerRect.Right);
                          double closestY = Math.Clamp(orb.y, playerRect.Top,
playerRect.Bottom);

                          //distance from circle center to that point
                          double dx = orb.x - closestX;
                          double dy = orb.y - closestY;
                          double distanceSq = dx * dx + dy * dy;

                          //compare to orb radius squared
                          double radius = orb.orbShape.Width / 2.0;
                          if (distanceSq < radius * radius)
                          {
                              game.myPlayer.ChangePlayerHealth(-orb.OrbDamage);
                              orb.OrbsHitPlayer = true;
                              if (orb.x < game.myPlayer.playerX)
                              {
                                  game.ApplyPlayerKnockback(1, 5);
                              }
                              else
                              {
                                  game.ApplyPlayerKnockback(-1, 5);
                              }
                          }
                      }

               }
           }

            //start one new orb charge periodically
            if (orbChargeDelay-- <= 0)
            {
                var available = orbitingOrbs.Where(o => !o.isCharging &&
!o.isFired).ToList();
                if (available.Count > 0)
                {
                    var pick = available[orbRand.Next(available.Count)];
                    pick.isCharging = true;
                    orbChargeDelay = (int)Math.Round(60 *
orbChargeSpeedMultiplier); //frames delay until next charge starts
                    orbChargeSpeedMultiplier -= (100 / orbCount) / 2 * 0.015;
                }
            }


           //update charging visuals + fire when ready
           for (int i = orbitingOrbs.Count - 1; i >= 0; i--)
           {
               var orb = orbitingOrbs[i];

               if (orb.isCharging && !orb.isFired)
               {
                   orb.chargeTimer++;
                   double p = Math.Min(orb.chargeTimer / orbFireInterval,
1.0);
                      double pulse = 0.6 + 0.4 * Math.Sin(time * 10.0);
                      double size = 28 * (1.0 + 0.35 * pulse * p);

                      byte r = (byte)(180 + 75 * pulse);
                      byte g = (byte)(30 * (1 - p));
                      byte b = (byte)(20 * (1 - p));
                      byte a = (byte)(200 + 55 * pulse);

                      orb.orbShape.Fill = new SolidColorBrush(Color.FromArgb(a,
r, g, b));
                    orb.orbShape.Stroke = new
SolidColorBrush(Color.FromArgb((byte)(a * 0.7), 255, 120, 80));
                    orb.orbShape.StrokeThickness = 3 + 2 * pulse;
                    orb.orbShape.Width = size;
                    orb.orbShape.Height = size;

                      if (orb.chargeTimer >= orbFireInterval)
                      {
                          orb.isCharging = false;
                          orb.isFired = true;
                          orb.chargeTimer = 0;

                         //aim at player
                         double dx = game.myPlayer.playerX - orb.x;
                         double dy = game.myPlayer.playerY - orb.y;
                         double len = Math.Sqrt(dx * dx + dy * dy);
                         if (len < 0.001) len = 0.001;
                         orb.velX = (dx / len) * currentSpeed *
orbSpeedMultiplier;
                         orb.velY = (dy / len) * currentSpeed *
orbSpeedMultiplier;
                    }
                }
                else if (!orb.isCharging && !orb.isFired)
                {
                    double glow = 0.5 + 0.5 * Math.Sin(time + i);
                    byte blue = (byte)(200 + 55 * glow);
                    byte green = (byte)(220 + 35 * glow);
                    orb.orbShape.Fill = new
SolidColorBrush(Color.FromArgb(200, 0, green, blue));
                    orb.orbShape.Stroke = Brushes.White;
                    orb.orbShape.Width = 20 + 2 * glow;
                    orb.orbShape.Height = 20 + 2 * glow;
                }

                //despawn fired off-screen


                if (orb.isFired && IsWorldPointOffscreen(game, orb.x, orb.y,
80))
               {
                   game.GameCanvas.Children.Remove(orb.orbShape);
                   orbitingOrbs.RemoveAt(i);
                   continue;
               }

                //draw
                Canvas.SetLeft(orb.orbShape, orb.x - game.cameraCenter.X +
game.halfCameraWidth - orb.orbShape.Width / 2);
                Canvas.SetTop(orb.orbShape, orb.y - game.cameraCenter.Y +
game.halfCameraHeight - orb.orbShape.Height / 2);
            }

           //when all fired move to hover
           if (orbitingOrbs.Count == 0 && !bossMovingToHover)
           {
               CleanupOrbs(game);
               orbsSpawned = false; //stop repeating phase
               currentState = EyeBossState.Hover;
           }
       }




        //animation + rotation
        private void UpdateAnimation()
        {
            if (eyeBossAnimationStopwatch.ElapsedMilliseconds >
MainWindow.WalkFrameInterval * 1.8)
            {
                if (isInStageOne)
                {
                    currentFrame = (currentFrame + 1) % stageOneFrames.Count;
                    CurrentImage = stageOneFrames[currentFrame];
                }
                else
                {
                    currentFrame = (currentFrame + 1) % stageTwoFrames.Count;
                    CurrentImage = stageTwoFrames[currentFrame];
                }

               eyeBossImageControl.Source = CurrentImage;
               eyeBossAnimationStopwatch.Restart();
           }
       }

       private void UpdateImageRotation(double angleDegrees)
       {
           angleDegrees -= 90; //image faces down by default

            eyeBossImageControl.RenderTransform = new
RotateTransform(angleDegrees, pivotImageX, pivotImageY);

           if (EyeBossRotationTransform != null)
           {
               EyeBossRotationTransform.Angle = angleDegrees;
               EyeBossRotationTransform.CenterX = pivotRectX;
               EyeBossRotationTransform.CenterY = pivotRectY;
           }
       }

   }


    public partial class MainWindow
    {
        private EyeBoss currentEyeBoss = null;
        private systemShapes.Rectangle EyeBossHitboxRectangle;
        private systemShapes.Rectangle EyeBossCenterHitboxRectangle;
        private void SpawnEyeBossSubroutine((double, double) XYs)
        {
            var stage1Frames = EnemyAssetLoader.GetAnimation($"EyeStage1");
            var stage2Frames = EnemyAssetLoader.GetAnimation($"EyeStage2");
            var eyeBoss = new EyeBoss(XYs.Item1, XYs.Item2 - 100,
stage1Frames, stage2Frames);
            currentEyeBoss = eyeBoss;
            Canvas.SetZIndex(eyeBoss.eyeBossImageControl, 8);
            GameCanvas.Children.Add(eyeBoss.eyeBossImageControl);

           EyeBossHitboxRectangle = new systemShapes.Rectangle
           {
               Stroke = Brushes.Blue,
               StrokeThickness = 2,
               Width = eyeBoss.Hitbox.Width,
               Height = eyeBoss.Hitbox.Height,
               Fill = Brushes.Transparent,
               Tag = "Hitbox"
           };

            eyeBoss.thisEyeBossHitboxRectangle = EyeBossHitboxRectangle;
            GameCanvas.Children.Add(eyeBoss.thisEyeBossHitboxRectangle);
            Canvas.SetZIndex(eyeBoss.thisEyeBossHitboxRectangle, 9);
            eyeBoss.thisEyeBossHitboxRectangle.Visibility = showHitboxes ?
Visibility.Visible : Visibility.Collapsed;

            eyeBoss.EyeBossRotationTransform = new RotateTransform(0);
            eyeBoss.thisEyeBossHitboxRectangle.RenderTransform =
eyeBoss.EyeBossRotationTransform;

           EyeBossCenterHitboxRectangle = new systemShapes.Rectangle
           {
               Stroke = Brushes.Blue,
               StrokeThickness = 6,

                Width = eyeBoss.eyeBossHitboxCenter.Width,
                Height = eyeBoss.eyeBossHitboxCenter.Height,
                Tag = "Hitbox"
           };

            eyeBoss.thisEyeBossCenterHitboxRectangle =
EyeBossCenterHitboxRectangle;
            GameCanvas.Children.Add(eyeBoss.thisEyeBossCenterHitboxRectangle);
            Canvas.SetZIndex(eyeBoss.thisEyeBossCenterHitboxRectangle, 9);
            eyeBoss.thisEyeBossCenterHitboxRectangle.Visibility = showHitboxes
? Visibility.Visible : Visibility.Collapsed;
        }

        private void SpawnAEyeBoss()
        {
            Random rand = new Random();
            int side = rand.Next(0,2);
            int multiplyingFactor = -1;
            //out of bounds
            if (-1 * CameraWidth * Utilities.TileSize + myPlayer.playerX <= 0)
            {
                multiplyingFactor = 1;
            }
            else if (1 * CameraWidth * Utilities.TileSize + myPlayer.playerX
>= newMapWidth * Utilities.TileSize)
            {
                multiplyingFactor = -1;
            }
            else //else normal random
            {
                if (side == 0)
                {
                    multiplyingFactor = 1;
                }
            }

            SpawnEyeBossSubroutine(((myPlayer.playerX + (multiplyingFactor *
(CameraWidth * Utilities.TileSize))), (myPlayer.playerY - 75)));

       }
       public bool EyeBossIsDead = false;
       private void UpdateEyeBoss()
       {
           if (currentEyeBoss != null)
           {
               currentEyeBoss.UpdateAI(this);
               UpdateEyeBossHitboxDisplay();
               DetectEyeBossHitPlayer(currentEyeBoss);

                if (EyeBossIsDead)
                {
                    RemoveEyeBoss(currentEyeBoss);


                    //drops
                    Random rand = new Random();
                    int randomNumber1 = rand.Next(3, 7); //-> 3 to 6
                    myInventory.AddItem(new HealingPotionItem(),
randomNumber1); //healing potions
                    int radnomNumber2 = rand.Next(10, 21); // 10 - 20
                    myInventory.AddItem(new LesserHealingPotionItem(),
radnomNumber2); //lesser healing potion
                    int randomNumber3 = rand.Next(50, 101); //-> 50 to 100
                    myInventory.AddItem(new GelItem(), randomNumber3); //gel
                    int randomNumber4 = rand.Next(50, 101); // 50 to 100
                    myInventory.AddItem(new PlatinumOreItem(), randomNumber4);
//platinum ore
                    EyeBossIsDead = false; //return to false
                }
            }
        }

       private void RemoveEyeBoss(EyeBoss eyeBoss)
       {
           GameCanvas.Children.Remove(eyeBoss.eyeBossImageControl);
           GameCanvas.Children.Remove(eyeBoss.eyeBossHealthBar);
           GameCanvas.Children.Remove(eyeBoss.thisEyeBossHitboxRectangle);

GameCanvas.Children.Remove(eyeBoss.thisEyeBossCenterHitboxRectangle);
            currentEyeBoss = null;
        }

        private void DetectEyeBossHitPlayer(EyeBoss eyeBoss)
        {
            //get world corners of arrows rotated hitbox
            Point[] eyeBossCorners =
GetEyeBossHitboxCornersWorld(currentEyeBoss);

            if (eyeBoss.hitPlayerStopwatch.ElapsedMilliseconds >= 600)
            {
                if (RotatedRectIntersectsAABB(eyeBossCorners,
myPlayer.Hitbox))
                {
                    //player hit
                    eyeBoss.hitPlayerStopwatch.Restart();
                    myPlayer.ChangePlayerHealth(-eyeBoss.Damage);

                   //knockback
                   if (myPlayer.playerX < eyeBoss.eyeBossX)
                   {
                       ApplyPlayerKnockback(-1, 8);
                   }
                   else
                   {
                       ApplyPlayerKnockback(1, 8);
                   }
               }

           }
       }
       private Point[] GetEyeBossHitboxCornersWorld(EyeBoss eyeBoss)
       {
           double eyeBossWorldX = eyeBoss.eyeBossX;
           double eyeBossWorldY = eyeBoss.eyeBossY;
           double eyeBossWidth = eyeBoss.Hitbox.Width;
           double eyeBossHeight = eyeBoss.Hitbox.Height;

            //rotation (in radians)
            double angle = currentEyeBoss.EyeBossRotationTransform.Angle *
Math.PI / 180;

           //local corner offsets from center
           Vector[] corners =
           {
               new Vector(-eyeBossWidth/2, -eyeBossHeight/2),
               new Vector(eyeBossWidth/2, -eyeBossHeight/2),
               new Vector(eyeBossWidth/2, eyeBossHeight/2),
               new Vector(-eyeBossWidth/2, eyeBossHeight/2)
           };

            Point[] worldCorners = new Point[4];
            for (int i = 0; i < 4; i++)
            {
                double rotatedX = corners[i].X * Math.Cos(angle) -
corners[i].Y * Math.Sin(angle);
                double rotatedY = corners[i].X * Math.Sin(angle) +
corners[i].Y * Math.Cos(angle);
                worldCorners[i] = new Point(eyeBossWorldX + rotatedX,
eyeBossWorldY + rotatedY);
            }

            return worldCorners;
       }

        private bool RotatedRectIntersectsRotatedRect(Point[] cornersA,
Point[] cornersB) //use 1d line and all overlap
        {
            if (cornersA == null || cornersB == null || cornersA.Length != 4
|| cornersB.Length != 4) return false; //check valid input

           //build edges (lines) from both rotated rects
           List<Vector> axes = new List<Vector>();

           for (int i = 0; i < 4; i++) //rect A edges
           {
               Vector edge = cornersA[(i + 1) % 4] - cornersA[i];
               Vector normal = new Vector(-edge.Y, edge.X); //perpendicular
axis
               if (normal.Length > 0.0001) //avoid zero vector
               {
                   normal.Normalize(); //make unit length

                      axes.Add(normal); //add to axis list
                  }
              }

              for (int i = 0; i < 4; i++) //rect B edges
              {
                  Vector edge = cornersB[(i + 1) % 4] - cornersB[i];
                  Vector normal = new Vector(-edge.Y, edge.X); //perpendicular
axis
                  if (normal.Length > 0.0001) //avoid zero vector
                  {
                      normal.Normalize(); //make unit length
                      axes.Add(normal); //add to axis list
                  }
              }

            foreach (var axis in axes) //check each axis
            {
                //project rect A onto axis
                double minRectA = double.MaxValue, maxRectA = double.MinValue;
                foreach (var point in cornersA)
                {
                    double projection = (point.X * axis.X) + (point.Y *
axis.Y); //dot product
                    minRectA = Math.Min(minRectA, projection); //lowest value
                    maxRectA = Math.Max(maxRectA, projection); //highest value
                }

                //project rect b onto axis
                double minRectB = double.MaxValue, maxRectB = double.MinValue;
                foreach (var point in cornersB)
                {
                    double projection = (point.X * axis.X) + (point.Y *
axis.Y); //dot product
                    minRectB = Math.Min(minRectB, projection); //lowest value
                    maxRectB = Math.Max(maxRectB, projection); //highest value
                }

                  if (maxRectA < minRectB || maxRectB < minRectA) //gap between
shadows
                  {
                      return false; //no collision
                  }
              }
              return true; //all axes overlap -> collision
          }
    }
}
