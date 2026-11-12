// Enemy.cs
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using systemShapes = System.Windows.Shapes;

namespace WPFTerraria
{
    public abstract class Enemy
    {
        public double enemyX { get; set; }
        public double enemyY { get; set; }
        public int _enemyMaxHealth { get; set; }
        public int _enemyHealth { get; set; }
        public double Speed { get; set; }
        public BitmapSource CurrentImage { get; set; }
        public Image enemyImageControl { get; set; }
        protected Stopwatch enemyAnimationStopwatch = new Stopwatch();
        public Stopwatch hitPlayerStopwatch = new Stopwatch(); //prevents
multiple hits in ticks
        public Stopwatch enemyBeenHit = new Stopwatch();
        public Stopwatch despawnEnemyStopwatch = new Stopwatch();
        public double halfImageWidth;
        public double halfImageHeight;
        public bool enemyIsGrounded = false;
        protected double velocityY;
        protected bool loadOnced = false;
        protected bool ApplyingEnemyKnockback = false;
        public int KnockbackDirection = 0;
        public EnemyHealthBar enemyHealthBar;
        protected List<UIElement> debugPathVisuals = new List<UIElement>();
//debug draw storage
        public bool enemyToDespawn = false;

       public Enemy(double x, double y, BitmapSource image)
       {
           enemyX = x;
           enemyY = y;
           CurrentImage = image;
           enemyImageControl = new Image
           {
               Source = image,
               Width = Utilities.TileSize * 2 - 2,
               Height = image.Height * 1.5
           };

           enemyAnimationStopwatch.Start();
           halfImageWidth = enemyImageControl.Width / 2;
           halfImageHeight = enemyImageControl.Height / 2;
           hitPlayerStopwatch.Start();
           enemyBeenHit.Start();
           enemyHealthBar = new EnemyHealthBar();
           var main = Application.Current.MainWindow as MainWindow;
           main.GameCanvas.Children.Add(enemyHealthBar);
           Canvas.SetZIndex(enemyHealthBar, 9);
       }

       public Rect Hitbox
       {
           get
           {
               return new Rect(
                   enemyX - (halfImageWidth),
                   enemyY - (halfImageHeight),
                   enemyImageControl.Width,
                   enemyImageControl.Height
               );
           }
       }

       public Rect enemyHitboxCenter
       {
           get
           {
               return new Rect(
                   enemyX - 2.5,
                   enemyY - 2.5,
                   5,
                   5
               );
           }
       }

       public systemShapes.Rectangle thisEnemiesHitboxRectangle;
       public systemShapes.Rectangle thisEnemiesCenterHitboxRectangle;

        public void CheckIfEnemyStillGrounded(MainWindow game)
        {
            if (enemyIsGrounded)
            {
                var groundCheck = game.CheckCollisonWithTiles(enemyX, enemyY +
1, 1, Hitbox.Width, Hitbox.Height);
                if (groundCheck == null)
                {
                    enemyIsGrounded = false;
                    velocityY = 0;
                }
            }
        }

         public void ApplyEnemyKnockback(int knockbackDirection)
         {
             velocityY = -12; //even if jumping still apply
             enemyIsGrounded = false;
             ApplyingEnemyKnockback = true;
             KnockbackDirection = knockbackDirection;
         }

        protected bool EnemyInsideActiveWindow(Enemy enemy, MainWindow game)
        {
            //recompute exact chunk window -> used same as
UpdateVisibleTiles()
            int cameraTileX = (int)(game.cameraCenter.X / Utilities.TileSize);
            int cameraTileY = (int)(game.cameraCenter.Y / Utilities.TileSize);

            int chunkStartX = (cameraTileX - (MainWindow.CameraWidth / 2) - 2)
/ game.chunkSize;
            int chunkEndX = (cameraTileX + (MainWindow.CameraWidth / 2) + 2) /
game.chunkSize;

            int chunkStartY = (cameraTileY - (MainWindow.CameraHeight / 2) - 3
+ 1) / game.chunkSize;
            int chunkEndY = (cameraTileY + (MainWindow.CameraHeight / 2) + 3)
/ game.chunkSize;

            chunkStartX = Math.Clamp(chunkStartX, 0, game.newMapWidth /
game.chunkSize);
            chunkEndX = Math.Clamp(chunkEndX, 0, game.newMapWidth /
game.chunkSize);
            chunkStartY = Math.Clamp(chunkStartY, 0, game.newMapHeight /
game.chunkSize);
            chunkEndY = Math.Clamp(chunkEndY, 0, game.newMapHeight /
game.chunkSize);
            //convert enemys hitbox (not js center) into chunk range
            double left = enemy.enemyX - enemy.halfImageWidth;
            double right = enemy.enemyX + enemy.halfImageWidth;
            double top = enemy.enemyY - enemy.halfImageHeight;
            double bottom = enemy.enemyY + enemy.halfImageHeight;

            int minChunkX = ((int)Math.Floor(left / Utilities.TileSize)) /
game.chunkSize;
            int maxChunkX = ((int)Math.Floor(right / Utilities.TileSize)) /
game.chunkSize;
            int minChunkY = ((int)Math.Floor(top / Utilities.TileSize)) /
game.chunkSize;
            int maxChunkY = ((int)Math.Floor(bottom / Utilities.TileSize)) /
game.chunkSize;

             //if any part of enemy is outside the active chunk window ->
unload
             bool insideX = minChunkX >= chunkStartX && maxChunkX <= chunkEndX;


            bool insideY = minChunkY >= chunkStartY && maxChunkY <= chunkEndY;
            return insideX && insideY;

        }
        public void ChangeEnemyHealth(int healthChange)
        {
            int newHealth = Math.Min(_enemyMaxHealth, (Math.Max(0,
(_enemyHealth + healthChange))));
            enemyHealthBar.EnemyHealth = newHealth;
            _enemyHealth = newHealth;


       }
       protected void ChangeEnemyMaxHealth(int maxHealthChange)
       {
           enemyHealthBar.EnemyMaxHealth = _enemyMaxHealth + maxHealthChange;
           _enemyMaxHealth = _enemyMaxHealth + maxHealthChange;
       }
       public void DeleteDebugPath(MainWindow game)
       {
           //remove all previously drawn debug shapes (dots + lines)
           foreach (var e in debugPathVisuals)
           {
               game.GameCanvas.Children.Remove(e);
           }
       }

