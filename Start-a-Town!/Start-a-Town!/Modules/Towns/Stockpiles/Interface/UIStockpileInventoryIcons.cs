using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Towns
{
    class UIStockpileInventoryIcons : GroupBox
    {
        TableScrollableCompact<KeyValuePair<ItemDef, int>> Table;
        Panel TablePanel;

        public UIStockpileInventoryIcons()
        {
            this.TablePanel = new Panel() { AutoSize = true };
            this.Table = new TableScrollableCompact<KeyValuePair<ItemDef, int>>(20) { ShowColumnLabels = false, ClientBoxColor = Color.Transparent };// new TableScrollableCompact<GameObject>(50);
            //Table.AddColumn(null, "Item", 150, (pair) => new IconButton(GameObject.Objects[pair.Key].GetIcon()));
            Table.AddColumn(null, "Item", 16, (pair) => {
                //var obj = GameObject.Objects[pair.Key];
                var def = pair.Key;
                var obj = def.Body;
                return new PictureBox(obj, .5f)
                {
                    HoverText = def.Name
                };
            }, showColumnLabels: false);
            Table.AddColumn(null, "Count", 20, (pair) => new Label(pair.Value.ToString()) { TextHAlign = HorizontalAlignment.Center }, showColumnLabels: false); ;

            //this.TablePanel.AddControls(this.Table);
            //this.AddControls(this.TablePanel);
            this.AddControls(this.Table);
            this.SetMousethrough(true, true);
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.StockpileContentsUpdated:
                   
                    //this.Refresh(e.Parameters[0] as Dictionary<int, int>, e.Parameters[1] as Dictionary<int, int>, e.Parameters[2] as Dictionary<int, int>);
                    break;

                default:
                    break;
            }
        }

        private void Refresh(Dictionary<ItemDef, int> added, Dictionary<ItemDef, int> removed, Dictionary<ItemDef, int> updated)
        {
            this.Table.AddItems(added);
            this.Table.RemoveItems(i => removed.ContainsKey(i.Key));
            foreach (var up in updated)
            {
                var element = this.Table.GetItem(p => p.Key == up.Key, "Count") as Label;
                if (element.Text != up.Value.ToString())
                    element.Text = up.Value.ToString();
            }
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Microsoft.Xna.Framework.Rectangle viewport)
        {
            //this.BoundsScreen.DrawHighlight(sb);
            base.Draw(sb, viewport);
        }
    }
    //class UIStockpileInventoryIcons : GroupBox
    //{
    //    TableScrollableCompact<KeyValuePair<int, int>> Table;
    //    Panel TablePanel;

    //    public UIStockpileInventoryIcons()
    //    {
    //        this.TablePanel = new Panel() { AutoSize = true };
    //        this.Table = new TableScrollableCompact<KeyValuePair<int, int>>(20) { ShowColumnLabels = false, ClientBoxColor = Color.Transparent };// new TableScrollableCompact<GameObject>(50);
    //        //Table.AddColumn(null, "Item", 150, (pair) => new IconButton(GameObject.Objects[pair.Key].GetIcon()));
    //        Table.AddColumn(null, "Item", 16, (pair) =>{ 
    //            var obj = GameObject.Objects[pair.Key];
    //            return new PictureBox(obj, .5f)
    //            {
    //                HoverText = obj.Name
    //            };
    //        },showColumnLabels:false);
    //        Table.AddColumn(null, "Count", 20, (pair) => new Label(pair.Value.ToString()) { TextHAlign = HorizontalAlignment.Center }, showColumnLabels: false); ;

    //        //this.TablePanel.AddControls(this.Table);
    //        //this.AddControls(this.TablePanel);
    //        this.AddControls(this.Table);
    //        this.SetMousethrough(true, true);
    //    }
    //    internal override void OnGameEvent(GameEvent e)
    //    {
    //        switch(e.Type)
    //        {
    //            case Components.Message.Types.StockpileContentsUpdated:
    //                //Dictionary<int, int> inv = e.Parameters[0] as Dictionary<int, int>;
    //                //this.Refresh(inv);
    //                this.Refresh(e.Parameters[0] as Dictionary<int, int>, e.Parameters[1] as Dictionary<int, int>, e.Parameters[2] as Dictionary<int, int>);
    //                break;

    //            default:
    //                break;
    //        }
    //    }

    //    private void Refresh(Dictionary<int, int> added, Dictionary<int, int> removed, Dictionary<int, int> updated)
    //    {
    //        this.Table.AddItems(added);
    //        this.Table.RemoveItems(i => removed.ContainsKey(i.Key));
    //        foreach(var up in updated)
    //        {
    //            var element = this.Table.GetItem(p => p.Key == up.Key, "Count") as Label;
    //            if(element.Text != up.Value.ToString())
    //            element.Text = up.Value.ToString();
    //        }
    //    }
    //    public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Microsoft.Xna.Framework.Rectangle viewport)
    //    {
    //        //this.BoundsScreen.DrawHighlight(sb);
    //        base.Draw(sb, viewport);
    //    }
    //}
}
