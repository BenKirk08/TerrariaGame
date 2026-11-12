using System;
using System.Collections.Generic;
using System.Linq;

namespace WPFTerraria
{
    public class ItemCrafting
    {
        private readonly List<RecipeModel> _all; //all recipes list
        public ItemCrafting(List<RecipeModel> recipes) => _all = recipes ??
new(); //constructor sets recipes or empty

        public IEnumerable<RecipeModel> AllRecipes => _all; //see recipes but
not edit

        public int MaxCraftableCount(Inventory myInventory, RecipeModel
thisRecipe) //max times craftable
        {
            if (myInventory == null || thisRecipe == null) return 0; //null
check

            var counts = new Dictionary<string,
int>(StringComparer.OrdinalIgnoreCase); //inventory item counts
            foreach (var slot in myInventory.PlayersItems) //loop inventory
            {
                if (slot?.Item == null || slot.Quantity <= 0)
                {
                    continue; //skip empty slot
                }
                counts[slot.Item.Name] = counts.TryGetValue(slot.Item.Name,
out int currentAmount)
                                        ? currentAmount + slot.Quantity :
slot.Quantity; //add to count if already have
            }

              int max = int.MaxValue; //start max possible
              foreach (var ingredient in thisRecipe.Ingredients) //loop
ingredients
              {
                counts.TryGetValue(ingredient.Item.Name, out int
itemQuantity); //how many player has
                if (ingredient.Amount <= 0)
                {
                    continue; //skip invalid
                }
                int canCraft = itemQuantity / ingredient.Amount; //calculate
craftable amount
                if (canCraft < max)
                {



                    max = canCraft; //set min across all -> ie 2 items to
craft, w wood cld craft sword
                }                   //twice but w ingots only 1, then only
will be able to craft 1 as min
            }
            return max == int.MaxValue ? 0 : max; //return 0 if no craft
        }

        public bool CanCraft(Inventory inv, RecipeModel recipe) =>
MaxCraftableCount(inv, recipe) > 0; //check craftable

        public int Craft(Inventory myInventory, RecipeModel thisRecipe, int
craftingTimes) //craft method
        {
            if (myInventory == null || thisRecipe == null || craftingTimes <=
0) return 0; //invalid check
            int max = MaxCraftableCount(myInventory, thisRecipe); //get max
craft count
            int craftTimes = Math.Min(craftingTimes, max); //clamp times ->
can only craft the max amount of times
            if (craftTimes <= 0)
            {
                return 0; //nothing craftable
            }

              foreach (var ingredient in thisRecipe.Ingredients) //remove
ingredients
              {
                int toRemove = ingredient.Amount * craftTimes; //amount needed
                for (int i = 0; i < myInventory.PlayersItems.Count && toRemove
> 0; i++) //loop slots as long as not amount gotten
                {
                    var slot = myInventory.PlayersItems[i]; //current slot
                    if (slot?.Item == null)
                    {
                        continue; //skip empty
                    }
                    if (!string.Equals(slot.Item.Name, ingredient.Item.Name,
StringComparison.OrdinalIgnoreCase))
                    {
                        continue; //check match -> if doesnt move to next slot
                    }

                      int take = Math.Min(slot.Quantity, toRemove); //take from
slot
                      slot.Quantity -= take; //reduce quantity
                      toRemove -= take; //update remaining

                  }
              }




              int totalOut = thisRecipe.OutputQuantity * craftTimes; //total
outputs
            var outItem =
RecipeToItemFactory.CreateByName(thisRecipe.OutputItem.Name); //create output
item
            myInventory.AddItem(outItem, totalOut); //add to inventory

              return craftTimes; //return times crafted
          }

        public IEnumerable<RecipeModel> FilterByTables(IEnumerable<int>
tableIds)
        {
            if (tableIds == null) tableIds = new List<int>();
            var tableSet = new HashSet<int>(tableIds);
            //always include 7
            return _all.Where(recipe => recipe.CraftingTableId == 7 ||
tableSet.Contains(recipe.CraftingTableId));
        }
    }
}
