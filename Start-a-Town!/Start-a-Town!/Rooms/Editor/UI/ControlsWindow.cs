using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.PlayerControl;

namespace Start_a_Town_.Editor
{
    class ControlsWindow : Window
    {
        Button BtnOrigin;
        public Bldg Bldg;
        public ControlsWindow()
        {
            //this.Bldg = bldg;
            this.Movable = true;
            AutoSize = true;
            this.Title = "Map Controls";
            this.BtnOrigin = new Button("Origin")
            {
                LeftClickAction = () =>
                {
                    //ScreenManager.CurrentScreen.ToolManager.ActiveTool = new BlockPainter()
                    EmptyTool tool = new EmptyTool();
                    tool.LeftClick = (t) =>
                    {
                        if (tool.Target == null)
                            return ControlTool.Messages.Default;

                        if (tool.Target.Type != TargetType.Position)
                            return ControlTool.Messages.Default;

                        this.Bldg.Origin = tool.Target.Global;
                        return ControlTool.Messages.Default;
                    };
                    ToolManager.SetTool(new GridTool());
                    //ScreenManager.CurrentScreen.ToolManager.ActiveTool = new GridTool();// tool;
                }
            };
            this.Client.Controls.Add(this.BtnOrigin);
            //this.Location = this.CenterScreen * 0.5f;
            this.SnapToScreenCenter();

        }
    }
}
