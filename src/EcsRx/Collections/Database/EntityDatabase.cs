using System;
using System.Collections.Generic;
using SystemsRx.Extensions;
using SystemsRx.MicroRx.Disposables;
using SystemsRx.MicroRx.Extensions;
using SystemsRx.MicroRx.Subjects;
using EcsRx.Collections.Entity;
using EcsRx.Events.Collections;
using EcsRx.Lookups;

namespace EcsRx.Collections.Database
{
    public class EntityDatabase : IEntityDatabase
    {
        private readonly CollectionLookup _collections;
        private readonly IDictionary<int, IDisposable> _collectionSubscriptions;

        public IReadOnlyList<IEntityCollection> Collections => _collections;
        public IEntityCollection this[int id] => _collections[id];

        public IEntityCollectionFactory EntityCollectionFactory { get; }
        
        public IObservable<CollectionEntityEvent> EntityAdded => _onEntityAdded;
        public IObservable<CollectionEntityEvent> EntityRemoved => _onEntityRemoved;
        public IObservable<IEntityCollection> CollectionAdded => _onCollectionAdded;
        public IObservable<IEntityCollection> CollectionRemoved => _onCollectionRemoved;

        private readonly Subject<IEntityCollection> _onCollectionAdded;
        private readonly Subject<IEntityCollection> _onCollectionRemoved;
        private readonly Subject<CollectionEntityEvent> _onEntityAdded;
        private readonly Subject<CollectionEntityEvent> _onEntityRemoved;

        public EntityDatabase(IEntityCollectionFactory entityCollectionFactory)
        {
            EntityCollectionFactory = entityCollectionFactory;

            _collections = new CollectionLookup();
            _collectionSubscriptions = new Dictionary<int, IDisposable>();
            _onCollectionAdded = new Subject<IEntityCollection>();
            _onCollectionRemoved = new Subject<IEntityCollection>();
            _onEntityAdded = new Subject<CollectionEntityEvent>();
            _onEntityRemoved = new Subject<CollectionEntityEvent>();

            CreateCollection(EntityCollectionLookups.DefaultCollectionId);
        }

        public void SubscribeToCollection(IEntityCollection collection)
        {
            var collectionDisposable = new CompositeDisposable();   
            collection.EntityAdded.Subscribe(x => _onEntityAdded.OnNext(x)).AddTo(collectionDisposable);
            collection.EntityRemoved.Subscribe(x => _onEntityRemoved.OnNext(x)).AddTo(collectionDisposable);
            _collectionSubscriptions.Add(collection.Id, collectionDisposable);
        }

        public void UnsubscribeFromCollection(int id)
        { _collectionSubscriptions.RemoveAndDispose(id); }
        
        public IEntityCollection CreateCollection(int id)
        {
            var collection = EntityCollectionFactory.Create(id);
            AddCollection(collection);
            return collection;
        }
        
        public void AddCollection(IEntityCollection collection)
        {
            _collections.Add(collection);
            SubscribeToCollection(collection);

            _onCollectionAdded.OnNext(collection);
        }

        public IEntityCollection GetCollection(int id = EntityCollectionLookups.DefaultCollectionId)
        { return _collections.Contains(id) ? _collections[id] : null; }

        public void RemoveCollection(int id, bool disposeEntities = true)
        {
            if(!_collections.Contains(id)) { return; }

            var collection = _collections[id];
            _collections.Remove(id);
            
            UnsubscribeFromCollection(id);

            _onCollectionRemoved.OnNext(collection);
        }

        public void Dispose()
        {
            _onEntityAdded.Dispose();
            _onEntityRemoved.Dispose();
            _collectionSubscriptions.RemoveAndDisposeAll();
        }
    }
}