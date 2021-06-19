using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class DoorComponent : BlockComponent
    {
        class DoorState
        {
            public bool Closed { get; set; }
            public int Part { get; set; }
            public DoorState(byte data)
            {
                this.Closed = (data & 0x4) != 0x4;
                this.Part = data &= 0x3;
            }
        }
        DoorState GetState(byte data)
        {
            return new DoorState(data);
        }

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

        void SetState(Map map, Vector3 bottom, bool closed)
        {
            throw new NotImplementedException();
            for (int i = 0; i < 3; i++)
            {
                Vector3 g = bottom + Vector3.UnitZ * i;
                Cell cell = g.GetCell(map);

            }
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

        public override void Despawn(//Net.IObjectProvider net,
                    GameObject parent)
        {
            //Vector3 baseLoc = GetBase(net, parent);
            //for (int i = 0; i < 3; i++)
            //{
            //    Vector3 g = baseLoc + new Vector3(0, 0, i);
            //    g.TrySetCell(net, Block.Types.Air, 0);
            //}
            foreach (var g in GetChildren(parent.Net, parent.Global))
                g.TrySetCell(parent.Net, Block.Types.Air, 0);
        }

        private static Vector3 GetBase(Net.IObjectProvider net, Vector3 global)
        {
            //byte data = global.GetData(net.Map);
            byte data = net.Map.GetData(global);

            //Vector3 doorBase = parent.Global - Vector3.UnitZ * data;
            byte masked = data &= 0x3;
            int baseZ = (int)(global.Z - masked);
            Vector3 baseLoc = new Vector3(global.X, global.Y, baseZ);
            return baseLoc;
        }
        private static List<Vector3> GetChildren(Net.IObjectProvider net, Vector3 global)
        {
            List<Vector3> list = new List<Vector3>();
            Vector3 baseLoc = GetBase(net, global);
            for (int i = 0; i < 3; i++)
            {
                Vector3 g = baseLoc + new Vector3(0, 0, i);
                list.Add(g);
            }
            return list;
        }

        public override bool IsSolid(Map map, Vector3 global)
        {
            //byte data = global.GetData(map);
            //return (data & 0x4) != 0x4;

            return GetState(global.GetData(map)).Closed;
           
        }

        public DoorComponent()
        {
            this.Type = Block.Types.Door;
            this.Opaque = false; //true;//
            BlockComponent.Blocks[Block.Types.Door] = this;
        }


        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Activate:
                    //Map map = e.Network.Map;
                    Net.IObjectProvider net = e.Network;
                    Vector3 global = parent.Global;
                    ToggleClosed(net, global);
                    return true;

                //case Message.Types.Activate:
                //    //Activate(e.Sender, parent);
                //    GameObject master;
                //    parent.Map.TryGetBlockObject(parent.Global, out master);
                //    GetChildren(master).ForEach(c =>
                //    {
                //        Cell cell = c.GetCell(parent.Map);
                //        bool closed = (cell.BlockData & 0x1) == 0x1;
                //        cell.Orientation += closed ? 1 : -1;
                //        cell.BlockData ^= 1;
                //    });
                //  //  Closed = !Closed;
                    //return true;

                default:
                    break;
            }
            return false;
        }
        private static void ToggleClosed(Net.IObjectProvider net, Vector3 global, bool closed)
        {
            foreach (var g in GetChildren(net, global))
            {
                //Cell cell = g.GetCell(net.Map);
                Cell cell = net.Map.GetCell(g);

                bool lastClosed = (cell.BlockData & 0x4) == 0x4;
                if (lastClosed != closed)
                    cell.Orientation += closed ? 1 : -1;
                if (closed)
                    cell.BlockData &= 0x4;
                else
                    cell.BlockData ^= 0x4;
            }
        }
        private static void ToggleClosed(Net.IObjectProvider net, Vector3 global)
        {
            foreach (var g in GetChildren(net, global))
            {
                //Cell cell = g.GetCell(net.Map);
                Cell cell = net.Map.GetCell(g);

                //bool closed = (cell.BlockData & 0x1) == 0x1;
                bool closed = (cell.BlockData & 0x4) == 0x4;
                cell.Orientation += closed ? 1 : -1;
                cell.BlockData ^= 0x4;// 1;
            }
        }

        void GetState(GameObject parent, out bool open)
        {
            States data = (States)parent.Global.GetData(parent.Map);
            open = (data & States.Closed) == States.Closed ? false : true;
        }

        public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        {
            //List<Interaction> list = e.Parameters[0] as List<Interaction>;
            bool open;
            GetState(parent, out open);
            list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.Activate, parent, open ? "Open" : "Close"));
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
