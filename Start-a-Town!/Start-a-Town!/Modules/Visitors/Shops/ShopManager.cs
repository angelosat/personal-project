using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    public class ShopManager : TownComponent
    {
        private class Packets
        {
            static public void Init()
            {
                return;
                Server.RegisterPacketHandler(PacketType.PlayerCreateShop, ReceivePlayerCreateShop);
                Client.RegisterPacketHandler(PacketType.PlayerCreateShop, ReceivePlayerCreateShop);

                Server.RegisterPacketHandler(PacketType.PlayerAddStockpileToShop, ReceivePlayerAddStockpileToShop);
                Client.RegisterPacketHandler(PacketType.PlayerAddStockpileToShop, ReceivePlayerAddStockpileToShop);

                Server.RegisterPacketHandler(PacketType.PlayerAssignWorkerToShop, HandlePlayerAssignWorkerToShop);
                Client.RegisterPacketHandler(PacketType.PlayerAssignWorkerToShop, HandlePlayerAssignWorkerToShop);

                Server.RegisterPacketHandler(PacketType.PlayerShopAssignCounter, ReceivePlayerShopAssignCounter);
                Client.RegisterPacketHandler(PacketType.PlayerShopAssignCounter, ReceivePlayerShopAssignCounter);
            }
            static public void SendPlayerShopAssignCounter(IObjectProvider net, PlayerData player, Shop shop, IntVec3 global)
            {
                var w = net.GetOutgoingStream();
                w.Write(PacketType.PlayerShopAssignCounter);
                w.Write(player.ID);
                w.Write(shop?.ID ?? -1);
                w.Write(global);
            }
            static void ReceivePlayerShopAssignCounter(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var manager = net.Map.Town.ShopManager;
                var shop = manager.GetShop<Shop>(r.ReadInt32());
                var global = r.ReadVector3();
                if (shop != null)
                    shop.Counter = global.Z < 0 ? null : global;
                else
                {
                    var prevShop = manager.Shopss.FirstOrDefault(s => s.Counter.Value == new IntVec3(global));
                    if (prevShop != null)
                        prevShop.Counter = null;
                }
                if (net is Server)
                    SendPlayerShopAssignCounter(net, player, shop, global);
            }

            static public void SendPlayerAssignWorkerToShop(IObjectProvider net, PlayerData player, Actor actor, Shop shop)
            {
                var w = net.GetOutgoingStream();
                w.Write(PacketType.PlayerAssignWorkerToShop);
                w.Write(player.ID);
                w.Write(actor.InstanceID);
                w.Write(shop.ID);
            }
            private static void HandlePlayerAssignWorkerToShop(IObjectProvider net, BinaryReader r)
            {
                var playerID = r.ReadInt32();
                var actorID = r.ReadInt32();
                var shopID = r.ReadInt32();
                var manager = net.Map.Town.ShopManager;
                var actor = net.GetNetworkObject(actorID) as Actor;
                var shop = manager.GetShop(shopID) as Shop;
                shop.AddWorker(actor);
                if (net is Server)
                    SendPlayerAssignWorkerToShop(net, net.GetPlayer(playerID), actor, shop);
            }

            static public void SendPlayerAddStockpileToShop(IObjectProvider net, int playerID, int shopID, int stockpileID)
            {
                if (shopID < 0)
                    return;
                var w = net.GetOutgoingStream();
                w.Write(PacketType.PlayerAddStockpileToShop);
                w.Write(playerID);
                w.Write(shopID);
                w.Write(stockpileID);
            }
            private static void ReceivePlayerAddStockpileToShop(IObjectProvider net, BinaryReader r)
            {
                var playerID = r.ReadInt32();
                var shopid = r.ReadInt32();
                var stockpileid = r.ReadInt32();
                var shopmanager = net.Map.Town.ShopManager;
                var stockpilemanager = net.Map.Town.StockpileManager;
                var stockpile = stockpilemanager.GetStockpile(stockpileid);
                //var shop = shopid < 0 ? null : shopmanager.GetShop(shopid);
                var shop = shopmanager.GetShop(shopid) as Shop;
                shop.AddStockpile(stockpile);

                if (net is Server)
                    SendPlayerAddStockpileToShop(net, playerID, shopid, stockpileid);
            }

            static public void SendPlayerCreateShop(IObjectProvider net, int playerID, int shopID = 0)
            {
                var w = net.GetOutgoingStream();
                w.Write(PacketType.PlayerCreateShop);
                w.Write(playerID);
                w.Write(shopID);
            }
            private static void ReceivePlayerCreateShop(IObjectProvider net, BinaryReader r)
            {
                var playerID = r.ReadInt32();
                var shopid = r.ReadInt32();
                var manager = net.Map.Town.ShopManager;

                if (net is Client)
                    manager.CurrentShopID = shopid;
                var id = manager.GetNextShopID();
                var shop = new Shop(manager, id);
                manager.AddShop(shop);
                if (net is Server)
                    SendPlayerCreateShop(net, playerID, shop.ID);
            }
        }

        static ShopManager()
        {
            Packets.Init();
        }
        
        int CurrentShopID = 1;
        public int GetNextShopID()
        {
            return this.CurrentShopID++;
        }

        public ShopManager(Town town) : base(town)
        {
        }
        

        public override string Name => "Shops";
        Dictionary<int, Shop> Shops = new();
        HashSet<Shop> Shopss = new();
        internal override IEnumerable<Tuple<string, Action>> OnQuickMenuCreated()
        {
            var win = new Lazy<Window>(() => this.GetUIManager().ToWindow("Shops"));
            yield return new Tuple<string, Action>("Shops", () => win.Value.Toggle());
        }
        
        public bool ShopExists(Shop shop)
        {
            return this.Shops.ContainsKey(shop.ID);
        }
        public IEnumerable<Shop> GetShops()
        {
            foreach (var shop in this.Shops.Values)
                yield return shop;
        }
        public Shop GetShop(int shopid)
        {
            if (shopid < 0)
                return null;
            return this.Shops[shopid];
        }
        public Shop FindShop(int stockpileID)
        {
            return this.Shopss.FirstOrDefault(sh => sh.HasStockpile(stockpileID));
        }
        public void AddShop(Shop shop)
        {
            this.Shopss.Add(shop);
            this.Shops.Add(shop.ID, shop);
            this.Town.Net.EventOccured(Components.Message.Types.ShopsUpdated, shop);
        }
        public Shop GetShop(Actor worker)
        {
            return this.Shopss.FirstOrDefault(s => s.HasWorker(worker));
        }
        internal void ToggleWorker(Actor a, Shop shop)
        {
            Packets.SendPlayerAssignWorkerToShop(a.Net, a.Net.GetPlayer(), a, shop);
        }
        internal override void OnTargetSelected(IUISelection info, ISelectable target)
        {
            if (target is not Stockpile stockpile)
                return;

            var net = stockpile.Town.Net;
            //var control = new Lazy<Control>(() => this.CreateUIShopList(sh => Packets.SendPlayerAddStockpileToShop(net, net.GetPlayer().ID, sh.ID, stockpile.ID)).ToPanelLabeled("Select shop").HideOnAnyClick());

            var control = new Lazy<Control>(
                () => 
                new GroupBox().AddControlsVertically(
                    new Button("None", () => Packets.SendPlayerAddStockpileToShop(net, net.GetPlayer().ID, stockpile.Town.ShopManager.FindShop(stockpile.ID)?.ID ?? -1, stockpile.ID), UIListWidth),
                    this.CreateUIShopList(sh => Packets.SendPlayerAddStockpileToShop(net, net.GetPlayer().ID, sh.ID, stockpile.ID)))
                .ToPanelLabeled("Select shop").HideOnAnyClick());

            info.AddTabAction("Shop", () => control.Value.SetLocation(UIManager.Mouse).Toggle());

            info.AddInfo(new Label(() => string.Format("Shop: {0}", this.Shopss.FirstOrDefault(sh => sh.HasStockpile(stockpile.ID))?.Name ?? "")));
        }
        const int UIListWidth = 150;

        public Control GetUIShopListWithNoneOption(Action<Shop> selectAction) // TODO make this a singleton
        {
            return new GroupBox().AddControlsVertically(
                    new Button("None", () => selectAction(null), UIListWidth),
                    this.CreateUIShopList(selectAction))
                .ToPanelLabeled("Select shop").HideOnAnyClick();
        }
        private ListBoxNew<Shop, Button> CreateUIShopList(Action<Shop> selectAction = null)
        {
            var shoplist = new ListBoxNew<Shop, Button>(UIListWidth, Button.DefaultHeight * 8, s => new Button(s.Name, () => selectAction?.Invoke(s)));
            shoplist.OnGameEventAction = e =>
            {
                switch (e.Type)
                {
                    case Components.Message.Types.ShopsUpdated:
                        var shop = e.Parameters[0] as Shop;
                        if (this.Shopss.Contains(shop))
                            shoplist.AddItems(shop);
                        else
                            shoplist.RemoveItems(shop);
                        break;

                    default:
                        break;
                }
            };
            //shoplist.AddItems(this.Shops.Values.ToArray());
            shoplist.OnShowAction = () =>
            {
                shoplist.Clear();
                shoplist.AddItems(this.Shops.Values.ToArray());
            };
            return shoplist;
        }

        Control GetUIManager()
        {
            var box = new GroupBox();
            var boxList = new GroupBox();

            var shopUI = new Lazy<(Control control, Action<Shop> refresh)>(Shop.CreateUI);
            var win = new Lazy<Window>(() => shopUI.Value.control.ToWindow("Shop"));
            ListBoxNew<Shop, Button> shoplist = CreateUIShopList(sh=>
            {
                shopUI.Value.refresh(sh);
                win.Value.Title = sh.Name;
                win.Value.Show();
            });

            shoplist.AddItems(this.Shops.Values.ToArray());
            var net = this.Town.Net;
            var btnNew = new Button("New", () => Packets.SendPlayerCreateShop(net, net.GetPlayer().ID), shoplist.Width);
            boxList.AddControlsVertically(shoplist, btnNew);
            box.AddControls(boxList);
            return box;
        }
        public void PlayerAssignCounter(Shop shop, IntVec3 global)
        {
            var net = this.Town.Net;
            Packets.SendPlayerShopAssignCounter(net, net.GetPlayer(), shop, global);
        }

        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.CurrentShopID.Save("ShopIDSequence"));
            tag.Add(this.Shopss.SaveNewBEST("Shops"));
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValueNew("ShopIDSequence", ref this.CurrentShopID);
            this.Shopss.TryLoadList<HashSet<Shop>, Shop>(tag, "Shops", this);
            this.Shops = this.Shopss.ToDictionary(i => i.ID, i => i);
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this.CurrentShopID);
            w.Write(this.Shopss);
        }
        public override void Read(BinaryReader r)
        {
            this.CurrentShopID = r.ReadInt32();
            this.Shopss = new(r.ReadList<Shop>(this));
            this.Shops = this.Shopss.ToDictionary(i => i.ID, i => i);
        }
    }
}
