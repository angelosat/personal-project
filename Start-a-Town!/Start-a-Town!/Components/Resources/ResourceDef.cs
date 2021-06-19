﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Start_a_Town_.Components.Resources;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public abstract class ResourceDef : Def
    {
        public class ResourceThreshold : Def
        {
            ResourceThreshold Next;
            readonly public float Value;
            public ResourceThreshold(string name, float value) : base(name)
            {
                //this.Name = name;
                this.Value = value;
            }
            public ResourceThreshold Get(float value)
            {
                if (value <= this.Value)
                    return this;
                else
                    return this.Next?.Get(value) ?? this;
            }
            public int GetDepth(float value)
            {
                var n = 0;
                var current = this;
                do
                {
                    if (value <= current.Value || current.Next == null)
                        return n;
                    current = current.Next;
                    n++;
                } while (true);// current != null);
                               //return n;
            }
            public float GetThresholdValue(int index)
            {
                var current = this;
                var n = 0;
                while (n < index)
                {
                    current = current.Next;
                }
                return current.Value;
            }
            public ResourceThreshold Add(ResourceThreshold next)
            {
                this.Next = this.Next?.Add(next) ?? next;
                if (next.Value <= this.Value)
                    throw new Exception();
                return this;
                //this.Next = next;
                //if (this.Next.Value <= this.Value)
                //    throw new Exception();
                //return this;
            }
            ResourceThreshold GetLast(out int depth)
            {
                depth = 0;
                var current = this;
                do
                {
                    if (current.Next == null)
                        return current;
                    current = current.Next;
                    depth++;
                } while (true);
            }
            public override string ToString()
            {
                return string.Format("{0}: {1}", this.Name, this.Value.ToString("##0%"));
            }
        }


        static public readonly ResourceDef Health =
            new Health()//; //new ResourceDef("Health");
                .AddThreshold("Dying", .25f)
                .AddThreshold("Critical", .5f)
                .AddThreshold("Injured", .75f)
                .AddThreshold("Healthy", 1f);

        static public readonly ResourceDef Stamina = //new ResourceDef("Stamina");
            new Stamina()
                .AddThreshold("Out of breath", .25f)
                .AddThreshold("Exhausted", .5f)
                .AddThreshold("Tired", .75f)
                .AddThreshold("Energetic", 1f);

        static public readonly ResourceDef Durability = new Durability().AddThreshold("Durability", 1);

        static ResourceDef()
        {
            Register(Health);
            Register(Stamina);
            Register(Durability);
        }

        static public Progress Recovery { get { return new Progress(0, Engine.TicksPerSecond, Engine.TicksPerSecond); } }
        public ResourceDef(string name) : base(name)
        {

        }
        internal virtual void HandleRemoteCall(GameObject parent, ObjectEventArgs e, Resource resource)
        {
        }
        public virtual void SetMaterial(Material mat) { }
        public enum ResourceTypes { Health, Mana, Stamina, Durability }

        readonly List<ResourceThreshold> Thresholds = new();

        ResourceThreshold Root;
        public ResourceDef AddThreshold(string label, float value)
        {
            var t = new ResourceThreshold(label, value);
            this.Thresholds.Add(t);
            this.Thresholds.Sort((a, b) => a.Value.CompareTo(b.Value));
            return this;
            //if (this.Root == null)
            //    this.Root = t;
            //else
            //    this.Root.Add(t);
            //return this;
        }
        public ResourceThreshold GetThreshold(Resource res)
        {
            return this.Root.Get(res.Value);
        }
        public float GetThresholdValue(Resource res, int index)
        {
            return this.Root?.GetThresholdValue(index) ?? 0;
        }
        public int GetThresholdDepth(Resource res)
        {
            return this.Root.GetDepth(res.Value);
        }
        static List<ResourceDef> _Registry;
        public static List<ResourceDef> Registry
        {
            get
            {
                if (_Registry == null)
                    Initialize();
                return _Registry;
            }
        }

        static void Initialize()
        {
            _Registry = new List<ResourceDef>()
            {
                new Health(),
                new Durability(),
                new Stamina()
            };
        }

        public string GetLabel(Resource res)
        {
            var label = this.Thresholds.FirstOrDefault(t => res.Percentage <= t.Value)?.Name;
            return label ?? "";
        }
        public abstract Color GetBarColor(Resource resource);
        public virtual string GetBarLabel(Resource resource)
        {
            return this.GetLabel(resource);
        }
        public virtual string GetBarHoverText(Resource resource)
        {
            return $"{resource.Value} / {resource.Max}";
        }
        public virtual Control GetControl(Resource resource)
        {
            var bar = new Bar()
            {
                Object = resource,
                ColorFunc = () => this.GetBarColor(resource),
                TextFunc = () => this.GetBarLabel(resource),
                HoverFunc = () => this.GetBarHoverText(resource)
            };//, HoverFunc = () => $"{resource.Value} / {resource.Max}" };
            return bar;
        }

        public abstract ResourceTypes ID { get; }
        public abstract string Description { get; }
        
        public virtual void Add(float add, Resource resource)
        {
            resource.Value += add;
        }
     
        public readonly float BaseMax = 100;
       
        public virtual void Tick(GameObject parent, Resource resource)
        {
            foreach (var ratemod in resource.Modifiers)
                this.Add(ratemod.Def.GetRateMod(parent), resource);
        }
        public virtual bool HandleMessage(Resource resource, GameObject parent, ObjectEventArgs e = null) { return false; }

        
        public virtual string Format { get { return ""; } }


        //public virtual Control GetControl(Resource resource) { return null; }

        public virtual void OnHealthBarCreated(GameObject parent, UI.Nameplate plate, Resource values) { }
        public virtual void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent) { }

        internal virtual void InitMaterials(Entity obj, Dictionary<string, Material> materials) { }
    }
}
