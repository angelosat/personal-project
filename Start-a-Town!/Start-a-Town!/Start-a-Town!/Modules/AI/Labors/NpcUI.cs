using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Components.AI;

namespace Start_a_Town_.AI
{
    class NpcUI : GroupBox
    {
        GameObject Npc;
        NeedsUI Needs;

        public NpcUI(GameObject npc)
        {
            var panelNeeds = new Panel() { AutoSize = true };
            this.Npc = npc;
            this.Needs = new NeedsUI(npc);
            panelNeeds.Controls.Add(this.Needs);



            var panelBtns = new Panel() { AutoSize = true, Location = panelNeeds.BottomLeft };
            var winlog = new NpcLogUINew(this.Npc).ToWindow(Npc.Name + "'s Log");
            var btnlog = new Button("Log", panelNeeds.ClientSize.Width) { LeftClickAction = ()=>ShowLog(winlog) };

            var winPersonality = AIState.GetState(npc).Personality.GetUI().ToWindow(Npc.Name + "'s Personality");
            var btnpersonality = new Button("Personality", panelNeeds.ClientSize.Width) { Location = btnlog.BottomLeft, LeftClickAction = () => winPersonality.ToggleSmart() };

            panelBtns.AddControls(btnlog, btnpersonality);

            this.AddControls(panelNeeds, panelBtns);
        }

        private void ShowLog(Window winlog)
        {
            winlog.ToggleSmart();
        }

    }
}
