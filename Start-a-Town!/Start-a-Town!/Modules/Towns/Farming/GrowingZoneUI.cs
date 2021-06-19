using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.Net.Packets;

namespace Start_a_Town_.Towns.Farming
{
    class GrowingZoneUI : GroupBox
    {
        static Dictionary<GrowingZone, Window> OpenWindows = new Dictionary<GrowingZone, Window>();

        GrowingZone Farmland;
        Panel PanelSeeds, PanelButtons;
        //Slot SlotSeed;
        GameObjectSlot Seed;
        CheckBoxNew ChkHarvesting, ChkPlanting;

        public GrowingZoneUI(GrowingZone farmland)
        {
            this.Farmland = farmland;
            this.PanelSeeds = new Panel() { AutoSize = true };

            var seedObjects = GameObject.Objects.Values.Where(foo => foo.GetComponent<SeedComponent>() != null).Select(f => f.ToSlotLink());
            var slotGrid = new SlotGrid(seedObjects.ToList(), 4, s =>
            {
                s.LeftClickAction = () => PacketFarmSetSeed.Send(PlayerOld.Actor.RefID, this.Farmland.ID, (int)s.Tag.Object.IDType);
            });
            //throw new Exception();
            this.Seed = GameObjectSlot.Empty;// this.Farmland.SeedType.ToSlotLink();
            var slotseed = new Slot() { Tag = this.Seed };
            slotseed.RightClickAction = () =>
            {
                SetSeed(null);
            };
            slotseed.LeftClickAction = SelectSeed;
            this.PanelSeeds.AddControls(slotseed);

            var paneloptions = new Panel() { Location = this.PanelSeeds.TopRight, AutoSize = true };//{ Location = this.PanelSeeds.BottomLeft, AutoSize = true };
            this.ChkHarvesting = new CheckBoxNew("Harvesting");
            this.ChkHarvesting.TickedFunc = () => this.Farmland.Harvesting;
            this.ChkHarvesting.LeftClickAction = () => SetHarvesting(this.Farmland, !this.ChkHarvesting.Value);

            this.ChkPlanting = new CheckBoxNew("Planting") { Location = this.ChkHarvesting.BottomLeft };
            this.ChkPlanting.TickedFunc = () => this.Farmland.Planting;
            this.ChkPlanting.LeftClickAction = () => SetPlanting(this.Farmland, !this.ChkPlanting.Value);

            paneloptions.AddControls(this.ChkHarvesting, this.ChkPlanting);

            this.PanelButtons = new Panel(paneloptions.BottomLeft) { AutoSize = true };
            var btnEdit = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Edit farm",// "Add/Remove stockpiles",
                LeftClickAction = () => ToolManager.SetTool(new ToolFarmDesignate(this.Farmland))// { RestrictZ = true, ValidityCheck = IsPositionValid }// this.Create)            

            };
            //var btnDelete = new IconButton()
            //{
            //    BackgroundTexture = UIManager.DefaultIconButtonSprite,
            //    Icon = Icon.X,
            //    HoverFunc = () => "Delete farm",
            //    LeftClickAction = farmland.RequestDelete// FarmlandDelete
            //};
            this.PanelButtons.Controls.Add(btnEdit
                //, btnDelete
                );
            this.PanelButtons.AlignLeftToRight();

            this.Controls.Add(this.PanelButtons, this.PanelSeeds, paneloptions);
        }

       

        private void SetPlanting(GrowingZone farm, bool p)
        {
            throw new Exception();
            //PacketFarmSync.Send(Player.Actor.Net, farm.ID, farm.Name, (int)farm.SeedType.IDType, farm.Harvesting, p);
        }

        private void SetHarvesting(GrowingZone farm, bool p)
        {
            throw new Exception();
            //PacketFarmSync.Send(Player.Actor.Net, farm.ID, farm.Name, (int)farm.SeedType.IDType, p, farm.Planting);
        }

        private void Rename(DialogInput dialog)
        {
            throw new Exception();
            //var input = dialog.Input;
            //dialog.Hide();
            //PacketFarmSync.Send(Player.Actor.Net, this.Farmland.ID, input, (int)this.Farmland.SeedType.IDType, this.Farmland.Harvesting, this.Farmland.Planting);
        }

