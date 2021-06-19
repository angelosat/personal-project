using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components.Stats;

namespace Start_a_Town_.Components
{
    class HaulComponent : Component
    {
        class StatModifierCarrying : ValueModifier
        {
            public StatModifierCarrying()
                : base("Carrying", (mod, parent, v) => v * mod.GetValue("a"), new ValueModifierValue("a", 0.5f))
                //: base("Carrying", GetFinalSpeed, new StatModifierValue("a", 0.5f))

            {
                //this.Description = (mod) => "Carrying an item reduces your speed by " + (1 - mod.GetValue("a")).ToString("##%");
                this.Modifier = this.GetFinalSpeed;
            }

            float GetFinalSpeed(ValueModifier mod, GameObject parent, float value)
            {
                var ratio = StatMaxWeight.GetRatio(parent);
                var final = 0.5f + 0.5f * ratio;
                return final;
            }
        }

        public override string ComponentName
        {
            get { return "Haul"; }
        }
        //StatModifier Modifier = new StatModifier("Carrying", (mod, v) => v * 0.5f);
        ValueModifier Modifier = new StatModifierCarrying();// new StatModifier("Carrying", (mod, parent, v) => v * mod.GetValue("a"), new StatModifierValue("a", 0.5f)) { Description = (mod) => "Carrying an item reduces your speed by " + (1 - mod.GetValue("a")).ToString("##%") };
    
        //public GameObjectSlot Slot = new GameObjectSlot();
        PersonalInventoryComponent Inventory;
        //int Index = 0;
        //public GameObjectSlot Slot = new GameObjectSlot();
        //GameObjectSlot Slot { get { return this.GetSlot(); } }
        public GameObjectSlot GetSlot()
        {
            //return this.Inventory.Slots.GetSlot(this.Index);
            return this.Inventory.GetHauling();
        }
        public GameObject GetObject()
        {
            //return this.Inventory.Slots.GetSlot(this.Index).Object;
            return this.Inventory.GetHauling().Object;
        }

        GameObject LastObj;
        public Graphics.AnimationCollection AnimationHaul;
        //public Graphics.AnimationCollection AnimationPickUp;

        public override void ComponentsCreated(GameObject parent)
        {
            //this.Watched = parent.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling];
            //this.Watched = GearComponent.GetSlot(parent, GearType.Hauling);
            //this.Slot = new GameObjectSlot();
            this.Inventory = parent.GetComponent<PersonalInventoryComponent>();
            if (this.Inventory == null || this.Inventory.Slots.Capacity == 0)
                throw new Exception("HaulComponent requires a parent entity with PersonalInventoryComponent and an inventory of at least size 1");
    
        }
        
        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            base.Update(net, parent, chunk);
            var slot = this.GetSlot();
            //var slot = this.Slot;
            //this.Slot = GearComponent.GetSlot(parent, GearType.Hauling);
            if (!slot.HasValue && LastObj != null)
            {
                StopCarrying(parent);
            }
            //else if (LastObj.IsNull() && this.Watched.HasValue)
            //{
            //    StartCarrying(parent);
            //}
            else if (slot.HasValue)
            {
                if(LastObj == null)
                    StartCarrying(parent);

                DrainStamina(parent);

                
            }
            this.LastObj = slot.Object;
        }

        private void DrainStamina(GameObject parent)
        {
            var slot = this.GetSlot();
            var obj = slot.Object;
            var w = obj.GetPhysics().Weight;
            ////var str = StatsComponent.GetStatOrDefault(parent, Stat.Types.Strength, 1);
            //AttributesComponent atts = parent.GetComponent<AttributesComponent>();
            //var str = atts.Attributes.FirstOrDefault(a => a.ID == Attribute.Types.Strength).Value;
            var ratio = StatMaxWeight.GetRatio(parent);
            var val = 1 - ratio;
            val *= 0.1f;
            Resource stamina = parent.GetComponent<ResourcesComponent>().Resources[Resource.Types.Stamina];
            stamina.Add(-val);
            if(stamina.Value <=0)
                parent.GetComponent<WorkComponent>().Perform(parent, new Interactions.DropCarried(), TargetArgs.Empty);
        }

