using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class BlockBricks : Block
    {
        public BlockBricks()
            : base(Types.Bricks)
        {
            this.LoadVariations("bricks/bricks");
            this.BuildProperties.WorkAmount = 20;
            this.ToggleConstructionCategory(ConstructionsManager.Walls, true);
            this.Ingredient = new Ingredient(RawMaterialDef.Boulders, null, null, 1);
        }

        public override IEnumerable<byte> GetEditorVariations()
        {
            return (from mat in MaterialDef.Registry.Values
                    where mat.Type == MaterialType.Stone
                    select (byte)mat.ID);
        }
        public override MaterialDef GetMaterial(byte data)
        {
            return MaterialDef.Registry[data];
        }
        public override byte GetDataFromMaterial(GameObject craftingReagent)
        {
            return (byte)craftingReagent.Body.Material.ID;
        }
        public override Color GetColor(byte data)
        {
            var mat = MaterialDef.Registry[data];
            var c = mat.Color;
            c.A = (byte)(255*mat.Type.Shininess);
            return c;
        }
        public override Vector4 GetColorVector(byte data)
        {
            var mat = MaterialDef.Registry[data];
            var c = mat.ColorVector;
            return c;
        }
        public override byte ParseData(string data)
        {
            var mat = MaterialDef.Registry.Values.FirstOrDefault(m => string.Equals(m.Name, data, StringComparison.OrdinalIgnoreCase));
            if (mat == null)
                return (byte)MaterialDefOf.Stone.ID;
            return (byte)mat.ID;
        }
    }
}
