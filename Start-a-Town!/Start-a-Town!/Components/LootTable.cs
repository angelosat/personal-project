using System;
using System.Collections.Generic;
using Start_a_Town_.Net;

namespace Start_a_Town_
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
                text += loot.ToString();
            }
            if (text.Length > 0)
                if (text[text.Length - 1] == '\n')
                    return text.Remove(text.Length - 1);
            return text;
        }
        public IEnumerable<GameObject> Generate(RandomThreaded rand)
        {
            foreach (var l in this)
                for (int i = 0; i < l.Generate(rand); i++)
                {
                    var obj = l.Factory();
                    var stacksize = rand.Next(l.StackMin, l.StackMax);
                    obj.StackSize = stacksize;
                    yield return obj;
                }
        }
        public IEnumerable<GameObject> Generate(Random rand)
        {
            foreach (var l in this)
                for (int i = 0; i < l.Generate(rand); i++)
                {
                    var obj = l.Factory();
                    var stacksize = rand.Next(l.StackMin, l.StackMax);
                    obj.StackSize = stacksize;
                    yield return obj;
                }
        }
        public static IEnumerable<GameObject> Generate(Random rand, params Loot[] loot)
        {
            for (int k = 0; k < loot.Length; k++)
            {
                var l = loot[k];
                for (int i = 0; i < l.Generate(rand); i++)
                {
                    var obj = l.Factory();
                    var stacksize = rand.Next(l.StackMin, l.StackMax);
                    obj.StackSize = stacksize;
                    yield return obj;
                }
            }
        }
    }
}
