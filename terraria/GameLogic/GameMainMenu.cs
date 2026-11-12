using System;
using System.Windows.Controls;
using System.Windows;
using WPFTerraria.View;
using System.IO;
using System.Windows.Threading;


namespace WPFTerraria
{
    public partial class MainWindow
    {
        MainMenuControl myMainMenu;
        GenderSelectionControl myGenderSelectionMenu;
        MapSizeMenu myMapSizeMenu;

        private string selectedOption; //choice made -> null means no choice
made yet
        private string selectedGender;
        private string selectedMapSize;
        private string WaitForMenuChoice()
        {
            selectedOption = null;
            InitililiseMainMenu();

             //wait until the user clicks button -> ensures ui responsive while
waiting
            while (selectedOption == null)
            {
                DispatcherFrame frame = new DispatcherFrame(); //temporarily
stop processing until something happens

Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                                                new Action(() =>
frame.Continue = false)); //after done all other tasks at hand -> stop frame
until something happens
                Dispatcher.PushFrame(frame); //pause program until dispatcher
lets continue.
            }

             //now selectedOption contains "New Game" or "Load Game" as not ==
null
             Console.WriteLine($"Selected option: {selectedOption}"); //write
in console

            MainMenuCanvas.Children.Clear();
            MainMenuCanvas.Visibility = Visibility.Collapsed; //stop showing
main menu canvas
            //force garbage collection for this menu
            GC.Collect();


              GC.WaitForPendingFinalizers();

              return selectedOption; //return selected option as string
        }
        private void InitililiseMainMenu()
        {
            CleanUpGame(); //cleanup game when called to make sure nothing in
variables

              myMainMenu = new MainMenuControl();

            //store selected option as string
            myMainMenu.OnMenuSelected = (choice) => selectedOption = choice;
//when OnMenuSelected is called update selectedOption with choice

              MainMenuCanvas.Width = CameraWidth * Utilities.TileSize;
              MainMenuCanvas.Height = CameraHeight * Utilities.TileSize;

              MainMenuCanvas.Children.Add(myMainMenu);

              Canvas.SetLeft(MainMenuCanvas, 0);
              Canvas.SetTop(MainMenuCanvas, 0);

              Canvas.SetLeft(myMainMenu, 0);
              Canvas.SetTop(myMainMenu, 0);

              MainMenuCanvas.Visibility = Visibility.Visible;
          }

          private string WaitForGenderChoice()
          {
              selectedGender = null;
              InitililisGenderSelectionMenu();

              //wait until the user clicks button -> ensures ui responsive while
waiting
            while (selectedGender == null)
            {
                DispatcherFrame frame = new DispatcherFrame(); //temporarily
stop processing until something happens

Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                                                new Action(() =>
frame.Continue = false)); //after done all other tasks at hand -> stop frame
until something happens
                Dispatcher.PushFrame(frame); //pause program until dispatcher
lets continue.
            }

              Console.WriteLine($"Selected gender: {selectedGender}"); //write
in console

              GenderSelectionCanvas.Children.Clear();

            GenderSelectionCanvas.Visibility = Visibility.Collapsed; //stop
showing Gender selction canvas
            //force garbage collection for this menu
            GC.Collect();
            GC.WaitForPendingFinalizers();

              return selectedGender; //return selected gender as string
          }

          private void InitililisGenderSelectionMenu()
          {
              myGenderSelectionMenu = new GenderSelectionControl();

            //store selected option as string
            myGenderSelectionMenu.OnPlayerSelected = (choice) =>
selectedGender = choice;

              GenderSelectionCanvas.Width = CameraWidth * Utilities.TileSize;
              GenderSelectionCanvas.Height = CameraHeight * Utilities.TileSize;

              GenderSelectionCanvas.Children.Add(myGenderSelectionMenu);

              Canvas.SetLeft(GenderSelectionCanvas, 0);
              Canvas.SetTop(GenderSelectionCanvas, 0);

              Canvas.SetLeft(myGenderSelectionMenu, 0);
              Canvas.SetTop(myGenderSelectionMenu, 0);

              GenderSelectionCanvas.Visibility = Visibility.Visible;
          }
          private void InitililiseMapSelectionMenu()
          {
              myMapSizeMenu = new MapSizeMenu();

              //store selected option as string
              myMapSizeMenu.OnMapSizeSelected = (choice) => selectedMapSize =
choice;

              MapSizeCanvas.Width = CameraWidth * Utilities.TileSize;
              MapSizeCanvas.Height = CameraHeight * Utilities.TileSize;

              MapSizeCanvas.Children.Add(myMapSizeMenu);

              Canvas.SetLeft(MapSizeCanvas, 0);
              Canvas.SetTop(MapSizeCanvas, 0);

              Canvas.SetLeft(myMapSizeMenu, 0);
              Canvas.SetTop(myMapSizeMenu, 0);

              GenderSelectionCanvas.Visibility = Visibility.Visible;
          }
          private string WaitForMapSizeChoice()
          {

              selectedMapSize = null;
              InitililiseMapSelectionMenu();

              //wait until the user clicks button -> ensures ui responsive while
waiting
            while (selectedMapSize == null)
            {
                DispatcherFrame frame = new DispatcherFrame(); //temporarily
stop processing until something happens

Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                                                new Action(() =>
frame.Continue = false)); //after done all other tasks at hand -> stop frame
until something happens
                Dispatcher.PushFrame(frame); //pause program until dispatcher
lets continue.
            }

              Console.WriteLine($"Selected mapSize: {selectedMapSize}"); //write
in console

            MapSizeCanvas.Children.Clear();
            MapSizeCanvas.Visibility = Visibility.Collapsed; //stop showing
Gender selction canvas
            //force garbage collection for this menu
            GC.Collect();
            GC.WaitForPendingFinalizers();

              return selectedMapSize; //return selected gender as string
          }

    }
}
