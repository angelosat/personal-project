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
    class ObjectSpawnTool : ControlTool
    {
        GameObject Entity;

        public ObjectSpawnTool(GameObject entity)
        {
            this.Entity = entity;
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
         
            var position = this.Target.Global + this.Target.Face + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : this.Target.Precise);
            switch (this.Target.Type)
            {
                case TargetType.Position:
                    //Net.Client.AddObject(this.Entity.Clone(), position);
                    SpawnEntity();
                    break;

                case TargetType.Entity:
                    if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                    {
                        Net.Client.RemoveObject(Target.Object);
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
            var position = this.Target.Global + this.Target.Face * new Vector3(1,1,blockHeight) + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : this.Target.Precise);
            //var position = this.Target.Global + this.Target.Face + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : this.Target.Precise);
            byte[] data = Network.Serialize(w =>
            {
                this.Entity.Write(w);
                //this.Target.Write(w);
                new TargetArgs(position).Write(w);
            });
            Net.Client.Instance.Send(PacketType.SpawnEntity, data);
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
                byte[] data = Net.Network.Serialize(w =>
                {
                    this.Entity.Write(w);
                    //w.Write(this.Target.Slot.Parent.Network.ID);
                    //w.Write((int)this.Target.Slot.ID);
                    //new TargetArgs(this.Target.Slot).Write(w);
                    this.Target.Write(w);
                });
                //Net.Client.Instance.Send(PacketType.SpawnChildObject, data);
                Net.Client.Instance.Send(PacketType.SpawnEntity, data);
            }
        }

        private void IncreaseQuantity()
        {
            byte[] data = Net.Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                //w.Write(this.Entity.Network.ID);
                //w.Write(this.Target.Slot.Object.Network.ID);
                w.Write(this.Target.Object.Network.ID);
                w.Write(this.Entity.StackSize);
            });
            Net.Client.Instance.Send(PacketType.IncreaseEntityQuantity, data);
        }

        public override ControlTool.Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }

        internal override void DrawAfterWorld(MySpriteBatch sb, IMap map, Camera cam)
        {
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            {
                Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
                //   sb.Draw(UIManager.Icons16x16, loc + Vector2.UnitX * 8, new Rectangle(0, 0, 16, 16), Color.White);
                return;
            }
            if (this.Target == null)
                return;
            //this.Target.ToConsole();
            var sprite = this.Entity.GetSprite();
            var blockHeight = Block.GetBlockHeight(map, this.Target.Global);
            var global = this.Target.Global + this.Target.Face * new Vector3(1,1,blockHeight) + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : this.Target.Precise);
            //var global = this.Target.FaceGlobalBlockHeight + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : this.Target.Precise);

            var pos = cam.GetScreenPositionFloat(global);
            //sprite.Draw(sb, global, cam, Color.White * 0.5f, 0, Microsoft.Xna.Framework.Graphics.SpriteEffects.None);

            Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;
            Game1.Instance.GraphicsDevice.Textures[1] = Sprite.Atlas.DepthTexture;
            //sprite.Draw(sb, pos, Color.White * 0.5f, 0, sprite.Origin, cam.Zoom, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, global.GetDrawDepth(Engine.Map, cam));
            //this.Entity.Body.DrawTree(this.Entity, sb, pos, Color.White * 0.5f, this.Entity.Body.Joint + this.Entity.Body.Offset, 0, cam.Zoom, SpriteEffects.None, global.GetDrawDepth(Engine.Map, cam));


            var body = this.Entity.Body;
            pos += body.OriginGroundOffset * cam.Zoom;
            //body.DrawTree(this.Entity, sb, pos, Color.White, Color.White, Color.White, Color.Transparent, body.RestingFrame.Offset, 0, cam.Zoom, SpriteEffects.None, 0.5f, global.GetDrawDepth(Engine.Map, cam));
            // TODO: fix difference between tint and material in this drawtree method
            var tint = Color.White * .5f;// Color.Transparent;
            body.DrawTree(this.Entity, sb, pos, Color.White, Color.White, tint, Color.Transparent, 0, cam.Zoom, 0, SpriteEffects.None, 0.5f, global.GetDrawDepth(Engine.Map, cam));
        }
    }
}