        public abstract void UpdateAI(MainWindow game);
   }

   public class Zombie : Enemy
   {
       private List<BitmapSource> AnimationFrames;
       private int currentFrame;

        private int ZombieMaxHealth = 100;
        private int ZombieSpeed = 2;
        private int JumpStrenth = -16;

        private readonly int enemyWidthTiles = 2; //tiles wide
        private readonly int enemyHeightTiles = 3; //tiles tall
        private readonly int maxJumpUpTiles = 5; //max jump tiles
        private readonly int searchMarginTiles = 24; //search width tiles
        private readonly int searchMarginYTiles = 16; //search height tiles
        private readonly Stopwatch pathStopwatch = new Stopwatch(); //timer
for path updates
        private readonly Stopwatch runAway = new Stopwatch(); //timer when no
path then runs away
        private readonly TimeSpan repathInterval =
TimeSpan.FromMilliseconds(350); //time between paths
        private List<Node> currentPath = new List<Node>(); //current path list
        private int currentWaypointIndex = 0; //index on path


          private Point lastPathGoal = new Point(double.NaN, double.NaN); //last
target



        private class Node //represents one walkable point on the floor graph
        {
            public int tileXIndex; //tile x index
            public int groundTileRow; //tile y index
            public double worldCenterX; //center x world
            public double worldCenterY; //center y world
            public List<Edge> neighbours = new List<Edge>(); //neighbor edges
            public override int GetHashCode() => (tileXIndex * 73856093) ^
(groundTileRow * 19349663); //custom hash to store in dictionary quickly
        }

