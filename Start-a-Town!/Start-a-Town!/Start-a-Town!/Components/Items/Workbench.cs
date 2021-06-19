using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Materials;

namespace Start_a_Town_.Components.Items
{
    partial class ItemTemplate
    {
        public class Workbench
        {
            static public readonly int ID = IDSequence;
            static public void Initialize()
            {
                Factory.Initialize();
                GameObject.Objects.Add(Factory.Default);
            }
            public class Factory : IItemFactory
            {
                public static void Initialize() { }
                public GameObject Create(List<GameObjectSlot> materials)
                {
                    return Create(materials.First(s => s.Name == "Body").Object.GetComponent<MaterialsComponent>().Parts["Body"].Material);
                }
                static GameObject Create(Material material)
                {
                    GameObject obj = new GameObject();
                    obj["Info"] = new GeneralComponent(ID, ObjectType.WorkBench, "WorkbenchReactions", "Used for crafting and storing blueprints.").Initialize(ItemSubType.Workbench);
                    obj.AddComponent<SpriteComponent>().Initialize(new Sprite("crate1", "crate1-z", new Vector2(16, 24), new Vector2(15, 32)));
                    //obj.AddComponent<WorkbenchReactionComponent>().Initialize(obj, materialCapacity: 4);
                    obj.AddComponent(new WorkbenchReactionComponent(4, 8));
                    obj["Physics"] = new PhysicsComponent(solid: false, height: 1, size: 1, weight: 6);
                    obj.AddComponent<GuiComponent>().Initialize(0);
                    obj["Workshop"] = new WorkshopComponent();
                    obj.AddComponent<PackableComponent>();
                    return obj;
                }
                static public readonly GameObject Default = Create(Material.LightWood);
            }
        }
    }
}
