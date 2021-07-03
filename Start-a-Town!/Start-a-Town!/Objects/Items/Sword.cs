using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Animations;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    partial class ItemTemplate
    {
        public class Sword
        {
            static readonly int ID = GetNextID();
            static public void Initialize()
            {
                Factory.Initialize();
                GameObject.Objects.Add(Factory.Default);
            }
            public class Factory
            {
                public static void Initialize() { }
                static readonly Sprite SpriteHilt = new Sprite("sword/swordhilt");
                static readonly Sprite SpriteBlade = new Sprite("sword/swordblade");
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
                    return Create(materials["Hilt"], materials["Blade"]);
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
                static Entity Create(Material handleMat, Material headMat)
                {
                    var obj = new Entity();
                    obj["Info"] = new DefComponent(ID, objType: ObjectType.Weapon, name: "Sword", description: "A basic sword").Initialize(ItemSubType.Sword);
                    
                    var hiltSprite = new Sprite(SpriteHilt);// { Tint = hilt.Color };
                    var bladeSprite = new Sprite(SpriteBlade);// { Tint = blade.Color };
                    //obj.Body.Sprite.Overlays.Add("Hilt", hiltSprite);
                    //obj.Body.Sprite.Overlays.Add("Blade", bladeSprite);

                    var handleSprite = new Sprite(hiltSprite) { OriginGround = new Vector2(25, 6), Material = handleMat };
                    var headSprite = new Sprite(bladeSprite) { OriginGround = new Vector2(25, 6), Material = headMat };
                    //Bone handle = new Bone(BoneDef.EquipmentHandle, handleSprite) { OriginGroundOffset = new Vector2(9, -25), Order = 0.001f, Material = handleMat }; // 
                    Bone handle = new Bone(BoneDef.EquipmentHandle, handleSprite) { OriginGroundOffset = new Vector2(0, -16), Order = 0.001f, Material = handleMat }; // 

                    //handle.Origins.Add(BoneDef.None, new Vector2(16, 31));
                    //handle.Origins.Add(BoneDef.Hauled, new Vector2(16, 31));
                    //handle.Origins.Add(BoneDef.Mainhand, new Vector2(25, 6));
                    handle.AddJoint(BoneDef.EquipmentHead);

                    Bone head = new Bone(BoneDef.EquipmentHead, headSprite) { Material = headMat };
                    //head.Origins.Add(BoneDef.EquipmentHead, new Vector2(16, 31));
                    handle.GetJoint(BoneDef.EquipmentHead).SetBone(head);
                    obj.AddComponent(new SpriteComponent(handle, new Sprite("sword", 0.5f) { OriginGround = new Vector2(16, 32) }));//Origin = new Vector2(16, 28) }));


                    //EquipComponent.Add(obj, Tuple.Create(Stat.Types.Knockback, 1f));
                    obj["Physics"] = new PhysicsComponent(weight: 1, size: 0);
                    //obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Equipping, Script.Types.PickUp);
                    obj["Weapon"] = new WeaponComponent(1, Tuple.Create(Stat.Types.Slash, 10f));
                    //obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Hilt", hilt), new PartMaterialPair("Blade", blade));
                    //obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                    return obj;
                }
                static public readonly GameObject Default = Create(MaterialDefOf.LightWood, MaterialDefOf.LightWood);
            }


        }
    }
}
