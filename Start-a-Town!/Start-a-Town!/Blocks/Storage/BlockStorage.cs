using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    partial class BlockStorage : Block
    {
        static readonly Texture2D ChestNormalMap = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/furniture/chestnormal");
        public override MaterialDef GetMaterial(byte blockdata)
        {
            return MaterialDef.Registry[blockdata];
        }
        public BlockStorage()
            : base("Bin", opaque: false)
        {
            var tex = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/furniture/chest").ToGrayscale();
            this.Variations.Add(Atlas.Load("chestgrayscale", tex, BlockDepthMap, ChestNormalMap));
            this.ToggleConstructionCategory(ConstructionsManager.Furniture, true);
        }

        public override IEnumerable<byte> GetEditorVariations()
        {
            var vars = (from mat in MaterialDef.Registry.Values
                        where mat.Type == MaterialType.Wood || mat.Type == MaterialType.Metal
                        select (byte)mat.ID);
            return vars;
        }
        public override BlockEntity CreateBlockEntity(IntVec3 originGlobal)
        {
            return new BlockStorageEntity(originGlobal);
        }
        public override Vector4 GetColorVector(byte data)
        {
            var mat = MaterialDef.Registry[data];
            var c = mat.ColorVector;
            return c;
        }

        public override void OnDrop(GameObject actor, GameObject dropped, TargetArgs target, int amount = -1)
        {
            var binEntity = actor.Map.GetBlockEntity(target.Global) as BlockStorageEntity;
            binEntity.Insert(dropped);
            actor.ClearCarried();
        }
    }
}
