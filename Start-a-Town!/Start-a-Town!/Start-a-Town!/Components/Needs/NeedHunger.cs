using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.AI;
using Start_a_Town_.AI;

namespace Start_a_Town_.Components.Needs
{
    class NeedHunger : Need
    {
        public override Need.Types ID
        {
            get
            {
                return Types.Hunger;
            }
        }
        public override string Name
        {
            get
            {
                return "Hunger";
            }
        }



        public NeedHunger()
            : base(Need.Types.Hunger, "Hunger", 100)
        {

        }

        public override List<AIJob> FindPotentialJobs(GameObject entity, Dictionary<GameObject, List<Interactions.Interaction>> allInteractions)
        {
            List<AIJob> jobs = new List<AIJob>();
            var state = AIState.GetState(entity);
            var memory = state.Knowledge.Objects;

            //var ordered = entity.Map.GetObjects().Where(o => o.Exists && !AIState.IsItemReserved(o)).ToList(); // no need to order here because i order them according to score later .OrderBy(o => Vector3.DistanceSquared(entity.Global, o.Global)).ToList();

            foreach (var m in allInteractions)// ordered)
            {
                if (entity == m.Key)
                    continue;
                var obj = m;

                var interactions = m.Value;// obj.GetInteractionsList();
                var action = interactions.FirstOrDefault(i =>
                    {
                        float value;
                        if (i.NeedSatisfaction.TryGetValue(this.ID, out value))
                            return value <= this.Max - this.Value;
                        return false;
                    });
                if (action == null)
                    continue;
                var job = new AIJob();
                job.AddStep(new AIInstruction(new TargetArgs(m.Key), action));
                jobs.Add(job);
            }
            return Order(entity, jobs);// jobs;
        }

        List<AIJob> Order(GameObject agent, List<AIJob> jobs)
        {
            if (jobs.Count == 0)
                return jobs;
            var instructions = jobs.Select(j => j.Instructions.First()).ToList();
            var ordered = this.GetScored(agent, instructions);
            return ordered.Select(i => new AIJob(new AIInstruction(i.Target, i.Interaction))).ToList();

            //TODO: take into account hunger satisfaction value
            return jobs.OrderBy(j => Vector3.DistanceSquared(agent.Global, j.CurrentStep.Target.Global)).ToList();
        }
        //float GetScore(GameObject parent, AIInstruction instruction, float minDistance, float maxDistance)
        //{
        //    var patience = AIState.GetState(parent).Personality.Patience;
        //    var distance = Vector3.DistanceSquared(parent.Global, instruction.Target.Global);
        //    var dHunger = Math.Abs(this.Value + instruction.Interaction.NeedSatisfaction[this.ID] - this.Max);

        //    var scorePatientFull = 1 - dHunger / this.Max;
        //    var scoreImpatientFull = 1 - (distance - minDistance) / (maxDistance - minDistance);

        //    var patienceNormalized = patience.Normalized;
        //    var score = (1 - patienceNormalized) * scoreImpatientFull + patienceNormalized * scorePatientFull;

        //    return score;
        //}

        List<AIInstruction> GetScored(GameObject parent, List<AIInstruction> instructions)
        {
            //var dMin = instructions.Min(i => Vector3.DistanceSquared(parent.Global, i.Target.Global));
            //var dMax = instructions.Max(i => Vector3.DistanceSquared(parent.Global, i.Target.Global));
            //var ord = instructions.OrderBy(i => Vector3.DistanceSquared(parent.Global, i.Target.Global));
            var ordered = instructions.ToDictionary(i => i, i => Vector3.DistanceSquared(parent.Global, i.Target.Global)).OrderBy(i => i.Value);
            var dMin = ordered.First().Value;
            var dMax = ordered.Last().Value;
            var patience = AIState.GetState(parent).Personality.Patience;
            //foreach(var i in ordered)
            //{
            //    //this.GetScore(parent, i, dMin, dMax);
            //    var score = this.GetScore(parent, i.Key, dMin, dMax, patience, i.Value);
            //}
            var scored = ordered.OrderBy(i => 1 - this.GetScore(parent, i.Key, dMin, dMax, patience, i.Value)).ToDictionary(i => i.Key, i => i.Value).Keys.ToList();
            return scored;
        }

        private float GetScore(GameObject parent, AIInstruction instruction, float minDistance, float maxDistance, Trait patience, float distance)
        {
            var dHunger = Math.Abs(this.Value + instruction.Interaction.NeedSatisfaction[this.ID] - this.Max);

            var scorePatientFull = 1 - dHunger / this.Max;
            var scoreImpatientFull = 1 - (distance - minDistance) / (maxDistance - minDistance);

            var patienceNormalized = patience.Normalized;
            var score = (1 - patienceNormalized) * scoreImpatientFull + patienceNormalized * scorePatientFull;

            return score;
        }
        public override object Clone()
        {
            return new NeedHunger();
        }
    }
}
