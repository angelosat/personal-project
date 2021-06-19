using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Animations;
using System.IO;

namespace Start_a_Town_
{
    public class Bone : ISaveable, ISerializable
    {
        public enum States { Unstarted, Stopped, Running, Finished, Manual }

        public States State;
        public Dictionary<BoneDef, Bone> Children;
      
        public Joint Joint;
        public Dictionary<BoneDef, Joint> Joints = new();
        public Bone AddJoint(BoneDef type, Joint joint)
        {
            this.Joints.Add(type, joint);
            joint.Parent = this;
            return this;
        }
        public Joint AddJoint(BoneDef type)
        {
            var joint = new Joint();
            this.Joints.Add(type, joint);
            joint.Parent = this;
            return joint;
        }
        public Bone AddJoint(Vector2 jointPos, Bone bone)
        {
            //var joint = this.AddJoint(bone.Type, new Joint(jointPos, bone));
            //joint.SetBone(bone);
            var joint = new Joint(jointPos);
            joint.SetBone(bone);
            joint.Parent = this;
            this.Joints.Add(bone.Def, joint);
            return this;
        }
        public Joint GetJoint(BoneDef type)
        {
            return this.Joints.GetValueOrDefault(type);
        }

        public Bone AddBone(Bone bone)
        {
            //this.Children[bone.Type] = bone;
            //bone.Parent = this;
            var joint = this.GetJoint(bone.Def);
            joint.SetBone(bone);
            return this;
        }

        public Bone Parent;
        public Material Material;

        

        //Material _Material;
        //public Material Material
        //{
        //    get { return this.MaterialGetter?.Invoke() ?? this._Material; }
        //    set { this._Material = value; }
        //}
        //public Func<GameObject, Material> MaterialGetter;

        public Color Tint = Color.White;

        /// <summary>
        /// The coordinates on the bone's parent (relative to it's parent sprite origin) where the current bone attaches to.
        /// </summary>
        public Vector2 ParentJoint;
        //Func<GameObject, Sprite> SpriteGetter;

        //public Sprite[] Orientations = new Sprite[4];
        public List<Sprite> Orientations = new();

        public void SetOrientations(Sprite sprite1)
        {
            this.Orientations.Add(sprite1);
        }
        public void SetOrientations(Sprite sprite1, Sprite sprite2)
        {
            this.Orientations.Add(sprite1);
            this.Orientations.Add(sprite2);
        }
        public void SetOrientations(Sprite sprite1, Sprite sprite2, Sprite sprite3, Sprite sprite4)
        {
            this.Orientations.Add(sprite1);
            this.Orientations.Add(sprite2);
            this.Orientations.Add(sprite3);
            this.Orientations.Add(sprite4);
        }
        public Sprite GetSprite(Camera cam)
        {
            var index = (int)(cam.Zoom % this.Orientations.Count);
            return this.Orientations[index];
        }
        public Sprite GetSprite(int orientation)
        {
            if (this.Orientations.Count == 0)
                return null;
            var index = (int)(orientation % this.Orientations.Count);
            return this.Orientations[index];
        }

        public Sprite Sprite//;// { get; set; }
        {
            get { return this.Orientations.FirstOrDefault(); }
            set
            {
                this.Orientations.Clear();
                this.Orientations.Add(value);
            }
        }
        public Func<GameObject, Sprite> SpriteFunc;
        public Func<GameObject, GameObjectSlot> SlotFunc;

        GameObjectSlot SpriteSlot;
        public Keyframe RestingFrame;

        public SortedDictionary<float, List<AnimationClip>> Layers;
        public float Angle = 0;
        public Vector2 Offset = Vector2.Zero;

        //public float Scale = 1;
        //public Func<GameObject, float> ScaleFunc;
        //public float GetScale(GameObject parent)
        //{
        //    return this.ScaleFunc?.Invoke(parent) ?? this.Scale;
        //}

        float _Scale = 1;
        public float Scale
        {
            get { return this.ScaleFunc?.Invoke() ?? this._Scale; }
            set { this._Scale = value; }
        }
        public Func<float> ScaleFunc;
      

        public BoneDef Def;
        public float Order;
        bool Enabled = true;

        public void SetEnabled(bool value, bool passToChildren)
        {
            this.Enabled = value;
            if (!passToChildren)
                return;
            foreach (var c in this.Joints.Values)
                if (c.Bone != null)
                    c.Bone.SetEnabled(value, passToChildren);
        }

        class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }
        static readonly DescendingComparer<float> Descending = new();
        Bone()
        {
            this.SpriteFunc = parent => { return this.Sprite; };
            this.Order = 0;
            this.Angle = 0;
            this.Offset = Vector2.Zero;
            this.Layers = new SortedDictionary<float, List<AnimationClip>>(Descending);
            this.Children = new Dictionary<BoneDef, Bone>();
            this.RestingFrame = new Keyframe(10, Vector2.Zero, 0, Interpolation.Sine);
        }

        /// <summary>
        /// Creates a clone of an existing spritenode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        static public Bone Clone(Bone node, Bone parent = null)
        {
            Bone newnode = Bone.Create(node.Def, node.Sprite, parent, node.ParentJoint, node.Order);

            newnode.SpriteFunc = node.SpriteFunc;
            newnode.RestingFrame = node.RestingFrame;
            newnode.SlotFunc = node.SlotFunc;
            newnode.Offset = node.Offset;
            newnode.OriginGroundOffset = node.OriginGroundOffset;
            newnode.Tint = node.Tint;
            newnode.Material = node.Material;
            newnode.ScaleFunc = node.ScaleFunc;
            foreach (var child in node.Children)
                Clone(child.Value, newnode);
            foreach (var joint in node.Joints)
            {
                var j = new Joint(joint.Value);
                
                newnode.AddJoint(joint.Key, j);
            }

            newnode.Orientations.Clear();
            foreach (var i in node.Orientations)
                newnode.Orientations.Add(new Sprite(i));
            return newnode;
        }
        public Bone Clone(Bone parent = null)
        {
            Bone newnode = Bone.Create(this.Def, this.Sprite, parent, this.ParentJoint, this.Order);

            newnode.SpriteFunc = this.SpriteFunc;
            newnode.RestingFrame = this.RestingFrame;
            newnode.SlotFunc = this.SlotFunc;
            newnode.Offset = this.Offset;
            newnode.OriginGroundOffset = this.OriginGroundOffset;
            newnode.Tint = this.Tint;
            newnode.Material = this.Material;
            newnode.DrawMaterialColor = this.DrawMaterialColor;
            newnode.ScaleFunc = this.ScaleFunc;

            //foreach (var child in this.Children)
            //    Clone(child.Value, newnode);
            foreach (var joint in this.Joints)
            {
                var j = new Joint(joint.Value);
                newnode.AddJoint(joint.Key, j);
            }

            newnode.Orientations.Clear();
            foreach (var i in this.Orientations)
                newnode.Orientations.Add(new Sprite(i));
            return newnode;
        }
        public Bone this[BoneDef type]
        {
            get { return this.Joints[type].Bone; }
        }

