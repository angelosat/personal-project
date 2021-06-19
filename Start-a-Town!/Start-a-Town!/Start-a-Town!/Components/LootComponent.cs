using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components
{
    public class LootTable : List<Loot>
    {
        public LootTable()
        {

        }

        public LootTable(params Loot[] loots)
        {
            this.AddRange(loots);
        }
        public override string ToString()
        {
            string text = "";
            foreach (Loot loot in this)
            {
                text += loot.ToString(); // GameObject.Objects[loot.ObjID].Name + " x" + loot.Count + " (" + loot.Chance * 100 + " %)";
            }
            if (text.Length > 0)
                if (text[text.Length - 1] == '\n')
                    return text.Remove(text.Length - 1);
            return text;
        }

    }

    public class Loot
    {
        public int Count, ObjID, StackMin = 1, StackMax = 1;
        //ItemType _ItemType;
        public float Chance;
        public Func<GameObject> Factory;
        //public ItemType ItemType
        //{get{return _ItemType;}}

        public override string ToString()
        {
            return GameObject.Objects[ObjID].Name + " x" + Count + " (" + Chance * 100 + " %)";
        }


        public Loot(Func<GameObject> factory, float chance, int count, int stackmin, int stackmax)
        {
            this.Factory = factory;
            Chance = chance;
            Count = count;
            ObjID = factory().GetInfo().ID;
            this.StackMin = stackmin;
            this.StackMax = stackmax;
        }
        public Loot(Func<GameObject> factory, float chance = 1, int count = 1)
        {
            this.Factory = factory;
            Chance = chance;
            Count = count;
            ObjID = factory().GetInfo().ID;
        }
        public Loot(GameObject.Types objID, float chance = 1, int count = 1)
        {
            ObjID = (int)objID;
            Chance = chance;
            //Min = min;
            Count = count;
            this.Factory = () => GameObject.Create(this.ObjID);
        }
        public Loot(int objID, float chance = 1, int count = 1)
        {
            ObjID = objID;
            Chance = chance;
            //Min = min;
            Count = count;
            this.Factory = () => GameObject.Create(this.ObjID);

        }
        public int Generate(RandomThreaded random)
        {
            int count = 0;
            for (int i = 0; i < Count; i++)
            {
                if (random.NextDouble() < Chance)
                    count++;
            }
            return count;
        }
        public int Generate(Random random)
        {
            int count = 0;
            for (int i = 0; i < Count; i++)
            {
                if (random.NextDouble() < Chance)
                    count++;
            }
            return count;
        }

        public static void PopLoot(Random random, Map map, Vector3 global, GameObject.Types objID, int amount = 1)
        {
            for (int i = 0; i < amount; i++)
            {
                GameObject obj = GameObject.Create(objID);
                Chunk.AddObject(obj, map, global);
                double angle = random.NextDouble() * (Math.PI + Math.PI);
                throw new NotImplementedException();
                //GameObject.PostMessage(obj, Message.Types.ApplyForce, null, new Vector3((float)Math.Cos(angle) * 0.05f, (float)Math.Sin(angle) * 0.05f, 0.05f));
            }
        }
        public static GameObject PopLoot(Random random, GameObject parent, GameObjectSlot objSlot)
        {
            return (objSlot.HasValue) ? PopLoot(random, parent, objSlot.Object) : null;
        }
        //public static GameObject PopLoot(GameObject parent, GameObject obj)
        public static GameObject PopLoot(Random random, GameObject parent, GameObject obj)
        {
            //Chunk.AddObject(obj, parent.Map, parent.Global + new Vector3(0, 0, (float)parent["Physics"]["Height"]));
            Vector3 global = parent.Global + new Vector3(0, 0, (float)parent["Physics"]["Height"]);
            double angle = random.NextDouble() * (Math.PI + Math.PI);
            double w = random.NextDouble() * Math.PI / 2f;
            // TODO: randomize but normalize all 3 axis
            Vector3 speed = new Vector3((float)Math.Cos(angle) * 0.05f, (float)Math.Sin(angle) * 0.05f, (float)Math.Sin(w) * 0.05f);
            //GameObject.PostMessage(obj, Message.Types.ApplyForce, parent, new Vector3((float)Math.Cos(angle) * 0.05f, (float)Math.Sin(angle) * 0.05f, 0.05f));
            Client.AddObject(obj, global, speed);
            return obj;
        }
        public static GameObject PopLoot(Random random, GameObject parent, GameObject.Types objID)
        {
            return PopLoot(random, parent, GameObject.Create(objID));
        }
    }
    class LootComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Loot";
            }
        }

        LootTable Loot { get { return (LootTable)this["Loot"]; } set { this["Loot"] = value; } }
        public LootComponent()
        {
          //  Loot = new List<Loot>();
            //this["Loot"] = new LootCollection();
            Loot = new LootTable();
        }

        //public override void Initialize(GameObject parent)
        //{
        //    List<GameObjectSlot> list = new List<GameObjectSlot>();
        //    Generate(list);
        //    foreach (GameObjectSlot objSlot in list)
        //        parent.PostMessage(Message.Types.Receive, parent, objSlot);
        //}


        public LootComponent(params Loot[] lootList) : this()
        {
           // Loot = new List<Loot>();
            foreach (Loot loot in lootList)
                GetProperty<LootTable>("Loot").Add(loot);
        }

        public LootComponent(params object[] lootInfo)
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

        //public void Add(ItemType itemType, float chance = 1, int min = 1, int max = 1)
        //{
        //    Loot.Add(new Loot(itemType, chance, min, max));
        //}

        public Dictionary<int, int> Generate(Random random)
        {
            Dictionary<int, int> finalLoot = new Dictionary<int, int>();
            int count;
            List<Loot> lootList = GetProperty<List<Loot>>("Loot");
            foreach (Loot loot in lootList)
            {
                count = loot.Generate(random);
                if (count > 0)
                    finalLoot[loot.ObjID] = count;
            }
            return finalLoot;
        }

        public void Generate(Random random, List<GameObjectSlot> list)
        {
            int count;
            List<Loot> lootList = GetProperty<List<Loot>>("Loot");
            foreach (Loot loot in lootList)
            {
                count = loot.Generate(random);
                if (count > 0)
                    list.Add(GameObject.Create(loot.ObjID).ToSlot(count));
            }
        }

        public override bool HandleMessage (GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            // GENERATE LOOT ONLY SERVERSIDE
            return base.HandleMessage(parent, e);
        }

        public override object Clone()
        {
            LootComponent lootComp = new LootComponent();
          //  lootComp.Loot = Loot;
            lootComp["Loot"] = Properties["Loot"];
            return lootComp;
        }
    }
}
