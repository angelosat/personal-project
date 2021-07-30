using System;
using System.IO;

namespace Start_a_Town_
{
    partial class ItemPreferencesManager
    {
        class ItemPreference : ISaveable, ISerializable
        {
            public ItemRole Role;
            public int ItemRefId;
            public int Score;
            public ItemPreference()
            {

            }
            public ItemPreference(ItemRole role)
            {
                this.Role = role;
                this.ItemRefId = 0;
                this.Score = 0;
            }
            public void CopyFrom(ItemPreference pref)
            {
                if (this.Role != pref.Role)
                    throw new Exception();
                this.ItemRefId = pref.ItemRefId;
                this.Score = pref.Score;
            }
            public override string ToString()
            {
                return $"{Role}:{ItemRefId}:{Score}";
            }

            public void Write(BinaryWriter w)
            {
                w.Write(this.Role.ToString());
                w.Write(this.ItemRefId);
                w.Write(this.Score);
            }

            public ISerializable Read(BinaryReader r)
            {
                this.Role = RegistryByName[r.ReadString()];
                this.ItemRefId = r.ReadInt32();
                this.Score = r.ReadInt32();
                return this;
            }

            public SaveTag Save(string name = "")
            {
                var tag = new SaveTag(SaveTag.Types.Compound, name);
                this.Role.ToString().Save(tag, "Role");
                this.ItemRefId.Save(tag, "ItemRefId");
                this.Score.Save(tag, "Score");
                return tag;
            }

            public ISaveable Load(SaveTag tag)
            {
                this.Role = RegistryByName[(string)tag["Role"].Value];
                this.ItemRefId = (int)tag["ItemRefId"].Value;
                this.Score = (int)tag["Score"].Value;
                return this;
            }

            internal void Clear()
            {
                this.ItemRefId = 0;
                this.Score = 0;
            }
        }
    }
}
