﻿using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class Entity : GameObject
    {
        private SpriteComponent _Sprite;
        public override GameObject Create()
        {
            return new Entity();
        }
        public Entity()
        {
            this.AddComponent(new DefComponent());
            this.AddComponent<PhysicsComponent>();
        }
        public Entity(ItemDef def):this()
        {
            Def = def;
        }

        public SpriteComponent Sprite
        {
            get
            {
                if (this._Sprite == null)
                    this._Sprite = this.GetComponent<SpriteComponent>(); 
                return this._Sprite;
            }

        }
        public Entity SetMaterial(BoneDef bone, Material mat)
        {
            this.Sprite.SetMaterial(bone, mat);
            return this;
        }

        

        internal void InitComps()
        {
            foreach (var props in this.Def.CompProps)
            {
                var compType = props.CompType;
                if (this.TryGetComponent(compType, out var c))
                {
                    c.Initialize(props);
                }
                else
                {
                    var comp = props.CreateComponent();
                    this.AddComponent(comp);
                }
            }
            foreach(var c in this.Components.Values)
            {
                c.OnObjectCreated(this);
            }
           
        }

        internal Material GetMaterial(BoneDef def)
        {
            return this.Sprite.GetMaterial(def);
        }
        internal virtual GameObject SetName(string v)
        {
            this.Name = v;
            return this;
        }

        internal Texture2D RenderIcon(int scale = 1)
        {
            return this.Body.RenderIcon(this, scale);
        }
        internal Entity SetMaterial(Material mat)
        {
            foreach (var c in this.Components.Values)
                c.SetMaterial(mat);
            this.Name = $"{mat.Prefix} {this.Def.Label}";
            mat.Apply(this);
            return this;
        }
        internal Entity SetMaterials(Dictionary<string, Material> materials)
        {
            foreach (var c in this.Components.Values)
                c.Initialize(this, materials);
            return this;
        }
        internal Entity SetQuality(Quality quality)
        {
            if (this.Def.QualityLevels)
                foreach (var c in this.Components.Values)
                    c.Initialize(this, quality);
            return this;
        }
       
    }
}
