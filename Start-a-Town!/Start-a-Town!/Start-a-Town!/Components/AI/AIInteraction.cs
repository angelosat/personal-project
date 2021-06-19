using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class AIInteraction : Behavior
    {
        GameObject Target { get { return (GameObject)this["Target"]; } set { this["Target"] = value; } }
        Interaction Interaction { get { return (Interaction)this["Interaction"]; } set { this["Interaction"] = value; } }
        float NeedBefore;
        public override string Name
        {
            get
            {
                return "Performing: " + Interaction.ToString();
            }
        }

        public AIInteraction()
        {
            this.Target = null;
            this.Interaction = null;
        }

        public AIInteraction(GameObject target, Interaction interaction)
        {
            this.Target = target;
            this.Interaction = interaction;
        }

        public override Behavior Initialize(GameObject parent)
        {
            //parent.HandleMessage(Message.Types.Begin, parent, Target, Interaction.Message);
            //if (parent["Control"]["Task"] == null)
            //    return new AIIdle();

           // NeedBefore = (float)((Personality)parent["AI"]["Personality"]).Needs[Interaction.Need.Name].Value;
            NeedBefore = (float)((NeedCollection)parent["Needs"]["Needs"])[Interaction.Need.Name].Value;
            parent.HandleMessage(Message.Types.Begin, parent, Target, Interaction.Message);
            
            return this;
        }

        public override BehaviorState Execute(GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            GameObject target = p[0] as GameObject;
            Interaction interaction = p[1] as Interaction;
            if (interaction == null)
                return BehaviorState.Fail;
            Vector3 difference = (target.Global - parent.Global);
            if (difference.Length() > 10) // if it's too far, finish the behavior
                return BehaviorState.Fail;
            if (difference.Length() > 1)
            {
                difference.Normalize();
                difference.Z = 0;
                parent.HandleMessage(Message.Types.Move, parent, difference, 1f);
                return BehaviorState.Running;
            }
            if (parent["Control"]["Task"] == null)
                parent.HandleMessage(Message.Types.Begin, parent, target, interaction.Message);
           // NeedBefore = (float)((Personality)parent["AI"]["Personality"]).Needs[interaction.Need.Name].Value;
            NeedBefore = (float)((NeedCollection)parent["Needs"]["Needs"])[Interaction.Need.Name].Value;
         //   Dictionary<string, object> results = new Dictionary<string,object>();
            parent.HandleMessage(Message.Types.Perform);//, parent, results);
            if (parent["Control"]["Task"] == null) //the interaction has finished
            {
              //  float needAfter = (float)((Personality)parent["AI"]["Personality"]).Needs[interaction.Need.Name].Value;
                float needAfter = (float)((NeedCollection)parent["Needs"]["Needs"])[Interaction.Need.Name].Value;
                float reward = needAfter - NeedBefore;
                MemoryEntry memory;
                if (!knowledge.Objects.TryGetValue(target, out memory))
                    knowledge.Objects[target] = new MemoryEntry(target, 100, 100, 1, new Need(interaction.Need.Name, reward));
                else
                {
                    Need need;
                    if (memory.Needs.TryGetValue(interaction.Need.Name, out need))
                        need.Value = reward;
                    else
                        memory.Needs[interaction.Need.Name] = new Need(interaction.Need.Name, reward);
                    //memory.Needs[interaction.Need.Name].Value = reward;
                }

                return BehaviorState.Success;
            }
            return BehaviorState.Running;
        }

        //public override bool Execute(GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        //{
        //    //if (Child != null)
        //    //{
        //    //    if (!Child.Execute(parent, personality, memory))
        //    //        return false;
        //    //    Child = null;
        //    //}

        //    Vector3 difference = (Target.Global - parent.Global);
        //    if (difference.Length() > 1)
        //    {
        //        difference.Normalize();
        //        difference.Z = 0;
        //        parent.HandleMessage(Message.Types.Move, parent, difference, 1f);
        //        return false;
        //    }
        //    //if (parent["Control"]["Task"] == null)
        //    ////parent["AI"]["Current"] = new AIIdle();
        //    //{
        //    //    NeedBefore = (float)personality.Needs[Interaction.Need].Value;
        //    //    parent.HandleMessage(Message.Types.Begin, parent, Target, Interaction.Message);
        //    //    //return false;
        //    //}
        //   // if (!
        //    //if (parent["Control"]["Task"] != null)
        //    //{
        //        parent.HandleMessage(Message.Types.Perform);
        //        return false;
        //  //  }
        // //   return true;

        //    //if (parent["Control"]["Task"] == null)
        //    //    parent["AI"]["Current"] = new AIIdle();
        // //   return true;
        //}

        public override bool HandleMessage(GameObject parent, GameObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.NoInteraction:
               //     Child = null;
                    Finalize(parent);
                    return true;

                case Message.Types.InteractionFinished:
                  //  parent["AI"]["Current"] = new AIIdle();
                 //   Child = null;
                    //float needAfter = (float)((Personality)parent["AI"]["Personality"]).Needs[Interaction.Need.Name].Value;
                    float needAfter = (float)((NeedCollection)parent["Needs"]["Needs"])[Interaction.Need.Name].Value;
                    float reward = needAfter - NeedBefore;
                 //   ((Memory)parent["AI"]["Memory"]).Add(new MemoryEntry(Target, reward, 30, 1, Interaction.Need));
                    return true;
                case Message.Types.InteractionFailed:
                  //  parent["AI"]["Current"] = new AIIdle();
              //      Child = null;
                    //Knowledge knowledge = parent["AI"].GetProperty<Knowledge>("Knowledge");
                    //MemoryEntry memory = knowledge.Find(foo => foo.Object == e.Sender);
                    //if (memory == null)
                    //    memory = new MemoryEntry(e.Sender, 100, 100, 1);
                    return true;
                case Message.Types.NeedItem:
                //    Child = new AIFindObject(32, (Predicate<GameObject>)e.Parameters[0], Message.Types.PickUp).Initialize(parent);
                    return true;
                default:
                    return false;
            }
        }

        void FindObject()
        {

        }
    }
}
