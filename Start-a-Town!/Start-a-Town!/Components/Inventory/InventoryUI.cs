using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using System.Linq;
using System;

namespace Start_a_Town_
{
    class InventoryUI : GroupBox
    {
        static InventoryUI Instance;
        EquipmentUI EquipmentSlots;
        static readonly int LineMax = 4;
        ScrollableBoxNewNew PanelSlots;
        Actor Actor => this.Tag as Actor;
        GuiCharacterCustomization colorsui;
        static InventoryUI Gui;

        public InventoryUI()
        {
            EquipmentSlots = new EquipmentUI();
            this.InitInvList();
        }
        [Obsolete]
        public InventoryUI(Actor actor)
        {
            throw new System.Exception();
            this.InitInvList();
        }
        public void Refresh(Actor actor)
        {
            this.Tag = actor;
            this.PanelSlots.ClearControls();
            this.PanelSlots.AddControls(actor.Inventory.Slots.Gui);
            colorsui.SetTag(actor);
        }
        private void InitInvSlots(Actor actor)
        {
            var inventory = actor.GetComponent<PersonalInventoryComponent>();

            var container = inventory.Slots;

            Controls.Remove(this.PanelSlots);
            this.PanelSlots.Controls.Clear();
            for (int i = 0; i < inventory.Capacity; i++)
            {
                //var invSlot = container.Slots[i];
                var invSlot = new GameObjectSlot(container[i]);
                int slotid = i; // must be here for correct variable capturing with anonymous method
                var slot = new InventorySlot(invSlot, actor, i)
                {
                    Location = new Vector2((i % LineMax) * UIManager.SlotSprite.Width, (i / LineMax) * UIManager.SlotSprite.Height),
                    DragDropAction = args =>
                    {
                        "npc inventory rearranging disabled".ToConsole();
                        return DragDropEffects.None;
                    },
                    LeftClickAction = () => SelectionManager.Select(invSlot.Object)
                };
                
                
                slot.RightClickAction = () =>
                {
                    if (invSlot.Object != null)
                    {
                        var contextActions = invSlot.Object.GetInventoryContextActions(actor);
                        ContextMenuManager.PopUp(contextActions.ToArray());
                    }
                };
                

                this.PanelSlots.Controls.Add(slot);
            }
            this.AddControls(this.PanelSlots);

            var customizationClient = new GroupBox();
            var colorsui = new GuiCharacterCustomization();
            colorsui.SetTag(actor);
            customizationClient.AddControls(colorsui);
            customizationClient.AddControlsBottomLeft(new Button("Apply", customizationClient.Width) { LeftClickAction = () => PacketEditAppearance.Send(actor, colorsui.Colors) });

            var uicolors = new Window($"Edit {actor.Name}", customizationClient) { Movable = true, Closable = true };

            var boxbtns = new GroupBox();
            var btncolors = new Button("Change colors",() => uicolors.SetLocation(UIManager.Mouse).Toggle(), 128);
            var btnprefs = new Button("Item Preferences", () => actor.ItemPreferences.Gui.Toggle(), 128);
            boxbtns.AddControlsVertically(btncolors, btnprefs);
            this.AddControlsBottomLeft(boxbtns);
        }
        private void InitInvList()
        {
            this.PanelSlots = new ScrollableBoxNewNew(256, 256);
            var customizationClient = new GroupBox();
            colorsui = new GuiCharacterCustomization();

            customizationClient.AddControls(colorsui);
            customizationClient.AddControlsBottomLeft(new Button("Apply", () => PacketEditAppearance.Send(this.Actor, colorsui.Colors), customizationClient.Width));

            var uicolors = new Window($"Edit colors", customizationClient) { Movable = true, Closable = true }; //Edit {this.Actor.Name}

            var boxbtns = new GroupBox();
            var btncolors = new Button("Change colors", () => uicolors.SetLocation(UIManager.Mouse).Toggle(), 128);
            var btnprefs = new Button("Item Preferences", () => this.Actor.ItemPreferences.Gui.Toggle(), 128);
            boxbtns.AddControlsVertically(btncolors, btnprefs);
            this.AddControlsVertically(
                this.PanelSlots.ToPanel(),
                boxbtns);
        }
        static public Control GetGui(Actor actor)
        {
            Window window;
            if (Instance is null)
            {
                Instance = new InventoryUI();
                window = new Window(Instance) { Closable = true, Movable = true };
                window.SnapToMouse();
            }
            else
                window = Instance.GetWindow();
            Instance.Tag = actor;
            window.Title = string.Format("{0} inventory", actor.Name);
            Instance.Refresh(actor);
            //Instance.RemoveControls(Instance.EquipmentSlots);
            //Instance.EquipmentSlots.Refresh(actor);
            //Instance.EquipmentSlots.Location = Instance.PanelSlots.TopRight;
            //Instance.AddControls(Instance.EquipmentSlots);

            window.Validate(true);
            return window;
        }
        static public Control GetGUI()
        {
            if(Gui == null)
            {
                Gui = new InventoryUI() { Name = "Inventory" };
                Gui.SetGetDataAction(o =>
                {
                    var actor = o as Actor;
                    Gui.InitInvSlots(actor);
                    Gui.RemoveControls(Gui.EquipmentSlots);
                    Gui.EquipmentSlots.Refresh(actor);
                    Gui.EquipmentSlots.Location = Gui.PanelSlots.TopRight;
                    Gui.AddControls(Gui.EquipmentSlots);
                });
            }
            return Gui;
        }
        
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            var actor = target.Object as Actor;
            if (actor != null && this.Tag != actor)
                GetGui(actor);
        }
       
    }
}
