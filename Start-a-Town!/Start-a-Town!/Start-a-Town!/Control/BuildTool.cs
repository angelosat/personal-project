using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.ConstructionSystem;
using Start_a_Town_.Tasks;
using Start_a_Town_.UI;
using Start_a_Town_.WorldEntities;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Control
{
    class BuildTool : ControlTool
    {
        //store a reference to the construction to be placed, check mouseover tile against allowed tiles for the construction to be placed on
        Cell LastCell;
        ConstructionPreview Preview;
        Cell PreviewCell;

        protected static BuildTool _Instance;
        public static BuildTool Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new BuildTool();
                return _Instance;
            }
        }

        //public ConstructionPreview Preview;

        public BuildTool()
        {
            Preview = new ConstructionPreview(ItemBrowser.SelectedItem);
            //Preview.Blend = Color.Lerp(Color.White, Color.Transparent, 0.5f);

        }

        void Instance_TopMostEntityChanged(object sender, EventArgs e)
        {
            //Tile tile = Map.Instance.TopMostEntity as Tile;
            //if (PreviewCell != null)
            //    PreviewCell.Remove(Preview);
            //PreviewCell = null;
            //if (tile != null)
            //{
            //    Cell cell = ItemBrowser.SelectedItem.RequiresBase ? tile.CellAbove : tile.TargetCell;
            //    if (cell != null)
            //    {
            //        if (cell.Entities.Count == 0)
            //        {
            //            PreviewCell = cell;
            //            PreviewCell.Add(Preview);
            //            Preview.Location = PreviewCell.IsoCoords;
            //        }
            //    }
            //}
        }

        void Instance_TargetCellChanged(object sender, EventArgs e)
        {
            ////PreviewCell = Map.Instance.TargetCell;
            //if (Map.Instance.TargetCell != null)
            //    Preview.Location = Map.Instance.TargetCell.IsoCoords;

            //if (PreviewCell != null)
            //    PreviewCell.Remove(Preview);

            PreviewCell = null;
            if (Map.Instance.ActiveCell != null)
                if (Map.Instance.ActiveCell.Tile != null)
                {
                    //Tile tile = Map.Instance.ActiveCell.Tile;

                    ////PreviewCell = Map.Instance.TargetCell;
                    //Cell cell = ItemBrowser.SelectedItem.RequiresBase ? tile.Cell.Above : tile.TargetCell;
                    //if (cell != null)
                    //{
                    //    if (cell.Entities.Count == 0)
                    //    {
                    //        PreviewCell = cell;
                    //        PreviewCell.Add(Preview);
                    //        Preview.Location = PreviewCell.Location;
                    //    }
                    //}
                }
            LastCell = PreviewCell;
        }

        void Map_ActiveCellChanged(object sender, EventArgs e)
        {
            //if (PreviewCell != null)
            //    PreviewCell.Remove(Preview);


            if (Map.Instance.ActiveCell != null)
            {

            }
        }

        void Controller_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.KeysNew.Contains(Microsoft.Xna.Framework.Input.Keys.OemComma) && !e.KeysOld.Contains(Microsoft.Xna.Framework.Input.Keys.OemComma))
                Preview.Rotation--;
            else if (e.KeysNew.Contains(Microsoft.Xna.Framework.Input.Keys.OemPeriod) && !e.KeysOld.Contains(Microsoft.Xna.Framework.Input.Keys.OemPeriod))
                Preview.Rotation++;

            if (e.KeysNew.Contains(Microsoft.Xna.Framework.Input.Keys.K) && !e.KeysOld.Contains(Microsoft.Xna.Framework.Input.Keys.K))
                Preview.Style--;
            else if (e.KeysNew.Contains(Microsoft.Xna.Framework.Input.Keys.L) && !e.KeysOld.Contains(Microsoft.Xna.Framework.Input.Keys.L))
                Preview.Style++;
        }

        //Tile CurrentTile, LastTile;
        //Cell LastCell;
        public override void Update()
        {
            //Tile tile = Map.Instance.TopMostEntity as Tile;
            //if (PreviewCell != null)
            //    PreviewCell.Remove(Preview);
            //PreviewCell = null;
            //if (tile != null)
            //{
            //    Cell cell = ItemBrowser.SelectedItem.RequiresBase ? tile.Cell.Above : tile.TargetCell;
            //    //if (cell != LastCell)
            //    //{
            //    //    if (PreviewCell != null)
            //    //        PreviewCell.Remove(Preview);
            //    //}
            //    if (cell != null)
            //    {
            //        if (cell.Entities.Count == 0)
            //        {
            //            PreviewCell = cell;
            //            PreviewCell.Add(Preview);
            //            Preview.Location = PreviewCell.Location;
            //        }
            //    }
            //}
            //LastCell = PreviewCell;
        }

        public override bool MouseLeft()
        {
            Cell cell = PreviewCell;
            if (cell == null)
                return false;

            //DrawableWorldEntity existing = cell.FindEntityType<ConstructionSite>();

            //if (existing == null)
            //{
            //    ConstructionSite site = new ConstructionSite();

            //    //site.Plan = ItemBrowser.SelectedItem;
            //    Plan plan = (Plan)ItemBrowser.SelectedItem.Clone();
            //    site.Plan = plan;
            //    plan.Style = Preview.Style;
            //    plan.Rotation = Preview.Rotation;


            //    //site.SetCell(Map.Instance.GetCellAt(cell.GlobalCoords));//tile);
            //    return false;
            //}
            //else
            //{
            //    if (Controller.Instance.GetKeys().Contains(Microsoft.Xna.Framework.Input.Keys.LeftControl))
            //    {
            //        //existing.Destroy();
            //        return false;
            //    }
            //}

            return false;
        }

        public override bool MouseRight()
        {

            //if (PreviewCell != null)
            //    PreviewCell.Remove(Preview);

            Preview = null;

            return true;
        }
    }
}
