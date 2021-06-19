using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Scripts
{
    class ScriptCrafting : Script
    {
        public override float BaseTimeInSeconds
        {
            get
            {
                return 1;
            }
        }

        float Percentage
        {
            get { return (float)(this.Time / this.Length); }
        }
        public double Time { get; set; }
        public double Length { get; set; }
        //public ScriptArgs Args { get; set; }

        public ScriptCrafting()
        {
            this.Name = "Crafting";
            this.ID = Script.Types.Crafting;
            this.Requirements =
                new InteractionConditionCollection(
                    new InteractionCondition(
                        (actor, target, args) =>
                        {
                            GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(args, 0);
                            GameObject bp = GameObject.Objects[bpID];
                            return BlueprintComponent.MaterialsAvailable(bp, target.GetComponent<InventoryComponent>().Containers.First());
                        }, "Materials missing!"),
                    new InteractionCondition(
                        (actor, target, args) =>
                        {
                            // check if actor has space
                            GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(args, 0);
                            GameObject bp = GameObject.Objects[bpID];
                            return actor.GetComponent<InventoryComponent>().Containers.First()
                                .Find(slot => slot.HasValue ? (slot.Object.ID == bp.GetComponent<BlueprintComponent>().Blueprint.ProductID && slot.StackSize < slot.StackMax) : true) != null;
                        }, "Not enough space to craft this item.")
                );
        }

        public override void Start(ScriptArgs args)
        {
            this.Time = 0;
            this.Length = GetTimeInMs(args.Actor);
            this.ScriptState = ScriptState.Running;
            this.ArgsSnapshot = args;
            args.Actor.GetComponent<ActorSpriteComponent>().Body.Start(Graphics.AnimationCollection.Working);
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            // if moving, interrupt
            if (parent.GetComponent<ControlComponent>().RunningScripts.ContainsKey(Script.Types.Walk))
            {
                Stop(new ScriptArgs(net, parent));
                return;
            }
            this.Time += 1000 / (float)Engine.TargetFps;
            if (this.Time < this.Length)
                return;
            //OnSuccess(this.Args);
            this.Success(this.ArgsSnapshot);
            this.ScriptState = ScriptState.Finished;
        }

        public override void Success(ScriptArgs args)
        {
            GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(this.ArgsSnapshot.Args, 0);
            GameObject bpObj = GameObject.Objects[bpID];
            ItemContainer container = args.Actor.GetComponent<InventoryComponent>().Containers.First();
            Blueprint bp = bpObj.GetComponent<BlueprintComponent>().Blueprint;
            GameObject product = bp.Craft();

            // consume materials
            foreach (var mat in bp.Stages.First())
            {
                int amount = mat.Value;
                //foreach 
                var foundMats = (
                    from slot in container
                    where slot.HasValue
                    where slot.Object.ID == mat.Key
                    select slot);
                //{

                //}
                var queue = new Queue<GameObjectSlot>(foundMats);
                while (amount > 0 && queue.Count > 0)
                {
                    var slot = queue.Peek();
                    amount--;
                    slot.StackSize--;
                    if (slot.StackSize == 0)
                        queue.Dequeue();
                }
                if (amount > 0)
                    throw new Exception("Invalid materials");
            }

            // send object
            args.Net.PostLocalEvent(args.Actor, Message.Types.Receive, product);
        }

        public override void Finish(ScriptArgs args)
        {
            args.Actor.GetComponent<ActorSpriteComponent>().Body.FadeOut(Graphics.AnimationCollection.Working);
        }

        public override void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        {
            Vector3 global = parent.Global;

            Rectangle bounds = camera.GetScreenBounds(global, parent["Sprite"].GetProperty<Sprite>("Sprite").GetBounds());
            Vector2 scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            Vector2 barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            Vector2 textLoc = new Vector2(barLoc.X, scrLoc.Y);
            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, Percentage);
            UIManager.DrawStringOutlined(sb, this.Name, textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
        }

        public override object Clone()
        {
            return new ScriptCrafting();
        }



        //public static GameObjectSlot ScriptCraftingPerson
        //{
        //    get
        //    {
        //        return Script.Create(Script.Types.CraftingPerson, 0, "Crafting", "Craft items.",
        //            (net, actor, target, args) =>
        //            {
        //                GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(args, 0);
        //                GameObject bpObj = GameObject.Objects[bpID];
        //                ItemContainer container = actor.GetComponent<InventoryComponent>().Containers.First();
        //                //bp.GetComponent<BlueprintComponent>().Blueprint.Craft(container, out product);
        //                Blueprint bp = bpObj.GetComponent<BlueprintComponent>().Blueprint;
        //                GameObject product = bp.Craft();

        //                // consume materials
        //                foreach (var mat in bp.Stages.First())
        //                {
        //                    foreach (var slot in
        //                        from slot in container
        //                        where slot.HasValue
        //                        where slot.Object.ID == mat.Key
        //                        select slot)
        //                    {

        //                    }


        //                }

        //                // send object
        //                net.PostLocalEvent(actor, ObjectEventArgs.Create(Message.Types.Receive, new object[] { product }));

        //                //net.PostLocalEvent(target.Object, ObjectEventArgs.Create(Message.Types.Craft, new object[] { bpID, actor }));
        //            },
        //            2,
        //            new Formula(Formula.Types.Default, "Default"),
        //            new InteractionConditionCollection(
        //              new InteractionCondition((actor, target, args) =>
        //              {
        //                  GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(args, 0);
        //                  GameObject bp = GameObject.Objects[bpID];
        //                  return BlueprintComponent.MaterialsAvailable(bp, target.GetComponent<InventoryComponent>().Containers.First());
        //              }, "Materials missing!"),
        //              new InteractionCondition((actor, target, args) =>
        //              {
        //                  // check if actor has space
        //                  GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(args, 0);
        //                  GameObject bp = GameObject.Objects[bpID];
        //                  return actor.GetComponent<InventoryComponent>().Containers.First()
        //                      .Find(slot => slot.HasValue ? (slot.Object.ID == bp.GetComponent<BlueprintComponent>().Blueprint.ProductID && slot.StackSize < slot.StackMax) : false) != null;
        //              }, "Not enough space!")
        //              )
        //            ,
        //            range: (a1, a2) => true);
        //    }
        //}
    }
}
