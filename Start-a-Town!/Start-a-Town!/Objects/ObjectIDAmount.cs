using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    [Obsolete]
    public class ObjectIDAmount : ISaveable, ISerializable
    {
        public int ObjectID;
        public int Amount;
        
        public ObjectIDAmount()
        {

        }
        public ObjectIDAmount(int objID)
        {
            this.ObjectID = objID;
            this.Amount = GameObject.Objects[objID].StackMax;
        }
        public ObjectIDAmount(int objID, int amount)
        {
            this.ObjectID = objID;
            this.Amount = amount;
            //if (amount > GameObject.Objects[objID].StackMax)
            //    throw new Exception();
            if (amount < 0)
                throw new Exception();
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.ObjectID.Save("ObjectID"));
            tag.Add(this.Amount.Save("Amount"));
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValue<int>("ObjectID", out this.ObjectID);
            tag.TryGetTagValue<int>("Amount", out this.Amount);
            return this;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.ObjectID);
            w.Write(this.Amount);
        }

        public ISerializable Read(BinaryReader r)
        {
            this.ObjectID = r.ReadInt32();
            this.Amount = r.ReadInt32();
            return this;
        }
    }
}
