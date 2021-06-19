using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class Plant : Entity
    {
        private PlantComponent _PlantComponent;
        public PlantComponent PlantComponent
        {
            get
            {
                if(this._PlantComponent == null)
                    this._PlantComponent = this.GetComponent<PlantComponent>();
                return this._PlantComponent;
            }
        }

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

        //static public Entity CreateTree(ItemDef def)//, Material woodMaterial, float growth = .05f)
        //{
        //    var tree = Create(def);
        //    tree.AddComponent(new TreeComponent());// growth));
        //    return tree;
        //}

        static public Entity CreateTree(ItemDef def, Material woodMaterial, float growth)
        {
            throw new Exception();
            var tree = ItemFactory.CreateFrom(def, woodMaterial);// Create(def);
            tree.AddComponent(new TreeComponent(growth));
            return tree;
        }
        //internal static Plant CreateBush(ItemDef def)//, float growth = .05f, float fruitGrowth = 0f)
        //{
        //    var bush = Create(def);
        //    bush.AddComponent(new PlantComponent(def));
        //    //bush.PlantComponent.SetGrowth(growth, fruitGrowth);
        //    return bush;
        //}
        [Obsolete]
        internal static Plant CreateBush(ItemDef def, float growth, float fruitGrowth)
        {
            var bush = Create(def);
            throw new Exception();
            //bush.AddComponent(new PlantComponent(def));
            bush.PlantComponent.SetGrowth(growth, fruitGrowth);
            return bush;
        }
        internal static Plant CreateBush(PlantProperties plant, float growth, float fruitGrowth)
        {
            var bush = Create(PlantDefOf.Bush);
            bush.GetComp<PlantComponent>().SetProperties(plant);
            bush.PlantComponent.SetGrowth(growth, fruitGrowth);
            return bush;
        }
        static Plant Create(ItemDef def)
        {
            var plant = new Plant(def);
            //var plant = ItemFactory.CreateItem(def);
            plant.Physics.Weight = def.Weight;
            plant.Physics.Height = def.Height; // the physicscomponent can read the values from the def instead of assisning values to it

            return plant;
        }
        public bool IsHarvestable => this.PlantComponent.IsHarvestable;// this.PlantComponent?.IsHarvestable ?? false;
        public float FruitGrowth => this.PlantComponent.FruitGrowth.Percentage;
    }
}
