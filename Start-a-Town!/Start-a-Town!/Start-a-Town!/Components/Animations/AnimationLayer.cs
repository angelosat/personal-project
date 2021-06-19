﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Graphics;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Animations
{
    public enum AnimationBlending { Override, Additive }

    class AnimationLayer : List<Animation>
    {
        public string Name { get; set; }
        public AnimationBlending Blending { get; set; }
        public float Priority { get; set; }
      //  public List<Animation> Animations { get; set; }

        public AnimationLayer()
        {
            this.Priority = 0;
            this.Name = "unnamed_layer";
            this.Blending = AnimationBlending.Override;
          //  this.Animations = new List<Animation>();
        }

        public void Update(Bone body)
        {
            foreach (var ani in this)//.Animations)
                ani.Update(body);
        }
    }
}
