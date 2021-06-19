using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;
using Start_a_Town_.Towns.Crafting;
using Start_a_Town_.Blocks;
using Start_a_Town_.AI;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Crafting
{
    abstract class BlockWorkstation : Block
    {
        public abstract AILabor Labor { get; }
        public BlockWorkstation(Types type)
            : base(type)
        {

        }
        public Interaction GetCraftingInteraction()
        {
            return new InteractionCraft();
        }
        public void Produce(GameObject actor, Vector3 global)
        {
            var entity = actor.Map.GetBlockEntity(global) as BlockEntityWorkstation;
            //var order = entity.GetQueuedOrders().First();
            var craft = entity.CurrentOrder;// order.Craft;
            var reactionID = craft.ReactionID;
            var reaction = Components.Crafting.Reaction.Dictionary[reactionID];
            var product = reaction.Products.First().GetProduct(reaction, null, craft.Materials, null);

            //var contents = entity.GetMaterialsContainer().Slots;// entity.Input.Slots;//.GetReagentSlots(actor);// actor.GetComponent<PersonalInventoryComponent>().GetContents().Concat(this.MaterialsContainer.Slots).ToList();
            var contents = entity.Input.Slots;// entity.Input.Slots;//.GetReagentSlots(actor);// actor.GetComponent<PersonalInventoryComponent>().GetContents().Concat(this.MaterialsContainer.Slots).ToList();

            if (!product.MaterialsAvailable(contents))
                return;
            product.ConsumeMaterials(actor.Net, contents);
            actor.Net.Map.EventOccured(Components.Message.Types.CraftingComplete, global, craft);
            craft.CraftProgress.Value = 0;
            // advance current order here or at manager?
            //if (entity.ExecutingOrders)
            //{
            //    //var orders = entity.GetQueuedOrders();
            //    //var order = orders.FirstOrDefault(o => o.Craft == craft);
            //    //if (order != null)
            //    //{
            //    //    //entity.RemoveOrder(actor.Net, order);
            //    //}
            //    //var nextorder = orders.FirstOrDefault();
            //    //if (nextorder != null)
            //    //{
            //    //    entity.CurrentProject = nextorder.Craft;
            //    //}
            //}
            //else
            //    entity.CurrentOrder = null;
            //entity.RemoveOrder(actor.Net, order);
            //actor.Map.EventOccured(Components.Message.Types.CraftingComplete, order);//craft);
            var server = actor.Net as Net.Server;

            if (server == null)
                return;
            product.Product.Global = global + Vector3.UnitZ;
            server.SyncInstantiate(product.Product);
            server.SyncSpawn(product.Product);
            return;
        }
        
        public abstract BlockEntityWorkstation GetWorkstationBlockEntity();

        public override ContextAction GetRightClickAction(Vector3 global)
        {
            //if (Components.PersonalInventoryComponent.GetHauling(Player.Actor).Object != null)
            //    return null;

            //var i = InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu) ? new InteractionBreakBlock() as Interaction : new InteractionAddMaterial() as Interaction;
            //list.Add(PlayerInput.RButton, i);

            if (!InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
                //if (Components.PersonalInventoryComponent.GetHauling(Player.Actor).Object != null)
                return new ContextAction(() => "Interface", () => ShowUI(global));
            return null;
        }
        internal override ContextAction GetContextRB(GameObject player, Vector3 global)
        {
            Func<bool> alt = () => InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu);
            Func<bool> inrange = () => Vector3.Distance(Player.Actor.Global, global) < RangeCheck.DefaultRange;
            Func<bool> hauling = () => Components.PersonalInventoryComponent.GetHauling(Player.Actor).Object != null;

            if (alt() && !hauling())
                return new ContextAction(new InteractionBreakBlock()) { Shortcut = PlayerInput.RButton, Available = () => alt() && !hauling() };
            else if (hauling())
                return new ContextAction(new InteractionAddMaterial()) { Shortcut = PlayerInput.RButton, Available = hauling };
            else
                return new ContextAction(() => "Interface", () => ShowUI(global)) { Shortcut = PlayerInput.RButton, Available = ()=> !alt() && !hauling() };
        }
        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Interaction> list)
        {
            if (Components.PersonalInventoryComponent.GetHauling(player).Object != null)
                list.Add(PlayerInput.RButton, new InteractionAddMaterial());
            else
                list.Add(PlayerInput.RButton, new InteractionBreakBlock());


            //list.Add(PlayerInput.RButton, new InteractionAddMaterial());
        }
        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, ContextAction> list)
        {
            Func<bool> alt = () => InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu);
            Func<bool> inrange = () => Vector3.Distance(Player.Actor.Global, global) < RangeCheck.DefaultRange;
            Func<bool> hauling = () => Components.PersonalInventoryComponent.GetHauling(Player.Actor).Object != null;
            var t = new TargetArgs(global);

            //if (Components.PersonalInventoryComponent.GetHauling(player).Object != null)
                list.Add(PlayerInput.RButton, new ContextAction(new InteractionAddMaterial(), player, t));
            //else
                list.Add(PlayerInput.RButton, new ContextAction(new InteractionBreakBlock(), player, t));
        }
        public override void GetContextActions(GameObject player, Vector3 global, ContextArgs a)
        {
            a.Actions.Add(new ContextAction(new InteractionAddMaterial()) { Shortcut = PlayerInput.RButton });
            a.Actions.Add(new ContextAction(new InteractionBreakBlock()) { Shortcut = PlayerInput.RButton });
            a.Actions.Add(new ContextAction("Interface", () => { ShowUI(global); return true; }) { Shortcut = PlayerInput.RButton });
            return;

            Func<bool> alt = ()=>InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu);
            Func<bool> inrange = () => Vector3.Distance(Player.Actor.Global, global) < RangeCheck.DefaultRange;
            Func<bool> hauling = () => Components.PersonalInventoryComponent.GetHauling(Player.Actor).Object != null;
            var t = new TargetArgs(global);
            Interaction i = new InteractionAddMaterial();
            a.Actions.Add(new ContextAction(PlayerInput.RButton.ToString() + ": " + i.Name, null, () => hauling() && i.Conditions.Evaluate(Player.Actor, t)));// () => true));
            i = new InteractionBreakBlock();
            a.Actions.Add(new ContextAction(PlayerInput.RButton.ToString() + ": " + i.Name, null, () => alt() && i.Conditions.Evaluate(Player.Actor, t)));// () => true));

            a.Actions.Add(new ContextAction(PlayerInput.RButton.ToString() + ": " + "Interface", () => { ShowUI(global); return true; }, () =>
                {
                    return (!hauling() && (!alt() && inrange()));
                }));
            //base.GetContextActions(player, global, a);
            
            return;

            //var alt = InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu);
            //var inrange = Vector3.Distance(Player.Actor.Global, global) < RangeCheck.DefaultRange;
            //var hauling = Components.PersonalInventoryComponent.GetHauling(Player.Actor).Object != null;
            //if (!hauling && (!alt && inrange))
            //    a.Actions.Add(new ContextAction(() => "Interface", () => ShowUI(global)));
            //else
            //    base.GetContextActions(player, global, a);
        }
        //public override void GetContextActions(GameObject player, Vector3 global, ContextArgs a)
        //{
        //    var inrange = Vector3.Distance(Player.Actor.Global, global) < RangeCheck.DefaultRange;
        //    if (!inrange)
        //        return;

        //    var hauling = Components.PersonalInventoryComponent.GetHauling(Player.Actor).Object != null;
        //    var alt = InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu);

        //    base.GetContextActions(player, global, a);
        //    if (alt && !hauling)
        //        a.Actions.Add(new ContextAction("Break", () => true));
        //        //if (Components.PersonalInventoryComponent.GetHauling(Player.Actor).Object != null)
        //    else if(hauling)
        //        a.Actions.Add(new ContextAction("Insert", () => true));
        //    else
        //        a.Actions.Add(new ContextAction(() => "Interface", () => ShowUI(global)));
        //}

        public override void OnDrop(GameObject a, GameObject dropped, TargetArgs t)
        {
            var entity = a.Map.GetBlockEntity(t.Global) as BlockEntityWorkstation;
            if (entity == null)
                throw new ArgumentException();
            entity.Insert(dropped);
        }
        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            return new List<Interaction>(){
                new InteractionCraft(),
                new InteractionAddMaterial()//map.GetBlockEntity(global) as Entity)
            };
        }
        public override BlockEntity GetBlockEntity()
        {
            return this.GetWorkstationBlockEntity();
        }

        private void ShowUI(Vector3 global)
        {
            var entity = Net.Client.Instance.Map.GetBlockEntity(global) as BlockEntityWorkstation;
            var window = new WindowEntityInterface(entity, this.Name, () => global);
            window.Location = ScreenManager.Current.Camera.GetScreenPosition(global);
            var ui = new WorkstationUI(global, entity);
            window.Client.Controls.Add(ui);
            window.ConformToScreen();

            window.Show();
        }
    }
}
