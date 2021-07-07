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
        UICameraSettings Widget;
        public CameraWidget(Camera camera)
        {
            this.BackgroundColorFunc = () => Color.Black * .5f;
            IconButton
                btn_rotleft = new IconButton("↺") { HoverText = "Rotate camera counterclockwise", LeftClickAction = () => camera.RotateCounterClockwise() },
                btn_rotright = new IconButton("↻") { HoverText = "Rotate camera clockwise", LeftClickAction = () => camera.RotateClockwise() },
                btn_rotreset = new IconButton("↶") { HoverText = "Reset rotation", LeftClickAction = () => { camera.RotationReset(); } },
                btn_zoomin = new IconButton("+") { HoverText = "Zoom in", LeftClickAction = () => camera.ZoomIncrease() },
                btn_zoomreset = new IconButton("●") { HoverText = "Reset zoom", LeftClickAction = () => { camera.ZoomReset(); } },
                btn_zoomout = new IconButton("-") { HoverText = "Zoom out", LeftClickAction = () => camera.ZoomDecrease() };

            //↶⊘
            this.BoxRotation.AddControlsHorizontally(1, btn_rotleft, btn_rotreset, btn_rotright);
            this.BoxZoom.AddControlsHorizontally(1, btn_zoomin, btn_zoomreset, btn_zoomout);
            this.AddControlsVertically(1, BoxRotation, BoxZoom);

            Widget = new UICameraSettings(camera);
            BtnSettings = new IconButton(Icon.ArrowDown) {
                LocationFunc = () => BoxZoom.BottomRight,
                Anchor = Vector2.UnitX,
                BackgroundTexture = UIManager.Icon16Background, 
                LeftClickAction = OpenCameraSettings
            };
            this.AddControls(BtnSettings);
        }

        public void OpenCameraSettings()
        {
            this.Widget.Location = BtnSettings.ScreenLocation + BtnSettings.Dimensions;
            this.Widget.Toggle();
        }
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
    }
}
