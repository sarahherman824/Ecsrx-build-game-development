using System;
using System.Collections.Generic;
using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Groups.Observable.Tracking.Events;
using EcsRx.Groups.Observable.Tracking.Types;
using SystemsRx.Extensions;
using SystemsRx.MicroRx.Disposables;
using SystemsRx.MicroRx.Extensions;
using SystemsRx.MicroRx.Subjects;

namespace EcsRx.Groups.Observable.Tracking
{
    public class ObservableGroupBatchTracker : IObservableGroupBatchTracker
    {
        private readonly Dictionary<int, IDisposable> _entitySubscriptions;
        
        public Dictionary<int, GroupMatchingType> EntityIdMatchTypes { get; }
        public LookupGroup LookupGroup { get; }
        public Subject<GroupStateChanged> OnGroupMatchingChanged { get; }

        public IObservable<GroupStateChanged> GroupMatchingChanged => OnGroupMatchingChanged;

        public ObservableGroupBatchTracker(LookupGroup lookupGroup)
        {
            _entitySubscriptions = new Dictionary<int, IDisposable>();
            EntityIdMatchTypes = new Dictionary<int, GroupMatchingType>();
            LookupGroup = lookupGroup;
            OnGroupMatchingChanged = new Subject<GroupStateChanged>();
        }
        
        public void StartTrackingEntity(IEntity entity)
        {
            var entitySubs = new CompositeDisposable();
            entity.ComponentsAdded.Subscribe(x => OnEntityComponentAdded(x, entity, LookupGroup)).AddTo(entitySubs);
            entity.ComponentsRemoving.Subscribe(x => OnEntityComponentRemoving(x, entity, LookupGroup)).AddTo(entitySubs);
            entity.ComponentsRemoved.Subscribe(x => OnEntityComponentRemoved(x, entity, LookupGroup)).AddTo(entitySubs);
            
            _entitySubscriptions.Add(entity.Id, entitySubs);
            EntityIdMatchTypes.Add(entity.Id, LookupGroup.CalculateMatchingType(entity));
        }

        public void StopTrackingEntity(IEntity entity)
        {
            if (!EntityIdMatchTypes.ContainsKey(entity.Id)) {return;}

            _entitySubscriptions[entity.Id].Dispose();
            _entitySubscriptions.Remove(entity.Id);
            EntityIdMatchTypes.Remove(entity.Id);
        }
        


        public void OnEntityComponentAdded(int[] componentsAdded, IEntity entity, LookupGroup group)
        {
            var entityMatchType = EntityIdMatchTypes[entity.Id];
            if (entityMatchType == GroupMatchingType.NoMatchesWithExcludes || 
                entityMatchType == GroupMatchingType.MatchesWithExcludes)
            { return; }
            
            if(entityMatchType == GroupMatchingType.MatchesNoExcludes)
            {
                if (group.ContainsAnyExcludedComponents(componentsAdded))
                {
                    EntityIdMatchTypes[entity.Id] = GroupMatchingType.MatchesWithExcludes;
                    OnGroupMatchingChanged.OnNext(new GroupStateChanged(entity, GroupActionType.LeavingGroup));
                    OnGroupMatchingChanged.OnNext(new GroupStateChanged(entity, GroupActionType.LeftGroup));
                }
                return;
            }

            if (group.ContainsAllRequiredComponents(entity))
            {
                if (group.ContainsAnyExcludedComponents(entity))
                {
                    EntityIdMatchTypes[entity.Id] = GroupMatchingType.MatchesWithExcludes;
                    return;
                }

                EntityIdMatchTypes[entity.Id] = GroupMatchingType.MatchesNoExcludes;
                OnGroupMatchingChanged.OnNext(new GroupStateChanged(entity, GroupActionType.JoinedGroup));
            }
        }
        
        public void OnEntityComponentRemoving(int[] componentsRemoving, IEntity entity, LookupGroup group)
        {
            var entityMatchType = EntityIdMatchTypes[entity.Id];
            if (entityMatchType == GroupMatchingType.NoMatchesNoExcludes)
            { return; }

            if (entityMatchType == GroupMatchingType.MatchesNoExcludes)
            {
                if(group.ContainsAnyRequiredComponents(componentsRemoving))
                { OnGroupMatchingChanged.OnNext(new GroupStateChanged(entity, GroupActionType.LeavingGroup)); }
            }
        }
        
        public void OnEntityComponentRemoved(int[] componentsAdded, IEntity entity, LookupGroup group)
        {
            var entityMatchType = EntityIdMatchTypes[entity.Id];
            if (entityMatchType == GroupMatchingType.NoMatchesNoExcludes)
            { return; }

            var containsAllComponents = group.ContainsAllRequiredComponents(entity);
            if (entityMatchType == GroupMatchingType.MatchesNoExcludes)
            {
                if(containsAllComponents)
                { return; }

                EntityIdMatchTypes[entity.Id] = GroupMatchingType.NoMatchesNoExcludes;
                OnGroupMatchingChanged.OnNext(new GroupStateChanged(entity, GroupActionType.LeftGroup));
            }

            var containsAnyExcluded = group.ContainsAnyExcludedComponents(entity);
            
            if (entityMatchType == GroupMatchingType.NoMatchesWithExcludes && !containsAnyExcluded)
            {
                EntityIdMatchTypes[entity.Id] = GroupMatchingType.NoMatchesNoExcludes;
                return;
            }
            
            if (entityMatchType == GroupMatchingType.MatchesWithExcludes && containsAllComponents && !containsAnyExcluded)
            {
                EntityIdMatchTypes[entity.Id] = GroupMatchingType.MatchesNoExcludes;
                OnGroupMatchingChanged.OnNext(new GroupStateChanged(entity, GroupActionType.JoinedGroup));
                return;
            }
        }

        public void Dispose()
        {
            OnGroupMatchingChanged?.Dispose();
            _entitySubscriptions.DisposeAll();
        }
    }
}