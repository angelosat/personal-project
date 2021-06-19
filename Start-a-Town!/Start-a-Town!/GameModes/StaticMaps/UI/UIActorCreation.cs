using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class UIActorCreation : GroupBox
    {
        public UIActorCreation(List<Actor> actors)//Action callback)
        {
            
            //var actors = new List<Actor> { Actor.Create(ActorDefOf.Npc), Actor.Create(ActorDefOf.Npc), Actor.Create(ActorDefOf.Npc) };
            actors.AddRange(new[] { Actor.Create(ActorDefOf.Npc), Actor.Create(ActorDefOf.Npc), Actor.Create(ActorDefOf.Npc) });

            var actorsUI = new ListBoxNew<Actor, ButtonNew>(200, UIManager.LargeButton.Height * 7);
            var addactorbutton = new Button("Create", actorsUI.Width);

            var actorslistbox = new PanelLabeledNew("Coloninsts");
            actorslistbox.Client.AddControlsVertically(actorsUI, addactorbutton);//, btnStart);
            var editbox = new Panel(0, 0, 500, actorslistbox.Height) { AutoSize = false };
            addactorbutton.LeftClickAction = addActor;

            //foreach (var actor in actors)
            //    actorsUI.AddItem(actor, btnInit);
            actorsUI.AddItems(actors, btnInit);

            //var btnStart = new Button("Start", actorsUI.Width) { LeftClickAction = callback };

            this.AddControlsVertically(actorslistbox);// actorsUI, new Button("Create", actorsUI.Width) { LeftClickAction = AddActor });//, btnStart);

            //var editbox = new GroupBox(300, 300) { Location = actorsUI.TopRight, BackgroundColor = Color.Red };
            editbox.Location = actorslistbox.TopRight;/// actorsUI.TopRight;
            this.AddControls(editbox);

            void addActor()
            {
                var a = ActorDefOf.Npc.Create() as Actor;
                actors.Add(a);
                actorsUI.AddItem(a, btnInit);
            }

            ButtonNew btnInit(Actor a)
            {
                var btn = ButtonNew.CreateBig(() => Expand(a, editbox), actorsUI.Client.Width, a.RenderIcon(), () => a.Npc.FullName);// actor.Name);
                //btn.AddControls(new UICloseButton() { Location = btn.TopRight, Anchor = Vector2.UnitX });//, LeftClickAction = ()=>
                var deleteBtn = IconButton.CreateCloseButton();
                deleteBtn.Location = btn.TopRight;
                deleteBtn.Anchor = Vector2.UnitX;
                deleteBtn.LeftClickAction = () => new MessageBox("", "Delete " + a.Name + " ?", () =>
                {
                    actors.Remove(a);
                    actorsUI.RemoveItem(a);
                }, () => { }).ShowDialog();
                btn.AddControls(deleteBtn);//, LeftClickAction = ()=>
                return btn;
            }
        }

        private void Expand(Actor a, Control editbox)
        {
            editbox.ClearControls();

            var boxName = new GroupBox();
            //boxName.AddControlsLeftToRight(
            //    new TextBox(100)
            //    {
            //        //Text = a.Firstname,
            //        TextFunc = () => a.Firstname,
            //        BackgroundColor = Color.White * .1f
            //    }
            //            .ToPanelLabeled("First name")
            //            .AddControlsTopRight(IconButton.CreateRandomizeButton(() => a.Firstname = NpcComponent.GetRandomName())
            //                                       .SetAnchor(Vector2.UnitX)),
            //    new TextBox(100)
            //    {
            //        //Text = a.LastName, 
            //        TextFunc = () => a.LastName,
            //        BackgroundColor = Color.White * .1f
            //    }
            //        .ToPanelLabeled("Last name")
            //        .AddControlsTopRight(IconButton.CreateRandomizeButton(() => a.Lastname = NpcComponent.GetRandomName())
            //                                       .SetAnchor(Vector2.UnitX))
            //    );

            var firstName = new TextBox(100)
            {
                TextFunc = () => a.Npc.FirstName,
                TextChangedFunc = t => a.Npc.FirstName = t,
                InputFilter = c => char.IsLetter(c),
                BackgroundColor = Color.White * .1f
            };
            var firstnamepanel = firstName.ToPanelLabeled("First name")
                     .AddControlsTopRight(IconButton.CreateRandomizeButton(() =>
                                                     {
                                                         a.Npc.FirstName = NpcComponent.GetRandomName();
                                                         firstName.Text = a.Npc.FirstName;
                                                     })
                                                    .SetAnchor(Vector2.UnitX));

            var lastName = new TextBox(100)
            {
                TextFunc = () => a.Npc.LastName,
                TextChangedFunc = t => a.Npc.LastName = t,
                InputFilter = c => char.IsLetter(c),
                BackgroundColor = Color.White * .1f
            };
            var lastnamepanel = lastName.ToPanelLabeled("Last name")
                    .AddControlsTopRight(IconButton.CreateRandomizeButton(() =>
                                                    {
                                                        a.Npc.LastName = NpcComponent.GetRandomName();
                                                        lastName.Text = a.Npc.LastName;
                                                    })
                                                   .SetAnchor(Vector2.UnitX));
            boxName.AddControlsHorizontally(firstnamepanel, lastnamepanel);
            editbox.AddControls(boxName);

            //editbox.AddControlsLeftToRight(
            editbox.AddControlsSmart(
                a.Personality.GetUI().ToPanelLabeled("Traits").AddControlsTopRight(IconButton.CreateRandomizeButton(() => a.Personality.Randomize()).SetAnchor(Vector2.UnitX)),
                a.Skills.GetUI().ToPanelLabeled("Skills").AddControlsTopRight(IconButton.CreateRandomizeButton(() => a.Skills.Randomize()).SetAnchor(Vector2.UnitX)),
                a.Attributes.GetUI().ToPanelLabeled("Attributes").AddControlsTopRight(IconButton.CreateRandomizeButton(() => a.Attributes.Randomize()).SetAnchor(Vector2.UnitX)),
                new UICharacterCustomization(a));
            editbox.Validate(true);
        }
        //Control InitEditBox()
        //{
        //    var box = new GroupBox();
        //    var boxtraits = new GroupBox();
        //    var boxskills = new GroupBox();
        //    return box;
        //}
    }
}
