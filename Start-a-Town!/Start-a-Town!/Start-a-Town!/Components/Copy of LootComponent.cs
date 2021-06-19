using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components
{
    class LootTable : List<Loot>
    {
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

    public struct Loot
    {
        public int Count;
        public GameObject.Types ObjID;
        //ItemType _ItemType;
        public float Chance;

        //public ItemType ItemType
        //{get{return _ItemType;}}

        public override string ToString()
        {
            return GameObject.Objects[ObjID].Name + " x" + Count + " (" + Chance * 100 + " %)";
        }

        public Loot(GameObject.Types objID, float chance = 1, int count = 1)
        {
            ObjID = objID;
            Chance = chance;
            //Min = min;
            Count = count;
        }

        public int Generate()
        {
            int count = 0;
            for (int i = 0; i < Count; i++)
            {
                if (Engine.Random.NextDouble() < Chance)
                    count++;
            }
            return count;
        }

        public static void PopLoot(Map map, Vector3 global, GameObject.Types objID, int amount = 1)
        {
            for (int i = 0; i < amount; i++)
            {
                GameObject obj = GameObject.Create(objID);
                Chunk.AddObject(obj, map, global);
                double angle = Engine.Random.NextDouble() * (Math.PI + Math.PI);
                GameObject.PostMessage(obj, Message.Types.ApplyForce, null, new Vector3((float)Math.Cos(angle) * 0.05f, (float)Math.Sin(angle) * 0.05f, 0.05f));
            }
        }
        public static GameObject PopLoot(GameObject parent, GameObjectSlot objSlot)
        {
            return (objSlot.HasValue) ? PopLoot(parent, objSlot.Object) : null;
        }
        public static GameObject PopLoot(GameObject parent, GameObject obj)
        {
            //Chunk.AddObject(obj, parent.Map, parent.Global + new Vector3(0, 0, (float)parent["Physics"]["Height"]));
            Vector3 global = parent.Global + new Vector3(0, 0, (float)parent["Physics"]["Height"]);
            double angle = Engine.Random.NextDouble() * (Math.PI + Math.PI);
            double w = Engine.Random.NextDouble() * Math.PI / 2f;
            // TODO: randomize but normalize all 3 axis
            Vector3 speed = new Vector3((float)Math.Cos(angle) * 0.05f, (float)Math.Sin(angle) * 0.05f, (float)Math.Sin(w) * 0.05f);
            //GameObject.PostMessage(obj, Message.Types.ApplyForce, parent, new Vector3((float)Math.Cos(angle) * 0.05f, (float)Math.Sin(angle) * 0.05f, 0.05f));
            Client.AddObject(obj, global, speed);
            return obj;
        }
        public static GameObject PopLoot(GameObject parent, GameObject.Types objID)
        {
            return PopLoot(parent, GameObject.Create(objID));
            //GameObject obj = GameObject.Create(objID);
            //Chunk.AddObject(obj, parent.Global + new Vector3(0, 0, (int)parent["Physics"]["Height"]));
            //double angle = Map.Instance.Random.NextDouble() * (Math.PI + Math.PI);
            //obj.GetComponent<MovementComponent>("Position").ApplyForce(new Vector3((float)Math.Cos(angle) * 0.05f, (float)Math.Sin(angle) * 0.05f, 0.05f));
            //return obj;
        }
    }
    class LootComponent : Component
    {
      //  public List<Loot> Loot;
        LootTable Loot { get { return (LootTable)this["Loot"]; } set { this["Loot"] = value; } }
        public LootComponent()
        {
          //  Loot = new List<Loot>();
            //this["Loot"] = new LootCollection();
            Loot = new LootTable();
        }

        public override void Initialize(GameObject parent)
        {
            List<GameObjectSlot> list = new List<GameObjectSlot>();
            Generate(ref list);
            foreach (GameObjectSlot objSlot in list)
                parent.PostMessage(Message.Types.Receive, parent, objSlot);
        }


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
            if (msg == Message.Types.Death)
            {
                Dictionary<GameObject.Types, int> loots = Generate();
                foreach (KeyValuePair<GameObject.Types, int> loot in loots)
                {
                    for (int i = 0; i < loot.Value; i++)
                    {
                        Vector3 parentGlobal = parent.GetComponent<MovementComponent>("Position").GetProperty<Position>("Position").Global;
                        Vector3 parentHeight = new Vector3(0, 0, parent["Physics"].GetProperty<float>("Height"));
                        
                        GameObject obj = GameObject.Create(loot.Key);
                        //Position lootPosition = obj["Position"].GetProperty<Position>("Position");
                        //lootPosition.Global = parentGlobal + parentHeight;
                        obj["Position"]["Position"] = new Position(parent.Map, parentGlobal + parentHeight - new Vector3(0,0,1));
                        
                        Chunk.AddObject(obj, parent.Map);
                        double angle = parent.Map.World.Random.NextDouble() * (Math.PI + Math.PI);
                        float upwards = 0.5f;//0.05f
                        Vector3 normal = new Vector3((float)Math.Cos(angle) * 0.05f, (float)Math.Sin(angle) * 0.05f, upwards);
                     //   normal.Normalize();
                      //  obj.GetComponent<MovementComponent>("Position").ApplyForce(normal);
                        GameObject.PostMessage(obj, Message.Types.ApplyForce, parent, normal);
                    }

                    //StaticObject obj = StaticObject.Create(loot.Key);
                    //obj.GetComponent<GuiComponent>("Gui").StackSize = loot.Value;
                    //Position.AddObject(obj, parent.GetComponent<MovementComponent>("Position").CurrentPosition.Global + new Vector3(0, 0, parent.GetInfo().Height));
                }
                return true;
            }
            return false;
        }

        //public override string ToString()
        //{
        //    //if (GlobalVars.Settings.DebugMode)
        //    //    return base.ToString();
        //    string text = "";
        //    bool first = true;
        //    List<Loot> lootList = GetProperty<List<Loot>>("Loot");
        //    foreach (Loot loot in lootList)
        //    {
        //        if (!first)
        //            text += "\n";
        //        text += StaticObject.Objects[loot.ObjID].Name + " x" + loot.Count + " (" + loot.Chance * 100 + " %)";
        //    }
        //    return text;
        //}

        public override object Clone()
        {
            LootComponent lootComp = new LootComponent();
          //  lootComp.Loot = Loot;
            lootComp["Loot"] = Properties["Loot"];
            return lootComp;
        }
    }
}
