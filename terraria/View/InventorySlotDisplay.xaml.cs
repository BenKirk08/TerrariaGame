using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Diagnostics;

namespace WPFTerraria.View
{
    public partial class InventorySlotDisplay : UserControl,
INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Inventory inventory = new Inventory();

        //pre-set slot images and paths
        private readonly string normalItemSlotPath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "Content",
"Images", "UI", "Bestiary", "Slot_Back.png");
        private readonly string selectedItemSlotPath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "Content",
"Images", "UI", "Bestiary", "Slot_Select.png");
        private readonly string hoverItemSlotPath =
Path.Combine(Utilities.GetSolutionDirectory(), "AllGameAssets", "Content",
"Images", "UI", "Bestiary", "Slot_Hover.png");

        public bool IsSelected => ThisInventoryItem?.IsSelected ?? false; //??
returns false if inventory item null, bound to InventoryItem.IsSelected so
updates when that updates

        //public properties
        private InventoryItem _inventoryItem; //saved inventory item in this
slot, for getter
        public InventoryItem ThisInventoryItem
        {
            get { return _inventoryItem; } //will get public values fromm
InverntoryItem -> Count, ItemImage, Name
            set
            {
                if (_inventoryItem != value) //on;y if changes
                {
                    if (_inventoryItem != null)
                    {
                        //unsubscribe from previous instance PropertyChanged
event -> before changed value
                        _inventoryItem.PropertyChanged -=
InventoryItem_PropertyChanged; //-> saves memory

                    }

                    _inventoryItem = value; //change the value

                    if (_inventoryItem != null)
                    {
                        //subscribe to new instance of PropertyChanged event
                        _inventoryItem.PropertyChanged +=
InventoryItem_PropertyChanged;

                        //update visibility -> initially
                        UpdateQuantityVisibility();
                        UpdateItemSize();
                    }

                    OnPropertyChanged(nameof(ThisInventoryItem));
                    UpdateSlotImage(); //Force slot image update
                }
            }
        }
        private int _slotPlace;
        public int SlotPlace
        {
            get { return _slotPlace; }
            set
            {
                _slotPlace = value;
                OnPropertyChanged(nameof(SlotPlace)); //change the number on
slotindex when changed
            }
        }

        private ImageSource _currentSlotImage;
        public ImageSource CurrentSlotImage
        {
            get { return _currentSlotImage; }
            set
            {
                _currentSlotImage = value;
                OnPropertyChanged(nameof(CurrentSlotImage)); //update the slot
image
            }
        }

        public static InventorySlotDisplay draggedSlot;
        public static Image draggedItemImage;
        public InventorySlotDisplay() //constructor
        {
            InitializeComponent();
            DataContext = this; //write for binding

            InitiliseSizes();


           //load normal background to begin
           CurrentSlotImage = Utilities.LoadImage(normalItemSlotPath);

            //event handlers
            MouseEnter += (s, e) => CurrentSlotImage = IsSelected ?
Utilities.LoadImage(selectedItemSlotPath) :
Utilities.LoadImage(hoverItemSlotPath); //when mouse hovers over change the
image only if not already selected
            MouseLeave += (s, e) => UpdateSlotImage(); //when leaves returns
to what it is

            this.MouseDown += OnSlotClicked;

            //mouse events for dragging
            //this.MouseMove += OnMouseMove;
            this.MouseDown += OnMouseDown;
            this.MouseUp += OnMouseUp;
       }


        //FOR DRAGGING
        private Stopwatch dragStopwatch = new Stopwatch();
        private const int HoldTimeDragging = 200; //0.2s wait
        private bool isDragging = false;
        private bool isMouseHeld = false;
        private async void OnMouseDown(object sender, MouseButtonEventArgs e)
