using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.PlayerControl
{
    class ObjectOrientationTool : ControlTool
    {
        public GameObject Object;
        SpriteComponent SpriteComponent;
        MultiTile2Component MultiComponent;
        public Sprite.OrientationType Orientation;
        public int Variation;
        int PreviewOrientation;

        public ObjectOrientationTool() { }
        public ObjectOrientationTool(GameObject obj)
        {
            Object = obj;
            SpriteComponent = Object.GetComponent<SpriteComponent>("Sprite");
            Object.TryGetComponent<MultiTile2Component>("Multi", out MultiComponent);
        }

        public override void HandleInput(InputState e)
        {
         //   int x = e.CurrentMouseState.X - Game1.Instance.graphics.PreferredBackBufferWidth/ 2, y = Game1.Instance.graphics.PreferredBackBufferHeight/2 - e.CurrentMouseState.Y;
          //  int x = e.CurrentMouseState.X - Game1.Instance.Window.ClientBounds.Width / 2, y = Game1.Instance.Window.ClientBounds.Height / 2 - e.CurrentMouseState.Y;
            //Camera camera = Rooms.Ingame.Instance.Camera;
            var camera = Net.Client.Instance.Map.Camera;
            int x = e.CurrentMouseState.X - camera.Width / 2, y = camera.Height / 2 - e.CurrentMouseState.Y;
            if (x <= 0 && y <= 0)
                Orientation = Sprite.OrientationType.South;
            else if (x > 0 && y <= 0)
                Orientation = Sprite.OrientationType.West;
            else if (x > 0 && y > 0)
                Orientation = Sprite.OrientationType.North;
            else if (x <= 0 && y > 0)
                Orientation = Sprite.OrientationType.East;

            PreviewOrientation = (int)Orientation;
            Orientation = (Sprite.OrientationType)(SpriteComponent.GetOrientation((int)Orientation, camera, ((Sprite)Object["Sprite"]["Sprite"]).SourceRects.First().Length));
           // Orientation = Enum.Parse(typeof(Sprite.Orientation), (int)Orientation + camera.Rotation);
        }

        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }

        public ControlTool.Messages OnMouseLeft(bool held)
        {
            if (held)
                return Messages.Default;
            //base.OnMouseLeft(held);
            return Messages.Remove;
        }

        public override ControlTool.Messages MouseWheel(InputState input, int value)
        {
            Variation += value;
            Variation %= SpriteComponent.Sprite.SourceRects.Length;// -1;
            if (Variation < 0)
                Variation = SpriteComponent.Sprite.SourceRects.Length - 1 + Variation;
            input.Handled = true;
            return Messages.Default;
        }

        internal override void DrawWorld(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, IMap map, Camera camera)
        {
            PositionComponent transform = this.Object.Transform;
            if (transform == null)
                return;

            Rectangle bounds;
            Vector2 screenLoc;
            if (MultiComponent != null)
            {
                MultiComponent.DrawPreview(sb, camera, Object.Global, PreviewOrientation);
            }
            else
            {

                bounds = transform.GetScreenBounds(camera, SpriteComponent); // camera.GetScreenBounds((int)Start.X + cellX, (int)Start.Y + cellY, cellZ, spriteComp.Sprite.GetBounds());
                screenLoc = new Vector2(bounds.X, bounds.Y);
                Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);

                sb.Draw(SpriteComponent.Sprite.Texture, screenLoc,
                    SpriteComponent.Sprite.SourceRects[Variation][PreviewOrientation], new Color(255, 255, 255, 127),
                    0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
                Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));

                Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);
            }

            string
                text = "Mouse move: Change orientation",
                text2 = "Mouse wheel: Change variation";
            Vector2
                textSize = UI.UIManager.Font.MeasureString(text),
                text2Size = UI.UIManager.Font.MeasureString(text2);
            UI.UIManager.DrawStringOutlined(sb, text,
                new Vector2(UI.UIManager.Width / 2 - (int)(textSize.X / 2), UI.UIManager.Height / 4), Vector2.Zero);
            UI.UIManager.DrawStringOutlined(sb, text2,
                new Vector2(UI.UIManager.Width / 2 - (int)(text2Size.X / 2), UI.UIManager.Height / 4 + textSize.Y), Vector2.Zero);

        }
    }
}
