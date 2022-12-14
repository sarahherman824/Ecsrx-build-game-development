using System.Collections.Generic;
using EcsRx.Collections.Entity;
using EcsRx.Components.Lookups;
using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Groups.Observable.Tracking.Trackers;

namespace EcsRx.Groups.Observable.Tracking
{
    public class GroupTrackerFactory : IGroupTrackerFactory
    {
        public IComponentTypeLookup ComponentTypeLookup { get; }

        public GroupTrackerFactory(IComponentTypeLookup componentTypeLookup)
        { ComponentTypeLookup = componentTypeLookup; }

        public ICollectionObservableGroupTracker TrackGroup(IGroup group, IEnumerable<IEntity> initialEntities, IEnumerable<INotifyingCollection> notifyingEntityComponentChanges)
        { return TrackGroup(ComponentTypeLookup.GetLookupGroupFor(group), initialEntities, notifyingEntityComponentChanges); }

        public ICollectionObservableGroupTracker TrackGroup(LookupGroup group, IEnumerable<IEntity> initialEntities, IEnumerable<INotifyingCollection> notifyingEntityComponentChanges)
        { return new CollectionObservableGroupTracker(group, initialEntities, notifyingEntityComponentChanges); }

        public IIndividualObservableGroupTracker TrackGroup(IGroup group, IEntity entity)
        { return TrackGroup(ComponentTypeLookup.GetLookupGroupFor(group), entity); }

        public IIndividualObservableGroupTracker TrackGroup(LookupGroup group, IEntity entity)
        { return new IndividualObservableGroupTracker(group, entity); }

        public IBatchObservableGroupTracker TrackGroup(IGroup group)
        { return TrackGroup(ComponentTypeLookup.GetLookupGroupFor(group)); }

        public IBatchObservableGroupTracker TrackGroup(LookupGroup group)
        { return new BatchObservableGroupTracker(group); }
    }
}