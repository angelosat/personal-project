using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.Components
{
    public class FarmingComponent : EntityComponent// : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "Farming"; }
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e) // GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (msg)
            {
                case Message.Types.Till:
                    // TODO: if i make it air before making it another block, then the lightning could process it inbetween states resulting in a dark invisible block
                 //   parent.Remove();

                   // GameObject.Create(GameObject.Types.Farmland).Spawn(parent.Map, parent.Global);
                    //e.Network.InstantiateObject(GameObject.Create(GameObject.Types.Farmland).SetGlobal(parent.Global));
                    e.Network.Spawn(GameObject.Create(GameObject.Types.Farmland), parent.Global);
                    return true;


                //case Message.Types.Query:
                //    Query(parent, e);
                //    return true;

                default:
                    return true;

            }
            return true;
        }

        //public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        //{
        ////    List<Interaction> list = e.Parameters[0] as List<Interaction>;
        //    list.Add(new InteractionOld(new TimeSpan(0, 0, 1), Message.Types.Till, parent, "Till", "Tilling", stat: Stat.Tilling, 
        //        cond: new ConditionCollection(
        //               new Condition((actor, target) => ControlComponent.HasAbility(actor, Message.Types.Till), "I need a tool to " + Message.Types.Till.ToString().ToLower() + " with.", //"Requires: " + Message.Types.Tilling,
        //                   new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Till), PlanType.FindInventory))
        //                   )));
        //}

        public override object Clone()
        {
            FarmingComponent comp = new FarmingComponent();
            foreach (KeyValuePair<string, object> parameter in Properties)
                comp[parameter.Key] = parameter.Value;
            return comp;
        }
        //public override string GetWorldText(GameObject actor)
        //{
        //    return "Right click: " + Stat.Tilling.Name;// +" " + GetProperty<float>(Stat.Value.Name);
        //}
    }
}
