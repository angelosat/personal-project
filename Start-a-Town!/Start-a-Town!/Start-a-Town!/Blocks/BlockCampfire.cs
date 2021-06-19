using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Particles;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Blocks
{

    class BlockCampfire : Block
    {
        public override Material GetMaterial(byte blockdata)
        {
            return Material.LightWood;
        }
        //static public ParticleEmitter FlameEmitter = new ParticleEmitter(vecto)
        public BlockCampfire()
            : base(Block.Types.Campfire, opaque: false, solid: false)
        {
            //this.MaterialType = MaterialType.Wood;
            //this.Material = Material.LightWood;
            this.Variations.Add(Block.Atlas.Load("blocks/campfire", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap));
        }
        public override BlockEntity GetBlockEntity()
        {
            return new Entity();
        }

        public override void Place(IMap map, Vector3 global, byte data, int variation, int orientation)
        {
            if (!map.GetBlock(global - Vector3.UnitZ).Opaque)
                return;
            base.Place(map, global, data, variation, orientation);
            map.SetBlockLuminance(global, 15);
        }
        public override void Remove(IMap map, Vector3 global)
        {
            base.Remove(map, global);
            map.SetBlockLuminance(global, 0);
        }


        class Entity : BlockEntity
        {
            ParticleEmitter Emitter = ParticleEmitter.Fire;
            public override void Update(IObjectProvider net, Vector3 global)
            {
                if (net is Server)
                    return;
                this.Emitter.Update();
            }
            public override void Draw(Camera camera, IMap map, Vector3 global)
            {
                this.Emitter.Draw(camera, map, global);
            }
            public override object Clone()
            {
                return new Entity();
            }
        }
    }
}
