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

namespace Start_a_Town_
{
    public class Cell : ISlottable
    {
        //public LightToken Light;
        //public MyVertex[] CachedVertices;
        #region Interfaces
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
        #endregion
        public Block Block = Block.Air;// { get { return Block.Registry[this.Type]; } }
        public void SetBlockType(Block.Types type)
        {
            this.Block = Block.Registry[type];
        }
        public void SetBlockType(int type)
        {
            this.Block = Block.Registry[(Block.Types)type];
        }
        static public bool TryGetObject(IMap map, Vector3 global, out GameObject gameObject)//Net.IObjectProvider net, Vector3 global, out GameObject gameObject)
        {
            Chunk chunk;
            Cell cell;
            gameObject = null;
            if (!map.TryGetAll(global, out chunk, out cell))
                return false;

            GameObject tar = GameObjectDb.BlockEmpty;
            BlockComponent tile = tar.GetComponent<BlockComponent>();
            tile.Type = cell.Block.Type;

            Block block = cell.Block;
            tile.Block = block;
            BlockEntity entity;
            if (chunk.BlockEntities.TryGetValue(global.ToLocal(), out entity))
                tile.BlockEntity = entity;
   
            tar["Info"] = block.GetEntity().GetInfo();

            tar.Global = cell.GetGlobalCoords(chunk);

            tile["HorEdges"] = map.GetCell(global).HorizontalEdges;
            tile["VerEdges"] = map.GetCell(global).VerticalEdges;
            byte slight, blight;
            map.GetLight(global, out slight, out blight);
            tile["Sunlight"] = slight;
            tile["Blocklight"] = blight;

            tar.SetGlobal(global); // WARNING! i don't want to add the temp object to the chunk!!!
            tar.Transform.Exists = true;
            tar.Net = Net.Client.Instance;
            gameObject = tar;

            //var obj = new GameObject();
            //obj.AddComponent(new BlockComponent());
            //obj.Global = cell.GetGlobalCoords(chunk);
            //gameObject = obj;
            return true;
        }


        public override string ToString()
        {
            return
                "Local: " + LocalCoords +
                "\nTile ID: " + this.Block.Type +
                // "\nNext Tile ID: " + NextTile +
                //  "\nSkylight: " + Skylight +
                // "\nBrightness: " + Brightness +
                "\nHorizontal Edges: " + HorizontalEdges +
                "\nVertical Edges: " + VerticalEdges +
                "\nStyle: " + Variation +
                "\nOrientation: " + Orientation +
                "\nVisible: " + Visible;// +
               // "\nSolid: " + Solid;
        }


        public Edges HorizontalEdges = Edges.All;
        //{
        //    get { return (Edges)Data2[_HorEdges]; }
        //    set {
        //        Data2[_HorEdges] = (int)value; 
        //    }
        //}
        public VerticalEdges VerticalEdges = VerticalEdges.All;
        //{
        //    get { return (VerticalEdges)Data2[_VerEdges]; }
        //    set {
        //        this.Data2[_VerEdges] = (int)value;
        //    }
        //}

        public byte AllEdges
        {
            get
            {
                return (byte)(((int)this.VerticalEdges << 4) + this.HorizontalEdges);
            }
        }


        //public BitVector32 Data; 
        public BitVector32 Data2;
        //public Block.Types Type;// { get; set; }

        #region Initialization
        static BitVector32.Section _X, _Y, _Z, _HasData, _Orientation, _Visible; // _QueuedForActivation,//,_Skylight , _Edges
        static BitVector32.Section _Variation, _HorEdges, _VerEdges, _Light, _BlockData; //_NextTile,_Solid, 
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
            _Light = BitVector32.CreateSection(15, _VerEdges); //4 bits
            _BlockData = BitVector32.CreateSection(15, _Light); //4 bits
        }
        #endregion

        public bool Opaque
        {
            //get { return BlockComponent.Blocks[this.Type].Opaque; }
            //get { return this.Block.Opaque; }
            get 
            { 
                return this.Block.IsOpaque(this);//.Opaque;
            }
        }
        //public bool IsOpaque()
        //{
        //    return this.Block.IsOpaque(this);
        //}
        public bool Valid = true;
        // TODO: convert to field
        public byte BlockData
        {
            get { return (byte)Data2[_BlockData]; }
            set { Data2[_BlockData] = value; }
        }
        
