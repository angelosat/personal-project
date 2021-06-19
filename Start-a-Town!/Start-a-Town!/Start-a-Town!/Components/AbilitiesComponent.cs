using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Start_a_Town_.Components
{
    class AbilitiesComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Abilities";
            }
        }

        public List<Ability> Abilities { get { return (List<Ability>)this["Abilities"]; } set { this["Abilities"] = value; } }

        public AbilitiesComponent()
        {
            this.Abilities = new List<Ability>();
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.ExecuteScript:

                    //e.Data.Translate(e.Network, reader =>
                    //{
                    //    Script.Types scriptID = (Script.Types)reader.ReadInt32();
                    //    TargetArgs target = TargetArgs.Read(e.Network, reader);
                    //    Script.GetAbilityObject(scriptID);
                    //    Interaction.StartNew(e.Network, parent, target, scriptID);
                    //});

                    ScriptEventArgs args = e.Data.Translate<ScriptEventArgs>(e.Network);
                  //  Script.GetAbilityObject(args.ScriptID);
                    InteractionOld.StartNew(e.Network, parent, args.Target, args.ScriptID, args.Parameters);

                    return true;

                default:
                    return false;
            }
        }

        public void Refresh(GameObject parent)
        {
            BodyComponent body;
            if (parent.TryGetComponent<BodyComponent>(out body))
            {
                foreach (var part in body.BodyParts)
                {

                }
            }
        }

        public override object Clone()
        {
            return new AbilitiesComponent();
        }
    }
}
