using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace WPFTerraria.View
{
    public partial class MapSizeMenu : UserControl
    {
        public Action<string> OnMapSizeSelected { get; set; } //passes string
when action is given -> buttons pressed
        public MapSizeMenu()
        {
            InitializeComponent();
            InitiliseGenderSelectionCanvas();
        }

        private void InitiliseGenderSelectionCanvas()
        {
            MapSelectionWindow.Width = MainWindow.CameraWidth *
Utilities.TileSize;
            MapSelectionWindow.Height = MainWindow.CameraHeight *
Utilities.TileSize;

            MapSelectionCanvas.Width = MainWindow.CameraWidth *
Utilities.TileSize;
            MapSelectionCanvas.Height = MainWindow.CameraHeight *
Utilities.TileSize;
            Canvas.SetLeft(MapSelectionCanvas, 0); //set canvas
            Canvas.SetTop(MapSelectionCanvas, 0);

           AddContent();
       }

       private void AddContent()
       {
           AddTitle();
           AddButton_Small();
           AddImage_Small();
           AddButton_Medium();
           AddImage_Medium();
           AddButton_Large();
           AddImage_Large();
       }

       private void AddTitle()
       {
           TextBlock txtTitle = new TextBlock();
           txtTitle.HorizontalAlignment = HorizontalAlignment.Center;

           txtTitle.Text = "Choose a Map Size to Play On!";

           txtTitle.FontSize = MapSelectionCanvas.Width / 15;
           txtTitle.FontWeight = FontWeights.Bold;
           txtTitle.Foreground = Brushes.DeepSkyBlue;

            //Force WPF to calculate ActualWidth
            txtTitle.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            txtTitle.Arrange(new Rect(txtTitle.DesiredSize));

            MapSelectionCanvas.Children.Add(txtTitle);
            Canvas.SetLeft(txtTitle, ((MapSelectionCanvas.Width / 2) -
(txtTitle.ActualWidth / 2)));
            Canvas.SetTop(txtTitle, (MapSelectionCanvas.Height * 0.05));
        }
        private void AddButton_Small()
        {
            Button btnSmall = new Button();
            btnSmall.HorizontalContentAlignment = HorizontalAlignment.Center;
            btnSmall.Content = "Small";

           btnSmall.FontSize = MapSelectionCanvas.Width / 25;
           btnSmall.Foreground = Brushes.White;
           btnSmall.Background = Brushes.LimeGreen;
           btnSmall.Padding = new Thickness(10, 0, 10, 0);

            //force WPF to calculate ActualWidth
            btnSmall.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            btnSmall.Arrange(new Rect(btnSmall.DesiredSize));

            MapSelectionCanvas.Children.Add(btnSmall);
            Canvas.SetLeft(btnSmall, ((MapSelectionCanvas.Width / 5) -
(btnSmall.ActualWidth / 2)));
            Canvas.SetTop(btnSmall, (MapSelectionCanvas.Height * 0.25));

           btnSmall.Click += SmallSelection_Click;
           Canvas.SetZIndex(btnSmall, 9);
       }

       private void SmallSelection_Click(object sender, RoutedEventArgs e)
       {
           OnMapSizeSelected?.Invoke("Small Map"); //sends back "Small Map"
           this.CleanUpGenderMenu();
       }


        private void AddImage_Small()
        {
            string PicturePath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "Content",
"Images", "Item_24.png");

            BitmapSource NormalImage = Utilities.LoadImage(PicturePath);
            BitmapSource ResizedImage = Utilities.ResizeImage(NormalImage,
MapSelectionCanvas.Width * 0.06, MapSelectionCanvas.Height * 0.1);
            Image SmallImage = new Image
            {
                Source = ResizedImage,
                Width = MapSelectionCanvas.Width * 0.06,
                Height = MapSelectionCanvas.Height * 0.1,
            };

            SmallImage.HorizontalAlignment = HorizontalAlignment.Center;
            //force WPF to calculate ActualWidth
            SmallImage.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            SmallImage.Arrange(new Rect(SmallImage.DesiredSize));

            MapSelectionCanvas.Children.Add(SmallImage);
            Canvas.SetLeft(SmallImage, ((MapSelectionCanvas.Width / 5) -
(SmallImage.ActualWidth / 2)));
            Canvas.SetTop(SmallImage, (MapSelectionCanvas.Height * 0.5));

       }
       private void AddButton_Medium()
       {
           Button btnMedium = new Button();
           btnMedium.HorizontalContentAlignment = HorizontalAlignment.Center;
           btnMedium.Content = "Medium";

           btnMedium.FontSize = MapSelectionCanvas.Width / 25;
           btnMedium.Foreground = Brushes.White;
           btnMedium.Background = Brushes.LightBlue;
           btnMedium.Padding = new Thickness(10, 0, 10, 0);

            //force WPF to calculate ActualWidth
            btnMedium.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            btnMedium.Arrange(new Rect(btnMedium.DesiredSize));

            MapSelectionCanvas.Children.Add(btnMedium);
            Canvas.SetLeft(btnMedium, ((MapSelectionCanvas.Width / 2) -
(btnMedium.ActualWidth / 2)));
            Canvas.SetTop(btnMedium, (MapSelectionCanvas.Height * 0.25));

           btnMedium.Click += MediumSelection_Click;
           Canvas.SetZIndex(btnMedium, 9);
       }
       private void MediumSelection_Click(object sender, RoutedEventArgs e)
       {
           OnMapSizeSelected?.Invoke("Medium Map"); //sends back "Medium Map"
           this.CleanUpGenderMenu();
       }
       private void AddImage_Medium()
       {

            string PicturePath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "Content",
"Images", "Item_24.png");
            BitmapSource NormalImage = Utilities.LoadImage(PicturePath);
            BitmapSource ResizedImage = Utilities.ResizeImage(NormalImage,
MapSelectionCanvas.Width * 0.09, MapSelectionCanvas.Height * 0.15);
            Image MediumImage = new Image
            {
                Source = ResizedImage,
                Width = MapSelectionCanvas.Width * 0.09,
                Height = MapSelectionCanvas.Height * 0.15,
            };

            MediumImage.HorizontalAlignment = HorizontalAlignment.Center;
            //force WPF to calculate ActualWidth
            MediumImage.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            MediumImage.Arrange(new Rect(MediumImage.DesiredSize));

            MapSelectionCanvas.Children.Add(MediumImage);
            Canvas.SetLeft(MediumImage, ((MapSelectionCanvas.Width / 2) -
(MediumImage.ActualWidth / 2)));
            Canvas.SetTop(MediumImage, (MapSelectionCanvas.Height * 0.475));

       }
       private void AddButton_Large()
       {
           Button btnLarge = new Button();
           btnLarge.HorizontalContentAlignment = HorizontalAlignment.Center;
           btnLarge.Content = "Large";

           btnLarge.FontSize = MapSelectionCanvas.Width / 25;
           btnLarge.Foreground = Brushes.White;
           btnLarge.Background = Brushes.IndianRed;
           btnLarge.Padding = new Thickness(10, 0, 10, 0);

            //force WPF to calculate ActualWidth
            btnLarge.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            btnLarge.Arrange(new Rect(btnLarge.DesiredSize));

            MapSelectionCanvas.Children.Add(btnLarge);
            Canvas.SetLeft(btnLarge, ((MapSelectionCanvas.Width / 1.25) -
(btnLarge.ActualWidth / 2)));
            Canvas.SetTop(btnLarge, (MapSelectionCanvas.Height * 0.25));

           btnLarge.Click += LargeSelection_Click;
           Canvas.SetZIndex(btnLarge, 9);
       }
       private void AddImage_Large()
       {



            string PicturePath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "Content",
"Images", "Item_24.png");
            BitmapSource NormalImage = Utilities.LoadImage(PicturePath);
            BitmapSource ResizedImage = Utilities.ResizeImage(NormalImage,
MapSelectionCanvas.Width * 0.12, MapSelectionCanvas.Height * 0.2);
            Image MediumImage = new Image
            {
                Source = ResizedImage,
                Width = MapSelectionCanvas.Width * 0.12,
                Height = MapSelectionCanvas.Height * 0.2,
            };

            MediumImage.HorizontalAlignment = HorizontalAlignment.Center;
            //force WPF to calculate ActualWidth
            MediumImage.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            MediumImage.Arrange(new Rect(MediumImage.DesiredSize));

            MapSelectionCanvas.Children.Add(MediumImage);
            Canvas.SetLeft(MediumImage, ((MapSelectionCanvas.Width / 1.25) -
(MediumImage.ActualWidth / 2)));
            Canvas.SetTop(MediumImage, (MapSelectionCanvas.Height * 0.45));
        }

        private void LargeSelection_Click(object sender, RoutedEventArgs e)
        {
            OnMapSizeSelected?.Invoke("Large Map"); //sends back "Large Map"
            this.CleanUpGenderMenu();
        }
        private void CleanUpGenderMenu()
        {
            //NOTE -> if use images clear here
            MapSelectionCanvas.Children.Clear();
            MapSelectionCanvas.Visibility = Visibility.Collapsed;
            //force garbage collection for this menu
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
