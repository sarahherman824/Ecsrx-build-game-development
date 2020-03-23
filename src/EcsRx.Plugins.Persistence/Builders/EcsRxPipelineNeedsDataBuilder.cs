using System.Collections.Generic;
using EcsRx.Infrastructure.Dependencies;
using EcsRx.Infrastructure.Extensions;
using LazyData.Serialization;
using Persistity.Endpoints;
using Persistity.Pipelines;
using Persistity.Pipelines.Builders;
using Persistity.Pipelines.Steps;
using Persistity.Pipelines.Steps.Types;
using Persistity.Processors;
using Persistity.Transformers;

namespace EcsRx.Plugins.Persistence.Builders
{
    public class EcsRxPipelineNeedsDataBuilder
    {
        private IDependencyContainer _container;
        private List<IPipelineStep> _steps;

        public EcsRxPipelineNeedsDataBuilder(IDependencyContainer container, List<IPipelineStep> steps)
        {
            _container = container;
            _steps = steps;
        }

        public EcsRxPipelineNeedsObjectBuilder DeserializeWith(IDeserializer deserializer)
        {
            _steps.Add(new DeserializeStep(deserializer));
            return new EcsRxPipelineNeedsObjectBuilder(_container, _steps);
        }
        
        public EcsRxPipelineNeedsObjectBuilder DeserializeWith<T>() where T : IDeserializer
        {
            _steps.Add(new DeserializeStep(_container.Resolve<T>()));
            return new EcsRxPipelineNeedsObjectBuilder(_container, _steps);
        }
        
        public EcsRxPipelineNeedsDataBuilder ProcessWith(IProcessor processor)
        {
            _steps.Add(new ProcessStep(processor));
            return this;
        }
        
        public EcsRxPipelineNeedsDataBuilder ProcessWith<T>() where T : IProcessor
        {
            _steps.Add(new ProcessStep(_container.Resolve<T>()));
            return this;
        }
        
        public EcsRxPipelineNeedsObjectBuilder ThenSendTo(ISendDataEndpoint endpoint)
        {
            _steps.Add(new SendEndpointStep(endpoint));
            return new EcsRxPipelineNeedsObjectBuilder(_container, _steps);
        }
        
        public EcsRxPipelineNeedsObjectBuilder ThenSendTo<T>() where T : ISendDataEndpoint
        {
            _steps.Add(new SendEndpointStep(_container.Resolve<T>()));
            return new EcsRxPipelineNeedsObjectBuilder(_container, _steps);
        }
        
        public IPipeline Build()
        { return new DefaultPipeline(_steps); }
    }
}