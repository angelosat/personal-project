using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

namespace Start_a_Town_
{
    public enum Tags : byte
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
        List,
        Compound
    }

    interface ITagInfo
    {
        byte TagType { get; set; }
        string Name { get; set; }
        object Value { get; set; }
    }

    public class TagBuilder
    {
        //static public T Create<T>() where T : Tag<KeyboardMap>
        //{
        //    return new List<
        //}
        static public Tag<byte> Create<byte>()
        {
            return new TagByte();
        }
    }
    public class TagBase
    {
        static public Dictionary<byte, Type> TagTypes = new Dictionary<byte,Type>(){
            {(byte)Tags.Byte, typeof(TagByte)},
            {(byte)Tags.Int, typeof(TagInt)},
            {(byte)Tags.String, typeof(TagString)},
        };

        public virtual object Value { get; set; }
    }
    public class Tag<T> : TagBase, ITagInfo
    {
        public byte TagType { get; set; }
        public string Name { get; set; }
        public bool IsNamed { get { return Name != null; } }
        public override T Value { get; set; }
        public virtual void Write(BinaryWriter writer)
        {
            //Tag<byte> tagg = new TagByte();//new Tag<byte>();
            //Tag<byte> tagg = new Tag<byte>();
            //TagList<byte> taggg = new TagList<byte>();
            //Tag<Dictionary<string, U>> tag2 = new Tag<Dictionary<string, U>>();
            if (IsNamed)
            {
                writer.Write(TagType);
                writer.Write(Name as string);
            }
        }

    }   

    public class TagEnd : Tag<byte>, ITagInfo
    {
        public override byte TagType
        { get { return (byte)Tags.End; } }
        public override void Write(BinaryWriter writer)
        {
            writer.Write((byte)0);
        }
    }
    public class TagInt : Tag<int>, ITagInfo
    {
        public override byte TagType
        { get { return (byte)Tags.Int; } }
        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write((int)Value);
        }
    }
    public class TagByte : Tag<byte>, ITagInfo
    {
        public override byte TagType
        { get { return (byte)Tags.Byte; } }
        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Value);
        }
    }
    public class TagString : Tag<string>, ITagInfo
    {
        public override byte TagType
        { get { return (byte)Tags.String; } }
        public override void Write(BinaryWriter writer)
        {
            base.Write(writer);
            writer.Write(Value);
        }
    }

    public class TagList<T> : IEnumerable, ITagInfo where T:ITagInfo//List<Tag<T>> 
    //public class TagList<T>
    {
        public string Name { get; set; }
        TagByte ListType = new TagByte();
        TagInt Length = new TagInt();
        public List<T> List = new List<T>();
        //List<Tag<T>> List = new List<Tag<T>>();
        public override byte TagType
        { get { return (byte)Tags.List; } }



        //public Tag<T> this[int k]
        //{ get { return this[k]; } }
        public TagList() { }
        public TagList(string name) { Name = name; }
        //public TagList(IEnumerable<Tag<T>> ienumerable)
        //{
        //    AddRange(ienumerable);
        //}

        public void Add(T item)
        {
            List.Add(item);
        }

        //public void Add(Tag<T> item)
        //{
        //    List.Add(item);
        //}

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Name);
            foreach (Tag<T> tag in this)
                tag.Write(writer);
        }
    }
    //public class TagCompound<K,T> : Tag<Dictionary<string, K>> where K : Tag<T>


    public class TagNode : ITagInfo
    {
        public string Name { get; set; }

        public override byte TagType
        { get { return (byte)Tags.Compound; } }

        TagList<byte> bytetags = new TagList<byte>();
        TagList<int> inttags = new TagList<int>();
        TagList<string> stringtags = new TagList<string>();
        TagList<ITagInfo> listtags = new TagList<ITagInfo>();
        TagList<TagNode> compoundtags = new TagList<TagNode>();
       
        public byte GetByte(string name)
        {
            Dictionary<string, Tag<byte>> tags = bytetags.ToDictionary(foo => foo.Name as string) as Dictionary<string, Tag<byte>>;
            return tags[name].Value;
        }
        public int GetInt(string name)
        {
            Dictionary<string, Tag<int>> tags = inttags.ToDictionary(foo => foo.Name as string) as Dictionary<string, Tag<int>>;
            return tags[name].Value;
        }
        public string GetString(string name)
        {
            Dictionary<string, Tag<string>> tags = stringtags.ToDictionary(foo => foo.Name as string) as Dictionary<string, Tag<string>>;
            return tags[name].Value;
        }
        public TagList<T> GetList<T>(string name)
        {
            Dictionary<string, TagList<T>> tags = listtags.ToDictionary(foo => foo.Name as string) as Dictionary<string, TagList<T>>;
            return tags[name];
        }

        

        public TagNode() { }
        public TagNode(string name) { Name = name; }

        public void Add(Tag<byte> tag)
        {
            bytetags.Add(tag);
        }
        public void Add(Tag<int> tag)
        {
            inttags.Add(tag);
        }
        public void Add(Tag<string> tag)
        {
            stringtags.Add(tag);
        }
        internal void Add(TagList<ITagInfo> taglist)
        {
            listtags.Add(taglist);
        }

        internal void Add(TagList<TagList<byte>> overlist)
        {
            throw new NotImplementedException();
        }
    }

    public class NBTDocument
    {
        #region Static Tag Constructors
        static public Tag<byte> CreateTag(byte value)
        {
            return new TagByte();
        }
        static public Tag<int> CreateTag(int value)
        {
            return new TagInt();
        }
        static public Tag<string> CreateTag(string value)
        {
            return new TagString();
        }

        static public Tag<byte> CreateTag(string name, byte value)
        {
            TagByte tag = new TagByte();
            tag.Name = name;
            return tag;
        }
        static public Tag<int> CreateTag(string name, int value)
        {
            TagInt tag = new TagInt();
            tag.Name = name;
            return tag;
        }
        static public Tag<string> CreateTag(string name, string value)
        {
            TagString tag = new TagString();
            tag.Name = name;
            return tag;
        }

        static public TagList<T> CreateList<T>(string name, IEnumerable<T> value)
        {
            TagList<T> list = new TagList<T>(value);

        }
        #endregion
    }
}
