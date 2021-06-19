using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    //class StatusConditionCollection : SortedDictionary<Stat.Types, GameObject>
    //{
    //    //public void Add(GameObject condition)
    //    //{
    //    //    this[condition.Name] = condition;
    //    //}

    //    internal void Add(Stat stat, int value, int duration)
    //    {
    //        GameObject condition = StatusCondition.Empty;// GameObjectDb.StatusCondition;
    //        //ConditionComponent condComp = condition.GetComponent<ConditionComponent>("Condition");
    //        //condComp.Properties["Type"].Value = stat.Type;
    //        //condComp.Properties["Value"].Value = value;
    //        //condComp.Properties["Duration"].Value = duration;
    //        condition.Components["Condition"] = new ConditionComponent(stat.Type, value, duration);
    //        this[stat.Type] = condition;
    //    }

    //    internal void Add(GameObject condition)
    //    {
    //        this[condition.Components["Condition"].GetProperty<Stat.Types>("Type")] = condition;
    //    }

    //    public override string ToString()
    //    {
    //        string text = "";
    //        foreach (KeyValuePair<Stat.Types, GameObject> condition in this)
    //        {
    //            text += condition.Value["Condition"];
    //        }
    //        return text;
    //    }
    //}
    class StatusComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Status";
            }
        }

        List<GameObject> Conditions { get { return (List<GameObject>)this["Conditions"]; } set { this["Conditions"] = value; } }

        public StatusComponent()
            : base()
        {
            Conditions = new List<GameObject>();
         //   Properties.Add("Conditions", new StatusConditionCollection());
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk)
        {

            List<GameObject> copy = new List<GameObject>(Conditions);
            foreach(GameObject condition in copy)
            {
                throw new NotImplementedException();
                //condition.PostMessage(Message.Types.Update, parent);
            }
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)//GameObject sender, Message.Types msg)
        {
            switch (e.Type)
            {
                case Message.Types.ConditionFinished:
                    Conditions.Remove(e.Sender);
                    throw new NotImplementedException();
                    //e.Sender.PostMessage(Message.Types.Unequip, parent);
                    return true;
                case Message.Types.Consume:

                    List<GameObject> conditions = e.Parameters[0] as List<GameObject>;
                    foreach (GameObject condition in conditions)
                    {
                        throw new NotImplementedException();
                        //parent.PostMessage((Message.Types)condition["Condition"]["Message"], parent, condition["Condition"]["Stat"], condition["Condition"]["Value"]);
                        if ((float)condition["Condition"]["Duration"] > 0)
                            Conditions.Add(condition.Clone());

                    }
                    return true;
                default:
                    return false;
            }
        }

        public override object Clone()
        {
            StatusComponent comp = new StatusComponent();
            return comp;
        }
    }
}
