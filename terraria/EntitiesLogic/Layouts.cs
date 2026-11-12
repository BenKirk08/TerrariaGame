using System;

namespace WPFTerraria
{
    public class Layouts
    {
        //say whats connected -> Full, Null or L/U/R/D (left, up, right, down
-> in that order) -> Each TileLayout needs 1 of these
        protected string Full_Connect = "Full_Connect";

        protected string URD_Connect = "URD_Connect";
        protected string LRD_Connect = "LRD_Connect";
        protected string LUD_Connect = "LUD_Connect";
        protected string LUR_Connect = "LUR_Connect";

        protected string LU_Connect = "LU_Connect";
        protected string LR_Connect = "LR_Connect";
        protected string LD_Connect = "LD_Connect";
        protected string UR_Connect = "UR_Connect";
        protected string UD_Connect = "UD_Connect";
        protected string RD_Connect = "RD_Connect";

        protected string L_Connect = "L_Connect";
        protected string U_Connect = "U_Connect";
        protected string R_Connect = "R_Connect";
        protected string D_Connect = "D_Connect";

        protected string Null_Connect = "Null_Connect";

        protected string NotUseful = null;

       public Layouts()
       {

       }

        protected void SetNullValues(string[,] givenArray) //set all values to
null in array
        {
            for (int row = 0; row < givenArray.GetLength(0); row++)
            {
                for (int col = 0; col < givenArray.GetLength(1); col++)
                {
                    givenArray[row, col] = NotUseful;
                }
            }
        }
        protected void GiveValues(string[,] givenArray, Dictionary<(int row,
int col), string> givenDictionary) //set string values into array of coords
that are in dictionary


       {
           foreach (var thisLine in givenDictionary)
           {
               var (row, col) = thisLine.Key;
               givenArray[row, col] = thisLine.Value;
           }
        }
    }
    public class NPCLayout : Layouts
    {
        //NOTE!! when creating 2D array, you say the amount of row/col you
want then when setting values, you start from 0 -> Array[25] has 25 values 0
to 24
        string[,] NPC_Array = new string[26, 1]; //26 photos in each image and
only 1 col
        public NPCLayout()
        {
            var NPC_Pictures = new Dictionary<(int row, int col), string>
            {
                { (0, 0) , "Still" },

                { (1, 0) , "Walk_1" },
                { (2, 0) , "Walk_2" },
                { (3, 0) , "Walk_3" },
                { (4, 0) , "Walk_4" },
                { (5, 0) , "Walk_5" },
                { (6, 0) , "Walk_6" }
           };

           SetNullValues(NPC_Array);
           GiveValues(NPC_Array, NPC_Pictures);

       }
       internal string[,] GetNPCArray()
       {
           return NPC_Array;
       }
    }
    public class BlockLayout : Layouts
    {
        string[,] BlockArray = new string[15, 16]; //15 rows by 16 cols of
blocks
        public BlockLayout()
        {
            //set important blocks in dictionary
            var blockConnections = new Dictionary<(int row, int col), string>
            {
                { (1, 1), Full_Connect },

                { (1, 0), URD_Connect },
                { (0, 1), LRD_Connect },
                { (1, 4), LUD_Connect },
                { (2, 1), LUR_Connect },

                { (4, 5), LU_Connect },
                { (4, 6), LR_Connect },
                { (3, 5), LD_Connect },
                { (4, 4), UR_Connect },
                { (2, 5), UD_Connect },
                { (3, 4), RD_Connect },

                { (0, 12), L_Connect },
                { (3, 6), U_Connect },
                { (2, 9), R_Connect },
                { (0, 8), D_Connect },

                { (3, 11), Null_Connect },
           };

           SetNullValues(BlockArray);
           GiveValues(BlockArray, blockConnections);
        }
        internal string[,] GetBlockArray()
        {
            return BlockArray;
        }
    }

}
