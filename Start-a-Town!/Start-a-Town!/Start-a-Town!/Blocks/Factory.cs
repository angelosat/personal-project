using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public partial class Block
    {
        public class Factory
        {
            Block Block;
            public Factory(Block block)
            {
                this.Block = block;
            }
            public virtual GameObject Create(List<GameObjectSlot> reagents)
            {
                GameObject obj = this.Block.GetEntity().Clone();
                //IBlockState state = new State(reagents.First().Object.GetComponent<MaterialsComponent>().Parts["Body"].Material);
                IBlockState state = this.Block.BlockState;
                state.FromMaterial(reagents.First().Object);
                byte data = 0;
                state.Apply(ref data);
                obj.GetComponent<BlockComponent>().Data = data;
                return obj;
            }

            //public abstract GameObject Create(List<GameObjectSlot> materials);
            //{
            //    GameObject obj = new GameObject();
            //    obj.AddComponent<GeneralComponent>().Initialize(EntityIDRange + (int)this.Type, ObjectType.Block, this.GetName());
            //    obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
            //    obj.AddComponent<BlockComponent>().Initialize(this);
            //    //obj.AddComponent<SpriteComponent>().Initialize(new Sprite(this.Variations.First()) { Origin = Block.OriginCenter });
            //    obj.AddComponent<SpriteComponent>().Initialize(new Sprite(this.Variations.First().Name, Map.BlockDepthMap) { Origin = Block.OriginCenter });
            //    //obj.AddComponent<SpriteComponent>().Initialize(TileSprites[this.Type]);
            //    obj.AddComponent<GuiComponent>().Initialize(new Icon(obj.GetSprite()));
            //    return obj;
            //}
        }
    }
}
