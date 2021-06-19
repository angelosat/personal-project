using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Components.Items
{
    public partial class ItemTemplate
    {
        public class Axe
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
                static readonly Sprite SpriteHandle = new Sprite("axe/axeHandle");
                static readonly Sprite SpriteHead = new Sprite("axe/axeHead");
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
                    return Create(materials["Handle"], materials["Head"]);
                }
                public GameObject Create(Dictionary<string, GameObject> materials)
                {
                    return Create(materials[this.HandleReagent].GetComponent<MaterialComponent>().Material, materials[this.HeadReagent].GetComponent<MaterialComponent>().Material);
                }
                public GameObject Create(List<GameObjectSlot> materials)
                {
                    //return Create(
                    //    materials.First(s => s.Name == this.HandleReagent).Object.GetComponent<MaterialComponent>().Material,
                    //    materials.First(s => s.Name == this.HeadReagent).Object.GetComponent<MaterialComponent>().Material);
                    return Create(
                        materials.First(s => s.Name == this.HandleReagent).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material,
                        materials.First(s => s.Name == this.HeadReagent).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material);
                }
                static GameObject Create(Material handleMaterial, Material headMaterial)
                {
                    GameObject obj = new GameObject();
                    //obj.AddComponent<GeneralComponent>().Initialize(ID, ObjectType.Equipment, "Axe", "Chops down trees.").Initialize(ItemSubType.Axe);
                    obj.AddComponent(new GeneralComponent(ID, ObjectType.Equipment, "Axe", "Chops down trees.") { StackMax = 1 });

                    //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("axe") { Origin = new Vector2(16, 24), Joint = new Vector2(16, 16) });//16));
                    //obj.AddComponent<GuiComponent>().Initialize(2, 1);
                    obj.AddComponent<PhysicsComponent>();

                    //var handleSprite = new Sprite(SpriteHandle) { Tint = handleMaterial.Color };
                    //var headSprite = new Sprite(SpriteHead) { Tint = headMaterial.Color };
                    //obj.Body.Sprite.Overlays.Add("Handle", handleSprite);
                    //obj.Body.Sprite.Overlays.Add("Head", headSprite);

                    //var handleSprite = new Sprite(SpriteHandle) { Origin = new Vector2(16, 24) };
                    //var headSprite = new Sprite(SpriteHead) { Origin = new Vector2(16, 24) };
                    //var handle = new Bone(Bone.Types.Torso, handleSprite) { Material = handleMaterial, ParentJoint = new Vector2(0) };//, OriginGroundOffset = new Vector2(0, -8), };
                    //var head = new Bone(Bone.Types.Head, headSprite, new Vector2(0,0), -.001f) { Material = headMaterial };//, ParentJoint = new Vector2(16, 16) };
                    //handle.AddChild(Bone.Types.Head, head);
                    //obj.AddComponent(new SpriteComponent(handle));

                    var handleSprite = new Sprite(SpriteHandle) { Origin = new Vector2(16, 16), Material = handleMaterial };
                    var headSprite = new Sprite(SpriteHead) { Origin = new Vector2(16, 16), Material = headMaterial };

                    //var handle = new Bone(Bone.Types.Torso, handleSprite) { Material = handleMaterial, ParentJoint = new Vector2(0) , OriginGroundOffset = new Vector2(0, -8), };
                    //var head = new Bone(Bone.Types.Head, headSprite, new Vector2(0, 0), -.001f) { Material = headMaterial };//, ParentJoint = new Vector2(16, 16) };
                    //handle.AddChild(Bone.Types.Head, head);

                    Bone handle = new Bone(Bone.Types.Mainhand, handleSprite) { OriginGroundOffset = new Vector2(0, -8), Order = 0.001f, Material = handleMaterial };
                    handle.AddJoint(Bone.Types.EquipmentHead);
                    Bone head = new Bone(Bone.Types.EquipmentHead, headSprite) { Material = headMaterial };
                    handle.GetJoint(Bone.Types.EquipmentHead).SetBone(head);
                    obj.AddComponent(new SpriteComponent(handle, new Sprite("axe") { Origin = new Vector2(16, 24)}));

                    //var handleSprite = new Sprite(SpriteHandle) { Origin = new Vector2(16, 24) };
                    //var headSprite = new Sprite(SpriteHead) { Origin = new Vector2(16, 24) };
                    //var handle = new Bone(Bone.Types.Torso, handleSprite) { Material = handleMaterial };// , Origin = new Vector2(0, -24) };
                    //var head = new Bone(Bone.Types.Head, headSprite, new Vector2(0, -8), -.001f) { Material = headMaterial };//, ParentJoint = new Vector2(16, 16) };
                    //handle.AddChild(Bone.Types.Head, head);
                    //obj.AddComponent(new SpriteComponent(handle));

                    //obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Handle", handleMaterial), new PartMaterialPair("Head", headMaterial));
                    //obj.AddComponent<SkillComponentNew>().Initialize(Skill.Chopping);
                    obj.AddComponent<SkillComponent>().Initialize(Skill.Chopping);

                    obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                    return obj;
                }
                static public readonly GameObject Default = Create(Material.LightWood, Material.LightWood);
            }
        }
    }
}
