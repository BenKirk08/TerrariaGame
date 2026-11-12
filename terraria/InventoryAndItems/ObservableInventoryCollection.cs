using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WPFTerraria
{
    public class ObservableInventoryCollection<T> : ObservableCollection<T>
where T : INotifyPropertyChanged //creates new obsevalecollection list with
differnet properties
    {
        public ObservableInventoryCollection() : base() { }

        public ObservableInventoryCollection(IEnumerable<T> collection) :
base(collection)
            //consructor //checks if any item changes at all, collection is
all of the items
        {
            foreach (var item in collection)
            {
                item.PropertyChanged += Item_PropertyChanged; //occurs if the
item changes value
            }
        }

        //has to be protected as in base class is, override as overiding the
base class
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            if (item != null)
            {
                item.PropertyChanged += Item_PropertyChanged; //occurs if new
item added
            }
        }

        protected override void RemoveItem(int index)
        {
            if (index >= 0 && index < Count) //count is number of items in
collection
            {
                var item = this[index]; //gets current item
                if (item != null) //if not null
                {
                    item.PropertyChanged -= Item_PropertyChanged; //occurs if
item removes, unsusbribe to prevent unneccessary event handling.
                }
            }


            base.RemoveItem(index);
       }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                if (item != null)
                {
                    item.PropertyChanged -= Item_PropertyChanged;   //occurs if
collection cleared
                }
            }
            base.ClearItems(); //uses base properties
        }

        private void Item_PropertyChanged(object sender,
PropertyChangedEventArgs e) //the item has changed event
        {
            //notify that the item has changed
            int index = IndexOf((T)sender); //gets index of the item sent
            if (index >= 0) //if not below 0 (not in collection for error
checking)
            {
                OnCollectionChanged(new
NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
sender, sender, index)); //replaces current item with itself so it refreshes,
causes ui update
            }
        }
    }
}
