using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.UI
{
    class CameraWidget : GroupBox
    {
        public CameraWidget()
        {
            IconButton
                btn_rotleft = new IconButton(UIManager.Icon16Background) { HoverText = "Rotate camera left", LeftClickAction = () => ScreenManager.CurrentScreen.Camera.Rotation += 1 },
                btn_rotright = new IconButton(UIManager.Icon16Background) { Location = btn_rotleft.TopRight, HoverText = "Rotate camera right", LeftClickAction = () => ScreenManager.CurrentScreen.Camera.Rotation -= 1 },
                btn_zoomin = new IconButton(UIManager.Icon16Background) { Location = btn_rotright.TopRight, HoverText = "Zoom in", LeftClickAction = () => ScreenManager.CurrentScreen.Camera.Zoom *= 2 },
                btn_zoomout = new IconButton(UIManager.Icon16Background) { Location = btn_zoomin.TopRight, HoverText = "Zoom out", LeftClickAction = () => ScreenManager.CurrentScreen.Camera.Zoom /= 2 },
                btn_reset = new IconButton(UIManager.Icon16Background) { Location = btn_zoomout.TopRight, HoverText = "Reset Camera", LeftClickAction = () => { ScreenManager.CurrentScreen.Camera.Rotation = 0; ScreenManager.CurrentScreen.Camera.Zoom = 1; } };

            Controls.Add(btn_rotleft, btn_rotright, btn_zoomin, btn_zoomout, btn_reset);

            //Button
            //    btn_up16 = new Button() { Location = btn_rotleft.BottomLeft, Width = this.Width, Text = "+16", LeftClickAction = () => Engine.Map.DrawLevel += 16 },
            //    btn_up1 = new Button() { Location = btn_up16.BottomLeft, Width = this.Width, Text = "+1", LeftClickAction = () => Engine.Map.DrawLevel += 1 },
            //    btn_down1 = new Button() { Location = btn_up1.BottomLeft, Width = this.Width, Text = "-1", LeftClickAction = () => Engine.Map.DrawLevel -= 1 },
            //    btn_down16 = new Button() { Location = btn_down1.BottomLeft, Width = this.Width, Text = "-16", LeftClickAction = () => Engine.Map.DrawLevel -= 16 };
            Button
                btn_up16 = new Button(btn_rotleft.BottomLeft, this.Width, "+16") { LeftClickAction = () => Rooms.Ingame.Instance.Camera.DrawLevel += 16 },
                btn_up1 = new Button(btn_up16.BottomLeft, this.Width, "+1") { LeftClickAction = () => Rooms.Ingame.Instance.Camera.DrawLevel += 1 },
                btn_down1 = new Button(btn_up1.BottomLeft, this.Width, "-1") { LeftClickAction = () => Rooms.Ingame.Instance.Camera.DrawLevel -= 1 },
                btn_down16 = new Button(btn_down1.BottomLeft, this.Width, "-16")
                {
                    LeftClickAction = () => {
                        Rooms.Ingame.Instance.Camera.DrawLevel -= 16;
                    }
                };
            //Button
            //    btn_up16 = new Button(btn_rotleft.BottomLeft, this.Width, "+16") { LeftClickAction = () => Engine.Map.DrawLevel += 16 },
            //    btn_up1 = new Button(btn_up16.BottomLeft, this.Width, "+1") { LeftClickAction = () => Engine.Map.DrawLevel += 1 },
            //    btn_down1 = new Button(btn_up1.BottomLeft, this.Width, "-1") { LeftClickAction = () => Engine.Map.DrawLevel -= 1 },
            //    btn_down16 = new Button(btn_down1.BottomLeft, this.Width, "-16") { LeftClickAction = () => Engine.Map.DrawLevel -= 16 };

            Controls.Add(btn_up16, btn_up1, btn_down1, btn_down16);
        }
    }
}
