using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using System.IO;

namespace Start_a_Town_
{
    class EntityCompRefuelable : BlockEntityComp, IPowerSource//, IStorage
    {
        static EntityCompRefuelable()
        {
            Server.RegisterPacketHandler(PacketType.RefuelableChangeFilters, ChangeFilters);
            Client.RegisterPacketHandler(PacketType.RefuelableChangeFilters, ChangeFilters);
        }
        private static void ChangeFiltersSend(IObjectProvider net, Vector3 entity, int[] nodeIndices = null, int[] leafIndices = null)
        {
            var s = net.GetOutgoingStream();
            s.Write(PacketType.RefuelableChangeFilters);
            s.Write(entity);
            s.Write(nodeIndices ?? new int[] { });
            s.Write(leafIndices ?? new int[] { });
        }
        private static void ChangeFilters(IObjectProvider net, BinaryReader r)
        {
            var entity = r.ReadVector3();
            //net.Map.GetBlockEntity(entity).GetComp<EntityCompRefuelable>().ToggleItemFilters(net.GetNetworkObjects(r.ReadIntArray()).ToArray());
            var nodes = r.ReadIntArray();
            var items = r.ReadIntArray();
            net.Map.GetBlockEntity(entity).GetComp<EntityCompRefuelable>().ToggleItemFiltersCategories(nodes);
            net.Map.GetBlockEntity(entity).GetComp<EntityCompRefuelable>().ToggleItemFilters(items);
            if (net is Server)
                ChangeFiltersSend(net, entity, nodes, items);
        }

