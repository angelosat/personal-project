using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    partial class ItemTemplate
    {
        public class Hoe
        {
            static readonly int ID = GetNextID();
            static public void Initialize()
            {
                Factory.Initialize();
                GameObject.Objects.Add(Factory.Default);
                //StorageCategory.Tools.Add(Factory.Default);
            }
            static public GameObject Template
            {
                get { return GameObject.Objects[ID]; }
            }
            public class Factory
            {
                public static void Initialize() { }
                static readonly Sprite SpriteHandle = new Sprite("hoe/hoeHandle", 0.5f);
                static readonly Sprite SpriteHead = new Sprite("hoe/hoeHead", 0.5f);
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
                    return Create(materials[this.HandleReagent].GetComponent<ItemCraftingComponent>().Material, materials[this.HeadReagent].GetComponent<ItemCraftingComponent>().Material);
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
                static Entity Create(Material handleMaterial, Material headMaterial)
                {
                    var obj = ItemTemplate.Item;// new GameObject();

                    //obj.AddComponent<DefComponent>().Initialize(ID, ObjectType.Equipment, "Hoe", "Used to dig soil and dirt.").Initialize(ItemSubType.Hoe);
                    obj.Def = ToolDefs.Hoe;

                    //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("hoe", 0) { Origin = new Vector2(16, 24), Joint = new Vector2(16, 16) });//16));
                    //obj.AddComponent<PhysicsComponent>();
                    obj.AddComponent<GuiComponent>().Initialize(4, 1);
                    //obj.AddComponent<UseComponentOld>().Initialize(Script.Types.Tilling);
                    obj.AddComponent<ToolAbilityComponent>();//.Initialize(ToolAbilityDef.Argiculture);
                    //obj.AddComponent(new SkillComponentNew());

                    //var handleSprite = new Sprite(SpriteHandle) { Tint = handle.Color };
                    //var headSprite = new Sprite(SpriteHead) { Tint = head.Color };
                    //obj.Body.Sprite.Overlays.Add("Handle", handleSprite);
                    //obj.Body.Sprite.Overlays.Add("Head", headSprite);
                    //obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Handle", handle), new PartMaterialPair("Head", head));


                    var handleSprite = new Sprite(SpriteHandle) { OriginGround = new Vector2(16, 16), Material = handleMaterial };
                    var headSprite = new Sprite(SpriteHead) { OriginGround = new Vector2(16, 16), Material = headMaterial };
                    //Bone handle = new Bone(BoneDef.EquipmentHandle, handleSprite) { OriginGroundOffset = new Vector2(0, -8), Order = 0.001f, Material = handleMaterial };
                    Bone handle = new Bone(BoneDef.EquipmentHandle, handleSprite) { OriginGroundOffset = new Vector2(0, -16), Order = 0.001f, Material = handleMaterial };

                    handle.AddJoint(BoneDef.EquipmentHead);
                    Bone head = new Bone(BoneDef.EquipmentHead, headSprite) { Material = headMaterial };
                    handle.GetJoint(BoneDef.EquipmentHead).SetBone(head);
                    obj.AddComponent(new SpriteComponent(handle, new Sprite("hoe", 0) { OriginGround = new Vector2(16, 32) }));//24)}));

                    obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                    return obj;
                }
                static public readonly Entity Default = Create(MaterialDefOf.LightWood, MaterialDefOf.LightWood);
            }

            //static public readonly ItemToolDef Def = new ItemToolDef("Hoe", new ToolAbility(ToolAbilityDef.Argiculture, 5))
            //{
            //    StackCapacity = 1,
            //    Description = "Used to dig soil and dirt.",
            //    SubType = ItemSubType.Hoe,
            //    ID = ID,
            //};
        }
    }
}
