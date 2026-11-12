using System;
using System.ComponentModel;

namespace WPFTerraria
{
    public class Inventory : INotifyPropertyChanged
    {
        //for GetPictureImagePath
        protected string DataBaseFileName = "ItemsDB.db";
        protected string PicturesTable = "Items";
        protected string PNGsHandle = "Item";

        public int HudSlotAmount { get; } = 8;
        public int TotalSlots { get; } = 64;
        public int SlotImageSize { get; private set; }

        //good as send notifactions when change
        public ObservableInventoryCollection<InventoryItem> PlayersItems {
get; } = new ObservableInventoryCollection<InventoryItem>();

        public Inventory()
        {
            //PlayersItems = new
ObservableInventoryCollection<InventoryItem>();

            for (int i = 0; i < TotalSlots; i++)
            {
                var slot = new InventoryItem(this);
                slot.PropertyChanged += OnSlotPropertyChanged; //listen to
slot changes
                PlayersItems.Add(slot);
                //initilise all to empty slots
            }
            SlotImageSize = (int)Math.Floor((double)MainWindow.CameraWidth);
//each slotimage is a tilesize percentage of the width of the display
        }
        private void OnSlotPropertyChanged(object sender,
PropertyChangedEventArgs thisEvent)
        {
            if (thisEvent.PropertyName == nameof(InventoryItem.Quantity))
            {
                var slot = (InventoryItem)sender;
                if (slot.Item != null)
                {
                    ItemChanged?.Invoke(this, new
ItemChangedEventArgs(slot.Item.Name));
                }
            }

        }
        public InventoryItem this[int index] //INDEXER -> very important,
allows to get the InventoryItem at a specific inventory slot ie,
myInventory[5] -> returns InventoryItem from slot 6 (starts from 0)
        {
            get { return PlayersItems[index]; } //get item at position
            set
            {
                PlayersItems[index] = value;
            }
        }

        public int AddItem(Item item, int quantity) //adds item or doesnt,
returns integer of remaining things to add
        {
            if (item == null) return quantity; //handle null items immediately

            //first try to stack with existing items
            foreach (var slot in PlayersItems.Where(x => x.Item != null))
//checks all slots
            {
                if (slot.Item.GetType() == item.GetType() && //if they are
same item (class)
                    slot.Quantity < item.MaxStackSize) //and theres not more
the max quantity in the slot
                {
                    int availableSpace = item.MaxStackSize - slot.Quantity;
//check the amount of space left in the stack
                    int addAmount = Math.Min(availableSpace, quantity); //only
add the max avaliable space

                    slot.Quantity += addAmount;
                    quantity -= addAmount; //if more than addamount, will be
positive and add to another spot


                    if (quantity <= 0) { return quantity; } //return
added/true if no more items to add
                }
            }

            //fill an empty slot
            foreach (var slot in PlayersItems.Where(x => x.Item ==
null))//checks all slots that are null ->o nly checking null saves power
            {
                slot.Item = item; //put item in empty slot
                slot.Quantity = Math.Min(quantity, item.MaxStackSize); //only
add up to maxstacksize
                quantity -= slot.Quantity; //minus from quantity to add

                if (quantity <= 0) { return quantity; } //return added/true if
no more items to add


           }
           return quantity; //inventorys full
       }


        private int _selectedInventoryIndex = -1; //-1 means nothing is
selected (initial) int in this class (inventory), that is selected currently
        public int SelectedInventoryIndex //used if use numbers on keyboard to
change selection & triggers in inventoryItem
        {
            get { return _selectedInventoryIndex; }
            set
            {
                if (_selectedInventoryIndex == value) return; //prevent
unnecessary updates

               _selectedInventoryIndex = value;
               UpdateSelection(); //will change the selection
           }
       }

        private void UpdateSelection()
        {
            for (int i = 0; i < PlayersItems.Count; i++)
            {
                //only update if state actually changes
                bool newValue = i == SelectedInventoryIndex;
                if (PlayersItems[i].IsSelected != newValue)
                {
                    PlayersItems[i].IsSelected = newValue; //id the index has
_selectedinventoryidex == true, then the player item at index i, isslected is
true
                }
            }
        }

        public event EventHandler<ItemChangedEventArgs> ItemChanged; //event
fired when an inventory item changes
        public void NotifyItemChanged(Item item) //trigger the event
        {
            if (item == null)
            {
                return; //stop if no item
            }
            ItemChanged?.Invoke(this, new ItemChangedEventArgs(item.Name));
//invoke event with item name in args
        }


       public event PropertyChangedEventHandler PropertyChanged;
       protected virtual void OnPropertyChanged(string propertyName)
       {


            PropertyChanged?.Invoke(this, new
PropertyChangedEventArgs(propertyName));
        }
    }
}
