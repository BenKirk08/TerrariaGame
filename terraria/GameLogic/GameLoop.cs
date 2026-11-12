using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using systemShapes = System.Windows.Shapes;

namespace WPFTerraria
{
    public partial class MainWindow
    {
        //renders when fast enough
        private protected void GameLoop(object sender, EventArgs e)
        {
            CheckIfPlayerDead();
            if (isInventoryOpen || isPaused || PlayerIsDead)
            {
                if (PlayerIsDead)
                {
                    if (PotionUsedLast.IsRunning)
                    {
                        PotionUsedLast.Restart();
                        PotionUsedLast.Stop();
                        NotUsedPotionYet = true;
                    }
                    if (RegainPlayerHealth.IsRunning)
                    {
                        RegainPlayerHealth.Restart();
                        RegainPlayerHealth.Stop();
                    }
                    return;
                }
                WasPaused = true;
                if (PotionUsedLast.IsRunning)
                {
                    PotionStopped_Timer = true;
                    PotionUsedLast.Stop();
                }
                if (RegainPlayerHealth.IsRunning)
                {
                    RegainHealthStopper_timer = true;
                    RegainPlayerHealth.Stop();
                }
                if (ZombieSpawnTimer.IsRunning)
                {
                    ZombieSpawnTimer_WasPaused = true;
                    ZombieSpawnTimer.Stop();
                }
                frameStopwatch.Restart();
                accumulatedTimeInMs = 0;
                return; //skip processing when inventory is open

            }
            //calculate time since last frame
            double timePassedInMs_1 =
frameStopwatch.Elapsed.TotalMilliseconds;
            frameStopwatch.Restart();
            accumulatedTimeInMs += timePassedInMs_1;

           playerMoved = false; //for updating interaction outline

           //only update game logic if enough time passed for 60 FPS
           while (accumulatedTimeInMs >= TargetFrameTimeMs)
           {
               RestartTimersIfPaused();
               UpdateFrameRate();

                //start actual logic
                UpdateDayNightAndCelestials(timePassedInMs_1 / 1000.0);
//day/night update

               //store current position
               playerCurrentX = myPlayer.playerX;
               playerCurrentY = myPlayer.playerY;

                //see if your walking
                playerIsWalking = pressedKeys.Contains(Key.A) ^
pressedKeys.Contains(Key.D); //xor operator -> if move left and right then
still

               UpdatePlayerMovement(); //update playerX/Y

               //handle animation & direction -> not actual movement here
               UpdatePlayerAnimation();

               ApplyPlayerGravity();


                //set animation image
                int slot = myInventory.SelectedInventoryIndex;
                if (isShooting && slot >= 0 && myInventory[slot].Quantity !=
0) playerImage.Source =
myPlayer.GetPlayerImage($"{playerGender}{currentAnimationType}Shoot");
                else if (isSwinging && slot >= 0 && myInventory[slot].Quantity
!= 0) playerImage.Source =
myPlayer.GetPlayerImage($"{playerGender}{currentAnimationType}Swing");
                else playerImage.Source =
myPlayer.GetPlayerImage($"{playerGender}{currentAnimationType}");

                //before swinging so can place based on where player is
                if (myPlayer.playerX != previousPlayerX || myPlayer.playerY !=
previousPlayerY) //only if player moved or not yet setup
                {
                    HandleCurrentVisiblility();
                }

               //swinging arm animation
               ArmSwingingLogic();

                //keep player within map bounds (pixels)
                myPlayer.playerX = Math.Clamp(myPlayer.playerX,
(myPlayer.HitboxWidth / 2), mapX_Width * Utilities.TileSize -
(myPlayer.HitboxWidth / 2));
                myPlayer.playerY = Math.Clamp(myPlayer.playerY,
(myPlayer.HitboxHeight / 2), mapY_Height * Utilities.TileSize -
(myPlayer.HitboxHeight / 2));

               HandleMouseClicks();

               parallaxUpdater.UpdateAll(cameraCenter);
               caveBackground.Update(cameraCenter);        3D loop when called in main
               caveBackground.UpdateClip(cameraCenter);
                                                           loop
               UpdateEnemiesGameLoop();

               CheckPlayerFallDamage();
               CheckForPlayerRegainHealth();

               UpdateMiningParticles();
           }
           mouseMoved = false;
       }

