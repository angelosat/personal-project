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
    class NpcLogUI : ScrollableBox
    {
        GameObject Agent;
        public NpcLogUI(GameObject agent)
            : base(new Rectangle(0, 0, 400, 200))
        {
            this.Agent = agent;
            Refresh(agent);
        }

        private void Refresh(GameObject agent)
        {
            this.Client.Controls.Clear();
            //this.Controls.Clear();

            var state = AIState.GetState(agent);
            var log = state.History;
            var entries = log.GetEntries();
            entries.Reverse();
            foreach (var entry in entries)
            {
                var lbl = new Label(entry.ToString()) { Location = this.Client.Controls.BottomLeft };
                //this.Client.Controls.Add(lbl);
                this.Add(lbl);

            }
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.JobComplete:
                    this.Refresh(this.Agent);
                    break;

                default:
                    break;
            }
        }
    }
}
