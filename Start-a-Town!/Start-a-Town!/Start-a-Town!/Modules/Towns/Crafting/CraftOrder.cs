using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.AI;

namespace Start_a_Town_.Towns.Crafting
{
    public class CraftOrder
    {
        public enum States { Stopped, Assigned, Finished }
        public States State;
        
        public int ID;
        public CraftOperation Craft;
        public int Amount;
        //public Vector3 Workstation;
        public AIJob Job;

        public CraftOrder(int id, CraftOperation craft, int amount)//, Vector3 workstation)
        {
            this.ID = id;
            this.Craft = craft;
            this.Amount = amount;
            //this.Workstation = workstation;
        }

        public bool IsAvailable()
        {
            return this.State == States.Stopped;
        }

        public SaveTag Save()
        {
            var tag = new SaveTag(SaveTag.Types.Compound);
            tag.Add(this.ID.Save("ID"));
            tag.Add(Craft.Save("Craft"));
            //tag.Add(this.Workstation.Save("Workstation"));
            return tag;
        }
        public void Load(SaveTag tag)
        {
            this.ID = tag.GetValue<int>("ID");
            tag.TryGetTag("Craft", v => this.Craft = new CraftOperation(v));
            //this.Workstation = tag.LoadVector3();
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            this.Craft.Write(w);
            //w.Write(this.Workstation);
        }
        public void Read(BinaryReader r)
        {
            this.ID = r.ReadInt32();
            this.Craft = new CraftOperation(r);
            //this.Workstation = r.ReadVector3();
        }
        public CraftOrder(SaveTag tag)
        {
            this.Load(tag);
        }

        public CraftOrder(BinaryReader r)
        {
            this.Read(r);
        }
    }
}
