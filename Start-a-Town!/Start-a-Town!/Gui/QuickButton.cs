﻿using Microsoft.Xna.Framework;
using System.Linq;

namespace Start_a_Town_.UI
{
    public class QuickButton : IconButton
    {
        IHotkey KeyBind;
        Label LabelShortcut;
        public QuickButton(char c, IHotkey keyBind, string label = "") : base(c)
        {
            KeyBind = keyBind;
            if (KeyBind != null)
            {
                LabelShortcut = new Label() { Location = Vector2.UnitX * UIManager.BorderPx, TextFunc = KeyBind.GetLabel, MouseThrough = true };
                AddControls(LabelShortcut);
            }
            if (!string.IsNullOrEmpty(label))
                this.AddControls(new Label(label) { Location = this.BottomCenter, Anchor = new Vector2(.5f, 1), MouseThrough = true });
        }
        public QuickButton(Icon icon, IHotkey keyBind, string label = "") : base(icon)
        {
            KeyBind = keyBind;
            if (KeyBind != null)
            {
                LabelShortcut = new Label() { Location = Vector2.UnitX * UIManager.BorderPx, TextFunc = KeyBind.GetLabel, MouseThrough = true };
                AddControls(LabelShortcut);
            }
            if (!string.IsNullOrEmpty(label))
                this.AddControls(new Label(label) { Location = this.BottomCenter, Anchor = new Vector2(.5f, 1), MouseThrough = true });
        }

        //public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        //{
        //    if (e.Handled)
        //        return;
        //    if (KeyBind?.ShortcutKeys.Contains(e.KeyCode) ?? false)
        //        Pressed = true;
        //    base.HandleKeyDown(e);
        //}
        //public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        //{
        //    if (e.Handled)
        //        return;
        //    if (KeyBind?.ShortcutKeys.Contains(e.KeyCode) ?? false)
        //    {
        //        PerformLeftClick();
        //        Pressed = false;
        //    }
        //    base.HandleKeyUp(e);
        //}
    }
}
