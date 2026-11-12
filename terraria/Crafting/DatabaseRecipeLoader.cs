using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace WPFTerraria
{
    public static class DatabaseRecipeLoader
    {
        public static List<RecipeModel> LoadAllRecipes()
        {
            string databasePath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "DataBases",
"ItemsDB.db"); //path to database file
            string connectionString = $"Data
Source={databasePath};Version=3;"; //sqlite connection string
            List<RecipeModel> recipes = new List<RecipeModel>(); //list to
store all recipes

            using (SQLiteConnection connection = new
SQLiteConnection(connectionString)) //open connection to database
            {
                connection.Open();

                string selectAll = @"
                    SELECT
                        Recipes.id AS RecipeId,
                        Recipes.craftingtable AS CraftingTableId,
                        Tables.name AS CraftingTableName,
                        Recipes.makeitemid AS OutputItemId,
                        Items1.name AS OutputItemName,
                        Recipes.quantity AS OutputQuantity,
                        ItemRecipe.itemid AS IngredientItemId,
                        Items2.name AS IngredientItemName,
                        ItemRecipe.amount AS IngredientAmount
                    FROM Recipes, Items AS Items1, Tables, ItemRecipe, Items
AS Items2
                    WHERE Recipes.makeitemid = Items1.id
                    AND Recipes.craftingtable = Tables.id
                    AND Recipes.id = ItemRecipe.recipeid
                    AND ItemRecipe.itemid = Items2.id;"; //use as to make
names easier
                                                         //Items table twice
as need 2 dif items tables for 2 item.ids

                using (SQLiteCommand command = new SQLiteCommand(selectAll,
connection)) //create sql command



                using (SQLiteDataReader reader = command.ExecuteReader())
//execute and read results
                {
                    Dictionary<int, RecipeModel> recipeMap = new
Dictionary<int, RecipeModel>(); //map recipes by id

                    while (reader.Read()) //loop rows
                    {
                        int recipeId =
reader.GetInt32(reader.GetOrdinal("RecipeId"));

                        if (!recipeMap.TryGetValue(recipeId, out var recipe))
                        {
                            string outputName =
reader.GetString(reader.GetOrdinal("OutputItemName"));
                            var outputItem =
RecipeToItemFactory.CreateByName(outputName);

                           //skip recipe if output item not implemented
                           if (outputItem == null)
                           {
                               continue;
                           }

                            int dbTableId =
reader.GetInt32(reader.GetOrdinal("CraftingTableId")); //get crafting table id
from db

                            //check if table exists in TileDictionary -> if
not, default to hand (7)
                            if
(!TileDictionary.WorkStationToID.ContainsKey(dbTableId) && dbTableId != 7)
                            {
                                dbTableId = 7; //default to hand crafting
                            }

                           recipe = new RecipeModel //create new recipe
                           {
                               Id = recipeId,
                               CraftingTableId = dbTableId, //use verified or
defaulted id
                                OutputItem = outputItem,
                                OutputQuantity =
reader.GetInt32(reader.GetOrdinal("OutputQuantity"))
                            };


                           //add it to map
                           recipeMap[recipeId] = recipe;
                           recipes.Add(recipe);
                       }



                        string ingredientName =
reader.GetString(reader.GetOrdinal("IngredientItemName"));
                        int ingredientAmount =
reader.GetInt32(reader.GetOrdinal("IngredientAmount"));
                        var ingredientItem =
RecipeToItemFactory.CreateByName(ingredientName);

                       //if ingredient missing, mark recipe invalid
                       if (ingredientItem == null)
                       {
                           continue;
                       }

                       recipe.Ingredients.Add(new Ingredient
                       {
                           Item = ingredientItem,
                           Amount = ingredientAmount
                       });

                   }
                }
                connection.Close(); //close database connection
            }

            //after loading remove recipes w no valid ingredients
            recipes.RemoveAll(r => r.Ingredients.Count == 0);


            return recipes; //return list of recipes
        }
    }
}
