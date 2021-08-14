﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Modules.Construction
{
    class ToolDrawingBoxFilled : ToolDrawingWithHeight
    {
        public override string Name { get; } = "Box Filled";
        public override Modes Mode { get; } = Modes.BoxFilled; 
        public ToolDrawingBoxFilled()
        {

        }
        public ToolDrawingBoxFilled(Action<Args> callback)
            : base(callback)
        {
        }
        
        protected override void DrawGrid(MySpriteBatch sb, MapBase map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;
            var end = this.End + IntVec3.UnitZ * this.Height;

            var box = this.Begin.GetBox(end);

            cam.DrawGridBlocks(sb, Block.BlockBlueprint, box, color);
        }
        public override IEnumerable<IntVec3> GetPositions()
        {
            foreach (var i in GetPositions(this.Begin, this.TopCorner))
                yield return i;
        }
        static public List<IntVec3> GetPositions(IntVec3 a, IntVec3 b)
        {
            var box = a.GetBox(b);
            return box;
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, Net.PlayerData player)
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

    }
}
