﻿using System.Collections.Generic;
using EcsRx.Entities;
using EcsRx.Groups;
using EcsRx.Groups.Observable;

namespace EcsRx.Pools
{
    public interface IPoolManager
    {
        IEnumerable<IPool> Pools { get; }

        IEnumerable<IEntity> GetEntitiesFor(IGroup group, string poolName = null);
        IObservableGroup CreateObservableGroup(IGroup group, string poolName = null);

        IPool CreatePool(string name);
        IPool GetPool(string name = null);
        void RemovePool(string name);
    }
}