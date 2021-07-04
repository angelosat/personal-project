using Start_a_Town_.UI;

namespace Start_a_Town_.Towns.Forestry
{
    class ChoppingManagerUI : GroupBox
    {
        ListBox<Grove, Label> ListGroves;
        ChoppingManager Manager;
        IconButton BtnDesignate;
        public ChoppingManagerUI(ChoppingManager manager)
        {
            this.Manager = manager;
            var panelgroves = new Panel() { AutoSize = true };
            this.ListGroves = new ListBox<Grove, Label>(80, 200);
            Refresh();
            panelgroves.AddControls(this.ListGroves);

            var panelbuttons = new Panel { Location = panelgroves.TopRight, AutoSize = true };
            this.BtnDesignate = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Designate chopping\n\nLeft click & drag: Add chopping\nCtrl+Left click: Remove chopping",
                LeftClickAction = this.Manager.EditChopping
            };
            
            panelbuttons.AddControls(this.BtnDesignate);
            this.Controls.Add(panelgroves, panelbuttons);
        }
    }
}
