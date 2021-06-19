using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;

namespace Start_a_Town_.UI
{
    class DesignUI : Control
    {
        #region Singleton
        static DesignUI _Instance;
        static public DesignUI Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new DesignUI();
                return _Instance;
            }
            set { _Instance = value; }
        }
        #endregion

        GroupBox Box_Buttons, Box_Stages;
        FpsCounter Fps;
        Button Btn_Blocks, Btn_StageNext, Btn_StagePrev, Btn_Back;

        DesignUI()
        {
            Btn_Blocks = new Button(Vector2.Zero, 70, "Blocks");
            Btn_Blocks.LeftClick += new UIEvent(Btn_Blocks_Click);

            Box_Buttons = new GroupBox();
            Box_Buttons.AutoSize = true;

            Btn_Back = new Button(Btn_Blocks.BottomLeft, 70, "Back");
            Btn_Back.LeftClick += new UIEvent(Btn_Back_Click);
            Box_Buttons.Controls.Add(Btn_Blocks, Btn_Back);

            Btn_StagePrev = new Button(Vector2.Zero, 100, "Previous Stage");
            Btn_StageNext = new Button(Btn_StagePrev.TopRight, 100, "Next Stage");

            Btn_StagePrev.LeftClick += new UIEvent(Btn_StagePrev_Click);
            Btn_StageNext.LeftClick += new UIEvent(Btn_StageNext_Click);

            Box_Stages = new GroupBox();
            Box_Stages.AutoSize = true;
            Box_Stages.Controls.Add(Btn_StageNext, Btn_StagePrev);

            Box_Buttons.Location = Box_Buttons.BottomLeftScreen;
          //  Box_Stages.Location.Y = Box_Buttons.Location.Y - Box_Stages.Height;
            Box_Stages.Location = Box_Stages.BottomRightScreen;

            Fps = new FpsCounter();
            Fps.Location = Fps.BottomCenterScreen;

            Controls.Add(Box_Buttons, Box_Stages, Fps);
        }

        void Btn_Back_Click(object sender, EventArgs e)
        {
            ScreenManager.GameScreens.Pop();
        }

        void Btn_StageNext_Click(object sender, EventArgs e)
        {
            DesignTool.NextStage();
            //DesignTool.Stage += 1;
        }

        void Btn_StagePrev_Click(object sender, EventArgs e)
        {
            DesignTool.LastStage();
            //DesignTool.Stage -= 1;
        }

        void Btn_Blocks_Click(object sender, EventArgs e)
        {
            //LandscapingWindow.Instance.Toggle();
            //LandscapingWindow.Instance.Location = Box_Buttons.Location + new Vector2(Btn_Blocks.Right, -LandscapingWindow.Instance.Height);
        }
    }
}
