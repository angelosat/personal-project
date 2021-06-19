using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Components.Items
{
    partial class ItemTemplate
    {
        public class Hammer
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
                static readonly Sprite SpriteHandle = new Sprite("hammer/hammerHandle", 0.5f);
                static readonly Sprite SpriteHead = new Sprite("hammer/hammerHead", 0.5f);
                Func<Material> HandleGetter;
                Func<Material> HeadGetter;
                string HandleReagent;
                string HeadReagent;

                /// <summary>
                /// initialize names of materials to derive from when creating item from a collection of materials
                /// </summary>
                /// <param name="handleReagent"></param>
                /// <param name="headReagent"></param>
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
                    obj.AddComponent<GeneralComponent>().Initialize(ID, ObjectType.Equipment, "Hammer", "Used to build constructions.").Initialize(ItemSubType.Hammer);
                    obj.AddComponent<PhysicsComponent>();
                    //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("hammer", new Vector2(16, 24), new Vector2(23, 8)));
                    obj.AddComponent<GuiComponent>().Initialize(3, 1);
                    obj.AddComponent<UseComponentOld>().Initialize(Script.Types.Build, Script.Types.BuildFootprint);
                    obj.AddComponent<SkillComponent>().Initialize(Skill.Building);
                    //var handleSprite = new Sprite(SpriteHandle) { Tint = handleMaterial.Color, Shininess = handleMaterial.Type.Shininess };// TODO: add material to sprite instead of color and shininess specifically
                    //var headSprite = new Sprite(SpriteHead) { Tint = headMaterial.Color, Shininess = handleMaterial.Type.Shininess };


                    var handleSprite = new Sprite(SpriteHandle) { Origin = new Vector2(16, 16), Material = handleMaterial };
                    var headSprite = new Sprite(SpriteHead) { Origin = new Vector2(16, 16), Material = headMaterial };
                    Bone handle = new Bone(Bone.Types.Mainhand, handleSprite) { OriginGroundOffset = new Vector2(0, -8), Order = 0.001f, Material = handleMaterial };
                    handle.AddJoint(Bone.Types.EquipmentHead);
                    Bone head = new Bone(Bone.Types.EquipmentHead, headSprite) { Material = headMaterial };
                    handle.GetJoint(Bone.Types.EquipmentHead).SetBone(head);
                    obj.AddComponent(new SpriteComponent(handle, new Sprite("hammer") { Origin = new Vector2(16, 24) }));

                    obj.Body.Sprite.Overlays.Add("Handle", handleSprite);
                    obj.Body.Sprite.Overlays.Add("Head", headSprite);
                    //obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Handle", handleMaterial), new PartMaterialPair("Head", head));
                    obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                    return obj;
                }
                static public readonly GameObject Default = Create(Material.LightWood, Material.LightWood);
            }

            //class InteractionBuild : Interaction
            //{
            //    public InteractionBuild()
            //    {
            //        this.Name = "Build";
            //        this.SetSeconds(2);
            //    }
            //}
        }
    }
}
