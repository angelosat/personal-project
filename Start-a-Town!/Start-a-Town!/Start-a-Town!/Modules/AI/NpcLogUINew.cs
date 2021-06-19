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
        GameObject Agent;
        public NpcLogUINew(GameObject agent)
            //: base(new Rectangle(0, 0, 400, 200))
        {
            this.Agent = agent;
            Refresh(agent);
        }

        private void Refresh(GameObject agent)
        {
            this.Controls.Clear();
            //this.Controls.Clear();

            TableScrollable<AILog.Entry> table = new TableScrollable<AILog.Entry>(10, BackgroundStyle.TickBox)
                .AddColumn(null, "Time", (int)UIManager.Font.MeasureString("HH:mm:ss").X, (e) => new Label(e.Time.ToString("HH:mm:ss")), 0)
                .AddColumn(null, "Description", 400, (e) => new Label(e.Text), 0);
            //table.ItemStyle = BackgroundStyle.TickBox;
            //table.MaxVisibleItems = 10;
            var state = AIState.GetState(agent);
            var log = state.History;
            var entries = log.GetEntries();
            entries.Reverse();
            table.Build(entries);
            //foreach (var entry in entries)
            //{
            //    var lbl = new Label(entry.ToString()) { Location = this.Controls.BottomLeft };
            //    //this.Client.Controls.Add(lbl);
            //    this.Controls.Add(lbl);
            //}
            this.Controls.Add(table);
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
    }
}
