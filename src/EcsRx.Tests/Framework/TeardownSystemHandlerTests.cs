using System;
using System.Reactive.Subjects;
using EcsRx.Collections;
using EcsRx.Entities;
using EcsRx.Executor.Handlers;
using EcsRx.Groups;
using EcsRx.Groups.Observable;
using EcsRx.Systems;
using NSubstitute;
using Xunit;

namespace EcsRx.Tests.Framework
{
    public class TeardownSystemHandlerTests
    {
        [Fact]
        public void should_correctly_handle_systems()
        {
            var mockCollectionManager = Substitute.For<IEntityCollectionManager>();
            var teardownSystemHandler = new TeardownSystemHandler(mockCollectionManager);
            
            var fakeMatchingSystem = Substitute.For<ITeardownSystem>();
            var fakeNonMatchingSystem1 = Substitute.For<IReactToEntitySystem>();
            var fakeNonMatchingSystem2 = Substitute.For<ISystem>();
            
            Assert.True(teardownSystemHandler.CanHandleSystem(fakeMatchingSystem));
            Assert.False(teardownSystemHandler.CanHandleSystem(fakeNonMatchingSystem1));
            Assert.False(teardownSystemHandler.CanHandleSystem(fakeNonMatchingSystem2));
        }
        
        [Fact]
        public void should_teardown_entity_when_removed()
        {
            var fakeEntity1 = Substitute.For<IEntity>();
            fakeEntity1.Id.Returns(Guid.NewGuid());
            var fakeEntities = new IEntity[] {};

            var removeSubject = new Subject<IEntity>();
            var mockObservableGroup = Substitute.For<IObservableGroup>();
            mockObservableGroup.OnEntityAdded.Returns(new Subject<IEntity>());
            mockObservableGroup.OnEntityRemoved.Returns(removeSubject);
            mockObservableGroup.Entities.Returns(fakeEntities);
            
            var mockCollectionManager = Substitute.For<IEntityCollectionManager>();

            var fakeGroup = Substitute.For<IGroup>();
            fakeGroup.MatchesComponents.Returns(new Type[0]);
            mockCollectionManager.CreateObservableGroup(Arg.Is(fakeGroup)).Returns(mockObservableGroup);
            
            var mockSystem = Substitute.For<ITeardownSystem>();
            mockSystem.TargetGroup.Returns(fakeGroup);

            var systemHandler = new TeardownSystemHandler(mockCollectionManager);
            systemHandler.SetupSystem(mockSystem);
            
            removeSubject.OnNext(fakeEntity1);
            
            mockSystem.Received(1).Teardown(Arg.Is(fakeEntity1));
            Assert.Equal(1, systemHandler.SystemSubscriptions.Count);
            Assert.NotNull(systemHandler.SystemSubscriptions[mockSystem]);
        }
    }
    
    public class ManualSystemHandlerTests
    {
        [Fact]
        public void should_correctly_handle_systems()
        {
            var mockCollectionManager = Substitute.For<IEntityCollectionManager>();
            var teardownSystemHandler = new ManualSystemHandler(mockCollectionManager);
            
            var fakeMatchingSystem = Substitute.For<IManualSystem>();
            var fakeNonMatchingSystem1 = Substitute.For<IReactToEntitySystem>();
            var fakeNonMatchingSystem2 = Substitute.For<ISystem>();
            
            Assert.True(teardownSystemHandler.CanHandleSystem(fakeMatchingSystem));
            Assert.False(teardownSystemHandler.CanHandleSystem(fakeNonMatchingSystem1));
            Assert.False(teardownSystemHandler.CanHandleSystem(fakeNonMatchingSystem2));
        }
        
        [Fact]
        public void should_start_system_when_added_to_handler()
        {
            var mockObservableGroup = Substitute.For<IObservableGroup>();
            var mockCollectionManager = Substitute.For<IEntityCollectionManager>();

            mockCollectionManager.CreateObservableGroup(Arg.Any<IGroup>()).Returns(mockObservableGroup);
            var mockSystem = Substitute.For<IManualSystem>();

            var systemHandler = new ManualSystemHandler(mockCollectionManager);
            systemHandler.SetupSystem(mockSystem);
            
            mockSystem.Received(1).StartSystem(Arg.Is(mockObservableGroup));
        }
        
        [Fact]
        public void should_stop_system_when_added_to_handler()
        {
            var mockObservableGroup = Substitute.For<IObservableGroup>();
            var mockCollectionManager = Substitute.For<IEntityCollectionManager>();

            mockCollectionManager.CreateObservableGroup(Arg.Any<IGroup>()).Returns(mockObservableGroup);
            var mockSystem = Substitute.For<IManualSystem>();

            var systemHandler = new ManualSystemHandler(mockCollectionManager);
            systemHandler.DestroySystem(mockSystem);
            
            mockSystem.Received(1).StopSystem(Arg.Is(mockObservableGroup));
        }
    }
}