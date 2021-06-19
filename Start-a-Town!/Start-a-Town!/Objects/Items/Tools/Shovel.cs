using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Animations;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_
{
    partial class ItemTemplate
    {
        static public class Shovel
    {
        static Shovel()
        {

        }
        static public readonly int ID = ItemTemplate.GetNextID();
        static public void Initialize()
        {
            Factory.Initialize();
            //GameObject.Objects.Add(Factory.Default);
        }
        static public GameObject Template
        {
            get { return GameObject.Objects[ID]; }
        }
        static readonly Sprite SpriteHandle = new Sprite("shovel/shovelhandle") { OriginGround = new Vector2(16, 16) };
        static readonly Sprite SpriteHead = new Sprite("shovel/shovelhead") { OriginGround = new Vector2(16, 16) };
        //static readonly Sprite SpriteHandle = new Sprite("shovel/shovelhandle") ;
        //static readonly Sprite SpriteHead = new Sprite("shovel/shovelhead");
        public class Factory
        {
            public static void Initialize() { }

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
            public Entity Create()
            {
                return Create(this.HandleGetter(), this.HeadGetter());
            }
            public Entity Create(Dictionary<string, Material> materials)
            {
                return Create(materials["Handle"], materials["Head"]);
            }
            public Entity Create(Dictionary<string, GameObject> materials)
            {
                return Create(materials[this.HandleReagent].GetComponent<ItemCraftingComponent>().Material, materials[this.HeadReagent].GetComponent<ItemCraftingComponent>().Material);
            }
            public Entity Create(List<GameObjectSlot> materials)
            {
                //return Create(
                //materials.First(s => s.Name == this.HandleReagent).Object.GetComponent<MaterialComponent>().Material,
                //materials.First(s => s.Name == this.HeadReagent).Object.GetComponent<MaterialComponent>().Material);
                return Create(
                    materials.First(s => s.Name == this.HandleReagent).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material,
                    materials.First(s => s.Name == this.HeadReagent).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material);
            }
            static public Entity Create(Material handleMat, Material headMat)
            {
                var obj = ItemTemplate.Item;// new GameObject();
                obj.Def = ToolDefs.Shovel;

                obj.AddComponent<GuiComponent>().Initialize(21, 1);
                obj.AddComponent<ToolAbilityComponent>();
                obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);

                //var handleSprite = new Sprite(SpriteHandle) { OriginGround = new Vector2(16, 16), Material = handleMat };
                //var headSprite = new Sprite(SpriteHead) { OriginGround = new Vector2(16, 16), Material = headMat };
                //Bone handle = new Bone(BoneDef.EquipmentHandle, handleSprite) { OriginGroundOffset = new Vector2(0, -16), Order = 0.001f, Material = handleMat };
                //handle.AddJoint(BoneDef.EquipmentHead);
                //Bone head = new Bone(BoneDef.EquipmentHead, headSprite) { Material = headMat };
                //handle.GetJoint(BoneDef.EquipmentHead).SetBone(head);
                //obj.AddComponent(
                //    new SpriteComponent(handle, new Sprite("shovel") { OriginGround = new Vector2(16, 32) })
                //        .AddMaterial(BoneDef.EquipmentHandle, handleMat)
                //        .AddMaterial(BoneDef.EquipmentHead, headMat));// 24) }));

                obj.AddComponent(
                    new SpriteComponent(obj.Def.Body, obj.Def.DefaultSprite)// DefaultSprite)
                        .SetMaterial(BoneDef.EquipmentHandle, handleMat)
                        .SetMaterial(BoneDef.EquipmentHead, headMat));// 24) }));

                return obj;
            }
            static public readonly Entity Default = Create(MaterialDefOf.LightWood, MaterialDefOf.LightWood);
        }

        //static public Sprite DefaultSprite = new Sprite("shovel") { OriginGround = new Vector2(16, 32) };
        //static public readonly Bone BoneHandle =
        //    new Bone(BoneDef.EquipmentHandle, SpriteHandle, Vector2.Zero, 0.001f) { DrawMaterialColor = true, OriginGroundOffset = new Vector2(0, -16) }
        //        .AddJoint(Vector2.Zero, new Bone(BoneDef.EquipmentHead, SpriteHead) { DrawMaterialColor = true });

        //static public readonly ItemToolDef Def = new ItemToolDef("Shovel", new ToolAbility(ToolAbilityDef.Digging, 5))
        //{
        //    StackCapacity = 1,
        //    Description = "Used to dig soil and dirt.",
        //    SubType = ItemSubType.Shovel,
        //    ID = ID,
        //    Body = BoneHandle
        //};
        }
    }
}
