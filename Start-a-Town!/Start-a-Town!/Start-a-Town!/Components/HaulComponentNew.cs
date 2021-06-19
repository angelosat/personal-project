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
    class HaulComponentNew : Component
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
        GameObjectSlot Slot { get { return this.GetSlot(); } }
        PersonalInventoryComponent Inventory;
        int HaulSlotID = 0;
        public GameObjectSlot GetSlot()
        {
            return this.Inventory.Slots.GetSlot(this.HaulSlotID);
        }
        public GameObject GetObject()
        {
            return this.Inventory.Slots.GetSlot(this.HaulSlotID).Object;
        }
        GameObject LastObj;
        Graphics.AnimationCollection Animation;
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
            //this.Slot = GearComponent.GetSlot(parent, GearType.Hauling);
            if (!this.Slot.HasValue && LastObj != null)
            {
                StopCarrying(parent);
            }
            //else if (LastObj.IsNull() && this.Watched.HasValue)
            //{
            //    StartCarrying(parent);
            //}
            else if (this.Slot.HasValue)
            {
                if(LastObj.IsNull())
                    StartCarrying(parent);

                DrainStamina(parent);

                
            }
            this.LastObj = Slot.Object;
        }

        private void DrainStamina(GameObject parent)
        {
            var obj = this.Slot.Object;
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
            this.Animation = Graphics.AnimationCollection.Hauling;
            //parent.GetComponent<SpriteComponent>().Body.Start(this.Animation);
            parent.Body.AddAnimation(this.Animation);
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
            this.StrModifier = new Attribute.ValueModifier(() => 1 - StatMaxWeight.GetRatio(parent));
            str.Modifiers.Add(this.StrModifier);
        }

        private void StopCarrying(GameObject parent)
        {
            //parent.GetComponent<SpriteComponent>().Body.FadeOut(this.Animation);
            parent.Body.FadeOutAnimation(this.Animation);
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

            GameObjectSlot hauling = this.Slot;

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
            GameObjectSlot hauling = this.Slot;
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
        public GameObjectSlot Holding
        {
            get
            {
                //GameObjectSlot slot = this.EquipmentSlots[GearType.Hauling];
                GameObjectSlot slot = this.Slot;

                //if (!slot.HasValue)
                //    //slot = this.EquipmentSlots[GearType.Mainhand];
                //    slot = this.Equipment.Slots.FirstOrDefault(f => f.ID == (int)GearType.Mainhand.ID);

                return slot;
            }
        }

        public override object Clone()
        {
            return new HaulComponentNew();
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            this.Slot.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Slot.Read(r);
        }
        internal override List<SaveTag> Save()
        {
            var save = new List<SaveTag>();
            save.Add(new SaveTag(SaveTag.Types.Compound, "Slot", this.Slot.Save()));
            return save;
        }
        internal override void Load(SaveTag save)
        {
            save.TryGetTag("Slot", tag => this.Slot.Load(tag));
        }

        public override void Draw(MySpriteBatch sb, GameObject parent, Camera camera)
        {
            if (this.Slot.Object == null)
                return;
            var body = this.Slot.Object.Body;
            var map = parent.Map;
            Vector2 direction = parent.Transform.Direction;
            Vector2 finalDir = Coords.Rotate(camera, direction);
            var global = parent.Global + Vector3.UnitZ * parent.GetPhysics().Height;
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
            body.DrawTree(this.Slot.Object, sb, finalpos, skyColor, blockColor, Color.White, fog, 0, camera.Zoom, (int)camera.Rotation, sprfx, 1f, depth);

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
