using System;
using System.Windows.Media.Imaging;
using System.IO;

namespace WPFTerraria
{
    public abstract class Item //forces you to use specific items -> cant js
initilise new Item()
    {
        //for GetPictureImagePath
        protected string DataBaseFileName = "ItemsDB.db";
        protected string PicturesTable = "Items";
        protected string PNGsHandle = "Item";

        public string Name { get; protected set; }
        public BitmapSource Image { get; protected set; }
        public bool IsPlaceable { get; protected set; }
        public int MaxStackSize { get; protected set; }
        public string ItemType { get; protected set; }

        public int ToolPower { get; protected set; }
        public int MiningLevel { get; protected set; }
        public int Damage { get; protected set; }
        public int HealingAmount;

        protected Item()
        {
            //default values
            IsPlaceable = false;
            MaxStackSize = 9999;
        }
        protected void SetItemImage()
        {
            Image =
Utilities.LoadImage(Utilities.GetPictureImagePath(DataBaseFileName,
PicturesTable, PNGsHandle, Name));
        }
        //polymorphism
        public virtual byte CreateTile() { return (0); } //default -> no tile
    }

   //blocks
   public class DirtBlockItem : Item
   {
       public DirtBlockItem()
       {
           Name = "Dirt Block";

           ItemType = "Block";
           IsPlaceable = true;


            SetItemImage();
        }
        public override byte CreateTile() { return (1); }// 1 -> tileID for
dirt block

   }
   public class StoneBlockItem : Item
   {
       public StoneBlockItem()
       {
           Name = "Stone Block";

           ItemType = "Block";
           IsPlaceable = true;

           SetItemImage();
       }
       public override byte CreateTile() { return (2); }

   }
   public class WoodBlockItem : Item
   {
       public WoodBlockItem()
       {
           Name = "Wood";
           ItemType = "Block";
           IsPlaceable = true;

           SetItemImage();
       }
       public override byte CreateTile() { return (30); }

   }
   public class ObsidianBlockItem : Item
   {
       public ObsidianBlockItem()
       {
           Name = "Obsidian";

           ItemType = "Block";
           IsPlaceable = true;

           SetItemImage();
       }
       public override byte CreateTile() { return (56); }

   }

   //decoration
   public class GoldBirdItem : Item
   {
       public GoldBirdItem()

    {
        Name = "Gold Bird";

        ItemType = "Decoration";
        MaxStackSize = 100;

        SetItemImage();
    }
}

//special items
public class LesserHealingPotionItem : Item
{

    public LesserHealingPotionItem()
    {
        Name = "Lesser Healing Potion";
        ItemType = "Healing";
        MaxStackSize = 99;
        HealingAmount = 20;

        SetItemImage();
    }
}

public class HealingPotionItem : Item
{

    public HealingPotionItem()
    {
        Name = "Healing Potion";
        ItemType = "Healing";
        MaxStackSize = 99;
        HealingAmount = 40;

        SetItemImage();
    }
}

//materials
public class GelItem : Item
{
    public GelItem()
    {
        Name = "Gel";
        ItemType = "Material";

        SetItemImage();
    }
}
public class LensItem : Item
{
    public LensItem()

    {
        Name = "Lens";
        ItemType = "Material";

        SetItemImage();
    }
}

//summons
public class SuspiciousLookingEyeItem : Item
{
    public SuspiciousLookingEyeItem()
    {
        Name = "Suspicious Looking Eye";
        ItemType = "Summon";

        SetItemImage();
    }
}


//crafting stations
public class WorkBenchItem : Item
{
    public WorkBenchItem()
    {
        Name = "Work Bench";

        ItemType = "Crafting Station";
        IsPlaceable = true;

        SetItemImage();
    }
    public override byte CreateTile() { return (18); }

}
public class FurnaceItem : Item
{
    public FurnaceItem()
    {
        Name = "Furnace";

        ItemType = "Crafting Station";
        IsPlaceable = true;

        SetItemImage();
    }
    public override byte CreateTile() { return (17); }

}
public class IronAnvilItem : Item
{
    public IronAnvilItem()

