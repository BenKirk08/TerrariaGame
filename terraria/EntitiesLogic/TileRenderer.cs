using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFTerraria
{
    public class TileRenderer : FrameworkElement
    {
        private const int TileSize = Utilities.TileSize;
        private int ChunkSize;
        private readonly int _screenWidth, _screenHeight;
        private readonly int _mapWidth, _mapHeight;

        private readonly Dictionary<(int, int), WriteableBitmap> _activeChunks
= new Dictionary<(int, int), WriteableBitmap>();
        private Point _cameraCenter;
        private HashSet<(int, int)> _currentChunks = new HashSet<(int,
int)>();

       public TileRenderer(int mapWidth, int mapHeight, int chunkSize)
       {
           this.SnapsToDevicePixels = true; //so pixels snap to eachother
           RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

           _mapWidth = mapWidth;
           _mapHeight = mapHeight;
           _screenWidth = MainWindow.CameraWidth * TileSize;
           _screenHeight = MainWindow.CameraHeight * TileSize;
           ChunkSize = chunkSize;
       }

        public void UpdateLoadedChunks(HashSet<(int, int)> loadedChunks,
BitmapSource[,] loadedImages)
        {
            //unload the old chunks
            var chunksToRemove = new List<(int, int)>();
            foreach (var chunk in _activeChunks.Keys)
            {
                if (!loadedChunks.Contains(chunk)) {
chunksToRemove.Add(chunk); } //if chunks no longer loaded, remove it
            }

            foreach (var chunk in chunksToRemove) { UnloadChunk(chunk); }
//batch unloading

           //load the chunks that are loaded
           foreach (var chunk in loadedChunks)
           {
               if (!_activeChunks.ContainsKey(chunk))


               {                                         3D Loop from
                   LoadChunk(chunk, loadedImages);       when called
               }
           }

           //update current chunks and redraw
           _currentChunks = new HashSet<(int, int)>(loadedChunks);
           InvalidateVisual();
       }

        private void LoadChunk((int x, int y) chunk, BitmapSource[,]
loadedImages)
        {
            //create a new bitmap for whole chunk
            var chunkBitmap = new WriteableBitmap(
                ChunkSize * TileSize,
                ChunkSize * TileSize,
                96, 96, //standard dpi (dots per inch) ie this is resolution
                PixelFormats.Pbgra32, //the required pixel format for WPF
                null
            );

            chunkBitmap.Lock(); //w/try finally secure buffer for writing ->
if changing in multiple places at same time
            try
            {
                int stride = chunkBitmap.BackBufferStride; //bytes per row in
the bitmap, ie width x 4 (4 as number of bytes per pixel in 32 bit)
                byte[] buffer = new byte[stride * chunkBitmap.PixelHeight];
//byte array to store pixel data for chunk

                //initialize w/ transparent
                for (int i = 3; i < buffer.Length; i += 4) { buffer[i] = 0; }
                //targets alpha channel (for transparency, 4th) and sets to
fully transparent ie 0

               //populate chunk with tiles
               for (int tileX = 0; tileX < ChunkSize; tileX++)
               {
                   for (int tileY = 0; tileY < ChunkSize; tileY++)
                   {
                       //convert to world coords
                       int worldX = chunk.x * ChunkSize + tileX;
                       int worldY = chunk.y * ChunkSize + tileY;

                        if (worldX >= _mapWidth || worldY >= _mapHeight) {
continue; } //skip if out of map

                       //gets tile texture from main game
                       var tileImage = loadedImages[worldX, worldY];
                       if (tileImage == null) { continue; }



                        //convert to correct format if needed
                        if (tileImage.Format != PixelFormats.Pbgra32)
                        {
                            tileImage = new FormatConvertedBitmap(tileImage,
PixelFormats.Pbgra32, null, 0);
                        }

                        //calculate the place in chunk
                        int placeX = tileX * TileSize;
                        int placeY = tileY * TileSize;

                       tileImage.CopyPixels(
                           new Int32Rect(0, 0, TileSize, TileSize),
//rectangle used
                            buffer, //destination buffer
                            stride, //bytes per row in chunk
                            placeY * stride + placeX * 4 //Starting offset
                       );
                   }
               }

                //copy entire bitmap at once
                chunkBitmap.WritePixels(
                    new Int32Rect(0, 0, chunkBitmap.PixelWidth,
chunkBitmap.PixelHeight),
                    buffer,
                    stride,
                    0
                );
            }
            finally
            {
                chunkBitmap.Unlock();
                _activeChunks[chunk] = chunkBitmap; //store in active chunks
            }
        }


        //outside to reduce remaking them each time -> stress on garbage
collection
        byte[] clear = new byte[TileSize * TileSize * 4]; //reusuable clear
buffer auto 0
        byte[] tilePixels = new byte[TileSize * TileSize * 4]; //reusuable
buffer -> changes
        public void UpdateSingleTile(int tileX, int tileY, BitmapSource
tileImage)
        {
            //ensure valid coords
            if (tileX < 0 || tileY < 0 || tileX >= _mapWidth || tileY >=
_mapHeight) { return; }

            //calculate chunk coords
            int chunkX = tileX / ChunkSize;

            int chunkY = tileY / ChunkSize;

            //get chunk bitmap
            if (!_activeChunks.TryGetValue((chunkX, chunkY), out
WriteableBitmap chunk)) { return; }

            //calculate position in chunk
            int posX = tileX % ChunkSize;
            int posY = tileY % ChunkSize;

            //calculate pixel position
            int placeX = posX * TileSize;
            int placeY = posY * TileSize;

            chunk.Lock(); //secure again
            try
            {
                int chunkStride = chunk.BackBufferStride; //bytes per row in
the bitmap, ie width x 4 (4 as number of bytes per pixel in 32 bit)

                //check valid
                if (placeX < 0 || placeY < 0 ||
                    placeX + TileSize > chunk.PixelWidth ||
                    placeY + TileSize > chunk.PixelHeight)
                    return;

                //calculate buffer offset/memory location
                int bufferOffset = placeY * chunkStride + placeX * 4; //4 as
number of bytes per pixel in 32bit

                IntPtr destPtr = IntPtr.Add(chunk.BackBuffer, bufferOffset);
//gets exact memory address for tiles pixel data
                //chunkbackbuffer is start of chunk pixel data in memory

                if (tileImage != null)
                {
                    //1. change to correct format
                    if (tileImage.Format != PixelFormats.Pbgra32)
                        tileImage = new FormatConvertedBitmap(tileImage,
PixelFormats.Pbgra32, null, 0);

                   //2. extract tile pixel data
                   int tileStride = TileSize * 4;
                   tileImage.CopyPixels(
                       new Int32Rect(0, 0, TileSize, TileSize),
                       tilePixels,
                       tileStride, //source stride (tile)
                       0
                   );

                   //3. write to the chunk with tile stride
                   chunk.WritePixels(


                        new Int32Rect(placeX, placeY, TileSize, TileSize),
                        tilePixels,
                        tileStride, //destination stride -> this as only
updates the rectangle of tilesize
                        0
                    );
                }
                else //if no image, clear
                {
                    //clear tile area with tile stride

                   chunk.WritePixels(
                       new Int32Rect(placeX, placeY, TileSize, TileSize),
                       clear, //uses clear bytes
                       TileSize * 4, //tile stride (96 bytes)
                       0
                   );
                }
            }
            finally
            {
                chunk.Unlock();
                InvalidateVisual();
            }
       }

        private void UnloadChunk((int x, int y) chunk) =>
_activeChunks.Remove(chunk);

       protected override void OnRender(DrawingContext drawingContext)
       {
           base.OnRender(drawingContext);

            //draw all active chunks
            foreach (var chunk in _currentChunks)
            {
                if (!_activeChunks.TryGetValue(chunk, out var bitmap))
continue;

                //convert chunk coords to world pixels
                int worldX = chunk.Item1 * ChunkSize * TileSize;
                int worldY = chunk.Item2 * ChunkSize * TileSize;

                //convert to screen place
                double screenX = worldX - _cameraCenter.X + _screenWidth / 2;
                double screenY = worldY - _cameraCenter.Y + _screenHeight / 2;

               //draw chunk bitmap
               drawingContext.DrawImage(bitmap, new Rect(
                   screenX,
                   screenY,
                   ChunkSize * TileSize,
                   ChunkSize * TileSize

                ));
            }
        }

        public void UpdateCamera(Point cameraCenter)
        {
            _cameraCenter = cameraCenter;
            InvalidateVisual();
        }

        public void ClearAll()
        {
            _activeChunks.Clear();
            _currentChunks.Clear();
            InvalidateVisual();
        }
    }
}
