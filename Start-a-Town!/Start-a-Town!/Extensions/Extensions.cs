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
            int stackMax = obj.Object.StackMax;// (int)obj.Object["Gui"]["StackMax"];
            foreach (var slot in list)
                capacity = slot.HasValue ? stackMax - slot.StackSize : stackMax;
            if (capacity < obj.StackSize)
                return false;
            foreach (var slot in list.FindAll(foo => foo.HasValue).FindAll(foo => foo.Object.IDType == obj.Object.IDType))
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
        

        public static void Remove(this List<GameObjectSlot> list, GameObject.Types objID, int amount)
        {
            var slots = new Queue<GameObjectSlot>(from obj in list
                                                                    where obj.HasValue
                                                                    where obj.Object.IDType == objID
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
        public static Vector2 ToVectorX(this int value)
        { return new Vector2(value, 0); }
        public static Vector2 ToVectorY(this int value)
        { return new Vector2(0, value); }
        public static Vector2 ToVectorX(this float value)
        { return new Vector2(value, 0); }
        public static Vector2 ToVectorY(this float value)
        { return new Vector2(0, value); }

        public static Vector2 ToVector(this Point point)
        { return new Vector2(point.X, point.Y); }

        //public static BlockComponent GetObject(this Block.Types block)
        //{
        //    return BlockComponent.Blocks[block]["Physics"] as BlockComponent;
        //}
        [Obsolete]
        public static bool IsNull(this object obj)
        {
            throw new Exception();
            return obj == null;
        }
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
            ////float mapX = worldGlobal.X - map.Global.X, mapY = worldGlobal.Y - map.Global.Y;
            //float gd = rotated.X + rotated.Y + 1; //+1 to compensate for (0,0) being at the center of the block

            //Vector2 furthest = Vector2.Zero;// -Vector2.One * 0.5f;//.GetRotated(camera)*0.5f;
            //Vector2 nearest = new Vector2(Chunk.Size);// + 0.5f);//.GetRotated(camera);

            ////float far = -1;
            ////float near = Chunk.Size * 2 + 1;

            //float far = furthest.X + furthest.Y;
            //float near = nearest.X + nearest.Y;

            //float range = near - far;
            //float d = (gd - far) / range;
            //return d;// 1 - d;
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
            //BlockComponent.Blocks[global.GetCell(map).Type].OnRemoved(map, global);
            //return true;
            return global.TryRemoveBlock(net, out _);
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
        

        public static bool TrySetCell(this Vector3 global, IObjectProvider net, Block.Types type)
        {
            return global.TrySetCell(net, type, global.GetData(net), 0, 0, out _);
        }
        public static bool TrySetCell(this Vector3 global, IObjectProvider net, Block.Types type, byte data, int variation = 0, int orientation = 0)
        {
            return global.TrySetCell(net, type, data, variation, orientation, out _);
        }
        public static bool TrySetCell(this Vector3 global, IObjectProvider net, Block.Types type, byte data, out Block.Types oldBlock)
        {
            return global.TrySetCell(net, type, data, 0, 0, out oldBlock, out _);
        }
        public static bool TrySetCell(this Vector3 global, IObjectProvider net, Block.Types type, byte data, int variation, int orientation, out CellOperation undoOperation)
        {
            return global.TrySetCell(net, type, data, variation, orientation, out _, out undoOperation);
        }
        [Obsolete]
        public static bool TrySetCell(this Vector3 global, IObjectProvider net, Block.Types type, byte data, int variation, int orientation, out Block.Types oldBlock, out CellOperation undoOperation)
        {
            throw new Exception();
            //Cell cell = global.GetCell(net.Map);
            Cell cell = net.Map.GetCell(global);

            if (cell == null)
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
                chunk.TryRemoveBlockEntity(cell.LocalCoords);
                //chunk.BlockEntitiesByPosition.Remove(cell.LocalCoords);
            Block block = Block.Registry[type];
            var blockentity = block.CreateBlockEntity();
            if (blockentity != null)
                chunk.AddBlockEntity(blockentity, cell.LocalCoords);
                //chunk.BlockEntitiesByPosition[cell.LocalCoords] = blockentity;

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
            //return global.TryGetCell(map, out cell) ? cell.BlockData : (byte)0;
            return map.TryGetCell(global, out Cell cell) ? cell.BlockData : (byte)0;
        }
        public static Cell GetCell(this Vector3 global, Map map)
        {
            return Position.GetCell(map, global);
        }
        public static bool TryGetCell(this Vector3 global, Map map, out Cell cell)
        {
            return global.TryGetAll(map, out Chunk chunk, out cell);
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
            Chunk.TryGetSunlight(map, global, out byte sunlight);
            return sunlight;
        }
        public static byte GetBlockLight(this Vector3 global, Map map)
        {
            Chunk.TryGetBlocklight(map, global, out byte blocklight);
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
        public static Vector3 SnapToBlock(this Vector3 vector)
        {
            vector.X = (int)Math.Round(vector.X);
            vector.Y = (int)Math.Round(vector.Y);
            vector.Z = (int)Math.Floor(vector.Z);
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
        public static TargetArgs ToTarget(this Vector3 global, IMap map)
        {
            return new TargetArgs(map, global);
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
            //float rx, ry;
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
            global += 0.5f * new Vector3(1, 1, 0); // shouldnt it be -0.5f?
            global -= global.FloorXY();// -global.RoundXY();
            return global;
        }
        static public Vector3 CeilingZ(this Vector3 global)
        {
            return new Vector3(global.X, global.Y, (float)Math.Ceiling(global.Z));
        }
        static public Vector3 FloorZ(this Vector3 global)
        {
            return new Vector3(global.X, global.Y, (float)Math.Floor(global.Z));
        }
        static public Vector3 Above(this Vector3 global)
        {
            return global + Vector3.UnitZ;
        }
        static public Vector3 Below(this Vector3 global)
        {
            return global - Vector3.UnitZ;
        }
        static public Vector3 West(this Vector3 global)
        {
            return global - Vector3.UnitX;
        }
        static public Vector3 East(this Vector3 global)
        {
            return global + Vector3.UnitX;
        }
        static public Vector3 North(this Vector3 global)
        {
            return global - Vector3.UnitY;
        }
        static public Vector3 South(this Vector3 global)
        {
            return global + Vector3.UnitY;
        }
        static public bool IsWithinChunkBounds(this Vector3 local)
        {
            //return !(local.X < 0 || local.X > Chunk.Size - 1 || local.Y < 0 || local.Y > Chunk.Size - 1 || local.Z < 0 || local.Z > this.Map.GetMaxHeight() - 1);
            return !(local.X < 0 || local.X > Chunk.Size - 1 || local.Y < 0 || local.Y > Chunk.Size - 1 || local.Z < 0 || local.Z > Start_a_Town_.Map.MaxHeight - 1);

        }
        static public bool IsZWithinBounds(this Vector3 local)
        {
            //return !(local.X < 0 || local.X > Chunk.Size - 1 || local.Y < 0 || local.Y > Chunk.Size - 1 || local.Z < 0 || local.Z > this.Map.GetMaxHeight() - 1);
            return !(local.Z < 0 || local.Z > Start_a_Town_.Map.MaxHeight - 1);

        }
       
        //public static bool TryGetBlockEntity(this Vector3 global, Map map, out BlockEntity entity)// IObjectProvider net)
        //{
        //    Chunk chunk = global.GetChunk(map);
        //    //chunk.BlockEntitiesByPosition.TryGetValue(global.ToLocal(), out entity);
        //    chunk.TryRemoveBlockEntity(global.ToLocal(), out entity);
        //    return entity != null;
        //}
        //public static Block GetBlock(this Vector3 global, Map map)// IObjectProvider net)
        //{
        //    if (!global.TryGetCell(map, out Cell cell))
        //        //throw new Exception("Block doesn't exist");
        //        return null;
        //    return cell.Block;//.Solid;
        //}
        //public static bool TryGetBlock(this Vector3 global, Map map, Action<Block, Cell> action)// IObjectProvider net)
        //{
        //    if (!global.TryGetCell(map, out Cell cell))
        //        //throw new Exception("Block doesn't exist");
        //        return false;
        //    action(cell.Block, cell);
        //    return true;
        //}
        public static DialogueOption ToDialogueOption(this string text, DialogueOptionHandler handler) { return new DialogueOption(text, handler); }
        public static DialogueOption ToDialogueOption(this string text, DialogueOptionHandler handler, Func<bool> condition) { return new DialogueOption(text, handler, condition); }

        public static UI.Label ToLabel(this string text) { return new UI.Label(Vector2.Zero, text); }
        public static UI.Label ToLabel(this string text, int width) { return new UI.Label(Vector2.Zero, text) { Width = width }; }
        public static UI.Label ToLabel(this string text, Vector2 location) { return new UI.Label(location, text); }
        public static UI.Label ToLabel(this string text, Vector2 location, int width) { return new UI.Label(location, text) { Width = width }; }

        public static int MaxWidth(this IEnumerable<string> strings, SpriteFont font)
        {
            var max = 0;
            foreach (var txt in strings)
                max = Math.Max(max, (int)font.MeasureString(txt).X);
            return max;
        }

        public static string Wrap(this string text, int maxWidthInPixels)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            string[] words = text.Split(' ');
            var newtext = new StringBuilder();
            string line = "";
            foreach (var word in words)
            {
                if ((int)(UI.UIManager.Font.MeasureString(line + word)).X > maxWidthInPixels)
                {
                    newtext.AppendLine(line);
                    line = "";
                }
                line += string.Format("{0} ", word);
            }
            if (line.Length > 0)
                newtext.Append(line);
            return newtext.ToString();
        }


        
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            // Ignore return value
            dictionary.TryGetValue(key, out TValue ret);
            return ret;
        }
        public static void ToConsole(this object obj)
        {
            Console.WriteLine(obj.ToString());
        }
        public static Time ToTime(this DateTime dateTime) { return new Time(dateTime); }
        public static string ToLocalTime(this DateTime dateTime)
        { return dateTime.ToString("MMM dd, HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-GB")); }


        public static List<Rectangle> Divide(this Rectangle rect, int count)
        {
            var list = new List<Rectangle>();
            var sqrt = (int)Math.Sqrt(count);
            var w = rect.Width / sqrt;
            var h = rect.Height / sqrt;
            for (int i = 0; i < sqrt; i++)
                for (int j = 0; j < sqrt; j++)
                    list.Add(new Rectangle(rect.X + i * w, rect.Y + j * h, w, h));
            return list;
        }

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
        public static void DrawHighlight(this Vector4 bounds, Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Color color, Vector2 origin, float rotation)
        {
            sb.Draw(UI.UIManager.Highlight, new Vector2(bounds.X, bounds.Y), null, color, rotation, origin, new Vector2(bounds.Z, bounds.W), Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
        }
        public static void DrawHighlightBorder(this Rectangle bounds, Microsoft.Xna.Framework.Graphics.SpriteBatch sb, float alpha = .5f, float thickness = 1, int padding = 0)
        {
            bounds.DrawHighlightBorder(sb, Color.White * alpha, Vector2.Zero, thickness, padding);
        }
        public static void DrawHighlightBorder(this Rectangle bounds, Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Color color, float thickness = 1)
        {
            bounds.DrawHighlightBorder(sb, color, Vector2.Zero, thickness);
        }
        public static void DrawHighlightBorder(this Rectangle bounds, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            bounds.DrawHighlightBorder(sb, Color.White, Vector2.Zero);
        }
        public static void DrawHighlightBorder(this Rectangle bounds, Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Color color, Vector2 origin, float thickness = 1, int padding = 0)
        {
            var intthickness = (int)Math.Max(1, thickness);
            var padpad = 2 * padding;
            // Draw top line
            sb.Draw(UI.UIManager.Highlight, new Rectangle(bounds.X - padding + intthickness , bounds.Y - padding, bounds.Width + padpad - intthickness, intthickness), color);

            // Draw left line
            sb.Draw(UI.UIManager.Highlight, new Rectangle(bounds.X - padding, bounds.Y - padding, intthickness, bounds.Height + padpad - intthickness), color);

            // Draw bottom line
            sb.Draw(UI.UIManager.Highlight, new Rectangle(bounds.X - padding,
                                            bounds.Y + padding + bounds.Height - intthickness,
                                            bounds.Width - intthickness + padpad,
                                            intthickness), color);

            // Draw right line
            sb.Draw(UI.UIManager.Highlight, new Rectangle((bounds.X + padding + bounds.Width - intthickness),
                                            bounds.Y - padding + intthickness,
                                            intthickness,
                                            bounds.Height - intthickness + padpad), color);

            //// Draw top line
            //sb.Draw(UI.UIManager.Highlight, new Rectangle(bounds.X, bounds.Y, bounds.Width, intthickness), color);

            //// Draw left line
            //sb.Draw(UI.UIManager.Highlight, new Rectangle(bounds.X, bounds.Y, intthickness, bounds.Height), color);

            //// Draw right line
            //sb.Draw(UI.UIManager.Highlight, new Rectangle((bounds.X + bounds.Width - intthickness),
            //                                bounds.Y,
            //                                intthickness,
            //                                bounds.Height), color);
            //// Draw bottom line
            //sb.Draw(UI.UIManager.Highlight, new Rectangle(bounds.X,
            //                                bounds.Y + bounds.Height - intthickness,
            //                                bounds.Width,
            //                                intthickness), color);
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
        public static void Write(this BinaryWriter w, Progress progress)
        {
            progress.Write(w);
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
        public static void Write(this BinaryWriter writer, IntVec3 vec3)
        {
            writer.Write(vec3.X);
            writer.Write(vec3.Y);
            writer.Write(vec3.Z);
        }
        public static void Write(this BinaryWriter writer, Vector3? vec3)
        {
            var has = vec3.HasValue;
            writer.Write(has);
            if (!has)
                return;
            var val = vec3.Value;
            writer.Write(val.X);
            writer.Write(val.Y);
            writer.Write(val.Z);
        }
        public static void Write(this BinaryWriter w, TargetArgs target)
        {
            target.Write(w);
        }
        public static void Write(this BinaryWriter writer, PacketType packetType)
        {
            writer.Write((int)packetType);
        }
        public static void Write(this BinaryWriter w, params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var argType = arg.GetType();
                if (argType == typeof(int))
                    w.Write((int)arg);
                else if (argType == typeof(float))
                    w.Write((float)arg);
                else if (argType == typeof(string))
                    w.Write((string)arg);
                else if (argType == typeof(byte))
                    w.Write((byte)arg);
                else if (argType == typeof(short))
                    w.Write((short)arg);
                else if (argType == typeof(bool))
                    w.Write((bool)arg);
                else if (argType == typeof(long))
                    w.Write((long)arg);
                else if (argType == typeof(uint))
                    w.Write((uint)arg);
                else if (argType == typeof(ushort))
                    w.Write((ushort)arg);
                else if (argType == typeof(ulong))
                    w.Write((ulong)arg);
                else if (argType == typeof(decimal))
                    w.Write((decimal)arg);
                else if (argType == typeof(IntVec3))
                    w.Write((IntVec3)arg);
                else
                    throw new ArgumentException();
            }
        }
        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }
        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        public static IntVec3 ReadIntVec3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        }
        public static Vector3? ReadVector3Nullable(this BinaryReader reader)
        {
            if (!reader.ReadBoolean())
                return null;
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
            using var r = new BinaryReader(new MemoryStream(data));
            reader(r);
        }
        public static void Translate(this byte[] data, Action<BinaryReader> reader)
        {
            using var r = new BinaryReader(new MemoryStream(data));
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
            using var w = new BinaryWriter(new MemoryStream());
            writer(w);
            return (w.BaseStream as MemoryStream).ToArray();
        }
        public static byte[] Decompress(this byte[] compressed)
        {
            using var compressedStream = new MemoryStream(compressed);
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var output = new MemoryStream();
            zipStream.CopyTo(output);
            return output.ToArray();
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

        //#region Game Components
        //static public Need CreateNew(this Need.Types id, float value = 100f, float decay = .1f, float tolerance = 50f)
        //{
        //    return Need.Factory.Create(id, value, decay, tolerance);
        //}
        //#endregion
        public static T SelectRandom<T>(this ICollection<T> collection, Random random)
        {
            return collection.ElementAt(random.Next(0, collection.Count));
        }

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
        public static List<T> Randomize<T>(this IEnumerable<T> list, Random random)
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
        
        public static SaveTag Save(this List<TargetArgs> list, string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.List, name, SaveTag.Types.Compound);
            foreach (var i in list)
                tag.Add(i.Save());
            return tag;
        }
        public static void Load(this List<TargetArgs> list, SaveTag tag)
        {
            list.Clear();
            var tags = tag.Value as List<SaveTag>;
            foreach (var i in tags)
                list.Add(new TargetArgs(null, i));

        }

        public static void Write(this BinaryWriter w, List<Vector2> list)
        {
            w.Write(list.Count);
            foreach (var g in list)
                w.Write(g);
        }
        public static void Write(this BinaryWriter w, List<Vector3> list)
        {
            w.Write(list.Count);
            foreach (var g in list)
                w.Write(g);
        }
        public static void Write(this BinaryWriter w, IEnumerable<Vector3> list)
        {
            w.Write(list.Count());
            foreach (var g in list)
                w.Write(g);
        }

        
        
        
        
        public static void Write(this BinaryWriter w, List<string> strings)
        {
            w.Write(strings.Count);
            foreach (var s in strings)
                w.Write(s);
        }

        public static SaveTag Save(this Dictionary<string, int> dic, string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(dic.Keys.Save("Keys"));
            tag.Add(dic.Values.Save("Values"));
            return tag;
        }
        public static Dictionary<string, int> Load(this Dictionary<string, int> dic, SaveTag save)
        {
            var keys = new List<string>().Load(save["Keys"].Value as List<SaveTag>);
            var values = new List<int>().Load(save["Values"].Value as List<SaveTag>);
            var tempdic = keys.Zip(values, (k, v) => new { k, v });//.ToDictionary(x => x.k, x => x.v);
            foreach (var i in tempdic)
                dic.Add(i.k, i.v);
            return dic;
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
            var list = new List<Vector2>();


            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    var vec2 = new Vector2(i, j);
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
        static public bool ContainsEntityFootprint(this Vector3 blockGlobal, GameObject entity)
        {
            var footprint = entity.GetFootprint();
            var blockbox = blockGlobal.GetBoundingBox();
            var containment = blockbox.Contains(footprint);
            return containment == ContainmentType.Contains;
        }
        static public Rectangle GetRectangle(this Vector2 vec1, Vector2 vec2)
        {
            int xm = (int)Math.Min(vec1.X, vec2.X);
            int ym = (int)Math.Min(vec1.Y, vec2.Y);

            int xM = (int)(vec1.X + vec2.X - xm);
            int yM = (int)(vec1.Y + vec2.Y - ym);

            //var m = new Vector2(xm, ym);
            //var M = new Vector2(xM, yM);

            return new Rectangle(xm, ym, xM - xm, yM - ym);
        }
        static public BoundingBox GetBoundingBox(this Vector3 vec1, Vector3 vec2)
        {
            int xm = (int)Math.Min(vec1.X, vec2.X);
            int ym = (int)Math.Min(vec1.Y, vec2.Y);
            int zm = (int)Math.Min(vec1.Z, vec2.Z);

            int xM = (int)(vec1.X + vec2.X - xm);
            int yM = (int)(vec1.Y + vec2.Y - ym);
            int zM = (int)(vec1.Z + vec2.Z - zm);

            var m = new Vector3(xm, ym, zm);
            var M = new Vector3(xM, yM, zM);

            return new BoundingBox(m, M);
        }
        
        static public List<Vector3> GetBox(this Vector3 begin, int dx, int dy, int dz)
        {
            var list = new List<Vector3>();

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
        static public List<Vector3> GetBox(this BoundingBox box)
        {
            return box.Min.GetBox(box.Max);
        }
        
        static public BoundingBox GetBoundingBox(this Vector3 blockCoords)
        {
            blockCoords = blockCoords.SnapToBlock(); //necessary? do i need this?
            return new BoundingBox(blockCoords - new Vector3(.5f, .5f, 0), blockCoords + new Vector3(.5f, .5f, 1));
        }

        static public List<Vector3> GetBox(this Vector3 begin, Vector3 end)
        {
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

            var list = new List<Vector3>((int)(dx * dy * dz));

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
        static public IEnumerable<IntVec3> GetBoxLazy(this IntVec3 begin, IntVec3 end)
        {
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


            for (int i = 0; i < dx; i++)
            {
                for (int j = 0; j < dy; j++)
                {
                    for (int k = 0; k < dz; k++)
                    {
                        yield return origin + new Vector3(i, j, k);
                    }
                }
            }
        }

        static public List<Vector3> GetEntityCorners(this Vector3 pos)
        {
            var corners = new List<Microsoft.Xna.Framework.Vector3>()
            {
                pos + new Microsoft.Xna.Framework.Vector3(-.25f, -.25f, 0),
                pos + new Microsoft.Xna.Framework.Vector3(.25f, -.25f, 0),
                pos + new Microsoft.Xna.Framework.Vector3(-.25f, .25f, 0),
                pos + new Microsoft.Xna.Framework.Vector3(.25f, .25f, 0)
            };
            return corners;
        }
        static public List<Vector3> GetRectangle(this Vector3 begin, int w, int h)
        {
            var list = new List<Vector3>();
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                        list.Add(begin + new Vector3(i, j, 0));
            return list;
        }
        //static public List<Vector3> GetBox(this Vector3 begin, int w, int h, int d)
        //{
        //    List<Vector3> list = new List<Vector3>();
        //    for (int i = 0; i < w; i++)
        //        for (int j = 0; j < h; j++)
        //            for (int k = 0; k < d; j++)
        //                list.Add(begin + new Vector3(i, j, d));
        //    return list;
        //}
        
        static public Vector3[] GetNeighborsSameZ(this Vector3 global)
        {
            Vector3[] neighbors = new Vector3[4];
            neighbors[0] = (global + new Vector3(1, 0, 0));
            neighbors[1] = (global - new Vector3(1, 0, 0));
            neighbors[2] = (global + new Vector3(0, 1, 0));
            neighbors[3] = (global - new Vector3(0, 1, 0));
            return neighbors;
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
        //[Obsolete]
        //static public List<Vector2> GetNeighbors(this Vector2 coords)
        //{
        //    var neighbors = new List<Vector2>
        //    {
        //        coords + new Vector2(1, 0),
        //        coords - new Vector2(1, 0),
        //        coords + new Vector2(0, 1),
        //        coords - new Vector2(0, 1)
        //    };
        //    return neighbors;
        //}
        static public IEnumerable<Vector2> GetNeighbors(this Vector2 coords)
        {
            //var neighbors = new List<Vector2>
            //{
            yield return coords + new Vector2(1, 0);
            yield return coords - new Vector2(1, 0);
            yield return coords + new Vector2(0, 1);
            yield return coords - new Vector2(0, 1);
            //};
            //return neighbors;
        }
        static public IEnumerable<Vector2> GetNeighbors8(this Vector2 coords)
        {
            yield return (coords + new Vector2(-1, -1));
            yield return (coords + new Vector2(-1, 0));
            yield return (coords + new Vector2(-1, 1));
            yield return (coords + new Vector2(0, -1));
            yield return (coords + new Vector2(0, 1));
            yield return (coords + new Vector2(1, -1));
            yield return (coords + new Vector2(1, 0));
            yield return (coords + new Vector2(1, 1));
        }
        static public Vector3[] GetAdjacentExceptDirectlyAboveOrBelow(this Vector3 global)
        {
            Vector3[] neighbors = new Vector3[]{
            (global + new Vector3(1, 0, 0)),
            (global + new Vector3(-1, 0, 0)),
            (global + new Vector3(0, 1, 0)),
            (global + new Vector3(0, -1, 0)),

            (global + new Vector3(-1, 0, -1)),
            (global + new Vector3(0, -1, -1)),
            (global + new Vector3(0, 1, -1)),
            (global + new Vector3(1, 0, -1)),

            (global + new Vector3(-1, 0, 1)),
            (global + new Vector3(0, -1, 1)),
            (global + new Vector3(0, 1, 1)),
            (global + new Vector3(1, 0, 1))};

            return neighbors;
        }
       
        static public IEnumerable<IntVec3> GetNeighborsDiag(this IntVec3 global)
        {
            yield return global + new IntVec3(1, 0, 0);
            yield return global + new IntVec3(-1, 0, 0);
            yield return global + new IntVec3(0, 1, 0);
            yield return global + new IntVec3(0, -1, 0);
            yield return global + new IntVec3(1, 1, 0);
            yield return global + new IntVec3(-1, 1, 0);
            yield return global + new IntVec3(1, -1, 0);
            yield return global + new IntVec3(-1, -1, 0);



            yield return global + new IntVec3(-1, -1, -1);
            yield return global + new IntVec3(-1, 0, -1);
            yield return global + new IntVec3(-1, 1, -1);
            yield return global + new IntVec3(0, -1, -1);
            yield return global + new IntVec3(0, 0, -1);
            yield return global + new IntVec3(0, 1, -1);
            yield return global + new IntVec3(1, -1, -1);
            yield return global + new IntVec3(1, 0, -1);
            yield return global + new IntVec3(1, 1, -1);



            yield return global + new IntVec3(-1, -1, 1);
            yield return global + new IntVec3(-1, 0, 1);
            yield return global + new IntVec3(-1, 1, 1);
            yield return global + new IntVec3(0, -1, 1);
            yield return global + new IntVec3(0, 0, 1);
            yield return global + new IntVec3(0, 1, 1);
            yield return global + new IntVec3(1, -1, 1);
            yield return global + new IntVec3(1, 0, 1);
            yield return global + new IntVec3(1, 1, 1);
        }
        static public IEnumerable<Vector3> GetNeighborsDiag(this Vector3 global)
        {
            yield return global + new Vector3(1, 0, 0);
            yield return global + new Vector3(-1, 0, 0);
            yield return global + new Vector3(0, 1, 0);
            yield return global + new Vector3(0, -1, 0);
            yield return global + new Vector3(1, 1, 0);
            yield return global + new Vector3(-1, 1, 0);
            yield return global + new Vector3(1, -1, 0);
            yield return global + new Vector3(-1, -1, 0);



            yield return global + new Vector3(-1, -1, -1);
            yield return global + new Vector3(-1, 0, -1);
            yield return global + new Vector3(-1, 1, -1);
            yield return global + new Vector3(0, -1, -1);
            yield return global + new Vector3(0, 0, -1);
            yield return global + new Vector3(0, 1, -1);
            yield return global + new Vector3(1, -1, -1);
            yield return global + new Vector3(1, 0, -1);
            yield return global + new Vector3(1, 1, -1);



            yield return global + new Vector3(-1, -1, 1);
            yield return global + new Vector3(-1, 0, 1);
            yield return global + new Vector3(-1, 1, 1);
            yield return global + new Vector3(0, -1, 1);
            yield return global + new Vector3(0, 0, 1);
            yield return global + new Vector3(0, 1, 1);
            yield return global + new Vector3(1, -1, 1);
            yield return global + new Vector3(1, 0, 1);
            yield return global + new Vector3(1, 1, 1);
        }
        static public List<Vector3> GetColumn(this Vector3 global)
        {
            var list = new List<Vector3>();
            for (int i = 0; i < Map.MaxHeight; i++)
                list.Add(new Vector3(global.X, global.Y, i));
            return list;
        }
        public static Queue<Tuple<T1, T2>> ToTupleQueue<T1, T2>(this object[] p)
        {
            var temp = new Queue<object>(p);
            var queue = new Queue<Tuple<T1, T2>>();
            while (temp.Count > 0)
                queue.Enqueue(Tuple.Create((T1)temp.Dequeue(), (T2)temp.Dequeue()));
            return queue;
        }
        static public Vector3 Average(this ICollection<IntVec3> positions)
        {
            Vector3 average = default;
            foreach (var pos in positions)
                average += (Vector3)pos;
            return average / positions.Count;

        }
        static public bool IsConnectedNew(this IEnumerable<IntVec3> globals)
        {
            var unvisited = globals.ToHashSet();
            var first = globals.First();
            var queue = new Queue<Vector3>();
            queue.Enqueue(first);
            while(queue.Any())
            {
                var current = queue.Dequeue();
                unvisited.Remove(current);
                foreach(var n in current.GetAdjacentLazy())
                {
                    if (unvisited.Contains(n))
                        queue.Enqueue(n);
                }
            }
            return !unvisited.Any();
        }
        static public List<HashSet<IntVec3>> GetAllConnectedSubGraphs(this IEnumerable<IntVec3> all)
        {
            var splitgraphs = new List<HashSet<IntVec3>>();
            var tocheck = all;

            do
            {
                var (connected, disconnected) = tocheck.GetConnectedSubGraph();
                splitgraphs.Add(connected);
                tocheck = disconnected;
            } while (tocheck.Any());
            return splitgraphs;
        }
        /// <summary>
        /// the disconnected graph might be further disconnected in itself
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        static public (HashSet<IntVec3> connected, HashSet<IntVec3> disconnected) GetConnectedSubGraph(this IEnumerable<IntVec3> positions)
        {
            var disconnected = positions.ToHashSet();
            var connected = new HashSet<IntVec3>();
            var first = positions.First();
            var queue = new Queue<Vector3>();
            queue.Enqueue(first);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                disconnected.Remove(current);
                connected.Add(current);
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (disconnected.Contains(n))
                        queue.Enqueue(n);
                }
            }
            return (connected, disconnected);
        }
        static public bool IsConnected(this IEnumerable<Vector3> globals)
        {
            var open = new Queue<Vector3>();
            var closed = new HashSet<Vector3>();
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
            return closed.Count == globals.Count();
        }
        static public bool IsConnected(this HashSet<Vector3> globals)
        {
            var open = new Queue<Vector3>();
            var closed = new HashSet<Vector3>();
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
                        //"Close", () => { },//ScreenManager.Remove(),
                        "Copy to Clipboard", () =>
                        {
                            var t = new Thread(() => System.Windows.Forms.Clipboard.SetText(e.ToString()));
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
            var tag = new List<SaveTag>
            {
                new SaveTag(SaveTag.Types.Byte, "R", c.R),
                new SaveTag(SaveTag.Types.Byte, "G", c.G),
                new SaveTag(SaveTag.Types.Byte, "B", c.B),
                new SaveTag(SaveTag.Types.Byte, "A", c.A)
            };
            return tag;
        }
        //public static void Load(this Color c, SaveTag tag)
        //{
        //    c.R = tag.GetValue<Byte>("R");
        //    c.G = tag.GetValue<Byte>("G");
        //    c.B = tag.GetValue<Byte>("B");
        //    c.A = tag.GetValue<Byte>("A");
        //}
        static public Color GetColor(this Random rand)
        {
            var array = new byte[3];
            rand.NextBytes(array);
            return new Color(array[0], array[1], array[2]);
        }
        public static void Write(this Color c, BinaryWriter w)
        {
            w.Write(c.R);
            w.Write(c.G);
            w.Write(c.B);
            w.Write(c.A);
        }
        public static void Write(this BinaryWriter w, Color c)
        {
            w.Write(c.R);
            w.Write(c.G);
            w.Write(c.B);
            w.Write(c.A);
        }
        public static Color ReadColor(this BinaryReader r)
        {
            var c = new Color
            {
                R = r.ReadByte(),
                G = r.ReadByte(),
                B = r.ReadByte(),
                A = r.ReadByte()
            };
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
        public static IOrderedEnumerable<GameObject> OrderByDistanceTo(this IEnumerable<GameObject> objList, GameObject obj)
        {
            return objList.OrderBy(o => Vector3.DistanceSquared(o.Global, obj.Global));
        }
        public static IOrderedEnumerable<Vector3> OrderByDistanceTo(this IEnumerable<Vector3> positions, GameObject obj)
        {
            return positions.OrderByDistanceTo(obj.Global);// positions.OrderBy(p => Vector3.DistanceSquared(p, obj.Global));
        }
        public static IOrderedEnumerable<Vector3> OrderByDistanceTo(this IEnumerable<Vector3> positions, Vector3 pos)
        {
            return positions.OrderBy(p => Vector3.DistanceSquared(p, pos));
        }
        public static IOrderedEnumerable<Vector3> OrderByRegionDistance(this IEnumerable<Vector3> positions, Actor obj)
        {
            //return positions.OrderBy(pos => obj.Map.GetRegionDistance(obj.StandingOn(), pos, (int)obj.Physics.Height));
            return positions.OrderBy(pos => obj.Map.GetRegionDistance(obj.StandingOn(), pos, obj));

        }
        public static IEnumerable<Entity> OrderByReachableRegionDistance(this IEnumerable<Entity> targets, Actor actor)
        {
            return from t in targets
                   let dist = actor.Map.GetRegionDistance(actor.StandingOn(), t.Global.SnapToBlock(), actor)
                   where dist != -1
                   orderby dist
                   select t;
        }
        public static IEnumerable<GameObject> OrderByReachableRegionDistance(this IEnumerable<GameObject> targets, Actor actor)
        {
            //var height = actor.Physics.Reach;//  (int)actor.Physics.Height;
            return from t in targets
                   let dist = actor.Map.GetRegionDistance(actor.StandingOn(), t.Global.SnapToBlock(), actor)
                   where dist != -1
                   orderby dist
                   select t;
        }
        public static IEnumerable<TargetArgs> OrderByReachableRegionDistance(this IEnumerable<TargetArgs> targets, Actor actor)
        {
            //var height = actor.Physics.Reach;//  (int)actor.Physics.Height;
            return from t in targets
                   let dist = actor.Map.GetRegionDistance(actor.StandingOn(), t.Global.SnapToBlock(), actor)
                   where dist != -1
                   orderby dist
                   select t;
        }
        public static IEnumerable<IntVec3> OrderByReachableRegionDistance(this IEnumerable<IntVec3> positions, Actor actor)
        {
            return from pos in positions
                   let dist = actor.Map.GetRegionDistance(actor.StandingOn(), pos, actor)
                   where dist != -1
                   orderby dist
                   select pos;
        }
        public static IEnumerable<Vector3> OrderByReachableRegionDistance(this IEnumerable<Vector3> positions, Actor actor)
        {
            return from pos in positions
                   let dist = actor.Map.GetRegionDistance(actor.StandingOn(), pos, actor)
                   where dist != -1
                   orderby dist
                   select pos;
        }
        public static Dictionary<int, int> ToDictionaryIdAmount(this IEnumerable<GameObject> objList)
        {
            var dic = new Dictionary<int, int>();
            foreach (var item in objList)
                dic.AddOrUpdate((int)item.IDType, item.StackSize, f => f + item.StackSize);
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
            if (dic.TryGetValue(key, out TValue existing))
                updater(key, existing);
            else
                dic.Add(key, value);
        }
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value, Func<TValue, TValue> updater)
        {
            if (dic.TryGetValue(key, out TValue existing))
                dic[key] = updater(existing);
            else
                dic.Add(key, value);
        }
        public static void Update<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Func<TValue, TValue> updater)
        {
            if (dic.TryGetValue(key, out TValue existing))
                dic[key] = updater(existing);
        }
        public static bool TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Action<TValue> action)
        {
            if (dic.TryGetValue(key, out TValue existing))
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

        /// <summary>
        /// https://stackoverflow.com/questions/14892594/how-to-get-an-xelement-and-create-it-if-it-doesnt-exist
        /// </summary>
        /// <param name="container"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XElement GetOrCreateElement(this XContainer container, string name, object value = null)
        {
            var element = container.Element(name);
            if (element == null)
            {
                element = new XElement(name, value);
                container.Add(element);
            }
            return element;
        }
        public static XElement GetOrCreateElements(this XContainer container, params string[] names)
        {
            var currentelement = container;
            for (int i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var nextelement = currentelement.Element(name);
                if (nextelement == null)
                {
                    nextelement = new XElement(name);
                    currentelement.Add(nextelement);
                }
                currentelement = nextelement;
            }
            return currentelement as XElement;
        }
        public static void SetValue(this XDocument document, string path, object value)
        {
            var names = path.Split('/');
            document.Root.GetOrCreateElements(names).SetValue(value.ToString());
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
            var copy = new Texture2D(tex.GraphicsDevice, tex.Width, tex.Height);
            copy.SetData(grayscale);
            return copy;
        }
        public static Color[] ToGrayscaleArray(this Texture2D tex)
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
            return grayscale;
        }

        public static bool Chance(this Random rand, double chance)
        {
            return chance > 0 && (chance >= 1 || rand.NextDouble() <= chance);
        }
      
        static public Dictionary<T, U> ToDictionary<T,U>(this IList<T> listA, IList<U> listB)
        {
            var count = listA.Count;
            if (count != listB.Count)
                throw new Exception();
            var dic = new Dictionary<T, U>();
            for (int i = 0; i < count; i++)
            {
                dic.Add(listA[i], listB[i]); 
            }
            return dic;
        }
        static public Dictionary<TResult, UResult> ToDictionary<T, U, TResult, UResult>(this IList<T> listA, IList<U> listB, Func<T, TResult> keySelector, Func<U, UResult> valueSelector)
        {
            var count = listA.Count;
            if (count != listB.Count)
                throw new Exception();
            var dic = new Dictionary<TResult, UResult>();
            for (int i = 0; i < count; i++)
            {
                dic.Add(keySelector(listA[i]), valueSelector(listB[i]));
            }
            return dic;
        }
        static public bool TryParseColor(this string text, out Color color)
        {
            var posFrom = text.IndexOf('{');
            if (posFrom != -1)
            {
                var posTo = text.IndexOf('}', posFrom + 1);
                if (posFrom != -1)
                {
                    var sub = text.Substring(posFrom + 1, posTo - posFrom - 1);
                    var elements = sub.Split(' ');
                    var values = elements.Select(e => int.Parse(e.Split(':')[1])).ToArray();
                    color = new Color(values[0], values[1], values[2], values[3]);
                    return true;
                }
            }
            color = Color.White;
            return false;
        }
    }
}
