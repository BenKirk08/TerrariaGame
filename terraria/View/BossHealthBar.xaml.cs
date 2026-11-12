using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace WPFTerraria
{
    public partial class BossHealthBar : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public double HealthBarWidth { get; private set; }
        public double HealthBarHeight { get; private set; }

       public BossHealthBar()
       {
           InitializeComponent();
           DataContext = this;
           HealthBarWidth = MainWindow.CameraWidth * Utilities.TileSize / 2;
           HealthBarHeight = 34;

           EnemyHealthBarProgress.Width = HealthBarWidth;
           EnemyHealthBarProgress.Height = HealthBarHeight;

        }
        private int _bossHealth;
        public int BossHealth
        {
            get { return _bossHealth; }
            set
            {
                _bossHealth = value;
                UpdateHealthTextNew(_bossHealth);
                BossHealthPercentage = (double)_bossHealth /
(double)_bossMaxHealth * 100;
            }
        }

       private double _bossHealthPercentage;
       public double BossHealthPercentage
       {
           get { return _bossHealthPercentage; }
           set
           {
               _bossHealthPercentage = value;
               OnPropertyChanged(nameof(BossHealthPercentage));
           }
       }

       private string _bossHealthText;
       public string BossHealthTxt
       {

           get { return _bossHealthText; }
           set
           {
               _bossHealthText = value;
               OnPropertyChanged(nameof(BossHealthTxt));
           }
        }
        private int _bossMaxHealth;
        public int BossMaxHealth
        {
            get { return _bossMaxHealth; }
            set
            {
                _bossMaxHealth = value;
                UpdateHealthTextMax(_bossMaxHealth);
            }
        }
        private void UpdateHealthTextNew(int newHealth)
        {
            BossHealthTxt = $"{newHealth}/{_bossMaxHealth}";
        }
        private void UpdateHealthTextMax(int newMaxHealth)
        {
            BossHealthTxt = $"{_bossHealth}/{newMaxHealth}";
        }
        private void OnPropertyChanged(string propertyName) //to simplify
        {
            PropertyChanged?.Invoke(this, new
PropertyChangedEventArgs(propertyName));
        }

    }
}
