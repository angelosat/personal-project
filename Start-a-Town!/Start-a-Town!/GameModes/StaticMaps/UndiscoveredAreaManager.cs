using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Start_a_Town_
{
    class UndiscoveredAreaManager
    {
        IMap Map;
        public UndiscoveredAreaManager(IMap map)
        {
            this.Map = map;

        }
        bool Valid;// = true;
        public void Init()
        {
            if (this.Map.Net is Net.Client)
                return; // if i'm saving the discovered property of cells, it means that i load and send the data to clients. no need for clients to initialize undiscovered areas themselves
            if (!this.Valid)
            {
                var watch = Stopwatch.StartNew();
                this.FloodFill(this.Map);
                watch.Stop();
                $"undiscovered areas initialized in {watch.ElapsedMilliseconds} ms".ToConsole();
            }
            this.Valid = true;
        }
        public void FloodFill(IMap map)
        {
            var current = new Vector3(0, 0, map.MaxHeight - 1);
            this.FloodFill(map, current, true);
        }
        public void FloodFill(IMap map, Vector3 begin, bool value)
        {
            var current = begin;
            var cell = map.GetCell(current);
            if (cell.Discovered == value)
                throw new Exception(); // was a test
            cell.Discovered = value;
            map.GetChunk(begin).InvalidateSlice(begin.Z);
            if (cell.Block != BlockDefOf.Air)
                return;
            var tohandle = new Queue<Vector3>();
            tohandle.Enqueue(current);
            while (tohandle.Any())
            {
                current = tohandle.Dequeue();
                foreach(var n in current.GetAdjacentLazy())
                {
                    if (map.TryGetCell(n, out Cell ncell))
                    {
                        if (!ncell.Discovered)
                        {
                            ncell.Discovered = true;
                            map.GetChunk(n).InvalidateSlice(n.Z);
                            if(!ncell.IsRoomBorder)
                                tohandle.Enqueue(n);
                        }
                    }
                    else
                    {

                    }
                }
            }
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Valid);
        }
        public UndiscoveredAreaManager Read(BinaryReader r)
        {
            this.Valid = r.ReadBoolean();
            return this;
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Valid.Save(tag, "Valid");
            return tag;
        }
        internal void Load(SaveTag tag)
        {
            tag.TryGetTagValueNew("Valid", ref this.Valid);
        }

        internal void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.BlocksChanged:
                    foreach (var pos in e.Parameters[1] as IEnumerable<Vector3>)
                        Handle(e.Parameters[0] as IMap, pos);
                    break;

                case Components.Message.Types.BlockChanged:
                    Handle(e.Parameters[0] as IMap, (Vector3)e.Parameters[1]);
                    break;

                default:
                    break;
            }
        }

        private void Handle(IMap map, Vector3 global)
        {
            if (map.TryGetCell(global, out Cell gc))
                if (!gc.Discovered)
                    return;
            foreach (var n in global.GetNeighbors())
            {
                if (map.TryGetCell(n, out Cell nc))
                {
                    if (!nc.Discovered)
                    {
                        this.FloodFill(map, n, true);
                    }
                }
            }
        }
        
        public bool IsUndiscovered(Vector3 global)
        {
            return !this.Map.GetCell(global).Discovered;
        }
    }
}