        readonly StorageSettings FuelSettings;
        //public StorageSettings Settings => this.FuelSettings;
        public Progress Fuel = new();
        readonly List<ItemDefMaterialAmount> StoredFuelItems = new();
        HashSet<FuelDef> AcceptedFuelTypes = new();
        HashSet<ItemSubType> AcceptedItemTypes = new();
        HashSet<int> CurrentAllowedItemTypes = new();
        HashSet<GameObject> _AllPermittedItemTypes;
        HashSet<GameObject> AllPermittedItemTypes
        {
            get
            {
                if (this._AllPermittedItemTypes == null)
                    InitPermittedTypes();
                return this._AllPermittedItemTypes;
            }
        }
        IEnumerable<IGrouping<ItemSubType, GameObject>> AllPermittedItemTypesByGroup;
        private void InitPermittedTypes()
        {
            this._AllPermittedItemTypes = new HashSet<GameObject>();
            var objects = GameObject.Objects;
            foreach (var obj in objects.Values)
            {
                //if (this.CanAccept(obj) && obj.IsHaulable)
                if (
                    this.AcceptedFuelTypes.Contains(obj.Material?.Fuel.Def)
                    && this.AcceptedItemTypes.Contains(obj.GetInfo().ItemSubType)
                    && obj.IsHaulable)
                    this._AllPermittedItemTypes.Add(obj);//.GetInfo().ItemSubType);
            }
            this.AllPermittedItemTypesByGroup = this._AllPermittedItemTypes.GroupBy(o => o.GetInfo().ItemSubType);
        }
        public EntityCompRefuelable SetDefaultFilter(Func<GameObject, bool> filter)
        {
            foreach (var i in this.AllPermittedItemTypes)
                if (filter(i))
                    this.CurrentAllowedItemTypes.Add(i.ID);
            return this;
        }
        public EntityCompRefuelable(int storedFuelCapacity = 100)
        {
            //this.AcceptedFuelTypes = new HashSet<ItemSubType>(fuelTypes);
            //this.StoredFuelCapacity = storedDuelCapacity;
            this.Fuel.Max = storedFuelCapacity;
        }
        public EntityCompRefuelable SetFuelTypes(params FuelDef[] fuelTypes)
        {
            this.AcceptedFuelTypes = new HashSet<FuelDef>(fuelTypes);
            return this;
        }
        public EntityCompRefuelable SetPermittedItemTypes(params ItemSubType[] itemTypes)
        {
            this.AcceptedItemTypes = new HashSet<ItemSubType>(itemTypes);
            //for (int i = 0; i < itemTypes.Length; i++)
            //    this.AcceptedItemTypes[itemTypes[i]] = false;
            return this;
        }
        public bool Accepts(Entity fuel)
        {
            return this.DefaultFilters.Filter(fuel);
            //return this.CurrentAllowedItemTypes.Contains(fuel.ID);
        }
        internal float GetTotalStoredFuel()
        {
            //return this.StoredFuelItems.Sum(o => GameObject.Objects[o.ObjectID].Fuel * o.Amount);
            return this.StoredFuelItems.Sum(o => o.Material.Fuel.Value * o.Amount);
        }
        private void StoreFuel(ItemDef iD, Material mat, int actualAmountToAdd)
        {
            var existing = this.StoredFuelItems.FirstOrDefault(o => o.Def == iD && o.Material == mat);
            if (existing == null)
                this.StoredFuelItems.Add(new ItemDefMaterialAmount(iD, mat, actualAmountToAdd));// new ObjectIDAmount(iD, actualAmountToAdd));
            else
                existing.Amount += actualAmountToAdd; //var existing = this.StoredFuelItems.FirstOrDefault(o => o.ObjectID == iD);
            //if (existing == null)
            //    this.StoredFuelItems.Add(new ObjectIDAmount(iD, actualAmountToAdd));
            //else
            //    existing.Amount += actualAmountToAdd;
        }
        public void ConsumePower(IMap map, float amount)
        {
            this.Fuel.Value -= amount;

            if (this.Fuel.Value < this.GetTotalStoredFuel())
            {
                var firstFuel = this.StoredFuelItems[0];
                firstFuel.Amount -= 1;
                if (firstFuel.Amount == 0)
                {
                    this.StoredFuelItems.RemoveAt(0);
                    map.EventOccured(Components.Message.Types.FuelConsumed, this);
                }
            }
        }
        internal override void Remove(IMap map, Vector3 global, BlockEntity parent)
        {
            if (map.Net is Net.Client)
                return;
            foreach (var oa in this.StoredFuelItems.ToList())
            {
                do
                {
                    var matEntity = oa.Create();// GameObject.Objects[oa.ObjectID].Clone();
                    var amountToSpawn = Math.Min(oa.Amount, matEntity.StackMax);
                    if (amountToSpawn == 0)
                        throw new Exception();
                    matEntity.StackSize = amountToSpawn;
                    oa.Amount -= amountToSpawn;
                    map.Net.PopLoot(matEntity, global, Vector3.Zero);
                } while (oa.Amount > 0);
                this.StoredFuelItems.Remove(oa); // just in case
            }
        }
        internal override void OnDrop(GameObject actor, GameObject item, TargetArgs target, int quantity)
        {
            //base.OnDrop(actor, item, target, quantity);
            this.AddFuelNew(item, quantity);
        }
        public override void OnEntitySpawn(BlockEntity entity, IMap map, Vector3 global)
        {
            var material = map.GetBlockMaterial(global);
            this.Fuel.Value += material.Fuel.Value;
        }
        private void AddFuelNew(GameObject item, int quantity)
        {
            //var e = actor.Map.GetBlockEntity<BlockSmeltery.BlockSmelteryEntity>(target.Global);
            var amount = quantity == -1 ? item.StackMax : quantity;
            if (amount == 0)
                throw new Exception();
            var fuel = item.Fuel;
            var fuelMissing = this.Fuel.Max - this.Fuel.Value;
            var desiredAmount = (int)(fuelMissing / fuel);
            var actualAmountToAdd = Math.Min(amount, desiredAmount);
            var actualFuelToAdd = actualAmountToAdd * fuel;
            item.StackSize -= actualAmountToAdd;

            // add fuel immediately or store item and consume it when power is requested?
            this.Fuel.Value += actualFuelToAdd;
            //this.StoreFuel(item.ID, actualAmountToAdd);
            this.StoreFuel(item.Def, item.PrimaryMaterial, actualAmountToAdd);

            item.Map.EventOccured(Components.Message.Types.FuelConsumed, this);
        }



