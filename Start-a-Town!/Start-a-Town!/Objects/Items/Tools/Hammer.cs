using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    partial class ItemTemplate
    {
        public class Hammer
        {
            static public int GetID() { return ID; }

            static public readonly int ID = GetNextID();
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
                //static public readonly ItemToolDef Def = new ItemToolDef("Hammer", new ToolAbility(ToolAbilityDef.Building, 5))
                //{
                //    StackCapacity = 1,
                //    Description = "Used to build constructions.",
                //    SubType = ItemSubType.Hammer,
                //    ID = ID,
                //};
                static Entity Create(Material handleMaterial, Material headMaterial)
                {
                    //GameObject obj = new GameObject();
                    //obj.AddComponent<DefComponent>().Initialize(ID, ObjectType.Equipment, "Hammer", "Used to build constructions.").Initialize(ItemSubType.Hammer);

                    var obj = ItemTemplate.Item;
                    obj.Def = ToolDefs.Hammer;

                    //obj.AddComponent<PhysicsComponent>();


                    obj.AddComponent<GuiComponent>().Initialize(3, 1);

                    obj.AddComponent<ToolAbilityComponent>().Initialize(ToolAbilityDef.Building);


                    //var handleSprite = new Sprite(SpriteHandle) { Tint = handleMaterial.Color, Shininess = handleMaterial.Type.Shininess };// TODO: add material to sprite instead of color and shininess specifically
                    //var headSprite = new Sprite(SpriteHead) { Tint = headMaterial.Color, Shininess = handleMaterial.Type.Shininess };


                    var handleSprite = new Sprite(SpriteHandle) { OriginGround = new Vector2(16, 16), Material = handleMaterial };
                    var headSprite = new Sprite(SpriteHead) { OriginGround = new Vector2(16, 16), Material = headMaterial };
                    //Bone handle = new Bone(BoneDef.EquipmentHandle, handleSprite) { OriginGroundOffset = new Vector2(0, -8), Order = 0.001f, Material = handleMaterial };
                    Bone handle = new Bone(BoneDef.EquipmentHandle, handleSprite) { OriginGroundOffset = new Vector2(0, -16), Order = 0.001f, Material = handleMaterial };

                    handle.AddJoint(BoneDef.EquipmentHead);
                    Bone head = new Bone(BoneDef.EquipmentHead, headSprite) { Material = headMaterial };
                    handle.GetJoint(BoneDef.EquipmentHead).SetBone(head);
                    obj.AddComponent(new SpriteComponent(handle, new Sprite("hammer") { OriginGround = new Vector2(16, 32) }));//24)}));

                    obj.Body.Sprite.Overlays.Add("Handle", handleSprite);
                    obj.Body.Sprite.Overlays.Add("Head", headSprite);
                    //obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Handle", handleMaterial), new PartMaterialPair("Head", head));
                    obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                    return obj;
                }
                static public readonly Entity Default = Create(MaterialDefOf.LightWood, MaterialDefOf.LightWood);
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
