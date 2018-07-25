﻿using EcsRx.Blueprints;
using EcsRx.Entities;
using EcsRx.Examples.ExampleApps.HealthExample.Components;
using EcsRx.Reactive;

namespace EcsRx.Examples.ExampleApps.HealthExample.Blueprints
{
    public class EnemyBlueprint : IBlueprint
    {
        public float Health { get; }

        public EnemyBlueprint(float health)
        {
            Health = health;
        }

        public void Apply(IEntity entity)
        {
            var healthComponent = new HealthComponent
            {
                Health = new ReactiveProperty<float>(Health),
                MaxHealth = Health
            };
            entity.AddComponent(healthComponent);
        }
    }
}