        public bool Visible
        {
            get { return Data2[_Visible] == 1; }
            set { Data2[_Visible] = value ? 1 : 0; }
        }

        bool _Queued;
        public bool QueuedForActivation
        {
            get { return _Queued;}// Data2[_QueuedForActivation] == 1; }
            set { _Queued = value;}// Data2[_QueuedForActivation] = value ? 1 : 0; }
        }

        public Cell()
        {
            //this.Block = Block.Air;
            //Data = new BitVector32();
           // Data2 = new BitVector32();
        }
        public Cell(int localX, int localY, int localZ)
           // : this()
        {
            this.X = (byte)localX;
            this.Y = (byte)localY;
            this.Z = (byte)localZ;
        }
        #region Private Fields 

        public int Orientation
        {
            get { return Data2[_Orientation]; }
            set { Data2[_Orientation] = value; }
        }
        public byte Variation;

        //public int Variation;
        //{
        //    get { return Data2[_Variation]; }
        //    set 
        //    {
        //        Data2[_Variation] = value;
        //    }
        //}
        
        public void SetVariation(BitVector32 data)
        {
            this.Variation = (byte)data[_Variation];
        }

        #endregion

        #region Public Fields

        public byte X;
        public byte Y;
        public byte Z;

        //public int X;
        //{
        //    get { return Data[_X]; }
        //    set { Data[_X] = value; }
        //}
        //public int Y;
        //{
        //    get { return Data[_Y]; }
        //    set { Data[_Y] = value; }
        //}
        //public int Z;
        //{
        //    get { return Data[_Z]; }
        //    set { Data[_Z] = value; }
        //}
        #endregion

        #region Properties

        public byte Luminance
        {
            get { return (byte)Data2[_Light]; }
            set { Data2[_Light] = value; }
        }


        public Vector3 GetGlobalCoords(Chunk chunk)
        {
            return new Vector3(chunk.Start.X + X, chunk.Start.Y + Y, Z);
        }
        public Vector3 LocalCoords
        {
            get { return new Vector3(X, Y, Z); }
            set
            {
                X = (byte)value.X;
                Y = (byte)value.Y;
                Z = (byte)value.Z;
                //Location = Coords.iso(GlobalCoords);

                //foreach (DrawableWorldEntity entity in Entities)
                //    entity.Location = Location;
            }
        }
        public bool IsSolid()
        {
            return this.Block.IsSolid(this);// Block.Registry[this.Type].IsSolid(this);
        }
        public bool IsSolid(IMap map)
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
        #endregion


        #region Saving and loading
        static public SaveTag Save(Cell cell)
        {
            SaveTag data = new SaveTag(SaveTag.Types.Compound);

            data.Add(new SaveTag(SaveTag.Types.Byte, "Tile", (byte)cell.Block.Type));

                //byte packed = (byte)((cell.Variation << 4) + cell.Orientation);
                //data.Add(new SaveTag(SaveTag.Types.Byte, "Packed", packed));
            data.Add(new SaveTag(SaveTag.Types.Byte, "Variation", cell.Variation));
                data.Add(new SaveTag(SaveTag.Types.Int, "Data", cell.Data2.Data));

            return data;
        }
        static public Cell Load(SaveTag data)
        {
            Cell cell = new Cell();

            //cell.Type = (Block.Types)data["Tile"].Value;
            cell.SetBlockType((Block.Types)data["Tile"].Value);

            //1
                //byte packed = (byte)data["Packed"].Value;
                //cell.Variation = (byte)((packed & 0xF0) >> 4);
                //cell.Orientation = (byte)(packed & 0x0F);
                //cell.Data2 = data.TagValueOrDefault<int>("Data", 0);
            //

            //2
                //data.TryGetTagValue<int>("Data", v => cell.Data2 = new BitVector32(v));
                //cell.SetVariation(cell.Data2);
                //data.TryGetTagValue<byte>("Variation", v => cell.Variation = v);
            //

            cell.Data2 = new BitVector32((int)data["Data"].Value);
            cell.SetVariation(cell.Data2);
            cell.Variation = (byte)data["Variation"].Value;
            return cell;
        }
        #endregion

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