        Attribute.ValueModifier StrModifier;
        private void StartCarrying(GameObject parent)
        {
            this.AnimationHaul = Graphics.AnimationCollection.Hauling;
            //parent.Body.AddAnimation(this.Animation);
            parent.Body.CrossFade(this.AnimationHaul, false, 10);

            StatsComponentNew.AddModifier(parent, Stat.Types.WalkSpeed, this.Modifier);
            //StatsComponent.SetStat(parent, Stat.Types.WalkSpeed, v => v * 0.5f);
            //StatsComponent stats = parent.GetComponent<StatsComponent>();
            //float walk;
            //stats.BaseStats.TryGetValue(Stat.Types.WalkSpeed, out walk);

            //// award strength
            //var str = AttributesComponent.GetAttribute(parent, Attribute.Types.Strength);
            ////str.AddToProgress(parent, 0.5f);
            //var ratio = StatMaxWeight.GetRatio(parent);
            //var gain = 1 - ratio;
            //str.GainRate += gain;
            var str = AttributesComponent.GetAttribute(parent, Attribute.Types.Strength);
            if (str == null)
                return;
            //var ratio = StatMaxWeight.GetRatio(parent);
            //this.StrModifier = new Attribute.ValueModifier(() => 1 - ratio);
            this.StrModifier = new Attribute.ValueModifier(() => 1 - StatMaxWeight.GetRatio(parent));
            str.Modifiers.Add(this.StrModifier);
        }

        private void StopCarrying(GameObject parent)
        {
            //parent.GetComponent<SpriteComponent>().Body.FadeOut(this.Animation);
            parent.Body.FadeOutAnimation(this.AnimationHaul);
            StatsComponent.SetStat(parent, Stat.Types.WalkSpeed, v => v * 2f);
            StatsComponentNew.RemoveModifier(parent, Stat.Types.WalkSpeed, this.Modifier);

            // stop awarding strength
            var str = AttributesComponent.GetAttribute(parent, Attribute.Types.Strength);
            if (str == null)
                return;
            str.Modifiers.Remove(this.StrModifier);
            //var ratio = StatMaxWeight.GetRatio(parent);
            //var gain = 1 - ratio;
            //str.GainRate -= gain;
            
        }



        public bool Carry(GameObject parent, GameObjectSlot objSlot)
        {
            return this.Carry(parent.Net, parent, objSlot);
        }
        public bool Carry(Net.IObjectProvider net, GameObject parent, GameObjectSlot objSlot)
        {
            if (objSlot == null)
                return true;
            if (!objSlot.HasValue)
                return true;

            //if (objSlot.Object.GetPhysics().Size != ObjectSize.Haulable)
            //    return true;
            if (!CheckWeight(parent, objSlot.Object))
                return true;

            //GameObjectSlot hauling = this.Slot;
            GameObjectSlot hauling = this.GetSlot();

            // if currently hauling object of same type, increase held stacksize and dispose other object
            var existing = hauling.Object;
            if (existing != null)
                if (existing.ID == objSlot.Object.ID)
                {
                    existing.StackSize++;
                    objSlot.Object.Despawn();
                    net.DisposeObject(objSlot.Object);
                    return true;
                }

            // else
            // drop currently hauled object and pick up new one
            //hauling.Clear();
            this.Throw(Vector3.Zero, parent);

            net.Despawn(objSlot.Object);
            hauling.Object = objSlot.Object;
            //hauling.Swap(objSlot);
            return true;
        }

        static public bool CheckWeight(GameObject a, GameObject t)
        {
            float w = t.GetPhysics().Weight;
            float maxW = StatsComponentNew.GetStatValueOrDefault(a, Stat.Types.MaxWeight, 0);
            return maxW >= w;
        }


