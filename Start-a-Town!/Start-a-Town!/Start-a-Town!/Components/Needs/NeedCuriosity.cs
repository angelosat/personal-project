using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.AI;
using Start_a_Town_.AI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Needs
{
    class NeedCuriosity : Need
    {
        public NeedCuriosity()
            : base(Need.Types.Curiosity, "Curiosity", 100)//, .1f)//, 10)
        {

        }

        public override List<AIJob> FindPotentialJobs(GameObject entity, Dictionary<GameObject, List<Interaction>> allInteractions)
        {
            var state = AIState.GetState(entity);
            var memory = state.Knowledge.Objects;
            List<AIJob> jobs = new List<AIJob>();

            //var randomized = new Queue<Memory>(state.Knowledge.Objects.Values.Where(o => o.Object.Exists && !AIState.IsItemReserved(o.Object)).Randomize<Memory>((entity.Net as Net.Server).GetRandom()));
            var randomized = new Queue<GameObject>(allInteractions.Keys.Randomize<GameObject>((entity.Net as Net.Server).GetRandom()));

            while(randomized.Count>0)
            {
                //var m = randomized.Dequeue();
                var obj = randomized.Dequeue(); //m.Object;
                if (obj == entity)
                    continue;
                var interactions = allInteractions[obj];// obj.GetInteractionsList();
                var examine = interactions.FirstOrDefault(i =>
                    {
                        float value;
                        if (i.NeedSatisfaction.TryGetValue(this.ID, out value))
                            return value <= this.Max - this.Value;
                        return false;
                    });
                if (examine == null)
                    continue;
                var job = new AIJob();
                job.AddStep( new AIInstruction(new TargetArgs(obj), examine));
                jobs.Add(job);
            }
            return jobs;
        }

       

        public override object Clone()
        {
            return new NeedCuriosity();
        }
    }
}