//async as uses task
        {
            var main = Application.Current.MainWindow as MainWindow;
            if (main == null || !main.isInventoryOpen) return; //ensure full
inventory open

           isMouseHeld = true;
           isDragging = false;
           dragStopwatch.Restart(); //start timing

           //immediatly select the item
           for (int i = 0; i < main.myInventory.PlayersItems.Count; i++)
           {
               main.myInventory.PlayersItems[i].IsSelected = false;
           }
           this.ThisInventoryItem.IsSelected = true; //set dragged item as
selected

            await Task.Delay(HoldTimeDragging);

           if (isMouseHeld) //if still holding after 0.2s start dragging
           {
               isDragging = true;
               StartDragging(e);
           }
       }


          private void StartDragging(MouseButtonEventArgs e)
          {
              var main = Application.Current.MainWindow as MainWindow;
              if (main == null || !main.isInventoryOpen) return;

            if (ThisInventoryItem != null && ThisInventoryItem.Item != null)
//if items in the slot
            {
                draggedSlot = this; //this InventorySlotDisplay is what ur
dragging

                  //create a replica image of the dragged item
                  draggedItemImage = new Image
                  {
                      Source = ThisInventoryItem.ItemImage,
                      Width = ItemImageDisplay.Width,
                      Height = ItemImageDisplay.Height,
                      Opacity = 0.75, //makes a bit transparent
                      IsHitTestVisible = false //mouse events go through
                  };
                  Canvas.SetZIndex(draggedItemImage, 15);//set ontop of
everything

                //set point initially
                Point position = e.GetPosition(main);
                Canvas.SetLeft(draggedItemImage, position.X -
draggedItemImage.Width / 2);
                Canvas.SetTop(draggedItemImage, position.Y -
draggedItemImage.Height / 2);

                  main.FullInventoryCanvas.Children.Add(draggedItemImage); //add
dragged item UI

                  main.MouseMove += OnMouseMove;
                  main.MouseUp += OnMouseUp;

                  e.Handled = true;
              }
          }
          private void OnMouseUp(object sender, MouseButtonEventArgs e)
          {
              isMouseHeld = false; //mouse released
              dragStopwatch.Stop(); //stop stopwatch

              var main = Application.Current.MainWindow as MainWindow;
              if (main == null || !main.isInventoryOpen || draggedSlot == null)
return;

              if (!isDragging) //if mouse was released before time cancel
dragging
             {
                  return;

             }

            //find InventorySlotDisplay under the mouse -> goes through the
children in fullinvenotrycanvas
            InventorySlotDisplay targetSlot = null;
            Point dropPosition = e.GetPosition(main.FullInventoryCanvas);
            foreach (var child in
main.myFullInventoryDisplay.FullInventory.Children)
            {
                if (child is InventorySlotDisplay slot &&
VisualTreeHelper.HitTest(slot, e.GetPosition(slot)) != null)
                {
                    targetSlot = slot;
                    break;
                }
            }

            //if dropping on a valid slot ie not outside or in same slot
            if (targetSlot != null && targetSlot != draggedSlot)
            {
                //gets indexes of where dropping and dropped from
                int draggedIndex =
main.myInventory.PlayersItems.IndexOf(draggedSlot.ThisInventoryItem);
                int targetIndex =
main.myInventory.PlayersItems.IndexOf(targetSlot.ThisInventoryItem);

                 if (draggedIndex >= 0 && targetIndex >= 0) //if valid(not == -
1)
                 {
                    InventoryItem draggedItem =
main.myInventory.PlayersItems[draggedIndex];
                    InventoryItem targetItem =
main.myInventory.PlayersItems[targetIndex];

                     //if same item combine quantities
                     if (targetItem?.Item != null && draggedItem.Item.GetType()
== targetItem?.Item.GetType())
                    {
                         int maxStack = targetItem.Item.MaxStackSize;
                         int totalQuantity = draggedItem.Quantity +
targetItem.Quantity;

                        //target slot gets as much as possible -> yp to
maxstacksize
                        targetItem.Quantity = Math.Min(totalQuantity,
maxStack);

                        //leftover stays in dragged slot
                        draggedItem.Quantity = totalQuantity -
targetItem.Quantity;

                        //if dragged slot is empty clear it -> should fix
anyway, but js to be sure

                        if (draggedItem.Quantity == 0)
                        {
                            main.myInventory.PlayersItems[draggedIndex] = new
InventoryItem(main.myInventory);
                        }
                    }
                    else
                    {
                        //if different items, swap as normal
                        var temp =
main.myInventory.PlayersItems[draggedIndex];
                        main.myInventory.PlayersItems[draggedIndex] =
main.myInventory.PlayersItems[targetIndex];
                        main.myInventory.PlayersItems[targetIndex] = temp;
                    }

                   //update selection
                   main.myInventory.SelectedInventoryIndex = targetIndex;
               }
           }

           //cleanup
           main.FullInventoryCanvas.Children.Remove(draggedItemImage);
           draggedSlot = null;
           draggedItemImage = null;
           main.MouseMove -= OnMouseMove;
           main.MouseUp -= OnMouseUp;
       }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (draggedSlot != null && draggedItemImage != null) //only if
