using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EcsRx.Collections;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Extensions;
using EcsRx.Polyfills;

namespace EcsRx.Groups.Observable
{
    public class ObservableGroup : IObservableGroup, IDisposable
    {
        public readonly IDictionary<Guid, IEntity> CachedEntities;
        public readonly IList<IDisposable> Subscriptions;

        public IObservable<IEntity> OnEntityAdded => _onEntityAdded;
        public IObservable<IEntity> OnEntityRemoved => _onEntityRemoved;
        public IObservable<IEntity> OnEntityRemoving => _onEntityRemoving;

        private readonly Subject<IEntity> _onEntityAdded;
        private readonly Subject<IEntity> _onEntityRemoved;
        private readonly Subject<IEntity> _onEntityRemoving;
        
        public ObservableGroupToken Token { get; }
        public IEntityCollection ParentCollection { get; }
        public IEventSystem EventSystem { get; }

        public ObservableGroup(IEventSystem eventSystem, ObservableGroupToken token, IEnumerable<IEntity> initialEntities, IEntityCollection parentCollection = null)
        {
            Token = token;
            EventSystem = eventSystem;
            ParentCollection = parentCollection;

            _onEntityAdded = new Subject<IEntity>();
            _onEntityRemoved = new Subject<IEntity>();
            _onEntityRemoving = new Subject<IEntity>();

            CachedEntities = initialEntities.ToDictionary(x => x.Id, x => x);
            Subscriptions = new List<IDisposable>();

            MonitorEntityChanges();
        }

        private void MonitorEntityChanges()
        {
            EventSystem.Receive<EntityAddedEvent>()
                .Subscribe(OnEntityAddedToCollection)
                .AddTo(Subscriptions);

            EventSystem.Receive<EntityRemovedEvent>()
                .Subscribe(OnEntityRemovedFromCollection)
                .AddTo(Subscriptions);

            EventSystem.Receive<ComponentsAddedEvent>()
                .Subscribe(OnEntityComponentAdded)
                .AddTo(Subscriptions);

            EventSystem.Receive<ComponentsBeforeRemovedEvent>()
                .Subscribe(OnEntityBeforeComponentRemoved)
                .AddTo(Subscriptions);

            EventSystem.Receive<ComponentsRemovedEvent>()
                .Subscribe(OnEntityComponentRemoved)
                .AddTo(Subscriptions);
        }

        public bool shouldProcessEntity(IEntity entity)
        { return ParentCollection == null || ParentCollection.ContainsEntity(entity.Id); }

        public void OnEntityComponentRemoved(ComponentsRemovedEvent args)
        {
            if(!shouldProcessEntity(args.Entity)) { return; }
            
            if (CachedEntities.ContainsKey(args.Entity.Id))
            {
                if (!Token.Group.ContainsAnyRequiredComponents(args.Components)) 
                {return;}
                
                CachedEntities.Remove(args.Entity.Id);
                _onEntityRemoved.OnNext(args.Entity);
                return;
            }

            if (!Token.Group.Matches(args.Entity)) {return;}
            
            CachedEntities.Add(args.Entity.Id, args.Entity);
            _onEntityAdded.OnNext(args.Entity);
        }

        public void OnEntityBeforeComponentRemoved(ComponentsBeforeRemovedEvent args)
        {
            if(!shouldProcessEntity(args.Entity)) { return; }
            
            if (!CachedEntities.ContainsKey(args.Entity.Id)) { return; }
            
            if(Token.Group.ContainsAnyRequiredComponents(args.Components))
            { _onEntityRemoving.OnNext(args.Entity); }
        }

        public void OnEntityComponentAdded(ComponentsAddedEvent args)
        {
            if(!shouldProcessEntity(args.Entity)) { return; }
            
            if (CachedEntities.ContainsKey(args.Entity.Id))
            {
                if(!Token.Group.ContainsAnyExcludedComponents(args.Components))
                { return; }

                _onEntityRemoving.OnNext(args.Entity);
                CachedEntities.Remove(args.Entity.Id); 
                _onEntityRemoved.OnNext(args.Entity);
                return;
            }
            
            if (!Token.Group.Matches(args.Entity)) { return; }

            CachedEntities.Add(args.Entity.Id, args.Entity);
            _onEntityAdded.OnNext(args.Entity);
        }

        public void OnEntityAddedToCollection(EntityAddedEvent args)
        {
            if(!shouldProcessEntity(args.Entity)) { return; }

            // This is because you may have fired a blueprint before it is created
            if (CachedEntities.ContainsKey(args.Entity.Id)) { return; }
            if (!args.Entity.Components.Any()) { return; }
            if (!Token.Group.Matches(args.Entity)) { return; }
            
            CachedEntities.Add(args.Entity.Id, args.Entity);
            _onEntityAdded.OnNext(args.Entity);
        }
        
        public void OnEntityRemovedFromCollection(EntityRemovedEvent args)
        {
            if(!shouldProcessEntity(args.Entity)) { return; }
            
            if (!CachedEntities.ContainsKey(args.Entity.Id)) { return; }
            
            CachedEntities.Remove(args.Entity.Id); 
            _onEntityRemoved.OnNext(args.Entity);
        }
        
        public bool ContainsEntity(Guid id)
        { return CachedEntities.ContainsKey(id); }

        public void Dispose()
        {
            Subscriptions.DisposeAll();
            _onEntityAdded.Dispose();
            _onEntityRemoved.Dispose();
            _onEntityRemoving.Dispose();
        }

        public IEnumerator<IEntity> GetEnumerator()
        { return CachedEntities.Values.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator()
        { return GetEnumerator(); }
    }
}