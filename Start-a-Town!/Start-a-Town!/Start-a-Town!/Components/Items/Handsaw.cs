using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Components.Items
{
    partial class ItemTemplate
    {
        public class Handsaw
        {
            static readonly int ID = IDSequence;
            static public void Initialize()
            {
                Factory.Initialize();
                GameObject.Objects.Add(Factory.Default);
            }
            public class Factory : IItemFactory
            {
                public static void Initialize() { }
                static readonly Sprite SpriteHandle = new Sprite("handsaw/handsawhandle");
                static readonly Sprite SpriteBlade = new Sprite("handsaw/handsawblade");
                Func<Material> HiltGetter;
                Func<Material> BladeGetter;
                string HandleReagent;
                string HeadReagent;
                public Factory(string handleReagent, string headReagent)
                {
                    this.HandleReagent = handleReagent;
                    this.HeadReagent = headReagent;
                }
                public Factory(Func<Material> handleGetter, Func<Material> headgetter)
                {
                    this.HiltGetter = handleGetter;
                    this.BladeGetter = headgetter;
                }
                public GameObject Create()
                {
                    return Create(this.HiltGetter(), this.BladeGetter());
                }
                public GameObject Create(Dictionary<string, Material> materials)
                {
                    //return Pickaxe(this.HandleGetter(), this.HeadGetter());
                    return Create(materials["Handle"], materials["Blade"]);
                }
                public GameObject Create(Dictionary<string, GameObject> materials)
                {
                    //return Pickaxe(this.HandleGetter(), this.HeadGetter());
                    return Create(materials[this.HandleReagent].GetComponent<MaterialComponent>().Material, materials[this.HeadReagent].GetComponent<MaterialComponent>().Material);
                }
                public GameObject Create(List<GameObjectSlot> materials)
                {
                    //return Pickaxe(this.HandleGetter(), this.HeadGetter());
                    return Create(
                        //materials.First(s => s.Name == this.HandleReagent).Object.GetComponent<MaterialComponent>().Material,
                        //materials.First(s => s.Name == this.HeadReagent).Object.GetComponent<MaterialComponent>().Material);
                        materials.First(s => s.Name == this.HandleReagent).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material,
                        materials.First(s => s.Name == this.HeadReagent).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material);
                }
                static GameObject Create(Material handle, Material blade)
                {
                    //var handleSprite = new Sprite(SpriteHandle) { Tint = handle.Color };
                    //var bladeSprite = new Sprite(SpriteBlade) { Tint = blade.Color };
                    GameObject obj = new GameObject();
                    obj.AddComponent<GeneralComponent>().Initialize(ID, objType: ObjectType.Equipment, name: "Handsaw", description: "Converts logs to planks").Initialize(ItemSubType.Handsaw);
                    //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("handsaw/handsaw", 0.5f) { Origin = new Vector2(16, 24), Joint = new Vector2(25, 6) }
                    //    .AddOverlay("Handle", handleSprite)
                    //    .AddOverlay("Blade", bladeSprite));//16));
                    obj.AddComponent<GuiComponent>().Initialize(14, 1);
                    obj.AddComponent<PhysicsComponent>().Initialize(weight: 1, size: 0);
                    //obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Handle", handle), new PartMaterialPair("Blade", blade));
                    obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                    obj.AddComponent<SkillComponent>().Initialize(Skills.Skill.Carpentry);
                    //obj.AddComponent<ResourcesComponent>().Initialize(Resource.Create(Resource.Types.Durability, 20, 20));// new Resources.Durability() { Max = 20 });


                    var handleSprite = new Sprite(SpriteHandle) { Origin = new Vector2(16, 16), Material = handle };
                    var headSprite = new Sprite(SpriteBlade) { Origin = new Vector2(16, 16), Material = blade };
                    Bone handleBone = new Bone(Bone.Types.Mainhand, handleSprite) { OriginGroundOffset = new Vector2(0, -8), Order = 0.001f, Material = handle };
                    handleBone.AddJoint(Bone.Types.EquipmentHead);
                    Bone head = new Bone(Bone.Types.EquipmentHead, headSprite) { Material = blade };
                    handleBone.GetJoint(Bone.Types.EquipmentHead).SetBone(head);
                    obj.AddComponent(new SpriteComponent(handleBone, new Sprite("handsaw/handsaw", 0.5f) { Origin = new Vector2(16, 24) }));


                    //obj["Info"] = new GeneralComponent(ID, objType: ObjectType.Weapon, name: "Sword", description: "A basic sword").Initialize(ItemSubType.Sword);
                    //obj.AddComponent<GuiComponent>().Initialize(iconID: 22);
                    //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("sword", 0.5f) { Origin = new Vector2(16, 30), Joint = new Vector2(25, 6) }); //Origin = new Vector2(16, 24)
                    //var hiltSprite = new Sprite(SpriteHandle) { Tint = hilt.Color };
                    //var bladeSprite = new Sprite(SpriteBlade) { Tint = blade.Color };
                    //obj.Body.Sprite.Overlays.Add("Hilt", hiltSprite);
                    //obj.Body.Sprite.Overlays.Add("Blade", bladeSprite);
                    //obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                    //EquipComponent.Add(obj, Tuple.Create(Stat.Types.Knockback, 1f));
                    //obj["Physics"] = new PhysicsComponent(weight: 1, size: 0);
                    //obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Equipping, Script.Types.PickUp);
                    //obj["Weapon"] = new WeaponComponent(1, Tuple.Create(Stat.Types.Slash, 10f));
                    //obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Hilt", hilt), new PartMaterialPair("Blade", blade));
                    //obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                    return obj;
                }
                static public readonly GameObject Default = Create(Material.Twig, Material.Stone);
            }
        }
    }
}
