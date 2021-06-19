using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.PlayerControl
{
    class DesignTool : ControlTool
    {
        #region Singleton
        static DesignTool _Instance;
        public static DesignTool Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new DesignTool();
                return _Instance;
            }
        }
        #endregion

        //public GameObject Project;
        static BuildingPlanComponent Component;
        //static public Dictionary<Vector3, DesignObject> Blocks;
        static public DesignObject Brush;
        static GameObject Project;
        static public int Stage;

        DesignTool()
        {
          //  Blocks = new Dictionary<Vector3, DesignObject>();
            Project = GameObjectDb.BuildingPlan;
            Component = Project.GetComponent<BuildingPlanComponent>("Project");
            Stage = 0;
        }

        static public void NextStage()
        {
            if (Stage == Component.Stages.Count - 1)
            {
                if (Component.Stages[Stage].Count == 0)
                    return;
                Component.Stages.Add(new Dictionary<Vector3, Components.DesignObject>());
            }
            foreach (var block in Component.Stages[Stage])
            {
                throw new NotImplementedException();
                //GameObject obj = GameObject.Create(BlockComponent.Blocks[block.Value.Type].Entity.ID);
                //Chunk.AddObject(obj, Engine.Map, block.Key);
                //obj.Initialize().Spawn();
            }
            Stage += 1;
        }

        static public void LastStage()
        {
            if (Stage == 0)
                return;
            Stage = Math.Max(0, Stage - 1);
            foreach (var block in Component.Stages[Stage])
            {
                //Cell.Remove(Engine.Map, block.Key);
                block.Key.TrySetCell(Net.Client.Instance, Block.Types.Air, 0, 0);
            }

        }

        static public void Initialize(GameObject project)
        {
            Project = project;
            Component = project.GetComponent<BuildingPlanComponent>("Project");
            Stage = 0;
        }

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            //if (this.TargetOld == null)
            //    return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            if (Brush.Type == Block.Types.Air)
                return Messages.Default;
            DesignObject obj;
            var currentStage = Component.Stages[Stage];
            //Blocks.Add(Target.Global + Face, Brush);
            if (Controller.Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
            {
                //Tile.Types dropper;
                //if (currentStage.TryGetValue(TargetOld.Global + Face, out obj))
                if (currentStage.TryGetValue(this.Target.FaceGlobal, out obj))
                    Brush.Type = obj.Type;
                //    currentStage.Remove(Target.Global + Face);
            }
            else
            {
                //if (currentStage.TryGetValue(Target.Global + Face, out obj))
                if (!currentStage.Remove(this.Target.FaceGlobal))
                    currentStage[this.Target.FaceGlobal] = new DesignObject(Brush.Type, Brush.Variation, Brush.Orientation);
                //if (!currentStage.Remove(TargetOld.Global + Face))
                //    currentStage[TargetOld.Global + Face] = new DesignObject(Brush.Type, Brush.Variation, Brush.Orientation);
            }
            return Messages.Default;
        }

        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }


        //internal override void DrawWorld(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, IMap map, Camera camera)
        //{
        //    //sb.Draw(UI.UIManager.Highlight, new Rectangle(0, 0, UI.UIManager.Width, UI.UIManager.Height), null, Color.Lerp(Color.Black, Color.Transparent, 0.5f), 0, Vector2.Zero, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);

        //    foreach (var tile in Component.Stages[Stage])
        //    {
        //        float gx = tile.Key.X, gy = tile.Key.Y, z = tile.Key.Z;
        //        Rectangle spriteBounds = Block.Bounds;
        //        Rectangle tileBounds = camera.GetScreenBounds(gx, gy, z, spriteBounds);
        //        Vector2 screenLoc = new Vector2(tileBounds.X, tileBounds.Y);
        //        //Sprite tileSprite = Block.TileSprites[tile.Value.Type];
        //        //Rectangle sourceRect = tileSprite.SourceRects[tile.Value.Variation][tile.Value.Orientation];
        //        Rectangle sourceRect = Block.Registry[tile.Value.Type].Variations.First().Rectangle;
        //        sb.Draw(Map.TerrainSprites, screenLoc, sourceRect, Color.White * 0.5f, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
        //    }

        //    if (TargetOld == null)
        //        return;
        //    Sprite tarSprite = TargetOld["Sprite"]["Sprite"] as Sprite;
        //    Rectangle tarBounds = tarSprite.GetBounds();
        //    Rectangle scrBounds = camera.GetScreenBounds(TargetOld.Global + this.Face, tarSprite.GetBounds()); // posComp.GetScreenBounds(camera, sprComp); // 
        //    Vector2 scrLoc = new Vector2(scrBounds.X, scrBounds.Y);
        //    sb.Draw(Map.TerrainSprites, scrLoc, Block.TileHighlights[0][2], Color.White * 0.5f, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);

        //    //tarSprite = Block.TileSprites[Brush.Type];
        //    //tarBounds = tarSprite.GetBounds();
        //    tarBounds = Block.Registry[Brush.Type].Variations.First().Texture.Bounds;
        //    scrBounds = camera.GetScreenBounds(TargetOld.Global + this.Face, tarBounds);//tarSprite.GetBounds()); 
        //    sb.Draw(Map.TerrainSprites, scrLoc, tarSprite.SourceRects[Brush.Variation][Brush.Orientation], Color.White * 0.5f, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
        //}

        //public override void  Update()
        //{
        //    TargetOld = Controller.Instance.Mouseover.Object as GameObject;
        //    Face = Controller.Instance.Mouseover.Face;
        //    //base.HandleMouseMove(e);
        //}
    }
}
