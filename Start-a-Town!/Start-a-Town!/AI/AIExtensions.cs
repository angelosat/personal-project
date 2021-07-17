using Start_a_Town_.AI;

namespace Start_a_Town_
{
    static class AIExtensions
    {
        static public bool HasLabor(this GameObject entity, JobDef labor)
        {
            return labor is null ? true : AIState.GetState(entity).HasJob(labor);
        }
    }
}
