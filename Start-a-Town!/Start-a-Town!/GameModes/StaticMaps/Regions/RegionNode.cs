using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class RegionNode
    {
        public Vector3 Local, Global;
        public RegionNode North, West, East, South;

        //RegionNode _North, _West, _East, _South;
        //public RegionNode North
        //{
        //    get { return this._North; }
        //    set
        //    {
        //        this._North = value;
        //        value._South = this;
        //    }
        //}
        //public RegionNode East
        //{
        //    get { return this._East; }
        //    set
        //    {
        //        this._East = value;
        //        value._West = this;
        //    }
        //}
        //public RegionNode West
        //{
        //    get { return this._West; }
        //    set
        //    {
        //        this._West = value;
        //        value._East = this;
        //    }
        //}
        //public RegionNode South
        //{
        //    get { return this._South; }
        //    set
        //    {
        //        this._South = value;
        //        value._North = this;
        //    }
        //}

        public override string ToString()
        {
            return string.Format("Global: {0} Region: {1} Links: {2}", this.Global, this.Region.RegionID, this.GetLinks().Count());
        }
        
        public Region Region;
        public RegionRoom Room { get { return this.Region.Room; } }
        public Chunk Chunk { get { return this.Region.Chunk; } }

        public IEnumerable<RegionNode> GetLinksLazy()
        {
            if (this.North != null) yield return this.North;
            if (this.West != null) yield return this.West;
            if (this.East != null) yield return this.East;
            if (this.South != null) yield return this.South;
        }
        public IEnumerable<RegionNode> GetLinks()
        {
            //if (this.North != null) yield return this.North;
            //if (this.West != null) yield return this.West;
            //if (this.East != null) yield return this.East;
            //if (this.South != null) yield return this.South;
            var list = new List<RegionNode>();
            if (this.North != null) list.Add(this.North);
            if (this.West != null) list.Add(this.West);
            if (this.East != null) list.Add(this.East);
            if (this.South != null) list.Add(this.South);
            return list;
        }
        public RegionNode(Region region, Vector3 local)
        {
            this.Region = region;
            this.Global = local.ToGlobal(region.Chunk);
            this.Local = local;
        }
        public bool AtEdge()
        {
            return this.Local.X == 0 || this.Local.X == Chunk.Size - 1
                || this.Local.Y == 0 || this.Local.Y == Chunk.Size - 1;
        }
        public void Replace(RegionNode oldNode)
        {
            if (oldNode.North != null && Math.Abs(this.Global.Z - oldNode.North.Global.Z) <= 1) this.North = oldNode.North;
            if (oldNode.South != null && Math.Abs(this.Global.Z - oldNode.South.Global.Z) <= 1) this.South = oldNode.South;
            if (oldNode.East != null && Math.Abs(this.Global.Z - oldNode.East.Global.Z) <= 1) this.East = oldNode.East;
            if (oldNode.West != null && Math.Abs(this.Global.Z - oldNode.West.Global.Z) <= 1) this.West = oldNode.West;
        }
        public bool TryReplace(RegionNode oldNode)
        {
            this.Replace(oldNode);
            if( this.GetLinks().Any())
            {
                if (this.North != null) this.North.South = this;
                if (this.South != null) this.South.North = this;
                if (this.East != null) this.East.West = this;
                if (this.West != null) this.West.East = this;
                return true;
            }
            return false;
        }

        internal void ClearLinks()
        {
            if (this.West != null)
                this.West.East = null;
            if (this.East != null)
                this.East.West = null;
            if (this.North != null)
                this.North.South = null;
            if (this.South != null)
                this.South.North = null;
            this.North = null;
            this.South = null;
            this.West = null;
            this.East = null;
        }
        //internal void Replace(RegionNode oldNode)
        //{
        //    this.North = oldNode.North;
        //    this.South = oldNode.South;
        //    this.West = oldNode.West;
        //    this.East = oldNode.East;
        //    if (this.West != null)
        //        this.West.East = this;
        //    if (this.East != null)
        //        this.East.West = this;
        //    if (this.North != null)
        //        this.North.South = this;
        //    if (this.South != null)
        //        this.South.North = this;
        //}

        internal void Delete()
        {
            this.Region.Remove(this);
        }

        internal void SetRegion(Region newRegion)
        {
            this.Region.Remove(this);
            newRegion.Add(this);
        }
        public RegionNode Link(RegionNode otherNode)
        {
            if (this.Global.North().XY() == otherNode.Global.XY())
            {
                this.North = otherNode;
                otherNode.South = this;
            }
            else if (this.Global.South().XY() == otherNode.Global.XY())
            {
                this.South = otherNode;
                otherNode.North = this;
            }
            else if (this.Global.East().XY() == otherNode.Global.XY())
            {
                this.East = otherNode;
                otherNode.West = this;
            }
            else if (this.Global.West().XY() == otherNode.Global.XY())
            {
                this.West = otherNode;
                otherNode.East = this;
            }
            else
                throw new Exception();
            return this;
        }
    }
}
