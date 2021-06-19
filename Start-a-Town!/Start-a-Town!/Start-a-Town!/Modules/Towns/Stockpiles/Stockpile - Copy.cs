using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.AI;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Towns
{
    public class Stockpile : Zone
    {
        public int ID;
        //public Vector3 Begin, End;
        //public int Width, Height;
        public Town Town;
        public string Name { get { return "Stockpile " + this.ID.ToString(); } }

        bool Valid;

        Dictionary<int, int> Inventory = new Dictionary<int, int>();
        List<GameObject> Contents = new List<GameObject>();
        List<Vector3> WatchedPositions = new List<Vector3>();
        List<Vector3> ReservedSpots = new List<Vector3>();
        List<Vector3> OwnedPositions = new List<Vector3>();

        void PopulateOwnedPositions()
        {
            var list = new List<Vector3>();
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            int z = (int)this.Begin.Z;
            for (int i = x; i < x + this.Width; i++)
                for (int j = y; j < y + this.Height; j++)
                {
                    Vector3 pos = new Vector3(i, j, z);
                    this.OwnedPositions.Add(pos);
                }
        }

         public List<GameObject> GetContents()
        {
            return this.Contents.ToList();
        }

        static readonly int UpdateTimerMax = Engine.TargetFps;
        int UpdateTimer = UpdateTimerMax;

        #region Constructors
        public Stockpile(Vector3 begin, int w, int h)
        {
            this.Begin = begin;
            this.End = begin + new Vector3(w, h, 0);
            this.Width = w;
            this.Height = h;
            this.PopulateOwnedPositions();
            //town.AddStockpile(this);
        }

        public Stockpile(BinaryReader r)
        {
            this.Begin = r.ReadVector3();
            this.Width = r.ReadInt32();
            this.Height = r.ReadInt32();
            this.End = Begin + new Vector3(Width, Height, 0);
            this.PopulateOwnedPositions();
        }
        #endregion

        public void Invalidate()
        {
            this.Valid = false;
            //this.WatchedCells.Clear();
        }
        public void Validate()
        {
            this.WatchedPositions.Clear();
        }

        public void Update()
        {
            //this.UpdateTimer--;
            //if(this.UpdateTimer<=0)
            //{
            //    this.UpdateTimer = UpdateTimerMax;
                this.UpdateContents();
            //}
        }

        private void UpdateContents()
        {
            var newList = this.QueryPositions();
            var newObjects = newList.Except(this.Contents);
            // todo: optimize this?
            var removedObjects = this.Contents.Except(newList);

            //bool changed = false;
            //var newInventory = this.GetInventory();
            //    foreach(var item in newInventory)
            //    {
            //        int existing;
            //        if (this.Inventory.TryGetValue(item.Key, out existing))
            //            changed |= (existing != item.Value);
            //        else
            //            changed = true;
            //    }
            OnContentsUpdated();

            // do something with new and removed objects?
            // i also have to update UI when stacksizes change
            //if ((newObjects.Count() == removedObjects.Count()) == 0)
            //    this.Town.UITownWindow.StockpileUI.Refresh();

            this.Contents = newList;
        }
        public List<GameObject> QueryPositions()
        {
            return Town.Map.GetObjects(this.Begin, this.End);
        }
        public Dictionary<int, int> GetInventory()
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            var contents = this.Contents;
            foreach (var item in contents)
                dic.AddOrUpdate(item.GetInfo().ID, item.StackSize, (key, value) => value += item.StackSize);
            return dic;
        }

        /// <summary>
        /// TODO: optimize: call this only when contents changed
        /// </summary>
        void OnContentsUpdated()
        {
            // signal UI here
            //this.Town.UITownWindow.StockpileUI.Refresh();
        }
        //public void QueryPositions()
        //{
        //    this.Contents.Clear();
        //    this.Contents = Town.Map.GetObjects(this.Global, this.End);
        //}

        public void DrawWorld(MySpriteBatch sb, Map map, Camera cam)
        {
            var gridSprite = Sprite.BlockFaceHighlights[Vector3.UnitZ];
            Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;// gridSprite.AtlasToken.Atlas.Texture;
            var fx = Game1.Instance.Content.Load<Effect>("blur");
            fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            var col = Color.Yellow;// this.Valid ? Color.Lime : Color.Red;
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);

            for (int i = x; i < x + this.Width; i++)
                for (int j = y; j < y + this.Height; j++)
                {
                    Vector3 global = new Vector3(i, j, this.Begin.Z);
                    //global.Draw(sb, cam, gridSprite.AtlasToken, col);
                    gridSprite.Draw(sb, global, cam, col);

                    //var bounds = cam.GetScreenBounds(global, Block.Bounds);
                    //var pos = new Vector2(bounds.X, bounds.Y);
                    //var depth = global.GetDrawDepth(Engine.Map, cam);
                    //sb.Draw(Sprite.Atlas.Texture, pos, gridSprite.AtlasToken.Rectangle, 0, Vector2.Zero, cam.Zoom, col, SpriteEffects.None, depth);
                }
        }

        public void CreateJob(GameObject toInsert)
        {
            TargetArgs freeSpot = new TargetArgs(this.Begin);
            TargetArgs toInsertTarget = new TargetArgs(toInsert);
            AIJob job = new AIJob();
            //job.Instructions.Add(new AIInstruction(toInsertTarget, new PickUp()));

            //job.Instructions.Enqueue(new AIInstruction(toInsertTarget, new PickUp()));
            job.AddStep(new AIInstruction(toInsertTarget, new PickUp()));

        }



        internal bool Accepts(GameObject entity)
        {
            return true;
        }

        internal bool Accepts(GameObject entity, out Vector3 freeSpot)
        {
            var spots = this.GetFreeSpots();
            freeSpot = spots.FirstOrDefault();
            //return freeSpot != null;
            return spots.Count > 0;
            
            //if (spots.Count == 0)
            //    return false;
            //freeSpot = spots.First();
            //return true;
        }
        internal bool TryDeposit(GameObject entity, out TargetArgs target)
        {
            var existing = this.Contents.Where(o => o.ID == entity.ID).FirstOrDefault(o=>o.StackSize + entity.StackSize <= o.StackMax);
            if(existing!=null)
            {
                target = new TargetArgs(existing);
                return true;
            }

            var spots = this.GetFreeSpots();
            target = new TargetArgs(spots.FirstOrDefault());
            return spots.Count > 0;
        }
        List<Vector3> GetFreeSpots()
        {
            //var list = new List<Vector3>();
            //int x = (int)Math.Min(this.Global.X, this.End.X);
            //int y = (int)Math.Min(this.Global.Y, this.End.Y);
            //int z = (int)this.Global.Z;
            //for (int i = x; i < x + this.Width; i++)
            //    for (int j = y; j < y + this.Height; j++)
            //    {
            //        Vector3 pos = new Vector3(i, j, z);
            //        if (this.ReservedSpots.Contains(pos))
            //            continue;
            //        if(this.Town.Map.IsEmpty(pos))
            //        {
            //            list.Add(pos);
            //        }
            //    }
            //return list;

            var list = new List<Vector3>();
            foreach(var pos in this.OwnedPositions)
            {
                if (this.ReservedSpots.Contains(pos))
                    continue;
                if (this.Town.Map.IsEmpty(pos))
                    list.Add(pos);
            }
            return list;
        }

        public bool ReserveSpot(Vector3 spot)
        {
            if (!this.OwnedPositions.Contains(spot))
                return false;
            if (this.ReservedSpots.Contains(spot))
                return false;
            this.ReservedSpots.Add(spot);
            return true;
        }
        public void ClearReserved()
        {
            this.ReservedSpots.Clear();
        }
    }
}
