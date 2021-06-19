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
    public partial class ItemTemplate
    {
        public class Axe
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
                    return Create(materials[this.HandleReagent].GetComponent<ItemCraftingComponent>().Material, materials[this.HeadReagent].GetComponent<ItemCraftingComponent>().Material);
                }
                public GameObject Create(List<GameObjectSlot> materials)
                {
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
                //static public readonly ItemToolDef Def = new ItemToolDef("Axe", new ToolAbility(ToolAbilityDef.Chopping, 5))
                //{
                //    StackCapacity = 1,
                //    Description = "Chops down trees.",
                //    //ObjType = ObjectType.Equipment,
                //    SubType = ItemSubType.Axe,
                //    ID = ID,
                //};
                static Entity Create(Material handleMaterial, Material headMaterial)
                {
                    Entity obj = ItemTemplate.Item;// new GameObject();
                    obj.Def = ToolDefs.Axe;

                    var handleSprite = new Sprite(SpriteHandle) { OriginGround = new Vector2(16, 16), Material = handleMaterial };
                    var headSprite = new Sprite(SpriteHead) { OriginGround = new Vector2(16, 16), Material = headMaterial };

                    Bone handle = new Bone(BoneDef.EquipmentHandle, handleSprite) { OriginGroundOffset = new Vector2(0, -16), Order = 0.001f, Material = handleMaterial };
                    handle.AddJoint(BoneDef.EquipmentHead);
                    Bone head = new Bone(BoneDef.EquipmentHead, headSprite) { Material = headMaterial };
                    handle.GetJoint(BoneDef.EquipmentHead).SetBone(head);
                    obj.AddComponent(new SpriteComponent(handle, new Sprite("axe") { OriginGround = new Vector2(16, 32)}));//24)}));

                    //obj.AddComponent<ToolAbilityComponent>().Initialize(Def.Ability);// Skill.Chopping);
                    obj.AddComponent(new ToolAbilityComponent());// Skill.Chopping);

                    obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                    return obj;
                }
                static public readonly GameObject Default = Create(MaterialDefOf.LightWood, MaterialDefOf.LightWood);
            }
        }
    }
}
