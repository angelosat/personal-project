﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Start_a_Town_;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public partial class WorkplaceManager : TownComponent
    {
        internal IEnumerable<T> GetShops<T>() where T : Workplace
        {
            return this.Shopss.OfType<T>();
        }

        static WorkplaceManager()
        {
            Packets.Init();
            Tavern.Init();
        }
        
        int CurrentShopID = 1;
        public int GetNextShopID()
        {
            return this.CurrentShopID++;
        }

        public WorkplaceManager(Town town) : base(town)
        {
        }

        public override string Name => "Shops";
        Dictionary<int, Workplace> Shops = new();
        readonly HashSet<Workplace> Shopss = new();
        internal override IEnumerable<Tuple<Func<string>, Action>> OnQuickMenuCreated()
        {
            var win = new Lazy<Window>(() => this.GetUIManager().ToWindow("Shops"));
            yield return new Tuple<Func<string>, Action>(()=>"Businesses", () => win.Value.Toggle());
        }
        
        public bool ShopExists(Workplace shop)
        {
            return this.Shops.ContainsKey(shop.ID);
        }
        public IEnumerable<Workplace> GetShops()
        {
            foreach (var shop in this.Shops.Values)
                yield return shop;
        }
        public Workplace GetShop(int shopid)
        {
            if (shopid < 0)
                return null;
            return this.Shops[shopid];
        }
        public Workplace GetShop(IntVec3 facility)
        {
            return this.Shopss.FirstOrDefault(s => s.GetFacilities().Any(f => f == facility));
        }
        public Workplace FindShop(Stockpile stockpile)
        {
            return this.Shopss.FirstOrDefault(sh => sh.HasStockpile(stockpile.ID));
        }
        public T FindShop<T>(Stockpile stockpile) where T : Workplace
        {
            return this.Shopss.FirstOrDefault(sh => sh.HasStockpile(stockpile.ID)) as T;
        }
        
        public void AddShop(Workplace shop)
        {
            this.Shopss.Add(shop);
            this.Shops.Add(shop.ID, shop);
            this.Town.Net.EventOccured(Components.Message.Types.ShopsUpdated, shop);
        }
        public void RemoveShop(int shopid)
        {
            var shop = this.Shops[shopid];
            this.Shopss.Remove(shop);
            this.Shops.Remove(shopid);
            this.Town.Net.EventOccured(Components.Message.Types.ShopsUpdated, shop);
        }
        public Workplace GetShop(Actor worker)
        {
            return this.Shopss.FirstOrDefault(s => s.HasWorker(worker));
        }
       
        public T GetShop<T>(Actor worker) where T : Workplace
        {
            return this.Shopss.FirstOrDefault(s => s.HasWorker(worker)) as T;
        }
        public T GetShop<T>(int shopid) where T : Workplace
        {
            return this.Shops[shopid] as T;
        }
        internal void ToggleWorker(Actor a, Workplace shop)
        {
            Packets.SendPlayerAssignWorkerToShop(a.Net, a.Net.GetPlayer(), a, shop);
        }
        internal override void OnTargetSelected(IUISelection info, ISelectable selected)
        {
            if (selected is Stockpile stockpile)
            {
                var net = stockpile.Town.Net;

                var control = new Lazy<Control>(
                    () =>
                    new GroupBox().AddControlsVertically(
                        new Button("None", () => Packets.SendPlayerAddStockpileToShop(net, net.GetPlayer().ID, stockpile.Town.ShopManager.FindShop(stockpile)?.ID ?? -1, stockpile.ID), UIListWidth),
                        this.CreateUIShopList(sh => Packets.SendPlayerAddStockpileToShop(net, net.GetPlayer().ID, sh.ID, stockpile.ID)))
                    .ToPanelLabeled("Select shop").HideOnAnyClick());

                info.AddTabAction("Shop", () => control.Value.SetLocation(UIManager.Mouse).Toggle());

                info.AddInfo(new Label(() => string.Format("Shop: {0}", this.Shopss.FirstOrDefault(sh => sh.HasStockpile(stockpile.ID))?.Name ?? "")));
            }
            else if (selected is TargetArgs target)
            {
                if (target.Type == TargetType.Position)
                {
                    var block = target.Block;
                    if (this.Shopss.Any(s => s.IsAllowed(block)))
                    {
                        info.AddTabAction("Shopp", () =>
                            this.GetUIShopListWithNoneOption<Workplace>(s => this.PlayerAssignCounter(s, target.Global), w => w.IsAllowed(block)).SetLocation(UIManager.Mouse).Toggle());
                    }
                }
            }
        }
        const int UIListWidth = 250;

        public Control GetUIShopListWithNoneOption<T>(Action<Workplace> selectAction, Func<T, bool> filter) where T : Workplace // TODO make this a singleton
        {
            var box = new GroupBox();
            void action(Workplace wp)
            {
                selectAction(wp);
                box.Hide();
            };
            return box.AddControlsVertically(
                    new Button("None", () => action(null), UIListWidth),
                    this.CreateUIShopList(action, filter))
                .ToPanelLabeled("Select shop").HideOnAnyClick();
        }
        private ListBoxNoScroll<Workplace, Button> CreateUIShopList(Action<Workplace> selectAction, Func<Workplace, bool> filter = null)
        {
            return this.CreateUIShopList<Workplace>(selectAction, filter);
        }

        private ListBoxNoScroll<T, Button> CreateUIShopList<T>(Action<T> selectAction, Func<T, bool> filter) where T : Workplace
        {
            //var shoplist = new ListBoxNew<T, Button>(UIListWidth, Button.DefaultHeight * 8, s => new Button(s.Name, () => selectAction?.Invoke(s)));
            var shoplist = new ListBoxNoScroll<T, Button>(s => new Button(s.Name, () => selectAction?.Invoke(s)));

            shoplist.OnGameEventAction = e =>
            {
                switch (e.Type)
                {
                    case Components.Message.Types.ShopsUpdated:
                        var shop = e.Parameters[0] as T;
                        if (this.Shopss.Contains(shop))
                            shoplist.AddItems(shop);
                        else
                            shoplist.RemoveItems(shop);
                        break;

                    default:
                        break;
                }
            };
            shoplist.OnShowAction = () =>
            {
                shoplist.Clear();
                shoplist.AddItems(this.Shops.Values.OfType<T>().Where(v=>filter?.Invoke(v) ?? true).ToArray());
            };
            return shoplist;
        }

        Control GetUIManager()
        {
            var box = new GroupBox();
            var boxList = new GroupBox();

            var shopUI = new Lazy<(Control control, Action<Workplace> refresh)>(Workplace.CreateUI);
            var win = new Lazy<Window>(() => shopUI.Value.control.ToWindow("Shop"));
            

            var shoplist = new TableScrollableCompactNewNew<Workplace>()
                .AddColumn(new(), "name", 200, sh => new Label(sh.Name, ()=> {
                        shopUI.Value.refresh(sh);
                }), 0)
                .AddColumn(new(), "delete", Icon.Cross.SourceRect.Width,
                    w => IconButton.CreateSmall(Icon.Cross,
                        () => MessageBox.CreateDialogue("Warning!", $"{w.Name} will be deleted. Are you sure?",
                            () => Packets.SendPlayerDeleteShop(this.Town.Net, this.Town.Net.GetPlayer(), w.ID))));
            shoplist.OnGameEventAction = e =>
            {
                switch (e.Type)
                {
                    case Components.Message.Types.ShopsUpdated:
                        var shop = e.Parameters[0] as Workplace;
                        if (this.Shopss.Contains(shop))
                            shoplist.AddItems(shop);
                        else
                            shoplist.RemoveItems(shop);
                        break;

                    default:
                        break;
                }
            };
            shoplist.OnShowAction = () =>
            {
                shoplist.ClearItems();
                shoplist.AddItems(this.Shops.Values.ToArray());
            };
            shoplist.AddItems(this.Shops.Values.ToArray());
            var net = this.Town.Net;
            var selectTypeMenu = selectShopType(t=> Packets.SendPlayerCreateShop(net, net.GetPlayer().ID, t));
            var btnNew = new Button("New", () => selectTypeMenu.Toggle(UIManager.Mouse));
            boxList.AddControlsVertically(shoplist, btnNew);
            box.AddControlsHorizontally(boxList, shopUI.Value.control);
            return box;

            Control selectShopType(Action<Type> callback)
            {
                //var list = new ListBoxNew<Type, Button>(150, Button.DefaultHeight * 2, t=>new Button(t.Name, ()=>callback(t)));
                var list = new ListBoxNoScroll<Type, Button>(t => new Button(t.Name, () => callback(t)));
                list.AddItems(typeof(Shop), typeof(Tavern));
                return list.ToContextMenu("Select shop type");
            }
        }
        public void PlayerAssignCounter(Workplace shop, IntVec3 global)
        {
            var net = this.Town.Net;
            Packets.SendPlayerShopAssignCounter(net, net.GetPlayer(), shop, global);
        }
        internal override void ResolveReferences()
        {
            foreach (var wp in this.Shopss)
                wp.ResolveReferences();
        }
        internal override void OnBlocksChanged(IEnumerable<IntVec3> positions)
        {
            foreach (var wp in this.Shopss)
                wp.OnBlocksChanged(positions);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.CurrentShopID.Save("ShopIDSequence"));
            this.Shopss.SaveAbstract(tag, "Shops");
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValueNew("ShopIDSequence", ref this.CurrentShopID);
            this.Shopss.LoadAbstract(tag, "Shops", this);
            this.Shops = this.Shopss.ToDictionary(i => i.ID, i => i);
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this.CurrentShopID);
            this.Shopss.WriteAbstract(w);
        }
        public override void Read(BinaryReader r)
        {
            this.CurrentShopID = r.ReadInt32();
            this.Shopss.ReadListAbstract(r, this);
            this.Shops = this.Shopss.ToDictionary(i => i.ID, i => i);
        }
    }
}
