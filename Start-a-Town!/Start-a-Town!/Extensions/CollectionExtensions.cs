using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    static public class CollectionExtensions
    {
        static public T SelectRandomWeighted<T>(this IEnumerable<T> collection, Random random, Func<T, int> weightGetter)
        {
            return collection.ToArray().SelectRandomWeighted(random, weightGetter);
        }
        static public T SelectRandomWeighted<T>(this ICollection<T> collection, Random random, Func<T, int> weightGetter)
        {
            return collection.ToArray().SelectRandomWeighted(random, weightGetter);
        }
        static public T SelectRandomWeighted<T>(this T[] list, Random random, Func<T, int> weightGetter)
        {
            int currentP = 0;
            var count = list.Length;
            var weights = new (T order, int prob)[count];// orders.ToDictionary(o => o, o => favs.Count(o.IsAllowed));
            for (int i = 0; i < count; i++)
            {
                var o = list[i];
                var p = 1 + weightGetter(o);
                currentP += p;
                weights[i] = (o, currentP);
            }
            var val = random.Next(currentP);
            for (int i = 0; i < count; i++)
            {
                var (order, prob) = weights[i];
                if (val <= prob)
                    return order;
            }
            throw new Exception();
        }
    }
}
