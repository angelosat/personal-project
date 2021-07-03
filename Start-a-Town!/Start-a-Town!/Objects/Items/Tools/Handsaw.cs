using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    partial class ItemTemplate
    {
        public class Handsaw
        {
            static public readonly int ID = GetNextID();
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
                public Entity Create()
                {
                    return Create(this.HiltGetter(), this.BladeGetter());
                }
                public Entity Create(Dictionary<string, Material> materials)
                {
                    //return Pickaxe(this.HandleGetter(), this.HeadGetter());
                    return Create(materials["Handle"], materials["Blade"]);
                }
                
                public Entity Create(List<GameObjectSlot> materials)
                {
                    //return Pickaxe(this.HandleGetter(), this.HeadGetter());
                    return Create(
                        //materials.First(s => s.Name == this.HandleReagent).Object.GetComponent<MaterialComponent>().Material,
                        //materials.First(s => s.Name == this.HeadReagent).Object.GetComponent<MaterialComponent>().Material);
                        materials.First(s => s.Name == this.HandleReagent).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material,
                        materials.First(s => s.Name == this.HeadReagent).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material);
                }
                //static public readonly ItemToolDef Def = new ItemToolDef("Handsaw", new ToolAbility(ToolAbilityDef.Carpentry, 5))
                //{
                //    StackCapacity = 1,
                //    Description = "Converts logs to planks.",
                //    SubType = ItemSubType.Handsaw,
                //    ID = ID,
                //};
                static Entity Create(Material handle, Material blade)
                {
                    //GameObject obj = new GameObject();
                    //obj.AddComponent<DefComponent>().Initialize(ID, objType: ObjectType.Equipment, name: "Handsaw", description: "Converts logs to planks").Initialize(ItemSubType.Handsaw);
                    Entity obj = ItemTemplate.Item;
                    obj.Def = ToolDefs.Handsaw;

                    obj.AddComponent<PhysicsComponent>().Initialize(weight: 1, size: 0);
                    //obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Handle", handle), new PartMaterialPair("Blade", blade));
                    obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                    obj.AddComponent<ToolAbilityComponent>();//.Initialize(ToolAbilityDef.Carpentry);
                    //obj.AddComponent<ResourcesComponent>().Initialize(Resource.Create(Resource.Types.Durability, 20, 20));// new Resources.Durability() { Max = 20 });


                    var handleSprite = new Sprite(SpriteHandle) { OriginGround = new Vector2(16, 16), Material = handle };
                    var headSprite = new Sprite(SpriteBlade) { OriginGround = new Vector2(16, 16), Material = blade };
                    //Bone handleBone = new Bone(BoneDef.EquipmentHandle, handleSprite) { OriginGroundOffset = new Vector2(0, -8), Order = 0.001f, Material = handle };
                    Bone handleBone = new Bone(BoneDef.EquipmentHandle, handleSprite) { OriginGroundOffset = new Vector2(0, -16), Order = 0.001f, Material = handle };

                    handleBone.AddJoint(BoneDef.EquipmentHead);
                    Bone head = new Bone(BoneDef.EquipmentHead, headSprite) { Material = blade };
                    handleBone.GetJoint(BoneDef.EquipmentHead).SetBone(head);
                    obj.AddComponent(new SpriteComponent(handleBone, new Sprite("handsaw/handsaw", 0.5f) { OriginGround = new Vector2(16, 32) }));//24)}));


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

                public Entity Create(Dictionary<string, Entity> materials)
                {
                    throw new NotImplementedException();
                }

                static public readonly Entity Default = Create(MaterialDefOf.Stone, MaterialDefOf.Stone);
            }

        }
    }
}
