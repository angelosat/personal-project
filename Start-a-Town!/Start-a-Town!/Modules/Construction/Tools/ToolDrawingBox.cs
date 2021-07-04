﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Modules.Construction
{
    class ToolDrawingBox : ToolDrawingWithHeight
    {
        public override string Name => "Box";
        public ToolDrawingBox()
        {

        }
        public ToolDrawingBox(Action<Args> callback)
            : base(callback)
        {
        }
        
        protected override void DrawGrid(MySpriteBatch sb, IMap map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;
            var end = this.End + Vector3.UnitZ * this.Height;

            var box = this.Begin.GetBox(end);

            cam.DrawGridBlocks(sb, Block.BlockBlueprint, box, color);
        }
        public override List<Vector3> GetPositions()
        {
            return GetPositions(this.Begin, this.TopCorner);
        }
        static public List<Vector3> GetPositions(Vector3 a, Vector3 b)
        {
            VectorHelper.GetMinMaxVector3(a, b, out Vector3 min, out Vector3 max);
            var dx = max.X - min.X;
            var dy = max.Y - min.Y;
            var dz = max.Z - min.Z;
            if (dx <= 1 || dy <= 1 || dz <= 1)
                return min.GetBox(max);
            else
                return min.GetBox(max).Except((min + Vector3.One).GetBox(max - Vector3.One)).ToList();
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, IMap map, Camera camera, Net.PlayerData player)
        {
            this.DrawGrid(sb, map, camera, Color.Red);
        }
        protected override void WriteData(System.IO.BinaryWriter w)
        {
            base.WriteData(w);
            w.Write(this.SettingHeight);
            w.Write(this.Height);
        }
        protected override void ReadData(System.IO.BinaryReader r)
        {
            base.ReadData(r);
            this.SettingHeight = r.ReadBoolean();
            this.Height = r.ReadInt32();
        }

        public override ToolDrawing.Modes Mode
        {
            get { return Modes.Box; }
        }
    }
}