        private void SelectSeed()
        {
            var itempicker = ItemPicker.Instance;
            itempicker.Show(UIManager.Mouse, obj => obj.HasComponent<SeedComponent>(), (obj) => SetSeed(obj));
        }

        private void SetSeed(GameObject obj)
        {
            PacketFarmSetSeed.Send(PlayerOld.Actor.RefID, this.Farmland.ID, obj == null ? -1 : (int)obj.IDType);
        }

        private void Edit(Vector3 global, int w, int h, bool arg4)
        {
            var begin = global;
            var end = global + new Vector3(w - 1, h - 1, 0);
            Client.Instance.Send(PacketType.FarmlandDesignate, PacketDesignate.Write(PlayerOld.Actor.RefID, this.Farmland.ID, begin, end, arg4));
        }
        private void Edit(Vector3 arg1, Vector3 arg2, bool arg3)
        {
            Client.Instance.Send(PacketType.FarmlandDesignate, PacketDesignate.Write(PlayerOld.Actor.RefID, this.Farmland.ID, arg1 + Vector3.UnitZ, arg2 + Vector3.UnitZ, arg3));
        }




        //internal override void OnGameEvent(GameEvent e)
        //{
        //    switch (e.Type)
        //    {
        //        case Message.Types.FarmSeedChanged:
        //        case Message.Types.FarmUpdated:
        //            //var farm = e.Parameters[0] as Farmland;
        //            var farm = e.Parameters[0] as GrowingZone;
        //            if (farm != this.Farmland)
        //                return;
        //            this.Seed.Object = farm.SeedType;
        //            //this.ChkHarvesting.Value = farm.Harvesting;
        //            //this.ChkPlanting.Value = farm.Planting;
        //            //this.GetWindow().Title = farm.Name;
        //            break;

        //        case Message.Types.FarmRemoved:
        //            //farm = e.Parameters[0] as Farmland;
        //            farm = e.Parameters[0] as GrowingZone;
        //            if (farm == this.Farmland)
        //                this.GetWindow().Hide();
        //            break;

        //        default:
        //            break;
        //    }
        //}

        private Window ToWindow(string name = "")
        {
            Window existing;
            if (OpenWindows.TryGetValue(this.Farmland, out existing))
                return existing;

            var win = base.ToWindow(name);
            win.AutoSize = true;
            win.Movable = true;
            win.Client.Controls.Add(this);


            var dialogRename = new DialogInput("Rename " + this.Farmland.Name, Rename, 16, this.Farmland.Name);
            win.Label_Title.LeftClickAction = () => dialogRename.ShowDialog();
            win.Label_Title.MouseThrough = false;
            win.Label_Title.Active = true;

            win.HideAction = () => OpenWindows.Remove(this.Farmland);
            win.ShowAction = () => OpenWindows[this.Farmland] = win;

            return win;
        }


