using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WPFTerraria.View
{
    public partial class RecipeSlotDisplay : UserControl,
INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

       public RecipeSlotDisplay()
       {
           InitializeComponent();
           DataContext = this; //bind ui to this object

            var slotBack =
Utilities.LoadImage(Path.Combine(Utilities.GetSolutionDirectory(),
"AllGameAssets", "Content", "Images", "UI", "Bestiary", "Slot_Back.png"));
//load slot background
            SlotBg.Source = slotBack; //set slot background image

            IngredientsList.ItemContainerGenerator.StatusChanged += (s, e) =>
//when ingredient slots generated
            {
                for (int i = 0; i < IngredientsList.Items.Count; i++) //loop
items
                {
                    if
(IngredientsList.ItemContainerGenerator.ContainerFromIndex(i) is
FrameworkElement fe) //get container
                    {
                        var small = Utilities.FindChild<Image>(fe,
"SmallSlotBg"); //find image named SmallSlotBg
                        if (small != null) small.Source = slotBack; //set its
background
                    }
                }
            };

            CraftOneBtn.Click += (s, e) => CraftRequested?.Invoke(this, new
CraftEventArgs(1)); //craft 1 button
            CraftAllBtn.Click += (s, e) => CraftRequested?.Invoke(this, new
CraftEventArgs(int.MaxValue)); //craft all button
        }

        public event EventHandler<CraftEventArgs> CraftRequested; //event
fired when craft button clicked -> sends to CraftingMenuDisaply


        public static readonly DependencyProperty RecipeProperty =
            DependencyProperty.Register(nameof(Recipe), typeof(RecipeModel),
typeof(RecipeSlotDisplay),
                new PropertyMetadata(null, OnRecipeChanged)); //means recipe
property is bindable and when it changes, update the UI by calling
OnRecipeChanged -> BindRecipe (subroutine)

       public RecipeModel Recipe
       {
           get { return (RecipeModel)GetValue(RecipeProperty); } //get recipe
           set
           {
               SetValue(RecipeProperty, value); //set recipe
           }
       }

        private bool _isCraftable; //if recipe is craftable
        public bool IsCraftable
        {
            get { return _isCraftable; }
            set
            {
                _isCraftable = value;
                UnavailableOverlay.Visibility = value ? Visibility.Collapsed :
Visibility.Visible; //hide overlay if craftable
                OnPropertyChanged(nameof(IsCraftable));
            }
        }

        private int _maxCraftCount; //how many times can craft
        public int MaxCraftCount
        {
            get { return _maxCraftCount; }
            set
            {
                if (_maxCraftCount != value)
                {
                    _maxCraftCount = value;
                    CraftAllBtn.Content = _maxCraftCount > 1 ? $"Craft All
({_maxCraftCount})" : "Craft All"; //update button text
                    OnPropertyChanged(nameof(MaxCraftCount));
                }
            }
        }
        private string _missingReason; //reason cant craft
        public string MissingReason
        {
            get { return _missingReason; }
            set
            {
                _missingReason = value;


                OnPropertyChanged(nameof(MissingReason));
            }
        }

        private static void OnRecipeChanged(DependencyObject DepObj,
DependencyPropertyChangedEventArgs e) //called when recipe changes
        {
            ((RecipeSlotDisplay)DepObj).BindRecipe();
        }

        private void BindRecipe() //bind recipe data to ui
        {
            if (Recipe == null) //if no recipe
            {
                OutputName.Text = "";
                OutputImage.Source = null;
                OutputQty.Text = "";
                IngredientsList.ItemsSource = null;
                return;
            }

            OutputName.Text = Recipe.OutputItem?.Name ?? "?"; //set output
name
            OutputImage.Source = Recipe.OutputItem?.Image; //set output image
            OutputQty.Text = $"x{Recipe.OutputQuantity}"; //set output
quantity
            IngredientsList.ItemsSource = Recipe.Ingredients; //bind
ingredient list

            double baseSize = 44; //default icon size
            string type = Recipe.OutputItem?.ItemType; //get item type
            if (type == "Block") baseSize = baseSize * 0.55; //smaller if
block
            else if (type == "Material") baseSize = baseSize * 0.6;
            else if (type == "Decoration") baseSize = baseSize * 0.75;
//medium if decoration (as smaller)
            OutputImage.Width = OutputImage.Height = baseSize; //apply size
        }

        public void UpdateCraftability(Inventory inv, ItemCrafting crafting)
//updateing crating slot w/o remaking
        {
            if (Recipe == null) return; //no recipe return

            int max = crafting.MaxCraftableCount(inv, Recipe); //get max
craftable count
            IsCraftable = max > 0;
            MaxCraftCount = max; //set it
        }

        private void OnPropertyChanged(string name) =>
PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); //notify ui


    }
    public class CraftEventArgs : EventArgs
    {
        public int RequestedTimes { get; } //how many times to craft
        public CraftEventArgs(int requestedTimes)
        {
            RequestedTimes = requestedTimes;
        }
    }
}