    {
        Name = "Iron Anvil";

        ItemType = "Crafting Station";
        IsPlaceable = true;

        SetItemImage();
    }
    public override byte CreateTile() { return (16); }

}

//ores
public class CopperOreItem : Item
{
    public CopperOreItem()
    {
        Name = "Copper Ore";

        ItemType = "Block";
        IsPlaceable = true;

        SetItemImage();
    }
    public override byte CreateTile() { return (7); }

}
public class LeadOreItem : Item
{
    public LeadOreItem()
    {
        Name = "Lead Ore";

        ItemType = "Block";
        IsPlaceable = true;

        SetItemImage();
    }
    public override byte CreateTile() { return (167); }

}
public class IronOreItem : Item
{
    public IronOreItem()
    {
        Name = "Iron Ore";

        ItemType = "Block";
        IsPlaceable = true;

        SetItemImage();
    }
    public override byte CreateTile() { return (6); }

}
public class GoldOreItem : Item
{
    public GoldOreItem()
    {
        Name = "Gold Ore";

        ItemType = "Block";
        IsPlaceable = true;

        SetItemImage();
    }
    public override byte CreateTile() { return (8); }

}
public class PlatinumOreItem : Item
{
    public PlatinumOreItem()
    {
        Name = "Platinum Ore";

        ItemType = "Block";
        IsPlaceable = true;

        SetItemImage();
    }
    public override byte CreateTile() { return (169); }

}

//Bars
public class CopperBarItem : Item
{
    public CopperBarItem()
    {
        Name = "Copper Bar";

        ItemType = "Material";
        IsPlaceable = false;

        SetItemImage();
    }
}
public class LeadBarItem : Item
{
    public LeadBarItem()
    {
        Name = "Lead Bar";

        ItemType = "Material";
        IsPlaceable = false;


        SetItemImage();
    }
}
public class IronBarItem : Item
{
    public IronBarItem()
    {
        Name = "Iron Bar";

        ItemType = "Material";
        IsPlaceable = false;

        SetItemImage();
    }
}
public class GoldBarItem : Item
{
    public GoldBarItem()
    {
        Name = "Gold Bar";

        ItemType = "Material";
        IsPlaceable = false;

        SetItemImage();
    }
}
public class PlatinumBarItem : Item
{
    public PlatinumBarItem()
    {
        Name = "Platinum Bar";

        ItemType = "Material";
        IsPlaceable = false;

        SetItemImage();
    }
}

//swords
public class CopperBroadswordItem : Item
{
    public CopperBroadswordItem()
    {
        Name = "Copper Broadsword";

        ItemType = "Tool";
        MaxStackSize = 1;
        Damage = 6;

        SetItemImage();
    }

}
public class LeadBroadswordItem : Item
{
    public LeadBroadswordItem()
    {
        Name = "Lead Broadsword";

        ItemType = "Tool";
        MaxStackSize = 1;
        Damage = 11;

        SetItemImage();
    }
}

public class IronBroadswordItem : Item
{
    public IronBroadswordItem()
    {
        Name = "Iron Broadsword";

        ItemType = "Tool";
        MaxStackSize = 1;
        Damage = 18;

        SetItemImage();
    }
}
public class GoldBroadswordItem : Item
{
    public GoldBroadswordItem()
    {
        Name = "Gold Broadsword";

        ItemType = "Tool";
        MaxStackSize = 1;
        Damage = 25;

        SetItemImage();
    }
}
public class PlatinumSwordItem : Item
{
    public PlatinumSwordItem()
    {
        Name = "Platinum Broadsword";

        ItemType = "Tool";
        MaxStackSize = 1;
        Damage = 34;

        SetItemImage();


    }
}

//axes
public class CopperAxeItem : Item
{
    public CopperAxeItem()
    {
        ToolPower = 2;
        MiningLevel = 2;
        Name = "Copper Axe";

        ItemType = "Tool";
        MaxStackSize = 1;
        Damage = 3;

        SetItemImage();
    }
}
public class LeadAxeItem : Item
{
    public LeadAxeItem()
    {
        ToolPower = 4;
        MiningLevel = 3;
        Name = "Lead Axe";

        ItemType = "Tool";
        MaxStackSize = 1;
        Damage = 4;

        SetItemImage();
    }
}
public class IronAxeItem : Item
{
    public IronAxeItem()
    {
        ToolPower = 6;
        MiningLevel = 4;
        Name = "Iron Axe";

        ItemType = "Tool";
        MaxStackSize = 1;
        Damage = 5;

        SetItemImage();

    }
}
public class GoldAxeItem : Item
{
    public GoldAxeItem()

