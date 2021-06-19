using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Modules.Crafting;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;
using Start_a_Town_.Towns.Crafting;
using Start_a_Town_.Blocks;
using Start_a_Town_.AI;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes;
using Start_a_Town_.Crafting;
using Start_a_Town_.Graphics;
namespace Start_a_Town_
{
    sealed class BlockWorkstation : BlockWithEntity, IBlockWorkstation//Workstation
    {
        AtlasDepthNormals.Node.Token[] Orientations = Block.TexturesCounter;
        Type BlockEntityType;
        public BlockWorkstation(Block.Types workstationType, Type blockEntityType)
            : base(workstationType, opaque: false, solid: true)
        {
            this.BlockEntityType = blockEntityType;
            this.Variations.Add(this.Orientations.First());
            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfMaterialType(MaterialType.Wood),
                        Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            Towns.Constructions.ConstructionsManager.Production.Add(this.Recipe);
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
        }

        public override Graphics.AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return this.Orientations[orientation];
        }
        public override BlockEntity CreateBlockEntity()
        {
            //return new BlockKitchenEntity();
            return Activator.CreateInstance(this.BlockEntityType) as BlockEntity;
        }

        public override BlockRecipe GetRecipe()
        {
            return this.Recipe;
        }
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Database[blockdata];
        }
        public override IEnumerable<byte> GetCraftingVariations()
        {
            return this.Ingredient.AllowedMaterials.Select(m => (byte)m.ID);
            throw new Exception();
            var vars = (from mat in Material.Database.Values
                        where mat.Type == MaterialType.Wood
                        select (byte)mat.ID).ToList();
            return vars;
        }
        public override Vector4 GetColorVector(byte data)
        {
            return this.GetColorFromMaterial(data);

        }

    }
    abstract class BlockWorkstationOld : Block
    {
        protected AtlasDepthNormals.Node.Token[] Orientations = new AtlasDepthNormals.Node.Token[4];
        public override bool IsRoomBorder => false;
        public abstract JobDef Labor { get; }
        public BlockWorkstationOld(Types type, bool opaque = true, bool solid = true)
            : base(type, opaque: opaque, solid: solid)// opaque: true)
        {

        }
        public Interaction GetCraftingInteraction()
        {
            return new InteractionCraftNew();
        }
      
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Database[blockdata];
        }
        public void Produce(GameObject actor, Vector3 global)
        {
            var entity = actor.Map.GetBlockEntity(global) as BlockEntityWorkstation;
            var craft = entity.CurrentOrder;// order.Craft;
            var reactionID = craft.ReactionID;
            var reaction = Components.Crafting.Reaction.Dictionary[reactionID];
            var product = reaction.Products.First().GetProduct(reaction, null, craft.Materials, null);

            var contents = entity.Input.Slots;
            if (!product.MaterialsAvailable(contents))
                return;
            product.ConsumeMaterials(actor.Net, contents);
            actor.Net.Map.EventOccured(Components.Message.Types.CraftingComplete, global, craft, actor);
            craft.CraftProgress.Value = 0;


            if (actor.Net is not Server server)
                return;
            product.Product.Global = global + Vector3.UnitZ;
            server.SyncInstantiate(product.Product);
            server.SyncSpawn(product.Product);
            return;
        }

        internal virtual void ConsumeFuel(IMap map, Vector3 global, float amount = 1)
        {
        }

        
        
        public abstract BlockEntityWorkstation CreateWorkstationBlockEntity();

        public override ContextAction GetRightClickAction(Vector3 global)
        {
            if (!InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
                return new ContextAction(() => "Interface", () => ShowUI(global));
            return null;
        }
        internal override ContextAction GetContextRB(GameObject player, Vector3 global)
        {
            bool alt() => InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu);
            bool inrange() => Vector3.Distance(PlayerOld.Actor.Global, global) < RangeCheck.DefaultRange;
            bool hauling() => Components.PersonalInventoryComponent.GetHauling(PlayerOld.Actor).Object != null;

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
        }
        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, ContextAction> list)
        {
            static bool alt() => InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu);
            bool inrange() => Vector3.Distance(PlayerOld.Actor.Global, global) < RangeCheck.DefaultRange;

            static bool hauling() => Components.PersonalInventoryComponent.GetHauling(PlayerOld.Actor).Object != null;
            var t = new TargetArgs(global);

            list.Add(PlayerInput.RButton, new ContextAction(new InteractionAddMaterial(), player, t));
            list.Add(PlayerInput.RButton, new ContextAction(new InteractionBreakBlock(), player, t));
        }
        public override void GetContextActions(GameObject player, Vector3 global, ContextArgs a)
        {
            a.Actions.Add(new ContextAction(new InteractionAddMaterial()) { Shortcut = PlayerInput.RButton });
            a.Actions.Add(new ContextAction(new InteractionBreakBlock()) { Shortcut = PlayerInput.RButton });
            a.Actions.Add(new ContextAction("Interface", () => { ShowUI(global); return true; }) { Shortcut = PlayerInput.RButton });
        }
        public override void OnDrop(GameObject a, GameObject source, TargetArgs t, int quantity = -1)
        {
            if (a.Map.GetBlockEntity(t.Global) is not BlockEntityWorkstation entity)
                throw new ArgumentException();
            entity.OnDrop(a, source, t, quantity);
           
        }
       
        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            return new List<Interaction>(){
                new InteractionCraftNew(),
                new InteractionAddMaterial()
            };
        }
        public override BlockEntity CreateBlockEntity()
        {
            return this.CreateWorkstationBlockEntity();
        }
        
        public override void GetInterface(Vector3 global)
        {
            base.GetInterface(global);
            WindowTargetInterface.Instance.Client.Controls.Add(new Button("Operate") { LeftClickAction = () => ShowUI(global) });
        }
        public override void GetInterface(IMap map, Vector3 global, WindowTargetManagement window)
        {
            window.PanelActions.AddControls(new Button("Orders") { LeftClickAction = () => ShowUI(global) });
        }
        internal override void Select(UISelectedInfo uISelectedInfo, IMap map, Vector3 vector3)
        {
            base.Select(uISelectedInfo, map, vector3);
            uISelectedInfo.AddTabAction("Orders", () => ShowUI(vector3));
        }
        
        internal override void GetQuickButtons(UISelectedInfo uISelectedInfo, IMap map, Vector3 vector3)
        {
            uISelectedInfo.AddTabAction("Orders", () => ShowUI(vector3));
        }
        internal override IEnumerable<Vector3> GetOperatingPositions(Cell cell)
        {
            yield return Front(cell);
        }
    }
}
