using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class Loot2Component : Component
    {
      //  public List<Loot> Loot;
        LootTable Loot { get { return (LootTable)this["Loot"]; } set { this["Loot"] = value; } }
        float Rate { get { return (float)this["Rate"]; } set { this["Rate"] = value; } }

        public override void Initialize(GameObject parent)
        {
            List<GameObjectSlot> list = new List<GameObjectSlot>();
            Generate(ref list);
            foreach (GameObjectSlot objSlot in list)
                parent.PostMessage(Message.Types.Receive, parent, objSlot);
        }

        public Loot2Component(float rate = 0)
        {
          //  Loot = new List<Loot>();
            //this["Loot"] = new LootCollection();
            this.Rate = rate;
            Loot = new LootTable();
        }

        public Loot2Component(float rate = 0, params Loot[] lootList) : this(rate)
        {
           // Loot = new List<Loot>();
            foreach (Loot loot in lootList)
                GetProperty<LootTable>("Loot").Add(loot);
        }

        public Loot2Component(float rate = 0, params object[] lootInfo) : this(rate)
        {
            if (lootInfo.Length % 3 != 0)
                throw (new ArgumentException());
            //Loot = new List<Loot>();
            LootTable loot = GetProperty<LootTable>("Loot");
            Queue<object> queue = new Queue<object>(lootInfo);
            while (queue.Count > 0)
            {
                loot.Add(new Loot((GameObject.Types)queue.Dequeue(), (float)queue.Dequeue(), (int)queue.Dequeue()));
            }
        }


        public Dictionary<GameObject.Types, int> Generate()
        {
            Dictionary<GameObject.Types, int> finalLoot = new Dictionary<GameObject.Types, int>();
            int count;
            List<Loot> lootList = GetProperty<List<Loot>>("Loot");
            foreach (Loot loot in lootList)
            {
                count = loot.Generate();
                if (count > 0)
                    finalLoot[loot.ObjID] = count;
            }
            return finalLoot;
        }

        public void Generate(ref List<GameObjectSlot> list)
        {
            int count;
            List<Loot> lootList = GetProperty<List<Loot>>("Loot");
            foreach (Loot loot in lootList)
            {
                count = loot.Generate();
                if (count > 0)
                    list.Add(GameObject.Create(loot.ObjID).ToSlot(count));
            }
        }

        public override bool HandleMessage(GameObject parent, GameObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (e.Type)
            {
                //if (msg == Message.Types.Death)
                //{
                case Message.Types.Death:
                    Dictionary<GameObject.Types, int> loots = Generate();
                    foreach (KeyValuePair<GameObject.Types, int> loot in loots)
                    {
                        for (int i = 0; i < loot.Value; i++)
                        {
                            Vector3 parentGlobal = parent.GetComponent<MovementComponent>("Position").GetProperty<Position>("Position").Global;
                            Vector3 parentHeight = new Vector3(0, 0, parent["Physics"].GetProperty<int>("Height"));

                            GameObject obj = GameObject.Create(loot.Key);
                            //Position lootPosition = obj["Position"].GetProperty<Position>("Position");
                            //lootPosition.Global = parentGlobal + parentHeight;
                            obj["Position"]["Position"] = new Position(parent.Map, parentGlobal + parentHeight - new Vector3(0, 0, 1));

                            Chunk.AddObject(obj, parent.Map);
                            double angle = parent.Map.World.Random.NextDouble() * (Math.PI + Math.PI);
                            float upwards = 0.5f;//0.05f
                            Vector3 normal = new Vector3((float)Math.Cos(angle) * 0.05f, (float)Math.Sin(angle) * 0.05f, upwards);
                            //   normal.Normalize();
                            GameObject.PostMessage(obj, Message.Types.ApplyForce, parent, normal);
                           // obj.GetComponent<MovementComponent>("Position").ApplyForce(normal);
                        }
                    }
                    return true;
                case Message.Types.Attack:
                    if (Rate == 0)
                        return false;
                    return true;
                default:
                    return false;
            }
            return false;
        }


        public override object Clone()
        {
            Loot2Component lootComp = new Loot2Component();
          //  lootComp.Loot = Loot;
            lootComp["Loot"] = Properties["Loot"];
            return lootComp;
        }
    }
}
