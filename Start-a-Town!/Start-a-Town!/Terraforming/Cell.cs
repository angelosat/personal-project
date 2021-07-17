﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Specialized;
using Start_a_Town_.UI;

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
                "Local: " + LocalCoords +
                "\nTile ID: " + this.Block.Type +
                "\nStyle: " + Variation +
                "\nOrientation: " + Orientation +
                "\nVisible: " + Visible;
        }

        static BitVector32.Section _X, _Y, _Z, _HasData, _Orientation, _Visible;
        static BitVector32.Section _Variation, _HorEdges, _VerEdges, _Luminance, _BlockData;
        static BitVector32.Section _Valid, _Discovered, 
            _HorizontalEdges, // TODO remove
            _VerticalEdges; // TODO remove

        static public void Initialize()
        {
            _X = BitVector32.CreateSection((short)(Chunk.Size - 1)); //5 bits
            _Y = BitVector32.CreateSection((short)(Chunk.Size - 1), _X); //5 bits
            _Z = BitVector32.CreateSection((short)(MapBase.MaxHeight - 1), _Y); //7 bits

            _HasData = BitVector32.CreateSection(1, _Z); //1 bits
            _Orientation = BitVector32.CreateSection(3); //2 bits
            _Variation = BitVector32.CreateSection(3, _Orientation); //2 bits
            _Visible = BitVector32.CreateSection(1, _Variation);//_QueuedForActivation); //1 bit
            _HorEdges = BitVector32.CreateSection(15, _Visible); //4 bits
            _VerEdges = BitVector32.CreateSection(3, _HorEdges); //2 bits
            _Luminance = BitVector32.CreateSection(15, _VerEdges); //4 bits
            _BlockData = BitVector32.CreateSection(15, _Luminance); //4 bits

            _Valid = BitVector32.CreateSection(1); //1 bits
            _Discovered = BitVector32.CreateSection(1, _Valid); //1 bits
            _HorizontalEdges = BitVector32.CreateSection(15, _Discovered); //1 bits
            _VerticalEdges = BitVector32.CreateSection(3, _HorizontalEdges); //1 bits
        }

        public byte X; // 1 byte
        public byte Y; // 1 byte
        public byte Z; // 1 byte
        public Block Block = BlockDefOf.Air; // 4 bytes
        public BitVector32 Data2; // 4 bytes
        public BitVector32 ValidDiscovered; // 4 bytes

        public bool Valid
        {
            get { return this.ValidDiscovered[_Valid] == 1; }
            set { this.ValidDiscovered[_Valid] = value ? 1 : 0; }
        }
        public bool Discovered
        {
            get { return this.ValidDiscovered[_Discovered] == 1; }
            set { this.ValidDiscovered[_Discovered] = value ? 1 : 0; }
        }
        public byte BlockData
        {
            get { return (byte)Data2[_BlockData]; }
            set { Data2[_BlockData] = value; }
        }
        public bool Opaque
        {
            get 
            { 
                return this.Block.IsOpaque(this);
            }
        }
        public bool Visible
        {
            get { return Data2[_Visible] == 1; }
            set { Data2[_Visible] = value ? 1 : 0; }
        }
        public int Orientation
        {
            get { return Data2[_Orientation]; }
            set { Data2[_Orientation] = value; }
        }
        public int Variation
        {
            get { return Data2[_Variation]; }
            set { Data2[_Variation] = value; }
        }
        public bool IsRoomBorder => this.Block.IsRoomBorder;
        public byte Luminance
        {
            get { return (byte)Data2[_Luminance]; }
            set { Data2[_Luminance] = value; }
        }
        public float Fertility
        {
            get { return this.Block.GetFertility(this); }
        }
        public IntVec3 LocalCoords
        {
            get { return new IntVec3(X, Y, Z); }
            set
            {
                X = (byte)value.X;
                Y = (byte)value.Y;
                Z = (byte)value.Z;
            }
        }
       
        public Material Material => this.Block.GetMaterial(this.BlockData);

        public Cell()
        {
            this.Valid = true;
        }
        public Cell(int localX, int localY, int localZ):this()
        {
            this.X = (byte)localX;
            this.Y = (byte)localY;
            this.Z = (byte)localZ;
        }

        public IntVec3 GetGlobalCoords(Chunk chunk)
        {
            return new IntVec3(chunk.Start.X + X, chunk.Start.Y + Y, Z);
        }
        public bool IsSolid()
        {
            return this.Block.IsSolid(this);
        }
        public bool IsInvisible()
        {
            return !this.Opaque;
        }
        static public bool IsInvisible(Cell cell)
        {
            return cell.IsInvisible();
        }
       
        public SaveTag Save()
        {
            SaveTag data = new SaveTag(SaveTag.Types.Compound);
            data.Add(new SaveTag(SaveTag.Types.Byte, "Tile", (byte)this.Block.Type));
            data.Add(new SaveTag(SaveTag.Types.Int, "Data", this.Data2.Data));
            this.ValidDiscovered.Data.Save(data, "Extra");
            data.Add(new SaveTag(SaveTag.Types.Int, "ValidDiscovered", this.ValidDiscovered.Data));
            return data;
        }
        public Cell Load(SaveTag data)
        {
            this.SetBlockType((Block.Types)data["Tile"].Value);
            this.Data2 = new BitVector32((int)data["Data"].Value);
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
            writer.Write(Data2.Data);
            writer.Write(this.BlockData);
            writer.Write(this.Discovered);
            
        }
        public Cell Read(BinaryReader reader)
        {
            this.SetBlockType((Block.Types)reader.ReadByte());
            this.X = reader.ReadByte();
            this.Y = reader.ReadByte();
            this.Z = reader.ReadByte();
            this.Variation = reader.ReadByte();
            this.Data2 = new BitVector32(reader.ReadInt32());
            this.BlockData = reader.ReadByte();
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
