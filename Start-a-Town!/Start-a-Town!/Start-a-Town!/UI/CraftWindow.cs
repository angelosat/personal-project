using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class CraftWindow : Window
    {
        static CraftWindow _Instance;
        static public CraftWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new CraftWindow();
                return _Instance;
            }
        }


        List<GameObjectSlot> CraftList { get; set; }
        
        Button Btn_Craft;
        Panel //Panel_Filters, 
            Panel_List, Panel_Buttons, Panel_Selected;
        ListBox<GameObject, Button> Box_List;
        List<GameObject> List;
        Action<GameObject> CraftAction;

        CraftWindow()
        {
            this.Title = "Crafting";
            this.AutoSize = true;
            this.Movable = true;

            //RadioButton
            //    rd_All = new RadioButton("All", Vector2.Zero, true)
            //    {
            //        LeftClickAction = () =>
            //        {
            //            Panel_Selected.Controls.Clear();
            //            Box_List.Build(List, foo => foo.Name, (foo, btn) => { btn.LeftClickAction = () => { RefreshSelected(foo); }; }
            //                //,
            //                //(foo, btn) =>
            //                //{
            //                //    btn.LeftClickAction = () =>
            //                //    {
            //                //        Panel_Selected.Controls.Clear();
            //                //        ScrollableBox box = new ScrollableBox(new Rectangle(0, 0, 300, 200));
            //                //        GroupBox gbox = new GroupBox();
            //                //        foo.GetTooltip(gbox);
            //                //        box.Add(gbox);
            //                //        Panel_Selected.Controls.Add(box);
            //                //    };
            //                //}
            //                );
            //        }
            //    },
            //    rd_MatsReady = new RadioButton("Have Materials", rd_All.TopRight)
            //    {
            //        LeftClickAction = () =>
            //        {
            //            Panel_Selected.Controls.Clear();
            //            Box_List.Build(List.FindAll(foo => BlueprintComponent.MaterialsAvailable(foo, InventoryComponent.GetSlots(Player.Actor))), foo => foo.Name, (foo, btn) => { btn.LeftClickAction = () => { RefreshSelected(foo); }; });
            //        }
            //    };
            //Panel_Filters = new Panel() { ClientDimensions = new Vector2(200),  AutoSize = true };
            //Panel_Filters.Controls.Add(rd_All, rd_MatsReady);

            Panel_List = new Panel() { AutoSize = true }; //Location = Panel_Filters.BottomLeft, 
            Box_List = new ListBox<GameObject, Button>(new Rectangle(0, 0, 200, 200));
            Panel_List.Controls.Add(Box_List);

            Panel_Selected = new Panel() { Location = Panel_List.BottomLeft, Size = Panel_List.Size};//, AutoSize = true };

            Panel_Buttons = new Panel() { Location = Panel_Selected.BottomLeft, AutoSize = true };
            Btn_Craft = new Button(Vector2.Zero, Box_List.Width, "Craft")
            {
                LeftClickAction = () =>
                {
                    GameObject bpObj = Box_List.SelectedItem;
                    if (bpObj.IsNull())
                        return;
                    //Net.Client.PostPlayerInput(Message.Types.ExecuteScript, w =>
                    Net.Client.PostPlayerInput(Message.Types.StartScript, w =>
                    {
                        Ability.Write(w, Script.Types.Crafting, new TargetArgs(Player.Actor),BitConverter.GetBytes((int)bpObj.ID));
                    });
                }
            };
            Panel_Buttons.Controls.Add(Btn_Craft);





            Client.Controls.Add(Panel_List, Panel_Selected, Panel_Buttons); //Panel_Filters,

            GameObject.MessageHandled += new EventHandler<ObjectEventArgs>(GameObject_MessageHandled);




            Location = CenterScreen;
        }

        void GameObject_MessageHandled(object sender, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Receive:
                case Message.Types.ArrangeInventory:
                case Message.Types.Refresh:
                    RefreshSelected(Box_List.SelectedItem);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// <para>arg0 gameobject list</para>
        /// <para>arg1 craft action</para>
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public override bool Show(params object[] p)
        {
            var list = p[0] as List<GameObject>;
            var action = p[1] as Action<GameObject>;
            //Btn_Craft.LeftClickAction = action;
            CraftAction = action;
            Box_List.Build(
                list,
                foo => foo.Name,
                onControlInit: (obj, ctrl) =>
                {
                    ctrl.LeftClickAction = () =>
                    {
                        RefreshSelected(obj);
                    };
                });
            this.List = list;
            return base.Show(p);
        }

        private void RefreshSelected(GameObject obj)
        {
            Panel_Selected.Controls.Clear();
            if (obj.IsNull())
                return;
            Blueprint bp = obj["Blueprint"]["Blueprint"] as Blueprint;
            GameObject product = GameObject.Objects[bp.ProductID];
            
            //ScrollableBox box = new ScrollableBox(Panel_List.ClientSize);
            //box.Add(product.GetTooltip());
            //Panel_Selected.Controls.Add(box);
            Panel_Selected.Controls.Add("Product:".ToLabel(Panel_Selected.Controls.BottomLeft));
            Panel_Selected.Controls.Add(new SlotWithText(Panel_Selected.Controls.BottomLeft) { Tag = product.ToSlot() });
            Panel_Selected.Controls.Add("Materials:".ToLabel(Panel_Selected.Controls.BottomLeft));
            foreach (var mat in bp.Stages[0])
            {
                SlotWithText matSlot = new SlotWithText(Panel_Selected.Controls.BottomLeft) { Tag = GameObject.Objects[mat.Key].ToSlot() };
                int amount = 0;
                InventoryComponent.GetSlots(Player.Actor)
                    .FindAll(s => s.HasValue)
                    .FindAll(s => s.Object.ID == mat.Key)
                    .ForEach(s => amount += s.StackSize);
                matSlot.Slot.CornerTextFunc = s => (amount.ToString() + "/" + mat.Value.ToString());
                Panel_Selected.Controls.Add(matSlot);
            }
        }
        public override bool Toggle()
        {
            return base.Toggle();
        }
    }
}