       private void CheckIfPlayerDead()
       {
           if (myPlayer._playerHealth <= 0)
           {
               PlayerIsDead = true;
               if (!deathCountdownTimer.IsRunning) //on first tick of death
               {
                   if (!GameCanvas.Children.Contains(deathTextBlock))
                   {
                       GameCanvas.Children.Add(deathTextBlock);
                   }
                   playerImage.Visibility = Visibility.Collapsed;
                   playerImageArm.Visibility = Visibility.Collapsed;
                   swingItemImage.Visibility = Visibility.Collapsed;
                   deathCountdownTimer.Start();
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
                }
                int timeInSeconds = respawnTimeInSeconds -
((int)deathCountdownTimer.ElapsedMilliseconds / 1000);
                if (timeInSeconds < 0) //if respawn timer over
                {
                    PlayerIsDead = false;
                    playerImage.Visibility = Visibility.Visible;
                    if (isSwinging)
                    {
                        playerImageArm.Visibility = Visibility.Visible;
                        swingItemImage.Visibility = Visibility.Visible;
                    }
                    deathTextBlock.Visibility = Visibility.Collapsed;
                    (myPlayer.playerX, myPlayer.playerY) =
InitialisePlayerPos((mapX_Width / 2), 100);
                    deathCountdownTimer.Restart();
                    deathCountdownTimer.Stop();
                    myPlayer.ChangePlayerHealth(myPlayer._playerMaxHealth);
                    isGrounded = false;

                   if (ZombieSpawnTimer.IsRunning)
                   {
                       ZombieSpawnTimer.Restart();
                   }

                }
                else //repsawining
                {
                    deathTextBlock.Visibility = Visibility.Visible;
                    deathTextBlock.Text = $"You Died, Respawning:
{timeInSeconds}";
                    Canvas.SetLeft(deathTextBlock, halfCameraWidth -
deathTextBlock.ActualWidth / 2);
                    Canvas.SetTop(deathTextBlock, halfCameraHeight -
deathTextBlock.ActualHeight / 2);
                }
            }
        }
        private void RestartTimersIfPaused()
        {
            if (WasPaused) //if was paused then restart any timers that were
running
            {
                WasPaused = false;
                if (PotionStopped_Timer)
                {
                    PotionUsedLast.Start();
                    PotionStopped_Timer = false;
                }
                if (RegainHealthStopper_timer)
                {

                   RegainPlayerHealth.Start();
                   RegainHealthStopper_timer = false;
               }
               if (ZombieSpawnTimer_WasPaused)
               {
                   ZombieSpawnTimer_WasPaused = false;
                   ZombieSpawnTimer.Start();
               }
            }
        }
        private void UpdateDayNightAndCelestials(double elapsedSeconds)
        {
            //1 -> smooth interpolation w factor 0 -> 1 in between tick
            double fractionOfTick = dayClockStopwatch.Elapsed.TotalSeconds /
secsToChangeTime;
            fractionOfTick = Math.Clamp(fractionOfTick, 0, 1);

            //2 -> renderTime by interpolating from last tick
            TimeSpan lastTickTime = dayClock.TimeOfDay; // last snapped tick
            TimeSpan nextTickTime =
lastTickTime.Add(TimeSpan.FromMinutes(tickMinutes));
            renderTime = TimeSpan.FromMinutes(lastTickTime.TotalMinutes +
(nextTickTime.TotalMinutes - lastTickTime.TotalMinutes) * fractionOfTick);
            renderTime = TimeSpan.FromMinutes(renderTime.TotalMinutes % 1440);
// wrap around 24h

             // 3 -> update background colour based on renderTime
             double brightness = GetBrightnessFactor(renderTime);
            Color dayColor = Colors.LightSkyBlue;
            Color nightColor = Colors.DarkSlateBlue;
             byte r = (byte)(nightColor.R + (dayColor.R - nightColor.R) *
brightness);
             byte g = (byte)(nightColor.G + (dayColor.G - nightColor.G) *
brightness);
             byte b = (byte)(nightColor.B + (dayColor.B - nightColor.B) *
brightness);
            GameCanvas.Background = new SolidColorBrush(Color.FromRgb(r, g,
b));

            // 4 -> Sun position
            double sunX = GetSunMoonX(renderTime, SunriseMid, SunsetMid,
SunImage.ActualWidth);
            Canvas.SetLeft(SunImage, sunX);
            Canvas.SetTop(SunImage, 35);
            SunImage.Opacity = (renderTime >= SunriseMid && renderTime <=
SunsetMid) ? 1 : 0;

            // 5 ->Moon position
            double moonX = GetSunMoonX(renderTime, SunsetMid, SunriseMid,
MoonImage.ActualWidth);
            Canvas.SetLeft(MoonImage, moonX);
            Canvas.SetTop(MoonImage, 80);


            MoonImage.Opacity = (renderTime >= SunsetMid || renderTime <=
SunriseMid) ? 1 : 0;

           // 6 -> change the dayClock when stopwatch passes tick
           if (dayClockStopwatch.Elapsed.TotalSeconds >= secsToChangeTime)
           {
               dayClock = dayClock.AddMinutes(tickMinutes); //add to clock
               timeTextBlock.Text = dayClock.ToString("HH:mm");
               dayClockStopwatch.Restart(); // reset for the next tick
           }
        }
        private double GetSunMoonX(TimeSpan now, TimeSpan startTime, TimeSpan
endTime, double imageWidth)
        {
            //Get X position for sun/moon sliding in from left
            bool crossesMidnight = endTime < startTime;
            bool inRange = crossesMidnight ? (now >= startTime || now <=
endTime) : (now >= startTime && now <= endTime); //range wraps past midnight
            if (!inRange)
            {
                return -imageWidth; //off-screen
            }
            double totalMinutes;
            double minutesPassed;
            if (crossesMidnight)
            {
                //normalise so end is always after start
                totalMinutes = (endTime - startTime +
TimeSpan.FromHours(24)).TotalMinutes;
                minutesPassed = (now - startTime + (now < startTime ?
TimeSpan.FromHours(24) : TimeSpan.Zero)).TotalMinutes;
            }
            else
            {
                totalMinutes = (endTime - startTime).TotalMinutes;
                minutesPassed = (now - startTime).TotalMinutes;
            }
            double percentageIntoBrightness = minutesPassed / totalMinutes; //
0 -> 1
            return -imageWidth + percentageIntoBrightness *
(GameCanvas.ActualWidth + imageWidth); // left -> right
        }
        private double GetBrightnessFactor(TimeSpan now)
        {
            if (now >= sunriseEnd && now < sunsetStart)
            {
                return 1; //full bright
            }
            if (now >= sunsetEnd || now < sunriseStart)
            {
                return 0; //full dark
            }


            if (now >= sunriseStart && now < sunriseEnd)
            {
                double t = (now - sunriseStart).TotalMinutes / (sunriseEnd -
sunriseStart).TotalMinutes;
                return t; //fade to dark
            }
            if (now >= sunsetStart && now < sunsetEnd)
            {
                double t = (now - sunsetStart).TotalMinutes / (sunsetEnd -
sunsetStart).TotalMinutes;
                return 1 - t; //fade to light
            }
            return 0;
        }
        private void UpdateFrameRate()
        {
            //update game state (at 60 FPS)
            accumulatedTimeInMs -= TargetFrameTimeMs; //carry over leftover
time

           //fps count
           frameRateCount++;

            if (fpsStopwatch.Elapsed.TotalSeconds >= 1.0)
            {
                currentFps = frameRateCount /
fpsStopwatch.Elapsed.TotalSeconds;
                fpsTextBlock.Text = $"{currentFps:0.0} FPS"; //if null then
0fps
                frameRateCount = 0;
                fpsStopwatch.Restart(); //reset for next second
            }
        }
        private void UpdatePlayerMovement()
        {
            //vertical movement
            if (pressedKeys.Contains(Key.Space) && isGrounded == true)
            {
                velocityY += jumpStrength;
                isGrounded = false;
                jumpingAnimationStopwatch.Restart();
            }

            //horizontal movement
            if (pressedKeys.Contains(Key.A))
            {
                systemShapes.Rectangle[] collidedTiles =
CheckCollisonWithTiles(playerCurrentX - myPlayer.playerSpeed, playerCurrentY,
2, myPlayer.HitboxWidth, myPlayer.HitboxHeight); //only create if moves to
save proccesing power when still

               if (collidedTiles == null)
               {

                    myPlayer.playerX -= myPlayer.playerSpeed;
                    CheckIfPlayerStillGrounded();
                }
                else
                {
                    bool stepUpHappened = false;
                    if (collidedTiles.Length == 1 && isGrounded) //only
attempt stepping up if there's exactly 1 blocking tile & on ground
                    {
                        systemShapes.Rectangle blockingTile =
collidedTiles[0];

                        double blockingTileY = Canvas.GetTop(blockingTile) +
cameraCenter.Y - (GameCanvas.Height / 2); //Y position of tile

                        if (blockingTileY > myPlayer.Hitbox.Bottom -
Utilities.TileSize - 1) //if the blocks at foot
                        {
                            //test step up
                            if (CheckCollisonWithTiles(playerCurrentX,
playerCurrentY - Utilities.TileSize, 1, myPlayer.HitboxWidth,          3D Loops when
myPlayer.HitboxHeight) == null) //check above
                                                                       called
                            {
                                if (CheckCollisonWithTiles(playerCurrentX -
myPlayer.playerSpeed, playerCurrentY - Utilities.TileSize, 1,
myPlayer.HitboxWidth, myPlayer.HitboxHeight) == null) //check above and left
                                {
                                    myPlayer.playerY = blockingTileY -
(myPlayer.HitboxHeight / 2); //move ontop of tile
                                    myPlayer.playerX -= myPlayer.playerSpeed;
//move forward
                                    stepUpHappened = true;
                                }
                            }
                        }
                    }
                    if (!stepUpHappened)
                    {
                        myPlayer.playerX = Canvas.GetLeft(collidedTiles[0]) +
cameraCenter.X - (GameCanvas.Width / 2) + collidedTiles[0].Width +
(myPlayer.HitboxWidth / 2); //fix movement after collision
                        playerIsWalking = false;
                    }
                }

            }

            if (pressedKeys.Contains(Key.D))
            {
                systemShapes.Rectangle[] collidedTiles =
CheckCollisonWithTiles(playerCurrentX + myPlayer.playerSpeed, playerCurrentY,
2, myPlayer.HitboxWidth, myPlayer.HitboxHeight); //only create if moves to
save proccesing power when still

                if (collidedTiles == null)
                {
                    myPlayer.playerX += myPlayer.playerSpeed;
                    CheckIfPlayerStillGrounded();
                }
                else
                {
                    bool stepUpHappened = false;
                    if (collidedTiles.Length == 1 && isGrounded) //only
attempt stepping up if there's exactly 1 blocking tile and on ground
                    {
                        systemShapes.Rectangle blockingTile =
collidedTiles[0];
                        double blockingTileY = Canvas.GetTop(blockingTile) +
cameraCenter.Y - (GameCanvas.Height / 2); ; //Y position of tile

                        if (blockingTileY > myPlayer.Hitbox.Bottom -
Utilities.TileSize - 1) //if the blocks at foot
                        {
                            //test step up
                            if (CheckCollisonWithTiles(playerCurrentX,
playerCurrentY - Utilities.TileSize, 1, myPlayer.HitboxWidth,
myPlayer.HitboxHeight) == null) //check above
                            {
                                if (CheckCollisonWithTiles(playerCurrentX +
myPlayer.playerSpeed, playerCurrentY - Utilities.TileSize, 1,
myPlayer.HitboxWidth, myPlayer.HitboxHeight) == null) //check above and right
                                {
                                    myPlayer.playerY = blockingTileY -
(myPlayer.HitboxHeight / 2); //move ontop of tile
                                    myPlayer.playerX += myPlayer.playerSpeed;
//move forward
                                    stepUpHappened = true;
                                }
                            }
                        }
                    }
                    if (!stepUpHappened)
                    {
                        myPlayer.playerX = Canvas.GetLeft(collidedTiles[0]) +
cameraCenter.X - (GameCanvas.Width / 2) - (myPlayer.HitboxWidth / 2);

                       playerIsWalking = false;
                   }
               }
           }
       }
       private void UpdatePlayerAnimation()
       {
           if (playerIsWalking) //conatins both right and left
           {
               if (!aimLocksFacing)
               {

                    //update direction based on direction moving
                    bool movingRight = pressedKeys.Contains(Key.D); //use
right as default so know what true and false mean
                    if (movingRight != facingRight) //if not looking way
facing -> (left or right)
                    {
                        facingRight = movingRight; //face way looking/walking
                                                   //then apply flip transform
                                                   // Flip arm image depending
on facing
                        ApplyFacingFlip();
                    }
                }

                if (isGrounded)
                {
                    //change animation frame every 100ms -> after gameloop
                    double timePassedInMs_2 =
animationStopwatch.Elapsed.TotalMilliseconds;
                    if (timePassedInMs_2 >= WalkFrameInterval)
                    {
                        currentWalkFrame = (currentWalkFrame + 1) % 6; //cycle
through 0->5, note mod 6 so doesnt go above 6
                        animationStopwatch.Restart(); //reset the timer for
the next frame
                    }


                   //update player image to current walk frame
                   currentAnimationType = $"Run{currentWalkFrame + 1}";
               }
           }
           else
           {
               //reset to still when not walking
               currentAnimationType = $"Standing";
               currentWalkFrame = 0; //reset animation
           }
        }
        private void ApplyFacingFlip()
        {
            swingItemImage.LayoutTransform = new ScaleTransform(facingRight ?
1 : -1, 1);
            playerImageArm.LayoutTransform = new ScaleTransform(facingRight ?
1 : -1, 1);
            playerImage.RenderTransform = new ScaleTransform(facingRight ? 1 :
-1, 1); //if facing right == false, and was previously facing right (from if
statement), X scale = -1 therfore flipped
        }
        private void ApplyPlayerGravity()
        {
            if (isGrounded == false)


            {
                double timePassedInMs_3 =
jumpingAnimationStopwatch.Elapsed.TotalMilliseconds;
                if (timePassedInMs_3 >= TimeToJumpImage)
                {
                    currentAnimationType = $"Jumping";
                }

                if (velocityY < Utilities.TileSize - (gravity * 1.5)) //max
velocity -> also so cant go through a tile
                {
                    velocityY += gravity;
                }
                                                                         3D Loop when
                //check for ground collision
                systemShapes.Rectangle[] collidingTiles =                called
CheckCollisonWithTiles(playerCurrentX, playerCurrentY + velocityY, 1,
myPlayer.HitboxWidth, myPlayer.HitboxHeight); //only need to check 1 tile
                if (collidingTiles != null) //if colliding
                {
                    systemShapes.Rectangle blockingTile = collidingTiles[0];
                    double blockingTileY = Canvas.GetTop(blockingTile) +
cameraCenter.Y - (GameCanvas.Height / 2); //Y position of tile

                    if (velocityY > 0) //velocity positive therfore going down
                    {
                        myPlayer.playerY = blockingTileY -
(myPlayer.HitboxHeight / 2); //place player on top of the tile
                        velocityY = 0; //Stop falling
                        isGrounded = true;
                    }
                    else if (velocityY <= 0) //velocity negative or zero
therefore going up or at top
                    {
                        myPlayer.playerY = blockingTileY +
collidingTiles[0].Height + (myPlayer.HitboxHeight / 2);
                        velocityY = 0; //start to go down but grounded still
not true
                    }
                }
                else
                {
                    myPlayer.playerY += velocityY;
                    isGrounded = false;
                }
            }
        }
        private void HandleCurrentVisiblility()
        {
            //NOTE: ORDER MATTERS -> camera first as dont wnat outdated camera
coords, as when next move appears to lag
            //center camera on player
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

           UpdateVisibleTiles();                       5D Loop when
           tileRenderer.UpdateCamera(cameraCenter);
                                                       called
           //calculate how much camera moved
           double cameraMoveX = cameraCenter.X - lastCameraPosition.X;
           double cameraMoveY = cameraCenter.Y - lastCameraPosition.Y;

            //calculate how much camera moved
            bool cameraMovedEnoughX = cameraMoveX >= chunkUpdateThreshold *
Utilities.TileSize ||
                      cameraMoveX <= -chunkUpdateThreshold *
Utilities.TileSize;

            bool cameraMovedEnoughY = cameraMoveY >= chunkUpdateThreshold *
Utilities.TileSize ||
                                      cameraMoveY <= -chunkUpdateThreshold *
Utilities.TileSize;

           if (cameraMovedEnoughX || cameraMovedEnoughY)                 4D Loop when
           {                                                             called
               UnloadFarChunks(); //only unload if chunks were loaded

                 lastCameraPosition = cameraCenter; //save camera position to
track movement
            }

           //update hovered tile mouse position when player moves
           if (MouseOnScreen)
           {
               var mousePos = Mouse.GetPosition(GameCanvas);
               hoveredTile = GetTileFromMousePosition(mousePos);

           }
           playerMoved = true; //player has moved in this frame
       }
       private void ArmSwingingLogic()

