using System.Runtime.InteropServices;
using EcsRx.Components;

namespace EcsRx.Examples.ExampleApps.Playground.Components
{
    [StructLayout(LayoutKind.Sequential)]
    public struct StructComponent2 : IComponent
    {
        public bool IsTrue;
        public int Value;
    }
}