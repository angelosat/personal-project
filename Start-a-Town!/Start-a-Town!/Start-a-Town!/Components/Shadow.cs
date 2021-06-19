using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Components
{
    class Shadow
    {
        public GameObject Parent;
        public Vector3 Global;
        Rectangle Rectangle;
        float DepthNear, DepthFar;
        //public Shadow(Vector3 parent, Rectangle rect)//, float depthNear, float depthFar)
        public Shadow(GameObject parent, Vector3 global, Rectangle rect)//, float depthNear, float depthFar)
        {
            //this.Global = parent;
            this.Parent = parent;
            this.Global = global;
            this.Rectangle = rect;

        }
        public void Draw(MySpriteBatch sb, IMap map, Camera camera)
        {
            float dn = this.Global.GetDrawDepth(map, camera);
            //Vector2 pos = new Vector2((Rectangle.X + Rectangle.Width/2), (Rectangle.Y + Rectangle.Height/2)).Floor();
            Vector2 pos = camera.GetScreenPosition(this.Global).Floor();//.Round();
            Sprite.Shadow.Draw(sb, pos, Color.White, 0, Sprite.Shadow.Origin, camera.Zoom, SpriteEffects.None, dn);// dn);

            //// TODO: BERDEMA
            //if (Player.Actor != null)
            //    if (this.Parent == Player.Actor.GetComponent<AttackComponent>().Target)
            //    {
            //        var border = Sprite.Shadow.GetBounds();
            //        border.Inflate(1, 1);
            //        //border.DrawHighlight(sb, Color.Red, Vector2.Zero, 0);
            //        var bounds = camera.GetScreenBounds(this.Global, border);
            //       // pos = camera.GetScreenPosition(this.Global + Vector3.UnitZ).Floor();
            //        //sb.Draw(UI.UIManager.Highlight, bounds, null, Color.Red, 0, Vector2.Zero, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
            //        //Sprite.Shadow.Draw(sb, camera.GetScreenPosition(this.Global + Vector3.UnitZ).Floor(), Color.White, 0, Sprite.Shadow.Origin, camera.Zoom, SpriteEffects.None, dn);
            //        Rectangle rect = Player.Actor.Body.GetMinimumRectangle();
            //        bounds = camera.GetScreenBounds(this.Global, rect);
            //        //sb.Draw(UI.UIManager.Highlight, UI.UIManager.Highlight.Bounds, bounds.ToVector4(), Color.Red, dn);
            //        //sb.Draw(Sprite.BlockFaceHighlights[Vector3.UnitZ], UI.UIManager.Highlight.Bounds, bounds.ToVector4(), Color.Red, dn);
            //        var sprite = Sprite.BlockFaceHighlights[Vector3.UnitZ];
            //        sprite.Draw(sb, pos, Color.Red, 0, sprite.Origin, camera.Zoom, SpriteEffects.None, dn);
            //    }
        }
        public void Draw(SpriteBatch sb, Map map, Camera camera)
        {
            //float dn = Cell.GetGlobalDepthNew(Global, map, camera);
           // sb.Draw(Map.Shadow, Rectangle, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, dn);
        }
    }

}
