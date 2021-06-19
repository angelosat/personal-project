using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Start_a_Town_.UI
{
    class ListItem : Label//, ITooltippable
    {
        //public Object Item;
        //public int Index;

        public ListItem(Vector2 position, string text, HorizontalAlignment align = HorizontalAlignment.Left) : base(position, text, align) { }

        //public override List<GroupBox> TooltipGroups
        //{
        //    get
        //    {
        //        if (Item is ITooltippable)
        //            return (Item as ITooltippable).TooltipGroups;
        //        return null;
        //    }
        //}

        //public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        //{
        //    if (WindowManager.TopMostControl == this)
        //        DrawHighlight(sb);
        //    base.Draw(sb);
        //}

        //public override void HandleInput(InputState input)
        //{
        //    //base.HandleInput(input);
        //    if (input.CurrentMouseState.LeftButton == ButtonState.Pressed && input.LastMouseState.LeftButton == ButtonState.Released)
        //        Pressed = true;
        //    if (input.CurrentMouseState.LeftButton == ButtonState.Released && input.LastMouseState.LeftButton == ButtonState.Pressed)
        //        if (Pressed)
        //        {
        //            Pressed = false;
        //            OnClick();
        //        }

        //    Keys[] keys = input.CurrentKeyboardState.GetPressedKeys();
        //    if (keys.Length > 0)
        //    {
        //        foreach (Keys key in keys)
        //        {
        //            if (input.IsKeyPressed(key))
        //                OnKeyPress(new KeyPressEventArgs(key));
        //        }
        //    }
        //}
    }
}
