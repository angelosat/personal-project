using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class AboutWindow : Window
    {
        public AboutWindow()
        {
            Title = "About";
            AutoSize = true;

            Panel panel_text = new Panel();
            panel_text.AutoSize = true;
            Label label_text = new Label("Start-a-Town!\nVersion " + GlobalVars.Version + " Alpha\n© 2009-2012 Angelo Tsimidakis\nAll Rights Reserved");
            panel_text.Controls.Add(label_text);
            string browserWindowText = "This will minimize the game and open a browser window!";
            Panel panel_btns = new Panel(new Vector2(0, panel_text.Bottom));
            panel_btns.AutoSize = true;
            Button btn_blog = new Button("Website", panel_text.Width / 2);
            btn_blog.HoverText = browserWindowText;
            Button btn_forum = new Button(new Vector2(0, btn_blog.Bottom), panel_text.Width / 2, "Forum");
            btn_forum.HoverText = browserWindowText;
            Button btn_facebook = new Button(new Vector2(btn_forum.Right, 0), panel_text.Width / 2, "Facebook");
            btn_facebook.HoverText = browserWindowText;
            Button btn_twitter = new Button(new Vector2(btn_forum.Right, btn_facebook.Bottom), panel_text.Width / 2, "Twitter");
            btn_twitter.HoverText = browserWindowText;
            panel_text.ClientSize = new Rectangle(0, 0, panel_text.Width, panel_text.ClientSize.Height);

            btn_blog.LeftClick += new UIEvent(btn_link_Click);
            btn_forum.LeftClick += new UIEvent(btn_forum_Click);
            btn_facebook.LeftClick += new UIEvent(btn_facebook_Click);
            btn_twitter.LeftClick += new UIEvent(btn_twitter_Click);
            panel_btns.Controls.Add(btn_forum, btn_facebook, btn_twitter, btn_blog);

            Client.Controls.Add(panel_btns, panel_text);
            Location = CenterScreen;
        }

        void btn_twitter_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.twitter.com/startatown");
        }

        void btn_facebook_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.facebook.com/startatown");
        }

        void btn_forum_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://startatown.smfforfree.com");
        }

        void btn_link_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("http://startatown.blogspot.com");
            System.Diagnostics.Process.Start("http://www.startatown.com");
        }
    }
}
