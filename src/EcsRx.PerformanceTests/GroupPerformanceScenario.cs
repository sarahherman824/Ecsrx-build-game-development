﻿using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Executor;
using EcsRx.Executor.Handlers;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Groups.Accessors;
using EcsRx.PerformanceTests.Helper;
using EcsRx.Pools;
using EcsRx.Reactive;

namespace EcsRx.PerformanceTests
{
    [Config(typeof(PerformanceConfig))]
    public class GroupPerformanceScenario
    {
        [Params(100, 10000)]
        public int Iterations;
        
        private IComponent[] _availableComponents;
        private readonly RandomGroupFactory _groupFactory = new RandomGroupFactory();
        private readonly Random _random = new Random();

        private IEventSystem _eventSystem;
        private IPoolManager _poolManager;
        private ISystemExecutor _systemExecutor;
        private IGroup[] _testGroups;
        private IPool _defaultPool;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _eventSystem = new EventSystem(new MessageBroker());
            
            var entityFactory = new DefaultEntityFactory(_eventSystem);
            var poolFactory = new DefaultPoolFactory(entityFactory, _eventSystem);
            var groupAccessorFactory = new DefaultObservableObservableGroupFactory(_eventSystem);
            _poolManager = new PoolManager(_eventSystem, poolFactory, groupAccessorFactory);
            
            var reactsToEntityHandler = new ReactToEntitySystemHandler(_poolManager);
            var reactsToGroupHandler = new ReactToGroupSystemHandler(_poolManager);
            var reactsToDataHandler = new ReactToDataSystemHandler(_poolManager);
            var manualSystemHandler = new ManualSystemHandler(_poolManager);
            var setupHandler = new SetupSystemHandler(_poolManager);
            _systemExecutor = new SystemExecutor(new IConventionalSystemHandler[]{ reactsToEntityHandler, reactsToGroupHandler, setupHandler, reactsToDataHandler, manualSystemHandler });

            _availableComponents = _groupFactory.GetComponentTypes
                .Select(x => Activator.CreateInstance(x) as IComponent)
                .ToArray();

            _testGroups = _groupFactory.CreateTestGroups().ToArray();

            foreach (var group in _testGroups)
            { _poolManager.CreateObservableGroup(group); }

            _defaultPool = _poolManager.GetPool();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            foreach (var pool in _poolManager.Pools)
            { pool.RemoveAllEntities(); }
        }

        [Benchmark]
        public void ProcessGroups()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var entity = _defaultPool.CreateEntity();
                entity.AddComponents(_availableComponents);
                entity.RemoveComponents(_availableComponents);
            }
        }
    }
}