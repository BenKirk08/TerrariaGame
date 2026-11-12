using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WPFTerraria
{
    public class Player
    {
        //player base settings
        public double playerX { get; set; } //helps w/ camera
        public double playerY { get; set; } //NOT TOP LEFT BUT MIDDLE
        public int playerSpeed = 4;

        private string[] StancesStrings = { "BoyArm", "BoyJumping",
"BoyStanding", "BoyRun1", "BoyRun2", "BoyRun3", "BoyRun4", "BoyRun5",
"BoyRun6", "BoyJumpingSwing", "BoyStandingSwing", "BoyRun1Swing",
"BoyRun2Swing", "BoyRun3Swing", "BoyRun4Swing", "BoyRun5Swing",
"BoyRun6Swing", "BoyJumpingShoot", "BoyStandingShoot", "BoyRun1Shoot",
"BoyRun2Shoot", "BoyRun3Shoot", "BoyRun4Shoot", "BoyRun5Shoot",
"BoyRun6Shoot", "GirlArm", "GirlJumping", "GirlStanding", "GirlRun1",
"GirlRun2", "GirlRun3", "GirlRun4", "GirlRun5", "GirlRun6",
"GirlJumpingSwing", "GirlStandingSwing", "GirlRun1Swing", "GirlRun2Swing",
"GirlRun3Swing", "GirlRun4Swing", "GirlRun5Swing", "GirlRun6Swing",
"GirlJumpingShoot", "GirlStandingShoot", "GirlRun1Shoot", "GirlRun2Shoot",
"GirlRun3Shoot", "GirlRun4Shoot", "GirlRun5Shoot", "GirlRun6Shoot", };
//player stances

        //resized hitbox & image for player
        public int ImageResizedWidth { get; }
        public int ImageResizedHeight { get; }

        public double HitboxWidth { get; }
        public double HitboxHeight { get; }

        //padding/empty pixels in image
        public double LeftPadding = -2; //adjusted to specific image //-
1,16.2,3,0
        public double RightPadding = 29;
        public double TopPadding = 10;
        public double BottomPadding = 0; //unknown

        protected BitmapSource NormalImage;
        protected BitmapSource ResizedImage;
        protected Dictionary<string, BitmapSource> Player_Pictures = new
Dictionary<string, BitmapSource>();
        public Player()
        {
            ImageResizedWidth = (int)Math.Round(Utilities.TileSize *
(double)3.49, 0);

            ImageResizedHeight = (int)Math.Round(Utilities.TileSize *
(double)3.65, 0);
            LoadAllPlayerImages();

           HitboxWidth = (double)Utilities.TileSize * 1.6; //1.6 tiles wide
           HitboxHeight = (double)Utilities.TileSize * 2.8; //2.8 tiles tall
           _playerMaxHealth = 100;
           ChangePlayerMaxHealth(0);
           _playerHealth = 100;
           ChangePlayerHealth(0); // -> call to update

        }
        private void LoadAllPlayerImages()
        {
            foreach (string stance in StancesStrings)
            {
                string PicturePath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "PixelArt",
"Player", stance + ".png");
                NormalImage = Utilities.LoadImage(PicturePath);
                ResizedImage = Utilities.ResizeImage(NormalImage,
ImageResizedWidth, ImageResizedHeight);
                Player_Pictures.Add(stance, ResizedImage);
            }
        }

        public Rect Hitbox
        {
            get
            {
                return new Rect(
                    playerX - (HitboxWidth / 2), //these numbers are based on
the pngs image -> theres empty space
                    playerY - (HitboxHeight / 2),
                    HitboxWidth,
                    HitboxHeight
                );
            }
        }
        public Rect playerHitboxCenter
        {
            get
            {
                return new Rect(
                    playerX - 2.5, //half of size
                    playerY - 2.5,
                    5,
                    5
                );
            }
        }



        public BitmapSource GetPlayerImage(string stanceType) //returns image
of that stance type from dictionary
        {
            try
            {
                return Player_Pictures[stanceType];
            }
            catch
            { return null; }
        }



        public int _playerHealth;
        public int _playerMaxHealth;
        public void ChangePlayerHealth(int healthChange)
        {
            var main = Application.Current.MainWindow as MainWindow;
            if (healthChange < 0)
            {
                main.RegainPlayerHealth.Restart();
            }
            int newHealth = Math.Min(_playerMaxHealth, (Math.Max(0,
(_playerHealth + healthChange))));
            main.myPlayerHealthBar.PlayerHealth = newHealth;
            _playerHealth = newHealth;
        }
        private void ChangePlayerMaxHealth(int maxHealthChange)
        {
            var main = Application.Current.MainWindow as MainWindow;
            main.myPlayerHealthBar.MaxHealth = _playerMaxHealth +
maxHealthChange;
            _playerMaxHealth = _playerMaxHealth + maxHealthChange;
        }
    }
}
