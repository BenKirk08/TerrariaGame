using System;

namespace WPFTerraria
{
    public class ItemChangedEventArgs : EventArgs
    //custom event args for item change
    {
        public string ItemName { get; }
        public ItemChangedEventArgs(string itemName)
        {
            ItemName = itemName; //sets item name of changed item
        }
    }
}
