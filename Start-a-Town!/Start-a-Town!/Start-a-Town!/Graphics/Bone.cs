using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components.Animations;
using Start_a_Town_.Components.Materials;

namespace Start_a_Town_.Graphics
{
    //public class BoneSlot
    //{
    //    public Bone Value { get; set; }
    //    public Vector2 Joint { get; set; }
    //    public float Depth { get; set; }
    //    public float Angle { get; set; }
    //    public BoneSlot(Vector2 joint, float angle, float depth)
    //    {
    //        this.Joint = joint;
    //        this.Angle = angle;
    //        this.Depth = depth;
    //    }
    //    public BoneSlot(Vector2 joint, float angle)
    //    {
    //        this.Joint = joint;
    //        this.Angle = angle;
    //        this.Depth = 0;
    //    }
    //}



    public class Bone
    {
        //class BoneType
        //{
        //    public Types Type;
        //    public string Name;
        //    BoneType(Types type, string name)
        //    {
        //        this.Type = type;
        //        this.Name = name;
        //    }
        //}

        public enum States { Unstarted, Stopped, Running, Finished, Manual }
        public enum Types { None, Hips, Torso, RightHand, LeftHand, RightFoot, LeftFoot, Head, Mainhand, Offhand, Hauled, Helmet, EquipmentHead, EquipmentHandle, Item }

        public States State;
        //Action _FinishAction = () => { };
        //public Action FinishAction { get { return _FinishAction; } set { _FinishAction = value; } }
        public Dictionary<Types, Bone> Children;
        //public Dictionary<Types, Vector2> Origins = new Dictionary<Types, Vector2>();
        //public Vector2 GetOrigin(Types type)
        //{
        //    Vector2 origin;
        //    if (this.Origins.TryGetValue(type, out origin))
        //        return origin;
        //    else
        //        return this.Sprite.Origin;
        //}

        public Joint Joint;
        public Dictionary<Types, Joint> Joints = new Dictionary<Types, Joint>();
        public void AddJoint(Types type, Joint joint)
        {
            this.Joints.Add(type, joint);
            joint.Parent = this;
        }
        public void AddJoint(Types type)
        {
            var joint = new Joint();
            this.Joints.Add(type, joint);
            joint.Parent = this;
        }
        public Joint GetJoint(Types type)
        {
            return this.Joints.GetValueOrDefault(type);
        }

        public Bone Parent;
        public Material Material;
        public Color Tint = Color.White;

        /// <summary>
        /// The coordinates on the bone's parent (relative to it's parent sprite origin) where the current bone attaches to.
        /// </summary>
        public Vector2 ParentJoint;


        //public Sprite[] Orientations = new Sprite[4];
        public List<Sprite> Orientations = new List<Sprite>();

        public void SetOrientations(Sprite sprite1)
        {
            //this.Sprite = sprite1;
            this.Orientations.Add(sprite1);
        }
        public void SetOrientations(Sprite sprite1, Sprite sprite2)
        {
            this.Orientations.Add(sprite1);
            this.Orientations.Add(sprite2);
            //this.Orientations[0] = sprite1;
            //this.Orientations[2] = sprite1;
            //this.Orientations[1] = sprite2;
            //this.Orientations[3] = sprite2;
        }
        public void SetOrientations(Sprite sprite1, Sprite sprite2, Sprite sprite3, Sprite sprite4)
        {
            this.Orientations.Add(sprite1);
            this.Orientations.Add(sprite2);
            this.Orientations.Add(sprite3);
            this.Orientations.Add(sprite4);

            //this.Orientations[0] = sprite1;
            //this.Orientations[1] = sprite2;
            //this.Orientations[2] = sprite3;
            //this.Orientations[3] = sprite4;
        }
        public Sprite GetSprite(Camera cam)
        {
            var index = (int)(cam.Zoom % this.Orientations.Count);
            return this.Orientations[index];
        }
        public Sprite GetSprite(int orientation)
        {
            var index = (int)(orientation % this.Orientations.Count);
            return this.Orientations[index];
        }

        public Sprite Sprite//;// { get; set; }
        {
            //get { return this.Orientations[0]; }
            //set { this.Orientations[0] = value; }
            get { return this.Orientations.FirstOrDefault(); }
            set
            {
                this.Orientations.Clear();
                this.Orientations.Add(value);
            }
        }
        public Func<GameObject, Sprite> SpriteFunc;// { get; set; }
        public Func<GameObject, GameObjectSlot> SlotFunc;//{ get; set; }
        GameObjectSlot SpriteSlot;// { get; set; }
        public Keyframe RestingFrame;
        //public int Frame = 0;
        // public Animation Animation  { get { return Layers.Last().Value.First(); } }// { get { return Layers.Peek(); } }// { get { return Layers.Peek().Animations.Last(); } }// { get { return Layers.Last().Value.First(); } }
        //public SortedDictionary<float, List<Animation>> Layers;
        //public PriorityQueue<float, AnimationLayer> Layers;

        //public PriorityQueue<float, Animation> Layers;
        //public Stack<Animation> Layers;

        public SortedDictionary<float, List<Animation>> Layers;
        public float Angle = 0;
        public Vector2 Offset = Vector2.Zero;
        //public string Name;
        public Types Type;
        public float Order;

        //public int Variation = 0, Orientation = 0;

        //public float Percentage
        //{
        //    get
        //    {
        //        return MathHelper.Clamp(Animation.Frame / (float)Animation.FrameCount, 0, 1);

