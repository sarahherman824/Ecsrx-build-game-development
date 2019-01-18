﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using EcsRx.Collections;
using EcsRx.Components;
using EcsRx.Components.Database;
using EcsRx.Entities;
using EcsRx.Events;
using NSubstitute;
using NSubstitute.Extensions;
using Xunit;

namespace EcsRx.Tests.Framework
{
    public class EntityCollectionTests
    {
        [Fact]
        public void should_create_new_entity_and_raise_event()
        {
            var mockEntityFactory = Substitute.For<IEntityFactory>();
            var mockEntity = Substitute.For<IEntity>();
            mockEntityFactory.Create(null).Returns(mockEntity);
       
            var entityCollection = new EntityCollection(1, mockEntityFactory);
            
            var wasCalled = false;
            entityCollection.EntityAdded.Subscribe(x => wasCalled = true);
            
            var entity = entityCollection.CreateEntity();
            
            Assert.Contains(mockEntity, entityCollection.EntityLookup);
            Assert.Equal(mockEntity, entity);
            Assert.True(wasCalled);
        }

        [Fact]
        public void should_raise_events_and_remove_components_when_removing_entity()
        {
            var mockEntityFactory = Substitute.For<IEntityFactory>();
            var mockEntity = Substitute.For<IEntity>();
            mockEntity.Id.Returns(1);
            
            mockEntityFactory.Create(null).Returns(mockEntity);
           
            var entityCollection = new EntityCollection(1, mockEntityFactory);
            
            var wasCalled = false;
            entityCollection.EntityRemoved.Subscribe(x => wasCalled = true);
            
            entityCollection.CreateEntity();
            entityCollection.RemoveEntity(mockEntity.Id);

            Assert.True(wasCalled);
            Assert.DoesNotContain(mockEntity, entityCollection);
        }
    }
}