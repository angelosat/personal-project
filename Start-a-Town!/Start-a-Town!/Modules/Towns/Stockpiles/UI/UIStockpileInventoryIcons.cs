using System.Collections.Generic;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_.Towns
{
    //class UIStockpileInventoryIcons : GroupBox
    //{
    //    TableObservable<KeyValuePair<ItemDef, int>> Table;

    //    public UIStockpileInventoryIcons()
    //    {
    //        this.Table = new TableObservable<KeyValuePair<ItemDef, int>>(20) { ShowColumnLabels = false, ClientBoxColor = Color.Transparent };
    //        Table.AddColumn(null, "Item", 16, (pair) => {
    //            var def = pair.Key;
    //            var obj = def.Body;
    //            return new PictureBox(obj, .5f)
    //            {
    //                HoverText = def.Name
    //            };
    //        });
    //        Table.AddColumn(null, "Count", 20, (pair) => new Label(pair.Value.ToString()) { TextHAlign = HorizontalAlignment.Center });
    //        this.AddControls(this.Table);
    //        this.SetMousethrough(true, true);
    //    }

    //    public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Microsoft.Xna.Framework.Rectangle viewport)
    //    {
    //        base.Draw(sb, viewport);
    //    }
    //}

    [Obsolete]
    class UIStockpileInventoryIcons : GroupBox
    {
        TableScrollableCompact<KeyValuePair<ItemDef, int>> Table;

        public UIStockpileInventoryIcons()
        {
            this.Table = new TableScrollableCompact<KeyValuePair<ItemDef, int>>(20) { ShowColumnLabels = false, ClientBoxColor = Color.Transparent };
            Table.AddColumn(null, "Item", 16, (pair) =>
            {
                var def = pair.Key;
                var obj = def.Body;
                return new PictureBox(obj, .5f)
                {
                    HoverText = def.Name
                };
            }, showColumnLabels: false);
            Table.AddColumn(null, "Count", 20, (pair) => new Label(pair.Value.ToString()) { TextHAlign = HorizontalAlignment.Center }, showColumnLabels: false); ;
            this.AddControls(this.Table);
            this.SetMousethrough(true, true);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Microsoft.Xna.Framework.Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
    }
}
