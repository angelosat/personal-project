using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Towns;

namespace Start_a_Town_
{
    class UIQuickMenu : Window
    {
        //Panel Panel;
        //private Town town;
        //public UIQuickMenu()
        //{
        //    this.Panel = new Panel() { AutoSize = true };
        //}
        public UIQuickMenu()
        {
            this.AutoSize = true;
            this.Closable = true;
            this.Movable = true;
            this.Color = Color.Black;
            //this.BackgroundStyle = BackgroundStyle.Tooltip;
        }
        //public UIQuickMenu(Town town)
        //{
        //    var actions = new List<Tuple<string, Action>>();
        //    foreach (var comp in town.TownComponents)
        //        actions.AddRange(comp.OnQuickMenuCreated());
        //    this.AddItems(actions);
        //}

        internal void AddItems(IEnumerable<Tuple<string, Action>> actions)
        {
            var w = Button.GetMaxWidth(actions.Select(i => i.Item1));
            var row = 0;
            var x = 0;
            var y = 0;
            var spacing = 1;
            var maxrows = Math.Ceiling(actions.Count() / 2f);
            foreach (var item in actions)
            {
                var button = new Button(item.Item1, w) { Location = new Vector2(x,y), LeftClickAction = () => { item.Item2(); this.Hide(); } };
                this.Client.AddControls(button);

                row++;
                if (row == maxrows)
                {
                    x += button.Width + spacing;
                    y = 0;
                    row = 0;
                }
                else
                    y += button.Height + spacing;
                //this.Client.AddControlsBottomLeft(new Button(item.Item1, w) { LeftClickAction = () => { item.Item2(); this.Hide(); } });
            }
        }
        //public override bool Show()
        //{
        //    this.Location = UIManager.Mouse;
        //    return base.Show();
        //}
    }

    //class UIQuickMenuOld : GroupBox
    //{
    //    Panel Panel;
    //    //private Town town;
    //    //ListBoxNew<Tuple<string, Action>, Button> Buttons;
    //    public UIQuickMenuOld()
    //    {
    //        this.Panel = new Panel() { AutoSize = true };
    //        //this.Buttons = new ListBoxNew<Tuple<string, Action>, Button>();
    //        //var l = new ListView
    //    }

    //    public UIQuickMenuOld(Town town)
    //    {
    //        var actions = new List<Tuple<string, Action>>();
    //        foreach (var comp in town.TownComponents)
    //            actions.AddRange(comp.OnQuickMenuCreated());
    //        this.AddItems(actions);
    //    }
    //    //internal void AddItem(Tuple<string, Action> action)
    //    //{
    //    //    this.AddItems(new Tuple<string, Action>[] { action });
    //    //}
    //    internal void AddItems(IEnumerable<Tuple<string, Action>> actions)
    //    {
    //        //this.Buttons.Build(actions, (item) => item.Item1, (item, btn) => btn.LeftClickAction = () => { item.Item2(); this.Hide(); });
    //        //var w = actions.Select(i => i.Item1).GetMaxWidth() + 20;
    //        var w = Button.GetMaxWidth(actions.Select(i => i.Item1));
    //        foreach (var item in actions)
    //        {
    //            this.Panel.AddControlsBottomLeft(new Button(item.Item1, w) { LeftClickAction = () => { item.Item2(); this.Hide(); } });
    //        }
    //        //this.Panel.AddControls(this.Buttons);
    //        this.AddControls(this.Panel);
    //    }
    //    public override bool Show()
    //    {
    //        this.Location = UIManager.Mouse;
    //        return base.Show();
    //    }
    //}

        
}
