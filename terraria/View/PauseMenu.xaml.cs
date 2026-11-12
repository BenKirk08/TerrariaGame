using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFTerraria.View
{
    public partial class PauseMenu : UserControl
    {
        public PauseMenu()
        {
            InitializeComponent();
            InitiliseMenu();
        }

        public double PauseMenuDisplayWidth { get; private set; }
        public double PauseMenuDisplayHeight { get; private set; }

        private void InitiliseMenu()
        {
            PauseMenuDisplayWidth = MainWindow.CameraWidth *
Utilities.TileSize * 0.75;
            PauseMenuDisplayHeight = MainWindow.CameraHeight *
Utilities.TileSize * 0.75;

           PauseMenuCanvas.Width = PauseMenuDisplayWidth;
           PauseMenuCanvas.Height = PauseMenuDisplayHeight;

            PauseMenuCanvas.Background = new
SolidColorBrush(Color.FromArgb(170, 255, 209, 223));

           AddButtonResumeGame();
           AddButtonReturnMenu();
           AddButtonSaveGame();
           AddButtonExitGame();
        }
        private void AddButtonResumeGame()
        {
            Button btnResumeGame = new Button();
            btnResumeGame.HorizontalContentAlignment =
HorizontalAlignment.Center;
            btnResumeGame.Content = "Resume Game";

           btnResumeGame.FontSize = PauseMenuCanvas.Width / 25;
           btnResumeGame.Foreground = Brushes.White;
           btnResumeGame.Background = Brushes.LightPink;
           btnResumeGame.Padding = new Thickness(10, 0, 10, 0);

            //force WPF to calculate ActualWidth
            btnResumeGame.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));

              btnResumeGame.Arrange(new Rect(btnResumeGame.DesiredSize));

            PauseMenuCanvas.Children.Add(btnResumeGame);
            Canvas.SetLeft(btnResumeGame, ((PauseMenuCanvas.Width / 2) -
(btnResumeGame.ActualWidth / 2)));
            Canvas.SetTop(btnResumeGame, (PauseMenuCanvas.Height * 0.05));

              btnResumeGame.Click += ResumeGame_Click;
          }
          private void ResumeGame_Click(object sender, RoutedEventArgs e)
          {
              var main = Application.Current.MainWindow as MainWindow;
              main.TogglePauseMenu();
          }

        private void AddButtonReturnMenu()
        {
            Button btnReturnGame = new Button();
            btnReturnGame.HorizontalContentAlignment =
HorizontalAlignment.Center;
            btnReturnGame.Content = "Return to Main Menu";

              btnReturnGame.FontSize = PauseMenuCanvas.Width / 25;
              btnReturnGame.Foreground = Brushes.White;
              btnReturnGame.Background = Brushes.LightPink;
              btnReturnGame.Padding = new Thickness(10, 0, 10, 0);

            //force WPF to calculate ActualWidth
            btnReturnGame.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            btnReturnGame.Arrange(new Rect(btnReturnGame.DesiredSize));

            PauseMenuCanvas.Children.Add(btnReturnGame);
            Canvas.SetLeft(btnReturnGame, ((PauseMenuCanvas.Width / 2) -
(btnReturnGame.ActualWidth / 2)));
            Canvas.SetTop(btnReturnGame, (PauseMenuCanvas.Height * 0.3));

              btnReturnGame.Click += ReturnMenu_Click;
          }
          private void ReturnMenu_Click(object sender, RoutedEventArgs e)
          {
              var main = Application.Current.MainWindow as MainWindow; //for
cleanup

              main.CleanUpGame(); //clear the memory

              //deeper cleanup before loading menu
              GC.Collect();
              GC.WaitForPendingFinalizers();
              GC.Collect(); //run GC twice -> better cleanup

              main.ChooseGame();
          }

        private void AddButtonSaveGame()
        {
            Button btnSaveGame = new Button();
            btnSaveGame.HorizontalContentAlignment =
HorizontalAlignment.Center;
            btnSaveGame.Content = "Save This Game";

           btnSaveGame.FontSize = PauseMenuCanvas.Width / 25;
           btnSaveGame.Foreground = Brushes.White;
           btnSaveGame.Background = Brushes.LightPink;
           btnSaveGame.Padding = new Thickness(10, 0, 10, 0);

            //force WPF to calculate ActualWidth
            btnSaveGame.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            btnSaveGame.Arrange(new Rect(btnSaveGame.DesiredSize));

            PauseMenuCanvas.Children.Add(btnSaveGame);
            Canvas.SetLeft(btnSaveGame, ((PauseMenuCanvas.Width / 2) -
(btnSaveGame.ActualWidth / 2)));
            Canvas.SetTop(btnSaveGame, (PauseMenuCanvas.Height * 0.55));

           btnSaveGame.Click += SaveGame_Click;
       }
       private void SaveGame_Click(object sender, RoutedEventArgs e)
       {
           var main = Application.Current.MainWindow as MainWindow;
           main.SaveGame();
           Console.WriteLine("Saved Game");

        }
        private void AddButtonExitGame()
        {
            Button btnExitGame = new Button();
            btnExitGame.HorizontalContentAlignment =
HorizontalAlignment.Center;
            btnExitGame.Content = "Exit Game";

           btnExitGame.FontSize = PauseMenuCanvas.Width / 25;
           btnExitGame.Foreground = Brushes.White;
           btnExitGame.Background = Brushes.LightPink;
           btnExitGame.Padding = new Thickness(10, 0, 10, 0);

            //force WPF to calculate ActualWidth
            btnExitGame.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            btnExitGame.Arrange(new Rect(btnExitGame.DesiredSize));

            PauseMenuCanvas.Children.Add(btnExitGame);
            Canvas.SetLeft(btnExitGame, ((PauseMenuCanvas.Width / 2) -
(btnExitGame.ActualWidth / 2)));
            Canvas.SetTop(btnExitGame, (PauseMenuCanvas.Height * 0.8));

            btnExitGame.Click += ExitGame_Click;
        }
        private void ExitGame_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(); //close application
        }
    }
}
