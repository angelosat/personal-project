﻿using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class HitPoints : ResourceDef
    {
        public HitPoints()
            : base("HitPoints")
        {
            this.AddThreshold("Hit points");
        }

        private const string _description = "Hit Points";
        private const string _format = "##0";//.00";
        public override string Format => _format;
        public override string Description => _description; 

        public override Color GetBarColor(Resource resource)
        {
            return Color.SeaGreen;
        }
    }
}
