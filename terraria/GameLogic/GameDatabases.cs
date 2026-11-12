using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.IO;

namespace WPFTerraria
{
    public partial class MainWindow
    {
        //Database Stuff
        private void CreateItemsDatabase()
        {
            string databasePath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "DataBases",
"ItemsDB.db");
            string connectionString = $"Data
Source={databasePath};Version=3;";

            using (SQLiteConnection connection = new
SQLiteConnection(connectionString))
            {
                connection.Open();

                //disable foreign key enforcement before creating tables (as
they have foreign keys to one another)
                SQLiteCommand disableForeignKeys = new SQLiteCommand("PRAGMA
foreign_keys = OFF;", connection);
                disableForeignKeys.ExecuteNonQuery();

               try
               {
                    //Create Tables table
                    string createTablesTable = @"
                    CREATE TABLE IF NOT EXISTS Tables (
                        id INTEGER PRIMARY KEY,
                        name TEXT NOT NULL
                    );";
                    SQLiteCommand createTablesTableCommand = new
SQLiteCommand(createTablesTable, connection);
                    createTablesTableCommand.ExecuteNonQuery();

                    //Create Items table
                    string createItemsTable = @"
                    CREATE TABLE IF NOT EXISTS Items (
                        id INTEGER PRIMARY KEY,
                        name TEXT NOT NULL
                    );";
                    SQLiteCommand createItemsTableCommand = new
SQLiteCommand(createItemsTable, connection);
                    createItemsTableCommand.ExecuteNonQuery();

                    //Create Recipes table
                    string createRecipesTable = @"
                    CREATE TABLE IF NOT EXISTS Recipes (
                        id INTEGER PRIMARY KEY,
                        craftingtable INTEGER NOT NULL,
                        makeitemid INTEGER NOT NULL,
                        quantity INTEGER NOT NULL,
                        FOREIGN KEY (craftingtable) REFERENCES Tables(id),
                        FOREIGN KEY (makeitemid) REFERENCES Items(id)
                    );";
                    SQLiteCommand createRecipesTableCommand = new
SQLiteCommand(createRecipesTable, connection);
                    createRecipesTableCommand.ExecuteNonQuery();

                    //Create ItemRecipe linking table
                    string createItemRecipeTable = @"
                    CREATE TABLE IF NOT EXISTS ItemRecipe (
                        recipeid INTEGER NOT NULL,
                        itemid INTEGER NOT NULL,
                        amount INTEGER NOT NULL,
                        PRIMARY KEY(recipeid,itemid),
                        FOREIGN KEY (recipeid) REFERENCES Recipes(id),
                        FOREIGN KEY (itemid) REFERENCES Items(id)
                    );";
                    SQLiteCommand createItemRecipeTableCommand = new
SQLiteCommand(createItemRecipeTable, connection);
                    createItemRecipeTableCommand.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in creating Database Items Tables
" + ex.Message);
                }

                //import C for each table (All in debug, bin so have rest of
location)
                string csvFilePathItems =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "csvFiles",
"itemsCSV.csv"); //csv file path for Items
                string csvFilePathRecipes =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "csvFiles",
"recipesCSV.csv"); //csv file path for Recipes
                string csvFilePathTables =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "csvFiles",
"tablesCSV.csv"); //csv file path for Tables
                string csvFilePathItemRecipe =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "csvFiles",
"itemrecipeCSV.csv"); //csv file path for ItemRecipe Link CSV

               try
               {

                    ImportCSVToTable(databasePath, csvFilePathItemRecipe,
"ItemRecipe");
                    ImportCSVToTable(databasePath, csvFilePathTables,
"Tables"); //import tables csv, least foreign keys so first
                    ImportCSVToTable(databasePath, csvFilePathItems, "Items");
//import items csv
                    ImportCSVToTable(databasePath, csvFilePathRecipes,
"Recipes"); //import recipes csv
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in Importing CSVToTables (Items)
" + ex.Message);
                }

                //enable foreign key
                SQLiteCommand enableForeignKeys = new SQLiteCommand("PRAGMA
foreign_keys = ON;", connection);
                enableForeignKeys.ExecuteNonQuery();

                 connection.Close();
           }
        }
        private void CreateTilesDatabase()
        {
            string databasePath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "DataBases",
"TilesDB.db");
            string connectionString = $"Data
