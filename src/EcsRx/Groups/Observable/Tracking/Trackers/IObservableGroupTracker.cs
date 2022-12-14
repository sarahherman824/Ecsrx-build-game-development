using System;
using EcsRx.Groups.Observable.Tracking.Events;

namespace EcsRx.Groups.Observable.Tracking.Trackers
{
    public interface IObservableGroupTracker : IDisposable
    {
        IObservable<EntityGroupStateChanged> GroupMatchingChanged { get; }
    }
}