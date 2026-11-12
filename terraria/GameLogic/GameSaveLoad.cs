using System;
using System.IO;
using System.Text.Json;
using WPFTerraria.StoreData;
using System.Linq;
using System.Windows.Media.Imaging;
using systemShapes = System.Windows.Shapes;

namespace WPFTerraria
{
    public partial class MainWindow
    {
        //saving game
        public void SaveGame()
        {
            SaveData saveData = new SaveData
            {
                MapHeight = mapY_Height,
                MapWidth = mapX_Width,
                PlayerX = myPlayer.playerX,
                PlayerY = myPlayer.playerY,
                PlayerGender = playerGender,
                PlayerHealth = myPlayer._playerHealth,
                Time = timeTextBlock.Text,
                Inventory = new List<InventoryItemData>(),
                Tiles = new List<TileData>(),
                tileHeights = new List<TileHeightData>()
            };


            //save inventory
            foreach (var (item, index) in
myInventory.PlayersItems.Select((item, index) => (item, index))) //pairs eacg
item with its index in playeritems
            {
                if (item.Item != null)
                {
                    saveData.Inventory.Add(new InventoryItemData
                    {
                        ItemName = item.Item.GetType().Name, //get items class
name
                        Quantity = item.Quantity,
                        InventoryIndex = index
                    });
                }
            }

           //save tiles
           for (int x = 0; x < mapX_Width; x++)
           {
               for (int y = 0; y < mapY_Height; y++)

               {
                   if (tileMapData[x, y] != 0)
                   {
                       saveData.Tiles.Add(new TileData
                       {
                           X = x,
                           Y = y,
                           TileID = tileMapData[x, y]
                       });
                   }
               }
           }
           //save orgional tile heights, for cave backgrounvector
           for (int x = 0; x < mapX_Width; x++)
           {
               saveData.tileHeights.Add(new TileHeightData
               {
                   X = x,
                   Y = smoothCeiling[x]
               });
           }

            //get the path
            string savePath = Path.Combine(Utilities.GetSolutionDirectory(),
"StoreData", "Saved Games");
            string fullSavePath = Path.Combine(savePath, "savegame.json");

            string json = JsonSerializer.Serialize(saveData, new
JsonSerializerOptions { WriteIndented = true }); //converts savedata to json
text
            File.WriteAllText(fullSavePath, json); //saves the json to file
            Console.WriteLine($"Game saved at: {fullSavePath}");
        }


        public void LoadGame()
        {
            string savePath = Path.Combine(Utilities.GetSolutionDirectory(),
"StoreData", "Saved Games");
            string fullSavePath = Path.Combine(savePath, "savegame.json");

           if (!File.Exists(fullSavePath)) //if the file exists
           {
               Console.WriteLine("No save file found");
               return;
           }

            string json = File.ReadAllText(fullSavePath); //get all the text
from the json file
            SaveData saveData = JsonSerializer.Deserialize<SaveData>(json);
//Load back into game using savedata layout

            //make maps dimesions

           mapX_Width = saveData.MapWidth;
           mapY_Height = saveData.MapHeight;
           newMapHeight = mapY_Height;
           newMapWidth = mapX_Width;
           tileMapData = new byte[mapX_Width, mapY_Height];
           connectionMap = new string[mapX_Width, mapY_Height];
           loadedTileImages = new BitmapSource[mapX_Width, mapY_Height];
           hitboxMap = new systemShapes.Rectangle[mapX_Width, mapY_Height];

           //for parrallax
           minHeight = mapY_Height * 0.7;
           //load the players position
           myPlayer.playerX = saveData.PlayerX;
           myPlayer.playerY = saveData.PlayerY;

           //load player Gender and health
           playerGender = saveData.PlayerGender;

            //change players health
            myPlayer.ChangePlayerHealth(-(myPlayer._playerMaxHealth -
saveData.PlayerHealth));

           //time
           dayClock = DateTime.ParseExact(saveData.Time, "HH:mm", null);

            //load the innventory
            myInventory.PlayersItems.Clear(); //make sure empty to begin with
            for (int i = 0; i < myInventory.TotalSlots; i++)
            {
                myInventory.PlayersItems.Add(new InventoryItem(myInventory));
//initilise all to empty slots
            }
            foreach (var itemData in saveData.Inventory)
            {
                Type itemType = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == itemData.ItemName);
                if (itemType != null)
                {
                    Item itemInstance =
(Item)Activator.CreateInstance(itemType); //dynamically creates instance of
class -> same to new
                    //check within valid index
                    if (itemData.InventoryIndex >= 0 &&
itemData.InventoryIndex < myInventory.PlayersItems.Count)
                    {
                        myInventory[itemData.InventoryIndex] = new
InventoryItem(myInventory)
                        {
                            Item = itemInstance,
                            Quantity = itemData.Quantity
                        };

                   }
               }
            }
            //load tiles
            foreach (var tileData in saveData.Tiles)
            {
                tileMapData[tileData.X, tileData.Y] = tileData.TileID;
            }
            foreach (var tileHeightData in saveData.tileHeights)
            {
                smoothCeiling[tileHeightData.X] = tileHeightData.Y;
            }
            Console.WriteLine("Game loaded!");
        }
    }
}
