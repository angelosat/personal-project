using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class StorageFilterCategoryNew
    {
        public string Label;
        public List<StorageFilterCategoryNew> Children = new();
        public List<StorageFilterNew> Leaves = new();

        public StorageFilterCategoryNew(string label)
        {
            this.Label = label;
        }
        public StorageFilterCategoryNew AddChildren(IEnumerable<StorageFilterCategoryNew> cats)
        {
            return this.AddChildren(cats.ToArray());
        }

        public StorageFilterCategoryNew AddChildren(params StorageFilterCategoryNew[] cats)
        {
            this.Children.AddRange(cats);
            return this;
        }
        public StorageFilterCategoryNew AddLeafs(IEnumerable<StorageFilterNew> filters)
        {
            return this.AddLeafs(filters.ToArray());
        }
        public StorageFilterCategoryNew AddLeafs(params StorageFilterNew[] filters)
        {
            this.Leaves.AddRange(filters);
            return this;
        }
        internal bool FindNodeIndex(StorageFilterCategoryNew c, out int i)
        {
            i = 0;
            foreach (var item in this.GetAllDescendantNodes())
            {
                if (c == item)
                    return true;
                i++;
            }
            return false;
        }
        internal bool FindLeafIndex(StorageFilterNew c, out int i)
        {
            i = 0;
            foreach (var item in this.GetAllDescendantLeaves())
            {
                if (c == item)
                    return true;
                i++;
            }
            return false;
        }
        
        internal StorageFilterNew GetLeafByIndex(int i)
        {
            var n = 0;
            var enumerator = this.GetAllDescendantLeaves().GetEnumerator();
            do { enumerator.MoveNext(); } while (n++ != i);
            return enumerator.Current;
        }
        internal StorageFilterCategoryNew GetNodeByIndex(int i)
        {
            var n = 0;
            var enumerator = this.GetAllDescendantNodes().GetEnumerator();
            do { enumerator.MoveNext(); } while (n++ != i);
            return enumerator.Current;
        }
        internal IEnumerable<StorageFilterNew> GetAllDescendantLeaves()
        {
            var queue = new Queue<StorageFilterCategoryNew>();
            queue.Enqueue(this);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                foreach (var leaf in current.Leaves)
                    yield return leaf;
                foreach (var child in current.Children)
                    queue.Enqueue(child);
            }
        }
        internal IEnumerable<StorageFilterCategoryNew> GetAllDescendantNodes()
        {
            var queue = new Queue<StorageFilterCategoryNew>();
            queue.Enqueue(this);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                yield return current;
                foreach (var child in current.Children)
                    queue.Enqueue(child);
            }
        }
       
        public Control GetGui(Action<int[], int[]> callback)
        {
            bool isEnabled(StorageFilterCategoryNew cat) => cat.Leaves.All(l => l.Enabled) && cat.Children.All(isEnabled);

            var box = new ScrollableBoxNewNew(300, 400, ScrollModes.Vertical);
            //var listcollapsible = new ListBoxCollapsible(300, 400);
            var listcollapsible = new ListCollapsibleNew();

            ListBoxCollapsibleNode createNode(StorageFilterCategoryNew cat)
            {
                var node = new ListBoxCollapsibleNode(cat.Label, n => new CheckBoxNew() 
                {
                    TickedFunc = () => isEnabled(cat),
                    LeftClickAction = () => 
                    {
                        this.FindNodeIndex(cat, out var k);
                        callback(new int[] { k }, null);
                    }
                });
                foreach (var child in cat.Children)
                    node.AddNode(createNode(child));
                foreach (var leaf in cat.Leaves)
                    node.AddLeaf(new CheckBoxNew(leaf.Label)
                    {
                        TickedFunc = ()=>leaf.Enabled, 
                        LeftClickAction = () => { 
                            this.FindLeafIndex(leaf, out var i);
                            callback(null, new int[] { i });
                        }
                    });
                return node;
            }
            listcollapsible.AddNode(createNode(this));
            listcollapsible.Build();
            //return listcollapsible;
            box.AddControls(listcollapsible);
            return box;
        }

        public bool Filter(Entity obj)
        {
            return this.Leaves.Any(f => f.Enabled && f.Condition(obj)) || this.Children.Any(c => c.Filter(obj));
        }
    }
}
