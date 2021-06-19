using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class NotificationArea : DrawableInterfaceElement
    {
        static List<Notification> Notifications;

        public NotificationArea()
        {
            Notifications = new List<Notification>();
            Game1.Instance.graphics.DeviceReset += new EventHandler<EventArgs>(graphics_DeviceReset);
        }

        void graphics_DeviceReset(object sender, EventArgs e)
        {
            foreach (Notification not in Notifications)
                not.Validate();
        }

        public override void Update()
        {
            List<Notification> temp = Notifications.ToList();
            foreach (Notification not in temp)
                not.Update();
        }

        public static void Write(string text)
        {
            Notification not = new Notification(text);
            //Console.WriteLine(not.Height.ToString());
            if (Notifications.Count > 0)
            {
                foreach (Notification notification in Notifications)
                    //notification.Location.Y -= not.Height;
                    notification.Offset.Y -= not.Height;
            }
                //not.Location.Y = Notifications[Notifications.Count - 1].Location.Y - not.Height;
            not.DurationFinished += new EventHandler<EventArgs>(Notification_DurationFinished);
            Notifications.Add(not);

            //Chat.Write(text);
        }

        static void Notification_DurationFinished(object sender, EventArgs e)
        {
            Notifications.Remove(sender as Notification);
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Begin();
            int n = 0;
            foreach (Notification not in Notifications)
            {
                not.Draw(sb);
              //  sb.Draw(not.TextSprite, new Vector2(UIManager.Width, UIManager.Height) / 2 + new Vector2(0,n), Color.White);
              //  n += not.TextSprite.Height;
            }
            sb.End();
        }
    }
}
