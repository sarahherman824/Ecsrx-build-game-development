using System;
using EcsRx.Components.Database;
using EcsRx.Components.Lookups;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Groups.Observable;
using EcsRx.MicroRx.Disposables;
using EcsRx.MicroRx.Extensions;
using EcsRx.Plugins.Batching.Builders;
using EcsRx.Plugins.Batching.Factories;
using EcsRx.Systems;
using EcsRx.Threading;

namespace EcsRx.Plugins.Batching.Systems
{
    public abstract class ManualBatchedSystem : IManualSystem 
    {
        public abstract IGroup Group { get; }
        
        public IComponentDatabase ComponentDatabase { get; }
        public IComponentTypeLookup ComponentTypeLookup { get; }
        public IThreadHandler ThreadHandler { get; }
        
        protected IObservableGroup ObservableGroup { get; set; }
        protected bool ShouldParallelize { get; private set; }
        protected IDisposable Subscriptions;

        protected ManualBatchedSystem(IComponentDatabase componentDatabase, IComponentTypeLookup componentTypeLookup, IThreadHandler threadHandler)
        {
            ComponentDatabase = componentDatabase;
            ComponentTypeLookup = componentTypeLookup;
            ThreadHandler = threadHandler;
        }

        protected abstract void RebuildBatch();
        protected abstract IObservable<bool> ReactWhen();

        protected virtual void BeforeProcessing(){}
        protected virtual void AfterProcessing(){}
        protected abstract void ProcessBatch();

        public virtual void StartSystem(IObservableGroup observableGroup)
        {
            ObservableGroup = observableGroup;
            ShouldParallelize = this.ShouldMutliThread();
            
            var subscriptions = new CompositeDisposable();
            ObservableGroup.OnEntityAdded.Subscribe(_ => RebuildBatch()).AddTo(subscriptions);
            ObservableGroup.OnEntityRemoved.Subscribe(_ => RebuildBatch()).AddTo(subscriptions);
            
            RebuildBatch();
            ReactWhen().Subscribe(_ => RunBatch()).AddTo(subscriptions);
            
            Subscriptions = subscriptions;
        }

        private void RunBatch()
        {
            BeforeProcessing();
            ProcessBatch();
            AfterProcessing();
        }

        public virtual void StopSystem(IObservableGroup observableGroup)
        { Subscriptions.Dispose(); }
    }
}