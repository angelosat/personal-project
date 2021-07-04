using System.Collections.Generic;
using Start_a_Town_.UI;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class PanelMapSizes : PanelLabeled
    {
        public StaticMap.MapSize SelectedSize = StaticMap.MapSize.Default;
        List<RadioButton> Buttons = new List<RadioButton>();
        public PanelMapSizes()
            : base("Size")
        {
            var list = StaticMap.MapSize.GetList();
            foreach(var size in list)
            {
                var rad = new RadioButton(size.Name, size == SelectedSize) { Location = this.Controls.BottomLeft };
                rad.Tag = size;
                rad.ValueChangedFunction = obj => { if (rad.Checked) this.SelectedSize = rad.Tag as StaticMap.MapSize; };
                this.Controls.Add(rad);
                this.Buttons.Add(rad);
            }
        }
    }
}
