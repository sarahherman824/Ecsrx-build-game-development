using EcsRx.Components;

namespace EcsRx.Examples.ExampleApps.Playground.Components
{
    public struct StructComponent2 : IComponent
    {
        public bool IsTrue { get; set; }
        public int Value { get; set; }
    }
}