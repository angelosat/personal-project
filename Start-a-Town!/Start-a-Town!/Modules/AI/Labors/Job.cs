using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class Job : ISerializable, ISaveable
    {
        public Entity PreferredTool;
        public JobDef Def;
        const byte InitialPriority = 5, MaxPriority = 10;
        byte _Priority = InitialPriority;
        public byte Priority
        {
            get { return this._Priority; }
            set
            {
                this._Priority = (byte)(value >= 0 ? value % MaxPriority : MaxPriority + (value % MaxPriority));
                //this._Enabled = this._Priority != 0;
            }
        }
        //bool _Enabled;
        public bool Enabled
        {
            //get
            //{
            //    return this._Enabled && this._Priority != 0;
            //}
            //set
            //{
            //    this._Enabled = value;
            //}

            get { return this._Priority != 0; }
            //set
            //{
            //    if (value && this._Priority == 0)
            //        this._Priority = InitialPriority;
            //    else if (!value && this._Priority != 0)
            //        this._Priority = 0;
            //}
        }
        public Job()
        {

        }
        public Job(JobDef def)
        {
            this.Def = def;
        }
        public void Toggle()
        {
            //this.Enabled = !this.Enabled;
            this.Priority = (byte)(this.Priority == 0 ? InitialPriority : 0);
        }
        public override string ToString()
        {
            return $"{this.Def.Name}: {this.Priority}";
        }
        public void Write(BinaryWriter w)
        {
            //w.Write(this._Enabled);
            w.Write(this.Def.Name);
            w.Write(this._Priority);
        }

        public ISerializable Read(BinaryReader r)
        {
            //this.Enabled = r.ReadBoolean();
            this.Def = Start_a_Town_.Def.GetDef<JobDef>(r.ReadString());
            this.Priority = r.ReadByte();
            return this;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Priority.Save(tag, "Priority");
            this.Def.Name.Save(tag, "Def");
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            this.Priority = tag.GetValue<byte>("Priority");
            tag.TryGetTagValue<string>("Def", v => this.Def = Start_a_Town_.Def.GetDef<JobDef>(v));
            return this;
        }
    }
}
