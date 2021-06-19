using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Components.Items
{
    partial class ItemTemplate
    {
        public class Bars
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
                    obj["Info"] = new GeneralComponent(ID, ObjectType.Bars, "Bars", "Used for crafting of weapons, armor, and tools.") { ItemSubType = ItemSubType.Bars };
                    obj.AddComponent<GuiComponent>().Initialize(17, 1);
                    obj["Sprite"] = new SpriteComponent(new Sprite("metalbars", Map.BlockDepthMap) { Origin = new Vector2(16, 24), Joint = new Vector2(16, 24), Tint = material.Color });
                    obj["Physics"] = new PhysicsComponent(size: 1);
                    obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools);
                    obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", material));
                    return obj;
                }
                static public readonly GameObject Default = Create(Material.Iron);
            }
        }
    }
}
