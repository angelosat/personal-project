using Start_a_Town_.UI;

namespace Start_a_Town_.Towns.Farming
{
    class FarmingManagerUI : GroupBox
    {
        IconButton BtnDesignate;
        Panel PanelList;

        public FarmingManagerUI(FarmingManager manager)
        {
            this.PanelList = new Panel() { AutoSize = true };

            this.BtnDesignate = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Designate farmland\n\nLeft click & drag: Add farmland\nCtrl+Left click: Remove farmland",
                LeftClickAction = () => ZoneNew.Edit(typeof(GrowingZone)),
                Location = this.PanelList.TopRight
            };
            this.Controls.Add(this.PanelList, this.BtnDesignate);
        }
    }
}
