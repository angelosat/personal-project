using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using System.Linq;

namespace Start_a_Town_
{
    class InventoryUI : GroupBox
    {
        static InventoryUI Instance;
        EquipmentUI EquipmentSlots;
        static readonly int LineMax = 4;
        Panel PanelSlots;
        public InventoryUI()
        {
            this.PanelSlots = new Panel() { AutoSize = true };
            EquipmentSlots = new EquipmentUI();
        }
        public InventoryUI(Actor actor)
        {
            this.PanelSlots = new Panel() { AutoSize = true };
            //this.InitInvSlots(actor);
            this.InitInvList(actor);
        }
        public void Refresh(Actor actor)
        {
            //this.InitInvSlots(actor);
            this.InitInvList(actor);
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

            var uicolors = new Window(string.Format("Edit {0}", actor.Name), customizationClient) { Movable = true, Closable = true };

            var boxbtns = new GroupBox();
            var btncolors = new Button("Change colors",() => uicolors.SetLocation(UIManager.Mouse).Toggle(), 128);
            var btnprefs = new Button("Item Preferences", () => actor.ItemPreferences.Gui.Toggle(), 128);
            boxbtns.AddControlsVertically(btncolors, btnprefs);
            this.AddControlsBottomLeft(boxbtns);
        }
        private void InitInvList(Actor actor)
        {
            var inv = actor.Inventory;

            Controls.Remove(this.PanelSlots);

            this.PanelSlots.Controls.Clear();
            this.PanelSlots.Controls.Add(inv.Slots.Gui);

            this.AddControls(this.PanelSlots);

            var customizationClient = new GroupBox();
            var colorsui = new GuiCharacterCustomization();
            colorsui.SetTag(actor);
            customizationClient.AddControls(colorsui);
            customizationClient.AddControlsBottomLeft(new Button("Apply", customizationClient.Width) { LeftClickAction = () => PacketEditAppearance.Send(actor, colorsui.Colors) });

            var uicolors = new Window($"Edit {actor.Name}", customizationClient) { Movable = true, Closable = true };

            var boxbtns = new GroupBox();
            var btncolors = new Button("Change colors", () => uicolors.SetLocation(UIManager.Mouse).Toggle(), 128);
            var btnprefs = new Button("Item Preferences", () => actor.ItemPreferences.Gui.Toggle(), 128);
            boxbtns.AddControlsVertically(btncolors, btnprefs);
            this.AddControlsBottomLeft(boxbtns);
        }

        static public Control GetGui(Actor actor)
        {
            Window window;
            if (Instance == null)
            {
                Instance = new InventoryUI();
                window = new Window(Instance) { Closable = true, Movable = true };
                window.SnapToMouse();
            }
            else
                window = Instance.GetWindow();
            Instance.Tag = actor;
            window.Title = string.Format("{0} inventory", actor.Name);
            //Instance.InitInvSlots(actor);
            Instance.InitInvList(actor);
            Instance.RemoveControls(Instance.EquipmentSlots);
            Instance.EquipmentSlots.Refresh(actor);
            Instance.EquipmentSlots.Location = Instance.PanelSlots.TopRight;
            Instance.AddControls(Instance.EquipmentSlots);

            window.Validate(true);
            return window;
        }
        static InventoryUI GUI;
        static public Control GetGUI()
        {
            if(GUI == null)
            {
                GUI = new InventoryUI() { Name = "Inventory" };
                GUI.SetGetDataAction(o =>
                {
                    var actor = o as Actor;
                    GUI.InitInvSlots(actor);
                    GUI.RemoveControls(GUI.EquipmentSlots);
                    GUI.EquipmentSlots.Refresh(actor);
                    GUI.EquipmentSlots.Location = GUI.PanelSlots.TopRight;
                    GUI.AddControls(GUI.EquipmentSlots);
                });
            }
            return GUI;
        }
        
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            var actor = target.Object as Actor;
            if (actor != null && this.Tag != actor)
                GetGui(actor);
        }
       
    }
}
