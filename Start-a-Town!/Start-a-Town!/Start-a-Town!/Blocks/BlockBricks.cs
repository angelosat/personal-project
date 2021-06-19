using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components.Materials;

namespace Start_a_Town_.Blocks
{
    class BlockBricks : Block
    {
        public BlockBricks()
            : base(Types.Bricks)
        {
            this.AssetNames = "bricks/bricks";
            //this.MaterialType = MaterialType.Mineral;
        }

        public override List<byte> GetVariations()
        {
            var vars = (from mat in Material.Templates.Values
                        where mat.Type == MaterialType.Mineral || mat.Type == MaterialType.Metal || mat.Type == MaterialType.Wood
                        select (byte)mat.ID).ToList();
            return vars;
        }
        public override Material GetMaterial(byte data)
        {
            return Material.Templates[data];
        }

        public override Color GetColor(byte data)
        {
            //var c = Components.Materials.Material.Templates[data].Color;// *.66f;
            //return c;
            var mat = Components.Materials.Material.Templates[data];
            var c = mat.Color;// *.66f;
            c.A = (byte)(255*mat.Type.Shininess);
            return c;
        }
        public override Vector4 GetColorVector(byte data)
        {
            var mat = Components.Materials.Material.Templates[data];
            var c = mat.ColorVector;
            return c;
        }
        public override byte ParseData(string data)
        {
            var mat = Material.Templates.Values.FirstOrDefault(m => string.Equals(m.Name, data, StringComparison.OrdinalIgnoreCase));
            if (mat == null)
                return (byte)Material.Stone.ID;
            //if (mat.Type != this.MaterialType)
            //    return (byte)Material.Stone.ID;
            return (byte)mat.ID;
        }

        
    }
}
