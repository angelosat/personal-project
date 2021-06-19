using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.PathFinding
{
    public class Path
    {
        public Stack<Vector3> Stack = new Stack<Vector3>();
        public void Build(NodeBase node)
        {
            this.Stack = new Stack<Vector3>();
            var current = node;
            while (current.Parent != null)
            {
                this.Stack.Push(current.Global);
                current = current.Parent;
            }
        }

        public override string ToString()
        {
            var text = "";
            foreach (var item in this.Stack)
                text += item.ToString() + "\n";
            return text.TrimEnd('\n');
        }
    }
}
