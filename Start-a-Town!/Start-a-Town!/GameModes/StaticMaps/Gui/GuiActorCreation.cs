﻿using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System.Collections.Generic;

namespace Start_a_Town_
{
    class GuiActorCreation : GroupBox
    {
        public GuiActorCreation(List<Actor> actors)
        {
            actors.AddRange(new[] { Actor.Create(ActorDefOf.Npc), Actor.Create(ActorDefOf.Npc), Actor.Create(ActorDefOf.Npc) });
            var actorslistbox = new ScrollableBoxNewNew(200, UIManager.LargeButton.Height * 8, ScrollModes.Vertical);

            var editbox = new Panel(0, 0, 500, actorslistbox.Height) { AutoSize = false };

            ListBoxNoScroll<Actor, ButtonNew> actorsUI = null;
            actorsUI = new ListBoxNoScroll<Actor, ButtonNew>(btnInit) { BackgroundColor = Color.Red };
            var addactorbutton = new Button("Create", addActor, actorslistbox.Client.Width);

            actorslistbox.AddControlsVertically(addactorbutton, actorsUI);

            actorsUI.AddItems(actors);

            this.AddControlsVertically(actorslistbox);

            editbox.Location = actorslistbox.TopRight;
            this.AddControls(editbox);

            void addActor()
            {
                var a = ActorDefOf.Npc.Create() as Actor;
                actors.Add(a);
                actorsUI.AddItems(a);
            }

            ButtonNew btnInit(Actor a)
            {
                var btn = ButtonNew.CreateBig(() => this.Expand(a, editbox), actorslistbox.Client.Width, a.RenderIcon(), () => a.Npc.FullName);
                var deleteBtn = IconButton.CreateCloseButton();
                deleteBtn.Location = btn.TopRight;
                deleteBtn.Anchor = Vector2.UnitX;
                deleteBtn.LeftClickAction = () => new MessageBox("", "Delete " + a.Name + " ?", () =>
                {
                    actors.Remove(a);
                    actorsUI.RemoveItems(a);
                }, () => { }).ShowDialog();
                btn.AddControls(deleteBtn);
                return btn;
            }
        }

        readonly GuiCharacterCustomization GuiCustomization = new();

        private void Expand(Actor a, Control editbox)
        {
            editbox.ClearControls();

            var boxName = new GroupBox();

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
            editbox.AddControlsSmart(
                a.Personality.GetUI().ToPanelLabeled("Traits").AddControlsTopRight(IconButton.CreateRandomizeButton(() => a.Personality.Randomize()).SetAnchor(Vector2.UnitX)),
                a.Skills.GetUI().ToPanelLabeled("Skills").AddControlsTopRight(IconButton.CreateRandomizeButton(() => a.Skills.Randomize()).SetAnchor(Vector2.UnitX)),
                a.Attributes.GetUI().ToPanelLabeled("Attributes").AddControlsTopRight(IconButton.CreateRandomizeButton(() => a.Attributes.Randomize()).SetAnchor(Vector2.UnitX)),
                GuiCustomization.SetTag(a));// a));
            editbox.Validate(true);
        }
    }
}
