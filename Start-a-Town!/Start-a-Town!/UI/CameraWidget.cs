using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class CameraWidget : GroupBox
    {
        GroupBox BoxRotation = new GroupBox();
        GroupBox BoxZoom = new GroupBox();
        IconButton BtnSettings;
        UICameraSettings Widget;// = new UICameraSettings();
        public CameraWidget(Camera camera)
        {
            this.BackgroundColorFunc = () => Color.Black * .5f;
            //var camera = Net.Client.Instance.Map.Camera;
            IconButton
                btn_rotleft = new IconButton("↺") { HoverText = "Rotate camera counterclockwise", LeftClickAction = () => camera.RotateCounterClockwise() },// ScreenManager.CurrentScreen.Camera.Rotation += 1 },
                btn_rotright = new IconButton("↻") { HoverText = "Rotate camera clockwise", LeftClickAction = () => camera.RotateClockwise() },//ScreenManager.CurrentScreen.Camera.Rotation -= 1 },
                btn_rotreset = new IconButton("↶") { HoverText = "Reset rotation", LeftClickAction = () => { camera.RotationReset(); } },
                btn_zoomin = new IconButton("+") { HoverText = "Zoom in", LeftClickAction = () => camera.ZoomIncrease() },// () => ScreenManager.CurrentScreen.Camera.Zoom *= 2 },
                btn_zoomreset = new IconButton("●") { HoverText = "Reset zoom", LeftClickAction = () => { camera.ZoomReset(); } },
                btn_zoomout = new IconButton("-") { HoverText = "Zoom out", LeftClickAction = () => camera.ZoomDecrease() };

            //↶⊘
            this.BoxRotation.AddControlsHorizontally(1, btn_rotleft, btn_rotreset, btn_rotright);
            this.BoxZoom.AddControlsHorizontally(1, btn_zoomin, btn_zoomreset, btn_zoomout);
            this.AddControlsVertically(1, BoxRotation, BoxZoom);

            Widget = new UICameraSettings(camera);
            BtnSettings = new IconButton(Icon.ArrowDown) {
                //Location = BoxZoom.BottomRight, 
                LocationFunc = () => BoxZoom.BottomRight,
                Anchor = Vector2.UnitX,
                BackgroundTexture = UIManager.Icon16Background, 
                LeftClickAction = OpenCameraSettings
            };
            this.AddControls(BtnSettings);

            //Controls.Add(btn_rotleft, btn_rotright, btn_zoomin, btn_zoomout, btn_zoomreset);
        }

        public void OpenCameraSettings()
        {
            //this.Widget.Location = UIManager.Mouse;
            this.Widget.Location = BtnSettings.ScreenLocation + BtnSettings.Dimensions;
            this.Widget.Toggle();
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
    }
    //class CameraWidgetOld : GroupBox
    //{
    //    public CameraWidgetOld()
    //    {
    //        var cam = Net.Client.Instance.Map.Camera;
    //        //IconButton
    //        //    btn_rotleft = new IconButton("↺") { HoverText = "Rotate camera counterclockwise", LeftClickAction = () => ScreenManager.CurrentScreen.Camera.RotateCounterClockwise() },// ScreenManager.CurrentScreen.Camera.Rotation += 1 },
    //        //    btn_rotright = new IconButton("↻") { Location = btn_rotleft.TopRight, HoverText = "Rotate camera clockwise", LeftClickAction = () => ScreenManager.CurrentScreen.Camera.RotateClockwise() },//ScreenManager.CurrentScreen.Camera.Rotation -= 1 },
    //        //    btn_zoomin = new IconButton("+") { Location = btn_rotright.TopRight, HoverText = "Zoom in", LeftClickAction = ()=>ScreenManager.CurrentScreen.Camera.ZoomIncrease() },// () => ScreenManager.CurrentScreen.Camera.Zoom *= 2 },
    //        //    btn_zoomout = new IconButton("-") { Location = btn_zoomin.TopRight, HoverText = "Zoom out", LeftClickAction = () => ScreenManager.CurrentScreen.Camera.ZoomDecrease() },//() => ScreenManager.CurrentScreen.Camera.Zoom /= 2 },
    //        //    btn_reset = new IconButton("↶") { Location = btn_zoomout.TopRight, HoverText = "Reset Camera", LeftClickAction = () => { ScreenManager.CurrentScreen.Camera.Rotation = 0; ScreenManager.CurrentScreen.Camera.ZoomReset(); } };
    //        IconButton
    //            btn_rotleft = new IconButton("↺") { HoverText = "Rotate camera counterclockwise", LeftClickAction = () => cam.RotateCounterClockwise() },// ScreenManager.CurrentScreen.Camera.Rotation += 1 },
    //            btn_rotright = new IconButton("↻") { Location = btn_rotleft.TopRight, HoverText = "Rotate camera clockwise", LeftClickAction = () => cam.RotateClockwise() },//ScreenManager.CurrentScreen.Camera.Rotation -= 1 },
    //            btn_zoomin = new IconButton("+") { Location = btn_rotright.TopRight, HoverText = "Zoom in", LeftClickAction = () => cam.ZoomIncrease() },// () => ScreenManager.CurrentScreen.Camera.Zoom *= 2 },
    //            btn_zoomout = new IconButton("-") { Location = btn_zoomin.TopRight, HoverText = "Zoom out", LeftClickAction = () => cam.ZoomDecrease() },//() => ScreenManager.CurrentScreen.Camera.Zoom /= 2 },
    //            btn_reset = new IconButton("↶") { Location = btn_zoomout.TopRight, HoverText = "Reset Camera", LeftClickAction = () => { cam.Rotation = 0; cam.ZoomReset(); } };

    //        Controls.Add(btn_rotleft, btn_rotright, btn_zoomin, btn_zoomout, btn_reset);

    //        // i dont need these buttons anymore cause i created a slider for the elevation
    //        //Button
    //        //    btn_up16 = new Button(btn_rotleft.BottomLeft, this.Width, "+16") { LeftClickAction = () => Rooms.Ingame.Instance.Camera.DrawLevel += 16 },
    //        //    btn_up1 = new Button(btn_up16.BottomLeft, this.Width, "+1") { LeftClickAction = () => Rooms.Ingame.Instance.Camera.DrawLevel += 1 },
    //        //    btn_down1 = new Button(btn_up1.BottomLeft, this.Width, "-1") { LeftClickAction = () => Rooms.Ingame.Instance.Camera.DrawLevel -= 1 },
    //        //    btn_down16 = new Button(btn_down1.BottomLeft, this.Width, "-16")
    //        //    {
    //        //        LeftClickAction = () => {
    //        //            Rooms.Ingame.Instance.Camera.DrawLevel -= 16;
    //        //        }
    //        //    };

    //        //Controls.Add(btn_up16, btn_up1, btn_down1, btn_down16);
    //    }
    //}
}
