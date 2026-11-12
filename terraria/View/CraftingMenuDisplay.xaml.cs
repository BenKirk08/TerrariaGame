using System.Windows;
using System.Windows.Controls;
using System;

namespace WPFTerraria.View
{
    public partial class CraftingMenuDisplay : UserControl
    {
        private Inventory _inventory;
        private ItemCrafting _itemCrafting;
        private List<RecipeModel> _allRecipes;

        public List<int> CurrentCraftingTableId { get; set; } = new
List<int>(); // null = show "hand" crafts too, if applicable
        private List<int> _lastKnownCraftingStationIds = new List<int>();

        public const int HandCraftID = 7; //id for hand crafting

        //map ingredient name to recipes that use it
        private Dictionary<string, List<RecipeModel>>
_recipesByIngredientName;
        //map recipe idto a slot
        private readonly Dictionary<int, RecipeSlotDisplay> _slotByRecipeId =
new Dictionary<int, RecipeSlotDisplay>();

        public CraftingMenuDisplay()
        {
            InitializeComponent();
            ShowOnlyCraftable.Checked += (_, __) => RefreshList(); //refresh
when checkbox toggled
            ShowOnlyCraftable.Unchecked += (_, __) => RefreshList();
        }

        public Inventory Inventory //property for current inventory
        {
            get { return _inventory; }
            set
            {
                if (_inventory != null)
                {
                    _inventory.ItemChanged -= OnInventoryItemChanged;
//unsubscribe from old inventory events
                }

                  _inventory = value; //set new inventory

                  _allRecipes = DatabaseRecipeLoader.LoadAllRecipes(); //load
recipes from db
                _itemCrafting = new ItemCrafting(_allRecipes); //create
crafting system with recipes


               BuildIngredientIndex(); //build lookup of items to recipes

                if (_inventory != null)
                {
                    _inventory.ItemChanged += OnInventoryItemChanged;
//subscribe to new inventory events
                }
            }
        }


        private void OnInventoryItemChanged(object sender,
ItemChangedEventArgs thisEvent) //called when item changes
        {
            string changedItem = thisEvent.ItemName; //get name of changed
item
            if (string.IsNullOrWhiteSpace(changedItem)) return; //ignore empty
            if (_recipesByIngredientName == null) return; //ignore if lookup
not built - should not happen

            bool onlyCraftable = ShowOnlyCraftable.IsChecked == true; //check
if filter toggle is on

            if (_recipesByIngredientName.TryGetValue(changedItem, out var
affectedRecipes)) //find recipes using this item
            {
                foreach (var recipe in affectedRecipes) //loop through each
affected recipe
                {
                    int max = _itemCrafting.MaxCraftableCount(_inventory,
recipe); //get max craftable count
                    bool hasMaterials = max > 0; //see if has materials
                    bool hasStation = recipe.CraftingTableId == HandCraftID ||
CurrentCraftingTableId.Contains(recipe.CraftingTableId); //see if correct
station is nearby

                    if (_slotByRecipeId.TryGetValue(recipe.Id, out var
recipeSlot)) //if recipe already has a slot
                    {
                        if (onlyCraftable && (!hasMaterials || !hasStation))
//if filter is on and cant craft
                        {
                            RemoveSlot(recipe.Id); //remove slot from ui
                        }
                        else
                        {
                            recipeSlot.IsCraftable = hasMaterials &&
hasStation; //update craftable state
                            recipeSlot.MaxCraftCount = max; //update craft
count



                            if (!recipeSlot.IsCraftable)
                            {
                                WriteMissingText(hasStation, hasMaterials,
recipe, recipeSlot);
                            }

                        }
                    }
                    else //if slot not present
                    {
                        bool shouldShow = !onlyCraftable || (hasMaterials &&
hasStation); //see if slot should show
                        if (shouldShow)
                        {
                            var newSlot = CreateSlot(recipe, max); //create
new slot for recipe
                            newSlot.IsCraftable = hasMaterials && hasStation;
//set craftable state
                            _slotByRecipeId[recipe.Id] = newSlot; //track in
map
                            InsertSlotSorted(newSlot); //insert into ui

                            if (!newSlot.IsCraftable)
                            {
                                WriteMissingText(hasStation, hasMaterials,
recipe, newSlot);
                            }

                        }
                    }
               }

               ReflowCanvas(); //reposition slots after updates
           }
       }


