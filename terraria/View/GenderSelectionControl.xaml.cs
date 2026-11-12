using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace WPFTerraria.View
{

    public partial class GenderSelectionControl : UserControl
    {
        public Action<string> OnPlayerSelected { get; set; } //passes string
when action is given -> buttons pressed
        public GenderSelectionControl()
        {
            InitializeComponent();
            InitiliseGenderSelectionCanvas();
        }
        private void InitiliseGenderSelectionCanvas()
        {
            GenderSelectionWindow.Width = MainWindow.CameraWidth *
Utilities.TileSize;
            GenderSelectionWindow.Height = MainWindow.CameraHeight *
Utilities.TileSize;

            GenderSelectionCanvas.Width = MainWindow.CameraWidth *
Utilities.TileSize;
            GenderSelectionCanvas.Height = MainWindow.CameraHeight *
Utilities.TileSize;
            Canvas.SetLeft(GenderSelectionCanvas, 0); //set canvas
            Canvas.SetTop(GenderSelectionCanvas, 0);

           AddContent();
       }

       private void AddContent()
       {
           AddTitle();
           AddButton_Girl();
           AddImage_Girl();
           AddButton_Boy();
           AddImage_Boy();
       }

       private void AddTitle()
       {
           TextBlock txtTitle = new TextBlock();
           txtTitle.HorizontalAlignment = HorizontalAlignment.Center;
           txtTitle.Text = "Choose a Character to Play!";


           txtTitle.FontSize = GenderSelectionCanvas.Width / 15;
           txtTitle.FontWeight = FontWeights.Bold;
           txtTitle.Foreground = Brushes.White;

            //Force WPF to calculate ActualWidth
            txtTitle.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            txtTitle.Arrange(new Rect(txtTitle.DesiredSize));

            GenderSelectionCanvas.Children.Add(txtTitle);
            Canvas.SetLeft(txtTitle, ((GenderSelectionCanvas.Width / 2) -
(txtTitle.ActualWidth / 2)));
            Canvas.SetTop(txtTitle, (GenderSelectionCanvas.Height * 0.05));
        }
        private void AddButton_Girl()
        {
            Button btnGirl = new Button();
            btnGirl.HorizontalContentAlignment = HorizontalAlignment.Center;
            btnGirl.Content = "Play as Girl";

           btnGirl.FontSize = GenderSelectionCanvas.Width / 25;
           btnGirl.Foreground = Brushes.White;
           btnGirl.Background = Brushes.LightPink;
           btnGirl.Padding = new Thickness(10, 0, 10, 0);

            //force WPF to calculate ActualWidth
            btnGirl.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            btnGirl.Arrange(new Rect(btnGirl.DesiredSize));

            GenderSelectionCanvas.Children.Add(btnGirl);
            Canvas.SetLeft(btnGirl, ((GenderSelectionCanvas.Width / 5) -
(btnGirl.ActualWidth / 2)));
            Canvas.SetTop(btnGirl, (GenderSelectionCanvas.Height * 0.25));

           btnGirl.Click += GirlSelection_Click;
           Canvas.SetZIndex(btnGirl, 9);
       }

        private void GirlSelection_Click(object sender, RoutedEventArgs e)
        {
            OnPlayerSelected?.Invoke("Girl Character"); //sends back "Girl
Character"
            this.CleanUpGenderMenu();
        }


        private void AddImage_Girl()
        {
            string PicturePath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "PixelArt",
"Player", "GirlStanding.png");
            BitmapSource NormalImage = Utilities.LoadImage(PicturePath);

            BitmapSource ResizedImage = Utilities.ResizeImage(NormalImage,
GenderSelectionCanvas.Width * 0.385, GenderSelectionCanvas.Height * 0.72);


           Image GirlImage = new Image
           {
               Source = ResizedImage,
               Width = GenderSelectionCanvas.Width * 0.385,
               Height = GenderSelectionCanvas.Height * 0.72,
           };

            GirlImage.HorizontalAlignment = HorizontalAlignment.Center;
            //force WPF to calculate ActualWidth
            GirlImage.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            GirlImage.Arrange(new Rect(GirlImage.DesiredSize));

            GenderSelectionCanvas.Children.Add(GirlImage);
            Canvas.SetLeft(GirlImage, ((GenderSelectionCanvas.Width / 5) -
(GirlImage.ActualWidth / 2)));
            Canvas.SetTop(GirlImage, (GenderSelectionCanvas.Height * 0.18));

       }
       private void AddButton_Boy()
       {
           Button btnBoy = new Button();
           btnBoy.HorizontalContentAlignment = HorizontalAlignment.Center;
           btnBoy.Content = "Play as Boy";

           btnBoy.FontSize = GenderSelectionCanvas.Width / 25;
           btnBoy.Foreground = Brushes.White;
           btnBoy.Background = Brushes.Blue;
           btnBoy.Padding = new Thickness(10, 0, 10, 0);

            //force WPF to calculate ActualWidth
            btnBoy.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            btnBoy.Arrange(new Rect(btnBoy.DesiredSize));

            GenderSelectionCanvas.Children.Add(btnBoy);
            Canvas.SetLeft(btnBoy, ((GenderSelectionCanvas.Width / 1.25) -
(btnBoy.ActualWidth / 2)));
            Canvas.SetTop(btnBoy, (GenderSelectionCanvas.Height * 0.25));

           btnBoy.Click += BoySelection_Click;
           Canvas.SetZIndex(btnBoy, 9);
        }
        private void AddImage_Boy()
        {
            string PicturePath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "PixelArt",
"Player", "BoyStanding.png");
            BitmapSource NormalImage = Utilities.LoadImage(PicturePath);

            BitmapSource ResizedImage = Utilities.ResizeImage(NormalImage,
GenderSelectionCanvas.Width * 0.385, GenderSelectionCanvas.Height * 0.71);


           Image BoyImage = new Image
           {
               Source = ResizedImage,
               Width = GenderSelectionCanvas.Width * 0.385,
               Height = GenderSelectionCanvas.Height * 0.71,
           };

            BoyImage.HorizontalAlignment = HorizontalAlignment.Center;
            //force WPF to calculate ActualWidth
            BoyImage.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            BoyImage.Arrange(new Rect(BoyImage.DesiredSize));

            GenderSelectionCanvas.Children.Add(BoyImage);
            Canvas.SetLeft(BoyImage, ((GenderSelectionCanvas.Width / 1.25) -
(BoyImage.ActualWidth / 2)));
            Canvas.SetTop(BoyImage, (GenderSelectionCanvas.Height * 0.195));

       }

        private void BoySelection_Click(object sender, RoutedEventArgs e)
        {
            OnPlayerSelected?.Invoke("Boy Character"); //sends back "Boy
Character"
            this.CleanUpGenderMenu();
        }
        private void CleanUpGenderMenu()
        {
            //NOTE -> if use images clear here
            GenderSelectionCanvas.Children.Clear();
            GenderSelectionCanvas.Visibility = Visibility.Collapsed;
            //force garbage collection for this menu
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
