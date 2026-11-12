using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.ComponentModel;

namespace WPFTerraria
{
    public partial class HealingTimer : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int HealthTimerCanvasWidth;
        public int HealthTimerCanvasHeight;
        public HealingTimer()
        {
            DataContext = this;
            InitializeComponent();
            HealthTimerCanvasWidth = 22;
            HealthTimerCanvasHeight = 30;
            AddUI();

        }
        private void AddUI()
        {
            Image HealingPotionImage = new Image
            {
                Source =
Utilities.LoadImage((Path.Combine(Utilities.GetSolutionDirectory(),
"AllGameAssets", "Content", "Images", "Item_28.png"))),
                Width = HealthTimerCanvasWidth,
                Height = HealthTimerCanvasHeight,
                Stretch = Stretch.Fill
            };
            HealingTimerCanvas.Children.Add(HealingPotionImage);
            Canvas.SetZIndex(HealingPotionImage, -1);
        }

       private string _healthTimerText;
       public string HealthTimerText
       {
           get { return _healthTimerText; }
           set
           {
               _healthTimerText = value;
               OnPropertyChanged(nameof(HealthTimerText));
           }
       }

       private void OnPropertyChanged(string propertyName) //to simplify
       {


            PropertyChanged?.Invoke(this, new
PropertyChangedEventArgs(propertyName));
        }
    }
}
