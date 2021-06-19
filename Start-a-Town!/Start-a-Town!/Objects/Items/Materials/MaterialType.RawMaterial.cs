using Start_a_Town_.Components;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public partial class MaterialType
    {
        public abstract class RawMaterial : IItemFactory// : GameObject
        {
            //static public readonly RawMaterial Bag = new Bags();
            //static public readonly RawMaterial Logs = new Logs();
            //static public readonly RawMaterial Planks = new Planks();
            //static public readonly RawMaterial Ore = new Ore();
            //static public readonly RawMaterial Rocks = new Rocks();
            //static public readonly RawMaterial Bars = new Bars();

            const int IDRange = 40000;
            static int _IDSequence = IDRange;
            public static int GetNextID() 
            { 
                return _IDSequence++;
            }
            //protected int ID = IDSequence;
            public int ID { get; protected set; }
            public HashSet<CanBeMadeInto> Children = new HashSet<CanBeMadeInto>();
            protected Dictionary<Material, GameObject> Templates = new Dictionary<Material, GameObject>();
            //protected abstract GameObject Initialize(Material mat);

            public RawMaterial()
            {

            }
            protected abstract Entity CreateTemplate();

            public Entity Initialize(Material mat)
            {
                this.ID = IDSequence;
                var obj = this.CreateTemplate();
                obj.Name = mat.Name + " " + obj.Name;
                Sprite sprite = new Sprite(obj.GetSprite()) { Material = mat };
                obj.GetComponent<SpriteComponent>().Sprite = sprite;
                obj.Body.Sprite = sprite;
                obj.Body.Material = mat;
                obj.Sprite.SetMaterial(obj.Body.Def, mat);
                this.Templates[mat] = obj;
                //StorageCategory.Manufactured.Add(obj);
                return obj;
            }
            public Entity CreateFrom(Material mat)
            {
                return this.Templates[mat].Clone() as Entity;
            }
            static public Entity Create(Material mat)
            {
                //return mat.ProcessingChain.First(obj => obj.GetComponent<MaterialsComponent>().Parts["Body"].Material == mat).Clone();
                return mat.ProcessingChain.First(obj => obj.Body.Material == mat).Clone() as Entity;

            }
            
            public RawMaterial CanBeMadeInto(params RawMaterial[] children)
            {
                //this.Children = new HashSet<CanBeMadeInto>(children);
                foreach (var item in children)
                    this.Children.Add(new CanBeMadeInto(item));
                return this;
            }
            public virtual Entity Create(Dictionary<string, Entity> materials) { return GameObject.Create(this.ID) as Entity; }

            public class Factory
            {
                string ReagentName;
                public Factory(string reagentName)
                {
                    this.ReagentName = reagentName;
                }
                public static Entity GetNextInChain(GameObject obj)
                {
                    Material mat = obj.Body.Material;// obj.GetComponent<MaterialsComponent>().Parts["Body"].Material;
                    MaterialType matType = mat.Type;

                    var chain = mat.ProcessingChain;
                    var currentStep = chain.FindIndex(item => item.IDType == obj.IDType);
                    if (currentStep == chain.Count - 1)
                        return null;
                    var product = chain[currentStep + 1].Clone();
                    return product as Entity;
                }

                public Entity Create(Dictionary<string, GameObject> materials)
                {
                    //var obj = materials.First(n => n.Name == ReagentName).Object;
                    //var tool = materials.FirstOrDefault(s => s.Name == "Tool");
                    var obj = materials[this.ReagentName];
                    var final = GetNextInChain(obj);
                    return final;
                }
                //public static GameObject Create(Material mat)
                //{
                //    MaterialType matType = mat.Type;

                //    var chain = mat.ProcessingChain;
                //    var currentStep = chain.FindIndex(item => item.Body.Material == mat);
                //    if (currentStep == chain.Count - 1)
                //        return null;
                //    var product = chain[currentStep].Clone();
                //    return product;
                //}
                //public GameObject Get(MaterialType mat)
                //{
                //    var obj = materials.First(n => n.Name == ReagentName).Object;
                //    var tool = materials.FirstOrDefault(s => s.Name == "Tool");
                //    var final = GetNextInChain(obj);
                //    return final;
                //}
            }
        }
    }
}
