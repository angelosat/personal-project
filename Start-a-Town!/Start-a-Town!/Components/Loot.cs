using System;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public class Loot
    {
        public int Count, ObjID, StackMin = 1, StackMax = 1;
        public float Chance;
        public Func<GameObject> Factory;
        public GameObject GenerateNew(Random rand)
        {
            var obj = this.Factory();
            var stacksize = rand.Next(this.StackMin, this.StackMax);
            obj.StackSize = stacksize;
            return obj;
        }
        public override string ToString()
        {
            return GameObject.Objects[ObjID].Name + " x" + Count + " (" + Chance * 100 + " %)";
        }

        public Loot(ItemDef def, float chance = 1, int count= 1, int amountmin= 1, int amountmax = 1) : this(() => def.Factory(def), chance, count, amountmin, amountmax)
        {
        }
        public Loot(Func<GameObject> factory, float chance, int count, int stackmin, int stackmax)
        {
            this.Factory = factory;
            Chance = chance;
            Count = count;
            this.StackMin = stackmin;
            this.StackMax = stackmax;
        }
        public Loot(Func<GameObject> factory, float chance = 1, int count = 1)
        {
            this.Factory = factory;
            Chance = chance;
            Count = count;
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
    }
}
