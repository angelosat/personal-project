using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class RegionNodeCollection : IEnumerable<KeyValuePair<Vector3, RegionNode>>
    {
        Dictionary<Vector3, RegionNode> Nodes = new Dictionary<Vector3, RegionNode>();
        public IEnumerator<KeyValuePair<Vector3, RegionNode>> GetEnumerator() { return this.Nodes.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
        Region Region;
        public RegionNodeCollection(Region region)
        {
            this.Region = region;
        }
        public void Clear()
        {
            this.Nodes.Clear();
        }
        public bool ContainsKey(Vector3 key)
        {
            return this.Nodes.ContainsKey(key);
        }
        public RegionNode GetValueOrDefault(Vector3 key)
        {
            this.Nodes.TryGetValue(key, out RegionNode val);
            return val;
            //return this.Nodes.GetValueOrDefault(key);
        }
        public bool TryGetValue(Vector3 key, out RegionNode value)
        {
            return this.Nodes.TryGetValue(key, out value);
        }
        public Dictionary<Vector3, RegionNode>.ValueCollection Values
        {
            get { return this.Nodes.Values; }
        }
        public int Count
        {
            get { return this.Nodes.Count; }
        }
        public RegionNode this[Vector3 key]
        {
            get { return this.Nodes[key]; }
            set { this.Nodes[key] = value; }
        }
        public bool Remove(Vector3 key)
        {
            return this.Nodes.Remove(key);
        }
        public void Add(RegionNode node)
        {
            if (node.Region != this.Region)
            {
                if (node.Region != null)
                    node.Region.Nodes.Remove(node.Global);
                node.Region = this.Region;
            }
            this.Nodes.Add(node.Global, node);
            //this.Nodes[node.Global] = node; // SHOULD I RESTRICT ?
        }
        //public void Add(Vector3 key, RegionNode value)
        //{
        //    if (value.Region != this.Region)
        //    {
        //        if (value.Region != null)
        //            value.Region.Remove(value);
        //        value.Region = this.Region;
        //    }
        //    this.Nodes.Add(key, value);
        //}

        internal void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Nodes.Count);
            foreach(var node in this.Nodes)
            {
                throw new NotImplementedException();
            }
        }
    }

    //public class RegionNodeCollection : Dictionary<Vector3, RegionNode>
    //{
    //    //Dictionary<Vector3, RegionNode> Nodes = new Dictionary<Vector3, RegionNode>();
    //    //public IEnumerator<KeyValuePair<Vector3, RegionNode>> GetEnumerator() { return this.Nodes.GetEnumerator(); }

    //    //public void Clear()
    //    //{
    //    //    this.Nodes.Clear();
    //    //}
    //    //public bool ContainsKey(Vector3 key)
    //    //{
    //    //    return this.ContainsKey(key);
    //    //}
    //    //public RegionNode GetValueOrDefault(Vector3 key)
    //    //{
    //    //    return this.Nodes.GetValueOrDefault(key);
    //    //}
    //    new public void Add(Vector3 key, RegionNode value)
    //    {

    //    }
    //}
    
}
