using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class QuestGiverProperties
    {
        public int Giver { get; private set; }
        //public HashSet<int> QuestsOffered;
        public int NextQuestReceiver { get; private set; } = -1;

        //public QuestGiverProperties(Actor giver)
        //{
        //    this.Giver = giver.InstanceID;
        //}
        public QuestGiverProperties(int giverID)
        {
            this.Giver = giverID;
        }
        internal void HandleReceiver(Actor actor)
        {
            if (this.NextQuestReceiver != -1)
                throw new Exception();
            this.NextQuestReceiver = actor.RefID;
        }
        internal void RemoveReceiver(Actor actor)
        {
            if (this.NextQuestReceiver != actor.RefID)
                throw new Exception();
            this.NextQuestReceiver = -1;
        }
        internal void RemoveReceiver()
        {
            if (this.NextQuestReceiver == -1)
                throw new Exception();
            this.NextQuestReceiver = -1;
        }
        public int GetNextQuestReceiverID()
        {
            return this.NextQuestReceiver;// == -1 ? null : this.Giver.Net.GetNetworkObject(this.NextQuestReceiver) as Actor;
        }
    }
}
