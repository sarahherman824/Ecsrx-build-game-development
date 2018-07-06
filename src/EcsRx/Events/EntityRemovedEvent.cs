using EcsRx.Collections;
using EcsRx.Entities;

namespace EcsRx.Events
{
    public class EntityRemovedEvent
    {
        public IEntity Entity { get; }
        public IEntityCollection EntityCollection { get; }

        public EntityRemovedEvent(IEntity entity, IEntityCollection entityCollection)
        {
            Entity = entity;
            EntityCollection = entityCollection;
        }
    }
}