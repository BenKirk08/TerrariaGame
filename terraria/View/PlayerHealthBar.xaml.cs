using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace WPFTerraria
{

   public partial class PlayerHealthBar : UserControl, INotifyPropertyChanged
   {
       public event PropertyChangedEventHandler PropertyChanged;
       public double HealthBarWidth { get; private set; }
       public double HealthBarHeight { get; private set; }

       public PlayerHealthBar()
       {
           InitializeComponent();
           DataContext = this;
           HealthBarWidth = 250;
           HealthBarHeight = 25;

        }
        private int _playerHealth;
        public int PlayerHealth
        {
            get { return _playerHealth; }
            set
            {
                _playerHealth = value;
                UpdateHealthTextNew(_playerHealth);
                if (_maxHealth != 0)
                {
                    PlayerHealthPercentage = (double)_playerHealth /
(double)_maxHealth * 100;
                }
            }
        }
        private double _playerHealthPercentage;
        public double PlayerHealthPercentage
        {
            get { return _playerHealthPercentage; }
            set
            {
                _playerHealthPercentage = value;
                OnPropertyChanged(nameof(PlayerHealthPercentage));
            }
        }
        private string _healthText;
        public string HealthText
        {


           get { return _healthText; }
           set
           {
               _healthText = value;
               OnPropertyChanged(nameof(HealthText));
           }
        }
        private int _maxHealth;
        public int MaxHealth
        {
            get { return _maxHealth; }
            set
            {
                _maxHealth = value;
                UpdateHealthTextMax(_maxHealth);
            }
        }
        private void UpdateHealthTextNew(int newHealth)
        {
            HealthText = $"Health: {newHealth}/{_maxHealth}";
        }
        private void UpdateHealthTextMax(int newMaxHealth)
        {
            HealthText = $"Health: {_playerHealth}/{newMaxHealth}";
        }
        private void OnPropertyChanged(string propertyName) //to simplify
        {
            PropertyChanged?.Invoke(this, new
PropertyChangedEventArgs(propertyName));
        }
    }
}
