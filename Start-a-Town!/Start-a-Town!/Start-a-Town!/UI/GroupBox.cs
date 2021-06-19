using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class GroupBox : Control
    {
        public GroupBox() { MouseThrough = true; AutoSize = true; }
        public GroupBox(Vector2 position) : base(position) { MouseThrough = true; AutoSize = true; }

        //public override void Update()
        //{
        //    base.Update();

        //    foreach (Control control in Controls)
        //        control.Update();
        //}
        //public override void OnHitTestPass()
        //{
        //    base.OnHitTestPass();
        //}

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            //DrawHighlight(sb);
            //this.ScreenBounds.DrawHighlight(sb, Color.White * .5f);
            base.Draw(sb, viewport);
        }
        
    }
}
