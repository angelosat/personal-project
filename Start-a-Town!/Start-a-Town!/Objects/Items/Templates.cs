using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public partial class ItemTemplate
    {
        static int _IDSequence = 0;
        //public static int IDSequence { get { return IDOffset + _IDSequence++; } }
        public static int GetNextID()
        {
            return IDOffset + _IDSequence++;
        }
        const int IDOffset = 10000;

        static public void Initialize()
        {
            //Pickaxe.Initialize();
            //Shovel.Initialize();
            //Sword.Initialize();
            //Axe.Initialize();
            //Hammer.Initialize();
            //Hoe.Initialize();
            //Handsaw.Initialize();

            //Sapling.Initialize();

            //FurnitureParts.Initialize();

            //ItemPie.Initialize();
        }

        //public abstract class Factory
        //{
        //    public abstract GameObject Create(List<GameObjectSlot> materials);
        //}

        static public Entity Item
        {
            get
            {
                var obj = new Entity();
                obj.AddComponent(new DefComponent());
                obj.AddComponent<PhysicsComponent>();
                obj.AddComponent(new OwnershipComponent());
                return obj;
            }
        }
        
        
        //public class Smeltery
        //{
        //    static public readonly int ID = IDSequence;
        //    static public void Initialize()
        //    {
        //        Factory.Initialize();
        //        GameObject.Objects.Add(Factory.Default);
        //    }
        //    public class Factory
        //    {
        //        public static void Initialize() { }
        //        public GameObject Create(List<GameObjectSlot> materials)
        //        {
        //            return Create(materials.First(s => s.Name == "Body").Object.GetComponent<MaterialsComponent>().Parts["Body"].Material);
        //        }
        //        static GameObject Create(Material material)
        //        {
        //            GameObject obj = new GameObject();
        //            obj["Info"] = new GeneralComponent(ID, ObjectType.Smeltery, "Smeltery", "Used for smelting things.").Initialize(ItemSubType.Smeltery);
        //            //obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.Cobblestone]);
        //            obj["Sprite"] = new SpriteComponent(new Sprite(Block.Stone.Variations.First().Name, Map.BlockDepthMap) { OriginGround = Block.OriginCenter, MouseMap = Block.BlockMouseMap });//new Sprite(Block.Stone.Variations.First()));
        //            //obj["Sprite"] = new SpriteComponent(new Sprite(Block.Stone.GetObject().GetSprite()));//new Sprite(Block.Stone.Variations.First()));
        //            obj["Physics"] = new PhysicsComponent(solid: false, height: 1, size: 1);
        //            obj.AddComponent<GuiComponent>().Initialize(0);
        //            obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Activate);
        //            obj["Crafting"] = new SmelteryComponent(4, 4, 4);
        //            return obj;
        //        }
        //        static public readonly GameObject Default = Create(Material.LightWood);
        //    }
        //}
    }
}
