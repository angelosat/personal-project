using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class LootComponent : EntityComponent
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
                    list.Add(GameObject.Create(loot.ObjID).ToSlotLink(count));
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
