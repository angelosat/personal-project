using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components.Stats;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Components
{
    partial class HaulComponent : EntityComponent
    {

        public override string ComponentName
        {
            get { return "Haul"; }
        }
        //StatModifier Modifier = new StatModifier("Carrying", (mod, v) => v * 0.5f);
        //readonly ValueModifier Modifier = new StatModifierCarrying();// new StatModifier("Carrying", (mod, parent, v) => v * mod.GetValue("a"), new StatModifierValue("a", 0.5f)) { Description = (mod) => "Carrying an item reduces your speed by " + (1 - mod.GetValue("a")).ToString("##%") };
    
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

        //GameObject LastObj;
        public Animation AnimationHaul = new(AnimationDef.Haul) { Weight = 0 };
        //public Graphics.AnimationCollection AnimationPickUp;

        public override void OnObjectCreated(GameObject parent)
        {
            //this.Watched = parent.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling];
            //this.Watched = GearComponent.GetSlot(parent, GearType.Hauling);
            //this.Slot = new GameObjectSlot();
            this.Inventory = parent.GetComponent<PersonalInventoryComponent>();
            if (this.Inventory == null || this.Inventory.Slots.Capacity == 0)
                throw new Exception("HaulComponent requires a parent entity with PersonalInventoryComponent and an inventory of at least size 1");
            parent.AddResourceModifier(new ResourceRateModifier(ResourceRateModifierDef.HaulingStaminaDrain));
            parent.AddStatModifier(new StatNewModifier(StatNewModifierDef.WalkSpeedHaulingWeight));
            parent.AddAnimation(this.AnimationHaul);
        }
        //public override void OnObjectLoaded(GameObject parent)
        //{
        //    parent.AddResourceModifier(new ResourceRateModifier(ResourceRateModifierDef.HaulingStaminaDrain));
        //    parent.AddAnimation(this.AnimationHaul);
        //}
        public override void Tick(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            /// cleanest code
            return;
            //if (parent.GetResource(ResourceDef.Stamina).Value <= 0 && parent.IsHauling())
            //{
            //    parent.Interact(new InteractionThrow(true));
            //    parent.Net.Log.Write(parent.Name + " became exhausted, dropping hauled object");
            //}


        }

        //private void DrainStamina(GameObject parent)
        //{
        //    var slot = this.GetSlot();
        //    //var obj = slot.Object;
        //    //var w = obj.Physics.Weight;
        //    ////var str = StatsComponent.GetStatOrDefault(parent, Stat.Types.Strength, 1);
        //    //AttributesComponent atts = parent.GetComponent<AttributesComponent>();
        //    //var str = atts.Attributes.FirstOrDefault(a => a.ID == Attribute.Types.Strength).Value;
        //    var ratio = StatMaxWeight.GetRatio(parent);
        //    var val = 1 - ratio;
        //    val *= .5f;// 0.1f;

        //    //ResourceDef stamina = parent.GetComponent<ResourcesComponent>().Resources[ResourceDef.ResourceTypes.Stamina];
        //    var stamina = parent.GetResource(ResourceDef.Stamina);

        //    stamina.Add(-val);
        //    if (stamina.Value <= 0)
        //        parent.Interact(new InteractionThrow(true));
            
        //        //parent.GetComponent<WorkComponent>().Perform(parent, new Interactions.DropCarried(), TargetArgs.Null);
        //}

        //AttributeStat.ValueModifier StrModifier;
        //private void StartCarrying(GameObject parent)
        //{
        //    //this.AnimationHaul = Graphics.AnimationCollection.Hauling;
        //    //this.AnimationHaul = Graphics.Animation.Hauling(parent);
        //    this.AnimationHaul = new Animation(AnimationDef.Haul);

        //    //parent.Body.CrossFade(this.AnimationHaul, false, 10);
        //    parent.CrossFade(this.AnimationHaul, false, 10);

        //    StatsComponentNew.AddModifier(parent, Stat.Types.WalkSpeed, this.Modifier);
        //    //StatsComponent.SetStat(parent, Stat.Types.WalkSpeed, v => v * 0.5f);
        //    //StatsComponent stats = parent.GetComponent<StatsComponent>();
        //    //float walk;
        //    //stats.BaseStats.TryGetValue(Stat.Types.WalkSpeed, out walk);

        //    //// award strength
        //    //var str = AttributesComponent.GetAttribute(parent, Attribute.Types.Strength);
        //    ////str.AddToProgress(parent, 0.5f);
        //    //var ratio = StatMaxWeight.GetRatio(parent);
        //    //var gain = 1 - ratio;
        //    //str.GainRate += gain;

        //    var str = AttributesComponent.GetAttribute(parent, AttributeDef.Strength);
        //    if (str == null)
        //        return;
        //    //var ratio = StatMaxWeight.GetRatio(parent);
        //    //this.StrModifier = new Attribute.ValueModifier(() => 1 - ratio);
        //    this.StrModifier = new AttributeStat.ValueModifier(() => 1 - StatMaxWeight.GetRatio(parent));
        //    str.Modifiers.Add(this.StrModifier);
        //}

        //private void StopCarrying(GameObject parent)
        //{
        //    //parent.Body.FadeOutAnimation(this.AnimationHaul);
        //    this.AnimationHaul.FadeOut();

        //    StatsComponent.SetStat(parent, Stat.Types.WalkSpeed, v => v * 2f);
        //    StatsComponentNew.RemoveModifier(parent, Stat.Types.WalkSpeed, this.Modifier);

        //    // stop awarding strength
        //    var str = AttributesComponent.GetAttribute(parent, AttributeDef.Strength);
        //    if (str == null)
        //        return;
        //    str.Modifiers.Remove(this.StrModifier);
        //    //var ratio = StatMaxWeight.GetRatio(parent);
        //    //var gain = 1 - ratio;
        //    //str.GainRate -= gain;
            
        //}



        public bool Carry(GameObject parent, GameObjectSlot objSlot)
        {
            return this.Carry(parent.Net, parent, objSlot);
        }
        public bool Carry(IObjectProvider net, GameObject parent, GameObjectSlot objSlot)
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
                if (existing.IDType == objSlot.Object.IDType)
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
            float w = t.Physics.Weight;
            float maxW = StatsComponentNew.GetStatValueOrDefault(a, Stat.Types.MaxWeight, 0);
            return maxW >= w;
        }


        bool Throw(IObjectProvider net, Vector3 velocity, GameObject parent, bool all)
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


            newobj.Global = parent.Global + new Vector3(0, 0, parent.Physics.Height);
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
        static public GameObjectSlot GetHolding(GameObject parent)
        {
            return parent.GetComponent<HaulComponent>().Holding;
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
            this.AnimationHaul.Write(w);

        }
        public override void Read(System.IO.BinaryReader r)
        {
            //this.Slot.Read(r);
            //this.Index = r.ReadInt32();
            this.AnimationHaul.Read(r);

        }
        
        internal override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.AnimationHaul.Save("AnimationHaul"));
        }
        internal override void Load(SaveTag save)
        {
            //save.TryGetTag("Slot", tag => this.Slot.Load(tag));
            //this.Index = save.TagValueOrDefault<int>("Index", 0);
            save.TryGetTag("AnimationHaul", this.AnimationHaul.Load);

        }

        public override void Draw(MySpriteBatch sb, GameObject parent, Camera camera)
        {
            return;
            //var slot = this.GetSlot();
            //if (slot.Object == null)
            //    return;
            //var body = slot.Object.Body;
            //var map = parent.Map;
            //Vector2 direction = parent.Transform.Direction;
            //Vector2 finalDir = Coords.Rotate(camera, direction);
            //var global = parent.Global + Vector3.UnitZ * parent.Physics.Height;
            //float depth = global.GetDrawDepth(map, camera);
            //SpriteEffects sprfx = (finalDir.X - finalDir.Y) < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            //byte skylight, blocklight;
            //parent.Map.GetLight(parent.Global.RoundXY(), out skylight, out blocklight);
            //var skyColor = map.GetAmbientColor() * ((skylight + 1) / 16f); //((skylight) / 15f);
            //skyColor.A = 255;
            //var blockColor = Color.Lerp(Color.Black, Color.White, (blocklight) / 15f);
            //var fog = camera.GetFogColor((int)parent.Global.Z);
            //var test = camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, new Rectangle(0, 0, 0, 0), Vector2.Zero);
            //var finalpos = new Vector2(test.X, test.Y) + (body.OriginGroundOffset * camera.Zoom); //screenLoc + 
            //body.DrawTree(slot.Object, sb, finalpos, skyColor, blockColor, Color.White, fog, 0, camera.Zoom, (int)camera.Rotation, sprfx, 1f, depth);
        }

        internal override void HandleRemoteCall(GameObject gameObject, Message.Types type, System.IO.BinaryReader r)
        {
            switch(type)
            {
                case Message.Types.Carry:
                    var objid = r.ReadInt32();
                    var obj = gameObject.Net.GetNetworkObject(objid);
                    this.Carry(gameObject, obj.ToSlotLink());
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
            return parent.GetComponent<HaulComponent>().Carry(parent, obj.ToSlotLink());
        }

        //static public bool Carry(GameObject pareng )
    }
}
