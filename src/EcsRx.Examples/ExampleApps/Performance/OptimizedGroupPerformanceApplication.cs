﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EcsRx.Components;
using EcsRx.Examples.Application;
using EcsRx.Examples.ExampleApps.Performance.Components;
using EcsRx.Examples.ExampleApps.Performance.Components.Specific;
using EcsRx.Examples.ExampleApps.Performance.Helper;
using EcsRx.Examples.ExampleApps.Performance.Modules;
using EcsRx.Extensions;
using EcsRx.Infrastructure.Dependencies;
using EcsRx.Infrastructure.Extensions;

namespace EcsRx.Examples.ExampleApps.Performance
{
    public class OptimizedGroupPerformanceApplication : EcsRxConsoleApplication
    {
        private const int ProcessCount = 100000;
        
        private IComponent[] _availableComponents;
        private int[] _availableComponentTypeIds;
        private readonly RandomGroupFactory _groupFactory = new RandomGroupFactory();
        private readonly Random _random = new Random();

        protected override void LoadModules()
        { Container.LoadModule<OptimizedFrameworkModule>(); }

        protected override void BindSystems()
        {}

        protected override void ApplicationStarted()
        {
            _availableComponents = _groupFactory.GetComponentTypes
                .Select(x => Activator.CreateInstance(x) as IComponent)
                .ToArray();
         
            _availableComponentTypeIds = Enumerable.Range(0, _availableComponents.Length-1).ToArray();
            
            var groups = _groupFactory.CreateTestGroups().ToArray();
            foreach (var group in groups)
            { EntityCollectionManager.GetObservableGroup(group); }

            var firstRun = ProcessEntities(ProcessCount);
            var secondRun = ProcessEntities(ProcessCount);
            var thirdRun = ProcessEntities(ProcessCount);

            Console.WriteLine($"Finished In: {(firstRun + secondRun + thirdRun).TotalSeconds}s");
            Console.WriteLine($"First Took: {firstRun.TotalSeconds}s");
            Console.WriteLine($"Second Took: {secondRun.TotalSeconds}s");
            Console.WriteLine($"Third Took: {thirdRun.TotalSeconds}s");
        }

        private TimeSpan ProcessEntities(int amount)
        {
            var defaultPool = EntityCollectionManager.GetCollection();
            EntityCollectionManager.Collections.ForEachRun(x => x.RemoveAllEntities());
            GC.Collect();
            
            var timer = Stopwatch.StartNew();

            for (var i = 0; i < amount; i++)
            {
                var entity = defaultPool.CreateEntity();
                entity.AddComponents(_availableComponents);
                entity.RemoveComponents(_availableComponentTypeIds);
            }

            timer.Stop();
            return TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds);
        }
    }
}