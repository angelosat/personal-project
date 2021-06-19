using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Farming
{
    class FarmlandUI : GroupBox
    {
        static Dictionary<Farmland, Window> OpenWindows = new Dictionary<Farmland, Window>();

        Farmland Farmland;
        Panel PanelSeeds, PanelButtons;
        //Slot SlotSeed;
        GameObjectSlot Seed;
        CheckBoxNew ChkHarvesting, ChkPlanting;

        public FarmlandUI(Farmland farmland)
        {
            this.Farmland = farmland;
            this.PanelSeeds = new Panel() { AutoSize = true };//new Rectangle(0, 0, 200, 200));

            var seedObjects = GameObject.Objects.Values.Where(foo => foo.GetComponent<SeedComponent>() != null).Select(f => f.ToSlot());
            //from obj in GameObject.Objects.Values
            //where obj.GetComponent<SeedComponent>() != null
            //select new GameObjectSlot(obj);
            var slotGrid = new SlotGrid(seedObjects.ToList(), 4, s =>
            {
                s.LeftClickAction = () => PacketFarmSetSeed.Send(Player.Actor.InstanceID, this.Farmland.ID, (int)s.Tag.Object.ID);
            });
            //this.PanelSeeds.Controls.Add(slotGrid);

            this.Seed = new GameObjectSlot(this.Farmland.SeedType);
            var slotseed = new Slot() { Tag = this.Seed };
            slotseed.RightClickAction = () =>
            {
                //slotseed.Tag.Clear();
                SetSeed(null);
            };
            slotseed.LeftClickAction = SelectSeed;
            this.PanelSeeds.AddControls(slotseed);

            var paneloptions = new Panel() { Location = this.PanelSeeds.BottomLeft, AutoSize = true };
            this.ChkHarvesting = new CheckBoxNew("Harvesting");
            this.ChkHarvesting.Value = this.Farmland.Harvesting;
            this.ChkHarvesting.LeftClickAction = () => SetHarvesting(this.Farmland, !this.ChkHarvesting.Value);

            this.ChkPlanting = new CheckBoxNew("Planting") { Location = this.ChkHarvesting.BottomLeft };
            this.ChkPlanting.Value = this.Farmland.Planting;
            this.ChkPlanting.LeftClickAction = () => SetPlanting(this.Farmland, !this.ChkPlanting.Value);

            paneloptions.AddControls(this.ChkHarvesting, this.ChkPlanting);

            this.PanelButtons = new Panel(paneloptions.BottomLeft) { AutoSize = true };
            var btnEdit = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Edit farm",// "Add/Remove stockpiles",
                LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ToolDesignatePositions(this.Edit, () => this.Farmland.Positions.Select(g => g - Vector3.UnitZ).ToList()) { RestrictZ = true, ValidityCheck = IsPositionValid }// this.Create)
            };
            this.PanelButtons.Controls.Add(btnEdit);

            this.Controls.Add(this.PanelButtons, this.PanelSeeds, paneloptions);
        }

        private void SetPlanting(Farmland farm, bool p)
        {
            Client.Instance.Send(PacketType.FarmSync, PacketFarmSync.Write(farm.ID, farm.Name, farm.GetSeedID(), farm.Harvesting, p));
        }

        private void SetHarvesting(Farmland farm, bool p)
        {
            Client.Instance.Send(PacketType.FarmSync, PacketFarmSync.Write(farm.ID, farm.Name, farm.GetSeedID(), p, farm.Planting));
        }

        private void SelectSeed()
        {
            var itempicker = ItemPicker.Instance;
            itempicker.Show(UIManager.Mouse, obj => obj.HasComponent<SeedComponent>(), (obj) => SetSeed(obj));
        }

        private void SetSeed(GameObject obj)
        {
            PacketFarmSetSeed.Send(Player.Actor.InstanceID, this.Farmland.ID, obj == null ? -1 : (int)obj.ID);
        }

        private bool IsPositionValid(Vector3 arg)
        {
            return Farmland.IsPositionValid(this.Farmland, arg);
            //return
            //    Block.GetBlockMaterial(this.Farmland.Town.Map, arg) == Components.Materials.Material.Soil
            //    && this.Farmland.Town.Map.GetBlock(arg + Vector3.UnitZ) == Block.Air;
        }

        private void Edit(Vector3 arg1, Vector3 arg2, bool arg3)
        {
            Net.Client.Instance.Send(PacketType.FarmlandDesignate, PacketDesignate.Write(Player.Actor.InstanceID, this.Farmland.ID, arg1 + Vector3.UnitZ, arg2 + Vector3.UnitZ, arg3));
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.FarmSeedChanged:
                case Message.Types.FarmUpdated:
                    var farm = e.Parameters[0] as Farmland;
                    //var seed = e.Parameters[1] as GameObject;
                    if (farm != this.Farmland)
                        return;
                    this.Seed.Object = farm.SeedType;
                    this.ChkHarvesting.Value = farm.Harvesting;
                    this.GetWindow().Title = farm.Name;
                    break;

                default:
                    break;
            }
        }

        private new Window ToWindow(string name = "")
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

        private void Rename(DialogInput dialog)
        {
            var input = dialog.Input;
            dialog.Hide();
            Client.Instance.Send(PacketType.FarmSync, PacketFarmSync.Write(this.Farmland.ID, input, this.Farmland.GetSeedID(), this.Farmland.Harvesting, this.Farmland.Planting));
        }


        internal static Window GetWindow(Farmland farmland)
        {
            Window existing;
            if (OpenWindows.TryGetValue(farmland, out existing))
                return existing;
            return new FarmlandUI(farmland).ToWindow(farmland.Name);
        }
    }
}