    {
        Name = "Gold Axe";

        ToolPower = 8;
        MiningLevel = 5;
        ItemType = "Tool";
        MaxStackSize = 1;
        Damage = 6;

        SetItemImage();

    }
}
public class PlatinumAxeItem : Item
{
    public PlatinumAxeItem()
    {
        Name = "Platinum Axe";

        ToolPower = 10;
        MiningLevel = 6;
        ItemType = "Tool";
        MaxStackSize = 1;
        Damage = 8;

        SetItemImage();

    }
}

//pickaxes
public class CopperPickaxeItem : Item
{
    public CopperPickaxeItem()
    {
        Name = "Copper Pickaxe";

        ToolPower = 3;
        MiningLevel = 2;
        ItemType = "Tool";
        MaxStackSize = 1;
        Damage = 3;

        SetItemImage();
    }
}
public class LeadPickaxeItem : Item
{
    public LeadPickaxeItem()
    {
        Name = "Lead Pickaxe";

        ToolPower = 6;

       MiningLevel = 3;
       ItemType = "Tool";
       MaxStackSize = 1;
       Damage = 4;

       SetItemImage();
   }
}
public class IronPickaxeItem : Item
{
    public IronPickaxeItem()
    {
        Name = "Iron Pickaxe";

       ToolPower = 10;
       MiningLevel = 3;
       ItemType = "Tool";
       MaxStackSize = 1;
       Damage = 5;

       SetItemImage();
   }
}
public class GoldPickaxeItem : Item
{
    public GoldPickaxeItem()
    {
        Name = "Gold Pickaxe";

       ToolPower = 15;
       MiningLevel = 4;
       ItemType = "Tool";
       MaxStackSize = 1;
       Damage = 6;

       SetItemImage();
   }
}
public class PlatinumPickaxeItem : Item
{
    public PlatinumPickaxeItem()
    {
        Name = "Platinum Pickaxe";

       ToolPower = 25;
       MiningLevel = 5;
       ItemType = "Tool";
       MaxStackSize = 1;
       Damage = 7;

       SetItemImage();

   }

}

//bows
public class CopperBowItem : Item
{
    public CopperBowItem()
    {
        Name = "Copper Bow";

        ItemType = "Bow";
        MaxStackSize = 1;
        Damage = 7;

        SetItemImage();
    }
}
public class LeadBowItem : Item
{
    public LeadBowItem()
    {
        Name = "Lead Bow";

        ItemType = "Bow";
        MaxStackSize = 1;
        Damage = 10;

        SetItemImage();
    }
}
public class IronBowItem : Item
{
    public IronBowItem()
    {
        Name = "Iron Bow";

        ItemType = "Bow";
        MaxStackSize = 1;
        Damage = 14;

        SetItemImage();
    }
}
public class GoldBowItem : Item
{
    public GoldBowItem()
    {
        Name = "Gold Bow";

        ItemType = "Bow";
        MaxStackSize = 1;
        Damage = 19;

        SetItemImage();

        }
    }
    public class PlatinumBowItem : Item
    {
        public PlatinumBowItem()
        {
            Name = "Platinum Bow";

            ItemType = "Bow";
            MaxStackSize = 1;
            Damage = 25;

            SetItemImage();
        }
    }
    public class WoodenArrowItem : Item
    {
        public WoodenArrowItem()
        {
            Name = "Wooden Arrow";

            ItemType = "Arrow";
            Damage = 3;

            SetItemImage();
        }
    }
}