        internal static Window GetWindow(GrowingZone farmland)
        {
            Window existing;
            if (OpenWindows.TryGetValue(farmland, out existing))
                return existing;
            return new GrowingZoneUI(farmland).ToWindow(farmland.Name);
        }
    }
    //class GrowingZoneUI : GroupBox
    //{
    //    static Dictionary<GrowingZone, Window> OpenWindows = new Dictionary<GrowingZone, Window>();

    //    GrowingZone Farmland;
    //    Panel PanelSeeds, PanelButtons;
    //    //Slot SlotSeed;
    //    GameObjectSlot Seed;
    //    CheckBoxNew ChkHarvesting, ChkPlanting;

    //    public GrowingZoneUI(GrowingZone farmland)
    //    {
    //        this.Farmland = farmland;
    //        this.PanelSeeds = new Panel() { AutoSize = true };//new Rectangle(0, 0, 200, 200));

    //        var seedObjects = GameObject.Objects.Values.Where(foo => foo.GetComponent<SeedComponent>() != null).Select(f => f.ToSlotLink());
    //        //from obj in GameObject.Objects.Values
    //        //where obj.GetComponent<SeedComponent>() != null
    //        //select new GameObjectSlot(obj);
    //        var slotGrid = new SlotGrid(seedObjects.ToList(), 4, s =>
    //        {
    //            s.LeftClickAction = () => PacketFarmSetSeed.Send(Player.Actor.InstanceID, this.Farmland.ID, (int)s.Tag.Object.IDType);
    //        });
    //        //this.PanelSeeds.Controls.Add(slotGrid);

    //        this.Seed = this.Farmland.SeedType.ToSlotLink();// new GameObjectSlot(this.Farmland.SeedType);
    //        var slotseed = new Slot() { Tag = this.Seed };
    //        slotseed.RightClickAction = () =>
    //        {
    //            //slotseed.Tag.Clear();
    //            SetSeed(null);
    //        };
    //        slotseed.LeftClickAction = SelectSeed;
    //        this.PanelSeeds.AddControls(slotseed);

    //        var paneloptions = new Panel() { Location = this.PanelSeeds.TopRight, AutoSize = true };//{ Location = this.PanelSeeds.BottomLeft, AutoSize = true };
    //        this.ChkHarvesting = new CheckBoxNew("Harvesting");
    //        //this.ChkHarvesting.Value = this.Farmland.Harvesting;
    //        this.ChkHarvesting.TickedFunc = () => this.Farmland.Harvesting;
    //        this.ChkHarvesting.LeftClickAction = () => SetHarvesting(this.Farmland, !this.ChkHarvesting.Value);

    //        this.ChkPlanting = new CheckBoxNew("Planting") { Location = this.ChkHarvesting.BottomLeft };
    //        //this.ChkPlanting.Value = this.Farmland.Planting;
    //        this.ChkPlanting.TickedFunc = () => this.Farmland.Planting;
    //        this.ChkPlanting.LeftClickAction = () => SetPlanting(this.Farmland, !this.ChkPlanting.Value);

    //        paneloptions.AddControls(this.ChkHarvesting, this.ChkPlanting);

    //        this.PanelButtons = new Panel(paneloptions.BottomLeft) { AutoSize = true };
    //        var btnEdit = new IconButton()
    //        {
    //            BackgroundTexture = UIManager.DefaultIconButtonSprite,
    //            Icon = new Icon(UIManager.Icons32, 12, 32),
    //            HoverFunc = () => "Edit farm",// "Add/Remove stockpiles",
    //            //LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ToolZoningPositions(this.Edit, () => this.Farmland.Positions.Select(g => g - Vector3.UnitZ).ToList())// { RestrictZ = true, ValidityCheck = IsPositionValid }// this.Create)
    //            //LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ToolZoningPositions(this.Edit, () => this.Farmland.Tasks.Select(g => g.Key - Vector3.UnitZ).ToList())// { RestrictZ = true, ValidityCheck = IsPositionValid }// this.Create)    
    //            //LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ToolZoningPositions(this.Edit, () => this.Farmland.Tasks.Keys.ToList())// { RestrictZ = true, ValidityCheck = IsPositionValid }// this.Create)            
    //            LeftClickAction = () => ToolManager.SetTool( new ToolFarmDesignate(this.Farmland))// { RestrictZ = true, ValidityCheck = IsPositionValid }// this.Create)            

    //        };
    //        //var btnDelete = new IconButton()
    //        //{
    //        //    BackgroundTexture = UIManager.DefaultIconButtonSprite,
    //        //    Icon = Icon.X,
    //        //    HoverFunc = () => "Delete farm",
    //        //    LeftClickAction = farmland.RequestDelete// FarmlandDelete
    //        //};
    //        this.PanelButtons.Controls.Add(btnEdit
    //            //, btnDelete
    //            );
    //        this.PanelButtons.AlignLeftToRight();

    //        this.Controls.Add(this.PanelButtons, this.PanelSeeds, paneloptions);
    //    }

    //    //private void FarmlandDelete()
    //    //{
    //    //    //Client.Instance.Send(PacketType.FarmDelete, PacketInt.Write(this.Farmland.ID));
    //    //    PacketFarmDelete.Send(this.Farmland.Town.Map.Net, this.Farmland.ID);
    //    //}

    //    private void SetPlanting(GrowingZone farm, bool p)
    //    {
    //        //Client.Instance.Send(PacketType.FarmSync, PacketFarmSync.Write(farm.ID, farm.Name, farm.GetSeedID(), farm.Harvesting, p));
    //        PacketFarmSync.Send(Player.Actor.Net, farm.ID, farm.Name, (int)farm.SeedType.IDType, farm.Harvesting, p);
    //    }

    //    private void SetHarvesting(GrowingZone farm, bool p)
    //    {
    //        //Client.Instance.Send(PacketType.FarmSync, PacketFarmSync.Write(farm.ID, farm.Name, farm.GetSeedID(), p, farm.Planting));
    //        PacketFarmSync.Send(Player.Actor.Net, farm.ID, farm.Name, (int)farm.SeedType.IDType, p, farm.Planting);
    //    }

    //    private void Rename(DialogInput dialog)
    //    {
    //        var input = dialog.Input;
    //        dialog.Hide();
    //        PacketFarmSync.Send(Player.Actor.Net, this.Farmland.ID, input, (int)this.Farmland.SeedType.IDType, this.Farmland.Harvesting, this.Farmland.Planting);
    //    }

    //    private void SelectSeed()
    //    {
    //        var itempicker = ItemPicker.Instance;
    //        itempicker.Show(UIManager.Mouse, obj => obj.HasComponent<SeedComponent>(), (obj) => SetSeed(obj));
    //    }

    //    private void SetSeed(GameObject obj)
    //    {
    //        PacketFarmSetSeed.Send(Player.Actor.InstanceID, this.Farmland.ID, obj == null ? -1 : (int)obj.IDType);
    //    }

    //    private void Edit(Vector3 global, int w, int h, bool arg4)
    //    {
    //        var begin = global;
    //        var end = global + new Vector3(w - 1, h - 1, 0);
    //        Client.Instance.Send(PacketType.FarmlandDesignate, PacketDesignate.Write(Player.Actor.InstanceID, this.Farmland.ID, begin, end, arg4));
    //    }
    //    private void Edit(Vector3 arg1, Vector3 arg2, bool arg3)
    //    {
    //        Client.Instance.Send(PacketType.FarmlandDesignate, PacketDesignate.Write(Player.Actor.InstanceID, this.Farmland.ID, arg1 + Vector3.UnitZ, arg2 + Vector3.UnitZ, arg3));
    //    }




    //    internal override void OnGameEvent(GameEvent e)
    //    {
    //        switch (e.Type)
    //        {
    //            case Message.Types.FarmSeedChanged:
    //            case Message.Types.FarmUpdated:
    //                //var farm = e.Parameters[0] as Farmland;
    //                var farm = e.Parameters[0] as GrowingZone;
    //                if (farm != this.Farmland)
    //                    return;
    //                this.Seed.Object = farm.SeedType;
    //                //this.ChkHarvesting.Value = farm.Harvesting;
    //                //this.ChkPlanting.Value = farm.Planting;
    //                //this.GetWindow().Title = farm.Name;
    //                break;

    //            case Message.Types.FarmRemoved:
    //                //farm = e.Parameters[0] as Farmland;
    //                farm = e.Parameters[0] as GrowingZone;
    //                if (farm == this.Farmland)
    //                    this.GetWindow().Hide();
    //                break;

    //            default:
    //                break;
    //        }
    //    }

    //    private new Window ToWindow(string name = "")
    //    {
    //        Window existing;
    //        if (OpenWindows.TryGetValue(this.Farmland, out existing))
    //            return existing;

    //        var win = base.ToWindow(name);
    //        win.AutoSize = true;
    //        win.Movable = true;
    //        win.Client.Controls.Add(this);


    //        var dialogRename = new DialogInput("Rename " + this.Farmland.Name, Rename, 16, this.Farmland.Name);
    //        win.Label_Title.LeftClickAction = () => dialogRename.ShowDialog();
    //        win.Label_Title.MouseThrough = false;
    //        win.Label_Title.Active = true;

    //        win.HideAction = () => OpenWindows.Remove(this.Farmland);
    //        win.ShowAction = () => OpenWindows[this.Farmland] = win;

    //        return win;
    //    }


    //    internal static Window GetWindow(GrowingZone farmland)
    //    {
    //        Window existing;
    //        if (OpenWindows.TryGetValue(farmland, out existing))
    //            return existing;
    //        return new GrowingZoneUI(farmland).ToWindow(farmland.Name);
    //    }
    //}
}
