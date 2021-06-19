using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Components.Items
{
    partial class ItemTemplate
    {
        public class Sword
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
                public GameObject Create()
                {
                    return Create(this.HiltGetter(), this.BladeGetter());
                }
                public GameObject Create(Dictionary<string, Material> materials)
                {
                    //return Pickaxe(this.HandleGetter(), this.HeadGetter());
                    return Create(materials["Hilt"], materials["Blade"]);
                }
                public GameObject Create(Dictionary<string, GameObject> materials)
                {
                    //return Pickaxe(this.HandleGetter(), this.HeadGetter());
                    return Create(materials[this.HandleReagent].GetComponent<MaterialComponent>().Material, materials[this.HeadReagent].GetComponent<MaterialComponent>().Material);
                }
                public GameObject Create(List<GameObjectSlot> materials)
                {
                    //return Pickaxe(this.HandleGetter(), this.HeadGetter());
                    return Create(
                        //materials.First(s => s.Name == this.HandleReagent).Object.GetComponent<MaterialComponent>().Material,
                        //materials.First(s => s.Name == this.HeadReagent).Object.GetComponent<MaterialComponent>().Material);
                        materials.First(s => s.Name == this.HandleReagent).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material,
                        materials.First(s => s.Name == this.HeadReagent).Object.GetComponent<MaterialsComponent>().Parts["Body"].Material);
                }
                static GameObject Create(Material handleMat, Material headMat)
                {
                    GameObject obj = new GameObject();
                    obj["Info"] = new GeneralComponent(ID, objType: ObjectType.Weapon, name: "Sword", description: "A basic sword").Initialize(ItemSubType.Sword);
                    obj.AddComponent<GuiComponent>().Initialize(iconID: 22);
                    //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("sword", 0.5f) { Origin = new Vector2(16, 30), Joint = new Vector2(25, 6) }); //Origin = new Vector2(16, 24)
                    var hiltSprite = new Sprite(SpriteHilt);// { Tint = hilt.Color };
                    var bladeSprite = new Sprite(SpriteBlade);// { Tint = blade.Color };
                    //obj.Body.Sprite.Overlays.Add("Hilt", hiltSprite);
                    //obj.Body.Sprite.Overlays.Add("Blade", bladeSprite);

                    var handleSprite = new Sprite(hiltSprite) { Origin = new Vector2(25, 6), Material = handleMat };
                    var headSprite = new Sprite(bladeSprite) { Origin = new Vector2(25, 6), Material = headMat };
                    Bone handle = new Bone(Bone.Types.EquipmentHandle, handleSprite) { OriginGroundOffset = new Vector2(9, -25), Order = 0.001f, Material = handleMat }; // 
                    //handle.Origins.Add(Bone.Types.None, new Vector2(16, 31));
                    //handle.Origins.Add(Bone.Types.Hauled, new Vector2(16, 31));
                    //handle.Origins.Add(Bone.Types.Mainhand, new Vector2(25, 6));
                    handle.AddJoint(Bone.Types.EquipmentHead);

                    Bone head = new Bone(Bone.Types.EquipmentHead, headSprite) { Material = headMat };
                    //head.Origins.Add(Bone.Types.EquipmentHead, new Vector2(16, 31));
                    handle.GetJoint(Bone.Types.EquipmentHead).SetBone(head);
                    obj.AddComponent(new SpriteComponent(handle, new Sprite("sword", 0.5f) { Origin = new Vector2(16, 31) }));//Origin = new Vector2(16, 28) }));


                    obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand); //Offhand);// 
                    EquipComponent.Add(obj, Tuple.Create(Stat.Types.Knockback, 1f));
                    obj["Physics"] = new PhysicsComponent(weight: 1, size: 0);
                    obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Equipping, Script.Types.PickUp);
                    obj["Weapon"] = new WeaponComponent(1, Tuple.Create(Stat.Types.Slash, 10f));
                    //obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Hilt", hilt), new PartMaterialPair("Blade", blade));
                    //obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                    return obj;
                }
                static public readonly GameObject Default = Create(Material.LightWood, Material.LightWood);
            }


        }
    }
}
