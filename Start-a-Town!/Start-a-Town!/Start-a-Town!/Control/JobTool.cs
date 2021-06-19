using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Rooms;

using Start_a_Town_.Components;

namespace Start_a_Town_.PlayerControl
{
    public class JobTool : ControlTool
    {
        public bool Valid;
        public GameObject.Types Type;
        public GameObject Object;

        static JobTool _Instance;
        public static JobTool Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new JobTool();
                return _Instance;
            }
        }

        public InteractionOld SelectedItem;
        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }

        JobTool()
        {
        //    ContextMenu.Instance.SelectedItemChanged += new EventHandler<EventArgs>(Instance_SelectedItemChanged);
        }

        //void Instance_SelectedItemChanged(object sender, EventArgs e)
        //{
        //    SelectedItem = ContextMenu.Instance.SelectedItem;
        //    ContextMenu.Instance.Hide();
        //    OnSelectedItemChanged();
        //}

        internal override void Jump()
        {
            //Player.Actor.PostMessage(Message.Types.Jump);
            return;
        }

        //internal override void KeyDown(InputState e)
        //{
        //    DefaultTool.MoveKeys(e);
        //}

        internal override void Throw()
        {
            throw new NotImplementedException();
            //Player.Actor.PostMessage(Message.Types.Throw, null, DefaultTool.GetDirection());
            return;
        }

        internal override void HandleContextMenuSelection(InteractionOld inter)
        {
            SelectedItem = inter;
            OnSelectedItemChanged();
        }

        public override Messages OnMouseLeft(bool held)
        {
            if (held)
                return Messages.Default;

            GameObject tar;
            if (!Controller.Instance.MouseoverNext.TryGet<GameObject>(out tar))
                return Messages.Default;




            return Messages.Default;
        }

        //public override Messages MouseRight(bool held)
        public override ControlTool.Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            GameObject tar;
            if (!Controller.Instance.Mouseover.TryGet<GameObject>(out tar))
                return Messages.Default;

          //  ContextMenu.Instance.Initialize(Player.Actor, tar);

            return base.MouseRightDown(e);
        }
      
        //internal override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        //{
        //    if (Controller.Instance.ksCurrent.IsKeyDown(Keys.LeftControl))
        //        return;

        //    GameObject tar;
        //    if (!Controller.Instance.Mouseover.TryGet<GameObject>(out tar))
        //        return;

        //    Vector3 f = Controller.Instance.Mouseover.Face;
        //    int rx, ry;
        //    Camera cam = new Camera(camera.Width, camera.Height);
        //    cam.Rotation = -camera.Rotation;
        //    Coords.Rotate(cam, f.X, f.Y, out rx, out ry);
        //    Vector3 global = tar.Global + new Vector3(rx, ry, f.Z);
        //    //Vector3 global = tar.Global + new Vector3(f.R, f.G, f.B);
        //    Sprite sprite = (Sprite)Object["Sprite"]["Sprite"];
        //    Sprite tileSprite = tar["Sprite"]["Sprite"] as Sprite;
        //    Rectangle tileBounds = tileSprite.GetBounds();
        //    Rectangle bounds = camera.GetScreenBounds(global, sprite.GetBounds()); // posComp.GetScreenBounds(camera, sprComp); // 
        //    Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
        //    Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);
        //    Valid = true;
        //    if(Object.Components.ContainsKey("Multi"))
        //        foreach (Vector3 vector in Object["Multi"].GetProperty<Dictionary<Vector3, Sprite>>("MultiTile").Keys)
        //        {
        //            Vector3 vg = global + vector;
        //            Rectangle scrBounds = camera.GetScreenBounds(vg, tileBounds);
        //            Vector2 scr = new Vector2(scrBounds.X, scrBounds.Y);
        //           // bool check = (Position.GetCell(vg).TileType == Tile.Types.Air && Position.GetCell(vg - new Vector3(0,0,1)).Solid);
        //            Cell cell;
        //            bool check;
        //            if (!Position.TryGetCell(vg, out cell))
        //                check = false;
        //            else
        //                check = (cell.TileType == Tile.Types.Air && Position.GetCell(vg - new Vector3(0, 0, 1)).Solid);
        //            Color c = check ? Color.White : Color.Red;
        //            Valid &= check;
        //            sb.Draw(Map.TerrainSprites, scr, Tile.TileHighlights[0][0], c * 0.5f, 0, new Vector2(0, -tileSprite.Origin.Y + 16), camera.Zoom, SpriteEffects.None, 0);
        //        }

        //    //sb.Draw(sprComp.Sprite.Texture, screenLoc,
        //    //    sprComp.Sprite.SourceRect[sprComp.Variation][sprComp.GetOrientation(camera)], new Color(255, 255, 255, 127),
        //    int v = (int)Object["Sprite"]["Variation"];
        //    int o = (int)Object["Sprite"]["Orientation"];
        //    int oLength = sprite.SourceRect[v].Length;
        //    sb.Draw(sprite.Texture, screenLoc,
        //            sprite.SourceRect[v][SpriteComponent.GetOrientation(o, camera, oLength)], Color.White * 0.5f,//new Color(255, 255, 255, 127),
        //            0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
        //    Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
        //    Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);
        //}
    }
}
