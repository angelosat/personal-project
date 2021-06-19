using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.AI;
using Start_a_Town_.AI;
using Start_a_Town_.UI;

namespace Start_a_Town_.AI.Behaviors
{
    class AICommands : BehaviorSequence
    {
        public Queue<AIInstruction> Commands = new Queue<AIInstruction>();
        AIState State;

        public override Behavior Initialize(AIState state)
        {
            this.State = state;
            return this;
        }
        public AICommands()
        {
            this.Children = new List<Behavior>(){
                new AINextCommand(),
                new AIMoveTo(),
                new AIPerformCommand()
            };
        }
        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            this.State = state;
            //state.Properties["Commands"] = this.Commands;
            state.Commands = this.Commands;
            return base.Execute(parent, state);
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.AICommand:
                    string action = (string)e.Parameters[0];
                    TargetArgs target = e.Parameters[1] as TargetArgs;
                    this.Commands.Enqueue(new AIInstruction(target, target.GetInteraction(parent.Net, action)));
                    break;

                default:
                    break;
            }
            return true;
        }

        protected override void HandleCommunication(GameObject parent, GameObject sender, string option)
        {
            switch (option)
            {
                case "Manage Inventory":
                    //ManageInventory(parent);
                    (parent.Net as Net.Server).RemoteProcedureCall(new TargetArgs(parent), Components.Message.Types.ManageEquipment, w =>
                    {
                        w.Write(sender.InstanceID);
                    });
                    break;

                default:
                    break;
            }
        }
        internal override void HandleRPC(GameObject parent, Components.Message.Types type, System.IO.BinaryReader r)
        {
            switch(type)
            {
                case Components.Message.Types.ManageEquipment:
                    if(parent.Net is Net.Client)
                    {
                        var handler = parent.Net.GetNetworkObject(r.ReadInt32());
                        if (handler == Player.Actor)
                            //ManageInventory(parent);
                            parent.Net.EventOccured(Components.Message.Types.ManageEquipment, parent);
                    }
                    break;

                default:
                    break;
            }
        }
        public override void GetDialogOptions(GameObject parent, GameObject speaker, List<DialogOption> options)
        {
            base.GetDialogOptions(parent, speaker, options);
            options.Add(new DialogOption("Manage Inventory", parent));
        }
        void ManageInventory(GameObject parent)
        {
            //this.State.AttentionDecay = 0;
            //this.State.Conversation.Add(parent, "Here's my stuff.");

            //(parent.Net as Net.Server).AIHandler.AIDialog(p)
            //var window = new Window();
            //window.Title = parent.Name;
            var window = new WindowEntityInterface(parent, parent.Name, () => parent.Global);
            window.HideAction = () => this.State.AttentionDecay = this.State.AttentionDecayDefault;
            var ui = new Components.Inventory.InventoryInterface().Initialize(parent);
            window.Client.Controls.Add(ui);
            window.Show();
        }
        public override object Clone()
        {
            return new AICommands();
        }
    }
}