Source={databasePath};Version=3;";

            using (SQLiteConnection connection = new
SQLiteConnection(connectionString))
            {
                connection.Open();
                try
                {
                    //Create Tiles Table
                    string createTilesTable = @"
                    CREATE TABLE IF NOT EXISTS Tiles (
                        id INTEGER,
                        sub_id INTEGER,
                        name TEXT NOT NULL,
                        PRIMARY KEY(id,sub_id)
                    );";
                    SQLiteCommand createTablesTableCommand = new
SQLiteCommand(createTilesTable, connection);
                    createTablesTableCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in creating Database Tiles Tables
" + ex.Message);

               }

                string csvFilePathTiles =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "csvFiles",
"tilesCSV.csv"); //csv file path for Tiles

               try
               {
                    ImportCSVToTable(databasePath, csvFilePathTiles, "Tiles");
//import tables csv, least foreign keys so first
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in Importing CSVToTables (Tiles)
" + ex.Message);
                }

               connection.Close();
           }
        }
        private void ImportCSVToTable(string databasePath, string csvFilePath,
string tableName)
        {
            //tells VB where database is located and how to connect it
            string connectionString = $"Data
Source={databasePath};Version=3;";

            using (SQLiteConnection connection = new
SQLiteConnection(connectionString)) //Like streamreader
            {
                connection.Open();

               //check if table already has data
               string checkForValues = $"SELECT COUNT(*) FROM {tableName}";
               SQLiteCommand checkCommand = new SQLiteCommand(checkForValues,
connection);
                int rowCount = Convert.ToInt32(checkCommand.ExecuteScalar());
//returns number of rows currently in table

               if (!File.Exists(csvFilePath)) //if file not found
               {
                   Console.WriteLine("CSV file not found: " + csvFilePath);
                   return;
               }

                //count number of lines in csv (excluding header)
                int csvRowCount = File.ReadLines(csvFilePath).Count() - 1;

                //if table and csv have same number of rows
                if (rowCount == csvRowCount)
                {
                    Console.WriteLine($"{tableName} already matches CSV row
count ({csvRowCount}), so not importing more.");

                   connection.Close();
                   return;
                }
                else
                {
                    Console.WriteLine($"{tableName} has {rowCount} rows but
CSV has {csvRowCount}, reimporting...");
                }

                //if it doesnt have correct amount of rows, clear and import
again
               string deleteData = $"DELETE FROM {tableName}";
               SQLiteCommand deleteCommand = new SQLiteCommand(deleteData,
connection);
               deleteCommand.ExecuteNonQuery(); //delete all rows from the
table
               Console.WriteLine("All data deleted from: " + tableName);

               using (StreamReader fileIn = new StreamReader(csvFilePath))
               {
                   string headerLine = fileIn.ReadLine(); //read header line
                   var fieldNames = headerLine.Split(','); //split into
column names

                   while (!fileIn.EndOfStream)
                   {
                       string thisLine = fileIn.ReadLine(); //read line
                       var values = thisLine.Split(','); //split into values

                        //Get values of that row
                        string insertRow = $"INSERT INTO {tableName}
({string.Join(", ", fieldNames)}) VALUES ({string.Join(", ", values.Select((v,
i) => $"@field{i}"))})"; //creates a new @field(number) parameter for each
element

                        //insert values into table
                        SQLiteCommand insertValuesCommand = new
SQLiteCommand(insertRow, connection);

                        //prevent SQL injection (values that mess with code)
if there is special character eg commas, quotes
                        for (int i = 0; i < values.Length; i++)
                        {

insertValuesCommand.Parameters.AddWithValue($"@field{i}", values[i]);
//value{i} par
                        }

                       insertValuesCommand.ExecuteNonQuery(); //execute the
insert
                   }
               }


                  Console.WriteLine($"Data imported into {tableName}
successfully");

                  connection.Close();
            }
        }

        private protected void CreateAllDatabases() //NOTE: private protected
allows use across partial classes
        {
            CreateItemsDatabase();
            CreateTilesDatabase();
        }


        //Load from database -> for checking database, unused in actual game
        private void LoadDataFromTable(string tableName)
        {
            string databasePath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "DataBases",
"ItemsDB.db");
            string connectionString = $"Data
Source={databasePath};Version=3;";

            using (SQLiteConnection connection = new
SQLiteConnection(connectionString))
            {
                connection.Open();

                  //get all values in table
                  string SelectAll = $"SELECT * FROM {tableName}";

                  //use SQLiteDataAdapter to use select statement and fill in
DataTable
                SQLiteDataAdapter dataAdapter = new
SQLiteDataAdapter(SelectAll, connection);
                DataTable dataTable = new DataTable();

                  //fill the DataTable with data from the database
                  dataAdapter.Fill(dataTable);

                  //atach the DataTable to the DataGridView
                  //dataGrid.ItemsSource = dataTable.DefaultView;
            }
        }
    }
}




En77esLogic
