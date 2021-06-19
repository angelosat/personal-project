using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.AI
{
    class AIInteraction : Behavior
    {
        public override string Name
        {
            get
            {
                return "Performing interaction";
            }
        }

        //public AIInteraction(GameObject parent, Interaction interaction)
        //{
        //    parent.HandleMessage(Message.Types.Begin, parent, Target, Interaction.Message);
        //}
    }
}
