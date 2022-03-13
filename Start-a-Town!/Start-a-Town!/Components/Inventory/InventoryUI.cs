using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class InventoryUI : GroupBox
    {
        static InventoryUI Instance;
        ScrollableBoxNewNew PanelSlots;
        Actor Actor => this.Tag as Actor;
        GuiCharacterCustomization colorsui;

        public InventoryUI()
        {
            this.InitInvList();
        }
      
        public void Refresh(Actor actor)
        {
            this.Tag = actor;
            this.PanelSlots.ClearControls();
            this.PanelSlots.AddControls(actor.Inventory.Contents.Gui);
            colorsui.SetTag(actor);
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
            window.Title = $"{actor.Name} inventory";
            Instance.Refresh(actor);
            
            window.Validate(true);
            return window;
        }
      
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            if (target.Object is Actor actor && this.Tag != actor)
                GetGui(actor);
        }
    }
}
