namespace WorkflowEngine.Events
{
    public class EventDataBuilder<TEntity>
    {
        private readonly EntityEventData<TEntity> _eventData;

        public EventDataBuilder(EntityEventData<TEntity> eventData)
        {
            _eventData = eventData;
        }

        public EntityEventData<TEntity> Build(TEntity entity)
        {
            _eventData.Entity = entity;
            return _eventData;
        }
    }


}
