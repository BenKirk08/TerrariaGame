using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace WPFTerraria.View
{
    public partial class FullInventoryDisplay : UserControl
    {
        public FullInventoryDisplay()
        {
            InitializeComponent();
            DataContext = this;
            this.MouseLeftButtonDown += OnBackgroundClicked;
        }
        //Hide panel when clicking elsewhere (background)
        private void OnBackgroundClicked(object sender, MouseButtonEventArgs
e)
        {
            // only act if panel is active
            if (isPanelActive)
            {
                var clickedElement = e.OriginalSource as DependencyObject;

                // check if click happened inside ItemSplitPanel
                bool insidePanel = false;
                while (clickedElement != null)
                {
                    if (clickedElement == ItemSplitPanel)
                    {
                        insidePanel = true;
                        break;
                    }
                    clickedElement =
VisualTreeHelper.GetParent(clickedElement);
                }

                // hide only if the click wasn't inside the panel
                if (!insidePanel)
                    HideSplitPanel();
           }
       }

        private InventorySlotDisplay currentSelectedSlot;
        private bool isPanelActive = false;

        public double FullInventoryDisplayWidth { get; private set; }
        public double FullInventoryDisplayHeight { get; private set; }


       private Inventory inventory;
       public Inventory Inventory
       {
           get { return inventory; }
           set
           {
               inventory = value;
               InitiliseSlots();


                SubscribeToInventoryChanges(); //called again every time
playersitems changes
                FullInventory.Children.Add(ItemSplitPanel);
            }
        }

       //ALL SIMILAR TOM INVENTORYHUDDISPLAY (mostly copy and paste
       public void InitiliseSlots()
       {
           if (inventory == null)
           {
               Console.WriteLine("Error, no inventory");
               return;
           }

            FullInventoryDisplayWidth = inventory.HudSlotAmount *
(inventory.SlotImageSize + 3);
            FullInventoryDisplayHeight =
Math.Ceiling((double)inventory.TotalSlots / (double)inventory.HudSlotAmount) *
(inventory.SlotImageSize + 3) + 10;
            FullInventory.Children.Clear();

            FullInventory.Width = FullInventoryDisplayWidth;
            FullInventory.Height = FullInventoryDisplayHeight;
            for (int i = 0; i < inventory.TotalSlots; i++)
            {
                InventorySlotDisplay thisInventorySlotDisplay = new
InventorySlotDisplay();
                if (i < inventory.PlayersItems.Count)
                {
                    thisInventorySlotDisplay.ThisInventoryItem =
inventory.PlayersItems[i]; //set the images, if in player items
                    thisInventorySlotDisplay.SlotPlace = i + 1; //returns
place in slot -> numbers to press to select item
                }

                Canvas.SetLeft(thisInventorySlotDisplay, (i %
inventory.HudSlotAmount) * (inventory.SlotImageSize + 3)); //3 pixel gap
between each
                if (i >= inventory.HudSlotAmount)
                {



                    Canvas.SetTop(thisInventorySlotDisplay, (i /
inventory.HudSlotAmount) * inventory.SlotImageSize + ((i / 8) * 3) + 10); //+
for gap between items in hud
                }
                else
                {
                    Canvas.SetTop(thisInventorySlotDisplay, 0);
                }

               FullInventory.Children.Add(thisInventorySlotDisplay);

                // 🔄 now uses double-click to trigger slot selection
                thisInventorySlotDisplay.MouseLeftButtonDown +=
OnSlotDoubleClicked;
            }
        }

        //handle collection changes
        private void SubscribeToInventoryChanges()
        {
            inventory.PlayersItems.CollectionChanged += (sender, e) => // if
something changes in playersinventory, call event and refresh the slots
            {
                for (int i = 0; i < inventory.TotalSlots; i++) //iterates over
the all slots
                {
                    var slot = FullInventory.Children[i] as
InventorySlotDisplay; //gets the current slot -> inventory item thats
currently being displayed
                    slot.ThisInventoryItem = (i <
inventory.PlayersItems.Count) ? inventory.PlayersItems[i] : null; //if theres
something in the slot, refresh it, else its empty
                }
            };
        }

        // Handles double-clicks only
        private void OnSlotDoubleClicked(object sender, MouseButtonEventArgs
e)
       {
           if (e.ClickCount == 2)
           {
               OnSlotSelected(sender, e);
               e.Handled = true;
           }
       }

       private void OnSlotSelected(object sender, MouseButtonEventArgs e)
       {
           // inventory must be open
           var main = (MainWindow)Application.Current.MainWindow;
           if (Inventory == null || main == null || !main.isInventoryOpen)
               return;

             var slot = sender as InventorySlotDisplay;

             // if empty slot or null -> hide panel and bail
             if (slot?.ThisInventoryItem == null || slot.ThisInventoryItem.Item
== null)
             {
                 HideSplitPanel();
                 return;
             }

             currentSelectedSlot = slot;

             var item = slot.ThisInventoryItem;

            // show panel for any item (even qty=1), but hide split controls
when qty <= 1
            bool canSplit = item.Quantity > 1;
            ToggleSplitControls(canSplit);

            if (canSplit)
            {
                // configure slider
                SplitSlider.Minimum = 1;
                SplitSlider.Maximum = item.Quantity - 1;
                SplitSlider.Value = Math.Clamp(item.Quantity / 2.0,
SplitSlider.Minimum, SplitSlider.Maximum);
                UpdateSplitTexts();
            }
            else
            {
                // hide numbers if no split
                LeftSplitText.Text = string.Empty;
                RightSplitText.Text = string.Empty;
            }

             // ensure have size before computing placement
             ItemSplitPanel.Visibility = Visibility.Visible; // must be visible
to measure
            ItemSplitPanel.Measure(new Size(double.PositiveInfinity,
double.PositiveInfinity));
            ItemSplitPanel.Arrange(new Rect(ItemSplitPanel.DesiredSize));
            ItemSplitPanel.UpdateLayout();

             // position panel relative to the FullInventory canvas

             //ABOVE + RIGHT of the slot
             var slotPos = slot.TranslatePoint(new Point(0, 0), FullInventory);

             double panelW = ItemSplitPanel.Width;
             double panelH = ItemSplitPanel.Height;

             double slotRight = slotPos.X + slot.ActualWidth;

            double slotTop = slotPos.Y;

            // initial target = above-right
            double left = slotRight + 6;
            double top = slotTop - panelH - 6;

            // if above overflows top, place below instead
            if (top < 0)
            {
                top = slotTop + slot.ActualHeight + 6;
            }

            // clamp inside the FullInventory canvas bounds
            double parentW = FullInventory.ActualWidth;
            double parentH = FullInventory.ActualHeight;

            left = Math.Max(4, Math.Min(left, Math.Max(4, parentW - panelW -
4)));
            top = Math.Max(4, Math.Min(top, Math.Max(4, parentH - panelH -
4)));

            // ItemSplitPanel be a child of a Canvas for these to happen

            Canvas.SetLeft(ItemSplitPanel, left);
            Canvas.SetTop(ItemSplitPanel, top);

            ItemSplitPanel.Visibility = Visibility.Visible;
            isPanelActive = true;
        }

        private void SplitSlider_ValueChanged(object sender,
RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isPanelActive) return;
            UpdateSplitTexts();
        }

        private void UpdateSplitTexts()
        {
            if (currentSelectedSlot?.ThisInventoryItem == null) return;

            int total = currentSelectedSlot.ThisInventoryItem.Quantity;
            if (total <= 1)
            {
                LeftSplitText.Text = string.Empty;
                RightSplitText.Text = string.Empty;
                return;
            }

            // slider now represents how many will remain in th current slot
            int keep = (int)Math.Round(SplitSlider.Value);
            keep = Math.Clamp(keep, 1, Math.Max(1, total - 1));


           LeftSplitText.Text = keep.ToString(); // what stays
           RightSplitText.Text = (total - keep).ToString(); // what moves
       }

        private void ConfirmSplitButton_Click(object sender, RoutedEventArgs
e)
       {
            if (currentSelectedSlot?.ThisInventoryItem == null) return;

            var main = (MainWindow)Application.Current.MainWindow;
            if (main == null) return;

            var item = currentSelectedSlot.ThisInventoryItem;
            int total = item.Quantity;

            // if cannot split, do nothing (UI should not allow this button
when qty <=1)
            if (total <= 1) return;

           // slider value = amount to keep in current slot
           int keepAmount = (int)Math.Round(SplitSlider.Value);
           keepAmount = Math.Clamp(keepAmount, 1, total - 1);

            int moveAmount = total - keepAmount;

            if (moveAmount <= 0) return;

           //reduce current slot to kept amount
           item.Quantity = keepAmount;

           //new stack gets the moved amount
           var type = item.Item.GetType();
           Item newStackItem = (Item)Activator.CreateInstance(type);

            int currentIndex = main.myInventory.PlayersItems.IndexOf(item);
            bool placed =
TryPlaceInNextEmptySlotSkippingIndex(main.myInventory, currentIndex,
newStackItem, moveAmount);
            if (!placed)
            {
                // no empty slot -> revert and tell user
                item.Quantity = total; // revert
                MessageBox.Show("Inventory is full. Could not split the
stack.", "Inventory", MessageBoxButton.OK, MessageBoxImage.Information);
                return;

           }

           HideSplitPanel();
       }

       private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
       {

             if (currentSelectedSlot?.ThisInventoryItem == null) return;

             // remove entire stack
             currentSelectedSlot.ThisInventoryItem.Quantity = 0;

             HideSplitPanel();
         }


         ///Hides the split panel and resets state.

         private void HideSplitPanel()
         {
             ItemSplitPanel.Visibility = Visibility.Collapsed;
             isPanelActive = false;
             currentSelectedSlot = null;
         }


         /// Shows/hides the slider + numbers + split button in one go.
         private void ToggleSplitControls(bool show)
         {

             var visibility = show ? Visibility.Visible : Visibility.Collapsed;
             SplitSlider.Visibility = visibility;
             ConfirmSplitButton.Visibility = visibility;
             LeftSplitText.Visibility = visibility;
             RightSplitText.Visibility = visibility;
         }

         ///Places the split into the first empty slot, skipping the provided
index.

        /// Returns true if placed; false if no space.
        private bool TryPlaceInNextEmptySlotSkippingIndex(Inventory inv, int
skipIndex, Item item, int qty)

             // Prefer truly empty slot to avoid remerging into the same stack
             for (int i = 0; i < inv.PlayersItems.Count; i++)
             {
                 if (i == skipIndex) continue;
                 var slot = inv.PlayersItems[i];
                 if (slot.Item == null)
                 {
                     slot.Item = item;
                     slot.Quantity = qty;
                     return true;
                 }
             }

             // no empty slot found
             return false;
         }

    }


}
