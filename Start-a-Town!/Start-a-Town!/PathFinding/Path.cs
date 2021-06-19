using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.PathFinding
{
    public class Path : IEnumerator<Vector3>, ISaveable
    {
        public Stack<TargetArgs> StackTargets = new Stack<TargetArgs>();
        public Stack<Vector3> Stack;// = new Stack<Vector3>();
        //Stack<List<Vector3>> PassecCells = new Stack<List<Vector3>>();
        Vector3[] Steps;
        int CurrentStepIndex;
        //public Vector3 CurrentStep
        //{
        //    get
        //    {
        //        return this.Steps[CurrentStepIndex];
        //    }
        //}
        public Vector3 Current => this.Steps[CurrentStepIndex];
        object IEnumerator.Current => this.Current;
        public Vector3 Previous => this.Steps[CurrentStepIndex-1];
        public bool IsLastStep => this.CurrentStepIndex == this.Steps.Length - 1;
        //HashSet<Vector3> CellsToTraverse = new HashSet<Vector3>();
        HashSet<Vector3> CellsToTraverse = new HashSet<Vector3>();

        public void Build(NodeBase node)
        {
            this.Stack = new Stack<Vector3>();
            var current = node;
            while (current.Parent != null)
            {
                this.Stack.Push(current.Global);
                current = current.Parent;
            }
            this.StackTargets = new Stack<TargetArgs>();
            var currentnode = node;
            while (current.Parent != null)
            {
                this.StackTargets.Push(currentnode.Target);
                currentnode = currentnode.Parent;
            }
            this.Steps = this.Stack.ToArray();
        }
        internal void Build(NodeBase node, Vector3 start)
        {
            var stack = new Stack<Vector3>();
            //var toTraverse = new HashSet<Vector3>();
            var current = node;
            while (current.Parent != null)
            {
                stack.Push(current.Global);

                var curr = current.Global;
                var prev = current.Parent.Global;
                if (curr.Z == prev.Z)
                {
                    //var cells = new HashSet<Vector3>(Line.Plot3dThickFrom2d(prev, curr));
                    var cells = current.CellsToTraverse;
                    //if(cells!=null) // if the the path only has one step (the actor is already at the destination), then the cellstotraverse collection will be null
                        foreach (var c in cells)
                            this.CellsToTraverse.Add(c);
                }
                current = current.Parent;
            }

            //if (current.Global.Z == start.Z)
            //{
            //    var cells = new HashSet<Vector3>(Line.Plot3dThickFrom2d(start, current.Global));
            //    foreach (var c in cells)
            //        this.CellsToTraverse.Add(c);
            //}

            this.Steps = stack.ToArray();
        }
        public Path()
        {

        }
        public Path(List<Vector3> steps)
        {
            this.Stack = new Stack<Vector3>(steps);
        }
        public override string ToString()
        {
            var text = "";
            foreach (var item in this.Stack)
                text += item.ToString() + "\n";
            return text.TrimEnd('\n');
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            //var prev = this.Current;
            this.CurrentStepIndex++;
            //var next = this.Current;
            //if (prev.Z == next.Z)
            //    this.CellsToTraverse = new HashSet<Vector3>(Line.Plot3dThickFrom2d(prev, next));
            return this.CurrentStepIndex < this.Steps.Length;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public bool IsValid(Actor entity)
        {
            var global = entity.Global.SnapToBlock();
            if (this.CellsToTraverse.Contains(global))
                this.CellsToTraverse.Remove(global);
            foreach(var cell in this.CellsToTraverse)
            {
                if (PathingSync.LosPathableFailCondition(entity.Map, cell))
                    return false;
            }
            return true;
        }

        public void ConformToBlockHeights(IMap map)
        {
            //var newstack = new Stack<Vector3>();
            //foreach (var step in path.Stack.Reverse())
            //{
            //    var blockHeight = Block.GetBlockHeight(map, step - Vector3.UnitZ);
            //    newstack.Push(new Vector3(step.X, step.Y, step.Z + blockHeight - 1));
            //}
            //path.Stack = newstack;
            for (int i = 0; i < this.Steps.Length; i++)
            {
                var step = this.Steps[i];
                var blockHeight = Block.GetBlockHeight(map, step - Vector3.UnitZ);
                this.Steps[i] = new Vector3(step.X, step.Y, step.Z + blockHeight - 1);
            }
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            //tag.Add(this.CurrentStepIndex.Save("CurrentStepIndex"));
            this.CurrentStepIndex.Save(tag, "CurrentStepIndex");
            this.Steps.Save(tag, "Steps");
            this.CellsToTraverse.Save(tag, "CellsToTraverse");
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            this.CurrentStepIndex.TryLoad(tag, "CurrentStepIndex");
            //this.Steps.TryLoad(tag, "Steps");
            //tag.TryLoad("Steps", out this.Steps);
            this.CellsToTraverse.TryLoad(tag, "CellsToTraverse");

            tag.TryLoad("Steps", ref this.Steps);
            tag.TryLoadCollection("CellsToTraverse", ref this.CellsToTraverse);

            return this;
        }
    }
}
