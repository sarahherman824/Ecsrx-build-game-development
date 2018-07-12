﻿using System;
using System.Diagnostics;
using System.Linq;
using EcsRx.Components;
using EcsRx.Examples.Application;
using EcsRx.Examples.ExampleApps.Performance.Helper;
using EcsRx.Extensions;

namespace EcsRx.Examples.ExampleApps.Performance
{
    public class GroupPerformanceApplication : EcsRxConsoleApplication
    {
        private IComponent[] _availableComponents;
        private readonly RandomGroupFactory _groupFactory = new RandomGroupFactory();
        private readonly Random _random = new Random();

        protected override void ApplicationStarted()
        {
            _availableComponents = _groupFactory.GetComponentTypes
                .Select(x => Activator.CreateInstance(x) as IComponent)
                .ToArray();
            
            var groups = _groupFactory.CreateTestGroups().ToArray();
            foreach (var group in groups)
            { EntityCollectionManager.GetObservableGroup(group); }

            var firstRun = ProcesEntities(10000);
            var secondRun = ProcesEntities(10000);
            var thirdRun = ProcesEntities(10000);

            Console.WriteLine($"Finished In: {(firstRun + secondRun + thirdRun).TotalSeconds}s");
            Console.WriteLine($"First Took: {firstRun.TotalSeconds}s");
            Console.WriteLine($"Second Took: {secondRun.TotalSeconds}s");
            Console.WriteLine($"Third Took: {thirdRun.TotalSeconds}s");
        }

        private TimeSpan ProcesEntities(int amount)
        {
            var defaultPool = EntityCollectionManager.GetCollection();
            EntityCollectionManager.Collections.ForEachRun(x => x.RemoveAllEntities());
            GC.Collect();
            
            var timer = Stopwatch.StartNew();

            for (var i = 0; i < amount; i++)
            {
                var entity = defaultPool.CreateEntity();
                entity.AddComponents(_availableComponents);
                entity.RemoveComponents(_availableComponents);
            }

            timer.Stop();
            return TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds);
        }
    }
}