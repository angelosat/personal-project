using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Start_a_Town_.Components.AI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI
{
    class NpcLogUINew : GroupBox// ScrollableBox
    {
        Actor Agent;
        public NpcLogUINew()
        {

        }
        public NpcLogUINew(Actor agent)
            //: base(new Rectangle(0, 0, 400, 200))
        {
            this.Agent = agent;
            Refresh(agent);
        }
        public new void Refresh()
        {
            this.Refresh(this.Agent);
        }
        public void Refresh(Actor agent)
        {
            this.Agent = agent;
            this.Controls.Clear();

            //TableScrollable<AILog.Entry> table = new TableScrollable<AILog.Entry>(10, BackgroundStyle.TickBox)
            //    .AddColumn(null, "Time", (int)UIManager.Font.MeasureString("HH:mm:ss").X, (e) => new Label(e.Time.ToString("HH:mm:ss")), 0)
            //    .AddColumn(null, "Description", 400, (e) => new Label(e.Text.Wrap(400)), 0);

            //var state = AIState.GetState(agent);
            //var log = state.History;
            //var entries = log.GetEntries();
            //entries.Reverse();
            //table.Build(entries);

            var table = AILog.UI.GetGUI(this.Agent);

            //table.ItemStyle = BackgroundStyle.TickBox;
            //table.MaxVisibleItems = 10;
            //foreach (var entry in entries)
            //{
            //    var lbl = new Label(entry.ToString()) { Location = this.Controls.BottomLeft };
            //    //this.Client.Controls.Add(lbl);
            //    this.Controls.Add(lbl);
            //}

            this.Controls.Add(table);
            this.Validate(true);
        }
        
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                //case Components.Message.Types.JobComplete:
                //    this.Refresh(this.Agent);
                //    break;

                case Components.Message.Types.AILogUpdated:
                    this.Refresh(this.Agent);
                    break;

                default:
                    break;
            }
        }

        static NpcLogUINew Instance;
        internal static Window GetUI(Actor actor)
        {
            Window window;

            if (Instance == null)
            {
                Instance = new NpcLogUINew();
                window = new Window(Instance) { Movable = true, Closable = true };
            }
            else
                window = Instance.GetWindow();
            Instance.Tag = actor;
            window.Title = string.Format("{0} log", actor.Name);
            Instance.Refresh(actor);
            return window;
        }
        internal override void OnSelectedTargetChanged(TargetArgs target)
        {
            var actor = target.Object as Actor;
            if (!actor?.Equals(this.Tag) ?? false)
            {
                GetUI(actor);
            }
        }
    }
}
