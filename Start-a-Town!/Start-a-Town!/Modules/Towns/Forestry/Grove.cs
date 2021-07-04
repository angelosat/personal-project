using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.AI;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Towns.Forestry
{
    public class Grove : Zone// : Designation
    {
        ChoppingManager Parent;
        //float _TargetDensity = .5f;
        //public float TargetDensity
        //{
        //    get
        //    {
        //        return this._TargetDensity;
        //    }
        //    set
        //    {
        //        this._TargetDensity = value; 
        //    }
        //}
        string CurrentName = "";
        public string Name
        {
            get { return string.IsNullOrEmpty(this.CurrentName) ? "Grove " + this.ID.ToString() : this.CurrentName; }
            set { this.CurrentName = value; }
        }
        public float TargetDensity = .5f;

        //int TreesToAdd;
        //public void SetDensity(float value)
        //{
        //    var oldDensity = this.TargetDensity;
        //    this.TargetDensity = value;
        //    if(oldDensity != value)
        //    {
        //        var size = this.Width * this.Height;
        //        var oldtrees = oldDensity * size;
        //        var newtrees = value * size;
        //        var diff = newtrees - oldtrees;
        //        this.TreesToAdd = (int)diff;
        //    }
        //}

        //HashSet<Vector3> PendingPlantings = new HashSet<Vector3>();
        Dictionary<Vector3, AIJob> PendingPlantings = new Dictionary<Vector3, AIJob>();

        Dictionary<GameObject, AIJob> PendingChoppings = new Dictionary<GameObject, AIJob>();
        Dictionary<Vector3, AIJob> PendingSaplingRemovals = new Dictionary<Vector3, AIJob>();

        List<Vector3> GetCurrentTrees(out List<GameObject> treelist, out List<Vector3> saplingBlocks)
        {
            //foreach(var pos in this.Positions)
            //{
            //    var global = pos + Vector3.UnitZ;

            //}
            var end = this.Begin + new Vector3(this.Width, this.Height, 0);
            treelist = (from entity in this.Town.Map.GetObjects(new BoundingBox(this.Begin, end)) where entity.HasComponent<TreeComponent>() select entity).ToList();
            //var trees = (from entity in this.Town.Map.GetObjects(new BoundingBox(this.Begin, end)) where entity.HasComponent<TreeComponent>() select entity.Global).ToList();
            var trees = treelist.Select(t => t.Global).ToList();
            saplingBlocks = new List<Vector3>();
            foreach (var pos in this.Begin.GetBox(this.Begin + new Vector3(this.Width, this.Height, 1) - Vector3.One))
            {
                var block = this.Town.Map.GetBlock(pos);
                if (block == BlockDefOf.Sapling)
                {
                    saplingBlocks.Add(pos);
                    trees.Add(pos);
                }
            }
            trees.AddRange(this.PendingPlantings.Keys);
            // TODO: do i want to plant new trees before chopping down existing ones?
            trees = trees.Except(this.PendingChoppings.Keys.Select(t => t.Global)).ToList();
            trees = trees.Except(this.PendingSaplingRemovals.Keys).ToList();
            return trees;
        }
        public Grove(ChoppingManager parent, int id, Vector3 global, int w, int h)
            : base(parent.Town, id, global, w, h)
        {
            this.Parent = parent;
        }
        bool IsGroundValid(Vector3 ground)
        {
            var block = this.Town.Map.GetBlock(ground);
            if (!(block == BlockDefOf.Soil || block == BlockDefOf.Grass))
                return false;
            var blockAbove = this.Town.Map.GetBlock(ground+Vector3.UnitZ);
            if (blockAbove != BlockDefOf.Air)
                return false;
            return true;
        }

        public GroupBox GetInterface()
        {
            return new GroveUI(this);
        }

        public override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Message.Types.BlockChanged:
                    var map = e.Parameters[0] as IMap;
                    var global = (Vector3)e.Parameters[1];
                    if (map.GetBlock(global) == BlockDefOf.Sapling)
                        this.PendingPlantings.Remove(global);
                    break;
                    
                default:
                    break;
            }
        }
        public SaveTag Save(string name)
        {
            SaveTag tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.ID.Save("ID"));
            tag.Add(this.CurrentName.Save("Name"));
            return tag;
        }
    }
}
