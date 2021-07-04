using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class UIQuickMenu : Window
    {
        public UIQuickMenu()
        {
            this.AutoSize = true;
            this.Closable = true;
            this.Movable = true;
            this.Color = Color.Black;
        }
      
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
            }
        }
    }
}