        bool Throw(Net.IObjectProvider net, Vector3 velocity, GameObject parent, bool all)
        {
            // throws hauled object, if hauling nothing throws equipped object, make it so it only throws hauled object?
            //if (!this.Holding.HasValue)
            //    return false;
            //GameObject newobj = this.Holding.Take();

            //GameObjectSlot hauling = this.EquipmentSlots[GearType.Hauling];

            var slot = this.GetSlot();
            GameObjectSlot hauling = slot;// this.Slot;
            if (hauling.Object == null)
                return false;
            //GameObject newobj = hauling.Take();

            GameObject newobj = all ? hauling.Object : hauling.Take();


            newobj.Global = parent.Global + new Vector3(0, 0, parent.GetPhysics().Height);
            newobj.Velocity = velocity;
            newobj.Physics.Enabled = true;
            net.Spawn(newobj);

            if (all)
                hauling.Clear();
            return true;
        }
        public bool Throw(Vector3 velocity, GameObject parent, bool all = false)
        {
            return this.Throw(parent.Net, velocity, parent, all);
        }
        public bool Throw(GameObject parent, Vector3 direction, bool all = false)
        {
            Vector3 velocity = direction * 0.1f + parent.Velocity;
            return this.Throw(parent.Net, velocity, parent, all);
        }
        static public bool ThrowHauled(GameObject parent, Vector3 direction, bool all = false)
        {
            var haulComp = parent.GetComponent<HaulComponent>();
            return haulComp.Throw(parent, direction, all);
        }
        public GameObjectSlot Holding
        {
            get
            {
                //GameObjectSlot slot = this.EquipmentSlots[GearType.Hauling];
                
                GameObjectSlot slot = this.GetSlot();//.Slot;

                //if (!slot.HasValue)
                //    //slot = this.EquipmentSlots[GearType.Mainhand];
                //    slot = this.Equipment.Slots.FirstOrDefault(f => f.ID == (int)GearType.Mainhand.ID);

                return slot;
            }
        }

        public override object Clone()
        {
            return new HaulComponent();
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            //this.Slot.Write(w);
            //w.Write(this.Index);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            //this.Slot.Read(r);
            //this.Index = r.ReadInt32();
        }
        internal override List<SaveTag> Save()
        {
            var save = new List<SaveTag>();
            //save.Add(new SaveTag(SaveTag.Types.Compound, "Slot", this.Slot.Save()));
            //save.Add(new SaveTag(SaveTag.Types.Int, "Index", this.Index));

            return save;
        }
        internal override void Load(SaveTag save)
        {
            //save.TryGetTag("Slot", tag => this.Slot.Load(tag));
            //this.Index = save.TagValueOrDefault<int>("Index", 0);
        }

        public override void Draw(MySpriteBatch sb, GameObject parent, Camera camera)
        {
            return;
            //var slot = this.Slot;
            var slot = this.GetSlot();
            if (slot.Object == null)
                return;
            var body = slot.Object.Body;
            var map = parent.Map;
            Vector2 direction = parent.Transform.Direction;
            Vector2 finalDir = Coords.Rotate(camera, direction);
            var global = parent.Global +Vector3.UnitZ * parent.GetPhysics().Height;
            float depth = global.GetDrawDepth(map, camera);
            SpriteEffects sprfx = (finalDir.X - finalDir.Y) < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            byte skylight, blocklight;
            parent.Map.GetLight(parent.Global.RoundXY(), out skylight, out blocklight);
            var skyColor = map.GetAmbientColor() * ((skylight + 1) / 16f); //((skylight) / 15f);
            skyColor.A = 255;
            var blockColor = Color.Lerp(Color.Black, Color.White, (blocklight) / 15f);
            var fog = camera.GetFogColor((int)parent.Global.Z);
            var test = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, new Rectangle(0, 0, 0, 0), Vector2.Zero);
            var finalpos = new Vector2(test.X, test.Y) + (body.OriginGroundOffset * camera.Zoom); //screenLoc + 

            //var hand = parent.Body.FindJoint(Graphics.Bone.Types.RightHand);
            //Vector2 finaljointpos = hand.GetFinalPosition();
            //finalpos += finaljointpos;

            body.DrawTree(slot.Object, sb, finalpos, skyColor, blockColor, Color.White, fog, 0, camera.Zoom, (int)camera.Rotation, sprfx, 1f, depth);
            
        }

        internal override void HandleRemoteCall(GameObject gameObject, Message.Types type, System.IO.BinaryReader r)
        {
            switch(type)
            {
                case Message.Types.Carry:
                    var objid = r.ReadInt32();
                    var obj = gameObject.Net.GetNetworkObject(objid);
                    this.Carry(gameObject, obj.ToSlot());
                    break;

                default:
                    break;
            }
        }
        //public bool Carry(GameObject parent, GameObject obj)
        //{
        //    return this.Carry(parent, obj.ToSlot());
        //}
        static public bool Carry(GameObject parent, GameObject obj)
        {
            return parent.GetComponent<HaulComponent>().Carry(parent, obj.ToSlot());
        }

        //static public bool Carry(GameObject pareng )
    }
}