your dragging an item
            {
                var main = Application.Current.MainWindow as MainWindow;
                if (main == null) return;

                //get position relative to FullInventoryCANVAS
                var position = e.GetPosition(main.FullInventoryCanvas);

                //dragged image stays inside FullInventoryCanvas boundaries
                double clampedX = Math.Clamp(position.X, 0,
main.FullInventoryCanvas.ActualWidth);
                double clampedY = Math.Clamp(position.Y, 0,
main.FullInventoryCanvas.ActualHeight);

                //set dragged item position
                Canvas.SetLeft(draggedItemImage, clampedX -
(draggedItemImage.Width / 2));
                Canvas.SetTop(draggedItemImage, clampedY -
(draggedItemImage.Height / 2));
            }
        }

        //subscriptions
        private void InventoryItem_PropertyChanged(object sender,
PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ThisInventoryItem.Quantity)) //if
the quantity property changes update text visibility.
            {
                UpdateQuantityVisibility();
            }
            if (e.PropertyName == nameof(ThisInventoryItem.IsSelected))
//define whether items selected
            {
                UpdateSlotImage(); //updates when selection changes
            }
            if (e.PropertyName == nameof(InventoryItem.Item.ItemType))
            {
                UpdateItemSize();
            }
        }
        private void OnSlotClicked(object sender, MouseButtonEventArgs e)
        {
            var main = Application.Current.MainWindow as MainWindow; //uses
mainwindow to access myInventory
            if (main == null)
            {
                Console.WriteLine("MainWindow not found");
                return;
            }

           if (main.myInventory == null)
           {
               Console.WriteLine("myInventory is null");
               return;
           }

            //find the clicked items index in the inventory and updates the
selected slot triggering UI changes
            //changes the selectedinventory index, which changes the
inventoryitem, which triggers is selected -> changes value in other places
aswell as here, is sent back to here in inventoryitem.cs

            //changing index
            int clickedIndex =
main.myInventory.PlayersItems.IndexOf(ThisInventoryItem);
            // If clicking the already selected slot, deselect it
            if (main.myInventory.SelectedInventoryIndex == clickedIndex) //if
already selected
            {
                main.myInventory.SelectedInventoryIndex = -1; //deselect
            }

           else
           {
               main.myInventory.SelectedInventoryIndex = clickedIndex;
//select
           }
       }

        //subroutines
        private void UpdateSlotImage()
        {
            CurrentSlotImage = IsSelected
                ? Utilities.LoadImage(selectedItemSlotPath) //if is selected
is true then selectimage else false and is normal
                : Utilities.LoadImage(normalItemSlotPath);
        }
        private void UpdateQuantityVisibility()
        {
            if (ThisInventoryItem != null && ThisInventoryItem.Quantity > 1)
            {
                QuantityTextDisplay.Visibility = Visibility.Visible;
            }
            else
            {
                QuantityTextDisplay.Visibility = Visibility.Collapsed; //if ==
0 or 1 no text
            }
        }
        private void UpdateItemSize()
        {
            if (ThisInventoryItem?.Item?.ItemType == "Block" ||
ThisInventoryItem?.Item?.ItemType == "Material") //if its a block or material
            {
                ItemImageDisplay.Width = inventory.SlotImageSize * 0.4;
//nearly quarter total normal size
                ItemImageDisplay.Height = inventory.SlotImageSize * 0.4;
            }
            else if (ThisInventoryItem?.Item?.ItemType == "Decoration" ||
ThisInventoryItem?.Item?.ItemType == "Arrow")
            {
                ItemImageDisplay.Width = inventory.SlotImageSize * 0.55; //in
between block and everything else if only decoration
                ItemImageDisplay.Height = inventory.SlotImageSize * 0.55;
            }
            else
            {
                ItemImageDisplay.Width = inventory.SlotImageSize * 0.7;
//normal sizes
                ItemImageDisplay.Height = inventory.SlotImageSize * 0.7;
            }
        }


        //initial

       private void InitiliseSizes()
       {
           this.Width = inventory.SlotImageSize;
           this.Height = inventory.SlotImageSize;

           SlotDisplayImage.Width = inventory.SlotImageSize;
           SlotDisplayImage.Height = inventory.SlotImageSize;

           ItemImageDisplay.Width = inventory.SlotImageSize * 0.7;
           ItemImageDisplay.Height = inventory.SlotImageSize * 0.7;

           QuantityTextDisplay.FontSize = inventory.SlotImageSize / 3.75;
           SlotPlaceText.FontSize = inventory.SlotImageSize / 5;
       }

        private void OnPropertyChanged(string propertyName) //to simplify
        {
            PropertyChanged?.Invoke(this, new
PropertyChangedEventArgs(propertyName));
        }
    }
}
