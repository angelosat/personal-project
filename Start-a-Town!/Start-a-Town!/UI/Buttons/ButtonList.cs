using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class ButtonList<T> : ScrollableBoxNew where T : class
    {
        public ButtonList(IEnumerable<T> list, int w, int h, Func<T, string> labelGetter, Action<T, Button> btnInit)//, Action<T> action)
            : base(new Rectangle(0, 0, w, h))
        {
            var i = 0;
            foreach (var item in list)
            {
                var btn = new Button(labelGetter(item), w);
                btnInit(item, btn);
                //btn.LeftClickAction = () => action(item);
                btn.Location = new Vector2(0, i);
                i += btn.Height;
                this.Add(btn);
            }
        }
        public ButtonList(IEnumerable<T> list, int w, Func<T, string> labelGetter, Action<T, Button> btnInit)//, Action<T> action)
            : base(new Rectangle(0, 0, w, list.Count() * Button.DefaultHeight))
        {
            var i = 0;
            foreach (var item in list)
            {
                var btn = new Button(labelGetter(item), w);
                btnInit(item, btn);
                //btn.LeftClickAction = () => action(item);
                btn.Location = new Vector2(0, i);
                i += btn.Height;
                this.Add(btn);
            }
        }
    }
}
