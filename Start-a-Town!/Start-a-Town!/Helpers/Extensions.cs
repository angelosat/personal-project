using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using System.Xml.Linq;
using System.Threading;

namespace Start_a_Town_
{
    public static class Extensions
    {
        public static bool Insert(this List<GameObjectSlot> list, GameObjectSlot obj)
        {
            int capacity = 0;
            int stackMax = obj.Object.StackMax;
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
                if (empty is not null)
                    empty.Swap(obj);
            }
            return true;
        }
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

        [Obsolete]
        public static bool IsNull(this object obj)
        {
            throw new Exception();
            return obj == null;
        }
        public static float GetMouseoverDepth(this Vector3 worldGlobal, IMap map, Camera camera)
        {
            Vector3 local = worldGlobal - new Vector3(map.GetOffset(), 0);
            Vector3 rotated = local.Rotate(camera);
            return (rotated.X + rotated.Y + worldGlobal.Z);
        }
        public static float GetDrawDepth(this Vector3 worldGlobal, IMap map, Camera camera)
        {
            Vector3 local = worldGlobal - new Vector3(map.GetOffset(), 0);
            Vector3 rotated = local.Rotate(camera);
            return (rotated.X + rotated.Y);
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
        static public Vector3 ToGlobal(this Vector3 local, Chunk chunk)
        {
            return new Vector3(chunk.Start.X + local.X, chunk.Start.Y + local.Y, local.Z);
        }
        static public Vector3 ToBlock(this Vector3 global)
        {
            global += 0.5f * new Vector3(1, 1, 0); // shouldnt it be -0.5f?
            global -= global.FloorXY();
            return global;
        }
        
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            // Ignore return value
            dictionary.TryGetValue(key, out TValue ret);
            return ret;
        }
        public static Time ToTime(this DateTime dateTime) { return new Time(dateTime); }
        public static string ToLocalTime(this DateTime dateTime)
        { return dateTime.ToString("MMM dd, HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-GB")); }
       
        public static bool Intersects(this Vector4 bounds, Vector2 position)
        {
            return (bounds.X <= position.X &&
                position.X < bounds.X + bounds.Z &&
                bounds.Y <= position.Y &&
                position.Y < bounds.Y + bounds.W);
        }

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
            list.Sort((u, v) =>
                {
                    if (u == v) return 0;
                    else if(Vector2.DistanceSquared(center, u) < Vector2.DistanceSquared(center, v)) return -1;
                    else return 1;
                });
            return list;
        }
        
        static public bool ContainsEntityFootprint(this Vector3 blockGlobal, GameObject entity)
        {
            var footprint = entity.GetFootprint();
            var blockbox = blockGlobal.GetBoundingBox();
            var containment = blockbox.Contains(footprint);
            return containment == ContainmentType.Contains;
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
            var corners = new List<Vector3>()
            {
                pos + new Vector3(-.25f, -.25f, 0),
                pos + new Vector3(.25f, -.25f, 0),
                pos + new Vector3(-.25f, .25f, 0),
                pos + new Vector3(.25f, .25f, 0)
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

        public static void ShowDialog(this Exception e)
        {
            UI.MessageBox.Create("Exception", e.ToString(),
                        "Copy to Clipboard", () =>
                        {
                            var t = new Thread(() => System.Windows.Forms.Clipboard.SetText(e.ToString()));
                            t.SetApartmentState(ApartmentState.STA);
                            t.Start();
                        }).ShowDialog();
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
    }
}
