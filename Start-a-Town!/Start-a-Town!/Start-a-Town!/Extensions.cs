using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.Blocks;
using Start_a_Town_.GameModes;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public static class Extensions
    {
        public static bool Insert(this List<GameObjectSlot> list, GameObjectSlot obj)
        {
            int capacity = 0;
            int stackMax = (int)obj.Object["Gui"]["StackMax"];
            foreach (var slot in list)
                capacity = slot.HasValue ? stackMax - slot.StackSize : stackMax;
            if (capacity < obj.StackSize)
                return false;
            foreach (var slot in list.FindAll(foo => foo.HasValue).FindAll(foo => foo.Object.ID == obj.Object.ID))
            {
                while (slot.StackSize < stackMax && obj.StackSize > 0)
                {
                    slot.StackSize += 1;
                    obj.StackSize -= 1;
                }
            }
            if (obj.StackSize > 0)
            {
                GameObjectSlot empty = list.FirstOrDefault(foo => !foo.HasValue);
                if (!empty.IsNull())
                    empty.Swap(obj);
            }
            return true;
        }
        public static bool Insert(this List<GameObjectSlot> list, GameObjectSlot obj, out List<GameObjectSlot> newList, out GameObjectSlot overflow)
        {
            overflow = obj.Clone();
            newList = new List<GameObjectSlot>();
          //  list.ForEach(foo => newList.Add(foo.Clone()));
            foreach (var slot in list)
                newList.Add(slot.Clone());
            int stackMax = (int)obj.Object["Gui"]["StackMax"];
            foreach (var slot in newList.FindAll(foo => foo.HasValue).FindAll(foo => foo.Object.ID == obj.Object.ID))
            {
                while (slot.StackSize < stackMax && overflow.StackSize > 0)
                {
                    slot.StackSize += 1;
                    overflow.StackSize -= 1;
                }
            }
            if (overflow.StackSize > 0)
            {
                GameObjectSlot empty = newList.FirstOrDefault(foo => !foo.HasValue);
                if (!empty.IsNull())
                    empty.Swap(overflow);
            }
                //foreach (var slot in newList.FindAll(foo => !foo.HasValue))
                //{
                //}
            return overflow.StackSize == 0;
        }

        public static void Remove(this List<GameObjectSlot> list, GameObject.Types objID, int amount)
        {
            Queue<GameObjectSlot> slots = new Queue<GameObjectSlot>(from obj in list
                                                                       where obj.HasValue
                                                                       where obj.Object.ID == objID
                                                                       select obj);
            while (amount > 0)
            {
                var slot = slots.Peek();
                slot.StackSize--;
                amount--;
                if (slot.StackSize == 0)
                    slots.Dequeue();
            }
        }

        //public static int Count(this List<GameObjectSlot> list, Func<GameObjectSlot, bool> condition)
        //{
        //    int amount = 0;
        //    (from slot in list
        //     where condition(slot)
        //     select slot)
        //     .ToList()
        //     .ForEach(slot => amount += slot.StackSize);
        //    return amount;
        //}
        public static int GetAmount(this List<GameObjectSlot> list, Func<GameObject, bool> condition)
        {
            int amount = 0;
            (from slot in list
             where slot.HasValue
             where condition(slot.Object)
             select slot)
             .ToList()
             .ForEach(slot => amount += slot.StackSize);
            return amount;
        }

        public static Vector2 ToVector(this Point point)
        { return new Vector2(point.X, point.Y); }

        //public static BlockComponent GetObject(this Block.Types block)
        //{
        //    return BlockComponent.Blocks[block]["Physics"] as BlockComponent;
        //}

        public static bool IsNull(this object obj) { return obj == null; }
        public static float GetMouseoverDepth(this Vector3 worldGlobal, IMap map, Camera camera)
        {
            //Vector3 local = worldGlobal - new Vector3(map.Global, 0);
            Vector3 local = worldGlobal - new Vector3(map.GetOffset(), 0);
            Vector3 rotated = local.Rotate(camera);
            return (rotated.X + rotated.Y + worldGlobal.Z);
        }
        
        public static float GetDrawDepth(this Vector3 worldGlobal, IMap map, Camera camera)
        {
            //return (worldGlobal.X + worldGlobal.Y);// +1;
            Vector3 local = worldGlobal - new Vector3(map.GetOffset(), 0);
            Vector3 rotated = local.Rotate(camera);// Vector3.Transform(local, camera.RotationMatrix);
            return (rotated.X + rotated.Y);// +1;
            //float mapX = worldGlobal.X - map.Global.X, mapY = worldGlobal.Y - map.Global.Y;
            float gd = rotated.X + rotated.Y + 1; //+1 to compensate for (0,0) being at the center of the block

            Vector2 furthest = Vector2.Zero;// -Vector2.One * 0.5f;//.GetRotated(camera)*0.5f;
            Vector2 nearest = new Vector2(Chunk.Size);// + 0.5f);//.GetRotated(camera);

            //float far = -1;
            //float near = Chunk.Size * 2 + 1;

            float far = furthest.X + furthest.Y;
            float near = nearest.X + nearest.Y;

            float range = near - far;
            float d = (gd - far) / range;
            return d;// 1 - d;
        }
        public static Vector3 Rotate(this Vector3 pos, double quarters)
        {
            double rotCos = Math.Cos((Math.PI / 2f) * quarters);
            double rotSin = Math.Sin((Math.PI / 2f) * quarters);

            rotCos = Math.Round(rotCos + rotCos) / 2f;
            rotSin = Math.Round(rotSin + rotSin) / 2f;
            return new Vector3((float)(pos.X * rotCos - pos.Y * rotSin), (float)(pos.X * rotSin + pos.Y * rotCos), pos.Z);

            //Matrix matrix = Matrix.CreateRotationZ((float)(quarters * Math.PI / 2f));
            //return Vector3.Transform(pos, matrix);
        }
        public static Vector3 Rotate(this Vector3 pos, Camera camera)
        {
            return new Vector3((
                float)(pos.X * camera.RotCos - pos.Y * camera.RotSin), 
                (float)(pos.X * camera.RotSin + pos.Y * camera.RotCos), 
                pos.Z);
        }
        public static Vector2 Rotate(this Vector2 pos, Camera camera)
        {
            return new Vector2((float)(pos.X * camera.RotCos - pos.Y * camera.RotSin), (float)(pos.X * camera.RotSin + pos.Y * camera.RotCos));
        }
        public static bool IsObstructed(this Vector3 global, Map map)
        {
            foreach (var obj in map.GetObjects())
                if (obj.Global.RoundXY() == global)
                    return true;
            return false;
        }
        [Obsolete]
        public static bool TryPlaceBlock(this Vector3 global, IObjectProvider net, Block.Types type, byte data = 0)
        {
            //BlockComponent.Blocks[type].OnPlaced(net, BlockComponent.Blocks[type].Entity.SetGlobal(global)); // possible problem with race condition for setglobal (since the template block object is a singleton)
            return true;
        }
        public static bool TryRemoveBlock(this Vector3 global, IObjectProvider net)
        {
            Block.Types oldBlock;
            //BlockComponent.Blocks[global.GetCell(map).Type].OnRemoved(map, global);
            //return true;
            return global.TryRemoveBlock(net, out oldBlock);
      //      return global.TrySetCell(map, Block.Types.Air);
            //Block.Types oldBlock;
            //if (!global.TrySetCell(map, Block.Types.Air, 0, out oldBlock))
            //    return false;
            //BlockComponent.Blocks[oldBlock].OnRemoved(map, global);
            //return true;
        }
        //public static bool TryRemoveBlock(this Vector3 global, Map map, out Block.Types oldBlock)
        //{
        //    oldBlock = global.GetCell(map).Type;
        //    BlockComponent.Blocks[oldBlock].OnRemoved(map, global);
        //    return true;
        //}
        public static bool TryRemoveBlock(this Vector3 global, IObjectProvider net, out Block.Types oldBlock)
        {
            var map = net.Map;
            //oldBlock = global.GetCell(map).Block.Type;
            oldBlock = map.GetCell(global).Block.Type;

            //BlockComponent.Blocks[oldBlock].OnDespawn(net, global);
            return true;
        }
        /// <summary>
        /// returns the old cell data
        /// </summary>
        /// <param name="global"></param>
        /// <param name="map"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte SetData(this Vector3 global, Map map, byte data = 0)
        {
            Cell cell = global.GetCell(map);
            byte old = cell.BlockData;
            cell.BlockData = data;
            return old;
        }
        //public static bool TrySetLuminance(this Vector3 global, IObjectProvider net, byte luminance = 0)
        //{
        //    global = global.Round();
        //    Cell cell;// = global.GetCell(net.Map);
        //    Chunk chunk;
        //    //if (!global.TryGetAll(net.Map, out chunk, out cell))
        //    if (!net.Map.TryGetAll(global, out chunk, out cell))
        //        return false;
        //    cell.Luminance = luminance;
        //    //chunk.Invalidate();//.Saved = false;
        //    net.Map.InvalidateCell(global);
        //    //new LightingEngine(net.Map).HandleBatchSync(new Vector3[] { global });
        //    //net.SpreadBlockLight(global);
        //    return true;
        //}
        //public static bool TrySetCell(this Vector3 global, Map map, Block.Types type, byte data = 0, int variation = 0, int orientation = 0)
        //{
        //    CellOperation undoOp;
        //    return global.TrySetCell(map, type, data, variation, orientation, out undoOp);
        //}
        //public static bool TrySetCell(this Vector3 global, Map map, Block.Types type, byte data, out Block.Types oldBlock)
        //{
        //    CellOperation undoOp;
        //    return global.TrySetCell(map, type, data, 0, 0, out oldBlock, out undoOp);
        //}
        //public static bool TrySetCell(this Vector3 global, Map map, Block.Types type, byte data, int variation, int orientation, out CellOperation undoOperation)
        //{
        //    Block.Types oldBlock;
        //    return global.TrySetCell(map, type, data, variation, orientation, out oldBlock, out undoOperation);
        //}
        //public static bool TrySetCell(this Vector3 global, Map map, Block.Types type, byte data, int variation, int orientation, out Block.Types oldBlock, out CellOperation undoOperation)
        //{
        //    Cell cell = global.GetCell(map);
        //    if (cell.IsNull())
        //    {
        //        undoOperation = null;
        //        oldBlock = 0;
        //        return false;
        //    }
        //    Chunk chunk = global.GetChunk(map);
        //    undoOperation = new CellOperation(map, global, cell.Type, cell.Variation, cell.Orientation);
        //    oldBlock = cell.Type;
        //    cell.Type = type;
        //    cell.BlockData = data;
        //    if (Cell.IsInvisible(cell))
        //        Chunk.Hide(chunk, cell);
        //    cell.Variation = variation;
        //    cell.Orientation = orientation;

        //    // TODO: fix lighting when doing rapid block changes, or is it the Chunk.Hide() ?
        //    //ConstructionComponent.RemoveDesignatedConstruction(global); // removed this until i find a way to pass the iobjectprovider
 
        //    var q = chunk.ResetHeightMapColumn(global.ToLocal());
        //    q.Enqueue(global);
        //    ChunkLighter.Enqueue(q);

        //    //Cell c = global.GetCell(map);
        //    //c.ToConsole();
        //    chunk.Changed = true;
        //    return true;
        //}

        //public static bool IsDrawable(this Vector3 global, Map map)
        //{
        //    Cell cell = global.GetCell(map);
        //    return cell.IsDrawable();
        //}

        public static bool TrySetCell(this Vector3 global, IObjectProvider net, Block.Types type)
        {
            CellOperation undoOp;
            return global.TrySetCell(net, type, global.GetData(net), 0, 0, out undoOp);
        }
        public static bool TrySetCell(this Vector3 global, IObjectProvider net, Block.Types type, byte data, int variation = 0, int orientation = 0)
        {
            CellOperation undoOp;
            return global.TrySetCell(net, type, data, variation, orientation, out undoOp);
        }
        public static bool TrySetCell(this Vector3 global, IObjectProvider net, Block.Types type, byte data, out Block.Types oldBlock)
        {
            CellOperation undoOp;
            return global.TrySetCell(net, type, data, 0, 0, out oldBlock, out undoOp);
        }
        public static bool TrySetCell(this Vector3 global, IObjectProvider net, Block.Types type, byte data, int variation, int orientation, out CellOperation undoOperation)
        {
            Block.Types oldBlock;
            return global.TrySetCell(net, type, data, variation, orientation, out oldBlock, out undoOperation);
        }
        public static bool TrySetCell(this Vector3 global, IObjectProvider net, Block.Types type, byte data, int variation, int orientation, out Block.Types oldBlock, out CellOperation undoOperation)
        {
            //Cell cell = global.GetCell(net.Map);
            Cell cell = net.Map.GetCell(global);

            if (cell.IsNull())
            {
                undoOperation = null;
                oldBlock = 0;
                return false;
            }
            //Chunk chunk = global.GetChunk(net.Map);

            Chunk chunk = net.Map.GetChunk(global);
            undoOperation = new CellOperation(net, global, cell.Block.Type, cell.Variation, cell.Orientation);
            oldBlock = cell.Block.Type;

            //cell.Type = type;
            cell.SetBlockType(type);
            if (type != oldBlock)
                chunk.BlockEntities.Remove(cell.LocalCoords);
            Block block = Block.Registry[type];
            var blockentity = block.GetBlockEntity();
            if (blockentity != null)
                chunk.BlockEntities[cell.LocalCoords] = blockentity;

            //cell.BlockData = data;
            //var wtf = global.GetData(net);
            //if (Cell.IsInvisible(cell))
            //    Chunk.Hide(chunk, cell);
            //if (type == Block.Types.Air)
            //    chunk.HideCell(cell);
            //chunk.ToggleCell(cell);
            cell.Variation = (byte)variation;
            cell.Orientation = orientation;
            cell.BlockData = data;

            if (Block.Registry[oldBlock].Opaque != Block.Registry[type].Opaque)
                net.UpdateLight(global);
            chunk.InvalidateCell(cell);
            chunk.Invalidate();//.Saved = false;
            net.EventOccured(Message.Types.BlockChanged, chunk.Map, global);
            return true;
        }
        public static byte GetData(this Vector3 global, IObjectProvider net)
        {
            return GetData(global, net.Map);
        }
        public static byte GetData(this Vector3 global, IMap map)
        {
            Cell cell;
            //return global.TryGetCell(map, out cell) ? cell.BlockData : (byte)0;
            return map.TryGetCell(global, out cell) ? cell.BlockData : (byte)0;
        }
        public static Cell GetCell(this Vector3 global, Map map)
        {
            return Position.GetCell(map, global);
        }
        public static bool TryGetCell(this Vector3 global, Map map, out Cell cell)
        {
            Chunk chunk;
            return global.TryGetAll(map, out chunk, out cell);
        }
        public static bool TryGetAll(this Vector3 global, Map map, out Chunk chunk, out Cell cell)
        {
            cell = null;
            chunk = null;
            if (map == null)
                return false;
            Vector3 rounded = global.RoundXY();
            if (rounded.Z < 0 || rounded.Z > map.World.MaxHeight - 1)
                return false;
            int chunkX = (int)Math.Floor(rounded.X / Chunk.Size);
            int chunkY = (int)Math.Floor(rounded.Y / Chunk.Size);
            //if (ChunkLoader.TryGetChunk(map, new Vector2(chunkX, chunkY), out chunk))
            if (map.ActiveChunks.TryGetValue(new Vector2(chunkX, chunkY), out chunk))
            {
                cell = chunk[(int)(rounded.X - chunk.Start.X), (int)(rounded.Y - chunk.Start.Y), (int)rounded.Z];
                return true;
            }
            return false;
        }
        public static bool TryGetChunk(this Vector3 global, Map map, out Chunk chunk)
        {
            chunk = Position.GetChunk(map, global);
            return chunk != null;
        }
        public static Chunk GetChunk(this Vector3 global, Map map)
        {
            return Position.GetChunk(map, global);
        }
        public static Vector2 GetChunkCoords(this Vector3 global)
        {
            int chunkX = (int)Math.Floor(Math.Round(global.X) / Chunk.Size);
            int chunkY = (int)Math.Floor(Math.Round(global.Y) / Chunk.Size);
            return new Vector2(chunkX, chunkY);
        }
        //public static bool TryGetAll(this Vector3 global, Map map, out Chunk chunk, out Cell cell)
        //{
        //    chunk = null;
        //    if (TryGetCell(global, map, out cell))
        //    {
        //        chunk = global.GetChunk(map);
        //        return true;
        //    }
        //    return false;
        //}
        public static byte GetSunLight(this Vector3 global, Map map)
        {
            byte sunlight;
            Chunk.TryGetSunlight(map, global, out sunlight);
            return sunlight;
        }
        public static byte GetBlockLight(this Vector3 global, Map map)
        {
            byte blocklight;
            Chunk.TryGetBlocklight(map, global, out blocklight);
            return blocklight;
        }
        public static bool GetLight(this Vector3 global, Map map, out byte sky, out byte block)
        {
            return Chunk.TryGetFinalLight(map, (int)global.X, (int)global.Y, (int)global.Z, out sky, out block);
        }
        public static void SetLight(this Vector3 global, Map map, byte sky, byte block)
        {
            Chunk ch = global.GetChunk(map);
            if (ch.IsNull())
                return;
            Vector3 loc = global.ToLocal();
            ch.SetSunlight(loc, sky);
            ch.SetBlockLight(loc, block);
            ch.InvalidateLight(global);
            return;
        }
        public static void SetBlockLight(this Vector3 global, Map map, byte blockLight)
        {
            Chunk ch = global.GetChunk(map);
            if (ch.IsNull())
                return;
            Vector3 loc = global.ToLocal();
            ch.SetBlockLight(loc, blockLight);
            return;
        }
        public static Vector2 Floor(this Vector2 vector)
        {
            vector.X = (int)Math.Floor(vector.X);
            vector.Y = (int)Math.Floor(vector.Y);
            return vector;
        }
        public static Vector3 Floor(this Vector3 vector)
        {
            vector.X = (int)Math.Floor(vector.X);
            vector.Y = (int)Math.Floor(vector.Y);
            vector.Z = (int)Math.Floor(vector.Z);
            return vector;
        }
        public static Vector2 Round(this Vector2 vector)
        {
            //vector.X = (int)vector.X; vector.Y = (int)vector.Y;
            vector.X = (int)Math.Round(vector.X);
            vector.Y = (int)Math.Round(vector.Y);
            return vector;
        }
        public static Vector2 Round(this Vector2 vector, int decimalpoints)
        {
            //vector.X = (int)vector.X; vector.Y = (int)vector.Y;
            vector.X = (float)Math.Round(vector.X, decimalpoints);
            vector.Y = (float)Math.Round(vector.Y, decimalpoints);
            return vector;
        }
        public static Vector3 RoundXY(this Vector3 vector)
        {
            vector.X = (int)Math.Round(vector.X);
            vector.Y = (int)Math.Round(vector.Y);
            vector.Z = (int)vector.Z;
            return vector;
        }
        public static Vector3 FloorXY(this Vector3 vector)
        {
            vector.X = (int)Math.Floor(vector.X);
            vector.Y = (int)Math.Floor(vector.Y);
            vector.Z = (int)vector.Z;
            return vector;
        }
        public static Vector3 Round(this Vector3 vector)
        {
            vector.X = (int)Math.Round(vector.X);
            vector.Y = (int)Math.Round(vector.Y);
            vector.Z = (int)Math.Round(vector.Z);
            return vector;
        }
        public static Vector3 Round(this Vector3 vector, int decimalPoints)
        {
            //var a = (int)Math.Pow(10, decimalPoints);
            //vector *= a;
            //vector = vector.Round();
            //vector /= a;
            vector.X = (float)Math.Round(vector.X, decimalPoints);
            vector.Y = (float)Math.Round(vector.Y, decimalPoints);
            vector.Z = (float)Math.Round(vector.Z, decimalPoints);
            return vector;
        }
        public static Vector3 Normalized(this Vector3 vector)
        {
            vector.Normalize();
            return vector;
        }
        public static Vector3 DirectionTo(this Vector3 vector, Vector3 target)
        {
            var dir = (target - vector).Normalized();
            return dir;
        }
        public static Vector3 GetCellCoords(this Vector3 vector)
        {
            return vector.RoundXY();
        }

        public static Vector2 XY(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }
        public static Vector2 YZ(this Vector3 vector)
        {
            return new Vector2(vector.Y, vector.Z);
        }
        public static Vector2 XZ(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Z);
        }

        public static Vector3 ToLocal(this Vector3 global)
        {
            float rx, ry;
            rx = global.X % Chunk.Size;
            rx = rx < 0 ? rx + Chunk.Size : rx;
            ry = global.Y % Chunk.Size;
            ry = ry < 0 ? ry + Chunk.Size : ry;
            return new Vector3(rx, ry, global.Z);
        }
        public static void ToLocal(this Vector3 global, out float x, out float y, out float z)
        {
            float rx, ry;
            x = global.X % Chunk.Size;
            x = x < 0 ? x + Chunk.Size : x;
            y = global.Y % Chunk.Size;
            y = y < 0 ? y + Chunk.Size : y;
            z = global.Z;
        }
        static public Vector3 ToGlobal(this Vector3 local, Chunk chunk)
        {
            return new Vector3(chunk.Start.X + local.X, chunk.Start.Y + local.Y, local.Z);
        }
        static public Vector3 ToGlobal(this Vector3 local, Vector2 chunkCoords)
        {
            //return new Vector3(chunkCoords.X + local.X, chunkCoords.Y + local.Y, local.Z);
            return new Vector3(chunkCoords.X * Chunk.Size + local.X, chunkCoords.Y * Chunk.Size + local.Y, local.Z);
        }
        static public Vector3 ToBlock(this Vector3 global)
        {
            global = global + 0.5f * new Vector3(1, 1, 0); // shouldnt it be -0.5f?
            global = global - global.FloorXY();// -global.RoundXY();
            return global;
        }
        //public static bool IsSolid(this Vector3 global, Map map)// IObjectProvider net)
        //{
        //    Cell cell;
        //    //global = global.Round();
        //    if (!global.TryGetCell(map, out cell))
        //        return true; // return true to prevent crashing by trying to add object to missing chunk
        //        //return false; // return false to let entity attempt to enter unloaded chunk so we can handle the event of that

        //    return cell.Block.IsSolid(map, global);//.Solid;
        //}
        //public static BlockEntity GetBlockEntity(this Vector3 global, Map map)// IObjectProvider net)
        //{
        //    Chunk chunk = global.GetChunk(map);
        //    BlockEntity entity;
        //    chunk.BlockEntities.TryGetValue(global.ToLocal(), out entity);
        //    return entity;
        //}
        public static bool TryGetBlockEntity(this Vector3 global, Map map, out BlockEntity entity)// IObjectProvider net)
        {
            Chunk chunk = global.GetChunk(map);
            chunk.BlockEntities.TryGetValue(global.ToLocal(), out entity);
            return entity!=null;
        }
        public static Block GetBlock(this Vector3 global, Map map)// IObjectProvider net)
        {
            Cell cell;
            if (!global.TryGetCell(map, out cell))
                //throw new Exception("Block doesn't exist");
                return null; 
            return cell.Block;//.Solid;
        }
        public static bool TryGetBlock(this Vector3 global, Map map, Action<Block, Cell> action)// IObjectProvider net)
        {
            Cell cell;
            if (!global.TryGetCell(map, out cell))
                //throw new Exception("Block doesn't exist");
                return false;
            action(cell.Block, cell);
            return true;
        }
        public static DialogueOption ToDialogueOption(this string text, DialogueOptionHandler handler) { return new DialogueOption(text, handler); }
        public static DialogueOption ToDialogueOption(this string text, DialogueOptionHandler handler, Func<bool> condition) { return new DialogueOption(text, handler, condition); }

        public static UI.Label ToLabel(this string text) { return new UI.Label(Vector2.Zero, text); }
        public static UI.Label ToLabel(this string text, int width) { return new UI.Label(Vector2.Zero, text) { Width = width }; }
        public static UI.Label ToLabel(this string text, Vector2 location) { return new UI.Label(location, text); }
        public static UI.Label ToLabel(this string text, Vector2 location, int width) { return new UI.Label(location, text) { Width = width }; }

        public static int MaxWidth(this IEnumerable<string> strings, SpriteFont font)
        {
            var max = 0;
            foreach(var txt in strings)
                max = Math.Max(max, (int)font.MeasureString(txt).X);
            return max;
        }

        public static List<SaveTag> Save(this Vector3 pos)
        {
            return new List<SaveTag>() { 
                new SaveTag(Start_a_Town_.SaveTag.Types.Float, "X", pos.X), 
                new SaveTag(Start_a_Town_.SaveTag.Types.Float, "Y", pos.Y), 
                new SaveTag(Start_a_Town_.SaveTag.Types.Float, "Z", pos.Z), 
            };
        }
        public static SaveTag Save(this Vector3 pos, string name)
        {
            return new SaveTag(SaveTag.Types.Compound, name, new List<SaveTag>() { 
                new SaveTag(Start_a_Town_.SaveTag.Types.Float, "X", pos.X), 
                new SaveTag(Start_a_Town_.SaveTag.Types.Float, "Y", pos.Y), 
                new SaveTag(Start_a_Town_.SaveTag.Types.Float, "Z", pos.Z), 
            });
        }
        public static SaveTag Save(this Vector2 pos, string name)
        {
            return new SaveTag(SaveTag.Types.Compound, name, new List<SaveTag>() { 
                new SaveTag(Start_a_Town_.SaveTag.Types.Float, "X", pos.X), 
                new SaveTag(Start_a_Town_.SaveTag.Types.Float, "Y", pos.Y)
            });
        }
        public static SaveTag Save(this int value, string name)
        {
            return new SaveTag(SaveTag.Types.Int, name, value);
        }
        public static SaveTag Save(this string value, string name)
        {
            return new SaveTag(SaveTag.Types.String, name, value);
        }
        public static SaveTag Save(this float value, string name)
        {
            return new SaveTag(SaveTag.Types.Float, name, value);
        }
        public static SaveTag Save(this bool value, string name)
        {
            return new SaveTag(SaveTag.Types.Bool, name, value);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue ret;
            // Ignore return value
            dictionary.TryGetValue(key, out ret);
            return ret;
        }
        public static void ToConsole(this object obj)
        { Console.WriteLine(obj.ToString()); }
        public static Time ToTime(this DateTime dateTime) { return new Time(dateTime); }
        public static string ToLocalTime(this DateTime dateTime)
        { return dateTime.ToString("MMM dd, HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-GB")); }

        public static Rectangle ToRectangle(this Vector4 bounds)
        {
            return new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Z, (int)bounds.W);
        }
        public static bool Intersects(this Vector4 bounds, Vector2 position)
        {
            return (bounds.X <= position.X &&
                position.X < bounds.X + bounds.Z &&
                bounds.Y <= position.Y &&
                position.Y < bounds.Y + bounds.W);
        }

        public static Vector4 ToVector4(this Rectangle rect)
        {
            return new Vector4(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
        public static void Clip(this Rectangle bounds, Rectangle source, Rectangle viewport, out Rectangle finalBounds, out Rectangle finalSource)
        {
            finalBounds = Rectangle.Intersect(bounds, viewport);
            finalSource =
                new Rectangle(
                    source.X + finalBounds.X - bounds.X,
                    source.Y + finalBounds.Y - bounds.Y,
                    source.Width - (bounds.Width - finalBounds.Width),
                    source.Height - (bounds.Height - finalBounds.Height)
                    );
            //finalSource = new Rectangle(source.X + finalBounds.X - bounds.X, source.Y + finalBounds.Y - bounds.Y, source.Width, source.Height);
        }

        public static void DrawHighlight(this Rectangle bounds, Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Vector2 origin, float rotation, float alpha = 0.5f)
        {
            //sb.Draw(UI.UIManager.Highlight, bounds, null, Color.Lerp(Color.Transparent, Color.White, alpha), rotation, origin, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
            bounds.DrawHighlight(sb, Color.Lerp(Color.Transparent, Color.White, alpha), origin, rotation);
        }
        public static void DrawHighlight(this Rectangle bounds, Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Color color, Vector2 origin, float rotation)
        {
            sb.Draw(UI.UIManager.Highlight, bounds, null, color, rotation, origin, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
        }
        public static void DrawHighlight(this Rectangle bounds, Microsoft.Xna.Framework.Graphics.SpriteBatch sb, float alpha = 0.5f)
        {
            bounds.DrawHighlight(sb, Vector2.Zero, 0, alpha);
            //sb.Draw(UI.UIManager.Highlight, bounds, null, Color.Lerp(Color.Transparent, Color.White, alpha), 0, Vector2.Zero, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
        }
        public static void DrawHighlight(this Rectangle bounds, Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Color color)
        {
            sb.Draw(UI.UIManager.Highlight, bounds, null, color, 0, Vector2.Zero, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
        }
        #region IO
        public static byte[] GetBytes(this Vector2 vector)
        {
            byte[] data = 
                BitConverter.GetBytes(vector.X)
                .Concat(BitConverter.GetBytes(vector.Y))
                .ToArray();
            return data;
        }
        public static byte[] GetBytes(this Vector3 vector)
        {
            byte[] data = 
                BitConverter.GetBytes(vector.X)
                .Concat(BitConverter.GetBytes(vector.Y))
                .Concat(BitConverter.GetBytes(vector.Z))
                .ToArray();
            return data;
        }
        public static void Write(this BinaryWriter writer, Vector2 vec2)
        {
            writer.Write(vec2.X);
            writer.Write(vec2.Y);
        }
        public static void Write(this BinaryWriter writer, Vector3 vec3)
        {
            writer.Write(vec3.X);
            writer.Write(vec3.Y);
            writer.Write(vec3.Z);
        }
        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }
        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        public static string ReadASCII(this BinaryReader reader)
        {
            return Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadInt32()));
        }
        public static void WriteASCII(this BinaryWriter writer, string text)
        {
            byte[] encoded = Encoding.ASCII.GetBytes(text);
            writer.Write(encoded.Length);
            writer.Write(encoded);
        }
        #endregion

        #region Xml
        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            using (var reader = xDocument.CreateReader())
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(reader);
                return xml;
            }
        }
        #endregion

        public static T Translate<T>(this object[] data, IObjectProvider objProvider) where T : PacketTranslator, new()
        {
            return new T().Translate(objProvider, data) as T;
        }

        #region Network
        public static void Send(this byte[] data, long packetID, PacketType type, Socket so, EndPoint ip)
        {
            Packet.Create(packetID, type, data).BeginSendTo(so, ip);
        }
        public static T Translate<T>(this byte[] data, IObjectProvider objProvider) where T : PacketTranslator, new()
        {
            return new T().Translate(objProvider, data) as T;
        }
        public static void Translate(this byte[] data, IObjectProvider objProvider, Action<BinaryReader> reader)
        {
            using (BinaryReader r = new BinaryReader(new MemoryStream(data)))
                reader(r);
        }
        public static void Translate(this byte[] data, Action<BinaryReader> reader)
        {
            using (BinaryReader r = new BinaryReader(new MemoryStream(data)))
                reader(r);
        }
        public static T Deserialize<T>(this byte[] data, Func<BinaryReader, T> reader)
        {
            return Network.Deserialize<T>(data, reader);
        }
        public static void Deserialize(this byte[] data, Action<BinaryReader> reader)
        {
            Network.Deserialize(data, reader);
        }
        public static byte[] GetBytes(this Action<BinaryWriter> writer)
        {
            using (BinaryWriter w = new BinaryWriter(new MemoryStream()))
            {
                writer(w);
                return (w.BaseStream as MemoryStream).ToArray();
            }
        }
        public static byte[] Decompress(this byte[] compressed)
        {
            using (var compressedStream = new MemoryStream(compressed))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                zipStream.CopyTo(output);
                return output.ToArray();
            }
        }
        public static byte[] Compress(this byte[] data)
        {
            //using (var decompressedStream = new MemoryStream(decompressed))
            //using (var zipStream = new GZipStream(decompressedStream, CompressionMode.Compress))
            //using (var output = new MemoryStream())
            //{
            //    zipStream.CopyTo(output);
            //    return output.ToArray();
            //}

            byte[] compressed;
            using (var output = new MemoryStream())
            {
                using (var input = new MemoryStream(data))
                using (var zip = new GZipStream(output, CompressionMode.Compress))
                    input.CopyTo(zip);
                compressed = output.ToArray();
            }
            return compressed;

            //byte[] data;
            //using (MemoryStream mem = new MemoryStream())
            //{
            //    using (BinaryWriter bin = new BinaryWriter(mem))
            //    {
            //        dataGetter(bin);

            //        using (MemoryStream outStream = new MemoryStream())
            //        {
            //            using (GZipStream zip = new GZipStream(outStream, CompressionMode.Compress))
            //            {
            //                mem.Position = 0;
            //                mem.CopyTo(zip);
            //            }
            //            data =  outStream.ToArray();
            //        }
            //    }
            //}
            //return data;
        
        }


        //public static Queue<Packet> GetPackets(this byte[] buffer, int bytesRead)
        //{
        //    return Packet.ReadBuffer(buffer, bytesRead);
        //}

        //public static Queue<Packet> GetPackets(this byte[] buffer, Packet previousPartial, out Packet packet)
        //{
        //    return Packet.ReadBuffer(buffer, previousPartial, out packet);
        //}
        #endregion

        #region Game Components
        static public Components.Needs.Need CreateNew(this Components.Needs.Need.Types id, float value = 100f, float decay = .1f, float tolerance = 50f)
        {
            return Components.Needs.Need.Factory.Create(id, value, decay, tolerance);
        }
        #endregion

        public static List<T> Randomize<T>(this IEnumerable<T> list, RandomThreaded random)
        {
            var unhandled = list.ToList();
            var randomized = new Queue<T>();
            while (unhandled.Count > 0)
            {
                var current = unhandled[random.Next(unhandled.Count)];
                unhandled.Remove(current);
                randomized.Enqueue(current);
            }
            return randomized.ToList();
        }

        public static void Write(this BinaryWriter w, List<Vector3> list)
        {
            w.Write(list.Count);
            foreach (var g in list)
                w.Write(g);
        }
        public static List<Vector3> ReadListVector3(this BinaryReader r)
        {
            var list = new List<Vector3>();
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(r.ReadVector3());
            return list;    
        }
        public static SaveTag Save(this List<Vector3> vectors, string name)
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Vector3);
            foreach (var pos in vectors)
                list.Add(new SaveTag(SaveTag.Types.Vector3, "", pos));
            return list;
        }
       
        public static List<Vector3> Load(this List<Vector3> list, List<SaveTag> positions)
        {
            foreach (var pos in positions)
                list.Add((Vector3)pos.Value);
            return list;
        }

        public static SaveTag Save(this IEnumerable<int> ints, string name)
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Int);
            foreach (var item in ints)
                list.Add(new SaveTag(SaveTag.Types.Int, "", item));
            return list;
        }
        public static List<int> Load(this List<int> list, List<SaveTag> positions)
        {
            foreach (var pos in positions)
                list.Add((int)pos.Value);
            return list;
        }

        public static SaveTag Save(this IEnumerable<string> strings, string name)
        {
            var list = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.String);
            foreach (var item in strings)
                list.Add(new SaveTag(SaveTag.Types.String, "", item));
            return list;
        }
        public static List<string> Load(this List<string> list, List<SaveTag> strings)
        {
            foreach (var s in strings)
                list.Add((string)s.Value);
            return list;
        }
        public static void Write(this BinaryWriter w, List<string> strings)
        {
            w.Write(strings.Count);
            foreach (var s in strings)
                w.Write(s);
        }
        public static List<string> ReadListString(this BinaryReader r)
        {
            List<string> list = new List<string>();
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                list.Add(r.ReadString());
            }
            return list;
        }

        public static void Write(this BinaryWriter w, List<int> items)
        {
            w.Write(items.Count);
            foreach (var i in items)
                w.Write(i);
        }
        public static List<int> ReadListInt(this BinaryReader r)
        {
            List<int> list = new List<int>();
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                list.Add(r.ReadInt32());
            }
            return list;
        }

        //public static List<Vector3> Load(this List<Vector3> list, SaveTag tag, string name)
        //{
        //    List<SaveTag> positions;
        //    if (tag.TryGetTagValue(name, out positions))
        //        foreach (var pos in positions)
        //            list.Add((Vector3)pos.Value);
        //    return list;
        //}
        public static List<Vector2> GetSpiral(this Vector2 center, int radius = Engine.ChunkRadius)
        {
            List<Vector2> list = new List<Vector2>();


            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    Vector2 vec2 = new Vector2(i, j);
                    if (vec2.Length() <= radius)
                        list.Add(center + vec2);
                }
            list.Sort((u, v) =>// (Vector2.DistanceSquared(center, u) <= Vector2.DistanceSquared(center, v) ? -1 : 1));
                {
                    if (u == v) return 0;
                    else if(Vector2.DistanceSquared(center, u) < Vector2.DistanceSquared(center, v)) return -1;
                    else return 1;
                });
            return list;
            //return list.OrderBy(foo => Vector2.DistanceSquared(center, foo)).ToList();
        }
        //static public List<Vector3> GetNeighbors(this Vector3 global)
        //{
        //    List<Vector3> neighbors = new List<Vector3>(6);
        //    neighbors.Add(global + new Vector3(1, 0, 0));
        //    neighbors.Add(global - new Vector3(1, 0, 0));
        //    neighbors.Add(global + new Vector3(0, 1, 0));
        //    neighbors.Add(global - new Vector3(0, 1, 0));
        //    neighbors.Add(global + new Vector3(0, 0, 1));
        //    neighbors.Add(global - new Vector3(0, 0, 1));
        //    return neighbors;
        //}

        static public List<Vector3> GetBox(this Vector3 begin, Vector3 end)
        {
            List<Vector3> list = new List<Vector3>();
            var xmin = Math.Min(begin.X, end.X);
            var ymin = Math.Min(begin.Y, end.Y);
            var zmin = Math.Min(begin.Z, end.Z);
            var xmax = begin.X + end.X - xmin;
            var ymax = begin.Y + end.Y - ymin;
            var zmax = begin.Z + end.Z - zmin;
            var dx = xmax - xmin + 1;
            var dy = ymax - ymin + 1;
            var dz = zmax - zmin + 1;

            var origin = new Vector3(xmin, ymin, zmin);

            //var sx = end.X > begin.X ? 1 : -1;
            //var sy = end.Y > begin.Y ? 1 : -1;
            //var sz = end.Z > begin.Z ? 1 : -1;
            //var dx = Math.Abs(end.X - begin.X);
            //var dy = Math.Abs(end.Y - begin.Y);
            //var dz = Math.Abs(end.Z - begin.Z);

            for (int i = 0; i < dx; i++)
            {
                for (int j = 0; j < dy; j++)
                {
                    for (int k = 0; k < dz; k++)
                    {
                        list.Add(origin + new Vector3(i, j, k));
                    }
                }
            }
            return list;
        }
        static public List<Vector3> GetBox(this Vector3 begin, int dx, int dy, int dz)
        {
            List<Vector3> list = new List<Vector3>();

            for (int i = 0; i < dx; i++)
            {
                for (int j = 0; j < dy; j++)
                {
                    for (int k = 0; k < dz; k++)
                    {
                        list.Add(begin + new Vector3(i, j, k));
                    }
                }
            }
            return list;
        }

        static public Vector3[] GetNeighbors(this Vector3 global)
        {
            Vector3[] neighbors = new Vector3[6];
            neighbors[0] = (global + new Vector3(1, 0, 0));
            neighbors[1] = (global - new Vector3(1, 0, 0));
            neighbors[2] = (global + new Vector3(0, 1, 0));
            neighbors[3] = (global - new Vector3(0, 1, 0));
            neighbors[4] = (global + new Vector3(0, 0, 1));
            neighbors[5] = (global - new Vector3(0, 0, 1));
            return neighbors;
        }
        static public List<Vector2> GetNeighbors(this Vector2 coords)
        {
            List<Vector2> neighbors = new List<Vector2>();
            neighbors.Add(coords + new Vector2(1, 0));
            neighbors.Add(coords - new Vector2(1, 0));
            neighbors.Add(coords + new Vector2(0, 1));
            neighbors.Add(coords - new Vector2(0, 1));
            return neighbors;
        }
        static public List<Vector3> GetNeighborsDiag(this Vector3 global)
        {
            List<Vector3> neighbors = new List<Vector3>();

            neighbors.Add(global + new Vector3(1, 0, 0));
            neighbors.Add(global + new Vector3(-1, 0, 0));
            neighbors.Add(global + new Vector3(0, 1, 0));
            neighbors.Add(global + new Vector3(0, -1, 0));
            neighbors.Add(global + new Vector3(1, 1, 0));
            neighbors.Add(global + new Vector3(-1, 1, 0));
            neighbors.Add(global + new Vector3(1, -1, 0));
            neighbors.Add(global + new Vector3(-1, -1, 0));

            //neighbors.Add(global + new Vector3(0, 0, -1));
            //neighbors.Add(global + new Vector3(1, 0, -1));
            //neighbors.Add(global + new Vector3(-1, 0, -1));
            //neighbors.Add(global + new Vector3(0, 1, -1));
            //neighbors.Add(global + new Vector3(0, -1, -1));
            //neighbors.Add(global + new Vector3(1, 1, -1));
            //neighbors.Add(global + new Vector3(-1, 1, -1));
            //neighbors.Add(global + new Vector3(1, -1, -1));
            //neighbors.Add(global + new Vector3(-1, -1, -1));

            neighbors.Add(global + new Vector3(-1, -1, -1));
            neighbors.Add(global + new Vector3(-1, 0, -1));
            neighbors.Add(global + new Vector3(-1, 1, -1));
            neighbors.Add(global + new Vector3(0, -1, -1));
            neighbors.Add(global + new Vector3(0, 0, -1));
            neighbors.Add(global + new Vector3(0, 1, -1));
            neighbors.Add(global + new Vector3(1, -1, -1));
            neighbors.Add(global + new Vector3(1, 0, -1));
            neighbors.Add(global + new Vector3(1, 1, -1));

            //neighbors.Add(global + new Vector3(1, 0, 1));
            //neighbors.Add(global + new Vector3(-1, 0, 1));
            //neighbors.Add(global + new Vector3(0, 1, 1));
            //neighbors.Add(global + new Vector3(0, -1, 1));
            //neighbors.Add(global + new Vector3(1, 1, 1));
            //neighbors.Add(global + new Vector3(-1, 1, 1));
            //neighbors.Add(global + new Vector3(1, -1, 1));
            //neighbors.Add(global + new Vector3(-1, -1, 1));

            neighbors.Add(global + new Vector3(-1, -1, 1));
            neighbors.Add(global + new Vector3(-1, 0, 1));
            neighbors.Add(global + new Vector3(-1, 1, 1));
            neighbors.Add(global + new Vector3(-1, -1, 1));
            neighbors.Add(global + new Vector3(-1, 0, 1));
            neighbors.Add(global + new Vector3(-1, 1, 1));
            neighbors.Add(global + new Vector3(-1, -1, 1));
            neighbors.Add(global + new Vector3(-1, 0, 1));
            neighbors.Add(global + new Vector3(-1, 1, 1));

            //neighbors.Add(global + new Vector3(1, 0, 0));
            //neighbors.Add(global - new Vector3(1, 0, 0));
            //neighbors.Add(global + new Vector3(0, 1, 0));
            //neighbors.Add(global - new Vector3(0, 1, 0));
            //neighbors.Add(global + new Vector3(0, 0, 1));
            //neighbors.Add(global - new Vector3(0, 0, 1));

            //neighbors.Add(global + new Vector3(1, 1, 1));
            //neighbors.Add(global + new Vector3(1, 1, -1));
            //neighbors.Add(global + new Vector3(1, -1, 1));
            //neighbors.Add(global + new Vector3(1, -1, -1));
            //neighbors.Add(global + new Vector3(-1, 1, 1));
            //neighbors.Add(global + new Vector3(-1, 1, -1));
            //neighbors.Add(global + new Vector3(-1, -1, 1));
            //neighbors.Add(global + new Vector3(-1, -1, -1));

            return neighbors;
        }
        static public List<Vector3> GetColumn(this Vector3 global)
        {
            List<Vector3> list = new List<Vector3>();
            for (int i = 0; i < Map.MaxHeight; i++)
                list.Add(new Vector3(global.X, global.Y, i));
            return list;
        }
        public static Queue<Tuple<T1, T2>> ToTupleQueue<T1, T2>(this object[] p)
        {
            Queue<object> temp = new Queue<object>(p);
            Queue<Tuple<T1, T2>> queue = new Queue<Tuple<T1, T2>>();
            while (temp.Count > 0)
                queue.Enqueue(Tuple.Create((T1)temp.Dequeue(), (T2)temp.Dequeue()));
            return queue;
        }
        static public bool IsConnected(this List<Vector3> globals)
        {
            Queue<Vector3> open = new Queue<Vector3>();
            HashSet<Vector3> closed = new HashSet<Vector3>();
            var first = globals.First();
            open.Enqueue(first);
            closed.Add(first);
            while (open.Count > 0)
            {
                var current = open.Dequeue();
                closed.Add(current);
                foreach (var n in current.GetNeighbors())
                {
                    if (!globals.Contains(n))
                        continue;
                    if (!closed.Contains(n))
                        open.Enqueue(n);
                }
            }
            return closed.Count == globals.Count;
        }

        public static void ShowDialog(this Exception e)
        {
            UI.MessageBox.Create("Exception", e.ToString(),// () => ScreenManager.Remove(), () => ScreenManager.Remove()).ShowDialog();
                        "Close", () => {},//ScreenManager.Remove(),
                        "Copy to Clipboard", () =>
                        {
                            Thread t = new Thread(() => System.Windows.Forms.Clipboard.SetText(e.ToString()));
                            t.SetApartmentState(ApartmentState.STA);
                            t.Start();
                        //    ScreenManager.Remove();
                        }).ShowDialog();
        }

        //public static Color operator +(Color c1, Color c2)
        //{
        //    return new Color(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B, c1.A + c2.A);
        //}
        //public static Color operator *(Color c1, Color c2)
        //{
        //    return new Color(c1.R * c2.R, c1.G * c2.G, c1.B * c2.B, c1.A * c2.A);
        //}
        public static Color Add(this Color c1, Vector4 c2)
        {
            return new Color(c1.R + c2.X * 255, c1.G + c2.Y * 255, c1.B + c2.Z * 255, c1.A + c2.W * 255);
        }
        public static Color Add(this Color c1, Color c2)
        {
            return new Color(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B, c1.A + c2.A);
        }
        //public static Color Multiply(this Color c1, Color c2)
        //{
        //    return new Color(c1.R * c2.R, c1.G * c2.G, c1.B * c2.B, c1.A * c2.A);
        //}
        public static Color Multiply(this Color c1, Color c2)
        {
            //c1 *= (1 / 255f);
            //c2 *= (1 / 255f);
            float r = (c1.R / 255f) * (c2.R / 255f);
            float g = (c1.G / 255f) * (c2.G / 255f);
            float b = (c1.B / 255f) * (c2.B / 255f);
            float a = (c1.A / 255f) * (c2.A / 255f);
            return new Color(r, g, b, a);
        }

        public static List<SaveTag> Save(this Color c)
        {
            List<SaveTag> tag = new List<SaveTag>();
            tag.Add(new SaveTag(SaveTag.Types.Byte, "R", c.R));
            tag.Add(new SaveTag(SaveTag.Types.Byte, "G", c.G));
            tag.Add(new SaveTag(SaveTag.Types.Byte, "B", c.B));
            tag.Add(new SaveTag(SaveTag.Types.Byte, "A", c.A));
            return tag;
        }
        //public static void Load(this Color c, SaveTag tag)
        //{
        //    c.R = tag.GetValue<Byte>("R");
        //    c.G = tag.GetValue<Byte>("G");
        //    c.B = tag.GetValue<Byte>("B");
        //    c.A = tag.GetValue<Byte>("A");
        //}
        
        public static void Write(this Color c, BinaryWriter w)
        {
            w.Write(c.R);
            w.Write(c.G);
            w.Write(c.B);
            w.Write(c.A);
        }
        public static Color ReadColor(this BinaryReader r)
        {
            Color c = new Color();
            c.R = r.ReadByte();
            c.G = r.ReadByte();
            c.B = r.ReadByte();
            c.A = r.ReadByte();
            return c;
        }

        public static GameObject GetObject(this GameObject.Types type)
        {
            return GameObject.Objects[type];
        }
        public static GameObject GetObject(this int type)
        {
            return GameObject.Objects[type];
        }

        public static Dictionary<int, int> ToDictionaryIdAmount(this IEnumerable<GameObject> objList)
        {
            var dic = new Dictionary<int, int>();
            foreach (var item in objList)
                dic.AddOrUpdate((int)item.ID, item.StackSize, f => f + item.StackSize);
            return dic;
        }
        public static Dictionary<GameObject, int> ToDictionaryGameObjectAmount(this IEnumerable<GameObject> objList)
        {
            var dic = new Dictionary<GameObject, int>();
            foreach (var item in objList)
                dic.AddOrUpdate(item, item.StackSize, f => f + item.StackSize);
            return dic;
        }

        public static int GetMaxWidth(this IEnumerable<string> strings)
        {
            int max = 0;
            foreach (var item in strings)
                max = (int)Math.Max(max, Math.Ceiling(UI.UIManager.Font.MeasureString(item).X));
            return max;
        }

        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value, Func<TKey, TValue, TValue> updater)
        {
            TValue existing;
            if (dic.TryGetValue(key, out existing))
                updater(key, existing);
            else
                dic.Add(key, value);
        }
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value, Func<TValue, TValue> updater)
        {
            TValue existing;
            if (dic.TryGetValue(key, out existing))
                dic[key] = updater(existing);
            else
                dic.Add(key, value);
        }
        public static void Update<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Func<TValue, TValue> updater)
        {
            TValue existing;
            if (dic.TryGetValue(key, out existing))
                dic[key] = updater(existing);
        }
        public static bool TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Action<TValue> action)
        {
            TValue existing;
            if (dic.TryGetValue(key, out existing))
            {
                action(existing);
                return true;
            }
            return false;
        }
        public static void Draw(this Vector3 global, MySpriteBatch sb, Camera cam, Graphics.AtlasWithDepth.Node.Token sprite, Color color)
        {
            var bounds = cam.GetScreenBounds(global, Block.Bounds);
            var pos = new Vector2(bounds.X, bounds.Y);
            var depth = global.GetDrawDepth(Engine.Map, cam);
            sb.Draw(sprite.Atlas.Texture, pos, sprite.Rectangle, 0, Vector2.Zero, cam.Zoom, color, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, depth);
        }


        public static XElement GetOrCreateElement(this XContainer container, string name)
        {
            var element = container.Element(name);
            if (element == null)
            {
                element = new XElement(name);
                container.Add(element);
            }
            return element;
        }

        public static Texture2D ToGrayscale(this Texture2D tex)
        {
            Color[] array = new Color[tex.Width * tex.Height];
            tex.GetData(array);
            Color[] grayscale = new Color[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                var c = array[i];
                var value = c.R + c.G + c.B;
                value /= 3;
                grayscale[i] = new Color(value, value, value, c.A);
            }
            Texture2D copy = new Texture2D(tex.GraphicsDevice, tex.Width, tex.Height);
            copy.SetData(grayscale);
            return copy;
        }
    }

}
