﻿using System;
using System.Collections.Generic;
using System.Linq;
using EcsRx.Examples.GroupPerformance.Components;
using EcsRx.Examples.GroupPerformance.Extensions;
using EcsRx.Groups;

namespace EcsRx.Examples.GroupPerformance.Helper
{
    public class RandomGroupFactory
    {
        public IEnumerable<Type> GetComponenTypes => _componenTypes;
        
        private Random _random = new Random();
        private List<Type> _componenTypes;

        public RandomGroupFactory()
        {
            PopulateComponentList();
        }
        
        private void PopulateComponentList()
        {
            _componenTypes = new List<Type>
            {
                typeof(Component1),
                typeof(Component2),
                typeof(Component3),
                typeof(Component4),
                typeof(Component5),
                typeof(Component6),
                typeof(Component7),
                typeof(Component8),
                typeof(Component9),
                typeof(Component10),
                typeof(Component11),
                typeof(Component12),
                typeof(Component13),
                typeof(Component14),
                typeof(Component15),
                typeof(Component16),
                typeof(Component17),
                typeof(Component18),
                typeof(Component19),
                typeof(Component20)
            };
        }

        public IGroup CreateRandomGroup()
        {
            var randomSize = _random.Next(_componenTypes.Count / 2);
            return new Group(_componenTypes.Random(randomSize).ToArray());
        }

        public IEnumerable<IGroup> CreateTestGroups(int cycles = 5)
        {
            for (var i = 1; i < cycles; i++)
            {
                for (var j = i; j < _componenTypes.Count; j+= i)
                {
                    yield return new Group(_componenTypes.Skip(j-i).Take(j).ToArray());
                }
            }
            /*
            foreach (var component in _componenTypes)
            {
                yield return new Group(component);
            }*/
        }
    }
}