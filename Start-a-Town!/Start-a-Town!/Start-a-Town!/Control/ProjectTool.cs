using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;

namespace Start_a_Town_.PlayerControl
{
    class ProjectTool : ControlTool
    {
        public GameObject Project;
        ProjectComponent ProjectComponent;
        public Vector3 Global;

        public ProjectTool(GameObject project)
        {
            this.Project = project;
            this.ProjectComponent = project.GetComponent<ProjectComponent>("Project");
        }

        public override Messages OnMouseLeft(bool held)
        {
            if (held)
                return Messages.Default;

            //if (!Controller.Instance.MouseoverNext.TryGet<GameObject>(out Target))
            //    return Messages.Default;



            //Global = TargetOld.Global + Controller.Instance.Mouseover.Face;
            Global = this.Target.FaceGlobal;

           // ProjectComponent.Tiles[Global] = InputState.IsKeyDown(Keys.LControlKey) ? Tile.Types.Air : UI.ProjectsWindow.Instance.SelectedTile;
            GameObject tile = GameObject.Create(GameObject.Types.ConstructionReservedTile);
            Chunk.AddObject(tile, Engine.Map, Global);
            //tile["Tile"]["Type"] = InputState.IsKeyDown(Keys.LControlKey) ? Tile.Types.Air : UI.ProjectsWindow.Instance.SelectedTile;

            throw new NotImplementedException();
            //tile.PostMessage(Components.Message.Types.SetTile, null, InputState.IsKeyDown(Keys.LControlKey) ? Block.Types.Air : UI.ProjectsWindow.Instance.SelectedTile);
            return Messages.Default;
        }

        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }

        public override void HandleInput(InputState e)
        {
            //TargetOld = Controller.Instance.Mouseover.Object as GameObject;
            this.Target = Controller.Instance.Mouseover.Target;
            //Face = Controller.Instance.Mouseover.Face;
            base.HandleInput(e);
        }

        internal override void DrawWorld(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, IMap map, Camera camera)
        {
            sb.Draw(UI.UIManager.Highlight, new Rectangle(0, 0, UI.UIManager.Width, UI.UIManager.Height), null, Color.Lerp(Color.Black, Color.Transparent, 0.5f), 0, Vector2.Zero, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);

            foreach (KeyValuePair<Vector3, Block.Types> tile in ProjectComponent.Tiles)
            {
                float gx = tile.Key.X, gy = tile.Key.Y, z = tile.Key.Z;
                Rectangle spriteBounds = Block.Bounds;
                Rectangle tileBounds = camera.GetScreenBounds(gx, gy, z, spriteBounds);
                Vector2 screenLoc = new Vector2(tileBounds.X, tileBounds.Y);
                //Sprite tileSprite = Block.TileSprites[tile.Value];
                //Rectangle sourceRect = tileSprite.SourceRects[0][0];
                Rectangle sourceRect = Block.Registry[tile.Value].Variations.First().Rectangle;
                sb.Draw(Map.TerrainSprites, screenLoc, sourceRect, Color.White, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, tile.Key.GetDrawDepth(Engine.Map, camera));
            }
        }
    }
}