        //        //return MathHelper.Clamp(Frame / (float)Animation.FrameCount, 0, 1);
        //       // return MathHelper.Clamp(Frame / (float)Animation.Last().FrameCount, 0, 1);
        //    }
        //}
        class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }
        static readonly DescendingComparer<float> Descending = new DescendingComparer<float>();
        Bone()
        {
            this.SpriteFunc = parent => { return this.Sprite; };
            this.Order = 0;
            //this.Name = "";
            this.Angle = 0;
            this.Offset = Vector2.Zero;
            //this.Animation = new List<Animation>() { new Animation(false, new Keyframe(0, Vector2.Zero, 0, Interpolation.Lerp)) { Name = "Idle" } };
            //this.Layers = new SortedDictionary<float, List<Animation>>() { { 0, new List<Animation>() { new Animation(false, new Keyframe(0, Vector2.Zero, 0, Interpolation.Lerp)) { Name = "Idle" } } } };

            //this.Layers = new PriorityQueue<float, Animation>();
            //this.Layers.Enqueue(0, new Animation(false, new Keyframe(0, Vector2.Zero, 0, Interpolation.Lerp)) { Name = "Idle" } );

            //this.Layers = new Stack<Animation>();
            //this.Layers.Push(new Animation(WarpMode.Once, new Keyframe(0, Vector2.Zero, 0, Interpolation.Lerp)) { Name = "Idle" });

            this.Layers = new SortedDictionary<float, List<Animation>>(Descending);

            this.Layers[0] = new List<Animation>() { new Animation(WarpMode.Once, new Keyframe(0, Vector2.Zero, 0, Interpolation.Lerp)) { Layer = 0, Name = "Idle" } };

            //this.Animation = new Animation(false, new Keyframe(0, Vector2.Zero, 0, Interpolation.Lerp)) { Name = "Idle" };
            this.Children = new Dictionary<Types, Bone>();
            this.RestingFrame = new Keyframe(10, Vector2.Zero, 0, Interpolation.Sine);
        }

        /// <summary>
        /// Creates a clone of an existing spritenode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        static public Bone Copy(Bone node, Bone parent = null)
        {
            Bone newnode = Bone.Create(node.Type, node.Sprite, parent, node.ParentJoint, node.Order);
            //Bone newnode = Bone.Create(node.Type, new Sprite(node.Sprite), parent, node.ParentJoint, node.Order);

            //Bone newnode = Bone.Create(node.Name, node.Sprite != null ? new Sprite(node.Sprite) : null, parent, node.Joint, node.Order);
            //newnode.Joints = node.Joints;
            newnode.SpriteFunc = node.SpriteFunc;
            newnode.RestingFrame = node.RestingFrame;
            newnode.SlotFunc = node.SlotFunc;
            newnode.Offset = node.Offset;
            newnode.OriginGroundOffset = node.OriginGroundOffset;
            newnode.Tint = node.Tint;
            newnode.Material = node.Material;
            foreach (var child in node.Children)
                Copy(child.Value, newnode);// newnode.Children.Add(child.Key, Create(child.Value, newnode));
            foreach (var joint in node.Joints)
                newnode.AddJoint(joint.Key, new Joint(joint.Value));// newnode.Children.Add(child.Key, Create(child.Value, newnode));
            //foreach (var offset in node.Origins)
            //    newnode.Origins.Add(offset.Key, offset.Value);
            //for (int i = 0; i < 4; i++)
            //    newnode.Orientations[i] = new Sprite(node.Orientations[i]);
            newnode.Orientations.Clear();
            foreach (var i in node.Orientations)
                newnode.Orientations.Add(new Sprite(i));
            return newnode;
        }

        public Bone this[Types type]
        {
            get { return this.Children[type]; }
        }

