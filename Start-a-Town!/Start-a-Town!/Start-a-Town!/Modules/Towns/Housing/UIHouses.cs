using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Modules.Towns.Housing;
using Start_a_Town_.Modules.Towns;
using Start_a_Town_.Towns;
using Start_a_Town_.Net;

namespace Start_a_Town_.Modules.Towns.Housing
{
    class UIHouses : GroupBox 
    {
        Button BtnCreate, BtnRemove, BtnClear, BtnRename;
        ListBox<House, Button> ListHouses;
        Panel PanelList;
        static DialogInput DialogHouseName;
        CheckBox Chk_HideWalls, Chk_HideRoof, Chk_HideCeiling;

        public UIHouses()
        {
            this.ListHouses = new ListBox<House, Button>(220, 400);
            this.PanelList = new Panel() { AutoSize = true };
            this.PanelList.Controls.Add(this.ListHouses);
            this.BtnCreate = new Button("Add") { Location = this.PanelList.BottomLeft, LeftClickAction = Create, HoverText = "Add new house" };
            this.BtnRemove = new Button("Delete") { Location = this.BtnCreate.TopRight, LeftClickAction = RemoveHouse, HoverText = "Remove existing house" };
            this.BtnRename = new Button("Rename") { Location = this.BtnRemove.TopRight, LeftClickAction = RenameHouse };
            this.BtnClear = new Button("Clear") { Location = this.BtnRename.TopRight, LeftClickAction = Clear };
            BuildList();

            this.Chk_HideRoof = new CheckBox("Hide Roof", House.HideRoof) { Location = this.BtnCreate.BottomLeft, ValueChangedFunction = val => House.HideRoof = val };
            this.Chk_HideWalls = new CheckBox("Hide Walls", House.HideWalls) { Location = this.Chk_HideRoof.BottomLeft, ValueChangedFunction = val => House.HideWalls = val };
            this.Chk_HideCeiling = new CheckBox("Hide Ceiling", House.HideCeiling) { Location = this.Chk_HideWalls.BottomLeft, ValueChangedFunction = val => House.HideCeiling = val };

            this.Controls.Add(this.PanelList, this.BtnCreate, this.BtnRemove, this.BtnClear, this.BtnRename,
                Chk_HideRoof, Chk_HideWalls, Chk_HideCeiling);
        }

       
        

        private void Clear()
        {
            var dialog = new MessageBox("Warning", "All houses will be deleted.", new ContextAction(() => "Accept", () =>
            {
                Server.Instance.Map.GetTown().ClearHouses();
                Client.Instance.Map.GetTown().ClearHouses();
            }), new ContextAction(()=>"Cancel", ()=>{}));
            dialog.ShowDialog();
        }

        static void Create()
        {
            ScreenManager.CurrentScreen.ToolManager.ActiveTool = new HouseTool(CreateHouse, RemoveHouse);
        }

        private void RemoveHouse()
        {
            RemoveHouse(this.ListHouses.SelectedItem);
        }
        private static void RemoveHouse(Vector3 global)
        {
            var house = Client.Instance.Map.GetTown().GetHouseAt(global);
            RemoveHouse(house);
        }
        private static void RemoveHouse(House house)
        {
            if (house == null)
                return;
            var dialog = new MessageBox("Warning", "Really delete " + house.Name + " ?", new ContextAction(() => "Accept", () =>
            {
                TownsPacketHandler.PlayerRemoveHouse(house);
            }), new ContextAction(() => "Cancel", () => { }));
            dialog.ShowDialog();
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.HousesUpdated:
                    BuildList();
                    break;

                default:
                    break;
            }
        }

        private void BuildList()
        {
            this.ListHouses.Build(Client.Instance.Map.GetTown().GetHouses(), h => h.Name);
        }

        private void RenameHouse()
        {
            var h = this.ListHouses.SelectedItem;
            if (h == null)
                return;
            DialogHouseName = new DialogInput("Enter house name", txt => RenameHouse(h, txt), 16, h.Name);
            DialogHouseName.ShowDialog();
        }
        static void RenameHouse(House house, string name)
        {
            name.Trim();
            if (!ValidateHouseName(name))
                return;
            //house.Name = name;
            if (DialogHouseName != null)
                DialogHouseName.Hide();
            TownsPacketHandler.PlayerRenameHouse(house, name);
        }
        static void CreateHouse(Vector3 global)
        {
            if (!ValidatePosition(global))
                return;
            if(InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey))
            {
                CreateHouse("", global);
                return;
            }
            DialogHouseName = new DialogInput("Enter house name", txt => CreateHouse(txt, global), 16, Client.Instance.Map.GetTown().GetDefaultHouseName());
            DialogHouseName.ShowDialog();
            ScreenManager.CurrentScreen.ToolManager.ClearTool();
        }

        private static bool ValidatePosition(Vector3 global)
        {
            foreach (var house in Client.Instance.Map.GetTown().GetHouses())
                if (house.Enterior.Contains(global))
                {
                    Client.Console.Write("House already exists at " + global.ToString());
                    return false;
                }
            return true;
        }
        static void CreateHouse(string name, Vector3 global)
        {
            name = name.Trim();
            if (!ValidateHouseName(name))
                return;
            if (DialogHouseName != null)
                DialogHouseName.Hide();
            TownsPacketHandler.PlayerCreateHouse(name, global);
        }
        static bool ValidateHouseName(string name)
        {
            //name.All(char.IsLetterOrDigit);
            // TODO: use regex to check name validity
            return true;
        }
        //public override bool Show(params object[] p)
        //{
        //    BuildList();
        //    return base.Show(p);
        //}
    }
}