        //static public bool UpdateEdges(Map map, Vector3 global, Edges horEdgesToCheck, VerticalEdges verEdgesToCheck)
        //{
        //    Cell cell;
        //    Chunk chunk;
        //    //if (map.Net is Net.Server)
        //    //    "gamw".ToConsole();
        //    if (!global.TryGetAll(map, out chunk, out cell))
        //        return false;
        //    Edges lastEdges = cell.HorizontalEdges;
        //    VerticalEdges lastVerticalEdges = cell.VerticalEdges;

        //    if ((horEdgesToCheck & Edges.West) == Edges.West)
        //    {
        //        Cell west;
        //        if ((global - new Vector3(1, 0, 0)).TryGetCell(map, out west))
        //        {
        //            if (!IsInvisible(west))
        //                cell.HorizontalEdges &= ~Edges.West;
        //            else
        //                cell.HorizontalEdges |= Edges.West;
        //        }
        //        else
        //            cell.HorizontalEdges &= ~Edges.West;
        //    }
        //    if ((horEdgesToCheck & Edges.North) == Edges.North)
        //    {
        //        Cell north;
        //        if ((global - new Vector3(0, 1, 0)).TryGetCell(map, out north))
        //        {
        //            if (!IsInvisible(north))
        //                cell.HorizontalEdges &= ~Edges.North;
        //            else
        //                cell.HorizontalEdges |= Edges.North;
        //        }
        //        else
        //            cell.HorizontalEdges &= ~Edges.North;
        //    }
        //    if ((horEdgesToCheck & Edges.South) == Edges.South)
        //    {
        //        Cell south;
        //        if ((global + new Vector3(0, 1, 0)).TryGetCell(map, out south))
        //        {
        //            if (!IsInvisible(south))
        //                cell.HorizontalEdges &= ~Edges.South;
        //            else
        //                cell.HorizontalEdges |= Edges.South;
        //        }
        //        else
        //            cell.HorizontalEdges &= ~Edges.South;
        //    }
        //    if ((horEdgesToCheck & Edges.East) == Edges.East)
        //    {
        //        Cell east;
        //        if ((global + new Vector3(1, 0, 0)).TryGetCell(map, out east))
        //        {
        //            if (!IsInvisible(east))
        //                cell.HorizontalEdges &= ~Edges.East;
        //            else
        //                cell.HorizontalEdges |= Edges.East;
        //        }
        //        else
        //            cell.HorizontalEdges &= ~Edges.East;
        //    }
        //    if ((verEdgesToCheck & VerticalEdges.Top) == VerticalEdges.Top)
        //    {
        //        Cell top;
        //        if ((global + new Vector3(0, 0, 1)).TryGetCell(map, out top))
        //        {
        //            if (!IsInvisible(top))
        //                cell.VerticalEdges &= ~VerticalEdges.Top;
        //            else
        //                cell.VerticalEdges |= VerticalEdges.Top;
        //        }
        //        else
        //            cell.VerticalEdges &= ~VerticalEdges.Top;
        //    }
        //    if ((verEdgesToCheck & VerticalEdges.Bottom) == VerticalEdges.Bottom)
        //    {
        //        Cell bottom;
        //        if ((global - new Vector3(0, 0, 1)).TryGetCell(map, out bottom))
        //        {
        //            if (!IsInvisible(bottom))
        //                cell.VerticalEdges &= ~VerticalEdges.Bottom;
        //            else
        //                cell.VerticalEdges |= VerticalEdges.Bottom;
        //        }
        //        else
        //            cell.VerticalEdges &= ~VerticalEdges.Bottom;
        //    }
        //    if (cell.VerticalEdges != lastVerticalEdges || cell.HorizontalEdges != lastEdges)
        //        //global.GetChunk(map).InvalidateLight(global);
        //        chunk.InvalidateLight(global);
        //    return true;
        //}
        //static public bool UpdateEdgesBoundaries(Map map, Vector3 global, Edges horEdgesToCheck, VerticalEdges verEdgesToCheck)
        //{
        //    Cell cell;
        //    Chunk chunk;
        //    //if (map.Net is Net.Server)
        //    //    "gamw".ToConsole();
        //    if (!global.TryGetAll(map, out chunk, out cell))
        //        return false;
        //    Edges lastEdges = cell.HorizontalEdges;
        //    VerticalEdges lastVerticalEdges = cell.VerticalEdges;

