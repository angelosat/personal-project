﻿using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class MessageBox : Window
    {
        Panel Panel_Text, Panel_Buttons;
       
        static public MessageBox Create(string title, string text, string b1text, Action b1action)
        {
            return new MessageBox(title, text, b1text, b1action);
        }
        static public MessageBox Create(string title, string text, Action yesAction = null, Action noAction = null)
        {
            return new MessageBox(title, text, yesAction, noAction);
        }
        static public MessageBox CreateDialogue(string title, string text, Action yesAction = null, Action noAction = null)
        {
            var box = Create(title, text, yesAction, noAction);
            box.ShowDialog();
            return box;
        }
        MessageBox(string title, string text, string b1text, Action b1action)
        {
            this.AutoSize = true;
            Title = title;
            AutoSize = true;

            int width = 600;
            
            Panel_Text = new Panel();
            Panel_Text.AutoSize = true;

            Label label = new Label(new Vector2(0), UIManager.WrapText(text, width));
            Panel_Text.Controls.Add(label);
            Panel_Text.ClientSize = new Rectangle(0, 0, width, Math.Max(label.Height, UIManager.SlotSprite.Height));

            Panel_Buttons = new Panel(new Vector2(0, Panel_Text.Bottom));
            Panel_Buttons.ClientSize = new Rectangle(0, 0, Panel_Text.ClientSize.Width, Button.DefaultHeight);

            Button btn = new Button(b1text, Panel_Buttons.ClientSize.Width) { LeftClickAction = b1action };

            Panel_Buttons.Controls.Add(btn);
            Client.Controls.Add(Panel_Text, Panel_Buttons);

            this.SnapToScreenCenter();
        }

        public MessageBox(string title, string text, Action yesAction = null, Action noAction = null)
        {
            Title = title;
            AutoSize = true;
            Closable = false;

            int width = 200;
            Panel_Text = new Panel();
            Panel_Text.AutoSize = true;

            Label label = new Label(new Vector2(0), UIManager.WrapText(text, width));
            Panel_Text.Controls.Add(label);
            Panel_Text.ClientSize = new Rectangle(0, 0, width, Math.Max(label.Height, UIManager.SlotSprite.Height));

            Panel_Buttons = new Panel(new Vector2(0, Panel_Text.Bottom)) { AutoSize = true };
            Panel_Buttons.ClientSize = new Rectangle(0, 0, Panel_Text.ClientSize.Width, Button.DefaultHeight);

            Button Yes = new Button(Vector2.Zero, width / 2, "Yes") { LeftClickAction = () => { this.Hide(); if (yesAction != null)yesAction(); } };
            Button No = new Button(new Vector2(Yes.Right, 0), width / 2, "No") { LeftClickAction = () => { this.Hide(); if (noAction != null) noAction(); } };
            Panel_Buttons.Controls.Add(Yes, No);

            Client.Controls.Add(Panel_Text, Panel_Buttons);

            this.SnapToScreenCenter();
        }
        public MessageBox(string title, string text, params ContextAction[] actions)
        {
            Title = title;
            AutoSize = true;
            Closable = false;

            int width = 200;
            Panel_Text = new Panel();
            Panel_Text.AutoSize = true;
            Label label = new Label(new Vector2(0), UIManager.WrapText(text, width)) { Width = width };
            Panel_Text.Controls.Add(label);
            Panel_Buttons = new Panel(new Vector2(0, Panel_Text.Bottom)) { AutoSize = true };

            CreateButtonsHor(actions, width);

            Client.Controls.Add(Panel_Text, Panel_Buttons);

            this.SnapToScreenCenter();
        }

        // TODO refactor to button helper class
        private void CreateButtonsHor(ContextAction[] actions, int width)
        {
            var w = width / actions.Length;
            foreach (var action in actions)
            {
                Button btn = new Button(action.Name(), w) { LeftClickAction = () => { action.Action(); this.Hide(); } };
                btn.Location = this.Panel_Buttons.Controls.TopRight;
                this.Panel_Buttons.AddControlsTopRight(btn);
            }
        }
    }
}