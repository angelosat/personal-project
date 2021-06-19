using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Vegetation;

namespace Start_a_Town_.Components.Items
{
    partial class ItemTemplate
    {
        public class Sapling
        {
            static readonly int ID = IDSequence;
            static public void Initialize()
            {
                Factory.Initialize();
                GameObject.Objects.Add(Factory.Default);
            }
            public class Factory
            {
                internal static void Initialize()
                {
                    
                }

                public static GameObject Create()
                {
                    GameObject obj = new GameObject();
                    obj.AddComponent<GeneralComponent>().Initialize(ID, ObjectType.Plant, "Sapling", "Used to grow trees.").Initialize(ItemSubType.Sapling);
                    obj.GetInfo().StackMax = 8;
                    obj.AddComponent<PhysicsComponent>().Initialize(size: 0);
                    //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("blocks/sapling", new Vector2(16, 32), new Vector2(16, 32)));
                    obj.AddComponent(new SpriteComponent((new Sprite("blocks/sapling", new Vector2(16, 32), new Vector2(16, 32)))));

                    obj.AddComponent<GuiComponent>().Initialize(3, 1);
                    obj.GetGui().StackMax = 8;
                    //obj.AddComponent<SkillComponent>().Initialize(Skill.Building);
                    obj.AddComponent(new SaplingComponent());
                    obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                    return obj;
                }

                static public readonly GameObject Default = Create();
            }
        }
    }
}
