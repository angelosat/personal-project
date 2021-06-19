using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.AI
{
    class ToolNpcControlNew : ControlTool
    {
        public GameObject Npc;
        PanelLabeled CommandsPanel;
        ListBox<ContextAction, Button> Commands;

        public ToolNpcControlNew(GameObject npc)
        {
            this.Npc = npc;
            this.CommandsPanel = new PanelLabeled("") { AutoSize = true };
            this.Commands = new ListBox<ContextAction, Button>(200, 100);
            //this.CommandsPanel.Controls.Add(this.Commands);
        }

        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;

            var global = this.Target.FinalGlobal;
            var capturedTarget = new TargetArgs(this.Target);// new TargetArgs(global);
            //capturedTarget.Global = global;
            var actions = this.Target.GetInteractions(Client.Instance);
            var contextActions = (from item in actions
                                  select new ContextAction(() => item.Key, () =>
                                  {
                                      byte[] data = Network.Serialize(w =>
                                      {
                                          w.Write((int)AI.AIPacketHandler.Channels.Command);
                                          w.Write(this.Npc.Network.ID);
                                          w.Write(item.Key);
                                          //this.Target.Write(w);
                                          capturedTarget.Write(w);
                                      });
                                      Net.Client.Instance.Send(PacketType.AI, data);
                                  })).ToList();

            contextActions.Add(new ContextAction(() => "Move", () =>
            {
                // something with globa
                // send command
                //var packet = new PacketEntityInteractionTarget(this.Npc.Network.ID, "Move", this.Target);
                byte[] data = Network.Serialize(w =>
                {
                    w.Write((int)AI.AIPacketHandler.Channels.Command);
                    w.Write(this.Npc.Network.ID);
                    w.Write("Move");
                    //this.Target.Write(w);
                    //capturedTarget.Write(w);
                    new TargetArgs(global).Write(w);
                });
                Net.Client.Instance.Send(PacketType.AI, data);
            }));

            this.Commands.Build(contextActions, i => i.Name(), (item, ctrl) =>
            {
                ctrl.LeftClickAction = () => item.Action();
            });
            this.CommandsPanel.Controls.RemoveAll(o => o != this.CommandsPanel.Label);
            this.CommandsPanel.Controls.Add(this.Commands);
            this.CommandsPanel.Show();
            this.CommandsPanel.Location = UIManager.Mouse;
            return Messages.Default;
        }

        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //this.CommandsPanel.Hide();
            //return Messages.Default;
            this.CommandsPanel.Hide();
            return Messages.Remove;
        }

        //public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        //{
        //    switch(e.KeyCode)
        //    {
        //        case System.Windows.Forms.Keys.Escape:
        //            return 
        //    }
        //}

        Icon Icon = new Icon(UI.UIManager.Icons32, 2, 32);
        internal override void DrawUI(SpriteBatch sb, Camera camera)
        {
            base.DrawUI(sb, camera);

            Icon.Draw(sb, UI.UIManager.Mouse);
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            {
                var icondelete = new Icon(UI.UIManager.Icons16x16, 0, 16);
                icondelete.Draw(sb, UI.UIManager.Mouse + new Vector2(Icon.SourceRect.Width / 2, 0));
            }
            //sb.Draw(Icon.SpriteSheet, UI.UIManager.Mouse, Icon.SourceRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }
    }
}
