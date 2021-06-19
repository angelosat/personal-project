using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.Components;
using Start_a_Town_.UI.Editor;
using Start_a_Town_.Editor;

namespace Start_a_Town_.UI
{
    class EditorUI : Control
    {
        #region Singleton
        static EditorUI _Instance;
        static public EditorUI Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new EditorUI();
                return _Instance;
            }
            set { _Instance = value; }
        }
        #endregion
        Bldg OpenBldg = new Bldg() { Origin = Rooms.EditorRoom.Start };

       // UndoableCollection<CellOperation> Operations = new UndoableCollection<CellOperation>();
        UndoableCollection Operations = new UndoableCollection();
        GroupBox Box_Stages;
        FpsCounter Fps;
        Button Btn_Blocks, Btn_Back, Btn_Control;
        RadioButton Rd_Add, Rd_Paint;
        Window Window_Blocks, Window_Save, Window_Load, Window_Control;// Window_Objects;
        MenuStrip Box_Buttons;
      //  Vector3 Origin = Rooms.EditorRoom.Start;
        bool DrawControls = true;
        bool Saved = true;
        EditorUI()
        {
            Box_Buttons = new MenuStrip();// new GroupBox();
            Box_Buttons.AutoSize = true;
      //      GetControlBlocks();
            MenuStripItem btn_file = new MenuStripItem(Box_Buttons) { Text = "File", };
            MenuStripItem btn_edit = new MenuStripItem(Box_Buttons) { Text = "Edit", Location = btn_file.TopRight, };
            MenuStripItem btn_view = new MenuStripItem(Box_Buttons) { Text = "View", Location = btn_edit.TopRight, };
            MenuStripItem btn_tools = new MenuStripItem(Box_Buttons) { Text = "Tools", Location = btn_view.TopRight };

            Button btn_Objects = new Button()
            {
                Color = Color.White * 0.5f,
                Text = "Objects",
                IdleColor = Color.Transparent,
                LeftClickAction = () =>
                {
                    //Window_Objects.Toggle();
                    //Window_Objects.Location = btn_tools.Dropdown.ScreenLocation + btn_tools.Dropdown.BottomLeft;
                    ObjectsWindow.Instance.Toggle();
                    ObjectsWindow.Instance.Location = btn_tools.Dropdown.ScreenLocation + btn_tools.Dropdown.BottomLeft;
                }
            };
            Btn_Blocks = new Button()
            {
                Location = btn_Objects.BottomLeft,
                Color = Color.White * 0.5f,
                Text = "Blocks",
                IdleColor = Color.Transparent,
                LeftClickAction = () =>
                    {
                        Start_a_Town_.Editor.TerrainWindow.Instance.Toggle();
                        Start_a_Town_.Editor.TerrainWindow.Instance.Location = btn_tools.Dropdown.ScreenLocation + btn_tools.Dropdown.BottomLeft;
                        //Window_Blocks.Toggle();
                        //Window_Blocks.Location = btn_tools.Dropdown.ScreenLocation + btn_tools.Dropdown.BottomLeft;// Btn_Blocks.ScreenLocation + Btn_Blocks.BottomLeft;
                    }
            };

            ControlsWindow WindowControl = new ControlsWindow();
            Btn_Control = new Button()
            {
                Location = Btn_Blocks.BottomLeft, 
                Text = "Control",
                Color = Color.White * 0.5f,
                IdleColor = Color.Transparent,
                LeftClickAction = () =>
                {
                    //Window_Control.Toggle();
                    //Window_Control.Location = btn_tools.Dropdown.ScreenLocation + btn_tools.Dropdown.BottomLeft;//Btn_Control.ScreenLocation + Btn_Control.BottomLeft;
                    WindowControl.Bldg = this.OpenBldg;
                    WindowControl.Toggle();
                    //WindowControl.Location = WindowControl.CenterScreen * 0.5f;// btn_tools.Dropdown.ScreenLocation + btn_tools.Dropdown.BottomLeft;//Btn_Control.ScreenLocation + Btn_Control.BottomLeft;
                    WindowControl.SnapToScreenCenter();

                }
            };

            var cam = Net.Client.Instance.Map.Camera;
            MenuStripItem btn_camera = new MenuStripItem(Box_Buttons)
            {
                Location = btn_tools.TopRight,
                Text = "Reset Camera",
                LeftClickAction = () =>
                {
                    //ScreenManager.CurrentScreen.Camera.Zoom = 1;
                    //ScreenManager.CurrentScreen.Camera.CenterOn(OpenBldg.Origin);
                    cam.Zoom = 1;
                    cam.CenterOn(OpenBldg.Origin);
                }//Vector3.Zero)
            };
            Button btn_new = new Button()
            {
             //   Location = btn_camera.TopRight,
                IdleColor = Color.Transparent,
                Color = Color.White * 0.5f,
                Text = "New",
                LeftClickAction = () =>
                {
                    if (!Saved)
                    {
                        MessageBox.Create("Changes not saved!", "Are you sure you want to start a new design? All changes since the last save will be lost!", yesAction: () => New()).ShowDialog();
                    }
                    else
                    {
                        New();
                    }
                }
            };
            Button btn_save = new Button()
            {
                Location = btn_new.BottomLeft,//TopRight,
                IdleColor = Color.Transparent,
                Color = Color.White * 0.5f,
                Text = "Save",
                LeftClickAction = () =>
                {
                    Window_Save.ShowDialog();
                }
            };
            Button btn_load = new Button()
            {
                Location = btn_save.BottomLeft,//TopRight,
                IdleColor = Color.Transparent,
                Color = Color.White * 0.5f,
                Text = "Load",
                LeftClickAction = () =>
                {
                    //Window_Load.ShowDialog();
                    //BldgLoadWindow.Instance.ShowDialog();
                    new BldgLoadWindow((fileinfo) => Load(fileinfo)).ShowDialog();
                }
            };
            Btn_Back = new Button()
            {
                Location = btn_load.BottomLeft,
                Color = Color.White * 0.5f,
                IdleColor = Color.Transparent,
                Text = "Back",
                LeftClickAction = () =>
                {
                    Rooms.EditorRoom.Instance.CancelLoading();
                    ScreenManager.GameScreens.Pop(); 
                }
            };

            Button btn_undo = new Button()
            {
             //   Location = btn_camera.TopRight,
                IdleColor = Color.Transparent,
                Color = Color.White * 0.5f,
                Text = "Undo",
                LeftClickAction = () =>
                {
                    //Undo();
                    Operations.Undo();
                }
            };
            Button btn_redo = new Button()
            {
                Location = btn_undo.BottomLeft,//TopRight,
                IdleColor = Color.Transparent,
                Color = Color.White * 0.5f,
                Text = "Redo",
                LeftClickAction = () =>
                {
                    //Redo();
                    Operations.Redo();
                }
            };
            CheckBox chk_controlBlocks = new CheckBox("Show Control Blocks")//"Show Control Blocks", true);
            {
                //IdleColor = Color.Transparent,
                //Color = Color.White * 0.5f,
                Checked = true,
                //Text = "Show Control Blocks",
                LeftClickAction = () => { DrawControls = !DrawControls; }
            };


            btn_file.Dropdown.Controls.Add(btn_new, btn_save, btn_load, Btn_Back);
            btn_edit.Dropdown.Controls.Add(btn_undo, btn_redo);
            btn_view.Dropdown.Controls.Add(chk_controlBlocks);
            btn_tools.Dropdown.Controls.Add(btn_Objects, Btn_Blocks, Btn_Control);

            Box_Buttons.Items.Add(btn_file, btn_edit, btn_view, btn_tools, btn_camera);
            Box_Stages = new GroupBox();
            Box_Stages.AutoSize = true;

            Box_Stages.Location = Box_Stages.BottomRightScreen;

            Fps = new FpsCounter();
            Fps.Location = Fps.BottomCenterScreen;

            //var cam = Net.Client.Instance.Map.Camera;
            //Controls.Add(Box_Buttons, Box_Stages, Fps, LogWindow.Instance, new CameraWidget(Rooms.EditorRoom.Instance.Camera) { Location = new Vector2(UIManager.Width, 0), Anchor = Vector2.UnitX });
            Controls.Add(Box_Buttons, Box_Stages, Fps, LogWindow.Instance, new CameraWidget(cam) { Location = new Vector2(UIManager.Width, 0), Anchor = Vector2.UnitX });
            CreateSaveWindow();
            //CreateLoadWindow();
            //CreateBlocksWindow();
            CreateControlWindow();
            //CreateObjectsWindow();
        }

        //private void CreateBlocksWindow()
        //{
        //    Window_Blocks = new Window() { Title = "Blocks", AutoSize = true, Movable = true };
        //    int i = 0, j = 0, n = 0;
        //    Panel panel = new Panel() { AutoSize = true };
        //    foreach (var block in BlockComponent.Blocks.Values)
        //    {
        //        Slot slot = new Slot(new Vector2(i * Slot.DefaultHeight, j * Slot.DefaultHeight));
        //        slot.Tag = block.Entity.ToSlotLink();
        //        EmptyTool tool = new EmptyTool()
        //        {
        //            LeftClick = (t) =>
        //            {
        //                if (t == null)
        //                    return ControlTool.Messages.Default;

        //                Vector3 target;
        //                Block.Types brush;
        //                bool solid;
        //                //if (Controller.Input.GetKeyDown(System.Windows.Forms.Keys.ControlKey))
        //                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
        //                {
        //                    target = t.Global;// obj.Global;
        //                    brush = Block.Types.Air;
        //                    solid = false;
        //                    //target.TryRemoveBlock(Engine.Map);
        //                    target.TryRemoveBlock(Net.Client.Instance); // WARNING!
        //                    target.TryRemoveBlock(Net.Server.Instance); // WARNING!
        //                    return ControlTool.Messages.Default;
        //                }
        //                else
        //                {
        //                    //target = obj.Global + (Rd_Add.Checked ? face : Vector3.Zero);
        //                    target = t.Global + (Rd_Add.Checked ? t.Face : Vector3.Zero);
        //                    brush = (Block.Types)slot.Tag.Object["Physics"]["Type"];
        //                    solid = (bool)slot.Tag.Object["Physics"]["Solid"];
        //                }
        //                //if (target == OpenBldg.Origin)
        //                //    return ControlTool.Messages.Default;
        //                CellOperation op = new CellOperation(Net.Client.Instance, target, brush);

        //                Operations.Push(op);
        //                Saved = false;
        //                throw new NotImplementedException();
        //                //target.TryPlaceBlock(Engine.Map, brush);
        //                return ControlTool.Messages.Default;
        //            }
        //        };
        //        //tool.KeyUp = (e) =>
        //        //{
        //        //    if (!Controller.Input.GetKeyDown(System.Windows.Forms.Keys.ControlKey))
        //        //        return ControlTool.Messages.Default;
        //        //    CellOperation op;
        //        //    switch (e.KeyData)
        //        //    {
        //        //        case System.Windows.Forms.Keys.Z: //undo
        //        //            Operations.Undo();
        //        //            break;
        //        //        case System.Windows.Forms.Keys.Y: //redo
        //        //            Operations.Redo();
        //        //            break;
        //        //        default:
        //        //            break;
        //        //    }
        //        //    return ControlTool.Messages.Default;
        //        //};
        //        tool.DrawAction = (sb, cam) =>
        //        {
        //            if (slot.Tag.Object.IsNull())
        //                return;
        //            //if (tool.TargetOld.IsNull())
        //            //    return;
        //            //slot.Tag.Object.DrawPreview(sb, cam, tool.TargetOld.Global + tool.Face, (tool.TargetOld.Global + tool.Face).GetDrawDepth(Engine.Map, cam));//(cam));
        //            if (tool.Target == null)
        //                return;
        //            slot.Tag.Object.DrawPreview(sb, cam, tool.Target.FaceGlobal, (tool.Target.FaceGlobal).GetDrawDepth(Engine.Map, cam));//(cam));
        //        };
        //        slot.LeftClickAction = () =>
        //        {
        //            ScreenManager.CurrentScreen.ToolManager.ActiveTool = tool;
        //        };
        //        n++;
        //        i = n % 8;
        //        j = n / 8;
        //        panel.Controls.Add(slot);
        //    }
        //    Panel panel_buttons = new Panel() { AutoSize = true, Location = panel.BottomLeft, BackgroundStyle = BackgroundStyle.TickBox };
        //    Rd_Add = new RadioButton("Add", Vector2.Zero, true);
        //    Rd_Paint = new RadioButton("Paint", Rd_Add.TopRight, false);
        //    panel_buttons.Controls.Add(Rd_Add, Rd_Paint);
        //    Window_Blocks.Client.Controls.Add(panel, panel_buttons);
        //}
        private void CreateSaveWindow()
        {
            Window_Save = new Window() { Title = "Save Map", AutoSize = true };
            Panel panel_txt = new Panel() { AutoSize = true, BackgroundStyle = BackgroundStyle.TickBox };
            TextBox txtbox = new TextBox() { Width = 150, };
            txtbox.TextEnterFunc = (e) =>
            {
                if (char.IsControl(e.KeyChar))
                    return;
                txtbox.Text += e.KeyChar;
            };
            panel_txt.Controls.Add(txtbox);

            Panel panel_btns = new Panel() { Location = panel_txt.BottomLeft, AutoSize = true };

            //Button btn_ok = new Button() { Text = "Save", Width = (int)panel_txt.ClientDimensions.Y };
            Button btn_ok = new Button(Vector2.Zero, panel_txt.ClientSize.Width / 2, "Save");
            btn_ok.LeftClickAction = () =>
            {
                string name = txtbox.Text;
                if (name.Length == 0)
                    return;
                //UI.Editor.Bldg bldg = new Editor.Bldg() { Name = name, Operations = new Stack<CellOperation>(this.LoadedMap.Union(this.Operations)), Origin = this.Origin };
                Bldg bldg = new Bldg()
                {
                    Name = name,
                    Operations = new Stack<CellOperation>(
                        OpenBldg.Operations.Union(this.Operations)
                        .Where(foo => foo is CellOperation)
                        .Select(foo => foo as CellOperation)),                    //new Stack<CellOperation>(OpenBldg.Operations.Union(this.Operations)),
                    Origin = OpenBldg.Origin
                };
                string dir = GlobalVars.SaveDir + @"\Maps\";
                string filename = name + ".bldg.sat";
                if (File.Exists(dir + filename))
                {
                    MessageBox.Create(
                        "File already exists!",
                        "Do you want to overwrite " + filename + " ?",
                        yesAction: () => Save(dir, filename, bldg))
                        .ShowDialog();
                }
                else
                {
                    //Save(name);
                    Save(dir, filename, bldg);
                }

            };

            Button btn_cancel = new Button(btn_ok.TopRight, panel_txt.ClientSize.Width / 2, "Cancel");// { Text = "Cancel", Location = btn_ok.TopRight, Width = btn_ok.Width };
            btn_cancel.LeftClickAction = () => { Window_Save.Hide(); };

            panel_btns.Controls.Add(btn_ok, btn_cancel);

            Window_Save.ShowAction = () => txtbox.Text = OpenBldg.Name;// Engine.Map.Name;
            Window_Save.Client.Controls.Add(panel_txt, panel_btns);

            //Window_Save.Location = Window_Save.CenterScreen;
            Window_Save.SnapToScreenCenter();

        }
        private void CreateLoadWindow()
        {
            string dir = GlobalVars.SaveDir + @"\Maps\";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            FileInfo[] mapFiles = dirInfo.GetFiles();

            Window_Load = new Window() { Title = "Load", AutoSize = true, };

            Panel panel_list = new Panel() { AutoSize = true };
            ListBox<FileInfo, Button> listbox = new ListBox<FileInfo, Button>(new Rectangle(0, 0, 150, 300));// { ItemChangedFunc = () => { } };
            listbox.Build(mapFiles, foo => foo.Name);

            panel_list.Controls.Add(listbox);

            Panel panel_btns = new Panel() { Location = panel_list.BottomLeft, AutoSize = true };
            panel_btns.Controls.Add(new Button()
            {
                Text = "Load",
                Width = panel_list.Width,
                LeftClickAction = () =>
                {
                    Load(listbox.SelectedItem);
                    Window_Load.Hide();
                }
            });

            Window_Load.Client.Controls.Add(panel_list, panel_btns);
            //Window_Load.Location = Window_Load.CenterScreen;
            Window_Load.SnapToScreenCenter();

        }
        private void CreateControlWindow()
        {
            Window_Control = new Window() { Title = "Control Blocks", AutoSize = true, Movable = true };
            Panel panel = new Panel() { AutoSize = true };
            Slot slot = new Slot()
            {
                Tag = BlueprintOrigin.ToSlotLink(),
                LeftClickAction = () =>
                {
                    ScreenManager.CurrentScreen.ToolManager.ActiveTool = new EmptyTool()
                    {
                        LeftClick = (t) =>
                        {
                            if (t == null)
                                return ControlTool.Messages.Default;
                            Vector3 global = t.Global;// +face;
                            //new List<CellOperation>() { 
                            //    new CellOperation(Engine.Map, OpenBldg.Origin, Tile.Types.Air, false),  
                            //    new CellOperation(Engine.Map, global, Tile.Types.EditorOrigin, false) }
                            //    .ForEach(op => { Engine.Map.SetCell(op); });
                            //OpenBldg.Origin = global;
                            //Saved = false;
                            //return ControlTool.Messages.Default;
                            EditorOperation op = new EditorOperation(EditorOperation.Types.SetOrigin, OpenBldg, global);
                            if (op.Perform())
                            {
                                Operations.Push(op);
                                Saved = false;
                            }
                            return ControlTool.Messages.Default;
                        }
                    };
                }
            };

            panel.Controls.Add(slot);
            Window_Control.AddControls(panel);
        }
        
        private void New()
        {
            World world = World.Create(new WorldArgs(
                name: "New Design",
                trees: false,
                seed: 0,
                terraformers: new Terraformer[] { Terraformer.Empty },
                lighting: false,
                defaultTile: Block.Types.Air));
            //Map map = Map.Create(world, Vector2.Zero);
            EditorMap map = EditorMap.Create(world, Vector2.Zero);

            Engine.Map = map;
            OpenBldg = new Bldg() { Origin = Rooms.EditorRoom.Start };
            //new Vector3(0,0,Map.MaxHeight / 2).TrySetCell

            Chunk chunk = new Chunk(map, Vector2.Zero);
            map.AddChunk(chunk);
            //map.Generate();



            if (!map.SetCell(0, 0, Map.MaxHeight / 2, Block.Types.Grass, 0))
                throw new Exception();
            for (int i = 0; i < Chunk.Size; i++)
            {
                for (int j = 0; j < Chunk.Size; j++)
                {
                    map.SetBlock(i, j, Map.MaxHeight / 2, Block.Types.Grass, 0);
                }
            }
            chunk.ResetVisibleCells();

            //Rooms.EditorRoom.Instance.Camera.CenterOn(OpenBldg.Origin);
            Net.Client.Instance.Map.Camera.CenterOn(OpenBldg.Origin);

        }
        private void Save(string dir, string filename, Start_a_Town_.Editor.Bldg bldg)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);
                List<SaveTag> operations = new List<Start_a_Town_.SaveTag>();
                SaveTag tag = new SaveTag(Start_a_Town_.SaveTag.Types.Compound, "Map");

                tag.Add(new SaveTag(Start_a_Town_.SaveTag.Types.String, "Name", bldg.Name));
                tag.Add(new SaveTag(Start_a_Town_.SaveTag.Types.Compound, "Origin", bldg.Origin.SaveAsList()));

                Dictionary<Vector3, CellOperation> dic = new Dictionary<Vector3, CellOperation>();
                foreach (var op in bldg.Operations)
                {
                    if (!op.Performed)
                        continue;
                    if (op.Type == Block.Types.Air)
                        dic.Remove(op.Global);
                    else
                        dic[op.Global] = op;

                    //// TEMPORARY WORKAROUND UNTIL I DIFFERENTIATE AIR FROM EMPTY TILES
                    //switch (op.Type)
                    //{
                    //    case Tile.Types.Air:
                    //        dic.Remove(op.Global);
                    //        continue;
                    //    case Tile.Types.EditorAir:
                    //        op.Type = Tile.Types.Air;
                    //        break;
                    //    default:
                    //        break;
                    //}
                    //dic[op.Global] = op;
                }

                //foreach (var op in bldg.Operations)
                //    if(op.Performed)
                foreach (var op in dic.Values)
                    operations.Add(op.Save());
                tag.Add(new SaveTag(Start_a_Town_.SaveTag.Types.Compound, "Operations", operations));
                tag.WriteTo(writer);
                stream.Position = 0;
                
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                using (FileStream outFile = File.Create(dir + filename))
                    using (GZipStream zip = new GZipStream(outFile, CompressionMode.Compress))
                        stream.CopyTo(zip);
                stream.Close();
            }
            Saved = true;
            Log.Enqueue(Log.EntryTypes.System, bldg.Name + " saved successfully");
            Window_Save.Hide();
        }
        private void Load(FileInfo fileInfo)
        {
            World world = World.Create(
                new WorldArgs(
                    name: "New Design", trees: false, seed: 0,
                    terraformers: new Terraformer[] { Terraformer.Empty },
                    lighting: false, defaultTile: Block.Types.Blueprint));
            Map map = Map.Create(world, Vector2.Zero);
            Operations = new UndoableCollection();//w UndoableCollection<CellOperation>();
            OpenBldg.Load(fileInfo);
            OpenBldg.Apply(map, OpenBldg.Origin);
            map.SetCell(new CellOperation(Net.Client.Instance, OpenBldg.Origin, Block.Types.EditorOrigin));
            //Rooms.EditorRoom.Instance.Camera.CenterOn(OpenBldg.Origin);//LoadedMap.Peek().Global);
            Net.Client.Instance.Map.Camera.CenterOn(OpenBldg.Origin);//LoadedMap.Peek().Global);

            Engine.Map = map;
            Rooms.EditorRoom.Instance.CancelLoading();
            ChunkLoader.ForceLoad(Rooms.EditorRoom.Instance, map, Rooms.EditorRoom.Instance.Cancel.Token);
        }

        static public List<GameObject> GetControlBlocks()
        {
            return ControlBlocks;
        }
        static public List<GameObject> ControlBlocks
        {
            get { return new List<GameObject>() { BlueprintOrigin, BlueprintAir }; }
        }
        static public GameObject BlueprintOrigin
        {
            get
            {
                GameObject obj = GameObjectDb.BlockDefault;
                obj["Info"] = new DefComponent(GameObject.Types.BlueprintBlock, ObjectType.Block, "Blueprint Origin", "The starting block of the design. Corresponds to the block that the design is placed upon in the world.");
                //obj.AddComponent<BlockComponent>().Initialize(Block.Types.EditorOrigin, hasData: false, transparency: 0, density: 1);
                return obj;
            }
        }
        static public GameObject BlueprintAir
        {
            get
            {
                GameObject obj = GameObjectDb.BlockDefault;
                obj["Info"] = new DefComponent(GameObject.Types.EditorAir, ObjectType.Block, "Air", "A control block signifying that this position will be cleared during construction.");
                //obj.AddComponent<BlockComponent>().Initialize(Block.Types.EditorAir, hasData: false, transparency: 0, density: 1);
                return obj;
            }
        }

        //public override void DrawOnCamera(SpriteBatch sb, Camera camera)
        //{
        //    if (DrawControls)
        //        GameObject.Objects[GameObject.Types.BlueprintBlock].DrawPreview(sb, camera, OpenBldg.Origin, OpenBldg.Origin.GetDrawDepth(Engine.Map, camera));//(camera));
        //}

        public override void DrawWorld(MySpriteBatch sb, Camera cam)
        {
            //base.Draw(sb, viewport);
            if (Engine.Map.IsNull())
                return;
            if (!DrawControls)
                return;

            //var cam = ScreenManager.CurrentScreen.Camera;
            var pos = cam.GetScreenPosition(OpenBldg.Origin);//, Block.Bounds);

            var gd = Game1.Instance.GraphicsDevice;
            //MySpriteBatch sb = new MySpriteBatch(gd);

            //Sprite.Blockighlight.Draw(mysb, )
            var depth = OpenBldg.Origin.GetDrawDepth(Engine.Map, cam);
            sb.Draw(Sprite.Atlas.Texture, pos, Sprite.BlockHighlight.AtlasToken.Rectangle, 0, Block.OriginCenter, cam.Zoom, Color.Red, SpriteEffects.None, depth);
            gd.SamplerStates[0] = cam.Zoom >= 1 ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;
            gd.SamplerStates[1] = cam.Zoom >= 1 ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;
            gd.Textures[0] = Sprite.Atlas.Texture;
            gd.Textures[1] = Sprite.Atlas.DepthTexture;
            var fx = Game1.Instance.Content.Load<Effect>("blur");
            fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
            fx.CurrentTechnique.Passes.First().Apply();
            sb.Flush();

        }

        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (!Controller.Input.GetKeyDown(System.Windows.Forms.Keys.ControlKey))
                return;
            CellOperation op;
            switch (e.KeyData)
            {
                case System.Windows.Forms.Keys.Z: //undo
                    Operations.Undo();
                    break;
                case System.Windows.Forms.Keys.Y: //redo
                    Operations.Redo();
                    break;
                default:
                    break;
            }
        }
    }
}