       {
            int slot = myInventory.SelectedInventoryIndex;
            if (isSwinging && slot >= 0 && myInventory[slot].Quantity != 0)
//if swinging and item in hand
            {
                //arm and item visible if not
                if (!isArmVisibile)
                {
                    isArmVisibile = true;
                    playerImageArm.Visibility = Visibility.Visible;
                    swingItemImage.Visibility = Visibility.Visible;
                }
                //always update incase changes item while not stop swinging -
> ir uses numebres to switch ite,
                swingItemImage.Source = myInventory[slot].ItemImage;
                swingItemImage.Height = myInventory[slot].ItemImage.Height *
1.65;
                swingItemImage.Width = myInventory[slot].ItemImage.Width *
1.65;

               //for positioning
               double dx = 0;
               double dy = 0;
               if (myInventory[slot].Item.ItemType != "Bow")
               {
                   //for pivoting on shoulder -> only change if is actually
swinging
                   if (facingRight)
                   {
                       itemRotateTransform.CenterX = -6;
                       itemRotateTransform.CenterY = 12;
                       armRotateTransform.CenterX = -6;
                       armRotateTransform.CenterY = 12;
                       toolHitboxTransform.CenterX = 0;
                       toolHitboxTransform.CenterY = 0;
                       toolHitboxConstantWhenDirection = -(playerHitbox.Width
/ 2 * 0.3);

                       armleftoffset = 0;
                   }
                   else
                   {
                       itemRotateTransform.CenterX = 13;
                       itemRotateTransform.CenterY = 13;
                       armRotateTransform.CenterX = 13;
                       armRotateTransform.CenterY = 13;

                       toolHitboxTransform.CenterX = 0;
                       toolHitboxTransform.CenterY = 0;
                       toolHitboxConstantWhenDirection = (playerHitbox.Width
/ 2 * 0.6);



                       armleftoffset = -2;
                   }

                    if (swingingForward)
                    {
                        currentArmRotation += swingSpeed;
                        if (currentArmRotation >= ArmRotationMax)
                        {
                            currentArmRotation = ArmRotationMax;
                            swingingForward = false;
                        }
                    }
                    else
                    {
                        currentArmRotation -= swingSpeed;
                        if (currentArmRotation <= -50)
                        {
                            currentArmRotation = -50;
                            swingingForward = true;
                        }
                    }
                    armRotateTransform.Angle = facingRight ?
currentArmRotation : -currentArmRotation;
                    itemRotateTransform.Angle = facingRight ?
currentArmRotation - 60 : -currentArmRotation + 60;

                    angleRad = (facingRight ? currentArmRotation + 85 : -
currentArmRotation + 95) * (Math.PI / 180);


                   if (myInventory[slot].Item.ItemType == "Tool")
                   {
                       if (showHitboxes)
                       {
                           ToolHitbox.Visibility = Visibility.Visible;

                        }
                        //relative offset from the pivot
                        dx = Math.Cos(angleRad) * rTool;
                        dy = Math.Sin(angleRad) * rTool;
                        toolHitboxTransform.Angle = facingRight ?
currentArmRotation + 180 : -currentArmRotation + 205;
                        Canvas.SetLeft(ToolHitbox, myPlayer.playerX -
cameraCenter.X + halfCameraWidth + toolHitboxConstantWhenDirection);
                        Canvas.SetTop(ToolHitbox, myPlayer.playerY -
cameraCenter.Y + halfCameraHeight + 4);

                       //detect if enmy hit
                       DetectToolHitsEnemyOrBoss(myInventory[slot].Item);

                   }
                   else
                   {

                       if (showHitboxes)
                       {
                           ToolHitbox.Visibility = Visibility.Hidden;
                       }
                       dx = Math.Cos(angleRad) * rItem;
                       dy = Math.Sin(angleRad) * rItem;
                   }

                    if (myInventory[slot].Item.ItemType == "Healing")
                    {
                        if (PotionUsedLast.ElapsedMilliseconds >
(TimeToNextPotionSecs * 1000) || NotUsedPotionYet)
                        {
                            if (myPlayer._playerHealth !=
myPlayer._playerMaxHealth)
                            {
                                NotUsedPotionYet = false;
                                PotionUsedLast.Restart();

myPlayer.ChangePlayerHealth(myInventory[slot].Item.HealingAmount);
                                myInventory[slot].Quantity -= 1;

                           }
                       }
                   }
                   else if (myInventory[slot].Item.ItemType == "Summon")
                   {
                       if (currentEyeBoss == null) //if not alreayd a boss
                       {
                           if (MoonImage.Opacity == 1) //if its night time
                           {
                               myInventory[slot].Quantity -= 1;
                               SpawnAEyeBoss(); //check error placing

                           }
                       }
                   }
               }
               else if (myInventory[slot].Item.ItemType == "Bow") //Bow
               {
                   isShooting = true;
                   aimLocksFacing = true; //lock until swing ends

                    bool aimRight;
                    if (CurrentMousePos.X - (myPlayer.playerX - cameraCenter.X
+ halfCameraWidth) >= 0)
                    {
                        aimRight = true;
                    }
                    else
                    {
                        aimRight = false;
                    }

                      double angleDeg = GetAngleOfMouseToPlayer();
                      itemRotateTransform.Angle = aimRight ? -angleDeg :
angleDeg;

                      armRotateTransform.Angle = aimRight ? -angleDeg + 90 :
angleDeg - 90;
                    angleRad = aimRight ? -(angleDeg * Math.PI / 180) :
(angleDeg * Math.PI / 180);

                      if (aimRight != facingRight)
                      {
                          facingRight = aimRight;
                          ApplyFacingFlip();
                      }


                      //apply pivot/offsets that depend on facing
                      if (facingRight)
                      {
                          itemRotateTransform.CenterX = -6;
                          itemRotateTransform.CenterY = 12;
                          armRotateTransform.CenterX = -6;
                          armRotateTransform.CenterY = 12;

                             armleftoffset = 0;
                             dx = Math.Cos(angleRad) * rItem * -1.4 - 6 + (17 * ((-
1 *(angleDeg - 90))       / 180));
                             dy = Math.Sin(angleRad) * rItem * -1.4 - 1;

                      }
                      else
                      {
                          itemRotateTransform.CenterX = 13;
                          itemRotateTransform.CenterY = 9;
                          armRotateTransform.CenterX = 13;
                          armRotateTransform.CenterY = 13;
                          armleftoffset = -2;

                            dx = Math.Cos(angleRad) * rItem + (8 * ((angleDeg +
90) / 180));
                            dy = Math.Sin(angleRad) * rItem * 0.9 - 1;
                      }

                      //fire an arrow
                      if (shootCooldownTimer.ElapsedMilliseconds > 800)
                      {
                          //check if has arrows
                          foreach (var thisSlot in myInventory.PlayersItems)
                          {
                              if (thisSlot?.Item?.ItemType == "Arrow")
                              {


                                 int TotalArrowDamage = thisSlot.Item.Damage +
myInventory[slot].Item.Damage;
                                thisSlot.Quantity -= 1;
                                shootCooldownTimer.Restart();
                                FireArrow(angleDeg, aimRight,
thisSlot.ItemImage, TotalArrowDamage);
                                break;
                            }
                        }
                    }
                }



                Canvas.SetLeft(playerImageArm, myPlayer.playerX -
cameraCenter.X + halfCameraWidth - playerImageArm.Width / 2 -
myPlayer.LeftPadding + armleftoffset);
                Canvas.SetTop(playerImageArm, myPlayer.playerY -
cameraCenter.Y + halfCameraHeight - playerImageArm.Height / 2 -
myPlayer.TopPadding);

                Canvas.SetLeft(swingItemImage, myPlayer.playerX -
cameraCenter.X + halfCameraWidth - swingItemImage.Width / 2 -
myPlayer.LeftPadding - dx + armleftoffset);
                Canvas.SetTop(swingItemImage, myPlayer.playerY -
cameraCenter.Y + halfCameraHeight - swingItemImage.Height / 2 -
myPlayer.TopPadding - dy);
            }
            else
            {
                if (isArmVisibile) //set not visible if is
                {
                    isArmVisibile = false;
                    playerImageArm.Visibility = Visibility.Hidden;
                    swingItemImage.Visibility = Visibility.Hidden;
                    currentArmRotation = -50;
                    armRotateTransform.Angle = -50;
                }
                if (showHitboxes)
                {
                    ToolHitbox.Visibility = Visibility.Hidden;
                }

               aimLocksFacing = false;
               isShooting = false;
           }
           UpdateArrows();
       }
       private void HandleMouseClicks()
       {
           if (isLeftMouseHeldIn && currentMiningTile != null)
           {
               mouseMoved = true; //mouse counted as moved due to mining

            }
            if (mouseMoved || playerMoved)
            {
                UpdateInteractionOutline();
            }
            if (isLeftMouseHeldIn) //if holding in left mouse click
            {
                HandleBlockInteraction(); //place/break block, keep on
breaking/ placing if still held in
            }
        }
        private void UpdateEnemiesGameLoop()
        {
            if (currentEyeBoss != null)
            {
                if (MoonImage.Opacity != 0) //if night time
                {
                    UpdateEyeBoss();
                }
                else //boss despawns
                {
                    RemoveEyeBoss(currentEyeBoss);
                }
            }
            else //only spawn zombies if no boss
            {
                if (EnemyAmount < 3 && MoonImage.Opacity == 1 &&
ZombieSpawnTimer.ElapsedMilliseconds > 24000)
                {
                    ZombieSpawnTimer.Restart();
                    SpawnAZombie();
                    EnemyAmount += 1;
                }
            }


            UpdateEnemies();

            if (ApplyingPlayerKnockback)
            {
                if (isGrounded)
                {
                    ApplyingPlayerKnockback = false;
                }
                else                                               3D Loop when
                {                                                  called
                    systemShapes.Rectangle[] collidedTiles =
CheckCollisonWithTiles(myPlayer.playerX + (3 * KnockbackDirection),
myPlayer.playerY, 1, myPlayer.HitboxWidth, myPlayer.HitboxHeight); //only
create if moves to save proccesing power when still
                    if (collidedTiles != null)
                    {
                        if (KnockbackDirection == -1) //going left

                       {
                            myPlayer.playerX =
Canvas.GetLeft(collidedTiles[0]) + cameraCenter.X - (GameCanvas.Width / 2) +
collidedTiles[0].Width + (myPlayer.HitboxWidth / 2);

                        }
                        else
                        {
                            myPlayer.playerX =
Canvas.GetLeft(collidedTiles[0]) + cameraCenter.X - (GameCanvas.Width / 2) -
(myPlayer.HitboxWidth / 2);
                        }
                    }
                    else
                    {
                        myPlayer.playerX += 3 * KnockbackDirection;
                    }
                }

            }
        }
        private void CheckPlayerFallDamage()
        {
            if (!isGrounded && velocityY > 0)
            {
                distanceFallen += velocityY;
            }
            else if (isGrounded && distanceFallen > 0)
            {
                if (distanceFallen > Utilities.TileSize * 18)
                {
                    distanceFallen -= Utilities.TileSize * 18;
                    int LostHealth = ((int)Math.Round(distanceFallen /
((double)(Utilities.TileSize * 1.5)))) * 2; //every 1 and half tiles after
falling distance is 2 health lost
                    myPlayer.ChangePlayerHealth(-LostHealth);
                    RegainPlayerHealth.Restart();
                }
                distanceFallen = 0;
            }
        }
        private void CheckForPlayerRegainHealth()
        {
            if (RegainPlayerHealth.ElapsedMilliseconds > 6500)
            {
                if (!PlayerRegainHealthNow.IsRunning)
                {
                    PlayerRegainHealthNow.Start();
                }
                if (PlayerRegainHealthNow.ElapsedMilliseconds > 1000)
                {
                    myPlayer.ChangePlayerHealth(1);
                    PlayerRegainHealthNow.Restart();

                }
            }
            else
            {
                 if (PlayerRegainHealthNow.IsRunning)
                {
                     PlayerRegainHealthNow.Stop();
                }
            }
            int HealthPotionRegenSeconds =
(int)Math.Floor((double)(PotionUsedLast.ElapsedMilliseconds / 1000));
            if (HealthPotionRegenSeconds < TimeToNextPotionSecs &&
!NotUsedPotionYet)
            {
                myHealingTimer.HealthTimerText = $"{(TimeToNextPotionSecs -
HealthPotionRegenSeconds)}"; //countdown
                HealthTimerCanvas.Visibility = Visibility.Visible;
            }
            else
            {
                HealthTimerCanvas.Visibility = Visibility.Collapsed;
            }
        }
        private double GetAngleOfMouseToPlayer() //90 to -90
        {
            double playerWorldX = myPlayer.playerX - cameraCenter.X +
halfCameraWidth;
            double playerWorldY = myPlayer.playerY - cameraCenter.Y +
halfCameraHeight;
            double mouseWorldX = CurrentMousePos.X;
            double mouseWorldY = CurrentMousePos.Y;

            double dx = Math.Abs(mouseWorldX - playerWorldX);
            double dy = playerWorldY - mouseWorldY; //as up is negative

            double angle = Math.Atan2(dy , dx);
            double angleInDegrees = angle * 180 / Math.PI;
            return angleInDegrees;
        }

    }
}
