using System;
using System.Collections;
using System.Collections.Generic;
using EcsRx.Components;
using EcsRx.Pools;

namespace EcsRx.Collections
{
    public struct ComponentPool<T> : IComponentPool<T>
        where T : IComponent
    {
        public IndexPool IndexPool { get; }
        public T[] Components { get; private set; }
        public IReadOnlyList<T> ReadOnlyComponents => Components;
        
        public int Count { get; private set; }
        public int IndexesRemaining => IndexPool.AvailableIndexes.Count;

        public ComponentPool(int expansionSize) : this(expansionSize, expansionSize)
        {}
        
        public ComponentPool(int expansionSize, int initialSize)
        {
            Count = initialSize;
            IndexPool = new IndexPool(expansionSize, initialSize);
            Components = new T[expansionSize];
        }
        

        public int Allocate() => IndexPool.AllocateInstance();
        public void Release(int index) => IndexPool.ReleaseInstance(index);
        
        public void Set(int index, object value) => Components.SetValue(value, index);

        public void Expand(int amountToAdd)
        {
            var newCount = Components.Length + amountToAdd;
            var newEntries = new T[newCount];            
            Components.CopyTo(newEntries, 0);
            IndexPool.Expand(newCount-1);
            Components = newEntries;
            Count = newCount;
        }

        public IEnumerator GetEnumerator() => Components.GetEnumerator();
    }
}