        private void BuildIngredientIndex() //create lookup from ingredient
name to recipes
        {
            _recipesByIngredientName = new Dictionary<string,
List<RecipeModel>>(StringComparer.OrdinalIgnoreCase); //init dictionary
ignoring case
            foreach (var recipe in _allRecipes) //loop through all recipes
            {
                foreach (var ingredient in recipe.Ingredients) //loop through
each ingredient in recipe
                {
                    if (ingredient?.Item?.Name == null) continue; //skip if
ingredient or name missing
                    if
(!_recipesByIngredientName.TryGetValue(ingredient.Item.Name, out var list))
//check if ingredient already in dictionary

                   {
                       list = new List<RecipeModel>(); //create new list if
not
                        _recipesByIngredientName[ingredient.Item.Name] = list;
//store list in dictionary
                    }
                    list.Add(recipe); //add recipe to ingredient list
                }
            }
        }

       public void RefreshList() //rebuild recipe ui list
       {
           if (_inventory == null || _itemCrafting == null) return;

           _slotByRecipeId.Clear();
           ListHost.Children.Clear();

            //get all recipes
            var allRecipes = _allRecipes.OrderBy(r => r.OutputItem?.Name,
StringComparer.OrdinalIgnoreCase);

            bool onlyCraftable = ShowOnlyCraftable.IsChecked == true; //see if
only cragftable checked

            foreach (var recipe in allRecipes) //for every recipe
            {
                int maxCraftCount =
_itemCrafting.MaxCraftableCount(_inventory, recipe); //get max craft count
                bool hasMaterials = maxCraftCount > 0; //se if has the
materials

                //determine if required crafting table is nearby
                bool hasStation = recipe.CraftingTableId == HandCraftID ||
CurrentCraftingTableId.Contains(recipe.CraftingTableId);

                //skip recipes if "Show Only Craftable" is active and dont
have materials or station
                if (onlyCraftable && (!hasMaterials || !hasStation))
                {
                    continue;
                }

                RecipeSlotDisplay slot = CreateSlot(recipe, maxCraftCount);
//create slot for recipe

               //determine craftable state & reason
               if (hasMaterials && hasStation)
               {
                   slot.IsCraftable = true;
               }
               else
               {

                   WriteMissingText(hasStation, hasMaterials, recipe, slot);
               }

               InsertSlotSorted(slot); //insert into ui sorted
               _slotByRecipeId[recipe.Id] = slot; //store slot in map
           }

            ReflowCanvas(); //position all slots
        }
        private void WriteMissingText(bool hasStation, bool hasMaterials,
RecipeModel recipe, RecipeSlotDisplay slot)
        {
            string stationName = string.Empty;
            if (!hasStation)
            {

TileDictionary.WorkStationToID.TryGetValue(recipe.CraftingTableId, out var
name);
                stationName = name;
            }
            slot.IsCraftable = false;

            //build message for why it’s missing
            if (hasMaterials && !hasStation)
            {
                slot.MissingReason = $"Missing Station: {stationName}";
            }
            else if (!hasMaterials && hasStation)
            {
                slot.MissingReason = "Missing Materials";
            }
            else
            {
                slot.MissingReason = $"Missing Materials + Station:
{stationName}";
            }
        }

