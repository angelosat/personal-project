using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    /// <summary>
    /// Gives the object the behaviour of a fluid block.
    /// </summary>
    public class FluidComponent : Component// BlockComponent
    {
        public override string ComponentName
        {
            get { return "Fluid"; }
        }

        static float FluidSpeed = 1;//5;


        //FluidComponent(Block.Types type, float transparency, float density)
        //{
        //    this.Type = type;
        //    this.Transparency = transparency;
        //    this.Density= density;
        //    Properties[Stat.Timer.Name] = Density * FluidSpeed * 60f;
        //}

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk)
        {
            var map = net.Map;
            float value = GetProperty<float>(Stat.Timer.Name) - 1;
            Properties[Stat.Timer.Name] = value;
            if (value <= 0)
            {

                Vector3 global = parent.Global;
                Vector3 w, e, n, s, d; //, u
                w = new Vector3(global.X - 1, global.Y, global.Z);
                e = new Vector3(global.X + 1, global.Y, global.Z);
                n = new Vector3(global.X, global.Y - 1, global.Z);
                s = new Vector3(global.X, global.Y + 1, global.Z);
                //      u = new Vector3(global.X, global.Y, global.Z + 1);
                d = new Vector3(global.X, global.Y, global.Z - 1);

                Cell cell;
                Chunk thischunk;
                //if it can flow downwards, don't check the same height neighbors
                //if (Position.TryGet(map, d, out cell, out thischunk))
                if (map.TryGetAll(d, out thischunk, out cell))
                {
                    if (cell.Block.Type == Block.Types.Air || cell.Block.Type == Block.Types.Water)
                    {
                        //cell.Type = Block.Types.Water;
                        cell.SetBlockType(Block.Types.Water);
                        //Chunk.Show(thischunk, cell);
                        thischunk.InvalidateCell(cell);
                   //     ChunkLighter.Enqueue(cell.GetGlobalCoords(thischunk));
                        GameObject tileObj = GameObject.Create(BlockComponent.Blocks[Block.Types.Water].Entity.ID);
                        Chunk.AddObject(tileObj, map, d);

      
                        //parent.Remove(e.net);
                        net.SyncDisposeObject(parent);
                        return;
                    }
                    //Position.AddObject(GameObjectDb.Water, check);
                }

                List<Vector3> neighbors = new List<Vector3>() {w, e, s, n};

                
                bool hasSpread = false;
                foreach (Vector3 adjacent in neighbors)
                {
                    //if (Position.TryGet(map, adjacent, out cell, out thischunk))
                    if (map.TryGetAll(adjacent, out thischunk, out cell))
                    {
                        if (cell.Block.Type == Block.Types.Air)
                        {
                            //cell.Type = Block.Types.Water;
                            cell.SetBlockType(Block.Types.Water);
                            thischunk.InvalidateCell(cell);

                        //    ChunkLighter.Enqueue(cell.GetGlobalCoords(thischunk));
                            GameObject tileObj = GameObject.Create(BlockComponent.Blocks[Block.Types.Water].Entity.ID);
                            Chunk.AddObject(tileObj, map, adjacent);
                            // TODO: fix the removing of sprite component
                            tileObj.Components.Remove("Sprite");
                           // tileObj.Spawn();
                            hasSpread = true;
                        }
                        //Position.AddObject(GameObjectDb.Water, check);
                    }
                }

                // Cell.Variation = 0;
                if (!hasSpread)
                    net.Despawn(parent);
                     //parent.Remove();
                else
                    Properties[Stat.Timer.Name] = GetProperty<float>(Stat.Density.Name) * FluidSpeed;
            }
        }
        public override object Clone()
        {
            FluidComponent comp = new FluidComponent();//this.Type, this.Transparency, this.Density);
            foreach (KeyValuePair<string, object> parameter in Properties)
                comp[parameter.Key] = parameter.Value;
            return comp;
        }

        static public FluidComponent Create(GameObject obj, Block.Types type, float transparency, float density)//, int density = 7)
        {
            return new FluidComponent();
            //BlockComponent.Create(obj, type, hasData: true, transparency: transparency, density: density);
            //return new FluidComponent(type, transparency, density);
        }
    }
}
