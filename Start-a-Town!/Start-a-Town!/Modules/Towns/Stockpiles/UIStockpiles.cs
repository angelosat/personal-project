using Start_a_Town_.UI;

namespace Start_a_Town_.Towns.Stockpiles
{
    class UIStockpiles : Window
    {
        IconButton BtnDesignate;

        public UIStockpiles()
        {
            this.Title = "Stockpiles";
            this.Movable = true;
            this.AutoSize = true;
            this.BtnDesignate = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Designate stockpile",
                LeftClickAction = () => ToolManager.SetTool(new ToolDesignateZone(Engine.Map.GetTown(), typeof(Stockpile)))

            };
            this.Client.Controls.Add(this.BtnDesignate);
        }
       
    }
}