        public void AddChild(Types type, Bone spriteNode)
        {
            this.Children[type] = spriteNode;
            // this.SortedChildren.Add(spriteNode.Depth, spriteNode);
            spriteNode.Parent = this;
        }
        public void AddChild(Bone bone)
        {
            this.Children[bone.Type] = bone;
            bone.Parent = this;
        }
        public Bone(Types type)
            : this()
        {
            this.Type = type;
        }
        public Bone(Types type, Sprite sprite, Vector2 joint, float depth, params Bone[] children)
            : this(type, sprite)//, keyframes)
        {
            this.Order = depth;
            this.ParentJoint = joint;
            foreach (var child in children)
            {
                child.Parent = this;
                this.Children[child.Type] = child;
            }
        }
        public Bone(Types type, Vector2 joint, float depth, params Bone[] children)
            : this()
        {
            this.Type = type;
            this.Order = depth;
            this.ParentJoint = joint;
            this.Children = new Dictionary<Types, Bone>();
            foreach (var child in children)
            {
                child.Parent = this;
                this.Children[child.Type] = child;
            }
        }
        public Bone(Types type, Sprite sprite)//, params Keyframe[] keyframes)
            : this()
        {
            this.Type = type;
            this.Sprite = sprite.IsNull() ? sprite : new Sprite(sprite);// sprite.Clone();
            this.ParentJoint = Vector2.Zero;
            //foreach (Keyframe kf in keyframes)
            //{
            //    FrameCount = Math.Max(FrameCount, kf.Time);
            //    this.Keyframes.Add(kf);
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="joint"></param>
        /// <param name="depth">Depth in relation to parent bone.</param>
        public Bone(Types type, Vector2 joint, float depth)
            : this()
        {
            this.Type = type;
            this.Order = depth;
            this.ParentJoint = joint;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sprite"></param>
        /// <param name="parent"></param>
        /// <param name="joint">Distance vector from point on the ground.</param>
        /// <param name="depth"></param>
        /// <param name="childrenJoints"></param>
        /// <returns></returns>
        //static public Bone Create(string name, Sprite sprite, Bone parent, Vector2 joint, float depth, params object[] childrenJoints)
        //{
        //    Bone sprnd = new Bone(name, sprite, joint, depth);//, keyframes);
        //    if(parent!=null)
        //        parent.AddChild(name, sprnd);

        //    Queue<object> q = new Queue<object>(childrenJoints);
        //    while (q.Count > 0)
        //        sprnd.ChildrenJoints[(string)q.Dequeue()] = new BoneSlot((Vector2)q.Dequeue(), (float)q.Dequeue(), (float)q.Dequeue());// Tuple.Create((Vector2)q.Dequeue(), (float)q.Dequeue());

        //    return sprnd;
        //}

        static public Bone Create(Types type, Sprite sprite, Bone parent, Vector2 joint, float depth, params Bone[] children)
        {
            Bone sprnd = new Bone(type, sprite, joint, depth);
            if (parent != null)
                parent.AddChild(type, sprnd);

            foreach (var child in children)
                sprnd.Children[child.Type] = child;

            return sprnd;
        }
        static public Bone Create(Types type, Bone parent, Vector2 joint, float depth, params Bone[] children)
        {
            Bone sprnd = new Bone(type, joint, depth);
            if (parent != null)
                parent.AddChild(type, sprnd);

            foreach (var child in children)
                sprnd.Children[child.Type] = child;

            return sprnd;
        }
        static public Bone Create(Types type, Sprite sprite)//, Vector2 joint)//, params Keyframe[] keyframes)
        {
            return new Bone(type, sprite);// { Joint = joint };//, keyframes);
        }
        static public Bone Create(Types type, Sprite sprite, Vector2 joint, float depth)
        {
            return new Bone(type, sprite) { Order = depth, ParentJoint = joint };//, Joint = joint };//, keyframes);
        }

        float UpdateInterval = 1, UpdateLeftOver = 0;
        public Bone Update()
        {
            //if (this.Type == Types.Hips)
            //    Layers.Count.ToConsole();
            //if (this.State == States.Stopped || this.State == States.Unstarted)
            if (this.State == States.Manual)
                return this;

            //float iterations = UpdateLeftOver + Animation.Speed;
            //while (iterations >= UpdateInterval)
            //{
            //    this.Frame += 1;// Animation.Speed; //*GlobalVars.DeltaTime 
            //    iterations--;
            //}
            //UpdateLeftOver = iterations;
            //GetValue2(Frame, Animation, out this.Offset, out this.Angle);

            float availableWeight = 1;
            float finalAngle = 0;
            Vector2 finalOffset = Vector2.Zero;
            GetWeightedValues(ref availableWeight, ref finalAngle, ref finalOffset);
            //this.Offset = finalOffset;
            //this.Angle = finalAngle;

            //this.Offset = this.RestingFrame.Offset + finalOffset;
            this.Offset = finalOffset;//  + this.Origin;
            this.Angle = this.RestingFrame.Angle + finalAngle;
            //GetValue2(Animation.Frame, Animation, out this.Offset, out this.Angle);

            //foreach (var node in Children)
            //    node.Value.Update();

            foreach (var node in this.Joints)
                node.Value.Update();
            return this;
        }
        /// <summary>
        /// The difference vector between the sprite's origin and the ground, for root bones.
        /// </summary>
        public Vector2 OriginGroundOffset = Vector2.Zero;
        private void GetWeightedValues(ref float availableWeight, ref float finalAngle, ref Vector2 finalOffset)
        {
            SortedDictionary<float, List<Animation>> nextLayers = new SortedDictionary<float, List<Animation>>(Descending);
            List<Animation> nextStack;
            //if (this.Layers.Count > 2)
            //    "asdasd".ToConsole();
            foreach (var layer in this.Layers)//.Reverse())
            //for (int i = this.Layers.Count - 1; i >= 0 ; i--)
            {
                //var layer = this.Layers[i];
                //var layer = this.Layers.ElementAt(i);
                nextStack = new List<Animation>();
                foreach (var ani in layer.Value)
                {
                    var name = ani.Name;
                    ani.Update();

                    if (ani.Weight <= 0)
                    {
                        ani.OnFadeOut();
                        if (ani.State == Animation.States.Finished)
                            continue;
                    }
                    float dang;
                    Vector2 doff;
                    //  GetValue2(ani.Frame, ani, out doff, out dang);
                    ani.GetValue(out doff, out dang);
                    finalAngle += dang * ani.Weight * availableWeight;
                    finalOffset += doff * ani.Weight * availableWeight;
                    availableWeight -= ani.Weight;
                    availableWeight = Math.Max(0, availableWeight);
                    nextStack.Add(ani);
                }
                if (nextStack.Count > 0)
                    nextLayers.Add(layer.Key, nextStack);
            }
            this.Layers = nextLayers;
        }
        [Obsolete]
        public void Draw(GameObject parent, SpriteBatch sb, Vector2 screenLoc, Color color, float scale, SpriteEffects sprFx, float depth)
        {
            Sprite sprite = SpriteSlot.IsNull() ? this.SpriteFunc(parent) : (SpriteSlot.HasValue ? SpriteSlot.Object.GetComponent<Components.SpriteComponent>().Sprite : null);
            if (sprite.IsNull())
                return;
            Rectangle sourceRect = sprite.SourceRects[0][0];
            Vector4 shaderRect = new Vector4(sourceRect.X / (float)sprite.Texture.Width, sourceRect.Y / (float)sprite.Texture.Height, sourceRect.Width / (float)sprite.Texture.Width, sourceRect.Height / (float)sprite.Texture.Height);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(shaderRect);
            Vector2 origin = sprite.Joint;//.Origin;
            origin.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - origin.X - 1 : origin.X);
            if (Parent == null)
            {
                sb.Draw(sprite.Texture, screenLoc + this.Offset * scale, sourceRect, color, this.Angle, sprite.Origin, scale, sprFx, depth);
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
                sb.Draw(Sprite.Texture, loc + this.Offset, source, color, this.Angle, Sprite.Origin, 1, sprFx, 0);
                return;
            }
            //sb.Draw(Sprite.Texture, loc + GetOffset() + Parent.GetJoint() + Coords.Rotate(Parent.GetAngle(), this.Joint), source, color, GetAngle(), Sprite.Origin, 1, sprFx, 0);
            sb.Draw(Sprite.Texture, loc + GetOffset() + Parent.GetJoint() - Parent.GetOrigin() + Coords.Rotate(Parent.GetAngle(), this.ParentJoint), source, color, GetAngle(), Sprite.Origin, 1, sprFx, 0);
            //sb.Draw(Sprite.Texture, loc + GetOffset() + Parent.GetJoint() - Parent.Sprite.Origin + Coords.Rotate(Parent.GetAngle(), this.Joint), source, color, GetAngle(), Sprite.Origin, 1, sprFx, 0);
        }
        //public void DrawTree(GameObject parent, SpriteBatch sb, Vector2 screenLoc, Color color, Vector2 loc, float angle, float scale, SpriteEffects sprFx, float alpha, float depth)
        //{
        //    Sprite sprite = SpriteSlot.IsNull() ? this.Sprite : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);

        //    if (sprite.IsNull())
        //        return;
        //    Rectangle sourceRect = sprite.GetSourceRect();
        //    Vector4 shaderRect = new Vector4(sourceRect.X / (float)sprite.Texture.Width, sourceRect.Y / (float)sprite.Texture.Height, sourceRect.Width / (float)sprite.Texture.Width, sourceRect.Height / (float)sprite.Texture.Height);
        //    Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(shaderRect);

        //    float finalAngle = angle + this.Angle;
        //    finalAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - finalAngle) : finalAngle);

        //    Vector2 joint = this.Parent.IsNull() ? sprite.Origin : sprite.Joint;
        //    joint.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - joint.X - 1 : joint.X);

        //    sprite.Draw(sb, screenLoc + loc * scale, color * sprite.Alpha * alpha, finalAngle, joint, scale, sprFx, depth - this.Order);

        //    foreach (var child in this.Children.Values)
        //    {
        //        Vector2 nextLoc = child.Joint + child.Offset;
        //        nextLoc.X *= (sprFx == SpriteEffects.FlipHorizontally ? -1 : 1);
        //        nextLoc = Coords.Rotate(finalAngle, nextLoc);
        //        nextLoc += loc;
        //        child.DrawTree(parent, sb, screenLoc, color, nextLoc, angle + this.Angle, scale, sprFx, alpha, depth - this.Order);
        //    }
        //}


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
        public void DrawTree(GameObject parent, MySpriteBatch sb, Vector2 screenLoc, Color color, Vector2 loc, float angle, float scale, SpriteEffects sprFx, float alpha, float depth)
        {
            //Sprite sprite = SpriteSlot.IsNull() ? this.SpriteFunc(parent) : (SpriteSlot.HasValue ? SpriteSlot.Object.Avatar.Sprite : null);//.GetComponent<Components.ActorSpriteComponent>().Sprite : null);
            Sprite sprite = SpriteSlot == null ? this.Sprite : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);//.GetComponent<Components.ActorSpriteComponent>().Sprite : null);

            if (sprite == null)
                return;
            Rectangle sourceRect = sprite.GetSourceRect();//[this.Variation, this.Orientation];// sprite.SourceRects[0][0];
            Vector4 shaderRect = new Vector4(sourceRect.X / (float)sprite.Texture.Width, sourceRect.Y / (float)sprite.Texture.Height, sourceRect.Width / (float)sprite.Texture.Width, sourceRect.Height / (float)sprite.Texture.Height);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(shaderRect);

            float finalAngle = angle + this.Angle;
            finalAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - finalAngle) : finalAngle);

            //Vector2 joint = sprite.Origin;// sprite.Joint;
            Vector2 joint = this.Parent == null ? sprite.Origin : sprite.Joint;//.Origin;
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
            Sprite sprite = SpriteSlot.IsNull() ? this.Sprite : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);//.GetComponent<Components.ActorSpriteComponent>().Sprite : null);

            if (sprite.IsNull())
                return;
            Rectangle sourceRect = sprite.GetSourceRect();//[this.Variation, this.Orientation];// sprite.SourceRects[0][0];
            Vector4 shaderRect = new Vector4(sourceRect.X / (float)sprite.Texture.Width, sourceRect.Y / (float)sprite.Texture.Height, sourceRect.Width / (float)sprite.Texture.Width, sourceRect.Height / (float)sprite.Texture.Height);
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(shaderRect);

            float finalAngle = angle + this.Angle;
            finalAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - finalAngle) : finalAngle);

            //Vector2 joint = sprite.Origin;// sprite.Joint;
            Vector2 joint = this.Parent.IsNull() ? sprite.Origin : sprite.Joint;//.Origin;
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

                Vector2 origin = this.Parent == null ? sprite.Origin : sprite.Joint;
                origin.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - origin.X - 1 : origin.X);

                //var finalColor = color * alpha;
                var finalColor = color;
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
        public void DrawTree(GameObject parent, MySpriteBatch sb, Vector2 screenLoc, Color sky, Color block, Color tint, Color fog, float angle, float scale, int orientation, SpriteEffects sprFx, float alpha, float depth)
        //public void DrawTree(GameObject parent, MySpriteBatch sb, Vector2 screenLoc, Vector2 offset, Color sky, Color block, Color color, Color fog, float angle, float scale, SpriteEffects sprFx, float alpha, float depth)
        {
            //Sprite sprite = SpriteSlot == null ? this.Sprite : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);
            Sprite sprite = SpriteSlot == null ? this.GetSprite(orientation) : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);

            //var nextangle = this.Angle + (this.Joint != null ? this.Joint.Angle : 0);
            var nextangle = this.Angle;// +(this.Joint != null ? this.Joint.Angle : 0);

            nextangle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - nextangle) : nextangle);
            var finalAngle = angle + nextangle;
            //finalAngle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - finalAngle) : finalAngle);

            //Vector2 finalpos = screenLoc + (this.Offset + (this.Parent == null ? this.OriginGroundOffset : Vector2.Zero)) * scale; // inject origingoundeffect from calling method?
            Vector2 finalpos = screenLoc + (this.Offset) * scale; // inject origingoundeffect from calling method?

            var finalDepth = depth - this.Order;
            if (sprite != null)
            {
                Rectangle sourceRect = sprite.GetSourceRect();
                //Vector2 origin = this.Parent == null ? sprite.Origin : sprite.Joint;

                //Vector2 origin = this.GetOffset(this.Type);
                Vector2 origin = sprite.Origin;

                origin.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - origin.X - 1 : origin.X);

                //var finalColor = color * alpha;
                var finalTint = tint;

                //if (this.Material != null)
                //{
                //    finalColor = finalColor.Multiply(this.Material.Color).Multiply(this.Tint);
                //    finalColor.A = (byte)(this.Material.Type.Shininess * 255);
                //}

                //sprite.Draw(sb, finalpos, sky, block, finalTint, fog, finalAngle, origin, scale, sprFx, finalDepth);
                sprite.Draw(sb, finalpos, this.Material, sky, block, finalTint, fog, finalAngle, origin, scale, sprFx, finalDepth);

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
                next.DrawTree(parent, sb, finalpos + nextLoc * scale, sky, block, tint, fog, finalAngle + childAngle, scale, orientation, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness
                //next.DrawTree(parent, sb, ch.Key, finalpos + nextLoc * scale, sky, block, tint, fog, finalAngle + childAngle, scale, orientation, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness

            }
        }


        //public void DrawTree(GameObject parent, MySpriteBatch sb, Types type, Vector2 screenLoc, Color sky, Color block, Color tint, Color fog, float angle, float scale, int orientation, SpriteEffects sprFx, float alpha, float depth)
        //{
        //    Sprite sprite = SpriteSlot == null ? this.GetSprite(orientation) : (SpriteSlot.HasValue ? SpriteSlot.Object.Body.Sprite : null);
        //    var nextangle = this.Angle;// +(this.Joint != null ? this.Joint.Angle : 0);
        //    nextangle = (sprFx == SpriteEffects.FlipHorizontally ? (float)(2 * Math.PI - nextangle) : nextangle);
        //    var finalAngle = angle + nextangle;
        //    //var offset = this.GetOrigin(type);

        //    Vector2 finalpos = screenLoc + (this.Offset) * scale; // inject origingoundeffect from calling method?
        //    //Vector2 finalpos = screenLoc + (this.Offset + offset) * scale; // inject origingoundeffect from calling method?

        //    var finalDepth = depth - this.Order;
        //    Vector2 origin = sprite.Origin;

        //    if (sprite != null)
        //    {
        //        Rectangle sourceRect = sprite.GetSourceRect();
        //        origin.X = (sprFx == SpriteEffects.FlipHorizontally ? sourceRect.Width - origin.X - 1 : origin.X);
        //        var finalTint = tint;
        //        sprite.Draw(sb, finalpos, this.Material, sky, block, finalTint, fog, finalAngle, origin, scale, sprFx, finalDepth);
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
        //        //next.DrawTree(parent, sb, ch.Key, origin + finalpos + nextLoc * scale, sky, block, tint, fog, finalAngle + childAngle, scale, orientation, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness
        //        next.DrawTree(parent, sb, ch.Key, finalpos + nextLoc * scale, sky, block, tint, fog, finalAngle + childAngle, scale, orientation, sprFx, alpha, finalDepth); // - Vector2.One*2*Graphics.Borders.Thickness
            
        //    }
        //}

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
            return (Parent != null ? Parent.GetOrigin() : this.Sprite.Origin);
        }
        public void ForEach(Action<Bone> action)
        {
            Queue<Bone> toHandle = new Queue<Bone>();
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
            foreach (KeyValuePair<Bone.Types, Bone> node in Children)
                node.Value.GetSorted(list, parentDepth + Order);
        }
        static public Rectangle GetBounds(SortedList<float, Bone> list)
        {
            //  Rectangle bounds = new Rectangle(0, 0, 0, 0);
            Rectangle? bounds = new Rectangle?();
            foreach (Bone node in list.Values)
            {
                Rectangle source = node.Sprite.SourceRects[0][0];
                //Vector2 offset = node.GetJoint() - node.Sprite.Origin - (node.Parent != null ? node.Parent.Sprite.Origin : Vector2.Zero);




                //Vector2 offset = node.GetJoint() - (node.Parent != null ? node.Parent.Sprite.Origin : Vector2.Zero); // - node.Sprite.Origin
                //offset -= new Vector2((float)Math.Cos(node.Angle) * node.Sprite.Origin.X - (float)Math.Sin(node.Angle) * node.Sprite.Origin.Y, (float)Math.Sin(node.Angle) * node.Sprite.Origin.X + (float)Math.Cos(node.Angle) * node.Sprite.Origin.Y);

                Vector2 offset = node.GetJoint() - node.GetOffset() - node.Sprite.Origin - (node.Parent != null ? node.Parent.Sprite.Origin : Vector2.Zero); // - node.Sprite.Origin 
                //Vector2 rotated = new Vector2((float)Math.Cos(node.Angle) * offset.X - (float)Math.Sin(node.Angle) * offset.Y, (float)Math.Sin(node.Angle) * offset.X + (float)Math.Cos(node.Angle) * offset.Y);
                //Vector2 size = new Vector2((float)Math.Cos(node.Angle) * source.Width - (float)Math.Sin(node.Angle) * source.Height, (float)Math.Sin(node.Angle) * source.Width + (float)Math.Cos(node.Angle) * source.Height);
                Rectangle nodeRect = new Rectangle((int)offset.X, (int)offset.Y, source.Width, source.Height);//(int)size.X, (int)size.Y);// source.Width, source.Height);
                bounds = !bounds.HasValue ? nodeRect : Rectangle.Union(bounds.Value, nodeRect);
            }
            return bounds.Value;
        }
        public override string ToString()
        {
            string text = this.Type.ToString();
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


        public void Start(Animation keyframes)
        {
            // this.Animation = keyframes;
            //this.Layers[keyframes.Layer].Add(keyframes);

            //this.Layers.Enqueue(keyframes.Layer, keyframes);
            //this.Layers.Push(keyframes);

            List<Animation> list;
            if (!this.Layers.TryGetValue(keyframes.Layer, out list))
            {
                list = new List<Animation>();
                this.Layers[keyframes.Layer] = list;
            }
            list.Add(keyframes);

            this.State = Bone.States.Unstarted;
            //this.Frame = 0;
        }

        public void Stop()
        {
            //  this.Transition = null;

            foreach (KeyValuePair<Bone.Types, Bone> node in Children)
                node.Value.Stop();
        }

        //public void Rest(AnimationCollection animation, int frame, AnimationCollection runningAnimation)
        //{
        //    float angle; Vector2 offset;
        //    //     GetValue(frame, runningAnimation[Name], out offset, out angle, runningAnimation.Loop);
        //    offset = this.Offset;
        //    angle = this.Angle;
        //    animation.Add(this.Name, AnimationCollection.Idle, new Keyframe(0, offset, angle, Interpolation.Linear), new Keyframe(10, Vector2.Zero, 0, Interpolation.Sine));
        //    foreach (KeyValuePair<string, Bone> node in Children)
        //        node.Value.Rest(animation, frame, runningAnimation);
        //}

        //public void Rest(bool children = true)
        //{
        //    float angle; Vector2 offset;
        //    offset = this.Offset;
        //    angle = this.Angle;
        //    this.Animation = new Animation(false,
        //        new Keyframe(0, offset, angle),
        //        RestingFrame
        //        ) { Name = "Idle" };
        //    this.State = States.Running;
        //    if (children)
        //        foreach (KeyValuePair<string, Bone> node in Children)
        //            node.Value.Rest(children);
        //}

        //public void Rest(Func<Bone, bool> condition)
        //{
        //    if (condition(this))
        //    {
        //        float angle; Vector2 offset;
        //        offset = this.Offset;
        //        angle = this.Angle;
        //        this.Animation = new Animation(false,
        //            new Keyframe(0, offset, angle),
        //            RestingFrame
        //            ) { Name = "Idle" };
        //    }
        //    foreach (KeyValuePair<string, Bone> node in Children)
        //        node.Value.Rest(condition);
        //}

        public void Restart(bool children = true)
        {
            this.Restart(foo => true, children);
            ////if (State == States.Finished)
            ////{
            //this.State = States.Unstarted;
            //this.Frame = 0;
            //this.Transition = null;
            ////  }
            //if (children)
            //    foreach (KeyValuePair<string, SpriteNode> node in Children)
            //    {
            //        node.Value.Restart(children);
            //        //this.Transition = null;
            //        //node.Value.Transition = null;
            //    }
        }
        public void Restart(Func<Bone, bool> condition, bool children = true)
        {
            if (condition(this))
            {
                this.State = States.Unstarted;
                //this.Frame = 0;

            }
            if (children)
                foreach (KeyValuePair<Bone.Types, Bone> node in Children)
                    node.Value.Restart(condition);
        }

        public Bone AddAnimation(Animation animation)
        {
            List<Animation> list;
            if (!this.Layers.TryGetValue(animation.Layer, out list))
            {
                list = new List<Animation>();
                this.Layers[animation.Layer] = list;
            }
            list.Add(animation);
            return this;
        }

        internal void AddAnimation(AnimationCollection animation)
        {
            if (animation == null)
                return;
            Queue<Bone> toHandle = new Queue<Bone>();
            toHandle.Enqueue(this);
            while (toHandle.Count > 0)
            {
                Bone bone = toHandle.Dequeue();
                Animation ani;
                if (animation.TryGetValue(bone.Type, out ani))
                {
                    bone.AddAnimation(ani);
                    bone.Restart(false);
                }
                foreach (var joint in bone.Joints.Values)
                    if (joint.Bone != null)
                        toHandle.Enqueue(joint.Bone);
            }
            //this.Frame = 0;
            this.State = States.Stopped;
        }
        internal void FadeOutAnimation(AnimationCollection animation)
        {
            if (animation == null)
                return;
            Queue<Bone> toHandle = new Queue<Bone>();
            toHandle.Enqueue(this);
            while (toHandle.Count > 0)
            {
                Bone bone = toHandle.Dequeue();
                Animation ani;
                if (animation.TryGetValue(bone.Type, out ani))
                {
                    List<Animation> anims;
                    if (bone.Layers.TryGetValue(ani.Layer, out anims))
                    {
                        foreach (var a in
                            from a in anims
                            where a.Name == ani.Name
                            select a)
                        {
                            a.WeightChange = -0.1f;// true;
                            a.State = Animation.States.Finished;
                        }
                    }
                }
                foreach (var joint in bone.Joints.Values)
                    if (joint.Bone != null)
                        toHandle.Enqueue(joint.Bone);
            }
        }
        internal void FadeOutAnimation(AnimationCollection animation, float seconds)
        {
            float frames = Engine.TargetFps * seconds;
            float dw = 1 / frames;
            Queue<Bone> toHandle = new Queue<Bone>();
            toHandle.Enqueue(this);
            while (toHandle.Count > 0)
            {
                Bone bone = toHandle.Dequeue();
                Animation ani;
                if (animation.TryGetValue(bone.Type, out ani))
                {
                    List<Animation> anims;
                    if (bone.Layers.TryGetValue(ani.Layer, out anims))
                    {
                        foreach (var a in
                            from a in anims
                            where a.Name == ani.Name
                            select a)
                        {
                            a.WeightChange = -dw;// true;
                            a.State = Animation.States.Finished;
                        }
                    }
                }
                foreach (var joint in bone.Joints.Values)
                    if (joint.Bone != null)
                        toHandle.Enqueue(joint.Bone);
            }
        }

        internal void StopAnimation(AnimationCollection animation)
        {
            Queue<Bone> toHandle = new Queue<Bone>();
            toHandle.Enqueue(this);
            while (toHandle.Count > 0)
            {
                Bone bone = toHandle.Dequeue();
                Animation ani;
                if (animation.TryGetValue(bone.Type, out ani))
                {
                    List<Animation> anims;
                    if (bone.Layers.TryGetValue(ani.Layer, out anims))
                    {
                        foreach (var a in
                            from a in anims
                            where a.Name == ani.Name
                            select a)
                        {
                            a.Stop();

                        }
                    }
                }

                foreach (var joint in bone.Joints.Values)
                    if (joint.Bone != null)
                        toHandle.Enqueue(joint.Bone);
            }
            //this.Frame = 0;
            this.State = States.Stopped;
        }
        internal void Start(AnimationCollection animation)
        {
            Queue<Bone> toHandle = new Queue<Bone>();
            toHandle.Enqueue(this);
            while (toHandle.Count > 0)
            {
                Bone bone = toHandle.Dequeue();
                Animation ani;
                if (animation.TryGetValue(bone.Type, out ani))
                {
                    bone.AddAnimation(ani);
                    //List<Animation> list;
                    //if (!bone.Layers.TryGetValue(ani.Layer, out list))
                    //{
                    //    list = new List<Animation>();
                    //    bone.Layers[ani.Layer] = list;
                    //}
                    //list.Add(ani);

                    bone.Restart(false);
                }
                foreach (var sub in bone.Children)
                    toHandle.Enqueue(sub.Value);
            }
            //this.Frame = 0;
            this.State = States.Stopped;
        }
        internal void CrossFade(AnimationCollection animation, bool preFade, int fadeLength)
        {
            this.CrossFade(animation, preFade, fadeLength, Interpolation.Lerp);//(a, b, c) => Interpolation.Lerp(a, b, c));
        }
        internal void CrossFade(AnimationCollection animation, bool preFade, int fadeLength, Func<float, float, float, float> fadeInterpolation)
        {
            foreach (var ani in animation.Inner)
            {
                ani.Value.FadeIn(preFade, fadeLength, fadeInterpolation);
            }
            this.AddAnimation(animation);
        }

        internal void CrossFadeOld(AnimationCollection animation, bool preFade, int fadeLength, Func<float, float, float, float> fadeInterpolation)
        {
            foreach (var ani in animation.Inner)
            {
                ani.Value.FadeIn(preFade, fadeLength, fadeInterpolation);
                //ani.Value.Weight = 0;
                //ani.Value.WeightChange = 0.1f;
            }
            //Animation.CrossFade(this, animation);
            this.Start(animation);
            //Queue<Bone> toHandle = new Queue<Bone>();
            //toHandle.Enqueue(this);
            //while (toHandle.Count > 0)
            //{
            //    Bone bone = toHandle.Dequeue();
            //    Animation ani;
            //    if (animation.TryGetValue(bone.Name, out ani))
            //    {
            //        bone.AddAnimation(ani);
            //        bone.Restart(false);
            //    }
            //    foreach (var sub in bone.Children)
            //        toHandle.Enqueue(sub.Value);
            //}
            ////this.Frame = 0;
            //this.State = States.Stopped;
        }


        internal void Stop(AnimationCollection animation)
        {
            Queue<Bone> toHandle = new Queue<Bone>();
            toHandle.Enqueue(this);
            while (toHandle.Count > 0)
            {
                Bone bone = toHandle.Dequeue();
                Animation ani;
                if (animation.TryGetValue(bone.Type, out ani))
                {
                    List<Animation> anims;
                    if (bone.Layers.TryGetValue(ani.Layer, out anims))
                    {
                        foreach (var a in
                            from a in anims
                            where a.Name == ani.Name
                            select a)
                        {
                            a.Stop();

                        }
                    }



                    //List<Animation> anims;
                    //if(bone.Layers.TryGetValue(ani.Layer, out anims))
                    //{
                    //    Animation found = anims.FirstOrDefault(a => a.Name == ani.Name);
                    //    if (found.IsNull())
                    //        continue;
                    //    found.Weight = 0;
                    //}
                }
                foreach (var sub in bone.Children)
                    toHandle.Enqueue(sub.Value);
            }
            //this.Frame = 0;
            this.State = States.Stopped;
        }
        internal void FadeOut(float layer, float fadeLength)
        {
            Queue<Bone> toHandle = new Queue<Bone>();
            toHandle.Enqueue(this);
            while (toHandle.Count > 0)
            {
                Bone bone = toHandle.Dequeue();
                List<Animation> anims;
                if (bone.Layers.TryGetValue(layer, out anims))
                {
                    foreach (var a in
                        from a in anims
                        select a)
                    {
                        a.WeightChange = -fadeLength;
                        a.State = Animation.States.Finished;
                    }
                }

                foreach (var sub in bone.Children)
                    toHandle.Enqueue(sub.Value);
            }
        }
        internal void FadeOut(AnimationCollection animation)
        {
            Queue<Bone> toHandle = new Queue<Bone>();
            toHandle.Enqueue(this);
            while (toHandle.Count > 0)
            {
                Bone bone = toHandle.Dequeue();
                Animation ani;
                if (animation.TryGetValue(bone.Type, out ani))
                {
                    List<Animation> anims;
                    if (bone.Layers.TryGetValue(ani.Layer, out anims))
                    {
                        foreach (var a in
                            from a in anims
                            where a.Name == ani.Name
                            select a)
                        {
                            a.WeightChange = -0.1f;// true;
                            a.State = Animation.States.Finished;
                        }
                    }
                }
                foreach (var sub in bone.Children)
                    toHandle.Enqueue(sub.Value);
            }
        }
        internal Animation Find(string aniName, Types type)
        {
            Queue<Bone> toHandle = new Queue<Bone>();
            toHandle.Enqueue(this);
            while (toHandle.Count > 0)
            {
                Bone bone = toHandle.Dequeue();
                if (bone.Type == type)
                    foreach (var layer in bone.Layers)
                        foreach (var ani in layer.Value)
                            if (ani.Name == aniName)
                                return ani;
                foreach (var sub in bone.Children)
                    toHandle.Enqueue(sub.Value);
            }
            return null;
        }
        internal Bone Find(Types type)
        {
            Bone bone;
            if (this.Children.TryGetValue(type, out bone))
                return bone;
            foreach (var child in this.Children)
            {
                bone = child.Value.Find(type);
                if (bone != null)
                    return bone;
            }
            return bone;
        }
        internal Joint FindJoint(Types type)
        {
            Joint joint;
            if (this.Joints.TryGetValue(type, out joint))
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
        internal List<Bone> GetChildren()
        {
            List<Bone> list = new List<Bone>() { this };
            //foreach (var child in this.Children.Values)
            //{
            //    list.AddRange(child.GetChildren());
            //}
            foreach (var child in this.Joints.Values)
            {
                if (child.Bone != null)
                    list.AddRange(child.Bone.GetChildren());
            }
            return list;
        }
        //internal void FadeOut(AnimationCollection animation)
        //{
        //    Queue<Bone> toHandle = new Queue<Bone>();
        //    toHandle.Enqueue(this);
        //    while (toHandle.Count > 0)
        //    {
        //        Bone bone = toHandle.Dequeue();
        //        Animation ani;
        //        if (animation.TryGetValue(bone.Name, out ani))
        //            ani.Fade = true;
        //        foreach (var sub in bone.Children)
        //            toHandle.Enqueue(sub.Value);
        //    }
        //}

        public Sprite Render()
        {
            //RenderTarget2D tex = new RenderTarget2D(Game1.Instance.GraphicsDevice, 17, 38);
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;

            SortedList<float, Bone> sortedNodes = new SortedList<float, Bone>();
            //Rectangle bounds = new Rectangle(0, 0, 0, 0);
            GetSorted(sortedNodes, this.Order);//, ref bounds);
            Rectangle bounds = new Rectangle(0, 0, 50, 50);// GetBounds(sortedNodes);
            // bounds = new Rectangle(0, 0, bounds.Width + bounds.X, bounds.Height + bounds.Y);
            RenderTarget2D tex = new RenderTarget2D(gfx, bounds.Width, bounds.Height);
            gfx.SetRenderTarget(tex);
            gfx.Clear(Color.Transparent);
            SpriteBatch sb = new SpriteBatch(gfx);
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
            if (!SlotFunc.IsNull())
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
            MySpriteBatch mysb = new MySpriteBatch(gd);
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

        //public RenderTarget2D RenderNew(GameObject parent)
        //{
        //    GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            
        //    var sprite = this.Sprite;
        //    Rectangle rect = new Rectangle(0, 0, 128, 128);//32, 64);// GetMinimumRectangle();// new Rectangle(3, 3, Width - 6, Height - 6);
        //    //Rectangle rect = GetMinimumRectangle();// new Rectangle(3, 3, Width - 6, Height - 6);

        //    var texture = new RenderTarget2D(gd, rect.Width, rect.Height);//, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
        //    gd.SetRenderTarget(texture);
        //    gd.Clear(Color.Transparent);

        //    var loc = new Vector2(rect.X, rect.Y);
        //    Effect fx = Game1.Instance.Content.Load<Effect>("blur");
        //    MySpriteBatch mysb = new MySpriteBatch(gd);
        //    fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
        //    //fx.Parameters["Viewport"].SetValue(new Vector2(this.Size.Width, this.Size.Height));
        //    fx.Parameters["Viewport"].SetValue(new Vector2(rect.Width, rect.Height));

        //    gd.Textures[0] = Sprite.Atlas.Texture;
        //    gd.Textures[1] = Sprite.Atlas.DepthTexture;
        //    fx.CurrentTechnique.Passes["Pass1"].Apply();

        //    var body = this;
        //    var scale = 1;

        //    loc += sprite.Origin;
        //    body.DrawTree(parent, mysb, new Vector2(rect.Width/2, rect.Height/2) + OriginGroundOffset + loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);
        //   // body.DrawTree(parent, mysb, loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);
        //    //body.DrawTree(parent, mysb, new Vector2(rect.Width/2, rect.Height/2), Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);

        //    var test = GameObject.Objects[GameObject.Types.Berries];
        //    test.Body.DrawTree(test, mysb, new Vector2(rect.Width / 2, rect.Height / 2) + loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);


        //    mysb.Flush();
        //    //gd.SetRenderTarget(null);

        //    return texture;
        //}
        //public void RenderNewer(GameObject parent, Vector2 viewport, Rectangle rect)
        //{
        //    GraphicsDevice gd = Game1.Instance.GraphicsDevice;

        //    var sprite = this.Sprite;
        //    //Rectangle rect = new Rectangle(0, 0, (int)viewport.X, (int)viewport.Y);//32, 64);// GetMinimumRectangle();// new Rectangle(3, 3, Width - 6, Height - 6);
        //    //Rectangle rect = GetMinimumRectangle();// new Rectangle(3, 3, Width - 6, Height - 6);

        //    //var texture = new RenderTarget2D(gd, rect.Width, rect.Height);//, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
        //    //gd.SetRenderTarget(texture);
        //    //gd.Clear(Color.Transparent);

        //    var loc = new Vector2(rect.X, rect.Y);
        //    Effect fx = Game1.Instance.Content.Load<Effect>("blur");
        //    MySpriteBatch mysb = new MySpriteBatch(gd);
        //    fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
        //    //fx.Parameters["Viewport"].SetValue(new Vector2(this.Size.Width, this.Size.Height));
        //    fx.Parameters["Viewport"].SetValue(viewport);//new Vector2(rect.Width, rect.Height));

        //    gd.Textures[0] = Sprite.Atlas.Texture;
        //    gd.Textures[1] = Sprite.Atlas.DepthTexture;
        //    fx.CurrentTechnique.Passes["Pass1"].Apply();

        //    var body = this;
        //    var scale = 1;

        //    loc += sprite.Origin;
        //    body.DrawTree(parent, mysb, new Vector2(rect.Width / 2, rect.Height / 2) + OriginGroundOffset + loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);
        //    // body.DrawTree(parent, mysb, loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);
        //    //body.DrawTree(parent, mysb, new Vector2(rect.Width/2, rect.Height/2), Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);

        //    var test = GameObject.Objects[GameObject.Types.Berries];
        //    test.Body.DrawTree(test, mysb, new Vector2(rect.Width / 2, rect.Height / 2) + loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);


        //    mysb.Flush();
        //    //gd.SetRenderTarget(null);

        //}

        public void RenderNewer(GameObject parent, RenderTarget2D texture)
        {
            //if (parent.ID == GameObject.Types.Actor)
            //    return;
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;

            var sprite = this.Sprite;
            //Rectangle rect = new Rectangle(0, 0, (int)viewport.X, (int)viewport.Y);//32, 64);// GetMinimumRectangle();// new Rectangle(3, 3, Width - 6, Height - 6);
            //Rectangle rect = GetMinimumRectangle();// new Rectangle(3, 3, Width - 6, Height - 6);
            var rect = texture.Bounds;
            //var texture = new RenderTarget2D(gd, rect.Width, rect.Height);//, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            gd.SetRenderTarget(texture);
            gd.Clear(Color.Transparent);

            var loc = new Vector2(rect.X, rect.Y);
            Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            MySpriteBatch mysb = new MySpriteBatch(gd);
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
            loc = new Vector2((int)((rect.Width) / 2), (int)((rect.Height - minrect.Height) / 2)) + new Vector2(0, this.Sprite.Origin.Y) - this.OriginGroundOffset * 2;
            loc *= scale;
            body.DrawTree(parent, mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);
            
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
            List<Rectangle> rects = new List<Rectangle>();
            this.GetRectangle(rects);
            //var minRect = new Rectangle(0, 0, 0, 0);
            int minx = 0, miny = 0, maxx = 0, maxy = 0;

            //foreach (var rect in rects)
            //{
            //    //minx = Math.Min(minx, rect.Left);
            //    //miny = Math.Min(miny, rect.Top);

            //    maxx = Math.Max(maxx, rect.Width);
            //    maxy = Math.Max(maxy, rect.Height);
            //}
            //return new Rectangle(minx, miny, maxx - minx, maxy - miny);

            var union = new Rectangle(0, 0, 0, 0);
            foreach (var rect in rects)
            {
                union = Rectangle.Union(union, rect);
            }
            return new Rectangle(0,0,union.Width, union.Height);
        }
        Rectangle GetRectangle(Vector2 offset)
        {
            var pos = offset + this.Offset - this.Sprite.Origin;
            return new Rectangle((int)pos.X, (int)pos.Y, this.Sprite.AtlasToken.Rectangle.Width, this.Sprite.AtlasToken.Rectangle.Height);
        }

        public List<Joint> Descendants(Bone.Types type)
        {
            List<Joint> list = new List<Joint>();
            Joint found;
            if (this.Joints.TryGetValue(type, out found))
                list.Add(found);
            foreach (var j in this.Joints.Values)
                if (j.Bone != null)
                    list.AddRange(j.Bone.Descendants(type));
            return list;
        }

        public void Write(BinaryWriter w)
        {
            //w.Write(this.Material.ID);
            w.Write(this.Material != null ? this.Material.ID : -1);

            foreach (var joint in this.Joints)
                if (joint.Value.Bone != null)
                    joint.Value.Bone.Write(w);
        }
        public void Read(BinaryReader r)
        {
            var matID = r.ReadInt32();
            this.Material = matID > -1 ? Material.Templates[matID] : null;
            foreach (var joint in this.Joints)
                if (joint.Value.Bone != null)
                    joint.Value.Bone.Read(r);
        }
    }
}
