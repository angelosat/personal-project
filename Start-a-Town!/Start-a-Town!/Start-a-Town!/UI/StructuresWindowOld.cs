using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class StructuresWindowOld : Window
    {
        static StructuresWindowOld _Instance;
        public static StructuresWindowOld Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new StructuresWindowOld();
                return _Instance;
            }
        }

        //PanelList<GameObject> List_Objects;
        ListBox<GameObject, Button> List_Objects;

        RadioButton Rd_All;
        GroupBox Box_Radios;
        TextBox Txt_Search;
        Button Btn_Build;
        Panel Panel_Search, Panel_Info;

        StructureTool Tool;

        public GameObject SelectedItem { get { return List_Objects.SelectedItem; } }
        static public event EventHandler BuildClick;
        void OnBuildClick()
        {
            if (BuildClick != null)
                BuildClick(this, EventArgs.Empty);
        }

        StructuresWindowOld()
        {
            Title = "Constructions";
            this.Movable = true;
            this.AutoSize = true;

            //List_Objects = new PanelList<GameObject>(Vector2.Zero, new Vector2(200, 300), foo => foo.Name);
            List_Objects = new ListBox<GameObject, Button>(new Rectangle(0, 0, 200, 300));
            List_Objects.SelectedItemChanged += new EventHandler<EventArgs>(List_Objects_SelectedItemChanged);
            //Txt_Search = new TextBox(Vector2.Zero, new Vector2(List_Objects.ClientSize.Width, Label.DefaultHeight));

            Panel_Search = new Panel(List_Objects.BottomLeft) { Dimensions = new Vector2(List_Objects.Width, 1), BackgroundStyle = BackgroundStyle.TickBox };
            Panel_Search.AutoSize = true;

            Txt_Search = new TextBox(Vector2.Zero, new Vector2(Panel_Search.ClientDimensions.X, Label.DefaultHeight));
            Txt_Search.TextEntered += new EventHandler<TextEventArgs>(Txt_Search_TextEntered);

            IconButton btn_clear = new IconButton(Txt_Search.TopRight)
            {
              //  Location = Txt_Search.TopRight,
                BackgroundTexture = UIManager.Icon16Background,
                Icon = new Icon(UIManager.Icons16x16, 0, 16),
                HoverFunc = () => "Clear",
                LeftClickAction = () =>
                {
                    Txt_Search.Text = "";
                    List_Objects.Build(WorkbenchComponent.Blueprints, foo => foo.Name, ListControlInit());
                },
                Anchor = Vector2.UnitX
            };
           // btn_clear.Anchor = Vector2.UnitX;
            Panel_Search.Controls.Add(Txt_Search, btn_clear);

            Box_Radios = new GroupBox(Panel_Search.BottomLeft);
            Box_Radios.AutoSize = true;

            Rd_All = new RadioButton("All", Vector2.Zero);
            Rd_All.Checked = true;
            Rd_All.Tag = "";
            Rd_All.LeftClick += new UIEvent(Rd_All_Click);
            Box_Radios.Controls.Add(Rd_All);
            List<string> types = GetPlanTypes();
            foreach (string type in types)
            {
                RadioButton rd = new RadioButton(type, Box_Radios.Controls.Last().BottomLeft) { Tag = type };//.SetTag(type);
                rd.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(rd_Click);
                Box_Radios.Controls.Add(rd);
                //Box_Radios.controls
            }


            Panel_Info = new Panel(List_Objects.TopRight, new Vector2(List_Objects.Width * 1.5f, List_Objects.Height + Panel_Search.Height + Box_Radios.Height));
            Btn_Build = new Button(new Vector2(0, Panel_Info.ClientSize.Bottom - Button.DefaultHeight), Panel_Info.ClientSize.Width, "Build");
            Btn_Build.LeftClick += new UIEvent(Btn_Build_Click);
            Client.Controls.Add(List_Objects, Box_Radios, Panel_Search, Panel_Info);

            this.Location = Vector2.Zero;// Center;
            //Tool = StructureTool.Instance;// new StructureTool();
            Initialize();
        }

        void Btn_Build_Click(object sender, EventArgs e)
        {
            OnBuildClick();

            Blueprint bp = this.SelectedItem["Blueprint"]["Blueprint"] as Blueprint;

            GameObject obj = GameObject.Create(GameObject.Types.Construction);// GameObjectDb.Construction;
            obj.GetComponent<ConstructionOldComponent>().SetBlueprint(bp, SelectedItem.GetComponent<SpriteComponent>().Variation, SelectedItem.GetComponent<SpriteComponent>().Orientation);
          //  throw new NotImplementedException();
            //obj.PostMessage(Message.Types.SetBlueprint, null, bp, (int)SelectedItem["Sprite"]["Variation"], (int)SelectedItem["Sprite"]["Orientation"]);
            if (!Tag.IsNull())
            {
                //Net.Client.AddObject(obj);
                //throw new Exception("Obsolete position handling");
                //throw new NotImplementedException();
                //obj.Spawn(Player.Actor.Map, obj.Global);
                return;
            }

        }

        void List_Objects_SelectedItemChanged(object sender, EventArgs e)
        {
            Panel_Info.Controls.Clear();
            GameObject obj = List_Objects.SelectedItem as GameObject;
            if (obj == null)
                return;
            GroupBox info = new GroupBox();
            obj.GetTooltip(info);

            Panel_Info.Controls.Add(info);//, Btn_Build); //new CraftingTooltip(obj.ToSlot()));// 
        }

        void Txt_Search_TextEntered(object sender, TextEventArgs e)
        {
            switch (e.Char)
            {
                case '\b':
                    break;

                default:
                    Txt_Search.Text += e.Char;
                    break;
            }
            string type = Box_Radios.Controls.Find(rd => (rd as RadioButton).Checked).Tag as string;
            //List_Objects.Build(WorkbenchComponent.Blueprints.ToList().FindAll(foo=>foo.Type.ToLower().Contains(type.ToLower())).FindAll(foo => foo.Name.ToLower().Contains(Txt_Search.Text.ToLower())));
            this.List_Objects.Build(
                WorkbenchComponent.Blueprints.ToList()
                    .FindAll(foo => foo.Type.ToLower().Contains(type.ToLower()))
                    .FindAll(foo => foo.Name.ToLower().Contains(Txt_Search.Text.ToLower())),
                foo => foo.Name,
                ListControlInit()
                );
        }

        private static Action<GameObject, Button> ListControlInit()
        {
            return (GameObject obj, Button btn) =>
            {
                GameObject bpObj = btn.Tag as GameObject;
                Blueprint bp = bpObj["Blueprint"]["Blueprint"] as Blueprint;

                btn.TooltipFunc = (tooltip) => obj.GetTooltipBasic(tooltip); btn.Color = Color.Black;
                EmptyTool tool = new EmptyTool();
                tool.LeftClick = (target) =>
                {
                    if (target == null)
                        return ControlTool.Messages.Default;
                    if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                    {
                        var entity = target.Object;
                        if (entity == null)
                            return ControlTool.Messages.Default;
                        if (entity.HasComponent<Components.ConstructionFootprint>())
                        {
                            Net.Client.RemoveObject(entity);
                            return ControlTool.Messages.Default;
                        }
                        //target.TryGetComponent<Components.ConstructionComponent>(c =>
                        //{
                        //    foreach (var req in c.Materials)
                        //    {
                        //        if (req.Amount > 0)
                        //            return;
                        //    }
                        //    Net.Client.RemoveObject(target);
                        //}); 
                        return ControlTool.Messages.Default;
                    }
                    GameObject constr = GameObject.Create(GameObject.Types.Construction);// GameObjectDb.Construction;
                    //constr.GetComponent<ConstructionComponent>().SetBlueprint(bp, obj.GetComponent<ActorSpriteComponent>().Variation, obj.GetComponent<ActorSpriteComponent>().Orientation);
                    constr.GetComponent<ConstructionFootprint>().SetBlueprint(bp, obj.GetComponent<SpriteComponent>().Variation, obj.GetComponent<SpriteComponent>().Orientation);
                    Net.Client.AddObject(constr, target.FinalGlobal);

                    return ControlTool.Messages.Default;
                };
                tool.DrawAction = (sb, cam) =>
                {
                    //if (tool.TargetOld.IsNull())
                    //    return;
                    if (tool.Target == null)
                        return;
                    if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                    {
                        Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
                        sb.Draw(UIManager.Icons16x16, loc + Vector2.UnitX * 8, new Rectangle(0, 0, 16, 16), Color.White);
                        return;
                    }
                    //GameObject.Objects[bp.ProductID].DrawPreview(sb, cam, tool.TargetOld.Global + tool.Face, (tool.TargetOld.Global + tool.Face).GetDrawDepth(Engine.Map, cam));//GetDepth(cam));
                    GameObject.Objects[bp.ProductID].DrawPreview(sb, cam, tool.Target.FaceGlobal, (tool.Target.FaceGlobal).GetDrawDepth(Engine.Map, cam));//GetDepth(cam));
                };
                btn.LeftClickAction = () =>
                {
                    ToolManager.Instance.ActiveTool = tool;
                };
            };
        }

        void Rd_All_Click(object sender, EventArgs e)
        {
            //List_Objects.Build(WorkbenchComponent.Blueprints);
            this.List_Objects.Build(
                WorkbenchComponent.Blueprints,
                foo => foo.Name,
                ListControlInit()
                );
        }

        void rd_Click(object sender, EventArgs e)
        {
            //List_Objects.Build();
            //List_Objects.Build(WorkbenchComponent.Blueprints.FindAll(foo=>foo.Type == (sender as Control).Tag as string));
            this.List_Objects.Build(
                WorkbenchComponent.Blueprints.FindAll(foo=>foo.Type == (sender as Control).Tag as string),
                foo => foo.Name,
                ListControlInit()
                );
        }

        List<string> GetPlanTypes()
        {
            List<string> planTypes = new List<string>();
            foreach (GameObject obj in WorkbenchComponent.Blueprints)
            {
                if (!planTypes.Contains(obj.Type))
                    planTypes.Add(obj.Type);
            }
            return planTypes;
        }

        public override void Initialize()
        {
            //List_Objects.Build(WorkbenchComponent.Blueprints);
            this.List_Objects.Build(
                WorkbenchComponent.Blueprints,
                foo => foo.Name,
                ListControlInit()
                );
            base.Initialize();
        }

        public override void DrawOnCamera(SpriteBatch sb, Camera camera)
        {
            if (SelectedItem.IsNull())
                return;
            if (Tag.IsNull())
                return;
            GameObject.Objects[(SelectedItem["Blueprint"]["Blueprint"] as Blueprint).ProductID].DrawPreview(sb, camera, (Vector3)Tag);
        }
    }
}
