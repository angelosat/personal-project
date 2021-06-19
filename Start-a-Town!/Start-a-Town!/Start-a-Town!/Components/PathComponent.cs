using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.PathFinding;

namespace Start_a_Town_.Components
{
    class MovementComponent : Component
    {
        static public List<GameObject> Entities;

        //public override Component.Types Type { get { return Component.Types.Movement; } }

        public Path Path;
        float Speed;

        public MovementComponent(GameObject entity)
        {
            Speed = ((Start_a_Town_.WorldEntities.Actor)entity).Speed;
        }

        public override void Update(GameObject entity)
        {
            //Path Path = ((Start_a_Town_.WorldEntities.Actor)entity).CurrentTask.Path;
            if (Path == null)
                return;
            Path.Update(Speed);
            if (Path.Ended)
                Path = null;
            //PositionComponent position = (PositionComponent)entity.Components[Component.Types.Position];
            //position.Position.Global = Path.LastPosition.Global + Path.Transition * (Path.NextPosition.Global - Path.LastPosition.Global);
            entity.GetComponent<PositionComponent>("Position").Global = Path.LastPosition.Global + Path.Transition * (Path.NextPosition.Global - Path.LastPosition.Global);
            
        }

        //public override void Attach(IEntity entity)
        //{
        //    //TODO put speed on a metadata/attribute component on the entity
        //    Speed = ((Start_a_Town_.WorldEntities.Actor)entity).Speed;
        //    base.Attach(entity);
        //}
    }

    
}
