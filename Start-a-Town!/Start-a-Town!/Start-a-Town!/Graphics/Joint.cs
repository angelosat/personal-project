using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Graphics
{
    public class Joint
    {
        public float Angle;
        public Bone Parent;
        //public Bone Bone;
        Bone _Bone;
        public Bone Bone
        {
            //get { return this.BoneGetter() ?? this._Bone; }
            get
            {
                if (this.Slot != null)
                    if (this.Slot.Object != null)
                        return this.Slot.Object.Body;
                return this._Bone;
            }
            set { this._Bone = value; }
        }
        public Vector2 Position = Vector2.Zero;
        //public Func<GameObject, Bone> BoneGetter = (entity) => null;
        public Func<GameObject, GameObjectSlot> SlotGetter = (entity) => null;
        GameObjectSlot Slot;
        public void MakeChildOf(GameObject parent)
        {
            this.Slot = SlotGetter(parent);
        }

        public Joint()
        {

        }
        public Joint(Vector2 pos)
        {
            this.Position = pos;
        }
        public Joint(int x, int y)
        {
            this.Position = new Vector2(x, y);
        }
        public Joint(Joint joint)
        {
            this.Angle = joint.Angle;
            this.Parent = joint.Parent;
            this.Position = joint.Position;
            //if (joint.Bone != null)
            //    this.Bone = Bone.Copy(joint.Bone);
            if (joint._Bone != null)
                this._Bone = Bone.Copy(joint._Bone);
            this.SlotGetter = joint.SlotGetter;
        }
        public void SetBone(Bone bone)
        {
            if(this.Bone != null)
            {
                this.Bone.Joint = null;
                this.Bone.Parent = null;
            }
            bone.Joint = this;
            bone.Parent = this.Parent;
            this.Bone = bone;
        }
        public void Update()
        {
            if (this.Bone != null)
                this.Bone.Update();
        }

        //internal Vector2 GetFinalPosition()
        //{
        //    var pos = this.Position;
        //    var current = this;
        //    var parent = this.Parent;
        //    while (parent != null)
        //    {
        //        pos += parent.ParentJoint;
        //        parent = parent.Parent;
        //    }
        //    return pos;
        //}
    }
}