        public void AddChild(BoneDef type, Bone spriteNode)
        {
            this.Children[type] = spriteNode;
            spriteNode.Parent = this;
        }
        public Bone AddChild(Bone bone)
        {
            this.Children[bone.Def] = bone;
            bone.Parent = this;
            return this;
        }
        public Bone(BoneDef type)
            : this()
        {
            this.Def = type;
        }
        public Bone(BoneDef type, Vector2 joint, float depth, params Bone[] children)
            : this(type)
        {
            this.Order = depth;
            this.ParentJoint = joint;
            foreach (var child in children)
            {
                child.Parent = this;
                this.Children[child.Def] = child;
            }
        }
        public Bone(BoneDef type, Sprite sprite, Vector2 joint, float depth, params Bone[] children)
            : this(type, sprite)
        {
            this.Order = depth;
            this.ParentJoint = joint;
            foreach (var child in children)
            {
                child.Parent = this;
                this.Children[child.Def] = child;
            }
        }
        
        public Bone(BoneDef type, Sprite sprite, float depth = 0)
            : this()
        {
            this.Def = type;
            this.Sprite = sprite == null ? sprite : new Sprite(sprite);
            this.ParentJoint = Vector2.Zero;
            this.Order = depth;
        }
        //public Bone(BoneDef type, Func<GameObject, Sprite> spriteGetter, float depth = 0)
        //    : this()
        //{
        //    this.Def = type;
        //    //this.SpriteGetter = spriteGetter;
        //    this.ParentJoint = Vector2.Zero;
        //    this.Order = depth;
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="joint"></param>
        /// <param name="depth">Depth in relation to parent bone.</param>
        public Bone(BoneDef type, Vector2 joint, float depth)
            : this()
        {
            this.Def = type;
            this.Order = depth;
            this.ParentJoint = joint;

        }

        static public Bone Create(BoneDef type, Sprite sprite, Bone parent, Vector2 joint, float depth, params Bone[] children)
        {
            Bone sprnd = new(type, sprite, joint, depth);
            if (parent != null)
                parent.AddChild(type, sprnd);

            foreach (var child in children)
                sprnd.Children[child.Def] = child;

            return sprnd;
        }
        static public Bone Create(BoneDef type, Bone parent, Vector2 joint, float depth, params Bone[] children)
        {
            Bone sprnd = new(type, joint, depth);
            if (parent != null)
                parent.AddChild(type, sprnd);

            foreach (var child in children)
                sprnd.Children[child.Def] = child;

            return sprnd;
        }
        static public Bone Create(BoneDef type, Sprite sprite)
        {
            return new Bone(type, sprite);
        }
        static public Bone Create(BoneDef type, Sprite sprite, Vector2 joint, float depth)
        {
            return new Bone(type, sprite) { Order = depth, ParentJoint = joint };
        }

        //float UpdateInterval = 1, UpdateLeftOver = 0;

        

        //public void UpdateAnimations(GameObject parent)
        //{
         
        //    var nextlayers = new SortedDictionary<float, List<AnimationClip>>();
        //    List<AnimationClip> nextAnimations;
        //    foreach (var layer in this.Layers)
        //    {
        //        nextAnimations = new List<AnimationClip>();
        //        foreach (var ani in layer.Value)
        //        {
        //            ani.Update(parent);
        //            if (!(ani.State == AnimationStates.Removed && ani.Weight <= 0))
        //                nextAnimations.Add(ani);
        //        }
        //        if (nextAnimations.Any())
        //            nextlayers[layer.Key] = nextAnimations;
        //    }
        //    this.Layers = nextlayers;
        //    foreach (var node in this.Joints)
        //        if (node.Value.Bone != null)
        //            node.Value.Bone.UpdateAnimations(parent);
        //}

        /// <summary>
        /// The difference vector between the sprite's origin and the ground, for root bones.
        /// </summary>
        public Vector2 OriginGroundOffset = Vector2.Zero;
        public bool DrawMaterialColor;
        //private void Evaluate(ref float availableWeight, ref float finalAngle, ref Vector2 finalOffset)
        //{
            
        //    foreach (var layer in this.Layers.Reverse())
        //    {
        //        var count = layer.Value.Count;
        //        for (int i = 0; i < count; i++)
        //        {
        //            var ani = layer.Value[i];
        //            var name = ani.Name;

