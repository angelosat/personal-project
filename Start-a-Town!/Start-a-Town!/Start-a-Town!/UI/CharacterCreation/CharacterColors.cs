using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;
using Start_a_Town_.Components.Materials;

namespace Start_a_Town_
{
    //public class CharacterColor
    //{
    //    public string Name { get; set; }
    //    public List<string> Paths { get; set; }
    //    public Color Color { get; set; }
    //    public CharacterColor(string name, params string[] paths) : this(name, Color.White, paths) { }
    //    public CharacterColor(string name, Color color, params string[] paths)
    //    {
    //        this.Name = name;
    //        this.Paths = new List<string>(paths);
    //        this.Color = color;
    //    }

    //    public void Apply(Bone body)
    //    {
    //        foreach (var path in this.Paths)
    //        {
    //            Bone bone = this.GetBone(path, body);
    //            Sprite overlay = this.GetOverlay(path, bone.Sprite);
    //            overlay.Tint = this.Color;
    //        }
    //        //Bone bone = this.GetBone(body);
    //        //Sprite overlay = this.GetOverlay(bone.Sprite);
    //        //overlay.Tint = this.Color;
    //    }
    //    public Bone GetBone(string path, Bone body)
    //    {
    //        string bonePath = path.Split(':').First();
    //        string[] bones = bonePath.Split('/');
    //        if (bones.Length == 1)
    //            if (string.IsNullOrWhiteSpace(bones[0]))
    //                return body;
    //        Queue<string> toFind = new Queue<string>(bones);
    //        Bone bone = body;
    //        while (toFind.Count > 0)
    //            bone = bone[toFind.Dequeue()];
    //        return bone;
    //    }
    //    public Sprite GetOverlay(string path, Sprite sprite)
    //    {
    //        string[] parts = path.Split(':');
    //        if (parts.Length == 1)
    //            return sprite;
    //        if (parts.Length > 2)
    //            throw new ArgumentException();
    //        string overlayName = parts[1];
    //        return sprite.Overlays[overlayName];
    //    }

    //    public List<SaveTag> Save()
    //    {
    //        List<SaveTag> tag = new List<SaveTag>();
    //        tag.Add(new SaveTag(SaveTag.Types.String, "Name", this.Name));
    //        //tag.Add(new SaveTag(SaveTag.Types.String, "Path", this.Path));
    //        SaveTag pathsTag = new SaveTag(SaveTag.Types.List, "Paths", SaveTag.Types.String);
    //        foreach (var path in this.Paths)
    //            pathsTag.Add(new SaveTag(SaveTag.Types.String, "", path));
    //        tag.Add(pathsTag);
    //        tag.Add(new SaveTag(SaveTag.Types.Compound, "Color", this.Color.Save()));
    //        return tag;
    //    }
    //    public CharacterColor(SaveTag tag)
    //    {
    //        this.Name = tag.GetValue<string>("Name");
    //        this.Paths = new List<string>();
    //        //this.Path = tag.GetValue<string>("Path");
    //        List<SaveTag> pathsTag = tag.GetValue<List<SaveTag>>("Paths");
    //        foreach(var item in pathsTag)
    //        {
    //            if (item == null)
    //                continue;
    //            this.Paths.Add(item.GetValue<string>(""));
    //        }
    //        tag.TryGetTag("Color", t => this.Color = t.LoadColor());
    //    }
    //    public void Write(BinaryWriter w)
    //    {
    //        w.WriteASCII(this.Name);
    //        //w.WriteASCII(this.Path);
    //        w.Write(this.Paths.Count);
    //        foreach (var item in this.Paths)
    //            w.Write(item);
    //        this.Color.Write(w);
    //    }
    //    public CharacterColor(BinaryReader r)
    //    {
    //        this.Name = r.ReadASCII();
    //        //this.Path = r.ReadASCII();
    //        this.Paths = new List<string>();
    //        int pathsCount = r.ReadInt32();
    //        for (int i = 0; i < pathsCount; i++)
    //        {
    //            this.Paths.Add(r.ReadString());
    //        }
    //        this.Color = r.ReadColor();
    //    }
    //}
    public class CharacterColor
    {
        public string Name { get; set; }
        public string Overlay { get; set; }
        public Color Color { get; set; }
        public CharacterColor(string name, string overlay) : this(name, Color.White, overlay) { }
        public CharacterColor(string name, Color color, string overlay)
        {
            this.Name = name;
            this.Overlay = overlay;
            this.Color = color;
        }

        public void Apply(Bone body)
        {
            foreach (var bone in body.GetChildren())
                if (bone.Sprite != null)
                    foreach (var overlay in bone.Sprite.GetOverlays())
                        if (overlay.OverlayName == this.Overlay)
                            //overlay.Tint = this.Color;
                            overlay.Material = Material.CreateColor(this.Color);
        }