          private class Edge //represents a single connection from one node to
another
          {
            public Node nodePointsTo; //the node this edge points to (where
you end up if you follow this connection)
            public double cost; //numeric cost of travelling this edge (used
by a* to find cheapest path, bigger cost = less preferred)
            public MoveKind movementKind; //type of movement needed to
traverse this edge (walk, jump, fall, stepUp) so ai can animate properly
        }

          private enum MoveKind { Walk, StepUp, Jump, Fall } //types of move

        private class OpenItem //represents one entry in the a* open list
        {
            public Node node; //the node (position) this entry represents
            public double f; //the total score for this node (f = g + h) lower
is better, used to pick next best node
            public double g; //the cost to get from start node to this node so
far (used to prefer shorter known paths)
            public Node parent; //the node we came from when reaching this
node (used later to reconstruct the final path)
        }


          public Zombie(double x, double y, List<BitmapSource> frames)
              : base(x, y, frames[0])
          {
              AnimationFrames = frames;
              currentFrame = 0;
              enemyAnimationStopwatch.Start();

              _enemyMaxHealth = ZombieMaxHealth; //set max health
              ChangeEnemyMaxHealth(0); //call set max health
              _enemyHealth = ZombieMaxHealth; //set health
              ChangeEnemyHealth(0); //call set health


           Speed = ZombieSpeed; //set speed
           pathStopwatch.Start(); //start path timer
       }

       public override void UpdateAI(MainWindow game)
       {
           double enemyCurrentX = enemyX; //cache x
           double enmeyCurrentY = enemyY; //cache y

           DeleteDebugPath(game);

            if (loadOnced)
            {
                if (!EnemyInsideActiveWindow(this, game))
                {
                    if (!despawnEnemyStopwatch.IsRunning)
despawnEnemyStopwatch.Start();
                    if (despawnEnemyStopwatch.ElapsedMilliseconds > 20000)
//if outside active window for 20 seconds despawn
                    {
                        enemyToDespawn = true;
                        return; //exit update
                    }
                    return; // freeze enemy update until area reloads
                }
                else
                {
                    despawnEnemyStopwatch.Reset();
                    despawnEnemyStopwatch.Stop();
                }

           }
           else loadOnced = true;

           DrawDebugPath(game); //draw path debug

            if (!enemyIsGrounded) //if not grounded
            {
                if (velocityY < Utilities.TileSize - (MainWindow.gravity *
1.5)) //apply gravity
                {
                    velocityY += MainWindow.gravity;
                }

                var collidingTiles =
game.CheckCollisonWithTiles(enemyCurrentX, enmeyCurrentY + velocityY, 1,
Hitbox.Width, Hitbox.Height); //check collision below
                if (collidingTiles != null) //if hit tile
                {
                    var blockingTile = collidingTiles[0]; //get tile
                    double tileYTop = Canvas.GetTop(blockingTile) +
game.cameraCenter.Y - (game.GameCanvas.Height / 2); //tile top world y

                    if (velocityY > 0) //falling down
                    {
                        enemyY = tileYTop - (Hitbox.Height / 2); //snap to top
                        velocityY = 0; //stop fall
                        enemyIsGrounded = true; //set grounded
                    }
                    else
                    {
                        enemyY = tileYTop + blockingTile.Height +
(Hitbox.Height / 2); //head bump
                        velocityY = 0; //stop jump
                    }
                }
                else
                {
                    enemyY += velocityY; //apply fall
                    enemyIsGrounded = false; //still air
                }
            }

            double _playerX = game.myPlayer.playerX; //player x -> dont have
to refernece every time
            double _playerY = game.myPlayer.playerY; //player y

            double enemyBottomY = enmeyCurrentY + Hitbox.Height / 2; //enemy
bottom y
            double playerBottomY = _playerY + game.myPlayer.Hitbox.Height / 2;
//player bottom y

            bool needRepath = false; //flag repath
            if (double.IsNaN(lastPathGoal.X) || Math.Abs(lastPathGoal.X -
_playerX) > Utilities.TileSize * 1.5 ||
                Math.Abs(lastPathGoal.Y - _playerY) > Utilities.TileSize *
1.5) //if player moved
            {
                needRepath = true;
            }
            if (currentPath == null || currentPath.Count == 0 ||
currentWaypointIndex >= currentPath.Count) //no path
            {
                needRepath = true;
            }
            if (pathStopwatch.Elapsed > repathInterval) //time expired
            {
                needRepath = true;
            }

            if (needRepath) //build new path
            {
                currentPath = BuildAndFindPath(game, enemyCurrentX,
enemyBottomY, _playerX, playerBottomY); //compute path
                currentWaypointIndex = 0; //reset index


                  lastPathGoal = new Point(_playerX, _playerY); //store goal for
if need to redo
                  pathStopwatch.Restart(); //reset timer
           }

            if (currentPath != null && currentPath.Count > 0 &&
currentWaypointIndex < currentPath.Count) //if have path
            {
                Node waypointNode = currentPath[currentWaypointIndex];
//current node
                double targetX = waypointNode.worldCenterX; //target x
                double targetY = waypointNode.worldCenterY; //target y

                  double enemyStartX = enemyX - (Hitbox.Width / 2); //left x
                  double enemyEndX = enemyX + (Hitbox.Width / 2); //right x

                  if (enemyStartX <= targetX && enemyEndX >= targetX)
                  {
                      currentWaypointIndex++;
                      if (currentWaypointIndex < currentPath.Count)
                      {
                          waypointNode = currentPath[currentWaypointIndex];
//update target
                          targetX = waypointNode.worldCenterX;
                          targetY = waypointNode.worldCenterY;
                      }
                  }

                  double dx = targetX - enemyX; //delta x
                  int dir = dx > 0 ? 1 : -1; //left or right

                  double step = Speed; //move step
                  double newX = enemyX + dir * step; //candidate x
                  double newY = enemyY; //y stays

                var collided = game.CheckCollisonWithTiles(newX, newY, 2,
Hitbox.Width, Hitbox.Height); //check forward collision
                if (collided == null) //if free
                {
                    enemyX = newX; //move
                    if ((dir > 0 &&
enemyImageControl.LayoutTransform.Value.M11 > 0) ||
                    (dir < 0 && enemyImageControl.LayoutTransform.Value.M11 <
0))
                    {
                        // Note the minus sign — fixes wrong-way flip
                        enemyImageControl.LayoutTransform = new
ScaleTransform(-dir, 1);
                    }
                    CheckIfEnemyStillGrounded(game); //check grounded
                }
                else


               {
                    bool stepped = false; //flag
                    if (enemyIsGrounded && collided.Length == 1) //if small
obstacle
                   {
                        var blockingTile = collided[0];
                        double blockingTileY = Canvas.GetTop(blockingTile) +
game.cameraCenter.Y - (game.GameCanvas.Height / 2);
                        if (blockingTileY > Hitbox.Bottom - Utilities.TileSize
- 1) //if stepable
                        {
                            if (game.CheckCollisonWithTiles(enemyX, enemyY -
Utilities.TileSize, 1, Hitbox.Width, Hitbox.Height) == null)
                            {
                                if (game.CheckCollisonWithTiles(newX, enemyY -
Utilities.TileSize, 1, Hitbox.Width, Hitbox.Height) == null)
                                {
                                    enemyY = blockingTileY - (Hitbox.Height /
2); //step up
                                    enemyX = newX;
                                    stepped = true;
                                }
                            }
                        }
                    }
                    if (!stepped) //else jump
                    {
                        if (enemyIsGrounded && targetY + Utilities.TileSize *
0.25 < enemyBottomY)
                        {
                            velocityY = JumpStrenth;
                            enemyIsGrounded = false;
                        }

                   }
               }
               runAway.Stop();
            }
            else
            {
                if (!runAway.IsRunning)
                {
                    runAway.Restart();
                }
                if (runAway.ElapsedMilliseconds < 6000) //before 6 seconds
chase player
                {
                    int dir = (game.myPlayer.playerX > enemyX) ? 1 : -1;
//chase player
                    double step = Speed;
                    var collided = game.CheckCollisonWithTiles(enemyX + dir *
step, enemyY, 2, Hitbox.Width, Hitbox.Height);
                    if (collided == null)

                   {
                       enemyX += dir * step; //move
                       CheckIfEnemyStillGrounded(game);
                    }
                    if ((dir > 0 &&
enemyImageControl.LayoutTransform.Value.M11 > 0) ||
                    (dir < 0 && enemyImageControl.LayoutTransform.Value.M11 <
0))
                    {
                        // Note the minus sign — fixes wrong-way flip
                        enemyImageControl.LayoutTransform = new
ScaleTransform(-dir, 1);
                    }
                }
                else //after 6 seconds run away and jump
                {
                    int dir = (game.myPlayer.playerX > enemyX) ? -1 : 1; //go
away from player
                    double step = Speed;
                    var collided = game.CheckCollisonWithTiles(enemyX + dir *
step, enemyY, 2, Hitbox.Width, Hitbox.Height);
                    if (collided == null)
                    {
                        enemyX += dir * step; //move
                        CheckIfEnemyStillGrounded(game);
                    }
                    else if (enemyIsGrounded)
                    {
                        velocityY = JumpStrenth; //jump
                        enemyIsGrounded = false;
                    }
                    if ((dir > 0 &&
enemyImageControl.LayoutTransform.Value.M11 > 0) ||
                    (dir < 0 && enemyImageControl.LayoutTransform.Value.M11 <
0))
                    {
                        // Note the minus sign — fixes wrong-way flip
                        enemyImageControl.LayoutTransform = new
ScaleTransform(-dir, 1);
                    }
                }

           }



            if (enemyAnimationStopwatch.ElapsedMilliseconds >
MainWindow.WalkFrameInterval *
                ((double)ZombieSpeed / (double)game.myPlayer.playerSpeed))
//animate
            {



               currentFrame = (currentFrame + 1) % AnimationFrames.Count;
//next frame
               CurrentImage = AnimationFrames[currentFrame]; //set image
               enemyImageControl.Source = CurrentImage;
               enemyAnimationStopwatch.Restart();
           }
           if (ApplyingEnemyKnockback)
           {
               if (enemyIsGrounded)
               {
                   ApplyingEnemyKnockback = false;

                }
                else
                {
                    systemShapes.Rectangle[] collidedTiles =
game.CheckCollisonWithTiles(enemyX + (5 * KnockbackDirection), enemyY, 1,
Hitbox.Width, Hitbox.Height); //only create if moves to save proccesing power
when still
                    if (collidedTiles != null)
                    {
                        if (KnockbackDirection == -1) //going left
                        {
                            enemyX = Canvas.GetLeft(collidedTiles[0]) +
game.cameraCenter.X - (game.GameCanvas.Width / 2) + collidedTiles[0].Width +
(Hitbox.Width / 2);

                        }
                        else
                        {
                            enemyX = Canvas.GetLeft(collidedTiles[0]) +
game.cameraCenter.X - (game.GameCanvas.Width / 2) - (Hitbox.Width / 2);
                        }
                    }
                    else
                    {
                        enemyX += 5 * KnockbackDirection;
                    }
                }
            }


            Canvas.SetLeft(enemyImageControl, enemyX - game.cameraCenter.X +
game.halfCameraWidth - halfImageWidth); //set x pos
            Canvas.SetTop(enemyImageControl, enemyY - game.cameraCenter.Y +
game.halfCameraHeight - halfImageHeight + 2); //set y pos
            Canvas.SetLeft(enemyHealthBar, enemyX - game.cameraCenter.X +
game.halfCameraWidth - (enemyHealthBar.HealthBarWidth / 2)); //set health bar
x pos
            Canvas.SetTop(enemyHealthBar, enemyY - game.cameraCenter.Y +
game.halfCameraHeight - halfImageHeight - enemyHealthBar.HealthBarHeight *
1.8); //set health bar y pos
        }

        private List<Node> BuildAndFindPath(MainWindow game, double enemyX,
double enemyBottomY, double playerX, double playerBottomY)
        {
            int tileSize = Utilities.TileSize; //so dont need to write
utulities each time
            double minX = Math.Max(tileSize, Math.Min(enemyX, playerX) -
searchMarginTiles * tileSize); //left bound
            double maxX = Math.Min(game.newMapWidth * tileSize - tileSize,
Math.Max(enemyX, playerX) + searchMarginTiles * tileSize); //right bound
            double minY = Math.Max(enemyHeightTiles * tileSize,
Math.Min(enemyBottomY, playerBottomY) - searchMarginYTiles * tileSize); //top
bound
            double maxY = Math.Min(game.newMapHeight * tileSize - tileSize,
Math.Max(enemyBottomY, playerBottomY) + searchMarginYTiles * tileSize);
//bottom bound

            int minTileX = Math.Max(0, (int)Math.Floor(minX / tileSize) - 1);
//left tile index
            int maxTileX = Math.Min(game.newMapWidth - 2, (int)Math.Floor(maxX
/ tileSize) - 1); //right tile index

            Dictionary<(int fx, int gRow), Node> nodes = new Dictionary<(int,
int), Node>(); //nodes map of all nodes
            for (int thisTileX = minTileX; thisTileX <= maxTileX; thisTileX++)
//scan columns
            {
                double thisWorldCenterX = (thisTileX + 0.5) * tileSize;
//center of tile column

                int startRow = Math.Max(0, (int)Math.Floor(minY / tileSize));
//top row
                int endRow = Math.Min(game.newMapHeight - 1,
(int)Math.Floor(maxY / tileSize)); //bottom row
                for (int thisYLevel = startRow; thisYLevel <= endRow;
thisYLevel++) //scan rows
                {
                    double worldYFeet = (thisYLevel + 1) * tileSize; //tile
top y where feet would stand
                    double worldEnemyCenterY = worldYFeet - (Hitbox.Height /
2.0); //center y if feet on that tile

                    if (worldYFeet < 0 || worldYFeet > game.newMapHeight *
tileSize) continue; //skip out of world
                                                                6D loop connected ->
                    if (IsStandable(game, thisWorldCenterX,
                                                               IsStandable
worldEnemyCenterY)) //check space using center y
                    {
                        var key = (thisTileX, thisYLevel); //node key
                        if (!nodes.ContainsKey(key)) //add node once
                            nodes[key] = new Node
                            {
                                tileXIndex = thisTileX, //tile x index

                                 groundTileRow = thisYLevel, //tile y index
                                 worldCenterX = thisWorldCenterX, //center x
world
                                 worldCenterY = worldYFeet //store feet y so
path sits on ground
                            };
                      }
                }
            }
            if (nodes.Count == 0) return new List<Node>(); //no nodes means no
path -> return empty
                                                                           4D loop
            foreach (var keyvalue in nodes.ToList()) //link neighbors    when
(keyvalue-> as dictionary)
                                                                         included in
            {
                Node thisNode = keyvalue.Value; //from node              game loop
                foreach (int direction in new int[] { -1, 1 }) //left and
right
                {
                    int neigbourTileXIndex = thisNode.tileXIndex + direction;
//neighbor column (x index)
                    for (int changeInHeight = -maxJumpUpTiles; changeInHeight
<= maxJumpUpTiles * 6; changeInHeight++) //vertical offsets -> jump up 5 to
fall down 25
                    {
                        int neighbourGroundTileRow = thisNode.groundTileRow +
changeInHeight; //neighbor row
                        var nKey = (neigbourTileXIndex,
neighbourGroundTileRow); //neighbor key
                        if (!nodes.ContainsKey(nKey)) continue; //must exist
                        Node neighbourNode = nodes[nKey]; //to node

                        double distanceBetweenY = neighbourNode.worldCenterY -
thisNode.worldCenterY; //feet height delta
                        double absDifferenceY = Math.Abs(distanceBetweenY);
//abs delta
                        double cost = 1.0 + (absDifferenceY / tileSize) *
0.35; //base cost -> not same as walking 1 tile

                        MoveKind movementKind; //move type
                        if (absDifferenceY <= tileSize * 0.6) movementKind =
MoveKind.Walk; //flatish
                        else if (distanceBetweenY < 0 && absDifferenceY <=
maxJumpUpTiles * tileSize) //going up
                        {
                            movementKind = (absDifferenceY <= tileSize * 1.2)
? MoveKind.StepUp : MoveKind.Jump; //small step vs jump
                            cost += 1.0 + (Math.Max(0, (-distanceBetweenY) /
tileSize) * 0.25); //extra jump cost
                        }
                        else if (distanceBetweenY > 0) //going down
                        {

                             movementKind = MoveKind.Fall; //fall move
                             cost += (distanceBetweenY / tileSize) * 0.15;
//small down cost
                         }
                         else continue; //skip invalid

                        thisNode.neighbours.Add(new Edge { nodePointsTo =
neighbourNode, cost = cost, movementKind = movementKind }); //add edge
                    }
                }
            }

            Node startNode = FindClosestNode(nodes.Values, enemyX,
enemyBottomY); //start near enemy feet
            Node goalNode = FindClosestNode(nodes.Values, playerX,
playerBottomY); //goal near player feet
            if (startNode == null || goalNode == null) return new
List<Node>(); //no endpoints -> return empty lift

            return AStar(nodes.Values, startNode, goalNode); //run a*
        }

        private bool IsStandable(MainWindow game, double enemyCenterX, double
enemyCenterY)
        {
            var free = game.CheckCollisonWithTiles(enemyCenterX, enemyCenterY,
1, Hitbox.Width, Hitbox.Height) == null;
            if (!free) return false;                               6D loop connected within
            var ground = game.CheckCollisonWithTiles(enemyCenterX, checkcollisonwithKles
enemyCenterY + 1, 1, Hitbox.Width, Hitbox.Height) != null;
            return ground; //is ground and nothing at feet
        }

        private Node FindClosestNode(IEnumerable<Node> nodes, double x, double
y)
        {
            Node best = null; //the closest node found so far
            double bestD = double.MaxValue; //smallest distance found so far,
start with infinity
            foreach (var thisNode in nodes)
            {
                double distanceToNodeX = thisNode.worldCenterX - x;
                double distanceToNodeY = thisNode.worldCenterY - y;
                double totalD = Math.Abs(distanceToNodeX) +
Math.Abs(distanceToNodeY) * 1.2; //y more unpreferrable therefore weight it
more
                if (totalD < bestD)
                {
                    bestD = totalD;
                    best = thisNode;
                }
                //doesnt use cost as js picking nearsest point to do


             }
             return best;
         }

         private List<Node> AStar(IEnumerable<Node> allNodes, Node start, Node
goal)
         {
            var gScore = new Dictionary<Node, double>(); //stores best known
cost to reach each node from start
            var fScore = new Dictionary<Node, double>(); //stores estimated
total cost (g + heuristic) for each node -> used for next node to check
            var parent = new Dictionary<Node, Node>(); //stores where we came
from so we can reconstruct path later
            var open = new List<OpenItem>(); //list of nodes to explore
(priority queue behavior)

             //initialise all nodes with infinite cost since we haven't visited
them
             foreach (var n in allNodes)
             {
                 gScore[n] = double.PositiveInfinity; //no path to n yet
                 fScore[n] = double.PositiveInfinity; //total cost unknown
             }

            //start node has cost 0 since we are already there
            gScore[start] = 0;
            fScore[start] = Heuristic(start, goal); //estimated total cost =
just heuristic distance for now
            open.Add(new OpenItem { node = start, g = 0, f = fScore[start],
parent = null }); //add to open list

             var closed = new HashSet<Node>(); //nodes we've already fully
processed

             //main search loop: keep going while there are nodes to explore
             while (open.Count > 0)
             {
                 //sort open list by lowest f (most promising path first)
                 open.Sort((a, b) => a.f.CompareTo(b.f)); //order
                 var current = open[0]; //pick the best candidate
                 open.RemoveAt(0); //remove it from open list

                //if this node is the goal, reconstruct the path and return
                if (current.node == goal)
                {
                    parent[current.node] = current.parent; //store final
parent
                     return ReconstructPath(parent, goal); //build path from
start to goal
                }

                 //skip if we already processed this node

                  if (closed.Contains(current.node)) continue;

                  //mark this node as done and remember its parent
                  closed.Add(current.node);
                  parent[current.node] = current.parent;

                //check all neighbors (edges from this node)
                foreach (var edge in current.node.neighbours)
                {
                    if (closed.Contains(edge.nodePointsTo)) continue; //skip
already processed node goes to

                    //calculate cost to neighbour through this node ->
tenetaive meanes not final but tmepory
                    double tentativeG = gScore[current.node] + edge.cost;

                      //if this is a cheaper way to reach neighbor, update
records
                      if (tentativeG < gScore[edge.nodePointsTo])
                      {
                          gScore[edge.nodePointsTo] = tentativeG; //save better
g
                        fScore[edge.nodePointsTo] = tentativeG +
Heuristic(edge.nodePointsTo, goal); //recalculate total predicted cost to find
next to search

                          //add neighbor to open list with updated scores
                          open.Add(new OpenItem
                          {
                              node = edge.nodePointsTo, //which node to explore
                              g = tentativeG,        //cost so far
                              f = fScore[edge.nodePointsTo], //total cost
prediction
                                parent = current.node   //remember how we got here
                          });
                      }
                  }
              }

              //if we run out of open nodes, no path was found
              return new List<Node>();
          }

        private double Heuristic(Node a, Node b) //most optimistic distance,
