using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Start_a_Town_.UI
{
    class SkillsWindow : Window
    {
        static SkillsWindow _Instance;
        public static SkillsWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new SkillsWindow();
                return _Instance;
            }
        }

        Rectangle DragBoxFromMouseDown = Rectangle.Empty;
        //GameObject Actor;
        public SkillsWindow()
        {
            Title = "Skills";
            //ClientSize = new Rectangle(0, 0, 100, 100);
            AutoSize = true;
            Location = Vector2.Zero;
            Panel Panel = new Panel();
            Panel.AutoSize = true;
            Panel.ClientSize = new Rectangle(0, 0, 200, 8 * UIManager.SlotSprite.Height);
            //foreach (Skill skill in Player.Instance.SkillSet)
            //for(int i=0; i<Player.Instance.Skills.Count; i++)
            //{
            //    Slot slot = new Slot(panel, new Vector2(0, i * UIManager.containerSprite.Height));
            //    //slot.ItemSlot = Player.Instance.Skills[i];
            //    slot.Item = Player.Instance.Skills[(uint)i];
            //    slot.MouseDown += new EventHandler<EventArgs>(slot_MouseDown);
            //    slot.MouseMove += new EventHandler<EventArgs>(slot_MouseMove);
            //    slot.Click += new UIEvent(slot_Click);
            //    panel.Controls.Add(slot);
            //    //Label slotLabel = new Label(panel, new Vector2(slot.Right, slot.Center.Y), slot.ItemSlot.Item.Name + "\nRank: " + (slot.ItemSlot.Item as Skill).Level.ToString(), TextAlignment.Left, TextAlignment.Center);
            //    Label slotLabel = new Label(panel, new Vector2(slot.Right, slot.Center.Y), slot.Item.Name + "\nRank: " + (slot.Item as Skill).Level.ToString(), TextAlignment.Left, TextAlignment.Center);
            //    panel.Controls.Add(slotLabel);
            //}

            //Panel p_skill;
            //int y = 0;
            //int i = 0;
            //foreach (uint id in Actor.Skills.Keys)
            //{



            //    //Skill skill = Player.Actor.Skills[id];
            //    //Slot slot = new Slot(new Vector2(0, i++ * UIManager.containerSprite.Height));
            //    //slot.Item = skill;
            //    //slot.MouseDown += new EventHandler<InputState>(slot_MouseDown);
            //    //slot.MouseMove += new EventHandler<InputState>(slot_MouseMove);
            //    //slot.Click += new UIEvent(slot_Click);
            //    //Panel.Controls.Add(slot);
            //    //Label slotLabel = new Label(new Vector2(slot.Right, slot.Top), skill.Name + "\nRank: " + (slot.Item as Skill).Level.ToString());
            //    //Panel.Controls.Add(slotLabel);
            //}

            Controls.Add(Panel);
            SizeToControl(Panel);
            Movable = true;
            Location = new Vector2(0, Hud.DefaultHeight);
        }

        void slot_Click(object sender, EventArgs e)
        {

            //Slot slot = sender as Slot;
            //if (slot.Item is IActivatable)
            //    (slot.Item as IActivatable).Activate();
        }

        void slot_MouseDown(object sender, EventArgs e)
        {
            //Console.WriteLine("PW RE BOUSTH");
            DragBoxFromMouseDown = new Rectangle(Controller.Instance.msCurrent.X - 4, Controller.Instance.msCurrent.Y - 4, 8, 8);
        }

        void slot_MouseMove(object sender, EventArgs e)
        {
            if(Controller.Instance.msCurrent.LeftButton == ButtonState.Pressed)
                if (DragBoxFromMouseDown != Rectangle.Empty && !DragBoxFromMouseDown.Contains(new Rectangle(Controller.Instance.msCurrent.X, Controller.Instance.msCurrent.Y, 1, 1)))
                {
                    Slot slot = sender as Slot;
                    //slot.DoDragDrop(slot.Tag, DragDropEffects.Link);
                    DragDropManager.Create(slot.Tag, slot, DragDropEffects.Link);
                    DragBoxFromMouseDown = Rectangle.Empty;
                }
        }
    }
}
