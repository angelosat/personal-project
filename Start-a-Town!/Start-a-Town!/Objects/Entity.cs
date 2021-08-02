using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class Entity : GameObject
    {
        private SpriteComponent _sprite;
        public SpriteComponent Sprite => this._sprite ??= this.GetComponent<SpriteComponent>();

        /// <summary>
        /// here or in tool class?
        /// </summary>
        ToolAbilityComponent _toolComponent;
        public ToolAbilityComponent ToolComponent => this._toolComponent ??= this.GetComponent<ToolAbilityComponent>();


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
            this.AddComponent(new SpriteComponent(def));
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

        internal bool ProvidesSkill(ToolAbilityDef skill)
        {
            return this.ToolComponent?.Props?.Ability.Def == skill;
            //return ToolAbilityComponent.HasSkill(this, skill);
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
        public GameObject Randomize(RandomThreaded random)
        {
            if (this.Def.CraftingProperties is not null) // HACK
            {
                var mats = ItemFactory.GetRandomMaterialsFor(this.Def);
                this.SetMaterials(mats);
                this.SetQuality(Quality.GetRandom());
            }
            foreach (var comp in this.Components.Values)
                comp.Randomize(this, random);
            return this;
        }

        internal void Select()
        {
            SelectionManager.Select(this);
        }
    }
}
