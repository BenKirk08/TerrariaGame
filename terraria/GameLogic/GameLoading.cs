using System;
using System.Windows.Media.Imaging;
using WPFTerraria.StoreData;

namespace WPFTerraria
{
    public partial class MainWindow
    {
        public int chunkSize = (int)Math.Round(Utilities.TileSize * 0.5);
        private void LoadChunk(int chunkX, int chunkY)
        {
            if (!loadedChunks.Add((chunkX, chunkY))) return; //skip if already
loaded, and also addds

            int startX = chunkX * chunkSize;
            int startY = chunkY * chunkSize;
            int endX = Math.Min(startX + chunkSize, mapX_Width);    3&4D Loops from
            int endY = Math.Min(startY + chunkSize, mapY_Height);   when called
            for (int worldX = startX; worldX < endX; worldX++)
            {
                for (int worldY = startY; worldY < endY; worldY++)
                {
                    byte tileID = tileMapData[worldX, worldY]; //get tile type
from byte array
                    string connection = connectionMap[worldX, worldY];

                   if (tileID == 0 ) continue; //skip empty tiles

                    BitmapSource newImage =
TileDictionary.GetTileImage(tileID, connectionMap[worldX, worldY]);
                    loadedTileImages[worldX, worldY] = newImage;
                }
            }
        }
        private void UnloadFarChunks()
        {
            //convert camera position to tile coordinates
            int cameraTileX = (int)(cameraCenter.X / Utilities.TileSize);
            int cameraTileY = (int)(cameraCenter.Y / Utilities.TileSize);

            //calculate chunk position of camera
            int cameraChunkX = cameraTileX / chunkSize;
            int cameraChunkY = cameraTileY / chunkSize;

            //how far outside the camera chunks should be unloaded
            int chunkUnloadStartX = (cameraTileX - (CameraWidth / 2) -
(Tile_PreloadX + 2)) / chunkSize;
            int chunkUnloadEndX = (cameraTileX + (CameraWidth / 2) +
(Tile_PreloadX + 2)) / chunkSize;


            int chunkUnloadStartY = (cameraTileY - (CameraHeight / 2) -
(Tile_PreloadY + 2) + 1) / chunkSize;
            int chunkUnloadEndY = (cameraTileY + (CameraHeight / 2) +
(Tile_PreloadY + 2)) / chunkSize;

              //clamp values to map bounds
              chunkUnloadStartX = Math.Clamp(chunkUnloadStartX, 0, mapX_Width /
chunkSize);
              chunkUnloadEndX = Math.Clamp(chunkUnloadEndX, 0, mapX_Width /
chunkSize);
              chunkUnloadStartY = Math.Clamp(chunkUnloadStartY, 0, mapY_Height /
chunkSize);
              chunkUnloadEndY = Math.Clamp(chunkUnloadEndY, 0, mapY_Height /
chunkSize);

              //find chunks that are outside unloading range
              List<(int, int)> chunksToRemove = new List<(int, int)>();

            foreach (var chunk in loadedChunks)
            {
                 if (chunk.Item1 < chunkUnloadStartX || chunk.Item1 >
chunkUnloadEndX ||
                     chunk.Item2 < chunkUnloadStartY || chunk.Item2 >
chunkUnloadEndY)
                {
                     chunksToRemove.Add(chunk);
                }
            }
            foreach (var chunk in chunksToRemove) //triple for loop
            {
                 int startX = chunk.Item1 * chunkSize;
                 int startY = chunk.Item2 * chunkSize;
                 int endX = Math.Min(startX + chunkSize, mapX_Width);
                 int endY = Math.Min(startY + chunkSize, mapY_Height);

                  for (int x = startX; x < endX; x++)                  4D Loop when
                  {
                                                                       called
                      for (int y = startY; y < endY; y++)
                      {
                          if (loadedTileImages[x, y] != null)
                          {
                              loadedTileImages[x, y] = null; //free memory when
necessary
                          }
                      }
                  }
                  loadedChunks.Remove(chunk);
              }
        }
    }
}