        //            if (ani.Weight <= 0)
        //            {
        //                //ani.OnFadeOut();
        //                // let's try not removing animations with zero weight!
        //                //if (ani.State == Animation.States.Finished)
        //                //    continue;
        //            }
        //            else
        //            {
        //                float dang;
        //                Vector2 doff;
        //                ani.GetValue(out doff, out dang);
        //                finalAngle += dang * ani.Weight * availableWeight;
        //                finalOffset += doff * ani.Weight * availableWeight;
        //                availableWeight -= ani.Weight;
        //                availableWeight = Math.Max(0, availableWeight);
        //            }
        //        }
        //    }
        //}
        private void Evaluate(List<Animation> animations, ref float availableWeight, ref float finalAngle, ref Vector2 finalOffset)
        {
            var byLayer = animations.OrderBy(a => a.Layer);
            foreach (var ani in byLayer.Reverse())
            {
                //var count = layer.Value.Count;
                //for (int i = 0; i < count; i++)
                //{
                //    var ani = layer.Value[i];
                //var name = ani.Name;

                if (ani.Weight <= 0)
                {
                    //ani.OnFadeOut();
                    // let's try not removing animations with zero weight!
                    //if (ani.State == Animation.States.Finished)
                    //    continue;
                }
                else
                {
                    float dang = 0;
                    Vector2 doff = Vector2.Zero;
                    //ani.GetValue(this.Type, ref doff, ref dang);
                    if (ani.TryGetValue(this.Def, ref doff, ref dang))
                    {
                        finalAngle += dang * ani.Weight * availableWeight;
                        finalOffset += doff * ani.Weight * availableWeight;
                        availableWeight -= ani.Weight;
                        availableWeight = Math.Max(0, availableWeight);
                    }
                }
                //}
            }
        }
        
        
        [Obsolete]
        public void Draw(GameObject parent, SpriteBatch sb, Vector2 screenLoc, Color color, float scale, SpriteEffects sprFx, float depth)
        {
            Sprite sprite = SpriteSlot == null ? this.SpriteFunc(parent) : (SpriteSlot.HasValue ? SpriteSlot.Object.GetComponent<Components.SpriteComponent>().Sprite : null);
            if (sprite == null)
                return;
            Rectangle sourceRect = sprite.SourceRects[0][0];
            Vector4 shaderRect = new(sourceRect.X / (float)sprite.Texture.Width, sourceRect.Y / (float)sprite.Texture.Height, sourceRect.Width / (float)sprite.Texture.Width, sourceRect.Height / (float)sprite.Texture.Height);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(shaderRect);
            Vector2 origin = sprite.Joint;//.Origin;
            origin.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - origin.X - 1 : origin.X);
            if (Parent == null)
            {
                sb.Draw(sprite.Texture, screenLoc + this.Offset * scale, sourceRect, color, this.Angle, sprite.OriginGround, scale, sprFx, depth);
                return;
            }
            Vector2 joint = this.ParentJoint;
            joint.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);

            Vector2 parentJoint = Parent.GetJoint();
            float parentAngle = Parent.GetAngle();

            parentJoint.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
            float angle = GetAngle();
            angle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - angle) : angle);

            parentAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - parentAngle) : parentAngle);
            Vector2 off = GetOffset();
            Vector2 rotated = Coords.Rotate(parentAngle, joint);
            Vector2 final = (off + parentJoint + rotated);
            sb.Draw(sprite.Texture, screenLoc + final * scale, sourceRect, color, angle, origin, scale, sprFx, depth);
        }
        [Obsolete]
        public void Draw(SpriteBatch sb, Vector2 loc, Color color, SpriteEffects sprFx)
        {
            Rectangle source = Sprite.SourceRects[0][0];
            //Vector4 shaderRect = new Vector4(source.X / (float)Sprite.Texture.Width, source.Y / (float)Sprite.Texture.Height, source.Width / (float)Sprite.Texture.Width, source.Height / (float)Sprite.Texture.Height);
            //Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(shaderRect);
            if (Parent == null)
            {
                sb.Draw(Sprite.Texture, loc + this.Offset, source, color, this.Angle, Sprite.OriginGround, 1, sprFx, 0);
                return;
            }
            //sb.Draw(Sprite.Texture, loc + GetOffset() + Parent.GetJoint() + Coords.Rotate(Parent.GetAngle(), this.Joint), source, color, GetAngle(), Sprite.Origin, 1, sprFx, 0);
            sb.Draw(Sprite.Texture, loc + GetOffset() + Parent.GetJoint() - Parent.GetOrigin() + Coords.Rotate(Parent.GetAngle(), this.ParentJoint), source, color, GetAngle(), Sprite.OriginGround, 1, sprFx, 0);
            //sb.Draw(Sprite.Texture, loc + GetOffset() + Parent.GetJoint() - Parent.Sprite.Origin + Coords.Rotate(Parent.GetAngle(), this.Joint), source, color, GetAngle(), Sprite.Origin, 1, sprFx, 0);
        }
        


        public void DrawSorted(GameObject parent, SpriteBatch sb, Vector2 screenLoc, Color color, float scale, SpriteEffects sprFx, float depth)
        {
            var sortedNodes = new SortedList<float, Bone>();
            this.GetSorted(sortedNodes, this.Order);
            foreach (var node in sortedNodes)
                node.Value.Draw(parent, sb, screenLoc, color, scale, sprFx, depth + node.Key);//  node.Value.Order);
        }
        //public void DrawSorted(GameObject parent, MySpriteBatch sb, Vector2 screenLoc, Color color, float scale, SpriteEffects sprFx, float depth)
        //{
        //    var sortedNodes = new SortedList<float, Bone>();
        //    this.GetSorted(sortedNodes, this.Order);
        //    foreach (var node in sortedNodes)
        //        node.Value.Draw(parent, sb, screenLoc, color, scale, sprFx, depth);
        //}
        //public void Draw(GameObject parent, MySpriteBatch sb, Vector2 screenLoc, Color color, Vector2 loc, float angle, float scale, SpriteEffects sprFx, float alpha, float depth)
        //{
        //}
        [Obsolete]
        public void DrawTree(GameObject parent, MySpriteBatch sb, Vector2 screenLoc, Color color, Vector2 loc, float angle, float zoom, SpriteEffects sprFx, float alpha, float depth)
        {
            //Sprite sprite = SpriteSlot.IsNull() ? this.SpriteFunc(parent) : (SpriteSlot.HasValue ? SpriteSlot.Object.Avatar.Sprite : null);//.GetComponent<Components.ActorSpriteComponent>().Sprite : null);
            Sprite sprite = SpriteSlot == null ? this.Sprite : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);//.GetComponent<Components.ActorSpriteComponent>().Sprite : null);

            if (sprite == null)
                return;
            Rectangle sourceRect = sprite.GetSourceRect();//[this.Variation, this.Orientation];// sprite.SourceRects[0][0];
            Vector4 shaderRect = new(sourceRect.X / (float)sprite.Texture.Width, sourceRect.Y / (float)sprite.Texture.Height, sourceRect.Width / (float)sprite.Texture.Width, sourceRect.Height / (float)sprite.Texture.Height);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(shaderRect);

            float finalAngle = angle + this.Angle;
            finalAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - finalAngle) : finalAngle);
            var scale = zoom * this.Scale;
            //Vector2 joint = sprite.Origin;// sprite.Joint;
            Vector2 joint = this.Parent == null ? sprite.OriginGround : sprite.Joint;//.Origin;
            joint.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - joint.X - 1 : joint.X);

            //sprite.Draw(sb, screenLoc + loc * scale, color * sprite.Alpha, finalAngle, joint, scale, sprFx, depth + this.Order);
            //var finalColor = color * sprite.Alpha * alpha;
            var finalColor = color * alpha;
            sprite.Draw(sb, screenLoc + loc * scale, finalColor, finalAngle, joint, scale, sprFx, depth - this.Order);

            foreach (var child in this.Children.Values)
            {
                //Vector2 nextLoc = loc + child.Joint + child.Offset;
                //nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
                //nextLoc = Coords.Rotate(finalAngle, nextLoc);

                Vector2 nextLoc = child.ParentJoint + child.Offset;
                nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
                nextLoc = Coords.Rotate(finalAngle, nextLoc);
                nextLoc += loc;
                child.DrawTree(parent, sb, screenLoc, color, nextLoc, angle + this.Angle, scale, sprFx, alpha, depth - this.Order); // - Vector2.One*2*Graphics.Borders.Thickness
            }
        }
        [Obsolete]
        public void DrawTree(GameObject parent, MySpriteBatch sb, Vector2 screenLoc, Color color, Vector2 loc, float angle, float scale, SpriteEffects sprFx, float depth)
        {
            this.DrawTree(parent, sb, screenLoc, color, loc, angle, scale, sprFx, 1f, depth);
        }
        [Obsolete]
        public void DrawTree(GameObject parent, SpriteBatch sb, Vector2 screenLoc, Color color, Vector2 loc, float angle, float scale, SpriteEffects sprFx, float depth)
        {
            //Sprite sprite = SpriteSlot.IsNull() ? this.SpriteFunc(parent) : (SpriteSlot.HasValue ? SpriteSlot.Object.Avatar.Sprite : null);//.GetComponent<Components.ActorSpriteComponent>().Sprite : null);
            Sprite sprite = SpriteSlot == null ? this.Sprite : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);//.GetComponent<Components.ActorSpriteComponent>().Sprite : null);

            if (sprite is null)
                return;
            Rectangle sourceRect = sprite.GetSourceRect();//[this.Variation, this.Orientation];// sprite.SourceRects[0][0];
            Vector4 shaderRect = new(sourceRect.X / (float)sprite.Texture.Width, sourceRect.Y / (float)sprite.Texture.Height, sourceRect.Width / (float)sprite.Texture.Width, sourceRect.Height / (float)sprite.Texture.Height);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(shaderRect);

            float finalAngle = angle + this.Angle;
            finalAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - finalAngle) : finalAngle);

            //Vector2 joint = sprite.Origin;// sprite.Joint;
            Vector2 joint = this.Parent is null ? sprite.OriginGround : sprite.Joint;//.Origin;
            joint.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - joint.X - 1 : joint.X);

            //sb.Draw(sprite.Texture, screenLoc + loc * scale, sourceRect, color * sprite.Alpha, finalAngle, joint, scale, sprFx, depth + this.Order);
            sprite.Draw(sb, screenLoc + loc * scale, color * sprite.Alpha, finalAngle, joint, scale, sprFx, depth - this.Order);
            foreach (var child in this.Children.Values)
            {
                //Vector2 nextLoc = loc + child.Joint + child.Offset;
                //nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
                //nextLoc = Coords.Rotate(finalAngle, nextLoc);

                Vector2 nextLoc = child.ParentJoint + child.Offset;
                nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
                nextLoc = Coords.Rotate(finalAngle, nextLoc);
                nextLoc += loc;
                child.DrawTree(parent, sb, screenLoc, color, nextLoc, angle + this.Angle, scale, sprFx, depth - this.Order); // - Vector2.One*2*Graphics.Borders.Thickness
            }
        }

        /// <summary>
        /// legacy method for ui drawing (slots)
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="sb"></param>
        /// <param name="screenLoc"></param>
        /// <param name="sky"></param>
        /// <param name="block"></param>
        /// <param name="color"></param>
        /// <param name="fog"></param>
        /// <param name="angle"></param>
        /// <param name="scale"></param>
        /// <param name="sprFx"></param>
        /// <param name="alpha"></param>
        /// <param name="depth"></param>
        public void DrawTree(GameObject parent, SpriteBatch sb, Vector2 screenLoc, Color sky, Color block, Color color, Color fog, float angle, float scale, SpriteEffects sprFx, float alpha, float depth)
        {
            Sprite sprite = SpriteSlot == null ? this.Sprite : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);
            ////finalAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - finalAngle) : finalAngle);
            //float finalAngle = angle + this.Angle + (this.Joint != null ? this.Joint.Angle : 0);
            ////finalAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - finalAngle) : finalAngle);
            ////if (this.Joint != null)
            ////{
            ////    var jointangle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - this.Joint.Angle) : this.Joint.Angle);
            ////    finalAngle += jointangle;
            ////}
            //var flippedAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - finalAngle) : finalAngle);

            var nextangle = this.Angle + (this.Joint != null ? this.Joint.Angle : 0);
            nextangle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - nextangle) : nextangle);
            var finalAngle = angle + nextangle;

            Vector2 finalpos = screenLoc + this.Offset * scale;
            var finalDepth = depth - this.Order;
            if (sprite != null)
            {
                Rectangle sourceRect = sprite.GetSourceRect();

                Vector2 origin = this.Parent == null ? sprite.OriginGround : sprite.Joint;
                origin.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - origin.X - 1 : origin.X);

                //var finalColor = color * alpha;
                //var finalColor = color;
                //if (this.Material != null)
                //{
                //    finalColor = finalColor.Multiply(this.Material.Color).Multiply(this.Tint);
                //    finalColor.A = (byte)(this.Material.Type.Shininess * 255);
                //}

                sprite.Draw(sb, finalpos, color, 0, Vector2.Zero, scale, SpriteEffects.None, finalDepth);
            }
            //return;
            foreach (var child in this.Joints.Values)
            {
                var next = child.Bone;// GetBone(parent);
                //if (child.Bone == null)
                if (next == null)
                    continue;
                Vector2 nextLoc = child.Position;// child.ParentJoint;// +child.Offset;// -this.Offset;// +child.Offset;
                nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
                nextLoc = Coords.Rotate(finalAngle, nextLoc);
                //nextLoc += loc;
                //child.DrawTree(parent, sb, screenLoc, sky, block, color, fog, nextLoc, angle + this.Angle, scale, sprFx, alpha, depth - this.Order); // - Vector2.One*2*Graphics.Borders.Thickness
                next.DrawTree(parent, sb, finalpos + nextLoc * scale, sky, block, color, fog, finalAngle, scale, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness

            }
        }


        /// <summary>
        /// the good one
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="sb"></param>
        /// <param name="screenLoc"></param>
        /// <param name="sky"></param>
        /// <param name="block"></param>
        /// <param name="tint"></param>
        /// <param name="fog"></param>
        /// <param name="angle"></param>
        /// <param name="scale"></param>
        /// <param name="sprFx"></param>
        /// <param name="alpha"></param>
        /// <param name="depth"></param>
       public void DrawGhost(MySpriteBatch sb, Vector2 screenLoc, Color sky, Color block, Color tint, Color fog, float angle, float zoom, int orientation, SpriteEffects sprFx, float alpha, float depth)
        {
            Sprite sprite = SpriteSlot == null ? this.GetSprite(orientation) : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);
            var nextangle = this.Angle;// +(this.Joint != null ? this.Joint.Angle : 0);
            nextangle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - nextangle) : nextangle);
            var finalAngle = angle + nextangle;
            var scale = zoom * this.Scale;

            Vector2 finalpos = screenLoc + (this.Offset) * scale; // inject origingoundeffect from calling method?
            var finalDepth = depth - this.Order;
            if (sprite != null)
            {
                Rectangle sourceRect = sprite.GetSourceRect();
                Vector2 origin = sprite.OriginGround;
                origin.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - origin.X + 1 : origin.X);
                var finalTint = tint;
                sprite.Draw(sb, finalpos, this.Material, sky, block, finalTint, fog, finalAngle, origin, scale, sprFx, finalDepth);
            }
            foreach (var ch in this.Joints)
            {
                var child = ch.Value;
                var next = child.Bone;
                if (next == null)
                    continue;
                Vector2 nextLoc = child.Position;
                nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
                nextLoc = Coords.Rotate(finalAngle, nextLoc);
                var childAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - child.Angle) : child.Angle);
                next.DrawGhost(sb, finalpos + nextLoc * scale, sky, block, tint, fog, finalAngle + childAngle, scale, orientation, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness
            }
        }

        //public void DrawTreeAnimationDeltas(GameObject parent, MySpriteBatch sb, Vector2 screenLoc, Color sky, Color block, Color tint, Color fog, float angle, float zoom, int orientation, SpriteEffects sprFx, float alpha, float depth)
        //{
        //    Sprite sprite = SpriteSlot == null ? this.GetSprite(orientation) : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);
        //    float aniAngle = 0;
        //    Vector2 aniOffset = Vector2.Zero;
        //    float weight = 1;
        //    this.Evaluate(ref weight, ref aniAngle, ref aniOffset);
        //    var nextangle = this.Angle + aniAngle;
        //    nextangle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - nextangle) : nextangle);
        //    var finalAngle = angle + this.RestingFrame.Angle + nextangle;
        //    var scale = zoom * this.Scale;
        //    Vector2 finalpos = screenLoc + (this.RestingFrame.Offset + this.Offset + aniOffset) * scale;
        //    var finalDepth = depth - this.Order;
        //    if (this.Enabled && sprite != null)
        //    {
        //        Rectangle sourceRect = sprite.GetSourceRect();
        //        Vector2 origin = sprite.OriginGround;
        //        origin.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - origin.X + 1 : origin.X);
        //        var finalTint = tint;
        //        sprite.Draw(sb, finalpos, this.DrawMaterialColor ? this.Material : null, sky, block, finalTint, fog, finalAngle, origin, scale, sprFx, finalDepth);
        //    }
        //    foreach (var ch in this.Joints)
        //    {
        //        var child = ch.Value;
        //        var next = child.Bone;
        //        if (next == null)
        //            continue;
        //        Vector2 nextLoc = child.Position;
        //        nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
        //        nextLoc = Coords.Rotate(finalAngle, nextLoc);
        //        var childAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - child.Angle) : child.Angle);
        //        next.DrawTreeAnimationDeltas(parent, sb, finalpos + nextLoc * scale, sky, block, tint, fog, finalAngle + childAngle, scale, orientation, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness
        //    }
        //}


        public void DrawTreeAnimationDeltas(GameObject parent, CharacterColors customization, List<Animation> animations, MySpriteBatch sb, Vector2 screenLoc, Color sky, Color block, Color tint, Color fog, float angle, float zoom, int orientation, SpriteEffects sprFx, float alpha, float depth)
        {
            var material = this.Material;
            //var material = this.GetMaterial(parent);
            //var material = (parent as Entity).GetMaterial(this.Def);
            Sprite sprite = SpriteSlot == null ? this.GetSprite(orientation) : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);
            float aniAngle = 0;
            Vector2 aniOffset = Vector2.Zero;
            float weight = 1;
            this.Evaluate(animations, ref weight, ref aniAngle, ref aniOffset);
            var nextangle = this.Angle + aniAngle;
            nextangle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - nextangle) : nextangle);
            var finalAngle = angle + this.RestingFrame.Angle + nextangle;
            var scale = zoom * this.Scale;// this.GetScale(parent);// this.Scale;
            Vector2 finalpos = screenLoc + (this.RestingFrame.Offset + this.Offset + aniOffset) * scale;
            var finalDepth = depth - this.Order;
            if (this.Enabled && sprite != null)
            {
                Rectangle sourceRect = sprite.GetSourceRect();
                Vector2 origin = sprite.OriginGround;
                origin.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - origin.X + 1 : origin.X);
                var finalTint = tint;
                sprite.Draw(sb, customization, finalpos, this.DrawMaterialColor ? material : null, sky, block, finalTint, fog, finalAngle, origin, scale, sprFx, finalDepth);
            }
            foreach (var child in this.Joints.Values)
            {
                if (!child.TryGetBone(out var next, parent))// ref parent))
                    continue;
                Vector2 nextLoc = child.Position;
                nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
                nextLoc = Coords.Rotate(finalAngle, nextLoc);
                var childAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - child.Angle) : child.Angle);
                next.DrawTreeAnimationDeltas(parent, customization, animations, sb, finalpos + nextLoc * scale, sky, block, tint, fog, finalAngle + childAngle, scale, orientation, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness
            }
            //foreach (var child in this.Joints.Values)
            //{
            //    if(!child.TryGetBone(out var next, ref parent))
            //        continue;
            //    Vector2 nextLoc = child.Position;
            //    nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
            //    nextLoc = Coords.Rotate(finalAngle, nextLoc);
            //    var childAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - child.Angle) : child.Angle);
            //    next.DrawTreeAnimationDeltas(parent, animations, sb, finalpos + nextLoc * scale, sky, block, tint, fog, finalAngle + childAngle, scale, orientation, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness
            //}
        }
        public void DrawGhost(GameObject parent, MySpriteBatch sb, Vector2 screenLoc, Color sky, Color block, Color tint, Color fog, float angle, float zoom, int orientation, SpriteEffects sprFx, float alpha, float depth)
        {
            //Sprite sprite = SpriteSlot == null ? this.Sprite : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);
            Sprite sprite = SpriteSlot == null ? this.GetSprite(orientation) : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);

            //var nextangle = this.Angle + (this.Joint != null ? this.Joint.Angle : 0);
            var nextangle = this.Angle;// +(this.Joint != null ? this.Joint.Angle : 0);

            nextangle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - nextangle) : nextangle);
            var finalAngle = angle + nextangle;
            //finalAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - finalAngle) : finalAngle);
            var scale = zoom * this.Scale;

            //Vector2 finalpos = screenLoc + (this.Offset + (this.Parent == null ? this.OriginGroundOffset : Vector2.Zero)) * scale; // inject origingoundeffect from calling method?
            Vector2 finalpos = screenLoc + (this.Offset) * scale; // inject origingoundeffect from calling method?
            var finalDepth = depth - this.Order;
            if (sprite != null)
            {
                Rectangle sourceRect = sprite.GetSourceRect();
                Vector2 origin = sprite.OriginGround;
                origin.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - origin.X + 1 : origin.X);
                var finalTint = tint;
                //sprite.Draw(sb, finalpos, this.Material, sky, block, finalTint, fog, finalAngle, origin, scale, sprFx, finalDepth);
                sprite.Draw(sb, finalpos, this.DrawMaterialColor ? this.GetMaterial() : null, sky, block, finalTint, fog, finalAngle, origin, scale, sprFx, finalDepth);
            }
            //return;
            foreach (var ch in this.Joints)
            {
                var child = ch.Value;
                var next = child.Bone;// GetBone(parent);
                //if (child.Bone == null)
                if (next == null)
                    continue;
                Vector2 nextLoc = child.Position;// child.ParentJoint;// +child.Offset;// -this.Offset;// +child.Offset;
                nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
                nextLoc = Coords.Rotate(finalAngle, nextLoc);
                var childAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - child.Angle) : child.Angle);

                //next.DrawTree(parent, sb, finalpos + nextLoc * scale, sky, block, color, fog, finalAngle, scale, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness
                //next.DrawTree(parent, sb, finalpos + nextLoc * scale, sky, block, tint, fog, finalAngle + childAngle, scale, orientation, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness

                //var nextOrigin = next.GetOffset(this.Type);
                next.DrawGhost(parent, sb, finalpos + nextLoc * scale, sky, block, tint, fog, finalAngle + childAngle, scale, orientation, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness
                //next.DrawTree(parent, sb, ch.Key, finalpos + nextLoc * scale, sky, block, tint, fog, finalAngle + childAngle, scale, orientation, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness

            }
        }


        Material GetMaterial()
        {
            return this.Material;
            //return parent.GetComponent<Components.SpriteComponent>().TryGetMaterial(this.Def) ?? this.Material;
            //return parent.GetComponent<Components.SpriteComponent>().GetMaterial(this.Def);
        }

        float GetAngle()
        {
            return this.Angle + (Parent != null ? Parent.GetAngle() : 0);
        }
        Vector2 GetOffset()
        {
            return this.Offset + (Parent != null ? Parent.GetOffset() : Vector2.Zero);
        }
        Vector2 GetJoint()
        {
            return this.ParentJoint + (Parent != null ? Parent.GetJoint() : Vector2.Zero); //this.Sprite.Origin);// Vector2.Zero);
        }
        Vector2 GetOrigin()
        {
            //return this.Sprite.Origin + (Parent != null ? Parent.GetOrigin() : Vector2.Zero);
            return (Parent != null ? Parent.GetOrigin() : this.Sprite.OriginGround);
        }
        public void ForEach(Action<Bone> action)
        {
            var toHandle = new Queue<Bone>();
            toHandle.Enqueue(this);
            while (toHandle.Count > 0)
            {
                Bone bone = toHandle.Dequeue();
                action(bone);
                foreach (var sub in bone.Children)
                    toHandle.Enqueue(sub.Value);
            }
        }
        //public void GetSorted(SortedList<float, Bone> list)
        //{
        //    list.Add(Order, this);
        //    foreach (KeyValuePair<string, Bone> node in Children)
        //        node.Value.GetSorted(list);
        //}
        public void GetSorted(SortedList<float, Bone> list, float parentDepth)
        {
            list.Add(parentDepth + Order, this);
            foreach (KeyValuePair<BoneDef, Bone> node in Children)
                node.Value.GetSorted(list, parentDepth + Order);
        }
        static public Rectangle GetBounds(SortedList<float, Bone> list)
        {
            //  Rectangle bounds = new Rectangle(0, 0, 0, 0);
            var bounds = new Rectangle?();
            foreach (Bone node in list.Values)
            {
                Rectangle source = node.Sprite.SourceRects[0][0];
                //Vector2 offset = node.GetJoint() - node.Sprite.Origin - (node.Parent != null ? node.Parent.Sprite.Origin : Vector2.Zero);




                //Vector2 offset = node.GetJoint() - (node.Parent != null ? node.Parent.Sprite.Origin : Vector2.Zero); // - node.Sprite.Origin
                //offset -= new Vector2((float)Math.Cos(node.Angle) * node.Sprite.Origin.X - (float)Math.Sin(node.Angle) * node.Sprite.Origin.Y, (float)Math.Sin(node.Angle) * node.Sprite.Origin.X + (float)Math.Cos(node.Angle) * node.Sprite.Origin.Y);

                Vector2 offset = node.GetJoint() - node.GetOffset() - node.Sprite.OriginGround - (node.Parent != null ? node.Parent.Sprite.OriginGround : Vector2.Zero); // - node.Sprite.Origin 
                //Vector2 rotated = new Vector2((float)Math.Cos(node.Angle) * offset.X - (float)Math.Sin(node.Angle) * offset.Y, (float)Math.Sin(node.Angle) * offset.X + (float)Math.Cos(node.Angle) * offset.Y);
                //Vector2 size = new Vector2((float)Math.Cos(node.Angle) * source.Width - (float)Math.Sin(node.Angle) * source.Height, (float)Math.Sin(node.Angle) * source.Width + (float)Math.Cos(node.Angle) * source.Height);
                var nodeRect = new Rectangle((int)offset.X, (int)offset.Y, source.Width, source.Height);//(int)size.X, (int)size.Y);// source.Width, source.Height);
                bounds = !bounds.HasValue ? nodeRect : Rectangle.Union(bounds.Value, nodeRect);
            }
            return bounds.Value;
        }

        internal IEnumerable<Bone> GetAllBones()
        {
            yield return this;
            foreach (var j in this.Joints.Values)
            {
                var b = j.Bone;
                if (b != null)
                {
                    foreach (var a in b.GetAllBones())
                        yield return a;
                }
            }
        }

        public override string ToString()
        {
            string text = this.Def.ToString();
            foreach (var layer in this.Layers.Reverse())
                foreach (var ani in layer.Value)
                    text += "\n  " + layer.Key + ": " + ani.ToString();//.Name;
            foreach (var node in this.Joints.Values)
            {
                if (node.Bone != null)
                    text += "\n " + node.Bone.ToString();
            }
            return text;
        }


        //public void Start(AnimationClip keyframes)
        //{
        //    if (!this.Layers.TryGetValue(keyframes.Layer, out var list))
        //    {
        //        list = new List<AnimationClip>();
        //        this.Layers[keyframes.Layer] = list;
        //    }
        //    list.Add(keyframes);

        //    this.State = Bone.States.Unstarted;
        //}

        public void Stop()
        {
            //  this.Transition = null;

            foreach (KeyValuePair<BoneDef, Bone> node in Children)
                node.Value.Stop();
        }

        

        //public void Restart(bool children = true)
        //{
        //    this.Restart(foo => true, children);
        //    ////if (State == States.Finished)
        //    ////{
        //    //this.State = States.Unstarted;
        //    //this.Frame = 0;
        //    //this.Transition = null;
        //    ////  }
        //    //if (children)
        //    //    foreach (KeyValuePair<string, SpriteNode> node in Children)
        //    //    {
        //    //        node.Value.Restart(children);
        //    //        //this.Transition = null;
        //    //        //node.Value.Transition = null;
        //    //    }
        //}
        public void Restart(Func<Bone, bool> condition, bool children = true)
        {
            if (condition(this))
            {
                this.State = States.Unstarted;
                //this.Frame = 0;

            }
            if (children)
                foreach (KeyValuePair<BoneDef, Bone> node in Children)
                    node.Value.Restart(condition);
        }

        
        internal AnimationClip Find(string aniName, BoneDef type)
        {
            var toHandle = new Queue<Bone>();
            toHandle.Enqueue(this);
            while (toHandle.Count > 0)
            {
                Bone bone = toHandle.Dequeue();
                if (bone.Def == type)
                    foreach (var layer in bone.Layers)
                        foreach (var ani in layer.Value)
                            if (ani.Name == aniName)
                                return ani;
                foreach (var sub in bone.Children)
                    toHandle.Enqueue(sub.Value);
            }
            return null;
        }
        internal Bone Find(BoneDef type)
        {
            if (this.Children.TryGetValue(type, out Bone bone))
                return bone;
            foreach (var child in this.Children)
            {
                bone = child.Value.Find(type);
                if (bone != null)
                    return bone;
            }
            return bone;
        }
        internal Joint FindJoint(BoneDef type)
        {
            if (this.Joints.TryGetValue(type, out Joint joint))
                return joint;
            foreach (var j in this.Joints)
            {
                if (j.Value.Bone == null)
                    continue;
                joint = j.Value.Bone.FindJoint(type);
                if (joint != null)
                    return joint;
            }
            return joint;
        }
        internal Bone FindBone(BoneDef type)
        {
            if (this.Def == type)
                return this;
            return this.FindJoint(type)?.Bone?.FindBone(type);
            //return this.FindJoint(type).Bone;
        }
        internal bool TryFindBone(BoneDef t, out Bone b)
        {
            if (this.Def == t)
                b = this;
            else
                b = this.FindJoint(t)?.Bone;
            return b != null;
        }
        internal List<Bone> GetChildren()
        {
            var list = new List<Bone>() { this };

            foreach (var child in this.Joints.Values)
            {
                if (child.Bone != null)
                    list.AddRange(child.Bone.GetChildren());
            }
            return list;
        }

        public Sprite Render()
        {
            //RenderTarget2D tex = new RenderTarget2D(Game1.Instance.GraphicsDevice, 17, 38);
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;

            var sortedNodes = new SortedList<float, Bone>();
            //Rectangle bounds = new Rectangle(0, 0, 0, 0);
            GetSorted(sortedNodes, this.Order);//, ref bounds);
            var bounds = new Rectangle(0, 0, 50, 50);// GetBounds(sortedNodes);
            // bounds = new Rectangle(0, 0, bounds.Width + bounds.X, bounds.Height + bounds.Y);
            var tex = new RenderTarget2D(gfx, bounds.Width, bounds.Height);
            gfx.SetRenderTarget(tex);
            gfx.Clear(Color.Transparent);
            var sb = new SpriteBatch(gfx);
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            foreach (Bone node in sortedNodes.Values.Reverse())
                //  node.Draw(sb, Vector2.Zero, Color.White, 1, SpriteEffects.None, 0);
                node.Draw(sb, new Vector2(bounds.Width / 2, bounds.Height), Color.White, SpriteEffects.None);
            //node.Draw(sb, Vector2.Zero, Color.White, SpriteEffects.None);
            sb.End();
            gfx.SetRenderTarget(null);

            return new Sprite(tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, new Vector2(tex.Bounds.Width / 2 - 1, tex.Bounds.Height - 1));// tex;
        }



        internal Bone SetSprite(Start_a_Town_.Sprite sprite)
        {
            this.Sprite = sprite;
            return this;
        }

        internal void MakeChildOf(GameObject parent)
        {
            if (SlotFunc != null)
                SpriteSlot = SlotFunc(parent);
            foreach (var child in this.Children.Values)
                child.MakeChildOf(parent);
            foreach (var joint in this.Joints.Values)
            {
                if (joint.Bone != null)
                    joint.Bone.MakeChildOf(parent);

                joint.MakeChildOf(parent);
            }
        }

        static internal RenderTarget2D Render(GameObject parent)
        {
            var rect = parent.GetSprite().AtlasToken.Rectangle;
            var gd = Game1.Instance.GraphicsDevice;
            var mysb = new MySpriteBatch(gd);
            gd.Textures[0] = Sprite.Atlas.Texture;
            var tex = new RenderTarget2D(gd, rect.Width, rect.Height, false, SurfaceFormat.Color, DepthFormat.Depth16);
            gd.SetRenderTarget(tex);
            gd.Clear(Color.Transparent);
            Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            fx.Parameters["Viewport"].SetValue(new Vector2(rect.Width, rect.Height));
            //fx.CurrentTechnique = fx.Techniques["Default"];
            fx.CurrentTechnique = fx.Techniques["EntityMouseover"];

            //blur.CurrentTechnique = blur.Techniques["Entities"];
            Bone body = parent.Body;
            body.DrawTree(parent, mysb, new Vector2(rect.Width / 2, rect.Height), Color.White, body.ParentJoint + body.Offset, 0, 1, SpriteEffects.None, 0.5f);
            //gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true };
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            mysb.Flush();
            gd.SetRenderTarget(null);
            return tex;
        }

        
        internal void RenderNewererest(GameObject parent, RenderTarget2D texture)
        {
            // same as gameobject.drawicon
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;

            //var sprite = this.Sprite;
            var rect = texture.Bounds;
            gd.SetRenderTarget(texture);
            gd.Clear(Color.Transparent);

            //var loc = new Vector2(rect.X, rect.Y);
            var fx = Game1.Instance.Content.Load<Effect>("blur");
            var mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            fx.Parameters["Viewport"].SetValue(new Vector2(rect.Width, rect.Height));

            gd.Textures[0] = Sprite.Atlas.Texture;
            gd.Textures[1] = Sprite.Atlas.DepthTexture;
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            var body = this;
            var scale = 1;
            //var minrect = GetMinimumRectangle();

            //var grounddistancefromcenter = rect.Height / 2 + OriginGroundOffset.Y;
            var loc = new Vector2((int)((rect.Width) / 2), texture.Height/2);// (int)(sprite.OriginGround.Y));
            loc *= scale;
            body.DrawGhost(parent, mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);

            mysb.Flush();
        }
        internal RenderTarget2D RenderIcon(GameObject parent, float scale = 1)
        {
            var minrect = GetMinimumRectangle();
            // same as gameobject.drawicon
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            var w = (int)(minrect.Width * scale);
            var h = (int)(minrect.Height * scale);

            //var texture = new RenderTarget2D(gd, (int)(minrect.Width * scale), (int)(minrect.Height * scale));//, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            var texture = new RenderTarget2D(gd, w, h);//, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            //var sprite = this.Sprite;
            //var rect = texture.Bounds;
            gd.SetRenderTarget(texture);
            gd.Clear(Color.Transparent);

            //var loc = new Vector2(rect.X, rect.Y);
            var fx = Game1.Instance.Content.Load<Effect>("blur");


            var mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            fx.Parameters["Viewport"].SetValue(new Vector2(w, h));

            /// IMPORTANT!!! maybe that's why the unitframes aren't drawing at the start of the game?
            fx.Parameters["FarDepth"].SetValue(0);
            fx.Parameters["NearDepth"].SetValue(1);
            ///
            gd.SamplerStates[0] = SamplerState.PointWrap;
            gd.Textures[0] = Sprite.Atlas.Texture;
            gd.Textures[1] = Sprite.Atlas.DepthTexture;
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            var body = this;
            //var scale = 1;
            var customization = parent.GetComponent<Components.SpriteComponent>().Customization;
            var grounddistancefromcenter = texture.Height / 2 + OriginGroundOffset.Y * scale;// ( OriginGroundOffset.Y - 1) * scale;
            //loc = new Vector2((int)((rect.Width) / 2), (int)((rect.Height - minrect.Height) / 2)) + new Vector2(0, rect.Height / 2 + grounddistancefromcenter);// new Vector2(0, this.Sprite.Origin.Y);// -this.OriginGroundOffset * 2;
            var loc = new Vector2((int)(texture.Width / 2), texture.Height / 2 + grounddistancefromcenter);// (int)(sprite.OriginGround.Y));
            //loc *= scale;
            //body.DrawTree(parent, mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);
            body.DrawTreeAnimationDeltas(parent, customization, new List<Animation>() { new Animation(AnimationDef.Null) }, mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);

            mysb.Flush();
            return texture;
        }

       

        internal void RenderIcon(GameObject parent, RenderTarget2D texture, CharacterColors overlayColors = null)
        {
            var minrect = GetMinimumRectangle();
            // same as gameobject.drawicon
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            //gd.RasterizerState = RasterizerState.CullNone;

            //var texture = new RenderTarget2D(gd, (int)minrect.Width, (int)minrect.Height);
            //var sprite = this.Sprite;
            var rect = texture.Bounds; //minrect;// 
            gd.SetRenderTarget(texture);
            gd.Clear(Color.Transparent);//Red); //
            //var loc = new Vector2(rect.X, rect.Y);
            var fx = Game1.Instance.Content.Load<Effect>("blur");
            var mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            fx.Parameters["Viewport"].SetValue(new Vector2(rect.Width, rect.Height));

            ///
            ///

            gd.SamplerStates[0] = SamplerState.PointClamp;

            gd.Textures[0] = Sprite.Atlas.Texture;
            gd.Textures[1] = Sprite.Atlas.DepthTexture;
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            var body = this;
            var scale =  texture.Bounds.Width / minrect.Width;// 1;
            var customization = overlayColors ?? parent.GetComponent<Components.SpriteComponent>().Customization;

            var grounddistancefromcenter = rect.Height / 2 + OriginGroundOffset.Y * scale;// - 1;
            //loc = new Vector2((int)((rect.Width) / 2), (int)((rect.Height - minrect.Height) / 2)) + new Vector2(0, rect.Height / 2 + grounddistancefromcenter);// new Vector2(0, this.Sprite.Origin.Y);// -this.OriginGroundOffset * 2;
            var loc = new Vector2(rect.Width / 2, rect.Height / 2 + grounddistancefromcenter);// (int)(sprite.OriginGround.Y));
            //loc *= scale;
            //body.DrawTree(parent, mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);
            body.DrawTreeAnimationDeltas(parent, customization, new List<Animation>() { new Animation(AnimationDef.Null) }, mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);

            mysb.Flush();
        }
        internal void RenderNewerer(GameObject parent, RenderTarget2D texture)
        {
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;

            //var sprite = this.Sprite;
            var rect = texture.Bounds;
            gd.SetRenderTarget(texture);
            gd.Clear(Color.Transparent);

            //var loc = new Vector2(rect.X, rect.Y);
            var fx = Game1.Instance.Content.Load<Effect>("blur");
            var mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            fx.Parameters["Viewport"].SetValue(new Vector2(rect.Width, rect.Height));

            gd.Textures[0] = Sprite.Atlas.Texture;
            gd.Textures[1] = Sprite.Atlas.DepthTexture;
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            var body = this;
            var scale = 1;
            var minrect = GetMinimumRectangle();

            var grounddistancefromcenter = rect.Height / 2 + OriginGroundOffset.Y;
            var loc = new Vector2((int)((rect.Width) / 2), (int)((rect.Height - minrect.Height) / 2)) + new Vector2(0, rect.Height/2 + grounddistancefromcenter);// new Vector2(0, this.Sprite.Origin.Y);// -this.OriginGroundOffset * 2;
            loc *= scale;
            body.DrawGhost(parent, mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);

            mysb.Flush();
        }
        public void RenderNewer(GameObject parent, RenderTarget2D texture)
        {
            //if (parent.ID == GameObject.Types.Actor)
            //    return;
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;

            //var sprite = this.Sprite;
            //Rectangle rect = new Rectangle(0, 0, (int)viewport.X, (int)viewport.Y);//32, 64);// GetMinimumRectangle();// new Rectangle(3, 3, Width - 6, Height - 6);
            //Rectangle rect = GetMinimumRectangle();// new Rectangle(3, 3, Width - 6, Height - 6);
            var rect = texture.Bounds;
            //var texture = new RenderTarget2D(gd, rect.Width, rect.Height);//, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            gd.SetRenderTarget(texture);
            gd.Clear(Color.Transparent);

            //var loc = new Vector2(rect.X, rect.Y);
            var fx = Game1.Instance.Content.Load<Effect>("blur");
            var mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            //fx.Parameters["Viewport"].SetValue(new Vector2(this.Size.Width, this.Size.Height));
            fx.Parameters["Viewport"].SetValue(new Vector2(rect.Width, rect.Height));

            gd.Textures[0] = Sprite.Atlas.Texture;
            gd.Textures[1] = Sprite.Atlas.DepthTexture;
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            var body = this;
            var scale = 1;
            var minrect = GetMinimumRectangle();
            //loc -= sprite.Origin;
            
            //loc += new Vector2(0, minrect.Height / 2);
            //loc*=scale;
            //loc += new Vector2(rect.Width / 2, rect.Height / 2) +OriginGroundOffset;
            //loc = new Vector2(rect.Width, rect.Height);// -new Vector2(minrect.Width, minrect.Height);
            //loc /= 2;
            //loc = Vector2.Zero;
            //loc += new Vector2(rect.Width / 2, sprite.Origin.Y - OriginGroundOffset.Y);

            //loc = new Vector2(rect.Width / 2, this.Sprite.Origin.Y - OriginGroundOffset.Y + (rect.Height - minrect.Height) / 2);
            var loc = new Vector2((int)((rect.Width) / 2), (int)((rect.Height - minrect.Height) / 2)) + new Vector2(0, this.Sprite.OriginGround.Y) - this.OriginGroundOffset * 2;
            loc *= scale;
            body.DrawGhost(parent, mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);
            
            //var test = GameObject.Objects[40008];
            //minrect = test.Body.GetMinimumRectangle();
            //loc = new Vector2((int)((rect.Width) / 2), (int)((rect.Height - minrect.Height) / 2)) + new Vector2(0, test.Body.Sprite.Origin.Y) - test.Body.OriginGroundOffset;
            //test.Body.DrawTree(test, mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);


            mysb.Flush();
            //gd.SetRenderTarget(null);

        }
        void GetRectangle(List<Rectangle> rects)
        {
            if (this.Sprite != null)
                //rects.Add(this.GetRectangle());//this.Sprite.AtlasToken.Rectangle);
                rects.Add(this.GetRectangle(Vector2.Zero));

            //foreach (var node in this.Children)
            //    node.Value.GetRectangle(rects);
            foreach (var node in this.Joints)
                if (node.Value.Bone != null)
                    //node.Value.Bone.GetRectangle(rects);
                    node.Value.Bone.GetRectangle(node.Value.Position, rects);
        }
        void GetRectangle(Vector2 offset, List<Rectangle> rects)
        {
            if (this.Sprite != null)
                //rects.Add(this.GetRectangle());//this.Sprite.AtlasToken.Rectangle);
                rects.Add(this.GetRectangle(offset));

            //foreach (var node in this.Children)
            //    node.Value.GetRectangle(rects);
            foreach (var node in this.Joints)
                if (node.Value.Bone != null)
                    node.Value.Bone.GetRectangle(offset + node.Value.Position, rects);
        }
        public Rectangle GetMinimumRectangle()
        {
            var rects = new List<Rectangle>();
            this.GetRectangle(rects);
            //int minx = 0, miny = 0, maxx = 0, maxy = 0;

            var union = new Rectangle(0, 0, 0, 0);
            foreach (var rect in rects)
            {
                union = Rectangle.Union(union, rect);
            }
            return new Rectangle(0,0,union.Width, union.Height);
        }
        Rectangle GetRectangle(Vector2 offset)
        {
            var pos = offset + this.Offset - this.Sprite.OriginGround;
            return new Rectangle((int)pos.X, (int)pos.Y, this.Sprite.AtlasToken.Rectangle.Width, this.Sprite.AtlasToken.Rectangle.Height);
        }

        public List<Joint> Descendants(BoneDef type)
        {
            var list = new List<Joint>();
            if (this.Joints.TryGetValue(type, out Joint found))
                list.Add(found);
            foreach (var j in this.Joints.Values)
                if (j.Bone != null)
                    list.AddRange(j.Bone.Descendants(type));
            return list;
        }
        public Vector2 GetTotalOffset()
        {
            var parent = this.Parent;
            var offset = -this.Joint.Position;
            while (parent != null)
            {
                if (parent.Joint != null)
                {
                    offset -= parent.Joint.Position;
                    parent = parent.Joint.Parent;
                }
                else
                {
                    offset -= parent.OriginGroundOffset;
                    //parent = parent.Parent;
                    parent = null;
                }
            }
            return offset;
        }
        //public Vector2 GetTotalOffset()
        //{
        //    var parent = this.Parent;
        //    var offset = -this.Joint.Position;
        //    while (parent != null)
        //    {
        //        if (parent.Joint != null)
        //            offset -= parent.Joint.Position;
        //        else
        //            offset -= parent.OriginGroundOffset;
        //        parent = parent.Parent;
        //    }
        //    return offset;
        //}

        public void Write(BinaryWriter w)
        {
            w.Write(this.Material != null ? this.Material.ID : -1);

            foreach (var j in this.Joints.Values)
                j.Bone?.Write(w);
        }

        public ISerializable Read(BinaryReader r)
        {
            this.Material = Material.GetMaterial(r.ReadInt32());
            foreach (var j in this.Joints.Values)
                j.Bone?.Read(r);
            return this;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add((this.Material?.ID ?? -1).Save("MaterialID"));
            var tagjoints = new SaveTag(SaveTag.Types.Compound, "Joints");
            foreach (var joint in this.Joints)
            {
                if (joint.Value.Bone != null)
                {
                    var childTag = joint.Value.Bone.Save(joint.Key.Name);
                    tagjoints.Add(childTag);
                }
            }
            tag.Add(tagjoints);
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValue<int>("MaterialID", i => this.Material = Material.GetMaterial(i));
            tag.TryGetTag("Joints", t =>
            {
                foreach (var j in this.Joints)
                {
                    t.TryGetTag(j.Key.Name, jtag => j.Value.Bone.Load(jtag));
                }
            });
            return this;
        }

        public class Props
        {
            public Material Material;
            public Sprite Sprite;
            public float Scale;
        }
    }
}