        internal override void GetSelectionInfo(IUISelection info, IMap map, Vector3 vector3)
        {
            var bar = new Bar()
            {
                ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, this.Fuel.Percentage),
                Object = this.Fuel,
                Name = "Fuel: ",
                //LeftClickAction = () => ShowPermittedTypesUI(map, vector3)
            };
            bar.TextFunc = () => bar.Percentage.ToString("##0%");
            info.AddInfo(bar);

            var boxcontents = new ListBox<ItemDefMaterialAmount, Label>(150, Label.DefaultHeight * 3);
            void refreshContents()
            {
                //boxcontents.Build(this.StoredFuelItems, oa => string.Format("{0} x{1}", oa.ToString(), oa.Amount), null);
                boxcontents.Build(this.StoredFuelItems);
            }
            refreshContents();
            boxcontents.OnGameEventAction = e =>
            {
                if (e.Type == Components.Message.Types.FuelConsumed)
                    if (e.Parameters[0] == this)
                        refreshContents();
            };
            info.AddInfo(boxcontents);

            //info.AddTabAction("Fuel", () => DefaultFilters.Value.GetControl().SetLocation(UIManager.Mouse).Toggle());
            info.AddTabAction("Fuel", () =>
            {
                //if (WindowPermittedTypes == null)
                //    WindowPermittedTypes =
                //DefaultFilters.GetControl((n, l) => ChangeFiltersSend(map.Net, vector3, n, l)).SetLocation(UIManager.Mouse).Toggle();
                DefaultFilters.GetControl((n, l) => ChangeFiltersSend(map.Net, vector3, n, l)).ToWindow("Select permitted fuel").Toggle();
            });
        }
        //static readonly Window WindowPermittedTypes;
        //[Obsolete]
        //private void ShowPermittedTypesUI(IMap map, Vector3 entityGlobal)
        //{
        //    throw new Exception();
        //    if (WindowPermittedTypes == null)
        //    {
        //        //var groupboxall = new GroupBox();
        //        var permitted = AllPermittedItemTypes;
        //        var listtest =
        //            new ListBoxCollapsible<GameObject, CheckBoxNew>(200, 200);//); //
        //        var grouping = this.AllPermittedItemTypesByGroup; //permitted.GroupBy(o => o.GetInfo().ItemSubType);
        //        foreach (var itype in grouping)
        //        {
        //            var inode = new ListBoxCollapsibleNode<GameObject, CheckBoxNew>(itype.Key.ToString())
        //            {
        //                OnNodeControlInit = (catTickBox) =>
        //                {
        //                    catTickBox.TickedFunc = () => itype.All(b => this.CurrentAllowedItemTypes.Contains(b.ID));
        //                    catTickBox.LeftClickAction = () =>
        //                    {
        //                        this.ToggleCategory(map, entityGlobal, itype);
        //                    };
        //                }
        //            };
        //            foreach (var i in itype)
        //                inode.AddLeaf(i);
        //            listtest.AddNode(inode);
        //        }

        //        listtest.Build((i, itemTickBox) =>
        //        {
        //            itemTickBox.TickedFunc = () =>
        //            (listtest.Tag as EntityCompRefuelable).CurrentAllowedItemTypes.Contains(i.ID);

        //        });
        //        listtest.CallBack = (e) => SendToggleItemFilters(map, entityGlobal, e);
        //        listtest.Tag = this;
        //        listtest.OnGameEventAction = e =>
        //        {
        //            if (e.Type == Components.Message.Types.SelectedChanged)
        //            {
        //                var target = (TargetArgs)e.Parameters[0];
        //                var compRefuel = target.GetBlockEntity()?.GetComp<EntityCompRefuelable>();
        //                if (compRefuel != null)
        //                {
        //                    listtest.Tag = compRefuel;
        //                    listtest.CallBack = (y) => SendToggleItemFilters(target.Map, target.Global, y);
        //                }
        //            }
        //        };
        //        //groupboxall.AddControls(listtest);
        //        WindowPermittedTypes = new Window("Permitted item types", listtest);// table);
        //    }
        //    WindowPermittedTypes.Toggle();
        //}
       
