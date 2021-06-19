using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Components
{
    class MultiTile2Component : Component
    {
        public override string ComponentName
        {
            get { return "MultiTile"; }
        }
        Dictionary<Vector3, Sprite> MultiTile { get { return GetProperty<Dictionary<Vector3, Sprite>>("MultiTile"); } set { Properties["MultiTile"] = value; } }
        Sprite Sprite { get { return (Sprite)this["Sprite"]; } set { this["Sprite"] = value; } }
        GameObject Master { get { return this.GetProperty<GameObject>("Master"); } set { this["Master"] = value; } }
        Dictionary<Vector3, GameObject> Children { get { return GetProperty<Dictionary<Vector3, GameObject>>("Children"); } set { Properties["Children"] = value; } }
        bool HasChildren { get { return (Children != null ? Children.Count > 0 : false); } }
        Vector3 Part { get { return (Vector3)this["Part"]; } set { this["Part"] = value; } }

        public Dictionary<Vector3, GameObject> GetChildren()
        {
            return Children.ToDictionary(foo => foo.Key, foo => foo.Value);
        }

        Dictionary<Vector3, GameObject> PendingChildren = new Dictionary<Vector3, GameObject>();

        public MultiTile2Component()
        {
            this.Properties.Add("Sprite");
            this.Properties.Add("MultiTile");
            this.Properties.Add("Master");
            this.Properties.Add("Children");
            this.Properties.Add("Part");
        }

        public MultiTile2Component(Sprite fullSprite, Dictionary<Vector3, Sprite> sprites)
                    : this()
       // static public MultiTile2Component Create(GameObject parent, Sprite fullSprite, Dictionary<Vector3, Sprite> sprites)
        {
            Sprite = fullSprite;
            MultiTile = sprites;
            Children = new Dictionary<Vector3, GameObject>();
            foreach (Vector3 part in sprites.Keys)
                if (part != Vector3.Zero)
                {
                    //Children.Add(part, null);
                    //GameObject child = CreateChild(parent, part);
                    Children.Add(part, null);
                }
           // parent["Multi"] = multi;
          //  return multi;
        }

        public MultiTile2Component(GameObject master, Vector3 part)
            : this()
        {
            Master = master;
            this.Part = part;
        }

        public override void Spawn(Net.IObjectProvider net, GameObject parent)
        {
            DeployParts(net, parent);
        }

        public override void Despawn(//Net.IObjectProvider net,
                    GameObject parent)
        {
            if (HasChildren)
                foreach (KeyValuePair<Vector3, GameObject> multi in Children)
                    parent.Net.SyncDisposeObject(multi.Value);
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            if (Master != null)
            {
                Master.PostMessage(e);
                return true;
            }
            switch (e.Type)
            {
                case Message.Types.Create:
                    break;

                default:
                    break;
            }
            return true;
        }

        public override void ChunkLoaded(Net.IObjectProvider net, GameObject parent)
        {
            DeployParts(net, parent);
        }

        static GameObject CreateChild(GameObject parent, Vector3 part)
        {
            return new GameObject(GameObject.Types.Dummy, parent.Name + " " + part, parent.Description);
        }

        private void DeployParts(Net.IObjectProvider net, GameObject parent)
        {
            int orientation = (int)parent["Sprite"]["Orientation"];

            foreach (GameObject obj in Children.Values)
                if (obj != null)
                    net.SyncDisposeObject(obj);
                    //obj.Remove();
            parent["Sprite"]["Sprite"] = MultiTile[Vector3.Zero];
            parent["Sprite"]["Orientation"] = orientation;
            Part = Vector3.Zero;
            Vector3 global = parent.Global;
            foreach (KeyValuePair<Vector3, Sprite> multi in MultiTile)
            {
                if (multi.Key == Vector3.Zero)
                    continue;
                Vector3 rotated;
                Coords.Rotate(orientation, multi.Key, out rotated);
                GameObject child = new GameObject(GameObject.Types.Dummy, parent.Name + " " + multi.Key, parent.Description);// parent.Clone(); parent.ID, GameObject.Types.Dummy
                child["Sprite"] = new SpriteComponent(multi.Value);
                child["Sprite"]["Orientation"] = orientation;
                child["Multi"] = new MultiTile2Component(parent, multi.Key);
                child["Physics"] = (PhysicsComponent)parent["Physics"].Clone();
                //Children[multi.Key] = child;
                //Chunk.AddObject(child, global + multi.Key);
                if (Chunk.AddObject(child, parent.Map, global + rotated))// multi.Key))
                    Children[multi.Key] = child;
                else
                    PendingChildren.Add(multi.Key, child);
           //     UI.Nameplate.Hide(child);
            }
        }

        public bool CheckValidity(Map map, Vector3 global)
        {
            foreach (Vector3 vector in MultiTile.Keys)
            {
                Vector3 vg = global + vector;
                //if (Position.GetCell(map, vg).Block.Type == Block.Types.Air && (vg - new Vector3(0, 0, 1)).IsSolid(map))// Position.GetCell(map, vg - new Vector3(0, 0, 1)).Solid)
                if (Position.GetCell(map, vg).Block.Type == Block.Types.Air && map.IsSolid(vg - new Vector3(0, 0, 1)))// Position.GetCell(map, vg - new Vector3(0, 0, 1)).Solid)
                    return false;
            }
            return true;
        }
        public bool CheckValidity(Map map, Vector3 global, ref Dictionary<Vector3, bool> positions)
        {
            bool check, valid = true;
            foreach (Vector3 vector in MultiTile.Keys)
            {
                Vector3 vg = global + vector;
                //check = (Position.GetCell(map, vg).Block.Type == Block.Types.Air && (vg - new Vector3(0, 0, 1)).IsSolid(map));//Position.GetCell(map, vg - new Vector3(0, 0, 1)).Solid);
                check = (Position.GetCell(map, vg).Block.Type == Block.Types.Air && map.IsSolid(vg - new Vector3(0, 0, 1)));//Position.GetCell(map, vg - new Vector3(0, 0, 1)).Solid);
                valid &= check;
                positions[vector] = check;
            }
            return valid;
        }


        //public Dictionary<Vector3, bool> CheckValidity(Vector3 global)
        //{
        //    Dictionary<Vector3, bool> positions = new Dictionary<Vector3, bool>();
        //    foreach (Vector3 vector in MultiTile.Keys)
        //    {
        //        Vector3 vg = global + vector;
        //        positions[vector] = (Position.GetCell(vg).TileType == Tile.Types.Air && Position.GetCell(vg - new Vector3(0, 0, 1)).Solid);
        //    }
        //    return positions;
        //}      

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            if (Part != Vector3.Zero)
                return;
            if (PendingChildren.Count > 0)
                foreach (KeyValuePair<Vector3, GameObject> part in PendingChildren.ToDictionary(foo => foo.Key, foo => foo.Value))
                    if (Chunk.AddObject(part.Value, net.Map, parent.Global + part.Key))
                    {
                        PendingChildren.Remove(part.Key);
                        Children[part.Key] = part.Value;
                    }
        }

        public override object Clone()
        {
            return new MultiTile2Component(Sprite, MultiTile);
        }

        public void DrawFootprint(SpriteBatch sb, Camera camera, Vector3 global, int orientation)
        {
            foreach (Vector3 vector in MultiTile.Keys)
            {
                //Sprite tileSprite = tar["Sprite"]["Sprite"] as Sprite;
                Vector3 rotated;
                Coords.Rotate(orientation, vector, out rotated); //-(int)camera.Rotation //Object.Global +
                Vector3 final = global + rotated;
                final = final.RoundXY();
                Rectangle tileBounds = Block.Bounds;//ileSprite.GetBounds();
                Rectangle scrBounds = camera.GetScreenBounds(final, tileBounds);
                Vector2 scr = new Vector2(scrBounds.X, scrBounds.Y);
                Cell cell;
                bool check;
                //if (!Position.TryGetCell(Engine.Map, final, out cell))
                if (!Engine.Map.TryGetCell(final, out cell))
                    check = false;
                else
                    //check = (cell.Block.Type == Block.Types.Air && (final - new Vector3(0, 0, 1)).IsSolid(Engine.Map));//Position.GetCell(Engine.Map, final - new Vector3(0, 0, 1)).Solid);
                    check = (cell.Block.Type == Block.Types.Air && Engine.Map.IsSolid(final - new Vector3(0, 0, 1)));//Position.GetCell(Engine.Map, final - new Vector3(0, 0, 1)).Solid);

                Color c = check ? Color.White : Color.Red;
                //sb.Draw(Map.TerrainSprites, scr, Tile.TileHighlights[0][0], c * 0.5f, 0, new Vector2(0, -Tile.OriginCenter.Y + 16), camera.Zoom, SpriteEffects.None, 0);
                sb.Draw(Map.TerrainSprites, scr, Block.TileHighlights[0][2], c * 0.5f, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
            }
        }

        public void DrawPreview(SpriteBatch sb, Camera camera, Vector3 global, int orientation)
        {
            DrawFootprint(sb, camera, global, orientation);
            Rectangle bounds;
            Vector2 screenLoc;
            foreach (KeyValuePair<Vector3, Sprite> multi in MultiTile)
            {
                Vector3 rotated;
                Coords.Rotate(orientation, multi.Key, out rotated); //-(int)camera.Rotation //Object.Global +
                Vector3 final = global + rotated;
                final = final.RoundXY();
                bounds = camera.GetScreenBounds(final, multi.Value.GetBounds());
                screenLoc = new Vector2(bounds.X, bounds.Y);
      //          Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);

                sb.Draw(multi.Value.Texture, screenLoc,
                    multi.Value.SourceRects[0][orientation], Color.White * 0.5f, //new Color(255, 255, 255, 127),
                    0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
                Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));

      //          Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);

                //Rectangle tileBounds = Tile.Bounds;//ileSprite.GetBounds();
                //Rectangle scrBounds = camera.GetScreenBounds(final, tileBounds);
                //Vector2 scr = new Vector2(scrBounds.X, scrBounds.Y);
                //bool check = (Position.GetCell(final).TileType == Tile.Types.Air && Position.GetCell(final - new Vector3(0, 0, 1)).Solid);
                //Color c = check ? Color.White : Color.Red;
                //sb.Draw(Map.TerrainSprites, scr, Tile.TileHighlights[0][0], c, 0, new Vector2(0, -Tile.OriginCenter.Y + 16), camera.Zoom, SpriteEffects.None, 0);
                //
            }
        }
        static public void DrawPreview(SpriteBatch sb, Camera camera, GameObject obj, Vector3 global, int orientation)
        {
            MultiTile2Component m;
            if (!obj.TryGetComponent<MultiTile2Component>("Multi", out m))
                return;
            m.DrawPreview(sb, camera, global, orientation);
        }

        public override void DrawMouseover(SpriteBatch sb, Camera camera, GameObject parent)
        {
            if (Master.IsNull())
                sb.Draw(Sprite.Texture, camera.GetScreenBounds(parent.Global, Sprite.SourceRects[0][0]), Sprite.SourceRects[0][0], new Color(255, 255, 255, 127), 0, Sprite.Origin, SpriteEffects.None, 0);
            else
                Master.DrawMouseover(sb, camera);
        }

        public override void OnHitTestPass(GameObject parent, Vector3 face, float depth)
        {
            Controller.Instance.MouseoverNext.Object = Master ?? parent;
            Controller.Instance.MouseoverNext.Face = face;
            Controller.Instance.MouseoverNext.Depth = depth;
        }

        static public void GetClosest(GameObject target, GameObject agent, ref Vector3 difference, ref float length)
        {
            //difference = (target.Global - agent.Global);
            //length = difference.Length();
            MultiTile2Component multiComp;
            if (!target.TryGetComponent("Multi", out multiComp))
                return;
            foreach (GameObject child in multiComp.GetChildren().Values)
            {
                Vector3 childDiff = (child.Global - agent.Global);
                float childLength = childDiff.Length();
                if (childLength < length)
                {
                    difference = childDiff;
                    length = childLength;
                }
            }
        }

        static public bool IsPositionValid(GameObject parent, IMap map, Vector3 global)
        {
            MultiTile2Component multiComp;
            if (!parent.TryGetComponent<MultiTile2Component>("Multi", out multiComp))
                return true;

            foreach (Vector3 vector in multiComp.GetProperty<Dictionary<Vector3, Sprite>>("MultiTile").Keys)
            {
                Vector3 vg = global + vector;
                vg = vg.RoundXY();
                Cell cell;
                bool check;
                //if (!Position.TryGetCell(map, vg, out cell))
                if (!map.TryGetCell(vg, out cell))
                    check = false;
                else
                    //check = (cell.Block.Type == Block.Types.Air && ((vg - new Vector3(0, 0, 1)).IsSolid(map)));//Position.GetCell(map, vg - new Vector3(0, 0, 1)).Solid);
                    check = (cell.Block.Type == Block.Types.Air && (map.IsSolid(vg - new Vector3(0, 0, 1))));//Position.GetCell(map, vg - new Vector3(0, 0, 1)).Solid);

                if (!check)
                    return false;
            }
            return true;
        }

        static public bool IsPositionValid(GameObject parent, IMap map, Vector3 global, int orientation)
        {
            MultiTile2Component multiComp;
            if (!parent.TryGetComponent<MultiTile2Component>("Multi", out multiComp))
                return true;
            Vector3 rotated;
            
            foreach (Vector3 vector in multiComp.GetProperty<Dictionary<Vector3, Sprite>>("MultiTile").Keys)
            {
                Coords.Rotate(orientation, vector, out rotated);
                Vector3 vg = global + rotated;
                vg = vg.RoundXY();
                Cell cell;
                bool check;
                //if (!Position.TryGetCell(map, vg, out cell))
                if (!map.TryGetCell(vg, out cell))
                    check = false;
                else
                    //check = (cell.Block.Type == Block.Types.Air && (vg - new Vector3(0, 0, 1)).IsSolid(map));// Position.GetCell(map, vg - new Vector3(0, 0, 1)).Solid);
                    check = (cell.Block.Type == Block.Types.Air && map.IsSolid(vg - new Vector3(0, 0, 1)));// Position.GetCell(map, vg - new Vector3(0, 0, 1)).Solid);

                if (!check)
                    return false;
            }
            return true;
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            if (Master == null)
                return;
            tooltip.Controls.Clear();
            Master.GetTooltip(tooltip);
        }

        internal override List<SaveTag> Save()
        {
            List<SaveTag> save = new List<SaveTag>();
            save.Add(new SaveTag(SaveTag.Types.Int, "PartX", (int)Part.X));
            save.Add(new SaveTag(SaveTag.Types.Int, "PartY", (int)Part.Y));
            save.Add(new SaveTag(SaveTag.Types.Int, "PartZ", (int)Part.Z));
            return save;
        }

        internal override void Load(SaveTag compTag)
        {
            Part = new Vector3((int)compTag["PartX"].Value, (int)compTag["PartY"].Value, (int)compTag["PartZ"].Value);
        }
    }
}
