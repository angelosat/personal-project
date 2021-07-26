using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class Entity : GameObject
    {
        private SpriteComponent _sprite;
        public SpriteComponent Sprite => this._sprite ??= this.GetComponent<SpriteComponent>();

        GearComponent _gear;
        public GearComponent Gear => this._gear ??= this.GetComponent<GearComponent>();

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
        internal GameObjectSlot GetEquipmentSlot(GearType.Types type)
        {
            return this.Gear.GetSlot(GearType.Dictionary[type]);
        }

        public Entity SetMaterial(BoneDef bone, MaterialDef mat)
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

        internal MaterialDef GetMaterial(BoneDef def)
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
        internal Entity SetMaterial(MaterialDef mat)
        {
            foreach (var c in this.Components.Values)
                c.SetMaterial(mat);
            this.Name = $"{mat.Prefix} {this.Def.Label}";
            mat.Apply(this);
            return this;
        }
        internal Entity SetMaterials(Dictionary<string, MaterialDef> materials)
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
