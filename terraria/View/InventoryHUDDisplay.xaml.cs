using System;
using System.Collections.Generic;
using System.Windows.Controls;


namespace WPFTerraria.View
{
    public partial class InventoryHUDDisplay : UserControl
    {
        private Inventory inventory;
        public Inventory Inventory //gets myinventory from main
        {
            get { return inventory; }
            set
            {
                inventory = value;
                InitiliseSlots();
                SubscribeToInventoryChanges(); //called again every time
playersitems changes
            }
        }
        public InventoryHUDDisplay()
        {
            InitializeComponent();
            DataContext = this;
        }
        public void InitiliseSlots()
        {
            if (inventory == null)
            { Console.WriteLine("Error, no inventory"); return; }

            InventoryHUD.Children.Clear();
            for (int i = 0; i < inventory.HudSlotAmount; i++)
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

                Canvas.SetLeft(thisInventorySlotDisplay, i *
(inventory.SlotImageSize + 3)); //3 pixel gap between each
                Canvas.SetTop(thisInventorySlotDisplay, 0);
                InventoryHUD.Children.Add(thisInventorySlotDisplay);
            }
        }


        //handle collection changes
        private void SubscribeToInventoryChanges()
        {
            inventory.PlayersItems.CollectionChanged += (sender, e) => // if
something changes in playersinventory, call event
            {
                for (int i = 0; i < inventory.HudSlotAmount; i++) //iterates
over the first HudSlotAmount slots
                {
                    var slot = InventoryHUD.Children[i] as
InventorySlotDisplay; //gets the current slot
                    slot.ThisInventoryItem = (i <
inventory.PlayersItems.Count) ? inventory.PlayersItems[i] : null; //if theres
something in the slot, refresh it, else its empty
                }
            };
        }
    }
}
