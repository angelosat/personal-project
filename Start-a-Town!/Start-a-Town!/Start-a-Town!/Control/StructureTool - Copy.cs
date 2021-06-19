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
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Components;

namespace Start_a_Town_.Control
{
    public class StructureTool : ControlTool
    {
        #region Singleton
        static StructureTool _Instance;
        public static StructureTool Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new StructureTool();
                return _Instance;
            }
        }
        #endregion

        public Vector3 Global;
        public bool Valid;
        public GameObject.Types Type;
        public GameObject Object, Target;
        public Interaction Interaction;
        StructureTool()
        {
        }
        StructureTool(GameObject obj)
        {
            Object = obj;

            Icon = obj.GetGui().GetProperty<Icon>("Icon");
        }

        public StructureTool Initialize(GameObject obj)
        {
            Object = obj;
            return this;
        }

        //internal override void KeyDown(InputState e)
        //{
        //    DefaultTool.MoveKeys(e);
        //}


        public override ControlTool.Messages  MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {

            if (!Controller.Instance.MouseoverNext.TryGet<GameObject>(out Target))
                return Messages.Default;

            BlockComponent tileComp;
            if (!Target.TryGetComponent<BlockComponent>("Physics", out tileComp))
                return Messages.Default;

            Global = Target.Global + Controller.Instance.Mouseover.Face;

            
            
                Blueprint bp = WorkbenchComponent.Blueprints.Skip(1).ToList().Find(foo => (foo["Blueprint"]["Blueprint"] as Blueprint).ProductID == Object.ID)["Blueprint"]["Blueprint"] as Blueprint;

                Target = GameObjectDb.Construction;
                throw new NotImplementedException();
                //Target.PostMessage(Message.Types.SetBlueprint, null, bp, (int)Object["Sprite"]["Variation"], (int)Object["Sprite"]["Orientation"]);

                throw new Exception("Obsolete position handling");
            Interaction inter = new Interaction(TimeSpan.Zero, Message.Types.Default, Target);
            if (Controller.Input.GetKeyDown(System.Windows.Forms.Keys.LMenu))
                JobBoardComponent.Enqueue(Job.Create(inter));
            else
                throw new NotImplementedException();
                //Player.Actor.PostMessage(Message.Types.BeginInteraction, null, inter);
            return Messages.Default;
            

            // create an AIR object to the corresponding face to act as the target, to fix an issue with not being in range with the target block
            Target = GameObject.Create(GameObject.Types.Air);
            //Target.Global = Global;
            throw new Exception("Obsolete position handling");
            //base.OnMouseLeft(held);
            //Interaction
                inter = new Interaction(new TimeSpan(0, 0, 0, 1), Message.Types.Structure, Target, "Construct",   //TimeSpan.Zero
                        effect: new List<NeedEffect>() { new NeedEffect("Work", 20) }, // new InteractionEffect("Work"),
                        cond: new ConditionCollection(
                              new Condition((actor, target) => true,//(GameObject.Objects[Type]["Blueprint"]["Blueprint"] as Blueprint)[0].Condition,
                                 // new Precondition("Equipped", i => true, Components.AI.PlanType.FindInventory))));
                                new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), Components.AI.PlanType.FindInventory))));
            //InteractionCondition badCond;
            ////Interaction.FindMatch(Target, Player.Actor, Message.Types.Structure, out inter);
            //if (!inter.ActorConditions.TryFinalCondition(Player.Actor, out badCond))
            List<Condition> badConds = new List<Condition>();
            if (!inter.TryConditions(Player.Actor, Target, badConds))
            {
                NotificationArea.Write(badConds.First().ErrorMessage);
                return Messages.Default;
            }
            Interaction = inter;
            throw new NotImplementedException();
            //Player.Actor.PostMessage(Message.Types.BeginInteraction, null, inter, Controller.Instance.Mouseover.Face, Object);
            //return Messages.Default;
            return Messages.Remove;
        }

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            Target = null;
            Interaction = null;
            return Messages.Default;
        }

        //public override Messages OnMouseLeft(bool held)
        //{
        //    if (!Controller.Instance.Mouseover.TryGet<GameObject>(out Target))
        //        return Messages.Default;

        //    Position targetPos = Target.GetComponent<MovementComponent>("Position").GetProperty<Position>("Position");

        //    TileComponent tileComp;
        //    if (!Target.TryGetComponent<TileComponent>("Physics", out tileComp))
        //        return Messages.Default;

        //    Vector3 offSet = Controller.Instance.MouseoverNext.Face;

        // //   Chunk.AddObject(Object, targetPos.Global + offSet);
        //    Interaction inter;
        //    Interaction.FindMatch(Target, Player.Actor, Message.Types.Construct, out inter);
        //    Player.Actor.HandleMessage(Message.Types.BeginInteraction, null, inter, Controller.Instance.Mouseover.Face);
        //    return Messages.Remove;
        //}

        public override Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            Rooms.Ingame.Instance.ToolManager.ActiveTool = null;
            return base.MouseRightDown(e);
        }

        //public override void HandleInput(InputState e)
        //{
        //    if (InputState.IsKeyDown(System.Windows.Forms.Keys.LButton))
        //    {
        //        if (Player.Actor["Control"]["Task"] != null)
        //            //        ToolManager.Instance.ActiveTool = null;
        //            Player.Actor.HandleMessage(Components.Message.Types.Perform, null);
        //    }
        //    else
        //    {
        //        Player.Actor.HandleMessage(Message.Types.Interrupt);
        //        Target = null;
        //        //  ToolManager.Instance.ActiveTool = null;
        //    }
        //}

        public override void HandleInput(InputState e)
        {
            if (Interaction == null)
            {
             //   ToolManager.Instance.ActiveTool = null;
                return;
            }
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.LButton))
            {
                if (Player.Actor["Control"]["Task"] == null)
                    ToolManager.Instance.ActiveTool = null;
                //Vector3 difference = (Interaction.Source.Global - Player.Actor.Global);
                //float length = difference.Length();
              //  if (Interaction.Range >= 0)
                if (!Interaction.Range(Player.Actor, Interaction.Source))//length))
                {
                    Vector3 difference = (Interaction.Source.Global - Player.Actor.Global);
                    float length = difference.Length();

                        difference.Normalize();
                        difference.Z = 0;
                        throw new NotImplementedException();
                        //Player.Actor.PostMessage(Message.Types.MoveToObject, Player.Actor, Interaction.Source, Interaction.Range, 1f);
                        return;
     
                }
                Player.Actor.PostMessage(Components.Message.Types.Perform);
            }
            else
            {
                Player.Actor.PostMessage(Message.Types.Interrupt);
             //   ToolManager.Instance.ActiveTool = null;
            }
        }

        internal override void DrawWorld(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        {
            if (Controller.Instance.ksCurrent.IsKeyDown(Keys.LeftControl))
                return;

            GameObject tar;
            if (Target != null)
                tar = Target;
            else
            if (!Controller.Instance.Mouseover.TryGet<GameObject>(out tar))
                return;

            Vector3 f = Controller.Instance.Mouseover.Face;
            int rx, ry;
            Camera cam = new Camera(camera.Width, camera.Height);
            cam.Rotation = -camera.Rotation;
            Coords.Rotate(cam, f.X, f.Y, out rx, out ry);
            Vector3 global = tar.Global + new Vector3(rx, ry, f.Z);
            //Vector3 global = tar.Global + new Vector3(f.R, f.G, f.B);
            Sprite sprite = (Sprite)Object["Sprite"]["Sprite"];
            Sprite tileSprite = tar["Sprite"]["Sprite"] as Sprite;
            Rectangle tileBounds = tileSprite.GetBounds();
            Rectangle bounds = camera.GetScreenBounds(global, sprite.GetBounds()); // posComp.GetScreenBounds(camera, sprComp); // 
            Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
            Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);
            Valid = true;
            if (Object.Components.ContainsKey("Multi"))
                foreach (Vector3 vector in Object["Multi"].GetProperty<Dictionary<Vector3, Sprite>>("MultiTile").Keys)
                {
                    Vector3 vg = global + vector;
                    Rectangle scrBounds = camera.GetScreenBounds(vg, tileBounds);
                    Vector2 scr = new Vector2(scrBounds.X, scrBounds.Y);
                    // bool check = (Position.GetCell(vg).TileType == Tile.Types.Air && Position.GetCell(vg - new Vector3(0,0,1)).Solid);
                    Cell cell;
                    bool check;
                    if (!Position.TryGetCell(Engine.Map, vg, out cell))
                        check = false;
                    else
                        check = (cell.Type == Block.Types.Air && (vg - new Vector3(0, 0, 1)).IsSolid(Engine.Map));//Position.GetCell(Engine.Map, vg - new Vector3(0, 0, 1)).Solid);
                    Color c = check ? Color.White : Color.Red;
                    Valid &= check;
                    //  sb.Draw(Map.TerrainSprites, scr, Tile.TileHighlights[0][0], c * 0.5f, 0, new Vector2(0, -tileSprite.Origin.Y + 16), camera.Zoom, SpriteEffects.None, 0);
                }

            //sb.Draw(sprComp.Sprite.Texture, screenLoc,
            //    sprComp.Sprite.SourceRect[sprComp.Variation][sprComp.GetOrientation(camera)], new Color(255, 255, 255, 127),
            int v = (int)Object["Sprite"]["Variation"];
            int o = (int)Object["Sprite"]["Orientation"];
            int oLength = sprite.SourceRects[v].Length;
            MultiTile2Component multi;
            if (Object.TryGetComponent<MultiTile2Component>("Multi", out multi))
                multi.DrawPreview(sb, camera, global, (int)Object["Sprite"]["Orientation"]);
            else
            {
                SpriteComponent spriteComp;
                if (Object.TryGetComponent<SpriteComponent>("Sprite", out spriteComp))
                    spriteComp.DrawPreview(sb, camera, global.Round(), (int)Object["Sprite"]["Orientation"]);
            }
            //sb.Draw(sprite.Texture, screenLoc,
            //        sprite.SourceRect[v][SpriteComponent.GetOrientation(o, camera, oLength)], Color.White * 0.5f,//new Color(255, 255, 255, 127),
            //        0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
            Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);

            if (!SpriteComponent.HasOrientation(Object))
                return;
            string
                text = "Press 'Q' to change orientation";
            Vector2
                textSize = UI.UIManager.Font.MeasureString(text);
            UI.UIManager.DrawStringOutlined(sb, text,
                new Vector2(UI.UIManager.Width / 2 - (int)(textSize.X / 2), UI.UIManager.Height / 4), Vector2.Zero);

        }
    }
}
