using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.Components.AI;

namespace Start_a_Town_.Components.Needs
{
    class NeedsWork : Need
    {
        public NeedsWork()
            : base(Need.Types.Work, "Work", 100)//, decay: 0, tolerance: 100)
        {

        }

        public override List<AIJob> FindPotentialJobs(GameObject entity, Dictionary<GameObject, List<Interactions.Interaction>> allInteractions)
        {
            return entity.Map.Town.AIJobs.ToList();
        }
        public override object Clone()
        {
            return new NeedsWork();
        }
    }
}
