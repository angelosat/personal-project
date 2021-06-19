using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Specialized;
using Start_a_Town_.Components;
using Start_a_Town_.Blocks;
using Start_a_Town_.GameModes;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    [Flags]
    public enum Edges { None = 0x0, West = 0x1, North = 0x2, East = 0x4, South = 0x8, All = 0xF }//, Top = 0x10, Bottom = 0x20 }
    [Flags]
    public enum VerticalEdges { None = 0x0, Top = 0x1, Bottom = 0x2, All = 0x3 }
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
        public void GetTooltipInfo(UI.Tooltip tooltip)
        {
            //tooltip.Controls.Add(new UI.Label(this.Type.ToString()) { Location = tooltip.Controls.BottomLeft });
            tooltip.Controls.Add(new UI.Label(this.Block.GetName(this.BlockData)) { Location = tooltip.Controls.BottomLeft });
        }
        public void DrawUI(SpriteBatch sb, Vector2 pos)
        {
            //Block.Registry[this.Type].Draw(sb, pos, Color.White, Vector4.One, 1, 0, this);
            //sb.Draw(this.Block.Variations.First().Atlas.Texture, pos, this.Block.BlockState.GetTint(this.BlockData));
            this.Block.DrawUI(sb, pos, this.BlockData);
        }
        public void SetBlockType(Block.Types type)
        {
            this.Block = Block.Registry[type];
        }

        //internal bool IsDeconstructible()
        //{
        //    return this.Block.IsDeconstructible(this.BlockData);
        //}

        public void SetBlockType(int type)
        {
            this.Block = Block.Registry[(Block.Types)type];
        }

        public override string ToString()
        {
            return
                "Local: " + LocalCoords +
                "\nTile ID: " + this.Block.Type +
                // "\nNext Tile ID: " + NextTile +
                // "\nBrightness: " + Brightness +
                "\nHorizontal Edges: " + HorizontalEdges +
                "\nVertical Edges: " + VerticalEdges +
                "\nStyle: " + Variation +
                "\nOrientation: " + Orientation +
                "\nVisible: " + Visible;// +
               // "\nSolid: " + Solid;
        }

        static BitVector32.Section _X, _Y, _Z, _HasData, _Orientation, _Visible; // _QueuedForActivation,//,_Skylight , _Edges
        static BitVector32.Section _Variation, _HorEdges, _VerEdges, _Luminance, _BlockData; //_NextTile,_Solid, 
        static BitVector32.Section _Valid, _Discovered, _HorizontalEdges, _VerticalEdges;

        static public void Initialize()
        {
            _X = BitVector32.CreateSection((short)(Chunk.Size - 1)); //5 bits
            _Y = BitVector32.CreateSection((short)(Chunk.Size - 1), _X); //5 bits
            _Z = BitVector32.CreateSection((short)(Map.MaxHeight - 1), _Y); //7 bits

            _HasData = BitVector32.CreateSection(1, _Z); //1 bits
            //_Orientation = BitVector32.CreateSection(3, _Z); //2 bits
            _Orientation = BitVector32.CreateSection(3); //2 bits
            _Variation = BitVector32.CreateSection(3, _Orientation); //2 bits
        //    _QueuedForActivation = BitVector32.CreateSection(1, _Variation); //1 bit
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

        #region fields
        public byte X; // 1 byte
        public byte Y; // 1 byte
        public byte Z; // 1 byte
        public Block Block = BlockDefOf.Air; // 4 bytes
        //public bool Valid = true; // 4 bytes
        //public bool Discovered; // 4 bytes
        public BitVector32 Data2; // 4 bytes
        public BitVector32 ValidDiscovered; // 4 bytes
        //public Edges HorizontalEdges = Edges.All; // 4 bytes
        //public VerticalEdges VerticalEdges = VerticalEdges.All; // 4 bytes
        #endregion

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
        public Edges HorizontalEdges
        {
            get { return (Edges)this.ValidDiscovered[_HorizontalEdges]; }
            set { this.ValidDiscovered[_HorizontalEdges] = (int)value; }

        }
        public VerticalEdges VerticalEdges
        {
            get { return (VerticalEdges)this.ValidDiscovered[_VerticalEdges]; }
            set {this.ValidDiscovered[_VerticalEdges] = (int)value; }
        }

        #region properties
        public byte BlockData
        {
            get { return (byte)Data2[_BlockData]; }
            set { Data2[_BlockData] = value; }
        }
        public bool Opaque
        {
            get 
            { 
                return this.Block.IsOpaque(this);//.Opaque;
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
        public bool IsRoomBorder => this.Block.IsRoomBorder;// this.Block.Opaque;
        public byte Luminance
        {
            get { return (byte)Data2[_Luminance]; }
            set { Data2[_Luminance] = value; }
        }
        public float Fertility
        {
            get { return this.Block.GetFertility(this); }
        }
        public Vector3 LocalCoords
        {
            get { return new Vector3(X, Y, Z); }
            set
            {
                X = (byte)value.X;
                Y = (byte)value.Y;
                Z = (byte)value.Z;
            }
        }
        internal bool IsExposed
        {
            get { return this.AllEdges != 0; }
        }
        public byte AllEdges
        {
            get
            {
                return (byte)(((int)this.VerticalEdges << 4) + this.HorizontalEdges);
            }
        }

        public Material Material => this.Block.GetMaterial(this.BlockData);
        #endregion



        public Cell()
        {
            this.Valid = true;
            this.HorizontalEdges = Edges.All; // 4 bytes
            this.VerticalEdges = VerticalEdges.All; // 4 bytes
        }
        public Cell(int localX, int localY, int localZ):this()
        {
            this.X = (byte)localX;
            this.Y = (byte)localY;
            this.Z = (byte)localZ;
        }

        public Vector3 GetGlobalCoords(Chunk chunk)
        {
            return new Vector3(chunk.Start.X + X, chunk.Start.Y + Y, Z);
        }
        public bool IsSolid()
        {
            return this.Block.IsSolid(this);// Block.Registry[this.Type].IsSolid(this);
        }
        public bool IsInvisible()
        {
            return !this.Opaque;
            //return this.Block.Type == Block.Types.Air;
        }
        static public bool IsInvisible(Cell cell)
        {
            //return cell.Block.Type == Block.Types.Air;
            return cell.IsInvisible();
        }
        public bool IsDrawable()
        {
            if (this.Block.Type == Block.Types.Air)
                return false;
            if (this.AllEdges == 0)
                return false;
            return true;
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

        static public List<Cell> GetNeighbors(Map map, Vector3 global)
        {
            var list = new List<Cell>();
            foreach (var vector in global.GetNeighbors())
            {
                Cell cell;
                if (vector.TryGetCell(map, out cell))
                    list.Add(cell);
            }
            return list;
        }

        

        static public bool CheckFace(Camera camera, Cell cell, Vector3 face)
        {
            return CheckFace(camera, cell.HorizontalEdges, cell.VerticalEdges, face);
        }
        static public bool CheckFace(Camera camera, Edges horEdges, VerticalEdges verEdges, Vector3 face)
        {
            Edges hor = Edges.None; ; VerticalEdges ver = VerticalEdges.None;
            int rx, ry;
            //Camera cam = new Camera(camera.Width, camera.Height, rotation: (int)-camera.Rotation);
            //Coords.Rotate(cam, face.X, face.Y, out rx, out ry);
            Coords.Rotate((int)camera.Rotation, face.X, face.Y, out rx, out ry);
            //   cell.GetOutlines(camera);
            // if (face.X == -1)
            if (rx == -1)
                hor = Edges.West;
            //  else if (face.X == 1)
            if (rx == 1)
                hor = Edges.East;
            //  if (face.Y == -1)
            if (ry == -1)
                hor = Edges.North;
            //  else if (face.Y == 1)
            if (ry == 1)
                hor = Edges.South;

            Edges transformed = horEdges;// cell.HorizontalEdges;// (Edges)cell.GetOutlines(camera);
            //Console.WriteLine(hor + " " + transformed);
            if (hor != Edges.None)
                if ((transformed & hor) == hor)
                    return true;

            if (face.Z == 1)
                ver = VerticalEdges.Top;
            else if (face.Z == -1)
                ver = VerticalEdges.Bottom;
            if (ver != VerticalEdges.None)
                //if ((cell.VerticalEdges & ver) == ver)
                if ((verEdges & ver) == ver)
                    return true;

            return false;
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
            //this.Type = ;
            this.SetBlockType((Block.Types)reader.ReadByte());
            this.X = reader.ReadByte();
            this.Y = reader.ReadByte();
            this.Z = reader.ReadByte();
            this.Variation = reader.ReadByte();
            //this.Data = new BitVector32(reader.ReadInt32());
            this.Data2 = new BitVector32(reader.ReadInt32());
            this.BlockData = reader.ReadByte();
            this.Discovered = reader.ReadBoolean();
            //this.SetVariation(this.Data2);
            return this;
        }


        public IntVec3 Front => Block.Front(this);
        public IntVec3 Back => Block.Back(this);

        internal IEnumerable<Vector3> GetOperatingPositions()
        {
            return this.Block.GetOperatingPositions(this);
        }
        internal IEnumerable<Vector3> GetOperatingPositionsGlobal(Vector3 global)
        {
            return this.Block.GetOperatingPositions(this, global);
        }
        internal bool IsHidden()
        {
            //return !this.IsExposed && ScreenManager.CurrentScreen.Camera.HideUnknownBlocks;
            return !this.IsExposed && Rooms.Ingame.CurrentMap.Camera.HideUnknownBlocks;

            //return this.AllEdges == 0 && !ScreenManager.CurrentScreen.Camera.DrawUnknownBlocks;
        }
        
    }
}
