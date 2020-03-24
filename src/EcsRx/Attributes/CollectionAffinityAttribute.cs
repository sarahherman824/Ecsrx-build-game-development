using System;
using EcsRx.Collections;
using EcsRx.Collections.Database;

namespace EcsRx.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectionAffinityAttribute : Attribute
    {
        public int[] CollectionIds { get; }

        public CollectionAffinityAttribute(int collectionId = EntityCollectionLookups.DefaultCollectionId)
        { CollectionIds = new []{collectionId}; }

        public CollectionAffinityAttribute(params int[] collectionIds)
        { CollectionIds = collectionIds; }
    }
}