least possible
        {
            double ChangeInX = Math.Abs(a.worldCenterX - b.worldCenterX);
            double ChangeInY = Math.Abs(a.worldCenterY - b.worldCenterY);
            return (ChangeInX / Utilities.TileSize) + (ChangeInY /
Utilities.TileSize) * 0.6; //x + y weighted
        }



        private List<Node> ReconstructPath(Dictionary<Node, Node> parent, Node
goal)
        {
            var list = new List<Node>(); //to hold order
            var cur = goal; //satrt at goal
            var seen = new HashSet<Node>(); //track visted node to avoid loop
            while (cur != null && !seen.Contains(cur))
            {
                list.Add(cur);
                seen.Add(cur);
                parent.TryGetValue(cur, out var p); //get previous node
                cur = p; //make new current
            }
            list.Reverse(); //reverse -> enmey to player
            return list;
        }

        private void DrawDebugPath(MainWindow game)
        {
            debugPathVisuals.Clear(); //reset list so we can track new shapes
this frame
            if (!game.showHitboxes || currentPath == null || currentPath.Count
== 0) return; //if hitbox debug mode is off or no path exists, stop here
            //loop through each node in the current path
            for (int i = 0; i < currentPath.Count; i++)
            {
                var currentNode = currentPath[i]; //current node
                //convert node world position to screen position relative to
camera
                double screenX = currentNode.worldCenterX -
game.cameraCenter.X + game.halfCameraWidth;
                double screenY = currentNode.worldCenterY -
game.cameraCenter.Y + game.halfCameraHeight;
                //draw a small green circle for this node
                var ellipse = new systemShapes.Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.LimeGreen, //node color
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(ellipse, screenX - 3); //center circle at node
                Canvas.SetTop(ellipse, screenY - 3);
                game.GameCanvas.Children.Add(ellipse); //add to canvas
                debugPathVisuals.Add(ellipse); //remember so can remove next
frame
                Canvas.SetZIndex(ellipse, 30); //ensure drawn on top
                //if not the last node, draw a line to the next node
                if (i < currentPath.Count - 1)
                {
                    var nextNode = currentPath[i + 1]; //next node in path

                    double nextNodeScreenX = nextNode.worldCenterX -
game.cameraCenter.X + game.halfCameraWidth;
                    double nextNodeScreenY = nextNode.worldCenterY -
game.cameraCenter.Y + game.halfCameraHeight;

                    //choose color based on vertical movement between nodes
                    Brush edgeColor = Brushes.Green; //default = walk
                    if (nextNode.worldCenterY < currentNode.worldCenterY -
Utilities.TileSize * 0.6) { edgeColor = Brushes.Orange; } //going up (jump)
                    else if (nextNode.worldCenterY > currentNode.worldCenterY
+ Utilities.TileSize * 0.6) {edgeColor = Brushes.DeepSkyBlue; } //going down
(fall)
                    //draw line between this node and the next node
                    var line = new systemShapes.Line
                    {
                        X1 = screenX,
                        Y1 = screenY,
                        X2 = nextNodeScreenX,
                        Y2 = nextNodeScreenY,
                        Stroke = edgeColor, //color indicates movement type
                        StrokeThickness = 2
                    };
                    game.GameCanvas.Children.Add(line);
                    debugPathVisuals.Add(line); //track line for removal next
frame
                    Canvas.SetZIndex(line, 30); //draw above terrain
                }
            }
        }
    }

   public partial class MainWindow
   {
       public List<Enemy> enemies = new List<Enemy>();
       private systemShapes.Rectangle EnemyHitboxRectangle;
       private systemShapes.Rectangle EnemyCenterHitboxRectangle;

       private void SpawnZombieSubroutine((double, double) XYs)
       {
           Random rand = new Random();
           int zombieType = rand.Next(1, 4);
           var frames = EnemyAssetLoader.GetAnimation($"Zombie{zombieType}");
           var zombie = new Zombie(XYs.Item1, XYs.Item2, frames);
           enemies.Add(zombie);
           Canvas.SetZIndex(zombie.enemyImageControl, 8);
           GameCanvas.Children.Add(zombie.enemyImageControl);

           EnemyHitboxRectangle = new systemShapes.Rectangle
           {
               Stroke = Brushes.Red,
               StrokeThickness = 2,
               Width = zombie.Hitbox.Width,
               Height = zombie.Hitbox.Height,

               Fill = Brushes.Transparent,
               Tag = "Hitbox"
            };
            zombie.thisEnemiesHitboxRectangle = EnemyHitboxRectangle;
            GameCanvas.Children.Add(zombie.thisEnemiesHitboxRectangle);
            Canvas.SetZIndex(zombie.thisEnemiesHitboxRectangle, 9);
            zombie.thisEnemiesHitboxRectangle.Visibility = showHitboxes ?
Visibility.Visible : Visibility.Collapsed;

            EnemyCenterHitboxRectangle = new systemShapes.Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 4,
                Width = zombie.enemyHitboxCenter.Width,
                Height = zombie.enemyHitboxCenter.Height,
                Tag = "Hitbox"
            };
            zombie.thisEnemiesCenterHitboxRectangle =
EnemyCenterHitboxRectangle;
            GameCanvas.Children.Add(zombie.thisEnemiesCenterHitboxRectangle);
            Canvas.SetZIndex(zombie.thisEnemiesCenterHitboxRectangle, 9);
            zombie.thisEnemiesCenterHitboxRectangle.Visibility = showHitboxes
? Visibility.Visible : Visibility.Collapsed;
        }

       private void SpawnAZombie()
       {
           Random rand = new Random();
           int side = rand.Next(0, 2);
           if (side == 0)
           {

SpawnZombieSubroutine(InitialiseEnemyPos((int)Math.Round((myPlayer.playerX /
Utilities.TileSize) - ((CameraWidth / 2) - (chunkSize / 2))), (chunkSize / 2 -
4), (int)Math.Round(myPlayer.playerY / Utilities.TileSize)));
            }
            else
            {

SpawnZombieSubroutine(InitialiseEnemyPos((int)Math.Round((myPlayer.playerX /
Utilities.TileSize) + ((CameraWidth / 2) + (chunkSize / 2))), (chunkSize / 2 -
4), (int)Math.Round(myPlayer.playerY / Utilities.TileSize)));
            }
        }

       private void UpdateEnemies()
       {
           var toRemove = new List<Enemy>();
           int numberEnemyesKilled = 0;

           foreach (var enemy in enemies)           6D loop connected within
           {                                        Enemy & main loop
               enemy.UpdateAI(this);

        DetectEnemyHitPlayer(enemy);

        if (enemy._enemyHealth <= 0)
        {
            numberEnemyesKilled++; //can get drops
            toRemove.Add(enemy);
        }
        else if (enemy.enemyToDespawn == true)
        {
            toRemove.Add(enemy); //cant get drops
        }

    }

    foreach (var enemy in toRemove)
    {
        RemoveEnemy(enemy);
    }
    Random rand = new Random();
    for (int i = 0; i < numberEnemyesKilled; i++)
    {
        int randomNumber1 = rand.Next(1, 101); //-> 1 to 100
        //Healing Potions

        if (randomNumber1 <= 5) //5%
        {
            myInventory.AddItem(new HealingPotionItem(), 1);
        }
        else if (randomNumber1 <= 15) //15% - 5% = 10%
        {
            myInventory.AddItem(new LesserHealingPotionItem(), 1);
        }

        //Gel
        int randomNumber2 = rand.Next(0, 3); //-> 0 to 2 gel
        myInventory.AddItem(new GelItem(), randomNumber2);

        int randomNumber3 = rand.Next(0, 101); //1 to 100
        if (randomNumber3 <= 10) //10%
        {
            myInventory.AddItem(new LensItem(), 1);
        }
    }
    UpdateEnemyHitboxDisplay();
}

