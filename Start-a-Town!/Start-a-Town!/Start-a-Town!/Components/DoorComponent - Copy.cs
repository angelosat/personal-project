using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class DoorComponent : Component
    {
        public override string ComponentName
        {
            get { return "Door"; }
        }
        [Flags]
        enum States {Open = 0x0, Closed = 0x1};

      //  List<Vector3> Children = new List<Vector3>();
        List<Vector3> GetChildren(GameObject parent)
        {
            return new List<Vector3>() { parent.Global, 
                        parent.Global + new Vector3(0, 0, 1), parent.Global + new Vector3(0, 0, 2) };
        }

        public override void Spawn(Net.IObjectProvider net, GameObject parent)
        {
            //foreach (var g in new List<Vector3>(){
            //        (parent.Global + new Vector3(0, 0, 0)),
            //        (parent.Global + new Vector3(0, 0, 1)),
            //        (parent.Global + new Vector3(0, 0, 2))})
            //    g.TrySetCell(net, Block.Types.Door);
            for (int i = 0; i < 3; i++)
            {
                Vector3 g = parent.Global + new Vector3(0, 0, i);
                byte data = (byte)i;
                g.TrySetCell(net, Block.Types.Door, data);
            }
        }

        public override void Despawn(Net.IObjectProvider net, GameObject parent)
        {
            byte data = parent.Global.GetData(net.Map);
            //Vector3 doorBase = parent.Global - Vector3.UnitZ * data;
            int baseZ = (int)(parent.Global.Z - data);
            for (int i = 0; i < 3; i++)
            {
                Vector3 g = parent.Global + new Vector3(0, 0, i);
                g.TrySetCell(net, Block.Types.Air, 0);
            }
        }

        //bool Closed { get { return (bool)this["Closed"]; } set { this["Closed"] = value; } }

        //public DoorComponent(bool closed = true)
        //{
        //   // this["Closed"] = closed;
        //}

        //public override void Spawn(GameObject parent)
        //{
        //    // parent.Global.TrySetCell(parent.Map, Block.Types.Door);
        //    Children = new List<Vector3>() { parent.Global, 
        //                parent.Global + new Vector3(0, 0, 1), parent.Global + new Vector3(0, 0, 2) };
        //    //Chunk.AddBlockObject(parent.Map, parent, parent.Global);
        //    //Chunk.AddBlockObject(parent.Map, parent, parent.Global + new Vector3(0, 0, 1));
        //    //Chunk.AddBlockObject(parent.Map, parent, parent.Global + new Vector3(0, 0, 2));

        //    foreach (var g in Children)
        //    {
        //        Chunk.AddBlockObject(parent.Map, parent, g);
        //        g.TrySetCell(parent.Map, Block.Types.Door);
        //    }

        //}

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                
                //case Message.Types.Removed:
                //    GetChildren(parent).ForEach(g => { 
                //        Chunk.RemoveBlockObject(parent.Map, g);
                //        g.TrySetCell(parent.Map, Block.Types.Air);
                //    });
                //  //  Chunk.RemoveBlockObject(parent.Map, parent.Global);
                //    return true;
                case Message.Types.Activate:
                    //Activate(e.Sender, parent);
                    GameObject master;
                    parent.Map.TryGetBlockObject(parent.Global, out master);
                    GetChildren(master).ForEach(c =>
                    {
                        Cell cell = c.GetCell(parent.Map);
                        bool closed = (cell.BlockData & 0x1) == 0x1;
                        cell.Orientation += closed ? 1 : -1;
                        cell.BlockData ^= 1;
                    });
                  //  Closed = !Closed;

                    
                    return true;

                default:
                    break;
            }
            return false;
        }

        void GetState(GameObject parent, out bool open)
        {
            States data = (States)parent.Global.GetData(parent.Map);
            open = (data & States.Closed) == States.Closed ? false : true;
        }

        public override void Query(GameObject parent, List<Interaction> list)//GameObjectEventArgs e)
        {
            //List<Interaction> list = e.Parameters[0] as List<Interaction>;
            bool open;
            GetState(parent, out open);
            list.Add(new Interaction(TimeSpan.Zero, Message.Types.Activate, parent, open ? "Open" : "Close"));
        }

        //public override bool Activate(GameObject actor, GameObject parent)
        //{
        //    Toggle();
        //    UpdateSprite(parent);
        //    parent.GetComponent<PhysicsComponent>("Physics")["Solid"] = Closed;
        //    //UpdateCell(parent);
        //    return true;
        //}

        //private void UpdateCell(GameObject parent)
        //{
        //    PhysicsComponent phys;
        //    int height = parent.TryGetComponent<PhysicsComponent>("Physics", out phys) ? (int)phys["Height"] : 1;
        //    for (int i = 0; i < height; i++)
        //        Position.GetCell(parent.Map, parent.Global + new Vector3(0, 0, i)).Solid = Closed;
        //}

        //bool Toggle()
        //{
        //    bool current = !(bool)this["Closed"];
        //    this["Closed"] = current;
        //    return current;
        //}

        //public bool Closed { get { return (bool)this["Closed"]; } set { this["Closed"] = value; } }

        //void UpdateSprite(GameObject parent)
        //{
        //    SpriteComponent sprComp;
        //    if (!parent.TryGetComponent<SpriteComponent>("Sprite", out sprComp))
        //        return;
        //    sprComp.Orientation += Closed ? -1 : 1;
        //}

        public override object Clone()
        {
            //return new DoorComponent((bool)this["Closed"]);
            return new DoorComponent();
        }

        //internal override List<SaveTag> Save()
        //{
        //    List<SaveTag> save = new List<SaveTag>();
        //    save.Add(new SaveTag(SaveTag.Types.Bool, "Closed", Closed));
        //    return save;
        //}

        //internal override void Load(SaveTag compTag)
        //{
        //    //Dictionary<string, Tag> byName = compTag.ToDictionary();
        //    //this.Closed = (bool)byName["Closed"].Value;
        //    this.Closed = compTag.TagValueOrDefault("Closed", false); 
        //}
    }
}
