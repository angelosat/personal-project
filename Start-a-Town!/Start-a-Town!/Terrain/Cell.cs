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
    public class Cell : ISlottable
    {
        public string GetName()
        {
            return this.Block.Type.ToString();
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
            tooltip.Controls.Add(new UI.Label(this.Block.GetName(this.BlockData)) { Location = tooltip.Controls.BottomLeft });
        }
        public void DrawUI(SpriteBatch sb, Vector2 pos)
        {
            this.Block.DrawUI(sb, pos, this.BlockData);
        }
        public void SetBlockType(Block.Types type)
        {
            this.Block = Block.Registry[type];
        }

        public void SetBlockType(int type)
        {
            this.Block = Block.Registry[(Block.Types)type];
        }

        public override string ToString()
        {
            return
                "Local: " + this.LocalCoords +
                "\nTile ID: " + this.Block.Type +
                "\nStyle: " + this.Variation +
                "\nOrientation: " + this.Orientation;
        }
        public static void Initialize()
        {

        }
        static readonly BitVector32.Section _orientation, _visible, _variation, _horEdges, _verEdges, _luminance, _blockData, _valid, _discovered;
        static Cell()
        {
            _orientation = BitVector32.CreateSection(3); //2 bits
            _variation = BitVector32.CreateSection(3, _orientation); //2 bits
            _visible = BitVector32.CreateSection(1, _variation);//_QueuedForActivation); //1 bit
            _horEdges = BitVector32.CreateSection(15, _visible); //4 bits
            _verEdges = BitVector32.CreateSection(3, _horEdges); //2 bits
            _luminance = BitVector32.CreateSection(15, _verEdges); //4 bits
            _blockData = BitVector32.CreateSection(15, _luminance); //4 bits

            _valid = BitVector32.CreateSection(1); //1 bits
            _discovered = BitVector32.CreateSection(1, _valid); //1 bits
        }

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

        public byte X; // 1 byte
        public byte Y; // 1 byte
        public byte Z; // 1 byte
        public Block Block = BlockDefOf.Air; // 4 bytes
        public BitVector32 Data; // 4 bytes
        public BitVector32 ValidDiscovered; // 4 bytes

        public bool Valid
        {
            get => this.ValidDiscovered[_valid] == 1;
            set => this.ValidDiscovered[_valid] = value ? 1 : 0;
        }
        public bool Discovered
        {
            get => this.ValidDiscovered[_discovered] == 1;
            set => this.ValidDiscovered[_discovered] = value ? 1 : 0;
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

        public MaterialDef Material => this.Block.GetMaterial(this.BlockData);

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
            SaveTag data = new SaveTag(SaveTag.Types.Compound);
            data.Add(new SaveTag(SaveTag.Types.Byte, "Tile", (byte)this.Block.Type));
            data.Add(new SaveTag(SaveTag.Types.Int, "Data", this.Data.Data));
            this.ValidDiscovered.Data.Save(data, "Extra");
            data.Add(new SaveTag(SaveTag.Types.Int, "ValidDiscovered", this.ValidDiscovered.Data));
            return data;
        }
        public Cell Load(SaveTag data)
        {
            this.SetBlockType((Block.Types)data["Tile"].Value);
            this.Data = new BitVector32((int)data["Data"].Value);
            data.TryGetTagValue<int>("ValidDiscovered", v => this.ValidDiscovered = new(v));
            return this;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((byte)this.Block.Type);
            writer.Write(this.X);
            writer.Write(this.Y);
            writer.Write(this.Z);
            writer.Write(this.Variation);
            writer.Write(this.Data.Data);
            //writer.Write(this.BlockData);
            writer.Write(this.Discovered);

        }
        public Cell Read(BinaryReader reader)
        {
            this.SetBlockType((Block.Types)reader.ReadByte());
            this.X = reader.ReadByte();
            this.Y = reader.ReadByte();
            this.Z = reader.ReadByte();
            this.Variation = reader.ReadByte();
            this.Data = new BitVector32(reader.ReadInt32());
            //this.BlockData = reader.ReadByte();
            this.Discovered = reader.ReadBoolean();
            return this;
        }

        public IntVec3 Front => GetFront(this.Orientation);
        public IntVec3 Back => -this.Front;

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
    }
}