        //    if ((horEdgesToCheck & Edges.West) == Edges.West)
        //    {
        //        Cell west;
        //        if ((global - new Vector3(1, 0, 0)).TryGetCell(map, out west))
        //        {
        //            if (!IsInvisible(west))
        //                cell.HorizontalEdges &= ~Edges.West;
        //            else
        //                cell.HorizontalEdges |= Edges.West;
        //        }
        //        else
        //            cell.HorizontalEdges |= Edges.West;
        //    }
        //    if ((horEdgesToCheck & Edges.North) == Edges.North)
        //    {
        //        Cell north;
        //        if ((global - new Vector3(0, 1, 0)).TryGetCell(map, out north))
        //        {
        //            if (!IsInvisible(north))
        //                cell.HorizontalEdges &= ~Edges.North;
        //            else
        //                cell.HorizontalEdges |= Edges.North;
        //        }
        //        else
        //            cell.HorizontalEdges |= Edges.North;
        //    }
        //    if ((horEdgesToCheck & Edges.South) == Edges.South)
        //    {
        //        Cell south;
        //        if ((global + new Vector3(0, 1, 0)).TryGetCell(map, out south))
        //        {
        //            if (!IsInvisible(south))
        //                cell.HorizontalEdges &= ~Edges.South;
        //            else
        //                cell.HorizontalEdges |= Edges.South;
        //        }
        //        else
        //            cell.HorizontalEdges |= Edges.South;
        //    }
        //    if ((horEdgesToCheck & Edges.East) == Edges.East)
        //    {
        //        Cell east;
        //        if ((global + new Vector3(1, 0, 0)).TryGetCell(map, out east))
        //        {
        //            if (!IsInvisible(east))
        //                cell.HorizontalEdges &= ~Edges.East;
        //            else
        //                cell.HorizontalEdges |= Edges.East;
        //        }
        //        else
        //            cell.HorizontalEdges |= Edges.East;
        //    }
        //    if ((verEdgesToCheck & VerticalEdges.Top) == VerticalEdges.Top)
        //    {
        //        Cell top;
        //        if ((global + new Vector3(0, 0, 1)).TryGetCell(map, out top))
        //        {
        //            if (!IsInvisible(top))
        //                cell.VerticalEdges &= ~VerticalEdges.Top;
        //            else
        //                cell.VerticalEdges |= VerticalEdges.Top;
        //        }
        //        else
        //            cell.VerticalEdges |= VerticalEdges.Top;
        //    }
        //    if ((verEdgesToCheck & VerticalEdges.Bottom) == VerticalEdges.Bottom)
        //    {
        //        Cell bottom;
        //        if ((global - new Vector3(0, 0, 1)).TryGetCell(map, out bottom))
        //        {
        //            if (!IsInvisible(bottom))
        //                cell.VerticalEdges &= ~VerticalEdges.Bottom;
        //            else
        //                cell.VerticalEdges |= VerticalEdges.Bottom;
        //        }
        //        else
        //            cell.VerticalEdges |= VerticalEdges.Bottom;
        //    }
        //    if (cell.VerticalEdges != lastVerticalEdges || cell.HorizontalEdges != lastEdges)
        //        //global.GetChunk(map).InvalidateLight(global);
        //        chunk.InvalidateLight(global);
        //    return true;
        //}

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
            //writer.Write(Data.Data);
            writer.Write(Data2.Data);
            writer.Write(this.BlockData);
            //if (this.Variation > 0)
            //    if(this.Type == Block.Types.Farmland)
            //    "fuck".ToConsole();
         //   writer.Write(this.LocalCoords);
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
            //this.SetVariation(this.Data2);
            return this;
        }
        //static public Cell Create(BinaryReader reader)
        //{
        //    Cell cell = new Cell().Read(reader);
        //    //cell.Type = (Block.Types)reader.ReadByte();
        //    //cell.Data = new BitVector32(reader.ReadInt32());
        //    //cell.Data2 = new BitVector32(reader.ReadInt32());
        //    return cell;
        //}

        //static public bool operator ==(Cell c1, Cell c2)
        //{
        //    if (c1.X == c2.X)
        //        if (c1.Y == c2.Y)
        //            if (c1.Z == c2.Z)
        //                return true;
        //    return false;
        //}
        //static public bool operator !=(Cell c1, Cell c2)
        //{
        //    if (c1.X == c2.X)
        //        if (c1.Y == c2.Y)
        //            if (c1.Z == c2.Z)
        //                return false;
        //    return true;
        //}
    }
}
