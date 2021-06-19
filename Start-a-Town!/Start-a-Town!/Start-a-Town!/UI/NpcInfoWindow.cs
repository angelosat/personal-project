using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.Components.AI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class NpcInfoWindow : Window
    {
        static NpcInfoWindow _Instance;
        public static NpcInfoWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new NpcInfoWindow();
                return _Instance;
            }
        }

        PanelList<GameObject> List_NpcList;
        PanelList<InteractionOld> List_NpcGoals;

        NpcInfoWindow()
        {
            this.Title = "Citizens";
            this.Movable = true;
            this.AutoSize = true;

            this.List_NpcList = new PanelList<GameObject>(Vector2.Zero, new Vector2(200, 300), foo => foo.Name);
            List_NpcList.SelectedItemChanged += new EventHandler<EventArgs>(List_NpcList_SelectedItemChanged);
            Init_NpcList();

            this.List_NpcGoals = new PanelList<InteractionOld>(List_NpcList.TopRight, new Vector2(200, 300), foo => foo.ToString());

            Client.Controls.Add(List_NpcList, List_NpcGoals);

            AIAutonomy.GoalsUpdated += new EventHandler<ParameterEventArgs>(AIAutonomy_GoalsUpdated);
            NpcComponent.NpcDirectoryChanged += new EventHandler<EventArgs>(NpcComponent_NpcDirectoryChanged);
        }

        void NpcComponent_NpcDirectoryChanged(object sender, EventArgs e)
        {
            Init_NpcList();
        }

        void AIAutonomy_GoalsUpdated(object sender, ParameterEventArgs e)
        {
            Init_NpcGoals();
        }

        void List_NpcList_SelectedItemChanged(object sender, EventArgs e)
        {
            Init_NpcGoals();
        }

        void Init_NpcList()
        {
            List_NpcList.Build(NpcComponent.NpcDirectory);
        }

        void Init_NpcGoals()
        {
            GameObject npc = List_NpcList.SelectedItem;
            if (npc == null)
                return;
            // Queue<Queue<Interaction>> goals = new Queue<Queue<Interaction>>();//(Queue<Queue<Interaction>>)npc["AI"]
            List<Queue<Queue<InteractionOld>>> goals = new List<Queue<Queue<InteractionOld>>>();

            throw new NotImplementedException();
            //npc.PostMessage(Message.Types.GetGoals, null, goals);

            List<InteractionOld> finalList = new List<InteractionOld>();
            foreach (Queue<InteractionOld> q in goals.First())
                foreach (InteractionOld inter in q)
                    finalList.Add(inter);
            List_NpcGoals.Build(finalList);
        }

        public override bool Show(params object[] p)
        {
            Init_NpcList();
            return base.Show(p);
        }
    }
}
