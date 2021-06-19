using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components.Vegetation
{
    class PlantableComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "Plantable"; }
        }
        public Action<GameObject, TargetArgs> PlantAction;
        public int Spacing = 1;

        public PlantableComponent()
        {

        }
        public PlantableComponent(Action<GameObject, TargetArgs> plantAction)
        {
            this.PlantAction = plantAction;
        }

        static public void PlantTree(GameObject a, TargetArgs t)
        {
            var slot = PersonalInventoryComponent.GetHauling(a);
            slot.Consume();
            BlockDefOf.Sapling.Place(a.Map, t.Global + Vector3.UnitZ, 0, 0, 0);
            if (a.Net is not Server server)
                return;
            var tree = GameObject.Create(GameObject.Types.Tree);
            TreeComponent.States.FreshlyPlanted(tree);
            server.InstantiateAndSpawn(tree);
        }

        static public void PlantSeed(GameObject a, TargetArgs t)
        {
            var itemSlot = PersonalInventoryComponent.GetHauling(a);
            var item = itemSlot.Object;
            BlockFarmland.Plant(a.Map, t.Global, item);
            //itemSlot.Consume(1);
            a.Net.EventOccured(Message.Types.ItemLost, a, item, 1);
        }

        internal override void GetAvailableTasks(GameObject parent, List<Interaction> list)
        {
            list.Add(new InteractionPlantNew());
        }
        public override object Clone()
        {
            return new PlantableComponent(this.PlantAction);
        }
        internal static Action<GameObject, TargetArgs> GetAction(GameObject a)
        {
            return a.GetComponent<PlantableComponent>().PlantAction;
        }
        public class InteractionPlantNew : Interaction
        {
            Action<GameObject, TargetArgs> PlantAction;

            public InteractionPlantNew()
                : base("PlantNewNew", 0)
            {

            }
            public InteractionPlantNew(Action<GameObject, TargetArgs> plantAction)
                : base("PlantNewNew", 0)
            {
                this.PlantAction = plantAction;
            }
            public override void Perform(GameObject a, TargetArgs t)
            {
                //this.PlantAction(a, t);
                PlantableComponent.GetAction(PersonalInventoryComponent.GetHauling(a).Object)(a, t);
            }
            public override object Clone()
            {
                return new InteractionPlantNew(this.PlantAction);
            }

        }

       
    }
}
