using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public class DiggableComponent : Component// : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "Diggable"; }
        }
        InteractionOld Action { get { return (InteractionOld)this["Action"]; } set { this["Action"] = value; } }

        public DiggableComponent(InteractionOld action, float maxHealth = 1, float initialPercentage = 1)
        {
            Properties.Add("Health", maxHealth * initialPercentage);
            Properties.Add("Max Health", maxHealth);
            Properties.Add("Skill", GameObjectDb.SkillMining);
            Action = action;
        }

        public override string GetTooltipText()
        {
            return "Left click: Dig";
        }

        public bool Break(GameObject self, Position pos)
        {
         //   Map.RemoveObject(self);

            return true;
        }

        public override object Clone()
        {
            DiggableComponent comp = new DiggableComponent(Action, (float)this["Max Health"], 1f );
           // foreach (KeyValuePair<string, object> parameter in Properties)
           //     comp[parameter.Key] = parameter.Value;
            return comp;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (e.Type)// (msg == Message.Types.Attack)
            {
                
                case Message.Types.Action:
                    //   Log.Enqueue(Log.EntryTypes.Death, sender, parent);
                    InteractionOld action = e.Parameters[0] as InteractionOld;
                    if (action.Message != Action.Message) //(Message.Types)this["Action"])
                        return false;
                    Break(parent, parent.Transform.Position);
                    throw new NotImplementedException();
                    break;
            
                default:
                    if (e.Type == Action.Message)
                    {
                        Break(parent, parent.Transform.Position);
                        throw new NotImplementedException();
                    }
                    break;
            }
            return false;
        }

        public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        {
            //List<Interaction> actions = e.Parameters[0] as List<Interaction>;
            //if (e.Parameters.Length > 1)
            //{
            //    Message.Types filter = (Message.Types)e.Parameters[1];
            //    if (Action.Message == filter)
            //        actions.Add(Action);
            //}
            //else

            //actions.Add(Action);
           // List<Interaction> list = e.Parameters[0] as List<Interaction>;
            list.Add(new InteractionOld(Action.Time, message: Action.Message, name: Action.Name, verb: Action.Verb, source: parent));
        }

        void Dig(GameObject actor)
        {
        }

        //public override void GetTooltip(GameObject parent, Tooltip tooltip)
        //{
        //    GroupBox box = new GroupBox();
        //    //Bar bar = new Bar(new Vector2(0, tooltip.Controls[tooltip.Controls.Count - 1].Bottom));
        //    Bar bar = new Bar(new Vector2(0, tooltip.Controls.Count > 0 ? tooltip.Controls.Last().Bottom : 0));
        //    bar.Percentage = GetProperty<float>("Health") / GetProperty<float>("Max Health");
        //    box.Controls.Add(bar);
        //    tooltip.Controls.Add(box);
        //}

    }
}
