using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_
{
    class UndoableCollection : Stack<IUndoable> //where TObject : IUndoable
    {
        public int Index;
        public UndoableCollection() : base() { }
        public UndoableCollection(IEnumerable<IUndoable> collection) : base(collection) { }
        //public void Reset()
        //{
        //    this.Index = 0;
        //}

        public void Undo()
        {
            if (Count == 0)
                return;
            IUndoable op = this.ElementAt(Index);// - 1);
            if (op.IsNull())
                return;
            if (op.Undo())
                Index = Math.Min(Count - 1, Index + 1);
        }
        public void Redo()
        {
            if (Count == 0)
                return;
            if (Index == 0)
                return;
            IUndoable op = this.ElementAt(Index - 1);
            if (op.IsNull())
                return;
            if (op.Redo())
                Index = Math.Max(0, Index - 1);
        }

        public new void Push(IUndoable item)
        {
            while (Index > 0)
            {
                Index--;
                this.Pop();
            }
            base.Push(item);
        }
        public void Push(IEnumerable<IUndoable> items)
        {
            foreach (var item in items)
            {
                this.Push(item);
            }
        }
    }
}
