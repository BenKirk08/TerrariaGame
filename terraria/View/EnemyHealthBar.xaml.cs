using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace WPFTerraria
{
    public partial class EnemyHealthBar : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public double HealthBarWidth { get; private set; }
        public double HealthBarHeight { get; private set; }

       public EnemyHealthBar()
       {
           InitializeComponent();
           DataContext = this;
           HealthBarWidth = 45;
           HealthBarHeight = 8;

       }
       private int _enemyHealth;
       public int EnemyHealth
       {
           get { return _enemyHealth; }
           set
           {
               _enemyHealth = value;
               UpdateHealthTextNew(_enemyHealth);
               if (_enemyMaxHealth != 0)
               {

                    EnemyHealthPercentage = (double)_enemyHealth /
(double)_enemyMaxHealth * 100;

               }
           }
       }

       private double _enemyHealthPercentage;
       public double EnemyHealthPercentage
       {
           get { return _enemyHealthPercentage; }
           set
           {
               _enemyHealthPercentage = value;
               OnPropertyChanged(nameof(EnemyHealthPercentage));
           }
       }
       private string _enemyHealthText;
       public string EnemyHealthTxt
       {

           get { return _enemyHealthText; }
           set
           {
               _enemyHealthText = value;
               OnPropertyChanged(nameof(EnemyHealthTxt));
           }
        }
        private int _enemyMaxHealth;
        public int EnemyMaxHealth
        {
            get { return _enemyMaxHealth; }
            set
            {
                _enemyMaxHealth = value;
                UpdateHealthTextMax(_enemyMaxHealth);
            }
        }
        private void UpdateHealthTextNew(int newHealth)
        {
            EnemyHealthTxt = $"{newHealth}/{_enemyMaxHealth}";
        }
        private void UpdateHealthTextMax(int newMaxHealth)
        {
            EnemyHealthTxt = $"{_enemyHealth}/{newMaxHealth}";
        }
        private void OnPropertyChanged(string propertyName) //to simplify
        {
            PropertyChanged?.Invoke(this, new
PropertyChangedEventArgs(propertyName));
        }
    }
}
