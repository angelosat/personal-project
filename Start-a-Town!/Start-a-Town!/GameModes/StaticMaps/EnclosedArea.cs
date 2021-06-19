﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class EnclosedArea //: ISelectable
    {
        public int ID;
        public HashSet<Vector3> Positions = new HashSet<Vector3>();

        //public bool Exists => true;

        public override string ToString()
        {
            return string.Format("ID: {0} Size: {1}", this.ID, this.Positions.Count);
        }
        public void Add(Vector3 global)
        {
            this.Positions.Add(global);
        }
        public bool Contains(Vector3 global)
        {
            return this.Positions.Contains(global);
        }

        internal void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            w.Write(this.Positions);
        }
        static public EnclosedArea Create(BinaryReader r)
        {
            var area = new EnclosedArea();
            area.ID = r.ReadInt32();
            area.Positions = new HashSet<Vector3>(r.ReadListVector3());
            return area;
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.ID.Save("ID"));
            tag.Add(this.Positions.ToList().Save("Positions"));
            return tag;
        }
        static public EnclosedArea Create(SaveTag tag)
        {
            var area = new EnclosedArea();
            area.ID = (int)tag["ID"].Value;
            area.Positions = new HashSet<Vector3>(tag["Positions"].LoadListVector3());
            return area;
        }
        static public bool BeginInclusive(IMap map, IntVec3 global, HashSet<Vector3> area, HashSet<Vector3> edges)
        {
            area.Clear();
            edges.Clear();
            //var area = new EnclosedArea();
            //area.Add(global);
            if (map.GetCell(global).IsRoomBorder)
                edges.Add(global);
            else
                area.Add(global);
            var queue = new Queue<Vector3>();
            var handled = new HashSet<Vector3>() { global };
            queue.Enqueue(global);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    //if (!map.IsAir(n))
                    //    continue;
                    if (!map.Contains(n))
                        continue;
                    if (!map.IsAboveHeightMap(n))
                    {
                        //queue.Enqueue(n);
                        //if (map.IsAir(n))
                        if (!map.GetCell(n).IsRoomBorder)
                        {
                            area.Add(n);
                            queue.Enqueue(n);
                        }
                        else
                            edges.Add(n);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        static public EnclosedArea BeginInclusive(IMap map, Vector3 global)
        {
            var area = new EnclosedArea();
            area.Add(global);
            var queue = new Queue<Vector3>();
            var handled = new HashSet<Vector3>() { global };
            queue.Enqueue(global);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    //if (!map.IsAir(n))
                    //    continue;
                    if (!map.Contains(n))
                        continue;
                    if (!map.IsAboveHeightMap(n))
                    {
                        area.Add(n);
                        //queue.Enqueue(n);
                        //if (map.IsAir(n))
                        if (!map.GetCell(n).IsRoomBorder)
                            queue.Enqueue(n);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return area;
        }
        static public EnclosedArea BeginExclusive(IMap map, Vector3 global)
        {
            var area = new EnclosedArea();
            area.Add(global);
            var queue = new Queue<Vector3>();
            var handled = new HashSet<Vector3>() { global };
            queue.Enqueue(global);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    //if (!map.IsAir(n))
                    //    continue;
                    if (!map.Contains(n))
                        continue;
                    if (!map.IsAboveHeightMap(n))
                    {
                        //queue.Enqueue(n);
                        if (map.IsAir(n))
                        {
                            queue.Enqueue(n);
                            area.Add(n);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return area;
        }
        static public HashSet<Vector3> BeginExclusiveAsList(IMap map, Vector3 global)
        {
            var area = new HashSet<Vector3>
            {
                global
            };
            var queue = new Queue<Vector3>();
            var handled = new HashSet<Vector3>() { global };
            queue.Enqueue(global);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    if (!map.Contains(n))
                        continue;

                    var cell = map.GetCell(n);
                    if (!cell.IsRoomBorder)
                    //if (map.IsAir(n))
                    {
                        if (map.IsAboveHeightMap(n))
                            return null;
                        queue.Enqueue(n);
                        area.Add(n);
                    }
                   
                }
            }
            return area;
        }
        //public string GetName()
        //{
        //    return string.Format("Room {0}", this.ID);
        //}

        //public void GetSelectionInfo(UISelectedInfo panel)
        //{
        //    //throw new NotImplementedException();
        //}

        //public void GetQuickButtons(UISelectedInfo panel)
        //{
        //    //throw new NotImplementedException();
        //}
        //static public IEnumerable<Vector3> BeginExclusiveLazy(IMap map, Vector3 global)
        //{
        //    //var area = new EnclosedArea();
        //    yield return global;
        //    var queue = new Queue<Vector3>();
        //    var handled = new HashSet<Vector3>() { global };
        //    queue.Enqueue(global);
        //    while (queue.Any())
        //    {
        //        var current = queue.Dequeue();
        //        foreach (var n in current.GetAdjacentLazy())
        //        {
        //            if (handled.Contains(n))
        //                continue;
        //            handled.Add(n);

        //            if (!map.Contains(n))
        //                continue;
        //            if (map.IsAboveHeightMap(n))
        //                continue;
        //                if (map.IsAir(n))
        //                {
        //                    queue.Enqueue(n);
        //                    area.Add(n);
        //                }

        //            else
        //            {
        //                return null;
        //            }
        //        }
        //    }
        //    return area;
        //}


    }
}
