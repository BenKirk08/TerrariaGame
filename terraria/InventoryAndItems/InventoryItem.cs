using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows;

namespace WPFTerraria
{
    public class InventoryItem : INotifyPropertyChanged
    {
        private Item _item;
        private int _quantity;
        private readonly Inventory _parent; //set at construction

       public InventoryItem(Inventory parent)
       {
           _parent = parent;
           _item = null;
           _quantity = 0; //initialize quantity to 0
       }

        public Item Item
        {
            get { return _item; }
            set
            {
                _item = value;
                if (_item != null)
                {
                    _parent?.NotifyItemChanged(_item);
                }
                //Note: before adding new collection WPF only listened for
changes to collection itself and not properties of items inside
                //when updated 1uantity or item, even though PropertyChanged
happened UI ditn change as it wasnt subscribed to those property changes
directly.

               OnPropertyChanged(nameof(Item));
               OnPropertyChanged(nameof(Name));
               OnPropertyChanged(nameof(ItemImage));
               OnPropertyChanged(nameof(Item.ItemType));


                //if the item changes, ensure quantity is within the new
item's max stack size
                if (_item != null && _quantity > _item.MaxStackSize)
//shouldnt get called
                {
                    Quantity = _item.MaxStackSize;
                    Console.WriteLine("new item adding error");
                }


              }
          }

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                int toAddAmount = 0;
                if (value > Item.MaxStackSize) { toAddAmount = value -
Item.MaxStackSize; }

                int maybeClampedValue = (Item != null && value >
Item.MaxStackSize) ? Item.MaxStackSize : value; //make sure not more than max
quantity size

                if (_quantity != maybeClampedValue)
                {
                    var main = Application.Current.MainWindow as MainWindow;
                    int delta = maybeClampedValue - _quantity; //how much item
quantiyt changed
                    _quantity = maybeClampedValue;

                      //notify ui of change
                      OnPropertyChanged(nameof(Quantity)); //for slot dispaly
                      _parent?.NotifyItemChanged(Item); //for crafting

                      if (Item != null && delta != 0) //check has item and
changed
                      {
                        main?.ShowItemChangeNotification(Item, delta); //call
mainwindow method to show notification
                    }

                      //clear item if quantity drops to 0
                      if (_quantity == 0)
                      {
                          Item = null; //will trigger additional property
updates
                      }
                      if (toAddAmount > 0)
                      {
                          //uses mainwindow to access myInventory
                          main.myInventory.AddItem(Item, toAddAmount);
                      }
                  }
              }
          }

        //helper properties for binding -> allow for nameof
        public string Name => Item?.Name ?? "Empty"; //gives item.name, if
null/not listed then empty


        public BitmapSource ItemImage => Item?.Image;   //gives item.image, if
null/not listed then null

        //if this iventory item is the one selected
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected)); //triggers
                    if (value == true)
                    {
                        var main = Application.Current.MainWindow as
MainWindow;
                        main.UpdateInteractionOutline(); //if changes selected
items, will automatically check if something that requires valid tiles to use
                    }
                }
            }
        }

        //if changed, cause event to happen
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            //Console.WriteLine($"OnPropertyChanged fired: {propertyName}");
            PropertyChanged?.Invoke(this, new
PropertyChangedEventArgs(propertyName));
        }
    }
}
