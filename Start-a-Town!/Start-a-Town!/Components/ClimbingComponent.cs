using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class ClimbingComponent : EntityComponent
    {
        static readonly float ClimbingSpeed = 0.1f;

        public override string ComponentName
        {
            get { return "Climbing"; }
        }

        public ClimbingComponent()
        {

        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.BlockCollision:
                    return true;

                    Vector3 blockGlobal = (Vector3)e.Parameters[0];
                    //BlockComponent.Blocks[blockGlobal.GetCell(e.Network.Map).Type].
                    //Cell cell = blockGlobal.GetCell(e.Network.Map);
                    Cell cell = e.Network.Map.GetCell(blockGlobal);

                    if (cell == null)
                        return true;

                    var blockobj = cell.Block.GetEntity();
                    if(blockobj == null)
                        return true;
                    if (!blockobj.HasComponent<LadderComponent>())
                        return true;
                    Vector3 vel = parent.Velocity;
                    parent.Velocity = new Vector3(vel.X, vel.Y, ClimbingSpeed);
                    return true;

                default:
                    return true;
            }
        }

        public override object Clone()
        {
            return new ClimbingComponent();
        }
    }
}
