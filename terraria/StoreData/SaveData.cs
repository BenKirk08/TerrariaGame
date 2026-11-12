using System;

namespace WPFTerraria.StoreData
{
    [Serializable]
    public class SaveData
    {
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }
        public double PlayerX { get; set; }
        public double PlayerY { get; set; }
        public string PlayerGender { get; set; }
        public int PlayerHealth { get; set; }
        public string Time { get; set; }
        public List<InventoryItemData> Inventory { get; set; }
        public List<TileData> Tiles { get; set; }
        public List<TileHeightData> tileHeights { get; set; }
    }

    [Serializable]
    public class InventoryItemData
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public int InventoryIndex { get; set; }
    }

    [Serializable]
    public class TileData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public byte TileID { get; set; }
    }
    [Serializable]
    public class TileHeightData
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
