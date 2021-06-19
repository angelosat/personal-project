using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.PlayerControl
{
    class PlanningTool : ControlTool
    {
        static Dictionary<Vector3, GameObject> Blocks = new Dictionary<Vector3, GameObject>();

        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if(this.TargetOld==null)
            //    return Messages.Default;

            //Blocks.Add(TargetOld.Global + Face, GameObject.Objects[GameObject.Types.Cobblestone]);

            if (this.Target == null)
                return Messages.Default;

            Blocks.Add(this.Target.FaceGlobal, GameObject.Objects[GameObject.Types.Cobblestone]);

            return Messages.Default;
        }

        internal override void DrawWorld(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, IMap map, Camera camera)
        {
            foreach (var pair in Blocks)
                SpriteComponent.DrawPreview(sb, camera, pair.Key, pair.Value);
        }

        public override void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //TargetOld = Controller.Instance.Mouseover.Object as GameObject;
            this.Target = Controller.Instance.MouseoverBlock.Target;
            //Face = Controller.Instance.Mouseover.Face;
            base.HandleMouseMove(e);
        }

        //internal override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        //{
        //    foreach (KeyValuePair<Vector3, GameObject> pair in Blocks)
        //    {
        //        Vector3 global = pair.Key;
        //        Sprite sprite = (Sprite)pair.Value["Sprite"]["Sprite"];
        //        Sprite tileSprite = pair.Value["Sprite"]["Sprite"] as Sprite;
        //        Rectangle tileBounds = tileSprite.GetBounds();
        //        Rectangle bounds = camera.GetScreenBounds(global, sprite.GetBounds()); // posComp.GetScreenBounds(camera, sprComp); // 
        //        Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
        //        Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);

        //        int v = (int)pair.Value["Sprite"]["Variation"];
        //        int o = (int)pair.Value["Sprite"]["Orientation"];
        //        int oLength = sprite.SourceRect[v].Length;
        //        sb.Draw(sprite.Texture, screenLoc,
        //                sprite.SourceRect[v][SpriteComponent.GetOrientation(o, camera, oLength)], Color.White * 0.5f,//new Color(255, 255, 255, 127),
        //                0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
        //        Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
        //        Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);
        //    }
        //}
    }
}
