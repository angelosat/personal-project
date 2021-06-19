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
    partial class ItemTemplate
    {
        public class Shovel
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
                static readonly Sprite SpriteHandle = new Sprite("shovel/shovelhandle");
                static readonly Sprite SpriteHead = new Sprite("shovel/shovelhead");
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
                    //materials.First(s => s.Name == this.HandleReagent).Object.GetComponent<MaterialComponent>().Material,
                    //materials.First(s => s.Name == this.HeadReagent).Object.GetComponent<MaterialComponent>().Material);
                    return Create(
                        materials.First(s => s.Name == this.HandleReagent).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material,
                        materials.First(s => s.Name == this.HeadReagent).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material);
                }
                static GameObject Create(Material handleMat, Material headMat)
                {
                    GameObject obj = new GameObject();
                    obj.AddComponent<GeneralComponent>().Initialize(ID, ObjectType.Equipment, "Shovel", "Used to dig soil and dirt.").Initialize(ItemSubType.Shovel);
                    //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("shovel") { Origin = new Vector2(16, 24), Joint = new Vector2(16, 16) });//16));
                    obj.AddComponent<GuiComponent>().Initialize(21, 1);
                    obj.AddComponent<UseComponentOld>().Initialize(Script.Types.Digging);
                    obj.AddComponent(new UseComponent(new Interactions.InteractionDigging()));
                    obj.AddComponent<SkillComponent>().Initialize(Skill.Digging);
                    obj.AddComponent<PhysicsComponent>();
                    //var handleSprite = new Sprite(SpriteHandle) { Tint = handleMat.Color };
                    //var headSprite = new Sprite(SpriteHead) { Tint = headMat.Color };
                    //obj.Body.Sprite.Overlays.Add("Handle", handleSprite);
                    //obj.Body.Sprite.Overlays.Add("Head", headSprite);
                    //obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Handle", handle), new PartMaterialPair("Head", head));

                    var handleSprite = new Sprite(SpriteHandle) { Origin = new Vector2(16, 16), Material = handleMat };
                    var headSprite = new Sprite(SpriteHead) { Origin = new Vector2(16, 16), Material = headMat };
                    Bone handle = new Bone(Bone.Types.Mainhand, handleSprite) { OriginGroundOffset = new Vector2(0, -8), Order = 0.001f, Material = handleMat };
                    handle.AddJoint(Bone.Types.EquipmentHead);
                    Bone head = new Bone(Bone.Types.EquipmentHead, headSprite) { Material = headMat };
                    handle.GetJoint(Bone.Types.EquipmentHead).SetBone(head);
                    obj.AddComponent(new SpriteComponent(handle, new Sprite("shovel") { Origin = new Vector2(16, 24) }));


                    obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                    //obj.AddComponent(new ToolComponent(ToolAbilities.Digging));
                    return obj;
                }
                static public readonly GameObject Default = Create(Material.LightWood, Material.LightWood);
            }
        }
    }
}
