﻿using System.Collections.Generic;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Executor;
using EcsRx.Executor.Handlers;
using EcsRx.Groups.Accessors;
using EcsRx.Pools;
using EcsRx.Reactive;
using EcsRx.Tests.Models;
using EcsRx.Tests.Systems;
using Xunit;

namespace EcsRx.Tests
{
    public class SanityTests
    {
        private IPoolManager CreatePoolManager()
        {
            var messageBroker = new EventSystem(new MessageBroker());
            var entityFactory = new DefaultEntityFactory(messageBroker);
            var poolFactory = new DefaultPoolFactory(entityFactory, messageBroker);
            var groupAccessorFactory = new DefaultObservableObservableGroupFactory(messageBroker);
            return new PoolManager(messageBroker, poolFactory, groupAccessorFactory);
        }
        
        private SystemExecutor CreateExecutor(IPoolManager poolManager)
        {
            var reactsToEntityHandler = new ReactToEntitySystemHandler(poolManager);
            var reactsToGroupHandler = new ReactToGroupSystemHandler(poolManager);
            var reactsToDataHandler = new ReactToDataSystemHandler(poolManager);
            var manualSystemHandler = new ManualSystemHandler(poolManager);
            var setupHandler = new SetupSystemHandler(poolManager);

            var conventionalSystems = new List<IConventionalSystemHandler>
            {
                setupHandler,
                reactsToEntityHandler,
                reactsToGroupHandler,
                reactsToDataHandler,
                manualSystemHandler
            };
            
            return new SystemExecutor(conventionalSystems);
        }

        [Fact]
        public void should_execute_setup_for_matching_entities()
        {
            var poolManager = CreatePoolManager();
            var executor = CreateExecutor(poolManager);
            executor.AddSystem(new TestSetupSystem());

            var defaultPool = poolManager.GetPool();
            var entityOne = defaultPool.CreateEntity();
            var entityTwo = defaultPool.CreateEntity();

            entityOne.AddComponent(new TestComponentOne());
            entityTwo.AddComponent(new TestComponentTwo());

            Assert.Equal("woop", entityOne.GetComponent<TestComponentOne>().Data);
            Assert.Null(entityTwo.GetComponent<TestComponentTwo>().Data);
        }
    }
}