using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

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
            //foreach(var props in this.Def.CompProps)
            //{
            //    var comp = props.CreateComponent();
            //    this.AddComponent(comp);
            //}
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
            //this.Sprite.InitMaterials(mat);
            foreach (var c in this.Components.Values)
                c.SetMaterial(mat);
            //obj.Name = mat.Name + " " + obj.Name;
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
        
        //internal Entity GetNameFromReagent(Entity material)
        //{
        //    this.Name = $"{material.Label} {this.Name}";
        //    return this;
        //}
        //internal Entity GetNameFromReagentMaterial(Entity material)
        //{
        //    this.Name = $"{material.DominantMaterial.Label} {this.Name}";
        //    return this;
        //}
    }
}
