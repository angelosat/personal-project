using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes.StaticMaps;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class PanelMapSizes : PanelLabeled
    {
        public StaticMap.MapSize SelectedSize = StaticMap.MapSize.Default;
        List<RadioButton> Buttons = new List<RadioButton>();
        //PanelMapSizes(string label)
        //    : base(label)
        //{

        //}
        public PanelMapSizes()
            : base("Size")
        {
            var list = StaticMap.MapSize.GetList();
            //var defSize = StaticMap.MapSize.Default;
            foreach(var size in list)
            {
                var rad = new RadioButton(size.Name, size == SelectedSize) { Location = this.Controls.BottomLeft };//, ValueChangedFunction = obj => this.SelectedSize = size };
                rad.Tag = size;
                rad.ValueChangedFunction = obj => { if (rad.Checked) this.SelectedSize = rad.Tag as StaticMap.MapSize; };
                this.Controls.Add(rad);
                this.Buttons.Add(rad);
            }
        }
    }
}
