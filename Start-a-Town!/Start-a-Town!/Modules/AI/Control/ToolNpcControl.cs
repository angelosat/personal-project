using System;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.AI
{
    [Obsolete]
    class ToolNpcControl : ControlTool
    {
        public GameObject Npc;
        PanelLabeled CommandsPanel;
        ListBox<ContextAction, Button> Commands;

        public ToolNpcControl(GameObject npc)
        {
            this.Npc = npc;
            this.CommandsPanel = new PanelLabeled("") { AutoSize = true };
            this.Commands = new ListBox<ContextAction, Button>(200, 100);
        }

        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;

            var global = this.Target.FinalGlobal;
            var capturedTarget = this.Target.Clone();

            var actions = this.Target.GetInteractions(Client.Instance);
            var contextActions = (from item in actions
                                  select new ContextAction(() => item.Key, () =>
                                  {
                                      byte[] data = Network.Serialize(w =>
                                      {
                                          w.Write((int)AI.AIPacketHandler.Channels.Command);
                                          w.Write(this.Npc.RefID);
                                          w.Write(item.Key);
                                          capturedTarget.Write(w);
                                      });
                                      Client.Instance.Send(PacketType.AI, data);
                                  })).ToList();

            contextActions.Add(new ContextAction(() => "Move", () =>
            {
                // something with globa
                // send command
                byte[] data = Network.Serialize(w =>
                {
                    w.Write((int)AI.AIPacketHandler.Channels.Command);
                    w.Write(this.Npc.RefID);
                    w.Write("Move");
                    new TargetArgs(this.Npc.Map, global).Write(w);
                });
                Client.Instance.Send(PacketType.AI, data);
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
            this.CommandsPanel.Hide();
            return Messages.Remove;
        }

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
        }
    }
}
