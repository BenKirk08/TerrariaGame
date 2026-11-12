using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace WPFTerraria.View
{
    public partial class MainMenuControl : UserControl
    {
        public Action<string> OnMenuSelected { get; set; } //passes string
when action is given -> buttons pressed
        public MainMenuControl()
        {
            InitializeComponent();
            InitiliseMenuCanavas();
        }
        private void InitiliseMenuCanavas()
        {
            MainMenuWindow.Width = MainWindow.CameraWidth *
Utilities.TileSize;
            MainMenuWindow.Height = MainWindow.CameraHeight *
Utilities.TileSize;

            MainMenusCanvas.Width = MainWindow.CameraWidth *
Utilities.TileSize;
            MainMenusCanvas.Height = MainWindow.CameraHeight *
Utilities.TileSize;
            Canvas.SetLeft(MainMenusCanvas, 0); //set mainmenu canvas
            Canvas.SetTop(MainMenusCanvas, 0);

           AddContent();
       }

       private void AddContent()
       {
           AddTitle();
           AddButtonNewGame();
           AddButtonLoadGame();
           AddButtonExitGame();
       }
       private void AddTitle()
       {
           TextBlock txtTitle = new TextBlock();
           txtTitle.HorizontalAlignment = HorizontalAlignment.Center;
           txtTitle.Text = "Ben Kirk's Terraria CourseWork!";

           txtTitle.FontSize = MainMenusCanvas.Width / 20;
           txtTitle.FontWeight = FontWeights.Bold;
           txtTitle.Foreground = Brushes.White;

            // Force WPF to calculate ActualWidth


            txtTitle.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            txtTitle.Arrange(new Rect(txtTitle.DesiredSize));

            MainMenusCanvas.Children.Add(txtTitle);
            Canvas.SetLeft(txtTitle, ((MainMenusCanvas.Width / 2) -
(txtTitle.ActualWidth / 2)));
            Canvas.SetTop(txtTitle, (MainMenusCanvas.Height * 0.05));
        }
        private void AddButtonNewGame()
        {
            Button btnNewGame = new Button();
            btnNewGame.HorizontalContentAlignment =
HorizontalAlignment.Center;
            btnNewGame.Content = "New Game";

           btnNewGame.FontSize = MainMenusCanvas.Width / 25;
           btnNewGame.Foreground = Brushes.White;
           btnNewGame.Background = Brushes.Black;
           btnNewGame.Padding = new Thickness(10, 0, 10, 0);

            //force WPF to calculate ActualWidth
            btnNewGame.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            btnNewGame.Arrange(new Rect(btnNewGame.DesiredSize));

            MainMenusCanvas.Children.Add(btnNewGame);
            Canvas.SetLeft(btnNewGame, ((MainMenusCanvas.Width / 2) -
(btnNewGame.ActualWidth / 2)));
            Canvas.SetTop(btnNewGame, (MainMenusCanvas.Height * 0.25));

           btnNewGame.Click += NewGame_Click;
       }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            OnMenuSelected?.Invoke("New Game"); //sends back "New Game"
            this.CleanUpMainMenu();
        }
        private void AddButtonLoadGame()
        {
            Button btnLoadGame = new Button();
            btnLoadGame.HorizontalContentAlignment =
HorizontalAlignment.Center;
            btnLoadGame.Content = "Load Game";

           btnLoadGame.FontSize = MainMenusCanvas.Width / 25;
           btnLoadGame.Foreground = Brushes.White;
           btnLoadGame.Background = Brushes.Black;
           btnLoadGame.Padding = new Thickness(10, 0, 10, 0);

            //force WPF to calculate ActualWidth


            btnLoadGame.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            btnLoadGame.Arrange(new Rect(btnLoadGame.DesiredSize));

            MainMenusCanvas.Children.Add(btnLoadGame);
            Canvas.SetLeft(btnLoadGame, ((MainMenusCanvas.Width / 2) -
(btnLoadGame.ActualWidth / 2)));
            Canvas.SetTop(btnLoadGame, (MainMenusCanvas.Height * 0.5));

           btnLoadGame.Click += LoadGame_Click;
       }
       private void LoadGame_Click(object sender, RoutedEventArgs e)
       {
           //load saved game
           OnMenuSelected?.Invoke("Load Game"); //sends back "Load Game"
           this.CleanUpMainMenu();
       }

        private void AddButtonExitGame()
        {
            Button btnExitGame = new Button();
            btnExitGame.HorizontalContentAlignment =
HorizontalAlignment.Center;
            btnExitGame.Content = "Exit Game";

           btnExitGame.FontSize = MainMenusCanvas.Width / 25;
           btnExitGame.Foreground = Brushes.White;
           btnExitGame.Background = Brushes.Red;
           btnExitGame.Padding = new Thickness(10, 0, 10, 0);

            //force WPF to calculate ActualWidth
            btnExitGame.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            btnExitGame.Arrange(new Rect(btnExitGame.DesiredSize));

            MainMenusCanvas.Children.Add(btnExitGame);
            Canvas.SetLeft(btnExitGame, ((MainMenusCanvas.Width / 2) -
(btnExitGame.ActualWidth / 2)));
            Canvas.SetTop(btnExitGame, (MainMenusCanvas.Height * 0.75));

           btnExitGame.Click += ExitGame_Click;
       }
       private void ExitGame_Click(object sender, RoutedEventArgs e)
       {
           Application.Current.Shutdown(); //close application
       }

       private void CleanUpMainMenu()
       {
           //NOTE -> if use images clear here
           MainMenusCanvas.Children.Clear();
           MainMenusCanvas.Visibility = Visibility.Collapsed;
           //force garbage collection for this pause menu

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
