using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class SceneState
    {
        public HashSet<GameObject> ObjectsDrawn { get; set; }
        public Dictionary<GameObject, Rectangle> ObjectBounds { get; set; }
        //public PlayerControl.SelectionRectangle Selection { get; set; }
        public SceneState()
        {
            this.ObjectBounds = new Dictionary<GameObject, Rectangle>();
            this.ObjectsDrawn = new HashSet<GameObject>();
        }
    }
}
