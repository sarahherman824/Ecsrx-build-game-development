using System.Collections.Generic;
using EcsRx.Components;
using EcsRx.Entities;
using EcsRx.Plugins.Batching.Descriptors;

namespace EcsRx.Plugins.Batching.Builders
{
    public interface IReferenceBatchBuilder<T1>
        where T1 : class, IComponent
    {
        ReferenceBatch<T1>[] Build(IReadOnlyList<IEntity> entities);
    }
    
    public interface IReferenceBatchBuilder<T1, T2>
        where T1 : class, IComponent
        where T2 : class, IComponent
    {
        ReferenceBatch<T1, T2>[] Build(IReadOnlyList<IEntity> entities);
    }
    
    public interface IReferenceBatchBuilder<T1, T2, T3>
        where T1 : class, IComponent
        where T2 : class, IComponent
        where T3 : class, IComponent
    {
        ReferenceBatch<T1, T2, T3>[] Build(IReadOnlyList<IEntity> entities);
    }
    
    public interface IReferenceBatchBuilder<T1, T2, T3, T4>
        where T1 : class, IComponent
        where T2 : class, IComponent
        where T3 : class, IComponent
        where T4 : class, IComponent
    {
        ReferenceBatch<T1, T2, T3, T4>[] Build(IReadOnlyList<IEntity> entities);
    }
    
    public interface IReferenceBatchBuilder<T1, T2, T3, T4, T5>
        where T1 : class, IComponent
        where T2 : class, IComponent
        where T3 : class, IComponent
        where T4 : class, IComponent
        where T5 : class, IComponent
    {
        ReferenceBatch<T1, T2, T3, T4, T5>[] Build(IReadOnlyList<IEntity> entities);
    }
    
    public interface IReferenceBatchBuilder<T1, T2, T3, T4, T5, T6>
        where T1 : class, IComponent
        where T2 : class, IComponent
        where T3 : class, IComponent
        where T4 : class, IComponent
        where T5 : class, IComponent
        where T6 : class, IComponent
    {
        ReferenceBatch<T1, T2, T3, T4, T5, T6>[] Build(IReadOnlyList<IEntity> entities);
    }
    
    
}