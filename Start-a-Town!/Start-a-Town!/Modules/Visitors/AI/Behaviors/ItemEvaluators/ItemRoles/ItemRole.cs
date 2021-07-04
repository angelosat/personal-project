using System.Collections.Generic;

namespace Start_a_Town_
{
    abstract class ItemRole
    {
        public ItemRole()
        {

        }
        abstract public int Score(Actor actor, Entity item);
        public Entity FindBest(Actor actor, IEnumerable<Entity> items)
        {
            Entity bestItem = null;
            int bestScore = 0;
            foreach (var i in items)
            {
                var score = this.Score(actor, i);
                if (score > bestScore)
                {
                    bestItem = i;
                    bestScore = score;
                }
            }
            return bestItem;
        }
    }
}