        private void ToggleCategory(IMap map, Vector3 global, IGrouping<ItemSubType, GameObject> itype)
        {
            var enabled = itype.Where(i => this.CurrentAllowedItemTypes.Contains(i.ID)).ToArray();
            var disabled = itype.Except(enabled).ToArray();
            var disabledCount = disabled.Length;
            var enabledCount = enabled.Length;
            if (enabledCount == 0 || disabledCount == 0)
                SendToggleItemFilters(map, global, itype.ToArray());
            else
            {
                var toSend = disabledCount >= enabledCount ? enabled : disabled;
                SendToggleItemFilters(map, global, toSend.ToArray());
            }
        }

        static void SendToggleItemFilters(IMap map, Vector3 entityGlobal, params GameObject[] filters)
        {
            ChangeFiltersSend(map.Net, entityGlobal, filters.Select(i => i.ID).ToArray());
        }
        private void ToggleItemFiltersCategories(int[] categoryIndices)
        {
            var indices = categoryIndices;
            foreach (var i in indices)
            {
                var c = DefaultFilters.GetNodeByIndex(i);
                var all = c.GetAllDescendantLeaves();
                var minor = all.GroupBy(a => a.Enabled).OrderBy(a => a.Count()).First();
                foreach (var f in minor)
                    f.Enabled = !minor.Key;
            }
        }
        private void ToggleItemFilters(int[] gameObjects)
        {
            var indices = gameObjects;
            foreach (var i in indices)
            {
                var f = DefaultFilters.GetLeafByIndex(i);
                f.Enabled = !f.Enabled;
            }
        }

        public bool HasAvailablePower(float amount)
        {
            return this.Fuel.Value >= amount;
        }

        public float GetRemaniningPower()
        {
            return this.Fuel.Value;
        }

        public override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.Fuel.Save("Fuel"));
            tag.Add(this.CurrentAllowedItemTypes.Save("CurrentAllowedItemTypes"));
            tag.Add(this.StoredFuelItems.SaveNewBEST("StoredFuelItems"));
        }

        public override void Load(SaveTag tag)
        {
            this.Fuel.Load(tag["Fuel"]);
            this.CurrentAllowedItemTypes.Load(tag, "CurrentAllowedItemTypes");
            //this.StoredFuelItems.TryLoadMutable(tag, "StoredFuelItems");
            this.StoredFuelItems.TryLoadMutable(tag, "StoredFuelItems");
        }
        public override void Write(BinaryWriter w)
        {
            this.Fuel.Write(w);
            w.Write(this.CurrentAllowedItemTypes);
            w.Write(this.StoredFuelItems);
        }
        public override ISerializable Read(BinaryReader r)
        {
            this.Fuel.Read(r);
            this.CurrentAllowedItemTypes = r.ReadIntCollection<HashSet<int>>();
            this.StoredFuelItems.ReadMutable(r);
            return this;
        }

        readonly StorageFilterCategoryNew DefaultFilters =
            new StorageFilterCategoryNew("Wood")
                .AddChildren(Def.Database.Values.OfType<ItemDef>()
                    .Where(d => d.DefaultMaterialType == MaterialType.Wood)
                    .Select(d => new StorageFilterCategoryNew(d.Label).AddLeafs(d.DefaultMaterialType.SubTypes.Select(m => new StorageFilterNew(d, m)))));

        //StorageFilterCategoryNew DefaultFiltersOld = //new Lazy<StorageFilterCategoryNew>(() =>
        //     new StorageFilterCategoryNew("Wood")
        //        .AddChildren(
        //            new StorageFilterCategoryNew("Logs")
        //                .AddLeafs(RawMaterialDef.Logs.PreferredMaterialType.SubTypes.Select(m => new StorageFilterNew(RawMaterialDef.Logs, m)).ToArray()),
        //            new StorageFilterCategoryNew("Planks")
        //                .AddLeafs(RawMaterialDef.Planks.PreferredMaterialType.SubTypes.Select(m => new StorageFilterNew(RawMaterialDef.Planks, m)).ToArray()));//);
    }
}
