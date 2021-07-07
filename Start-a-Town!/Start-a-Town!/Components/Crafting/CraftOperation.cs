using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Crafting
{
    [Obsolete]
    public class CraftOperation
    {
        public int ReactionID;
        public List<ItemRequirement> Materials = new List<ItemRequirement>();
        public TargetArgs Building = TargetArgs.Null;
        public Container Container;
        //public TargetArgs Tool = TargetArgs.Empty;
        public GameObject Tool;
        public Vector3 WorkstationEntity;
        public Progress CraftProgress = new Progress();


        public CraftOperation(int reactionID, IEnumerable<ItemRequirement> materials, Vector3 WorkstationEntity)
        {
            this.ReactionID = reactionID;
            this.Materials = new List<ItemRequirement>(materials);
            this.WorkstationEntity = WorkstationEntity;
        }
        public CraftOperation(int reactionID, IEnumerable<ItemRequirement> materials, GameObject building)
        {
            this.ReactionID = reactionID;
            this.Materials = new List<ItemRequirement>(materials);
            if (building != null)
                this.Building = new TargetArgs(building);
        }
        public CraftOperation(int reactionID, IEnumerable<ItemRequirement> materials, GameObject building, GameObject tool, Container materialsContainer)
            : this(reactionID, materials, building)
        {
            this.Tool = tool;
            this.Container = materialsContainer;
        }
        public CraftOperation(IObjectProvider net, BinaryReader r)
        {
            Read(net, r);
        }

        private void Read(IObjectProvider net, BinaryReader r)
        {
            this.ReactionID = r.ReadInt32();
            int reqCount = r.ReadInt32();
            this.Materials = new List<ItemRequirement>();
            for (int i = 0; i < reqCount; i++)
                this.Materials.Add(new ItemRequirement(r));
            if (r.ReadBoolean())
                this.Building = TargetArgs.Read(net, r);
            //this.Tool = TargetArgs.Read(net, r);
            bool hasTool = r.ReadBoolean();
            if (hasTool)
                this.Tool = net.GetNetworkObject(r.ReadInt32());

            var hasContainer = r.ReadBoolean();
            if (hasContainer)
            {
                var containerEntity = net.GetNetworkObject(r.ReadInt32());
                this.Container = containerEntity.GetContainer(r.ReadInt32());
            }

            this.WorkstationEntity = r.ReadVector3();
        }
        public void WriteOld(System.IO.BinaryWriter w)
        {
            w.Write(this.ReactionID);
            w.Write(this.Materials.Count);
            foreach (var item in this.Materials)
                item.Write(w);

            w.Write(this.Building != null);
            if (this.Building != null)
                this.Building.Write(w);

            w.Write(this.Tool != null);
            if (this.Tool != null)
                //this.Tool.Write(w);
                w.Write(this.Tool.RefID);

            w.Write(this.Container != null);
            if (this.Container != null)
            {
                w.Write(this.Container.Parent.RefID);
                w.Write(this.Container.ID);
            }

            w.Write(this.WorkstationEntity);
        }
        public void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.ReactionID);
            w.Write(this.Materials.Count);
            foreach (var item in this.Materials)
                item.Write(w);
            w.Write(this.WorkstationEntity);
            this.CraftProgress.Write(w);
        }
        private void Read(BinaryReader r)
        {
            this.ReactionID = r.ReadInt32();
            int reqCount = r.ReadInt32();
            this.Materials = new List<ItemRequirement>();
            for (int i = 0; i < reqCount; i++)
                this.Materials.Add(new ItemRequirement(r));
            this.WorkstationEntity = r.ReadVector3();
            this.CraftProgress.Read(r);
        }
        public SaveTag Save(string name)
        {
            SaveTag tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.ReactionID.Save("ReactionID"));
            var matsTag = new SaveTag(SaveTag.Types.List, "Materials", SaveTag.Types.Compound);
            foreach (var mat in this.Materials)
                matsTag.Add(new SaveTag(SaveTag.Types.Compound, "", mat.Save()));
            tag.Add(matsTag);
            tag.Add(this.WorkstationEntity.SaveOld("Workstation"));
            tag.Add(this.CraftProgress.Save("Progress"));
            return tag;
        }
      
        public void Load(SaveTag tag)
        {
            tag.TryGetTagValue<int>("ReactionID", v => this.ReactionID = v);
            tag.TryGetTagValue<List<SaveTag>>("Materials", v =>
            {
                foreach (var mattag in v)
                    this.Materials.Add(new ItemRequirement(mattag));
            });
            //tag.TryGetTagValue<SaveTag>("Workstation", v => this.WorkstationEntity = v.ToVector3());
            tag.TryGetTag("Workstation", v => this.WorkstationEntity = v.LoadVector3());
            tag.TryGetTag("Progress", v => this.CraftProgress.Load(v));
            //tag.ToVector3();
        }
        public CraftOperation(SaveTag tag)
        {
            this.Load(tag);
        }
        public CraftOperation(BinaryReader r)
        {
            this.Read(r);
        }


        public Reaction.Product.ProductMaterialPair GetProduct()
        {
            var reaction = Reaction.Dictionary[this.ReactionID];
            return Reaction.Dictionary[this.ReactionID].Products.First().GetProduct(reaction, null, this.Materials, null);
        }
    }
}
