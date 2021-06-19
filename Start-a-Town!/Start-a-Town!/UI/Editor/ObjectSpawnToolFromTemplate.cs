using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.UI.Editor
{
    class ObjectSpawnToolFromTemplate : ToolManagement// ControlTool
    {
        GameObject Entity;
        int TemplateID;
        public ObjectSpawnToolFromTemplate()
        {

        }
       
        public ObjectSpawnToolFromTemplate(GameObject entity, int templateID)
        {
            this.Entity = entity;
            this.TemplateID = templateID;
        }
        public override void Update()
        {
            base.Update();
            if(this.Target != null)
                this.Target.Precise = InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? this.Target.Precise : Vector3.Zero;
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //e.Handled = true;
            return Messages.Default;
        }
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //e.Handled = true;
            if (this.Target == null)
                return Messages.Default;

            var position = this.Target.Global + this.Target.Face + GetPrecise();// (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : this.Target.Precise);
            switch (this.Target.Type)
            {
                case TargetType.Position:
                    SpawnEntity();
                    break;

                case TargetType.Entity:
                    if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                    {
                        PacketEntityRequestDispose.Send(Client.Instance, Target.Object.RefID);
                        //Client.RemoveObject(Target.Object);
                    }
                    else
                    {
                        if (Entity.GetID() != this.Target.Object.GetID())
                            break;
                        // increase entity's quantity
                        IncreaseQuantity();
                    }
                    break;

                case TargetType.Slot:
                    //spawn entity as a child
                    SpawnChild();
                    break;

                default:
                    break;
            }
            return Messages.Default;
        }

        private void SpawnEntity()
        {
            var blockHeight = Block.GetBlockHeight(Engine.Map, this.Target.Global);
            var position = this.Target.Global + this.Target.Face * new Vector3(1,1,blockHeight) + GetPrecise();
            //PacketEntityRequestSpawn.Send(Client.Instance, this.Entity.ID, new TargetArgs(position));
            PacketEntityRequestSpawn.SendTemplate(Client.Instance, this.TemplateID, new TargetArgs(position));

        }

        private void SpawnChild()
        {
            if (this.Target.Slot.HasValue)
            {
                if (this.Target.Slot.Object.GetID() != this.Entity.GetID())
                    return;
                else
                {
                    // increase entity's quantity
                    IncreaseQuantity();
                    return;
                }
            }
            else
            {
                byte[] data = Network.Serialize(w =>
                {
                    this.Entity.Write(w);
                    this.Target.Write(w);
                });
                Client.Instance.Send(PacketType.SpawnEntity, data);
            }
        }

        private void IncreaseQuantity()
        {
            //byte[] data = Network.Serialize(w =>
            //{
            //    w.Write(this.Target.Object.InstanceID);
            //    //w.Write(this.Entity.StackSize);, InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey)
            //    w.Write(InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? 5 : 1);
            //});
            //Client.Instance.Send(PacketType.IncreaseEntityQuantity, data);

            var obj = this.Target.Object;
            obj.SyncSetStackSize(obj.StackSize + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? 5 : 1));
        }

        public override ControlTool.Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Default;
        }
        internal override void DrawAfterWorld(MySpriteBatch sb, IMap map)
        {
            var cam = map.Camera;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            {
                Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
                return;
            }
            if (this.Target == null || this.Target.Type == TargetType.Null)
                return;
            this.Entity.DrawPreview(sb, cam, this.Target, InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey));
            //return;
            //var blockHeight = Block.GetBlockHeight(map, this.Target.Global);
            //var global = this.Target.Global + this.Target.Face * new Vector3(1, 1, blockHeight) + (GetPrecise());
            //this.Entity.DrawPreview(sb, cam, global);
            //return;
            ////var global = this.Target.FaceGlobalBlockHeight + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : this.Target.Precise);

            //var pos = cam.GetScreenPositionFloat(global);
            ////sprite.Draw(sb, global, cam, Color.White * 0.5f, 0, Microsoft.Xna.Framework.Graphics.SpriteEffects.None);

            //Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            //fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            //fx.CurrentTechnique.Passes["Pass1"].Apply();
            //Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;
            //Game1.Instance.GraphicsDevice.Textures[1] = Sprite.Atlas.DepthTexture;
            ////sprite.Draw(sb, pos, Color.White * 0.5f, 0, sprite.Origin, cam.Zoom, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, global.GetDrawDepth(Engine.Map, cam));
            ////this.Entity.Body.DrawTree(this.Entity, sb, pos, Color.White * 0.5f, this.Entity.Body.Joint + this.Entity.Body.Offset, 0, cam.Zoom, SpriteEffects.None, global.GetDrawDepth(Engine.Map, cam));


            //var body = this.Entity.Body;
            //pos += body.OriginGroundOffset * cam.Zoom;
            ////body.DrawTree(this.Entity, sb, pos, Color.White, Color.White, Color.White, Color.Transparent, body.RestingFrame.Offset, 0, cam.Zoom, SpriteEffects.None, 0.5f, global.GetDrawDepth(Engine.Map, cam));
            //// TODO: fix difference between tint and material in this drawtree method
            //var tint = Color.White * .5f;// Color.Transparent;
            //body.DrawGhost(this.Entity, sb, pos, Color.White, Color.White, tint, Color.Transparent, 0, cam.Zoom, 0, SpriteEffects.None, 0.5f, global.GetDrawDepth(Engine.Map, cam));
        }

        private Vector3 GetPrecise()
        {
            return InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? this.Target.Precise : Vector3.Zero;

            //return InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : this.Target.Precise;
        }
        protected override void WriteData(System.IO.BinaryWriter w)
        {
            w.Write(this.TemplateID);
            //w.Write(this.Entity.ID);
        }
        protected override void ReadData(System.IO.BinaryReader r)
        {
            this.TemplateID = r.ReadInt32();
            this.Entity = GameObject.Templates[this.TemplateID];
            //this.Entity = GameObject.Objects[r.ReadInt32()];
        }

        internal override void DrawAfterWorldRemote(MySpriteBatch sb, IMap map, Camera camera, Net.PlayerData player)
        {
            this.Entity.DrawPreview(sb, camera, player.Target, true);
        }
    }
}
