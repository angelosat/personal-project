using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class ActionBar : Control
    {
        static ActionBar _Instance;
        static public ActionBar Instance
        {
            get
            {
                if(_Instance==null)
                    _Instance = new ActionBar();
                return _Instance;
            }
        }
        List<Slot> Slots = new List<Slot>();
        public ActionBar()
        {
            AutoSize = true;
            for (int i = 0; i < 5; i++)
            {
                Slot slot = new Slot(new Vector2(Controls.Count > 0 ? Controls.Last().Right : 0, 0));
                slot.Tag = new GameObjectSlot();
                //string txt = Ability.GetSlotText((AbilitySlot)i);
                Label lbltxt = new Label("ok") { MouseThrough = true, ClipToBounds = false };
                slot.Controls.Add(lbltxt);

                //slot.DrawMode = DrawMode.OwnerDrawFixed;
                //slot.DrawItem += new EventHandler<DrawItemEventArgs>(slot_DrawItem);
                Controls.Add(slot);
                Slots.Add(slot);
            }
            Location = BottomCenterScreen;

            //Slots[0].SetBottomRightText("LM");
            //Slots[1].SetBottomRightText("RM");
            //Slots[2].SetBottomRightText("E");
            //Slots[3].SetBottomRightText("F");
        }

        public void Initialize()
        {
            BodyPart mainhand = PlayerOld.Actor["Equipment"].GetProperty<BodyPart>(Stat.Mainhand.Name);
            List<GameObjectSlot> mainhandAbilities = mainhand.Object["Abilities"].GetProperty<List<GameObjectSlot>>("Abilities");

            int n = 0;
            //foreach (GameObjectSlot ability in ControlComponent.GetAbility(Player.Actor).Values)// mainhandAbilities)
            //{
            //    Slot slot = Controls[n++] as Slot;
            //    slot.Tag = ability;
            //}
        }

        public void Refresh()
        {
            //Controls.Clear();
            //Slots.Clear();
            //foreach (KeyValuePair<System.Windows.Forms.Keys, GameObjectSlot> ability in Player.GetKeysAbilities())// mainhandAbilities)
            //{
            //    Slot slot = new Slot(new Vector2(Controls.Count > 0 ? Controls.Last().Right : 0, 0));
            //    slot.Tag = ability.Value;
            //    //slot.SetBottomRightText(ability.Key.ToString());//Ability.GetSlotText(ability.Key));
            //    string txt = ability.Key.ToString();
            //    txt = txt.Substring(0, Math.Min(2, txt.Length));
            //    Label lbltxt = new Label(txt) { MouseThrough = true, ClipToBounds = false };
            //    slot.Controls.Add(lbltxt);
            //    Controls.Add(slot);
            //}
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }

        //static void slot_DrawItem(object sender, DrawItemEventArgs e)
        //{
        //    if (Player.Actor == null)
        //        return;
        //    Slot slot = sender as Slot;
        //    int index = Instance.Controls.FindIndex(foo=>foo == slot);
        //    Slot.Draw(e.SpriteBatch, Start_a_Town_.Control.DefaultTool.GetAction(index), slot.ScreenLocation, slot.SprFx, slot.Opacity, ((ActionButton)index).ToString());// Enum.TryParse<Start_a_Town_.Control.ActionButton>(
        //}
    }
}
