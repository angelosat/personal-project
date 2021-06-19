using System;
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
        //public bool TrySetValue(string name, ref object value)
        //{
        //    //if(!ToDictionary().ContainsKey(name))
        //        if (!(this.Value as Dictionary<string, SaveTag>).ContainsKey(name))
        //        return false;
        //    value = this[name];
        //    return true;
        //}

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
            Vector3
        }


        public byte Type;
        public string Name;
        public Object Value;
        byte ListType;

        bool HasName 
        { get { return Name != null; } }
        //public Object Value 
        //{ get { return Value; } set { Value = value; } }

        #region Indexers
        public T GetValue<T>(string name)
        {
            return (T)this[name].Value;
        }
        public T TagValueOrDefault<T>(string name, T def)
        {
            SaveTag tag;
            if (!TryGetTag(name, out tag))
                return def;
            else
                return (T)tag.Value;
        }
        public TResult TagValueOrDefault<TValue, TResult>(string name, Func<TValue, TResult> valueConvert, TResult valueDefault)
        {
            SaveTag tag;
            if (!TryGetTag(name, out tag))
                return valueDefault;
            else
                return valueConvert((TValue)tag.Value);
        }
        public bool TagValueOrDefault<TValue, TResult>(string name, Func<TValue, TResult> valueConvert, TResult valueDefault, out TResult result)
        {
            SaveTag tag;
            if (!TryGetTag(name, out tag))
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
            SaveTag tag;
            if (!TryGetTag(name, out tag))
            {
                return false;
            }
            else
            {
                action(tag);
                return true;
            }
        }
        public bool TryGetTagValue<TValue>(string name, out TValue value)
        {
            SaveTag tag;
            if (!this.TryGetTag(name, out tag))
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
            SaveTag tag;
            if (!this.TryGetTag(name, out tag))
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
            SaveTag tag;
            if (!TryGetTag(name, out tag))
                return valueDefault;
            else
                return valueConvert(tag);
        }
        public bool TagOrDefault<TResult>(string name, Func<SaveTag, TResult> valueConvert, TResult valueDefault, out TResult result)
        {
            SaveTag tag;
            if (!TryGetTag(name, out tag))
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
        Dictionary<string, SaveTag> _Dictionary;
        //Dictionary<string, SaveTag> Dictionary;

        //public Dictionary<string, SaveTag> ToDictionary()
        //{
        //    return this.Value as Dictionary<string, SaveTag>;
        //    //return (Value as List<SaveTag>).ToDictionary(foo => foo.Name);
        //    if (_Dictionary.IsNull())
        //        _Dictionary = (Value as List<SaveTag>).ToDictionary(foo => foo.Name);
        //    return _Dictionary;
        //}

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
                default:
                    Value = value;
                    break;
            }
            Type = type;
            Name = name;
        }
        #endregion

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Type);
            writer.Write(Name);
            Write(writer);
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
                    throw (new NotImplementedException());
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
                default:
                    break;
            }
        }

        static Object Read(BinaryReader reader, byte type)
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
                    throw (new NotImplementedException());
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
                    Dictionary<string, SaveTag> cmpdnlist = new Dictionary<string, SaveTag>(capacity);
                    byte nexttype;
                    do
                    {
                        nexttype = reader.ReadByte();
                        string nextname = "";
                        if (nexttype != 0)
                            nextname = reader.ReadString();

                        //try
                        //{
                            cmpdnlist.Add(nextname, new SaveTag(nexttype, nextname, Read(reader, nexttype)));
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
            }
            return null;
        }
        static public SaveTag Read(BinaryReader reader)
        {
            byte type = reader.ReadByte();
            string name = reader.ReadString();
            return new SaveTag(type, name, Read(reader, type));
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

        //public struct Enumerator : IEnumerator
        //{
        //}
        //Enumerator Enum;
        //public Enumerator GetEnumerator()
        //{
        //    return Enum;
        //}

        //static public Vector3 LoadVector3(Tag tag)
        //{
        //    return new Vector3((float)tag["X"].Value, (float)tag["Y"].Value, (float)tag["Z"].Value);
        //}

        public Vector3 LoadVector3()
        {
            return new Vector3((float)this["X"].Value, (float)this["Y"].Value, (float)this["Z"].Value);
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

    }
}
