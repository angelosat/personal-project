using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Rooms;

using Start_a_Town_.Components;

namespace Start_a_Town_.Control
{
    public class HaulTool : ControlTool
    {
        public GameObjectSlot Hauling;
        public HaulTool(GameObjectSlot hauling)
        {
            this.Hauling = hauling;
        }

        internal override void Jump()
        {
            PhysicsComponent phys = Player.Actor.GetComponent<PhysicsComponent>("Physics");
            PositionComponent pos = Player.Actor.GetComponent<PositionComponent>("Position");

            if (pos.GetProperty<Vector3>("Speed").Z == 0)
                pos["Speed"] = pos.GetProperty<Vector3>("Speed") + new Vector3(0, 0, 0.4f);
        }


        internal override void KeyDown(InputState e)
        {
            if (e.KeyHandled)
                return;
            int xx = 0, yy = 0;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.W))
            {
                xx -= 1;
                yy -= 1;
            }
            else if (InputState.IsKeyDown(System.Windows.Forms.Keys.S))
            {
                yy += 1;
                xx += 1;
            }
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.A))
            {
                yy += 1;
                xx -= 1;
            }
            else if (InputState.IsKeyDown(System.Windows.Forms.Keys.D))
            {
                yy -= 1;
                xx += 1;
            }

            if (xx != 0 || yy != 0)
            {
                PositionComponent posComp = Player.Actor.GetComponent<PositionComponent>("Position");
                PhysicsComponent physComp = Player.Actor.GetComponent<PhysicsComponent>("Physics");

                double rx, ry;
                double cos = Math.Cos((-Rooms.Ingame.Instance.Camera.Rotation) * Math.PI / 2f);
                double sin = Math.Sin((-Rooms.Ingame.Instance.Camera.Rotation) * Math.PI / 2f);
                rx = (xx * cos - yy * sin);
                ry = (xx * sin + yy * cos);
                int roundx, roundy;
                roundx = (int)Math.Round(rx);
                roundy = (int)Math.Round(ry);

                Vector2 NextStep = new Vector2(roundx, roundy);
                NextStep.Normalize();

                float walkSpeed = Player.Actor.GetComponent<StatsComponent>("Stats").GetProperty<float>("Walk Speed");
                if (walkSpeed == 0)
                    Console.WriteLine("MALAKA TO WALKSPEED EINAI MHDEN");
                NextStep *= walkSpeed;
                //Console.WriteLine(NextStep);
                //posComp.Speed = new Vector3(NextStep.X, NextStep.Y, posComp.Speed.Z);
                posComp["Speed"] = new Vector3(NextStep.X, NextStep.Y, posComp.GetProperty<Vector3>("Speed").Z);
            }
        }

        internal override void PickUp()
        {
            GameObject tar;// = Controller.Instance.Mouseover.Object as GameObject;
            //if (tar == null)
            if(!Controller.Instance.Mouseover.TryGet<GameObject>(out tar))
                return;
            //tar.HandleMessage(Player.Actor, Message.Types.Activate);
            Position pos = Player.Actor.GetComponent<PositionComponent>("Position").GetProperty<Position>("Position");
            Position targetPos = tar.GetComponent<PositionComponent>("Position").GetProperty<Position>("Position");
            bool canReach = CanReach(Player.Actor, tar);//(Vector3.Distance(pos.Global, targetPos.Global) < 2);
            if (canReach)
            {
                throw new NotImplementedException();
                //tar.PostMessage(Message.Types.PickUp, Player.Actor);

            }
            else
                NotificationArea.Write("Out of reach!");
        }


        public event EventHandler<EventArgs> Destroy;

        public void OnDestroy()
        {
            if (Destroy != null)
                Destroy(this, EventArgs.Empty);
        }

        static public bool CanReach(GameObject obj1, GameObject obj2)
        {
            //int height1 = obj1.GetInfo().Height, height2 = obj2.GetInfo().Height;

            float height1 = obj1["Physics"].GetProperty<int>("Height");
            PhysicsComponent phys;
            float height2 = obj2.TryGetComponent<PhysicsComponent>("Physics", out phys) ? (int)phys["Height"] : 1;
            //if (!obj2.TryGetComponent<PhysicsComponent>("Physics", out phys))
            //    height2 = 1;
            //else
            //    height2 = obj2["Physics"].GetProperty<int>("Height");

            Vector3 global1 = obj1.GetComponent<PositionComponent>("Position").GetProperty<Position>("Position").Global, global2 = obj2.GetComponent<PositionComponent>("Position").GetProperty<Position>("Position").Global;
            for (int i = 0; i < height1; i++)
                for (int j = 0; j < height2; j++)
                {
                    float dist = Vector3.Distance(global1 + new Vector3(0, 0, i), global2 + new Vector3(0, 0, j));
                    if (dist < 2)
                        return true;
                }

            return false;
        }

        //public override void Update()
        //{
        //    base.Update();
        //}

        public override Messages OnMouseLeft(bool held)
        {
            GameObjectSlot dragobj = DragDropManager.Instance.Source as GameObjectSlot;
            GameObject tar ;

            if(!Controller.Instance.Mouseover.TryGet<GameObject>(out tar))
                return Messages.Default;
            bool canReach;
            Position pos = Player.Actor.GetComponent<PositionComponent>("Position").GetProperty<Position>("Position");

            Position targetPos = tar.GetComponent<PositionComponent>("Position").GetProperty<Position>("Position");
            canReach = CanReach(Player.Actor, tar);
            
            if (canReach)
            {
                InventoryComponent inv = Player.Actor.GetComponent<InventoryComponent>("Inventory");
            GameObjectSlot hauling = inv.GetProperty<GameObjectSlot>("Holding");
            if (hauling.Object != null)
                {
                    Vector3 face = Controller.Instance.Mouseover.Face;
                    int rx, ry;
                    Camera cam = new Camera(Ingame.Instance.Camera.Width, Ingame.Instance.Camera.Height, rotation: (int)-Ingame.Instance.Camera.Rotation);
                    Coords.Rotate(cam, face.X, face.Y, out rx, out ry);
                    Vector3 tranformedFace = new Vector3(rx, ry, face.Z);
                    tar.PostMessage(new ObjectEventArgs(Message.Types.Give, Player.Actor, inv["Holding"], tranformedFace)); //Message.Types.Give
                //if(hauling.Object==null)

                    return Messages.Remove;
                }
                else
                {
                    if (Player.Actor["Cooldown"].GetProperty<float>("Cooldown") > 0)
                        return Messages.Default;
                    throw new NotImplementedException();
                    //tar.PostMessage(Message.Types.Attack, Player.Actor);
                }
            }
            else
            {
                NotificationArea.Write("Out of reach!");
                return Messages.Remove;
            }
            return Messages.Default;
        }

        public override ControlTool.Messages MouseRight(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Default;

            MaterialComponent material;
            //if (!Hauling.Object.TryGetComponent<MaterialComponent>("Material", out material))
            if (!Hauling.Object.Components.Values.Any(foo => foo is MaterialComponent))
                return Messages.Default;
            GameObject tar;
            if (!Controller.Instance.Mouseover.TryGet<GameObject>(out tar))
                return Messages.Default;

            Vector3 face = Controller.Instance.Mouseover.Face;
            int rx, ry;
            Camera cam = new Camera(Ingame.Instance.Camera.Width, Ingame.Instance.Camera.Height, rotation: (int)-Ingame.Instance.Camera.Rotation);
            Coords.Rotate(cam, face.X, face.Y, out rx, out ry);
            Vector3 tranformedFace =new Vector3(rx, ry, face.Z);
            tar.PostMessage(new ObjectEventArgs(Message.Types.Give, Player.Actor, new GameObjectSlot(GameObject.Create(GameObject.Types.Construction)), tranformedFace));
            Hauling.StackSize -= 1;
          //  Chunk.AddObject(GameObject.Create(GameObject.Types.Construction), 
            //Hauling.Object.HandleMessage(Player.Actor, Message.Types.Activate);
            return Messages.Remove;
        }

        //public override Messages MouseRight(bool held)
        //{
        //    if (Player.Actor.GetComponent<CooldownComponent>("Cooldown").GetProperty<float>("Cooldown") > 0)
        //        return Messages.Default;

        //    if (DragDropManager.Instance.Clear())
        //        return Messages.Default;

        //    GameObject tar;

        //    if (!Controller.Instance.Mouseover.TryGet<GameObject>(out tar))
        //        return Messages.Default;

        //    Position pos = Player.Actor.GetComponent<MovementComponent>("Position").GetProperty<Position>("Position");
        //    Position targetPos = tar.GetComponent<MovementComponent>("Position").GetProperty<Position>("Position");

            

        //    bool canReach = CanReach(Player.Actor, tar);
        //    if (canReach)
        //    {
        //        if (Player.Actor["Cooldown"].GetProperty<float>("Cooldown") > 0)
        //            return Messages.Remove;
        //        if (tar.HandleMessage(Player.Actor, Message.Types.Activate))
        //            return Messages.Remove;
        //    }

        //    return Messages.Default;
        //}

    }
}
