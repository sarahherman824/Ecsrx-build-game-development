using System.Numerics;
using System.Threading.Tasks;
using EcsRx.Entities;
using EcsRx.Examples.ExampleApps.Playground.Batches;
using EcsRx.Examples.ExampleApps.Playground.Components;
using EcsRx.Plugins.Batching.Builders;
using EcsRx.Plugins.Batching.Descriptors;

namespace EcsRx.Examples.ExampleApps.Playground.StructBased
{
    public class Struct4BApplication : BasicLoopApplication
    {
        private Batch<StructComponent, StructComponent2>[] _componentBatch;
        
        protected override void SetupEntities()
        {
            _componentDatabase.PreAllocateComponents(StructComponent1TypeId, EntityCount);
            _componentDatabase.PreAllocateComponents(StructComponent2TypeId, EntityCount);
            
            base.SetupEntities();
            
            var batchBuilder = _batchBuilderFactory.Create<StructComponent, StructComponent2>();
            _componentBatch = batchBuilder.Build(_collection);
        }

        protected override string Description { get; } = "Uses auto batching to group components for quicker reads, but larger overhead in sync structs";

        protected override void SetupEntity(IEntity entity)
        {
            entity.AddComponent<StructComponent>(StructComponent1TypeId);
            entity.AddComponent<StructComponent2>(StructComponent2TypeId);
        }

        protected override void RunProcess()
        {
            Parallel.For(0, _componentBatch.Length, i =>
            {
                unsafe
                {
                    ref var batch = ref _componentBatch[i];
                    ref var basic = ref *batch.Component1;
                    ref var basic2 = ref *batch.Component2;
                    basic.Position += Vector3.One;
                    basic.Something += 10;
                    basic2.IsTrue = true;
                    basic2.Value += 10;
                }
            });
        }
    }
}