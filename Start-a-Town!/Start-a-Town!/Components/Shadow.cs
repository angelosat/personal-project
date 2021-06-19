using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Components
{
    struct Shadow
    {
        public GameObject Parent;
        public Vector3 Global;
        public float Alpha;
        const int ShadowVisibilityRange = 4;

        //Rectangle Rectangle;
        //float DepthNear, DepthFar;
        public Shadow(GameObject parent, Vector3 global)// Rectangle rect)
        {
            //this.Global = parent;
            this.Parent = parent;
            this.Global = global;
            this.Alpha = Math.Max(0, 1 - (parent.Global.Z - global.Z) / ShadowVisibilityRange);
            //this.Rectangle = rect;

        }
        public void Draw(MySpriteBatch sb, IMap map, Camera camera)
        {
            if (camera.IsCompletelyHiddenByFog(this.Global.Z))
                return;
            float dn = this.Global.GetDrawDepth(map, camera);
            Vector2 pos = camera.GetScreenPosition(this.Global).Floor();
            //Sprite.Shadow.Draw(sb, pos, Color.White, 0, Sprite.Shadow.OriginGround, camera.Zoom, SpriteEffects.None, dn);
            Sprite.Shadow.Draw(sb, pos, Color.White * this.Alpha, 0, Sprite.Shadow.OriginGround, camera.Zoom*Parent.Body.Scale, SpriteEffects.None, dn);

        }
    }

}
