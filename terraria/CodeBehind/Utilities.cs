using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Data.SQLite;
using System.Windows;

namespace WPFTerraria
{
    public static class Utilities //helper method -> static so no object
creation needed
    {
        //game settings needed for multiple classes
        public const int TileSize = 24; //TileSize in pixels

        //Goes from basedirectory (in bin/debug) and goes out to main file
where allGameAssets are
        public static string GetSolutionDirectory() //static so can call w/o
instance of class in other classes
        {
            string solutionDirectory = AppDomain.CurrentDomain.BaseDirectory;

           try
           {
                 while (!Directory.Exists(Path.Combine(solutionDirectory,
"AllGameAssets")) && Directory.GetParent(solutionDirectory) != null) //while
the current directory doesnt have AllGameAssets and no file left
                 {
                     solutionDirectory =
Directory.GetParent(solutionDirectory)?.FullName; //gets 1 up of current
directory, ? null operator, fullname makes sure its full directory
                 }
                 return solutionDirectory;
            }
             catch (Exception ex)
            {
                 Console.WriteLine("Error getting Solution directory " +
ex.Message);
                 return null;
            }
        }


       public static BitmapSource LoadImage(string ImagePath)
       {
           var bitmap = new BitmapImage();

            //force immediate load

           bitmap.BeginInit();
           bitmap.UriSource = new Uri($"{ImagePath}", UriKind.Relative);
           bitmap.CacheOption = BitmapCacheOption.OnLoad;
           bitmap.EndInit();
           bitmap.Freeze(); //makes it thread-safe

               return bitmap;
       }


        //get the image path of the requested png using database
        public static string GetPictureImagePath(string DataBaseFileName,
string PicturesTable, string PNGsHandle, string PictureName)
        {
            string databasePath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "DataBases",
$"{DataBaseFileName}");
            string connectionString = $"Data
Source={databasePath};Version=3;";

            using (SQLiteConnection connection = new
SQLiteConnection(connectionString))
            {
                connection.Open();

                //get id of the item
                string GetPictureID = $"SELECT id FROM {PicturesTable} WHERE
name = \'{PictureName}\'";

                  SQLiteCommand command = new SQLiteCommand(GetPictureID,
connection);
                  string PictureID = null; //initilise

                  try
                  {
                    PictureID = Convert.ToString(command.ExecuteScalar());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in finding PictureID (in
Utilitys): " + ex.Message);
                    return null; //if not found return Null
                }
                connection.Close();

                  try
                  {
                    string PicturePath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "Content",
"Images", $"{PNGsHandle}_{PictureID}.png");
                    return PicturePath;
                }
                catch (Exception ex)

                  {
                      Console.WriteLine("Error in finding Image Path: " +
ex.Message);
                      return null;
                  }
           }
       }

        //resize an image -> not dynamic but static for performance ie,
creates an actual bitmap w/these pixels
        public static BitmapSource ResizeImage(BitmapSource sourceImage,
double targetWidth, double targetHeight)
        {
            //create a scale to transform the image
            double scaleX = (double)targetWidth / sourceImage.PixelWidth;
            double scaleY = (double)targetHeight / sourceImage.PixelHeight;

            //use TransformedBitmap to apply scaling->same image, js scaled
            TransformedBitmap transformedBitmap = new
TransformedBitmap(sourceImage, new ScaleTransform(scaleX, scaleY));

            RenderOptions.SetBitmapScalingMode(transformedBitmap,
BitmapScalingMode.NearestNeighbor);

               return transformedBitmap;
       }


        //Images in png with multiple images -> Tiles, players, npc, enemys
        public static void SetAllImages(BitmapSource tileset, string[,]
layout, Dictionary<string, BitmapSource> imageRelations, string[]
RelationTypes,
            int tileSizeWidth, int tileSizeHeight, int spacingSize, int
resizedImageWidth, int resizedImageHeight) //not optimal if dont often use all
images, useful in animations where all used regularly
        {
            foreach (var relationType in RelationTypes)
            {                                                            3D Loop when
                //get coords for the current connection type             called
                var coords = GetTileImageCoords(layout, relationType);

                  //If has value coords exist
                  if (coords.HasValue)
                  {
                      int row = coords.Value.row;
                      int col = coords.Value.col;

                    //extract the tile image from the tileset
                    BitmapSource image = ImagePNGExtractor(tileset,
tileSizeWidth, tileSizeHeight, spacingSize, row, col);

                    BitmapSource resizedImage = Utilities.ResizeImage(image,
resizedImageWidth, resizedImageHeight);

                     //add the image to the dictionary
                     imageRelations[relationType] = resizedImage;
                 }
                 else
                 {
                     Console.WriteLine("No tile found for connection type: " +
relationType);
                 }
           }
       }



        public static BitmapSource ImagePNGExtractor(BitmapSource tileset, int
tileSizeWidth, int tileSizeHeight, int SpacingSize, int row, int col)
        {
            //calc the position of the tile in the tileset (w/considering
spacing) -> start row,col at 0,0
            int x = col * (tileSizeWidth + SpacingSize);
            int y = row * (tileSizeHeight + SpacingSize);

            //define the area of the tile using a Rectangle structure
            Int32Rect tileArea = new Int32Rect(x, y, tileSizeWidth,
tileSizeHeight);

           //create the cropped image from the specified area
           BitmapSource croppedTile = new CroppedBitmap(tileset, tileArea);
           return croppedTile;
       }


        //Gets the row and col corresponding to the wanted 'connected' tile
        public static (int row, int col)? GetTileImageCoords(string[,] layout,
string relationType) //allows return row and col or null
        {
            //returns row and col of where that tile image of connected (LURD)
is
            for (int row = 0; row < layout.GetLength(0); row++)
            {                                                          3D Loop when
                for (int col = 0; col < layout.GetLength(1); col++)    called
                {
                    if (layout[row, col] == relationType)
                    {
                        return (row, col);
                    }
                }
            }
            //else nothing
            Console.WriteLine("no connected tile found");
            return null;
        }


        public static BitmapSource GetSpecificImage(Dictionary<string,
BitmapSource> ImageRelations, string relationType)
        {
            if (ImageRelations.ContainsKey(relationType))
            {
                return ImageRelations[relationType];
            }

            Console.WriteLine($"Tile image not found for connection type:
{relationType}");
            return null;
        }

        public static T FindChild<T>(DependencyObject parent, string
childName) where T : DependencyObject //for crafting
        {
            if (parent == null) return null; //stop if no parent to search

            int count = VisualTreeHelper.GetChildrenCount(parent); //get how
many children this parent has
            for (int i = 0; i < count; i++) //loop through all children
            {
                var child = VisualTreeHelper.GetChild(parent, i); //get child
at index

                if (child is T t && (child as FrameworkElement)?.Name ==
childName) //check if child matches type and name
                {
                    return t; //found it -> return result
                }
                                                          Recursion
                //RECURISION!!!!!
                var result = FindChild<T>(child, childName); //recursively
search inside this child
                if (result != null) //if found deeper down
                {
                    return result; //bubble result back up
                }
            }
            return null; //no match found in this branch

        }
    }
}
