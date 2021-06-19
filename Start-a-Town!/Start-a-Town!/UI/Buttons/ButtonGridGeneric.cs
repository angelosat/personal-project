using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class ButtonGridGeneric<TagType, ButtonType> : ScrollableBoxNew
        where TagType : class
        where ButtonType : ButtonBase, new()
    {
        public ButtonGridGeneric(int visibleHor, int visibleVer, Rectangle bounds, IEnumerable<TagType> tags, Action<ButtonType, TagType> btnInit)
            : this(bounds, tags, visibleHor, btnInit)
        { }
        public ButtonGridGeneric(Rectangle bounds, IEnumerable<TagType> tags, int lineMax, Action<ButtonType, TagType> btnInit)
            : base(bounds)
        {
            var lastx = 0;
            var lasty = 0;

            foreach (var tag in tags)
            {
                //var btn = new IconButton()
                //{
                //    BackgroundTexture = UIManager.DefaultIconButtonSprite, //UIManager.SampleButton,// 
                //    //Icon = tag.GetIcon(),
                //    //HoverText = tag.GetName()
                //};
                
                var btn = new ButtonType();
                
                btnInit(btn, tag);
                btn.Location = new Vector2(lastx, lasty);
                lastx += btn.Width;
                this.Add(btn);
                if (this.Client.Controls.Count % lineMax == 0)
                {
                    lastx = 0;
                    lasty += btn.Height;
                }
            }
        }

        public ButtonGridGeneric(int visibleHor, int visibleVer)
            : this(new Rectangle(0, 0, UIManager.DefaultIconButtonSprite.Width * visibleHor, UIManager.DefaultIconButtonSprite.Height * visibleVer), new List<TagType>() { }, visibleHor, (a, b) => { })
        {
        }
    }

   


}
