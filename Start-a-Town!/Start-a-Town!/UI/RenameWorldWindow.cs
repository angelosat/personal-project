using System;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.UI
{
    class RenameWorldWindow : Window
    {
        static RenameWorldWindow _Instance;
        public static RenameWorldWindow Instance => _Instance ??= new RenameWorldWindow();

        Panel Panel_Name, Panel_Buttons;
        TextBox Txt_Name;
        Button Btn_Accept, Btn_Cancel;

        RenameWorldWindow()
        {
            this.Title = "Rename World";
            this.AutoSize = true;

            Panel_Name = new Panel();
            Panel_Name.AutoSize = true;
            Panel_Name.BackgroundStyle = BackgroundStyle.TickBox;

            Panel_Buttons = new Panel(Panel_Name.BottomLeft);
            Panel_Buttons.AutoSize = true;

            Txt_Name = new TextBox(Vector2.Zero, new Vector2(200, Label.DefaultHeight));
            Txt_Name.TextEntered += new EventHandler<TextEventArgs>(Txt_Name_TextEntered);
            Txt_Name.BackgroundStyle = BackgroundStyle.TickBox;

            Panel_Name.Controls.Add(Txt_Name);

            Panel_Buttons = new Panel(Panel_Name.BottomLeft);
            Panel_Buttons.AutoSize = true;
            Btn_Accept = new Button(Vector2.Zero, 50, "Accept");
            Btn_Cancel = new Button(Btn_Accept.TopRight, 50, "Cancel");
            Btn_Accept.LeftClick += new UIEvent(Btn_Accept_Click);
            Btn_Cancel.LeftClick += new UIEvent(Btn_Cancel_Click);
            Panel_Buttons.Controls.Add(Btn_Accept, Btn_Cancel);

            this.Controls.Add(Panel_Name, Panel_Buttons);
            this.SnapToScreenCenter();
        }

        void Txt_Name_TextEntered(object sender, TextEventArgs e)
        {
            TextBox.DefaultTextHandling(Txt_Name, e);
        }

        static public RenameWorldWindow Initialize(IWorld world)
        {
            Instance.Tag = world;
            Instance.Txt_Name.Text = world.GetName();// world.Name;
            return Instance;
        }

        void Btn_Cancel_Click(object sender, EventArgs e)
        {
            base.Hide();
        }

        void Btn_Accept_Click(object sender, EventArgs e)
        {
            World world = this.Tag as World;
            //   if (world == null)
            //       return;
            //   string saveDir, oldworldDir, oldworldFile, worldDir, worldFile; ;
            //   world.GetFileInfo(out saveDir, out oldworldDir, out oldworldFile);
            //   world.Name = Txt_Name.Text;
            //   world.GetFileInfo(out saveDir, out worldDir, out worldFile);
            ////   world.Save();
            //   File.Move(saveDir + oldworldDir + oldworldFile, saveDir + oldworldDir + worldFile);
            //   Directory.Move(saveDir + oldworldDir, saveDir + worldDir);
            //   world.Save();
            if (RenameWorld(world, Txt_Name.Text))
                OnWorldRenamed();
            base.Hide();
        }

        static public bool RenameWorld(World world, string newName)
        {
            if (world == null)
                return false;
            string saveDir, oldworldDir, oldworldFile, worldDir, worldFile; ;
            world.GetFileInfo(out saveDir, out oldworldDir, out oldworldFile);
            world.Name = newName;
            world.GetFileInfo(out saveDir, out worldDir, out worldFile);
            //   world.Save();
            File.Move(saveDir + oldworldDir + oldworldFile, saveDir + oldworldDir + worldFile);
            Directory.Move(saveDir + oldworldDir, saveDir + worldDir);
            world.Save();
            return true;
        }
    }
}
