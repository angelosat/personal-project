using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class QuickButton : IconButton
    {
        KeyBind KeyBind;
        Label LabelShortcut;
        //Label LabelName;
        public QuickButton(char c, KeyBind keyBind, string label = "") : base(c)
        {
            KeyBind = keyBind;
            if (KeyBind != null)
            {
                LabelShortcut = new Label() { Location = Vector2.UnitX * UIManager.BorderPx, TextFunc = () => KeyBind.Key.ToString(), MouseThrough = true };
                AddControls(LabelShortcut);
            }
            if (!string.IsNullOrEmpty(label))
                this.AddControls(new Label(label) { Location = this.BottomCenter, Anchor = new Vector2(.5f, 1), MouseThrough = true });
        }
        public QuickButton(Icon icon, KeyBind keyBind, string label = "") : base(icon)
        {
            KeyBind = keyBind;
            if (KeyBind != null)
            {
                LabelShortcut = new Label() { Location = Vector2.UnitX * UIManager.BorderPx, TextFunc = () => KeyBind.Key.ToString(), MouseThrough = true };
                AddControls(LabelShortcut);
            }
            if (!string.IsNullOrEmpty(label))
                this.AddControls(new Label(label) { Location = this.BottomCenter, Anchor = new Vector2(.5f, 1), MouseThrough = true });
        }
        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;
            if (KeyBind?.Key != e.KeyCode)
                return;
            Pressed = true;
            base.HandleKeyDown(e);
        }
        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            
            if (e.Handled)
                return;
            if (KeyBind?.Key != e.KeyCode)
                return;
            PerformLeftClick();
            Pressed = false;
            base.HandleKeyUp(e);
        }
    }
}
