using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components
{
    class PackageComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "Package"; }
        }
        public GameObjectSlot Content { get { return (GameObjectSlot)this["Content"]; } set { this["Content"] = value; } }
        public PackageComponent()
        {
            this.Content = GameObjectSlot.Empty;// null;
        }

        public void Unpack(IObjectProvider net, GameObject parent)
        {
            if (!MultiTile2Component.IsPositionValid(this.Content.Object, parent.Map, parent.Global))
                return;

            //parent.Remove(e.Network);
            net.Despawn(parent);

            //e.Network.Spawn(this.Content.Object.SetGlobal(parent.Global));
            net.Spawn(this.Content.Object.SetGlobal(parent.Global));
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Activate: //Build://
                    this.Unpack(e.Network, parent);
                    return true;

                case Message.Types.SetContent:
                    this.Content.Object = e.Parameters[0] as GameObject;
                    return true;
                //case Message.Types.Query:
                //    Query(parent, e);
                //    return true;
                case Message.Types.ManageEquipment:
                   // e.Sender.HandleMessage(Message.Types.ChangeOrientation, parent);
                    SpriteComponent.ChangeOrientation(this.Content.Object);
                    return true;
                default:
                    return false;
            }
        }

        //public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        //{
        //    if (!Content.HasValue)
        //        return;
        //  //  List<Interaction> list = e.Parameters[0] as List<Interaction>;
        //    if (MultiTile2Component.IsPositionValid(this.Content.Object, parent.Map, parent.Global))
        //        //list.Add(new Interaction(new TimeSpan(0, 0, 1), Message.Types.Build, parent, "Unpack", "Unpacking"));
        //        list.Add(new InteractionOld(new TimeSpan(0, 0, 1), Message.Types.Activate, parent, "Unpack", "Unpacking"));
        //    if (Content != null)
        //        if (SpriteComponent.HasOrientation(this.Content.Object))
        //            list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.ManageEquipment, parent, "Change Orientation", "Changing Orientation"));
        //}

        public override object Clone()
        {
            return new PackageComponent();// Content = this.Content };
        }

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            if (!Content.HasValue)
                return;

            //Panel content = new Panel(tooltip.Controls.BottomLeft) { AutoSize = true };
            //Label label = new Label("Contents") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            //Slot icon = new Slot() { Location = label.BottomLeft, Tag = this.Content };
            //GroupBox box = new GroupBox() { Location = icon.TopRight };
            //this.Content.Object.GetInfo().GetTooltip(this.Content.Object, box);
            //content.Controls.Add(label, icon, box);
            //tooltip.Controls.Add(content);

            PanelLabeled panel = new PanelLabeled("Contents") { Location = tooltip.Controls.BottomLeft, AutoSize = true };
            Slot icon = new Slot() { Location = panel.Controls.BottomLeft, Tag = this.Content };
            GroupBox box = new GroupBox() { Location = icon.TopRight };
            this.Content.Object.GetInfo().OnTooltipCreated(this.Content.Object, box);
            panel.Controls.Add(icon, box);
            tooltip.Controls.Add(panel);
        }


        public override void DrawMouseover(SpriteBatch sb, Camera camera, GameObject parent)// DrawObjectArgs e)
        {
            //if (e.Controller.Mouseover.Object != e.Object)
            //    return;
            if (!Content.HasValue)
                return;
            MultiTile2Component multi;
            if (Content.Object.TryGetComponent<MultiTile2Component>("Multi", out multi))
                multi.DrawPreview(sb, camera, parent.Global, (int)Content.Object["Sprite"]["Orientation"]);
            else
            {
                SpriteComponent sprite;
                if (Content.Object.TryGetComponent<SpriteComponent>("Sprite", out sprite))
                    sprite.DrawPreview(sb, camera, parent.Global.RoundXY(), (int)Content.Object["Sprite"]["Orientation"]);
            }
        }
        internal override List<SaveTag> Save()
        {
            List<SaveTag> save = new List<SaveTag>();
            if (this.Content.HasValue)
                save.Add(new SaveTag(SaveTag.Types.Compound, "Content", this.Content.Save()));
            return save;
        }
        internal override void Load(SaveTag save)
        {
            save.TryGetTag("Content", tag => this.Content.Load(tag));
        }
        public override void Write(System.IO.BinaryWriter writer)
        {
            this.Content.Write(writer);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            this.Content.Read(reader);
        }
    }
}
