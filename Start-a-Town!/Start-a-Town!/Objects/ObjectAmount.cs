using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class ObjectAmount : ISaveable//, ISerializable
    {
        TargetArgs ObjectTarget;
        public GameObject Object
        {
            get { return this.ObjectTarget.Object; }
            set { this.ObjectTarget = new TargetArgs(value); }
        }
        int _Amount;
        public int Amount
        {
            get
            {
                return this._Amount;
            }
            set
            {
                if (this.Object == null)
                    throw new Exception();
                if (value > this.Object.StackMax)
                    throw new Exception();
                this._Amount = Math.Max(0, value);
            }
        }
        public ObjectAmount()
        {

        }
        public ObjectAmount(GameObject obj)
        {
            this.Object = obj;
            this.Amount = obj.StackSize;
        }
        public ObjectAmount(GameObject obj, int amount)
        {
            this.Object = obj;
            this.Amount = amount;
        }
        public ObjectAmount((Entity i, int amount) tuple)
        {
            this.Object = tuple.i;
            this.Amount = tuple.amount;
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.ObjectTarget.Save("Object"));
            tag.Add(this.Amount.Save("Amount"));
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTag("Object", t => this.ObjectTarget = new TargetArgs(t));
            tag.TryGetTagValue<int>("Amount", t => this._Amount = t);
            return this;
        }
        public override string ToString()
        {
            return this.Object.Name + ": " + this.Amount.ToString();
        }
        //public void Write(BinaryWriter w)
        //{
        //    this.ObjectTarget.Write(w);
        //    w.Write(this.Amount);
        //}

        //public ISerializable Read(BinaryReader r)
        //{
        //    this.ObjectTarget = TargetArgs.Read(r);
        //    this.Amount = r.ReadInt32();
        //    return this;
        //}
        //public void MapLoaded(IMap map)
        //{
        //    this.ObjectTarget.Map = map;
        //}
    }
}
