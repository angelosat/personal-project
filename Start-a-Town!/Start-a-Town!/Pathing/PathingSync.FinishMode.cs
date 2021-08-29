using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public partial class PathingSync
    {
        public abstract class FinishMode
        {
            static public readonly FinishMode Touching = new FinishModeDefault();
            static public readonly FinishMode Exact = new FinishModeOnGoal();
            static public readonly FinishMode Any = new FinishModeOnGoalOrTouching();
            static public readonly FinishMode InteractionSpot = new FinishModeInteractionSpot();


            public virtual bool IsFinish(Vector3 goal, Vector3 current, Vector3 neighbor) { throw new Exception(); }
            public abstract bool IsFinish(Actor actor, Vector3 goal, Vector3 current);

            public class FinishModeDefault : FinishMode
            {
                public override bool IsFinish(Actor actor, Vector3 goal, Vector3 current)
                {
                    if ((goal.Z < current.Z - 1) || (goal.Z > current.Z + actor.Physics.Reach))
                        return false;
                    var xdist = Math.Abs(goal.X - current.X);
                    var ydist = Math.Abs(goal.Y - current.Y);
                    if ((xdist == 1 && ydist == 0) || (xdist == 0 && ydist == 1))
                        return true;
                    return false;
                }
            }
            class FinishModeOnGoal : FinishMode
            {
                public override bool IsFinish(Actor actor, Vector3 g, Vector3 c)
                {
                    return g == c;
                }
            }
            class FinishModeInteractionSpot : FinishMode
            {
                public override bool IsFinish(Actor actor, Vector3 g, Vector3 c)
                {
                    return Cell.GetFreeInteractionSpots(actor.Map, g, actor).Contains(c);
                }
            }
            class FinishModeOnGoalOrTouching : FinishMode
            {
                public override bool IsFinish(Actor actor, Vector3 g, Vector3 c)
                {
                    if (g == c)
                        return true;
                    if ((g.Z < c.Z - 1) || (g.Z > c.Z + actor.Physics.Reach))
                        return false;
                    var xdist = Math.Abs(g.X - c.X);
                    var ydist = Math.Abs(g.Y - c.Y);
                    if ((xdist == 1 && ydist == 0) || (xdist == 0 && ydist == 1))
                        return true;
                    return false;
                }
            }
        }
    }
}