        public List<SaveTag> Save()
        {
            List<SaveTag> tag = new List<SaveTag>();
            tag.Add(new SaveTag(SaveTag.Types.String, "Name", this.Name));
            tag.Add(new SaveTag(SaveTag.Types.String, "Overlay", this.Overlay));
            //SaveTag pathsTag = new SaveTag(SaveTag.Types.List, "Paths", SaveTag.Types.String);
            //foreach (var path in this.Paths)
            //    pathsTag.Add(new SaveTag(SaveTag.Types.String, "", path));
            //tag.Add(pathsTag);
            tag.Add(new SaveTag(SaveTag.Types.Compound, "Color", this.Color.Save()));
            return tag;
        }
        public CharacterColor(SaveTag tag)
        {
            this.Name = tag.GetValue<string>("Name");
            this.Overlay = tag.GetValue<string>("Overlay");
            //this.Paths = new List<string>();
            //List<SaveTag> pathsTag = tag.GetValue<List<SaveTag>>("Paths");
            //foreach (var item in pathsTag)
            //{
            //    if (item == null)
            //        continue;
            //    this.Paths.Add(item.GetValue<string>(""));
            //}
            tag.TryGetTag("Color", t => this.Color = t.LoadColor());
        }
        public void Write(BinaryWriter w)
        {
            w.WriteASCII(this.Name);
            //w.WriteASCII(this.Path);
            w.Write(this.Overlay);
            //w.Write(this.Paths.Count);
            //foreach (var item in this.Paths)
            //    w.Write(item);
            this.Color.Write(w);
        }
        public CharacterColor(BinaryReader r)
        {
            this.Name = r.ReadASCII();
            this.Overlay = r.ReadString();
            //this.Path = r.ReadASCII();
            //this.Paths = new List<string>();
            //int pathsCount = r.ReadInt32();
            //for (int i = 0; i < pathsCount; i++)
            //{
            //    this.Paths.Add(r.ReadString());
            //}
            this.Color = r.ReadColor();
        }
        public CharacterColor(CharacterColor toClone)
        {
            this.Name = toClone.Name;
            this.Overlay = toClone.Overlay;
            this.Color = toClone.Color;
        }
    }

    public class CharacterColors
    {
        public Dictionary<string, CharacterColor> Colors;

        public CharacterColors(params CharacterColor[] parts):this()
        {
            foreach (var item in parts)
                this.Colors.Add(item.Name, item);
        }
        public CharacterColors()
        {
            this.Colors = new Dictionary<string, CharacterColor>();
            //this.Colors.Add("Hair", new CharacterColor("Hair", "Head:Hair"));
            //this.Colors.Add("Shirt", new CharacterColor("Chest", ""));
        }

        public void Apply(Bone body)
        {
            foreach (var item in this.Colors.Values)
                item.Apply(body);
        }

        public List<SaveTag> Save()
        {
            List<SaveTag> tag = new List<SaveTag>();
            SaveTag list = new SaveTag(SaveTag.Types.List, "Colors", SaveTag.Types.Compound);
            foreach (var item in this.Colors.Values)
                list.Add(new SaveTag(SaveTag.Types.Compound, "", item.Save()));
            tag.Add(list);
            return tag;
        }
        public CharacterColors(SaveTag tag)
        {
            this.Colors = new Dictionary<string, CharacterColor>();
            //List<SaveTag> list = tag.GetValue<List<SaveTag>>("Colors");
            List<SaveTag> list;
            if (!tag.TryGetTagValue<List<SaveTag>>("Colors", out list))
                return;
            foreach (var item in list)
            {
                if (item.IsNull())
                    continue;
                var part = new CharacterColor(item);
                this.Colors.Add(part.Name, part);
            }
        }
        //public CharacterColors(SaveTag tag)
        //{
        //    this.Colors = new Dictionary<string, CharacterColor>();
        //    List<SaveTag> list = tag.GetValue<List<SaveTag>>("Colors");
        //    foreach (var item in list)
        //    {
        //        if (item.IsNull())
        //            continue;
        //        var part = new CharacterColor(item);
        //        this.Colors.Add(part.Name, part);
        //    }
        //}
        public void Write(BinaryWriter w)
        {
            w.Write(this.Colors.Count);
            foreach (var item in this.Colors.Values)
                item.Write(w);
        }
        public CharacterColors(BinaryReader r)
        {
            this.Colors = new Dictionary<string, CharacterColor>();
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var part = new CharacterColor(r);
                this.Colors.Add(part.Name, part);
            }
        }

        //public Dictionary<string, Color> Colors;
        //public CharacterColors()
        //{
        //    this.Colors = new Dictionary<string, Color>();
        //    this.Colors.Add("Hair", Color.White);
        //    this.Colors.Add("Shirt", Color.White);
        //}
        public CharacterColors(CharacterColors toClone)
        {
            this.Colors = new Dictionary<string, CharacterColor>();
            foreach (var item in toClone.Colors)
                this.Colors.Add(item.Key, new CharacterColor(item.Value));
        }
    }
}
