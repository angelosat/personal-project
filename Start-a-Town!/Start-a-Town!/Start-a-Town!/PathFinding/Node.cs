using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.PathFinding
{
    public abstract class NodeBase
    {
        public IMap Map;
        public Vector3 Global;
        public NodeBase Parent;
        public float CostToGoal;
        public float CostFromStart;
    }
    public class Node : NodeBase
    {
        //public Vector3 Global { get; set; }
        //public NodeBase Parent { get; set; }
        Func<Vector3, Vector3, float> Heuristic;
        //public float CostToGoal, 
        // public float  CostFromStart;
        public Node(IMap map, Vector3 global, Vector3 goal, Func<Vector3, Vector3, float> h)
        {
            this.Map = map;
            this.Global = global;
            this.CostToGoal = h(global, goal);// Heuristic(global, goal);
        }
        //public Node(IMap map, Vector3 global, Vector3 goal)
        //{
        //    this.Map = map;
        //    this.Global = global;
        //    this.CostToGoal = Heuristic(global, goal);
        //}

        public override string ToString()
        {
            return this.Global.ToString() + " from " + (this.Parent != null ? this.Parent.Global.ToString() : "null");
        }
    }
}
