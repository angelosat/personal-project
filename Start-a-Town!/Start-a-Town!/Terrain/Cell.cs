using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class Cell : Inspectable, ISlottable
    {
        public string GetName()
        {
            return this.Block.Label;
        }
        public Icon GetIcon()
        {
            return new Icon(this.Block.Variations.First());
        }
        public string GetCornerText()
        {
            return "";
        }
        public Color GetSlotColor()
        {
            return Color.White;
        }
        public void GetTooltipInfo(Control tooltip)
        {
            tooltip.Controls.Add(new Label(this.Block.GetName(this.BlockData)) { Location = tooltip.Controls.BottomLeft });
        }
        public void DrawUI(SpriteBatch sb, Vector2 pos)
        {
            this.Block.DrawUI(sb, pos, this.BlockData);
        }

        
        public static void Initialize()
        {

        }
        static readonly BitVector32.Section _orientation, _variation,_luminance, _blockData, _valid, _discovered;

        static Cell()
        {
            _valid = BitVector32.CreateSection(1); //1 bits
            _discovered = BitVector32.CreateSection(1, _valid); //1 bits
            _orientation = BitVector32.CreateSection(3, _discovered); //2 bits
            _variation = BitVector32.CreateSection(3, _orientation); //2 bits
            _luminance = BitVector32.CreateSection(15, _variation); //4 bits
            _blockData = BitVector32.CreateSection(15, _luminance); //4 bits // sum: 14bits
        }

        public byte X; // 1 byte
        public byte Y; // 1 byte
        public byte Z; // 1 byte
        public Block Block = BlockDefOf.Air; // 4 bytes
        public MaterialDef Material = MaterialDefOf.Air;
        //public MaterialDef Material => this.Block.GetMaterial(this.BlockData);
        public BitVector32 Data; // 4 bytes

        public Cell()
        {
            this.Valid = true;
        }
        public Cell(int localX, int localY, int localZ) : this()
        {
            this.X = (byte)localX;
            this.Y = (byte)localY;
            this.Z = (byte)localZ;
        }

        public IntVec3 Front => GetFront(this.Orientation);
        public IntVec3 Back => -this.Front;
        public override string Label => this.Block.Label;
        public bool Valid
        {
            get => this.Data[_valid] == 1;
            set => this.Data[_valid] = value ? 1 : 0;
        }
        public bool Discovered
        {
            get => this.Data[_discovered] == 1;
            set => this.Data[_discovered] = value ? 1 : 0;
        }
        public byte BlockData
        {
            get => (byte)this.Data[_blockData];
            set => this.Data[_blockData] = value;
        }
        public bool Opaque => this.Block.IsOpaque(this);
        public int Orientation
        {
            get => this.Data[_orientation];
            set => this.Data[_orientation] = value;
        }
        public int Variation
        {
            get => this.Data[_variation];
            set => this.Data[_variation] = value;
        }
        public bool IsRoomBorder => this.Block.IsRoomBorder;
        public byte Luminance
        {
            get => (byte)this.Data[_luminance];
            set => this.Data[_luminance] = value;
        }
        public float Fertility => this.Block.GetFertility(this);
        public IntVec3 LocalCoords => new(this.X, this.Y, this.Z);

        public IntVec3 GetGlobalCoords(Chunk chunk)
        {
            return new IntVec3(chunk.Start.X + this.X, chunk.Start.Y + this.Y, this.Z);
        }
        public bool IsSolid()
        {
            return this.Block.IsSolid(this);
        }
        public bool IsInvisible()
        {
            return !this.Opaque;
        }
        public static bool IsInvisible(Cell cell)
        {
            return cell.IsInvisible();
        }

        public SaveTag Save()
        {
            var data = new SaveTag(SaveTag.Types.Compound);
            data.Save(this.Block, "Block");
            this.Material.Save(data, "Material");
            this.Data.Data.Save(data, "Data");
            return data;
        }
        public Cell Load(SaveTag data)
        {
            this.Block = data.LoadBlock("Block");
            this.Data = new BitVector32((int)data["Data"].Value);
            this.Material = data.LoadDef<MaterialDef>("Material");
            return this;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Block);
            this.Material.Write(w);
            w.Write(this.Data.Data);

        }
        public Cell Read(BinaryReader r)
        {
            this.Block = r.ReadBlock();
            this.Material = Def.GetDef<MaterialDef>(r);
            this.Data = new BitVector32(r.ReadInt32());
            return this;
        }

        public static IntVec3 GetFront(int orientation)
        {
            return orientation switch
            {
                0 => new IntVec3(0, 1, 0),
                1 => new IntVec3(-1, 0, 0),
                2 => new IntVec3(0, -1, 0),
                3 => new IntVec3(1, 0, 0),
                _ => throw new Exception(),
            };
        }

        internal IEnumerable<IntVec3> GetOperatingPositions()
        {
            return this.Block.GetOperatingPositions(this);
        }
        internal IEnumerable<IntVec3> GetOperatingPositionsGlobal(IntVec3 global)
        {
            return this.Block.GetOperatingPositions(this, global);
        }

        //public override string ToString()
        //{
        //    return
        //        "Local: " + this.LocalCoords +
        //        "\nBlock: " + this.Block.Label +
        //        "\nMaterial: " + this.Material.Label +
        //        "\nVariation: " + this.Variation +
        //        "\nOrientation: " + this.Orientation +
        //        "\nDiscovered: " + this.Discovered +
        //        "\nValid: " + this.Valid;
        //}
    }
}
