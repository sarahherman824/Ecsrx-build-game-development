﻿using System.Linq;
using EcsRx.Views.Pooling;
using EcsRx.Views.ViewHandlers;
using NSubstitute;
using Xunit;

namespace EcsRx.Tests.Views
{
    public class ViewPoolTests
    {
        [Fact]
        public void should_pre_allocate_views()
        {
            var mockViewHandler = Substitute.For<IViewHandler>();
            mockViewHandler.CreateView().Returns(null);

            var pool = new ViewPool(5, mockViewHandler);
            pool.PreAllocate(20);

            mockViewHandler.Received(20).CreateView();
            Assert.Equal(20, pool._pooledObjects.Count);
            Assert.All(pool._pooledObjects, x => Assert.False(x.IsInUse));
            Assert.All(pool._pooledObjects, x => Assert.Null(x.ViewObject));
        }

        [Fact]
        public void should_only_deallocate_unsued_views()
        {
            var mockViewHandler = Substitute.For<IViewHandler>();

            var pool = new ViewPool(5, mockViewHandler);
            for (var i = 0; i < 10; i++)
            {
                var viewObject = new ViewObjectContainer(null);

                if (i < 5)
                { viewObject.IsInUse = true; }

                pool._pooledObjects.Add(viewObject);
            }

            pool.DeAllocate(10);

            mockViewHandler.Received(5).DestroyView(Arg.Any<object>());
            Assert.Equal(5, pool._pooledObjects.Count);
            Assert.All(pool._pooledObjects, x => Assert.True(x.IsInUse));
        }

        [Fact]
        public void should_empty_pool()
        {
            var mockViewHandler = Substitute.For<IViewHandler>();

            var pool = new ViewPool(5, mockViewHandler);
            for (var i = 0; i < 10; i++)
            {
                var viewObject = new ViewObjectContainer(null);

                if (i < 5)
                { viewObject.IsInUse = true; }

                pool._pooledObjects.Add(viewObject);
            }

            pool.EmptyPool();

            mockViewHandler.Received(10).DestroyView(Arg.Any<object>());
            Assert.Empty(pool._pooledObjects);
        }

        [Fact]
        public void should_allocate_in_bulk_when_needing_more_instances()
        {
            var mockViewHandler = Substitute.For<IViewHandler>();
            var pool = new ViewPool(5, mockViewHandler);
            pool.AllocateInstance();

            mockViewHandler.Received(5).CreateView();
            Assert.Equal(5, pool._pooledObjects.Count);
            Assert.Equal(4, pool._pooledObjects.Count(x => x.IsInUse == false));
            Assert.Equal(1, pool._pooledObjects.Count(x => x.IsInUse));
        }

        [Fact]
        public void should_not_allocate_in_bulk_when_views_not_in_use()
        {
            var mockViewHandler = Substitute.For<IViewHandler>();
            var pool = new ViewPool(5, mockViewHandler);

            var viewObject = new ViewObjectContainer(null);
            pool._pooledObjects.Add(viewObject);

            pool.AllocateInstance();

            mockViewHandler.Received(0).CreateView();
            mockViewHandler.Received(1).SetActiveState(Arg.Any<object>(), true);
            Assert.Equal(1, pool._pooledObjects.Count);
            Assert.Equal(1, pool._pooledObjects.Count(x => x.IsInUse));
        }

        [Fact]
        public void should_not_destroy_on_deallocation()
        {
            var mockViewHandler = Substitute.For<IViewHandler>();
            var pool = new ViewPool(5, mockViewHandler);

            var actualView = new object();
            var viewObject = new ViewObjectContainer(actualView) { IsInUse = true };
            pool._pooledObjects.Add(viewObject);

            pool.ReleaseInstance(actualView);

            mockViewHandler.Received(0).DestroyView(actualView);
            mockViewHandler.Received(1).SetActiveState(actualView, false);
            Assert.Equal(1, pool._pooledObjects.Count);
            Assert.Equal(1, pool._pooledObjects.Count(x => x.IsInUse == false));
        }
    }
}