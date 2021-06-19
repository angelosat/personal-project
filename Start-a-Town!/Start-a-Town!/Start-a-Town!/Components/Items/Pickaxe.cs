using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Components.Items
{
    partial class ItemTemplate
    {
        public class Pickaxe
        {
            static readonly int ID = IDSequence;
            static public void Initialize()
            {
                Factory.Initialize();
                GameObject.Objects.Add(Factory.Default);
            }
            public class Factory
            {
                public static void Initialize() { }
                static readonly Sprite SpriteHandle = new Sprite("pickaxe/pickaxeHandle");//, 0);
                static readonly Sprite SpriteHead = new Sprite("pickaxe/pickaxeHead");//, 0);
                Func<Material> HandleGetter;
                Func<Material> HeadGetter;
                string HandleReagent;
                string HeadReagent;
                public Factory(string handleReagent, string headReagent)
                {
                    this.HandleReagent = handleReagent;
                    this.HeadReagent = headReagent;
                }
                public Factory(Func<Material> handleGetter, Func<Material> headgetter)
                {
                    this.HandleGetter = handleGetter;
                    this.HeadGetter = headgetter;
                }
                public GameObject Create()
                {
                    return Create(this.HandleGetter(), this.HeadGetter());
                }
                public GameObject Create(Dictionary<string, Material> materials)
                {
                    //return Pickaxe(this.HandleGetter(), this.HeadGetter());
                    return Create(materials["Handle"], materials["Head"]);
                }
                public GameObject Create(Dictionary<string, GameObject> materials)
                {
                    //return Pickaxe(this.HandleGetter(), this.HeadGetter());
                    return Create(materials[this.HandleReagent].GetComponent<MaterialComponent>().Material, materials[this.HeadReagent].GetComponent<MaterialComponent>().Material);
                }
                public GameObject Create(List<GameObjectSlot> materials)
                {
                    //return Pickaxe(this.HandleGetter(), this.HeadGetter());
                    //return Create(
                    //    materials.First(s => s.Name == this.HandleReagent).Object.GetComponent<MaterialComponent>().Material, 
                    //    materials.First(s => s.Name == this.HeadReagent).Object.GetComponent<MaterialComponent>().Material);

                    //return Create(
                    //    materials.First(s => s.Name == this.HandleReagent).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material,
                    //    materials.First(s => s.Name == this.HeadReagent).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material);

                    return Create(
                        materials.First(s => s.Name == this.HandleReagent).Object.Body.Material,
                        materials.First(s => s.Name == this.HeadReagent).Object.Body.Material);

                }
                static GameObject Create(Material handleMaterial, Material headMaterial )
                {
                    //Material handle = null;
                    //Material head = null;
                    GameObject obj = new GameObject();
                    obj.AddComponent<GeneralComponent>().Initialize(ID, ObjectType.Equipment, "Pickaxe", "Used for mining").Initialize(ItemSubType.Pickaxe);
                    obj.GetInfo().StackMax = 1;
                    //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("pickaxe") { Origin = new Vector2(16, 24), Joint = new Vector2(16, 16) });
                    //var handleSprite = new Sprite(SpriteHandle) { Tint = handle.Color };
                    //var headSprite = new Sprite(SpriteHead) { Tint = head.Color };
                    //obj.Body.Sprite.Overlays.Add("Handle", handleSprite);
                    //obj.Body.Sprite.Overlays.Add("Head", headSprite);
                    //obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Handle", handle), new PartMaterialPair("Head", head));


                    var handleSprite = new Sprite(SpriteHandle) { Origin = new Vector2(16, 16), Material = handleMaterial };
                    var headSprite = new Sprite(SpriteHead) { Origin = new Vector2(16, 16), Material = headMaterial };
                    Bone handle = new Bone(Bone.Types.Mainhand, handleSprite) { OriginGroundOffset = new Vector2(0, -8), Order = 0.001f, Material = handleMaterial };
                    handle.AddJoint(Bone.Types.EquipmentHead);
                    Bone head = new Bone(Bone.Types.EquipmentHead, headSprite) { Material = headMaterial };
                    handle.GetJoint(Bone.Types.EquipmentHead).SetBone(head);
                    obj.AddComponent(new SpriteComponent(handle, new Sprite("pickaxe") { Origin = new Vector2(16, 24) }));


                    obj.AddComponent<GuiComponent>().Initialize(1, 1);
                    obj.AddComponent<UseComponentOld>().Initialize(Script.Types.Mining);
                    obj.AddComponent<PhysicsComponent>();
                    obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                    obj.AddComponent<SkillComponent>().Initialize(Skill.Mining);
                    return obj;
                }
                static public readonly GameObject Default = Create(Material.LightWood, Material.LightWood);
            }

        }
    }

}
