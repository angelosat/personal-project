using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Towns;

namespace Start_a_Town_.UI
{
    class ButtonGridGenericNew<TagType> : SelectableItemList<TagType>// GroupBox, ISelectableItemList<TagType>
        where TagType : INamed
    {
        public ButtonGridGenericNew()
        {

        }
        public ButtonGridGenericNew(IEnumerable<TagType> items, Action<TagType, Button> btnInit)
        {
            this.AddItems(items, btnInit);
        }

        internal ButtonGridGenericNew<TagType> AddItems(IEnumerable<TagType> items, Action<TagType, Button> btnInit)
        {
            //var w = Button.GetMaxWidth(items.Select(i => i.Item1));
            var w = Button.GetMaxWidth(items.Select(i => i.Name));
            var row = 0;
            var x = 0;
            var y = 0;
            var spacing = 1;
            var maxrows = Math.Ceiling(items.Count() / 2f);
            foreach (var item in items)
            {
                //var button = new Button(item.Item1, w) { Location = new Vector2(x, y), LeftClickAction = () => { item.Item2(); this.Hide(); } };
                var button = new Button(item.Name, w) { Location = new Vector2(x, y)};//, LeftClickAction = () => { item.Item2(); this.Hide(); } };
                btnInit(item, button);
                var prevAction = button.LeftClickAction;
                button.LeftClickAction = () =>
                { 
                    prevAction();
                    //Func < Action < TagType >> func = (() => { return this.SelectAction; });
                    //func()(item);  //this.SelectAction
                    this.SelectAction(item);
                };
                this.AddControls(button);

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
            return this;
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
    //    private Town town;
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
