using System;

namespace WPFTerraria
{
    public class Ingredient
    {
        public Item Item { get; set; }
        public int Amount { get; set; }
    }

    public class RecipeModel
    {
        public int Id { get; set; }
        public int CraftingTableId { get; set; }
        public Item OutputItem { get; set; }
        public int OutputQuantity { get; set; }
        public List<Ingredient> Ingredients { get; } = new List<Ingredient>();
//can have multiple ingredients per recipe
    }
}
