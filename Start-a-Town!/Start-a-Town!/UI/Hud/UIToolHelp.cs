using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class UIToolHelp : GroupBox
    {
        ControlTool PrevTool;

        public UIToolHelp()
        {
            this.BackgroundColorFunc = () => Color.Black * .5f;
        }

        public override void Update()
        {
            base.Update();
            if (ScreenManager.CurrentScreen.ToolManager.ActiveTool != this.PrevTool)
                this.Refresh(ScreenManager.CurrentScreen.ToolManager.ActiveTool);
            this.PrevTool = ScreenManager.CurrentScreen.ToolManager.ActiveTool;
        }

        private void Refresh(ControlTool activeTool)
        {
            this.Location += this.Width * Vector2.UnitX;
            this.ClearControls();
            var text = activeTool.HelpText;
            if (string.IsNullOrWhiteSpace(text))
            {
                this.Hide();
                return;
            }
            this.AddControls(new Label(text));
            this.ConformToControls();
            this.AnchorTo(this.Location, Vector2.UnitX);
            this.Show();
        }
    }
}