private void RemoveEnemy(Enemy enemy)
{
    GameCanvas.Children.Remove(enemy.enemyImageControl);
    GameCanvas.Children.Remove(enemy.enemyHealthBar);
    GameCanvas.Children.Remove(enemy.thisEnemiesHitboxRectangle);


GameCanvas.Children.Remove(enemy.thisEnemiesCenterHitboxRectangle);
            enemy.DeleteDebugPath(this);
            enemies.Remove(enemy);
            EnemyAmount -= 1;
        }
        private void DetectEnemyHitPlayer(Enemy thisEnemy)
        {
            if (thisEnemy.hitPlayerStopwatch.ElapsedMilliseconds > 1000 &&
playersBeenHitStopwatch.ElapsedMilliseconds > 200 &&
thisEnemy.Hitbox.IntersectsWith(myPlayer.Hitbox))
            {
                playersBeenHitStopwatch.Restart();
                thisEnemy.hitPlayerStopwatch.Restart();
                myPlayer.ChangePlayerHealth(-5);

                if (thisEnemy.enemyX > myPlayer.playerX) //enemy on right
therfore hit left
                {
                    ApplyPlayerKnockback(-1, 8);
                }
                else
                {
                    ApplyPlayerKnockback(1, 8);
                }

           }
       }
       public void ApplyPlayerKnockback(int knockbackDirection, int VelY)
       {
           if (isGrounded)
           {
               velocityY -= VelY;
               isGrounded = false;
           }

           ApplyingPlayerKnockback = true;

           KnockbackDirection = knockbackDirection;
       }

       private void DetectToolHitsEnemyOrBoss(Item thisTool)
       {
           if (ToolHitbox == null) return;

            Point[] toolCorners = GetToolHitboxCornersWorld(ToolHitbox,
GameCanvas, cameraCenter, halfCameraWidth, halfCameraHeight); //as rotated
hitbox

           foreach (var enemy in enemies)
           {
               if (enemy.enemyBeenHit.ElapsedMilliseconds > 500)
               {

                    if (RotatedRectIntersectsAABB(toolCorners, enemy.Hitbox))
                    {
                        enemy.enemyBeenHit.Restart();
                        enemy.ChangeEnemyHealth(-thisTool.Damage);
                        if (myPlayer.playerX > enemy.enemyX)
                        {
                            enemy.ApplyEnemyKnockback(-1);
                        }
                        else
                        {
                            enemy.ApplyEnemyKnockback(1);
                        }

                    }
                }
            }
            if (currentEyeBoss != null)
            {
                 if (currentEyeBoss.eyeBossBeenHit.ElapsedMilliseconds > 500)
                {
                     Point[] eyeBossCorners =
GetEyeBossHitboxCornersWorld(currentEyeBoss);
                     if (RotatedRectIntersectsRotatedRect(toolCorners,
eyeBossCorners))
                     {
                         currentEyeBoss.eyeBossBeenHit.Restart();
                         currentEyeBoss.ChangeBossHealth(-thisTool.Damage);
                         //no boss knowckback
                     }
                }
            }
        }

        private Point[] GetToolHitboxCornersWorld(systemShapes.Rectangle rect,
Canvas canvas, Point cameraCenter, double halfCameraWidth, double
halfCameraHeight)
        {
            //use this as angle hard to get from toolbox
            if (rect == null) return Array.Empty<Point>();

            //tranform to canvas coordinates
            GeneralTransform gt = rect.TransformToAncestor(canvas);

            Point[] screenCorners =
            {
                gt.Transform(new Point(0, 0)),
                gt.Transform(new Point(rect.Width, 0)),
                gt.Transform(new Point(rect.Width, rect.Height)),
                gt.Transform(new Point(0, rect.Height))
            };

            //convert to world coordinates
            for (int i = 0; i < screenCorners.Length; i++)

            {
                screenCorners[i] = new Point(
                    screenCorners[i].X + cameraCenter.X - halfCameraWidth,
                    screenCorners[i].Y + cameraCenter.Y - halfCameraHeight
                );
            }

            return screenCorners;
        }

        private bool RotatedRectIntersectsAABB(Point[] corners, Rect aabb)
