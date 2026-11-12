using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFTerraria
{
    public static class TileDictionary
    {
        public struct TileInfo
        {
                                             Record
            public string TileName;
            public bool IsSolid;
            public int TileLevel;
            public Dictionary<string, object> ExtraData;
            public Action<byte> DoMethods;
            public bool IsWalkableThrough;
            public bool PickaxeBroken;
            public bool IsWorkStation;
            public Color Colour;

            public TileInfo(string tileName, bool isSolid, int tileLevel,
Dictionary<string, object> extraData, Action<byte> doMethods, bool
isWalkableThrough, bool pickaxeBroken, bool isWorkStation, Color colour)
            {
                TileName = tileName;
                IsSolid = isSolid;
                TileLevel = tileLevel;
                ExtraData = extraData ?? new Dictionary<string, object>();
                DoMethods = doMethods;
                IsWalkableThrough = isWalkableThrough;
                PickaxeBroken = pickaxeBroken;
                IsWorkStation = isWorkStation;
                Colour = colour;
            }
        }

        public static Dictionary<byte, TileInfo> TileData = new
Dictionary<byte, TileInfo>()
        {
            //empty
            { 0, new TileInfo(
                "Empty",
                false,
                -1,
                null,
                null,
                false,
                false, //broken by none
                false,
                Colors.Transparent


)},
//dirt block
{ 1, new TileInfo(
    "Dirt Block",
    true,
    1,
    null,
    null,
    false,
    true,
    false,
    Colors.Brown
)},
//stone block
{ 2, new TileInfo(
    "Stone Block",
    true,
    2,
    null,
    null,
    false,
    true,
    false,
    Colors.LightGray
)},
//copper block
{ 7, new TileInfo(
    "Copper Ore",
    true,
    2,
    null,
    null,
    false,
    true,
    false,
    Colors.Orange
)},
//lead ore
{ 167, new TileInfo(
    "Lead Ore",
    true,
    2,
    null,
    null,
    false,
    true,
    false,
    Colors.DarkBlue
)},
//iron ore
{ 6, new TileInfo(
    "Iron Ore",

   true,
   3,
   null,
   null,
   false,
   true,
   false,
   Colors.Silver
)},
//platimun ore
{ 169, new TileInfo(
    "Platinum Ore",
    true,
    4,
    null,
    null,
    false,
    true,
    false,
    Colors.LightBlue
)},
//gold ore
{ 8, new TileInfo(
    "Gold Ore",
    true,
    3,
    null,
    null,
    false,
    true,
    false,
    Colors.Gold
)},
//obsidian-> ground floor
{ 56, new TileInfo(
    "Obsidian",
    true,
    9999,
    null,
    null,
    false,
    true,
    false,
    Colors.Black
)},
//wood
{ 30, new TileInfo(
    "Wood",
    true,
    1,
    null,
    null,

   false,
   true,
   false,
   Colors.SaddleBrown
)},
//living wood
{ 191, new TileInfo(
    "Living Wood",
    true,
    1,
    null,
    null,
    true,
    false,
    false,
    Colors.SaddleBrown
)},
//Leaf Block
{ 192, new TileInfo(
    "Leaf Block",
    true,
    1,
    null,
    null,
    true,
    false,
    false,
    Colors.GreenYellow
)},
//Workstations
{ 16, new TileInfo(
    "Iron Anvil",
    true,
    1,
    new Dictionary<string, object>()
    {
        { "WorkStation ID", 22 }
    },
    null,
    true,
    true,
    true,
    Colors.Gray
)},
{ 18, new TileInfo(
    "Work Bench",
    true,
    1,
    new Dictionary<string, object>()
    {
        { "WorkStation ID", 37 }
    },

               null,
               true,
               true,
               true,
               Colors.SaddleBrown
            )},
            { 17, new TileInfo(
                "Furnace",
                true,
                1,
                new Dictionary<string, object>()
                {
                    { "WorkStation ID", 14 }
                },
                null,
                true,
                true,
                true,
                Colors.LightGray
            )}

       };


        public static Dictionary<string, Item> nameToItem = new
Dictionary<string, Item>
        {
            { "Dirt Block", new DirtBlockItem() },
            { "Stone Block", new StoneBlockItem() },
            { "Copper Ore", new CopperOreItem() },
            { "Lead Ore", new LeadOreItem() },
            { "Iron Ore", new IronOreItem() },
            { "Gold Ore", new GoldOreItem() },
            { "Platinum Ore", new PlatinumOreItem() },
            { "Obsidian", new ObsidianBlockItem() },
            { "Wood", new WoodBlockItem() },
            { "Living Wood", new WoodBlockItem() },
            { "Iron Anvil", new IronAnvilItem() },
            { "Work Bench", new WorkBenchItem() },
            { "Furnace", new FurnaceItem() }

        };
        public static Dictionary<int, string> WorkStationToID = new
Dictionary<int, string>
        {
            { 37, "Work Bench" },
            { 22, "Iron Anvil"},
            { 14, "Furnace"}

       };




        //for GetPictureImagePath
        private const string tileDataBaseFileName = "TilesDB.db";
        private const string tilePicturesTable = "Tiles";
        private const string tilePNGsHandle = "Tiles";

        private const int ContentTileSize = 16;
        private const int ContentSpacingSize = 2;

        //dictionary to cache images per tile type
        private static Dictionary<byte, Dictionary<string, BitmapSource>>
TileTextures = new Dictionary<byte, Dictionary<string, BitmapSource>>();


        //lazy-load images on first request -> only added when first called.
Slightly increase processing power, major decrease in memory
        public static BitmapSource GetTileImage(byte tileID, string
ConnectionType)
        {
            if (TileDictionary.TileData[tileID].IsWorkStation == true)
            {
                //just return the image of the workstation -> no connections
needed
                return
Utilities.ResizeImage(Utilities.LoadImage(Utilities.GetPictureImagePath(tileDa
taBaseFileName, tilePicturesTable, tilePNGsHandle,
TileDictionary.TileData[tileID].TileName)), Utilities.TileSize,
Utilities.TileSize);
            }

           if (!TileTextures.TryGetValue(tileID, out var imageDict))
           {

               //if tile type has no images cached -> create new dictionary
               imageDict = new Dictionary<string, BitmapSource>();
               TileTextures[tileID] = imageDict;
           }

            //if not already cached
            if (!imageDict.ContainsKey(ConnectionType))
            {
                //Console.WriteLine($"Loading image for Tile ID {tileID}
{ConnectionType}");
                LoadSingleImage(tileID, ConnectionType); //returns image of
that connection type from dictionary
            }
            return imageDict[ConnectionType];

       }




        private static void LoadSingleImage(byte tileID, string
connectionType) //lazy-load image into cache -> this will only be the first
image displayed until changed
        {
            var layout = GetTileLayout(tileID); //get tile layout array
            var coords = Utilities.GetTileImageCoords(layout, connectionType);
            if (coords.HasValue)
            {
                //extract needed image
                BitmapSource image = Utilities.ImagePNGExtractor(

Utilities.LoadImage(Utilities.GetPictureImagePath(tileDataBaseFileName,
tilePicturesTable, tilePNGsHandle, TileDictionary.TileData[tileID].TileName)),
                    ContentTileSize,
                    ContentTileSize,
                    ContentSpacingSize,
                    coords.Value.row,
                    coords.Value.col
                );

               //resize it to match ingame tile size
               BitmapSource resizedImage = Utilities.ResizeImage(
                   image,
                   Utilities.TileSize,
                   Utilities.TileSize
               );

               //store it in dictionary for future use
               TileTextures[tileID][connectionType] = resizedImage;
            }
            else
            {
                Console.WriteLine($"error loading single image for TileID
{tileID} and connection type {connectionType}");
            }
        }


        private static Dictionary<byte, string[,]> TileLayouts = new
Dictionary<byte, string[,]>();
        public static void LoadTileLayout(byte tileID)
        {
            if (!TileLayouts.ContainsKey(tileID))
            {
                TileLayouts[tileID] = new BlockLayout().GetBlockArray();
//fetch layout once
            }
        }
        public static string[,] GetTileLayout(byte tileID)
        {
            if (!TileLayouts.TryGetValue(tileID, out var layout))
            {
                LoadTileLayout(tileID); //load layout if missing

                layout = TileLayouts[tileID];
            }
            return layout;
        }
    }
}
