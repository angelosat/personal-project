using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Blocks
{
    //public abstract class BlockEntity : Component, ICloneable
    public abstract class BlockEntity : ICloneable, IDisposable//, IHasChildren
    {
        public virtual void Update(IObjectProvider net, Vector3 global) { }
        public virtual void GetTooltip(UI.Control tooltip) { }
        public abstract object Clone();
        /// <summary>
        /// Dipose any children GameObjects here.
        /// </summary>
        public virtual void Dispose() { } // maybe make this abstract so i don't forget it?
        public virtual void Remove(IMap map, Vector3 global) { }
        public virtual void Break(IMap map, Vector3 global) { }

        //public Vector3 Global { get; }
        public virtual GameObjectSlot GetChild(string containerName, int slotID) { return null; }
        public virtual List<GameObjectSlot> GetChildren() { return new List<GameObjectSlot>(); }

        public virtual SaveTag Save(string name) 
        {
            return null; 
            //return new SaveTag(SaveTag.Types.Compound, name);
        }
        public virtual void Load(SaveTag tag) { }
        public virtual void Write(BinaryWriter w) { }
        public virtual void Read(BinaryReader r) { }

        internal virtual void HandleRemoteCall(GameModes.IMap map, Vector3 vector3, ObjectEventArgs e) { }

        internal void Instantiate(Action<GameObject> instantiator)
        {
            foreach (var entity in this.GetChildren())
                entity.Instantiate(instantiator);
        }

        public virtual void Draw(Camera camera, IMap map, Vector3 global) { }
        public virtual void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera cam, Vector3 global) { }
    }
}