//use 1D line and all overlap
        {
            if (corners == null || corners.Length != 4) return false; //check
valid input

            //build edge (lines) from rotated rect
            Vector[] axes = new Vector[4];
            axes[0] = corners[1] - corners[0]; //top edge
            axes[1] = corners[2] - corners[1]; //right edge
            axes[2] = corners[3] - corners[2]; //bottom edge
            axes[3] = corners[0] - corners[3]; //left edge

            for (int i = 0; i < axes.Length; i++) //for each edge
            {
                Vector edge = axes[i];
                Vector normal = new Vector(-edge.Y, edge.X); //perpendicular
axis
                if (normal.Length > 0.0001) //avoid zero vector
                {
                    normal.Normalize(); //unit length           Vectors and vector
                    axes[i] = normal; //replace with normal     normalisaKon
                }
            }

            //add aabb's own axes (world x and y)
            axes = axes.Concat(new[] { new Vector(1, 0), new Vector(0, 1)
}).ToArray();

            foreach (var axis in axes) //check each axis              Vector axes
            {
                //project rotated rect onto axis
                double minRotated = double.MaxValue, maxRotated =
double.MinValue;
                foreach (var point in corners)                   5D loop with arrow collison
                {
                    double projection = (point.X * axis.X) + (point.Y *
axis.Y); //dot product
                    minRotated = Math.Min(minRotated, projection); //track min
                    maxRotated = Math.Max(maxRotated, projection); //track max
                }

                 //project aabb onto same axis

                  Point[] aabbCorners =
                  {
                      new Point(aabb.Left, aabb.Top),
                      new Point(aabb.Right, aabb.Top),
                      new Point(aabb.Right, aabb.Bottom),
                      new Point(aabb.Left, aabb.Bottom)
                  };

                double minAligned = double.MaxValue, maxAligned =
double.MinValue;
                foreach (var point in aabbCorners)
                {
                    double projection = (point.X * axis.X) + (point.Y *
axis.Y); //dot product
                    minAligned = Math.Min(minAligned, projection);
                    maxAligned = Math.Max(maxAligned, projection);
                }

                  if (maxRotated < minAligned || maxAligned < minRotated) //gap
along this axis
                  {
                      return false; //no collision
                  }
            }

            return true; //is all collide coliion
        }
    }
}
