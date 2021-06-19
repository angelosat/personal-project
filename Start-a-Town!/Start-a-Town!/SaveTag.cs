﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class SaveTag// : IEnumerable
    {
        public enum Types : byte
        {
            End,
            Byte,
            Short,
            Int,
            Long,
            Float,
            Double,
            ByteArray,
            String,
            List, //the value of a list tag is always a list of unnamed tags of the same type
            Compound, //the value of a compound tag is always a list of tags that can be accessed by name (a dictionary?)
            UInt16,
            UInt32,
            Bool,
            Vector3,
            Reference
        }

        static Dictionary<string, SaveTag> SaveReferences = new Dictionary<string, SaveTag>();
        static Dictionary<string, ISaveable> LoadReferences = new Dictionary<string, ISaveable>();
        //static Dictionary<string, SaveTag> LoadReferences = new Dictionary<string, SaveTag>();


        public byte Type;
        public string Name;
        public object Value;
        byte ListType;

        //bool HasName 
        //{ get { return Name != null; } }
        

        #region Indexers
        public T GetValue<T>(string name)
        {
            return (T)this[name].Value;
        }
        public T TagValueOrDefault<T>(string name, T def)
        {
            if (!TryGetTag(name, out SaveTag tag))
                return def;
            else
                return (T)tag.Value;
        }
        public TResult TagValueOrDefault<TValue, TResult>(string name, Func<TValue, TResult> valueConvert, TResult valueDefault)
        {
            if (!TryGetTag(name, out var tag))
                return valueDefault;
            else
                return valueConvert((TValue)tag.Value);
        }
        public bool TagValueOrDefault<TValue, TResult>(string name, Func<TValue, TResult> valueConvert, TResult valueDefault, out TResult result)
        {
            if (!TryGetTag(name, out var tag))
            {
                result = valueDefault;
                return false;
            }
            else
            {
                result = valueConvert((TValue)tag.Value);
                return true;
            }
        }
        public bool TryGetTag(string name, Action<SaveTag> action)
        {
            if (!TryGetTag(name, out var tag))
            {
                return false;
            }
            else
            {
                action(tag);
                return true;
            }
        }
        public bool TryGetTagValueNew<TValue>(string name, ref TValue value)
        {
            if (!this.TryGetTag(name, out var tag))
            {
                return false;
            }
            else
            {
                value = (TValue)tag.Value;
                return true;
            }
        }
        public bool TryGetTagValue<TValue>(string name, out TValue value)
        {
            if (!this.TryGetTag(name, out var tag))
            {
                value = default(TValue);
                return false;
            }
            else
            {
                value = (TValue)tag.Value;
                return true;
            }
        }
        public bool TryGetTagValue<TValue>(string name, Action<TValue> valueConvert)
        {
            if (!this.TryGetTag(name, out var tag))
            {
                return false;
            }
            else
            {
                valueConvert((TValue)tag.Value);
                return true;
            }
        }
        public TResult TagOrDefault<TResult>(string name, Func<SaveTag, TResult> valueConvert, TResult valueDefault)
        {
            if (!TryGetTag(name, out var tag))
                return valueDefault;
            else
                return valueConvert(tag);
        }
        public bool TagOrDefault<TResult>(string name, Func<SaveTag, TResult> valueConvert, TResult valueDefault, out TResult result)
        {
            if (!TryGetTag(name, out var tag))
            {
                result = valueDefault;
                return false;
            }
            else
            {
                result = valueConvert(tag);
                return true;
            }
        }

        public bool TryGetTag(string name, out SaveTag tag)
        {
            if (Type != 10)
            {
                tag = null;
                return false;
            }
            //Dictionary<string, SaveTag> dic = (Value as List<SaveTag>).ToDictionary(foo => foo.Name);
            //return dic.TryGetValue(name, out tag);
            //return this.ToDictionary().TryGetValue(name, out tag);
            return (this.Value as Dictionary<string, SaveTag>).TryGetValue(name, out tag);//
        }

        public SaveTag this[string name]
        {
            get
            {
                if (Type != 10)
                    return null;
                //Dictionary<string, SaveTag> dic = (Value as List<SaveTag>).ToDictionary(foo => foo.Name);
                //// TODO: maybe do a tryget
                //return dic[name];

                //return this.ToDictionary()[name];
                return (this.Value as Dictionary<string, SaveTag>)[name];
            }
        }
        //Dictionary<string, SaveTag> _Dictionary;
        

        public Object this[int n]
        {
            get
            {
                if (Type != 9)
                    return null;
                return (Value as List<SaveTag>)[n].Value;
            }
        }
        #endregion

        public override string ToString()
        {
            return Type + ": " + Name + ": " + Value.ToString();
        }

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="value">If the type is compound, the value is dropped unless it's a list of tags. 
        /// If the type is list, then the value can either be a list of tags (from which the type of the list is derived from the first element), 
        /// or the type of the list.</param>
        public SaveTag(Types type, string name = "", Object value = null) : this((byte)type, name, value) { }
    //    public Tag(Types type, object obj = null, Object value = null) : this((byte)type, obj != null ? obj.ToString() : "", value) { }
        SaveTag(byte type, string name = "", Object value = null)
        {
            //this.Dictionary = new Dictionary<string, SaveTag>();
            switch (type)
            {
                case 9:
                    if (value is List<SaveTag>)
                    {
                        ListType = (value as List<SaveTag>)[0].Type;
                        Value = value;
                    }
                    else 
                    {
                        ListType = (byte)value;
                        Value = new List<SaveTag>();
                    }
                    //Value = new List<Tag>();
                    break;
                case 10:
                    if (value is Dictionary<string, SaveTag>)
                        Value = value;
                    else if(value is List<SaveTag>)
                    {
                        // for backwards compatibility
                        Value = (value as List<SaveTag>).ToDictionary(foo => foo.Name);
                    }
                    else
                    {
                        if (value != null)
                            Console.WriteLine(name + " value dropped");
                        Value = new Dictionary<string, SaveTag>();// { Value as Tag }; //when you create a compound tag with a value, it initializes a new list<tag> with the value as its first element
                    }
                    //if (value is List<SaveTag>)
                    //    Value = value;
                    //else
                    //{
                    //    if (value != null)
                    //        Console.WriteLine(name + " value dropped");
                    //    Value = new List<SaveTag>();// { Value as Tag }; //when you create a compound tag with a value, it initializes a new list<tag> with the value as its first element
                    //}
                    break;

                //case 15: //reference
                //    var reference = value as ILoadReferencable;
                //    if (reference == null)
                //        throw new Exception();
                //    var refID = reference.GetUniqueLoadID();
                //    this.Value = refID;
                //    //    //if(!References.TryGetValue(refID, out var existing))
                //    //    //    References[refID] = reference;
                //    //    if (!SaveReferences.ContainsKey(refID))
                //    //    {
                //    //        var reftag = new SaveTag(SaveTag.Types.Compound, refID);
                //    //        //newtag.Add(refID.Save("RefID"));
                //    //        reftag.Add(reference.GetType().FullName.Save("TypeName"));
                //    //        reftag.Add(reference.Save("Data"));
                //    //        SaveReferences[refID] = reftag;
                //    //        //SaveReferences[refID] = reference.Save();
                //    //    }
                //    //    //this.Value = reference; //refID;
                //    //    this.Value = refID;

                //    break;

                default:
                    Value = value;
                    break;
            }
            Type = type;
            Name = name;
        }
        #endregion

        //void Flush(BinaryWriter w)
        //{
        //    this.FlushWithReferences(w);
        //    this.WriteTo(w);
        //}
        public void WriteTo(BinaryWriter w)
        {
            w.Write(Type);
            w.Write(Name);
            Write(w);
        }
        public void WriteWithRefs(BinaryWriter w)
        {
            // write referencables
            w.Write(SaveReferences.Count);
            foreach (var i in SaveReferences)
            {
                i.Value.WriteTo(w);
            }
            SaveReferences.Clear();

            this.WriteTo(w);
        }
        void Write(BinaryWriter writer)
        {
            switch (Type)
            {
                case 0:
                    break;
                case 1:
                    writer.Write((byte)this.Value);
                    break;
                case 2:
                    writer.Write((short)this.Value);
                    break;
                case 3:
                    writer.Write((int)this.Value);
                    break;
                case 4:
                    writer.Write((long)this.Value);
                    break;
                case 5:
                    writer.Write((float)this.Value);
                    break;
                case 6:
                    writer.Write((double)this.Value);
                    break;
                case 7:
                    writer.Write(((byte[])this.Value).Length);
                    writer.Write((byte[])this.Value);
                    break;
                case 8:
                    writer.Write((string)this.Value);
                    break;
                case 9:
                    writer.Write(ListType);
                    writer.Write((Value as List<SaveTag>).Count);
                    foreach (SaveTag tag in (List<SaveTag>)Value)
                        tag.Write(writer);
                    break;
                case 10:
                    Dictionary<string, SaveTag> Tags = (Dictionary<string, SaveTag>)Value;
                    //Tags.Add(new Tag(Tag.Types.End));
                    writer.Write(Tags.Count);
                    foreach (SaveTag tag in Tags.Values)
                    {
                        writer.Write(tag.Type);
                        writer.Write(tag.Name);
                        tag.Write(writer);
                    }
                    writer.Write((byte)0);

                    //List<SaveTag> Tags = (List<SaveTag>)Value;
                    ////Tags.Add(new Tag(Tag.Types.End));
                    //foreach (SaveTag tag in Tags)
                    //{
                    //    writer.Write(tag.Type);
                    //    //if (tag.Type > 0)
                    //    //{
                    //        writer.Write(tag.Name);
                    //        tag.Write(writer);
                    //    //}
                    //}
                    //writer.Write((byte)0);
                    break;
                case 11:
                    writer.Write((UInt16)this.Value);
                    break;
                case 12:
                    writer.Write((UInt32)this.Value);
                    break;
                case 13:
                    writer.Write((bool)this.Value);
                    break;
                case 14:
                    writer.Write((Vector3)this.Value);
                    break;
                case 15: // reference
                    writer.Write((string)this.Value);
                    //writer.Write(this.Name);
                    break;
                default:
                    break;
            }
        }

        static object Read(BinaryReader reader, byte type)
        {
            switch (type)
            {
                case 0:
                    return null;
                case 1:
                    return reader.ReadByte();
                case 2:
                    return reader.ReadInt16();
                case 3:
                    return reader.ReadInt32();
                case 4:
                    return reader.ReadInt64();
                case 5:
                    return reader.ReadSingle();
                case 6:
                    return reader.ReadDouble();
                case 7:
                    int l = reader.ReadInt32();
                    return reader.ReadBytes(l);
                case 8:
                    return reader.ReadString();
                case 9:
                    byte listtype = reader.ReadByte();
                    //string wtf = reader.ReadString();
                    int length = reader.ReadInt32();
                    List<SaveTag> list = new List<SaveTag>(length);
                    for (int i = 0; i < length; i++)
                    {
                        list.Add(new SaveTag(listtype, "", Read(reader, listtype)));
                        //list.Add((Tag)Read(reader, listtype));
                    }
                    if (length > 0)
                        return list;
                    else
                        return listtype;
                case 10:
                    //List<SaveTag> cmpdnlist = new List<SaveTag>();
                    int capacity = reader.ReadInt32();
                    Dictionary<string, SaveTag> cmpdnlist = new Dictionary<string, SaveTag>(capacity + 1); 
                    // initialize dictionary to capacity + 1 because we add an empty escape item in the dictionary at the end 
                    // can we avoid that? we already have the item count (capacity), so can we just iterate that many times? 
                    byte nexttype;
                    do
                    {
                        nexttype = reader.ReadByte();
                        string nextname = "";
                        if (nexttype != 0)
                            nextname = reader.ReadString();
                        var t = new SaveTag(nexttype, nextname, Read(reader, nexttype));
                        //try
                        //{
                            cmpdnlist.Add(nextname, t);
                        //}
                        //catch (Exception e) 
                        //{
                        //    nextname.ToConsole();
                        //}
                        //cmpdnlist[nextname] = new SaveTag(nexttype, nextname, Read(reader, nexttype));
                    } while (nexttype > 0);
                    return cmpdnlist;
                case 11:
                    return reader.ReadUInt16();
                case 12:
                    return reader.ReadUInt32();
                case 13:
                    return reader.ReadBoolean();
                case 14: 
                    return reader.ReadVector3();
                case 15: //references
                    //var refID = reader.ReadString();
                    //return LoadReferences[refID];
                    return reader.ReadString();
            }
            return null;
        }
        static public SaveTag ReadWithRefs(BinaryReader reader)
        {
            LoadReferences.Clear();
            int refCount = reader.ReadInt32();
            for (int i = 0; i < refCount; i++)
            {
                var typeRef = reader.ReadByte();
                var nameRef = reader.ReadString();
                if (!LoadReferences.ContainsKey(nameRef))
                {
                    var tag = new SaveTag(typeRef, nameRef, Read(reader, typeRef));
                    var typeName = tag.GetValue<string>("TypeName");
                    var item = Activator.CreateInstance(System.Type.GetType(typeName)) as ISaveable;
                    LoadReferences[nameRef] = item.Load(tag["Data"]);
                }
            }
            var s = Read(reader);
            return s;
        }
        static public SaveTag Read(BinaryReader r)
        {
            byte type = r.ReadByte();
            string name = r.ReadString();
            return new SaveTag(type, name, Read(r, type));
        }
        public SaveTag Add(string name, int value)
        {
            this.Add(new SaveTag(SaveTag.Types.Int, name, value));
            return this;
        }
        public SaveTag Add(string name, string value)
        {
            this.Add(new SaveTag(SaveTag.Types.String, name, value));
            return this;
        }
        public void Add(SaveTag tag)
        {
            //if(!(Type == 9 || Type == 10))
            //    throw(new ArrayTypeMismatchException())

            if (this.Type == 9)
                (Value as List<SaveTag>).Add(tag);
            else if (this.Type == 10)
                (Value as Dictionary<string, SaveTag>).Add(tag.Name, tag);
        }

        public IntVec3 LoadIntVec3(string name)
        {
            var tag = this[name];
            return (Vector3)tag.Value; // it's vector3 internally
            return new IntVec3((int)tag["X"].Value, (int)tag["Y"].Value, (int)tag["Z"].Value);
        }
        public IntVec3 LoadIntVec3()
        {
            return (Vector3)this.Value;
            return new IntVec3((int)this["X"].Value, (int)this["Y"].Value, (int)this["Z"].Value);
        }
        public Vector3 LoadVector3()
        {
            return new Vector3((float)this["X"].Value, (float)this["Y"].Value, (float)this["Z"].Value);
        }
        public List<Vector3> LoadListVector3()
        {
            return new List<Vector3>().Load(this);
        }
        public List<int> LoadListInt()
        {
            return new List<int>().Load(this);
        }
        public List<int> LoadListInt(string name)
        {
            return new List<int>().Load(this[name]);
        }
        public Vector2 LoadVector2()
        {
            return new Vector2((float)this["X"].Value, (float)this["Y"].Value);
        }

        public Color LoadColor()
        {
            Color c = new Color();
            c.R = this.GetValue<Byte>("R");
            c.G = this.GetValue<Byte>("G");
            c.B = this.GetValue<Byte>("B");
            c.A = this.GetValue<Byte>("A");
            return c;
        }

        
        private static string RegisterRef(ILoadReferencable reference)
        {
            var refID = reference.GetUniqueLoadID();
            if (!SaveReferences.ContainsKey(refID))
            {
                var reftag = new SaveTag(SaveTag.Types.Compound, refID);
                reftag.Add(reference.GetType().FullName.Save("TypeName"));
                reftag.Add(reference.Save("Data"));
                SaveReferences[refID] = reftag;
            }
            return refID;
        }

        public bool TrySaveRef(ILoadReferencable reference, string name)
        {
            if (reference == null)
                return false;
            this.Add(SaveRef(reference, name));
            return true;
        }
        public bool TrySaveRefs<T>(IList<T> refs, string name) where T : ILoadReferencable
        {
            if (refs == null)
                return false;
            var tag = new SaveTag(Types.List, name, Types.Reference);
            foreach (var item in refs)
            {
                var refID = RegisterRef(item);
                tag.Add(new SaveTag(SaveTag.Types.Reference, "", refID));
            }
            this.Add(tag);
            return true;
        }
        static public SaveTag SaveRef(ILoadReferencable reference, string name)
        {
            if (reference == null)
                throw new Exception();
            string refID = RegisterRef(reference);
            return new SaveTag(SaveTag.Types.Reference, name, refID);
        }
        static public SaveTag SaveRefs<T>(List<T> refs, string name) where T : ILoadReferencable
        {
            var tag = new SaveTag(Types.List, name, Types.Reference);
            foreach(var item in refs)
            {
                var refID = RegisterRef(item);
                tag.Add(new SaveTag(SaveTag.Types.Reference, "", refID));
            }
            return tag;
        }
        public T LoadRef<T>() where T : class, ISaveable
        {
            var refID = (string)this.Value;
            return LoadReferences[refID] as T;
        }
        public T LoadRef<T>(string name) where T : class, ISaveable
        {
            var refID = (string)this[name].Value;
            return LoadReferences[refID] as T;
        }
        public bool TryLoadRef<T>(string name, out T item) where T : class, ISaveable
        {
            if (this.TryGetTag(name, out var t))
            {
                var refID = (string)this[name].Value;
                item = LoadReferences[refID] as T;
                return true;
            }
            item = null;
            return false;
        }
        public List<T> LoadRefs<T>(string name) where T : class, ISaveable
        {
            var list = this[name].Value as List<SaveTag>;
            var itemList = new List<T>();
            foreach(var tag in list)
            {
                var refID = (string)tag.Value;
                itemList.Add(LoadReferences[refID] as T);
            }
            return itemList;
        }
        public bool TryLoadRefs<T>(string name, ref List<T> list) where T : class, ISaveable
        {
            if (this.TryGetTag(name, out var l))
            {
                var taglist = l.Value as List<SaveTag>;
                //list = new List<T>();
                foreach (var tag in taglist)
                {
                    var refID = (string)tag.Value;
                    list.Add(LoadReferences[refID] as T);
                }
                return true;
            }
            //list = null;
            return false;
        }
    }
}