        private RecipeSlotDisplay CreateSlot(RecipeModel recipe, int max)
//create a recipe slot ui element
        {
            var slot = new RecipeSlotDisplay
            {
                Recipe = recipe, //bind recipe
                Width = 380,
                Height = 80
            };

           slot.IsCraftable = max > 0; //set craftable state
           slot.MaxCraftCount = max; //set max craft count

           slot.CraftRequested += (_, args) => //subscribe to craft button
click

           {
               int times = args.RequestedTimes == int.MaxValue //decide times
to craft
                    ? _itemCrafting.MaxCraftableCount(_inventory, recipe) //if
all - use max craft count
                    : args.RequestedTimes; //otherwise use requested

               if (times <= 0) return; //stop if none
               _itemCrafting.Craft(_inventory, recipe, times); //perform
crafting
                                                                 //no full
refresh as inventory events update affected slots
            };
            return slot;
        }

        private void InsertSlotSorted(RecipeSlotDisplay slot) //insert slot
into list sorted by name
        {
            string name = slot.Recipe?.OutputItem?.Name ?? string.Empty; //get
name of recipe output
            int index = 0; //start index
            while (index < ListHost.Children.Count) //loop through ui children
            {
                RecipeSlotDisplay thisSlot =
(RecipeSlotDisplay)ListHost.Children[index]; //get another slot
                var thisSlotsName = thisSlot.Recipe?.OutputItem?.Name ??
string.Empty; //get slots name
                if (string.Compare(name, thisSlotsName,
StringComparison.OrdinalIgnoreCase) < 0)
                {
                    break; //insert before if comes first alphabetically
                }
                index++; //else go to next slot
            }
            ListHost.Children.Insert(index, slot); //insert slot at position
        }

        private void RemoveSlot(int recipeId) //remove recipe slot by id
        {
            if (_slotByRecipeId.TryGetValue(recipeId, out var slot)) //check
if exists in map
            {
                ListHost.Children.Remove(slot); //remove from ui
                _slotByRecipeId.Remove(recipeId); //remove from map
            }
        }

       private void ReflowCanvas() //update positions of slots
       {
           double y = 0; //start y position
           const double gap = 8; //gap between slots


            foreach (RecipeSlotDisplay child in ListHost.Children) //loop
throygh slots
            {
                Canvas.SetLeft(child, 0);
                Canvas.SetTop(child, y);
                y += child.Height + gap; //increase y by slot height and gap
            }
            ListHost.Height = y; //set canvas height to total
        }

        public List<int> GetNearbyCraftingStationIDs()
        {
            CurrentCraftingTableId = new List<int>();
            var main = Application.Current.MainWindow as MainWindow;
            int playerCurrentTileX = (int)main.myPlayer.playerX /
Utilities.TileSize;
            int playerCurrentTileY = (int)main.myPlayer.playerY /
Utilities.TileSize;

            for (int x = -8; x <= 8; x++)
            {
                if (!(x + playerCurrentTileX < 0 || (x + playerCurrentTileX >=
main.newMapWidth)))
                {
                    for (int y = -8; y <= 8; y++)
                    {
                        if (!(y + playerCurrentTileY < 0 || (y +
playerCurrentTileY >= main.newMapHeight)))
                        {
                            if
(TileDictionary.TileData[main.tileMapData[playerCurrentTileX + x,
playerCurrentTileY + y]].IsWorkStation)
                            {

TileDictionary.TileData[main.tileMapData[playerCurrentTileX + x,
playerCurrentTileY + y]].ExtraData.TryGetValue("WorkStation ID", out var
value);
                                try
                                { CurrentCraftingTableId.Add((int)(value)); }
                                catch { Console.WriteLine("Error adding
crafting table ID to list"); }
                            }
                        }
                    }
                }
            }
            return CurrentCraftingTableId;
        }
        public void UpdateCraftingStationsIfNeeded(List<int> newStationIds)
        {
            if (newStationIds == null) newStationIds = new List<int>();

            //compare with last known

            bool changed = _lastKnownCraftingStationIds.Count !=
newStationIds.Count
                           ||
!_lastKnownCraftingStationIds.All(newStationIds.Contains);

            if (!changed)
                return; //no change, skip heavy refresh

            _lastKnownCraftingStationIds = new List<int>(newStationIds);
//store copy
            CurrentCraftingTableId = newStationIds; //update current table IDs
            RefreshList(); //refresh visible recipes
        }
    }
}
