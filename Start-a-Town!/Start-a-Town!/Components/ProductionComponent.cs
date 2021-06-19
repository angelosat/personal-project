using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Net;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.Components
{
    class ProductionComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Convertible";
            }
        }

        //Interaction Action { get { return (Interaction)this["Action"]; } set { this["Action"] = value; } }
        Message.Types ProductionMessage { get { return (Message.Types)this["Message"]; } set { this["Message"] = value; } }
        //GameObject.Types Object { get { return (GameObject.Types)this["Object"]; } set { this["Object"] = value; } }
        LootTable LootTable { get { return (LootTable)this["LootTable"]; } set { this["LootTable"] = value; } }
        string ActionName { get { return (string)this["ActionName"]; } set { this["ActionName"] = value; } }
        string Verb { get { return (string)this["Verb"]; } set { this["Verb"] = value; } }
        TimeSpan TimeSpan { get { return (TimeSpan)this["TimeSpan"]; } set { this["TimeSpan"] = value; } }
        Func<GameObject, float> Speed { get { return (Func<GameObject, float>)this["Speed"]; } set { this["Speed"] = value; } }
        public Action<IObjectProvider, GameObject, GameObject> OnSuccess { get { return (Action<IObjectProvider, GameObject, GameObject>)this["OnSuccess"]; } set { this["OnSuccess"] = value; } }

       // Action<GameObject> OnFailure { get { return (Action<GameObject>)this["OnFailure"]; } set { this["OnFailure"] = value; } }

        //public ConvertComponent(Message.Types msg, GameObject.Types objType)
        //{
        //    Message = msg;
        //    Object = objType;
        //}

     //   ConvertComponent() { Loot = new List<Loot>(); }

        //public ConvertibleComponent(Interaction action, params Loot[] loot)
        //{
        //    this.Action = action;
        //    Loot = new List<Loot>(loot);
        //}

        //public ConvertibleComponent(Interaction action, List<Loot> loot)
        //{
        //    this.Action = action;
        //    Loot = loot;
        //}

        public ProductionComponent()
        {
            this.OnSuccess = (net, foo, bar) => { };
            this.ProductionMessage = Message.Types.Default;
            this.ActionName = "";
            this.Initialize(Message.Types.Default, "unnamed", "unnamed", TimeSpan.Zero, o => 0);
        }

        public ProductionComponent Initialize(Message.Types message, string action, string verb, TimeSpan timespan, Func<GameObject, float> speed, params Loot[] loot)
        {
            this.Speed = speed != null ? speed : foo => 1f;
            this.ProductionMessage = message;
            this.LootTable = new LootTable(loot);
            this.ActionName = action;
            this.Verb = verb;
            this.TimeSpan = timespan;
            this.Speed = foo => 1f;
            return this;
        }

        public ProductionComponent(Message.Types message, string action, string verb, TimeSpan timespan, Func<GameObject, float> speed, params Loot[] loot)
            : this(message, action, verb, timespan, loot)
        {
            this.Speed = speed != null ? speed : foo => 1f;
        }

        public ProductionComponent(Message.Types message, string action, string verb, TimeSpan timespan, params Loot[] loot)
            : this()
        {
            this.ProductionMessage = message;
            this.LootTable = new LootTable(loot);
            this.ActionName = action;
            this.Verb = verb;
            this.TimeSpan = timespan;
            this.Speed = foo => 1f;
        }

        //public ProductionComponent(Message.Types message, string action, string verb, TimeSpan timespan, LootTable loot)
        //    : this()
        //{
        //    this.ProductionMessage = message;
        //    this.LootTable = loot;
        //    this.ActionName = action;
        //    this.Verb = verb;
        //    this.TimeSpan = timespan;
        //    this.Speed = foo => 1f;
        //}

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            // GENERATE LOOT SERVER SIDE
            //return base.HandleMessage(parent, e);

            switch (e.Type)
            {
                default:

                    if (e.Type != ProductionMessage)
                        return true;
                    //Dictionary<GameObject.Types, int> loots = Generate();
                    //foreach (KeyValuePair<GameObject.Types, int> loot in loots)
                    //{
                    //    for (int i = 0; i < loot.Value; i++)
                    //    {
                    //        // signal server to generate loot (cause the random class is on the server)
                    //        //    Components.Loot.PopLoot(parent, GameObject.Create(loot.Key));
                    //        //       continue;
                    //        e.Net.PopLoot(parent, GameObject.Create(loot.Key));
                    //    }
                    //}
                    e.Network.PopLoot(LootTable, parent.Global, parent.Velocity);

                    GameObject sender = e.Parameters[0] as GameObject;

                    this.OnSuccess(e.Network, sender, parent);
                //    parent.PostMessage(Message.Types.Death, e.Sender);
                    e.Network.Despawn(parent);
                    e.Network.DisposeObject(parent);//.NetworkID);
                    // Client.DestroyObject(parent);
                    //   parent.Remove();
                    return true;

            }

        }

        //public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        //{
        ////    List<Interaction> list = e.Parameters[0] as List<Interaction>;
        //    list.Add(new InteractionOld(
        //               new TimeSpan(0, 0, 2),
        //               //Message.Types.Saw,
        //               //name: "Produce",
        //               //verb: "Producing",
        //               ProductionMessage,parent, ActionName, Verb,  
                       
        //        //cond: new InteractionCondition("Equipped", value: true, planType: AI.PlanType.FindInventory,
        //        //    precondition: new Predicate<GameObject>(foo => FunctionComponent.HasAbility(foo, ProductionMessage)),
        //        //    finalCondition: subject => ControlComponent.HasAbility(subject, ProductionMessage)
        //               cond: new ConditionCollection(
        //               new Condition((actor, target) => ControlComponent.HasAbility(actor, ProductionMessage), "I need a tool to " + ProductionMessage.ToString().ToLower() + " with.", //"Requires: " + ProductionMessage,
        //                   new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, ProductionMessage), PlanType.FindInventory))
        //                   ),
        //               effect: new NeedEffectCollection(){new AIAdvertisement("Production", 20)},// new ("Production"), 
        //               speed: this.Speed
        //               ));
        //}

        public Dictionary<int, int> Generate(Random random)
        {
            Dictionary<int, int> finalLoot = new Dictionary<int, int>();
            int count;
            //List<Loot> lootList = GetProperty<List<Loot>>("Loot");
            foreach (Loot loot in LootTable)
            {
                count = loot.Generate(random);
                if (count > 0)
                    finalLoot[loot.ObjID] = count;
            }
            return finalLoot;
        }

        public override object Clone()
        {
            return new ProductionComponent(ProductionMessage, ActionName, Verb, TimeSpan, this.Speed, LootTable.ToArray()) { OnSuccess = this.OnSuccess };
        }

        public bool CanDrop(Predicate<GameObject> pred)
        {
            foreach (Loot loot in LootTable)
            {
                if (pred(GameObject.Objects[loot.ObjID]))
                    return true;
            }
            return false;
        }
        //static public bool CanProduce(GameObject subject, Predicate<GameObject> pred)
        //{
        //    ProductionComponent conv;
        //    if (!subject.TryGetComponent<ProductionComponent>("Convertible", out conv))
        //        return false;
        //    foreach (Loot loot in conv.Loot)
        //    {
        //        if (pred(GameObject.Objects[loot.ObjID]))
        //            return true;
        //    }
        //    return false;
        //}
        static public bool CanProduce(GameObject subject, Predicate<GameObject> pred)
        {
            Queue<GameObject> toCheck = new Queue<GameObject>();
            toCheck.Enqueue(subject);
            while (toCheck.Count > 0)
            {
                GameObject current = toCheck.Dequeue();
                ProductionComponent conv;
                if (pred(current))
                    return true;
                if (!current.TryGetComponent<ProductionComponent>("Convertible", out conv))
                    continue;
                foreach (Loot loot in conv.LootTable)
                {
                    //if (pred(GameObject.Objects[loot.ObjID]))
                   //     return true;
                    toCheck.Enqueue(GameObject.Objects[loot.ObjID]);
                }
            }
            return false;
        }
    }
}
