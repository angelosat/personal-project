using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Abilities;

namespace Start_a_Town_.Components
{
    public class Ability
    {


        Message.Types MessageType;
        string Name;



        static public Script GetScript(Script.Types scriptID)
        {
            return Script.Registry[scriptID].Clone() as Script;
        }


        //static public GameObjectSlot Create(
        //    Script.Types id, 
        //    int iconIndex, 
        //    string name, 
        //    string description, 
        //    Action<Net.IObjectProvider, GameObject, TargetArgs, byte[]> execute, 
        //    float basetime = 0, 
        //    Formula speedMod = null, 
        //    ConditionCollection reqs = null, 
        //    Func<GameObject, TargetArgs, float, bool> range = null,
        //    float rangevalue = Interaction.DefaultRange)// Func<GameObject, float> time)
        //{
        //    GameObject obj = new GameObject();
        //    obj["Info"] = new GeneralComponent(GameObject.Types.Ability, ObjectType.Ability, name, description);
        //    obj.AddComponent<GuiComponent>().Initialize(iconIndex);
        //    Script script = new Script(id, a => { }, name,
        //        basetime, speedMod, range, rangevalue, reqs) { Execute = execute };
        //    obj["Ability"] = script;

        //    // register script to new script registry until i completely remove the old one
        //    Script.Registry.Add(script.ID, script);
        //    return new GameObjectSlot(obj);
        //}

        static public AbilitySlot GetAbilityFromKey(System.Windows.Forms.Keys key)
        {
            switch (key)
            {
                case System.Windows.Forms.Keys.LButton:
                    return AbilitySlot.Primary;
                case System.Windows.Forms.Keys.RButton:
                    return AbilitySlot.Secondary;
                case System.Windows.Forms.Keys.E:
                    return AbilitySlot.Function1;
                case System.Windows.Forms.Keys.F:
                    return AbilitySlot.PickUp;
                case System.Windows.Forms.Keys.Q:
                    return AbilitySlot.Function3;
                case System.Windows.Forms.Keys.G:
                    return AbilitySlot.Throw;
                default:
                    return AbilitySlot.None;
            }
        }

        static public string GetSlotText(AbilitySlot slot)
        {
            switch (slot)
            {
                case AbilitySlot.Primary:
                    return "LM";
                case AbilitySlot.Secondary:
                    return "RB";
                case AbilitySlot.Function1:
                    return "E";
                case AbilitySlot.PickUp:
                    return "F";
                case AbilitySlot.Function3:
                    return "Q";
                case AbilitySlot.Throw:
                    return "G";
                default:
                    return "";
            }
        }

        static public void Write(BinaryWriter writer, Script.Types scriptID, GameObject target, Vector3? face)
        {
            writer.Write((int)scriptID);
            TargetArgs.Write(writer, target, face);
            writer.Write(0);
        }
        static public void Write(BinaryWriter writer, Script.Types scriptID, TargetArgs target)
        {
            writer.Write((int)scriptID);
            target.Write(writer);
            writer.Write(0);
        }
        static public void Write(BinaryWriter writer, Script.Types scriptID)
        {
            writer.Write((int)scriptID);
            TargetArgs.Empty.Write(writer);
            writer.Write(0);
        }
        static public void Write(BinaryWriter writer, Script.Types scriptID, TargetArgs target, params byte[][] data)
        {
            writer.Write((int)scriptID);
            target.Write(writer);

            int count = 0;
            for (int i = 0; i < data.Length; i++)
                count += data[i].Length;
            byte[] combined = new byte[count];
            int index = 0;
            for (int i = 0; i < data.Length; i++)
            {
                data[i].CopyTo(combined, index);
                index += data[i].Length;
            }

            writer.Write(count);
            writer.Write(combined);
        }
        static public void Write(BinaryWriter writer, Script.Types scriptID, TargetArgs target, Action<BinaryWriter> argsWriter)
        {
            writer.Write((int)scriptID);
            target.Write(writer);

            using (BinaryWriter w = new BinaryWriter(new MemoryStream()))
            {
                argsWriter(w);
                //writer.Write(w.BaseStream.Length);
                byte[] data = (w.BaseStream as MemoryStream).ToArray();
                writer.Write(data.Length);
                writer.Write(data);
            }
        }
    }

}
