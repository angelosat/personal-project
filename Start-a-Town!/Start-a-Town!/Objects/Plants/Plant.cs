using System;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class Plant : Entity
    {
        private PlantComponent _PlantComponent;
        public PlantComponent PlantComponent => this._PlantComponent ??= this.GetComponent<PlantComponent>();
      
        public override GameObject Create()
        {
            return new Plant();
        }
        public Plant() : base()
        {

        }

        public Plant(ItemDef def):base()
        {
            this.Def = def;
            this.AddComponent(new SpriteComponent(Def));
        }
        [Obsolete]
        static public Entity CreateTree(ItemDef def, Material woodMaterial, float growth)
        {
            throw new Exception();
            var tree = ItemFactory.CreateFrom(def, woodMaterial);
            tree.AddComponent(new TreeComponent(growth));
            return tree;
        }
        
        [Obsolete]
        internal static Plant CreateBush(ItemDef def, float growth, float fruitGrowth)
        {
            throw new Exception();
            var bush = Create(def);
            bush.PlantComponent.SetGrowth(growth, fruitGrowth);
            return bush;
        }
        [Obsolete]
        static Plant Create(ItemDef def)
        {
            throw new Exception();
            var plant = new Plant(def);
            plant.Physics.Weight = def.Weight;
            plant.Physics.Height = def.Height; // the physicscomponent can read the values from the def instead of assisning values to it
            return plant;
        }
        public bool IsHarvestable => this.PlantComponent.IsHarvestable;
        public float FruitGrowth => this.PlantComponent.FruitGrowth.Percentage;
    }
}
