using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Animations
{
    public class Joint
    {
        public float Angle;
        public Bone Parent;
        //public Bone Bone;
        Bone _Bone;
        public Bone Bone
        {
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
        public Func<GameObject, GameObjectSlot> SlotGetter = (entity) => null;
        public Func<GameObject, GameObject> AttachmentFunc;

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
            if (joint._Bone != null)
            {
                this._Bone = joint._Bone.Clone();// Bone.Clone(joint._Bone);
                this._Bone.Parent = joint.Parent;
                this._Bone.Joint = this;
            }
            this.SlotGetter = joint.SlotGetter;
            this.AttachmentFunc = joint.AttachmentFunc;
        }
        public Joint Clone()
        {
            var j = new Joint
            {
                Angle = this.Angle,
                Parent = this.Parent,
                Position = this.Position
            };
            if (this._Bone != null)
            {
                j._Bone = this._Bone.Clone(this.Parent);
                this._Bone.Joint = this;
            }
            j.SlotGetter = this.SlotGetter;
            return j;
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

        internal GameObject GetAttachment(GameObject parent)
        {
            return this.AttachmentFunc?.Invoke(parent);
        }
        [Obsolete]
        internal bool TryGetBone(out Bone next, ref GameObject parent)
        {
            if (this.Bone != null)
            {
                next = this.Bone;
                return true;
            }
            else
            {
                var attachment = this.AttachmentFunc?.Invoke(parent);
                if(attachment != null)
                {
                    parent = attachment;
                    next = attachment.Body;
                    return true;
                }
            }
            next = null;
            return false;
        }
        internal bool TryGetBone(out Bone next, GameObject parent)
        {
            if (this.Bone != null)
            {
                next = this.Bone;
                return true;
            }
            else
            {
                var attachment = this.AttachmentFunc?.Invoke(parent);
                if (attachment != null)
                {
                    //parent = attachment;
                    next = attachment.Body;
                    return true;
                }
            }
            next = null;
            return false;
        }
    }
}
