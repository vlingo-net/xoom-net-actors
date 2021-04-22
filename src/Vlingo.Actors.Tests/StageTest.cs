// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vlingo.Actors.Plugin.Mailbox.TestKit;
using Vlingo.Xoom.Common;
using Xunit;
using TestResults = Vlingo.Actors.Tests.WorldTest.TestResults;
using SimpleActor = Vlingo.Actors.Tests.WorldTest.SimpleActor;
using Vlingo.Actors.TestKit;

namespace Vlingo.Actors.Tests
{
    public class StageTest : ActorsTest
    {
        [Fact]
        public void TestActorForDefinitionAndProtocol()
        {
            var test = World.Stage.ActorFor<INoProtocol>(typeof(TestInterfaceActor));

            Assert.NotNull(test);
            Assert.NotNull(TestInterfaceActor.Instance.Value);
            Assert.Equal(World.DefaultParent, TestInterfaceActor.Instance.Value.LifeCycle.Environment.Parent);
        }

        [Fact]
        public void TestActorForNoDefinitionAndProtocol()
        {
            var testResults = new TestResults(1);
            var simple = World.Stage.ActorFor<ISimpleWorld>(() => new SimpleActor(testResults));
            simple.SimpleSay();

            Assert.True(testResults.Invoked);

            var test = World.Stage.ActorFor<INoProtocol>(typeof(TestInterfaceActor));
            Assert.NotNull(test);
            Assert.NotNull(TestInterfaceActor.Instance.Value);
            Assert.Equal(World.DefaultParent, TestInterfaceActor.Instance.Value.LifeCycle.Environment.Parent);
        }

        [Fact]
        public void TestActorForAll()
        {
            World.ActorFor<INoProtocol>(typeof(ParentInterfaceActor));

            var definition = Definition.Has<TestInterfaceActor>(
                Definition.NoParameters,
                ParentInterfaceActor.Instance.Value,
                TestMailbox.Name,
                "test-actor");

            var test = World.Stage.ActorFor<INoProtocol>(definition);

            Assert.NotNull(test);
            Assert.NotNull(TestInterfaceActor.Instance.Value);
        }

        [Fact]
        public void TestDirectoryScan()
        {
            var address1 = World.AddressFactory.UniqueWith("test-actor1");
            var address2 = World.AddressFactory.UniqueWith("test-actor2");
            var address3 = World.AddressFactory.UniqueWith("test-actor3");
            var address4 = World.AddressFactory.UniqueWith("test-actor4");
            var address5 = World.AddressFactory.UniqueWith("test-actor5");

            var address6 = World.AddressFactory.UniqueWith("test-actor6");
            var address7 = World.AddressFactory.UniqueWith("test-actor7");

            World.Stage.Directory.Register(address1, new TestInterfaceActor());
            World.Stage.Directory.Register(address2, new TestInterfaceActor());
            World.Stage.Directory.Register(address3, new TestInterfaceActor());
            World.Stage.Directory.Register(address4, new TestInterfaceActor());
            World.Stage.Directory.Register(address5, new TestInterfaceActor());

            var scanResults = new ScanResult(7);

            Action<INoProtocol> afterConsumer = actor =>
            {
                Assert.NotNull(actor);
                scanResults.Found();
            };

            World.Stage.ActorOf<INoProtocol>(address5).AndThenConsume(afterConsumer);
            World.Stage.ActorOf<INoProtocol>(address4).AndThenConsume(afterConsumer);
            World.Stage.ActorOf<INoProtocol>(address3).AndThenConsume(afterConsumer);
            World.Stage.ActorOf<INoProtocol>(address2).AndThenConsume(afterConsumer);
            World.Stage.ActorOf<INoProtocol>(address1).AndThenConsume(afterConsumer);

            World.Stage
                .MaybeActorOf<INoProtocol>(address6)
                .AndThenConsume(maybe =>
                {
                    if (maybe.IsPresent)
                    {
                        scanResults.Found();
                    }
                    else
                    {
                        scanResults.NotFound();
                    }
                });
            World.Stage
                .MaybeActorOf<INoProtocol>(address7)
                .AndThenConsume(maybe =>
                {
                    if (maybe.IsPresent)
                    {
                        scanResults.Found();
                    }
                    else
                    {
                        scanResults.NotFound();
                    }
                });

            Assert.Equal(5, scanResults.FoundCount);
            Assert.Equal(2, scanResults.NotFoundCount);
        }

        [Fact]
        public void TestDirectoryScanMaybeActor()
        {
            var address1 = World.AddressFactory.UniqueWith("test-actor1");
            var address2 = World.AddressFactory.UniqueWith("test-actor2");
            var address3 = World.AddressFactory.UniqueWith("test-actor3");
            var address4 = World.AddressFactory.UniqueWith("test-actor4");
            var address5 = World.AddressFactory.UniqueWith("test-actor5");

            var address6 = World.AddressFactory.UniqueWith("test-actor6");
            var address7 = World.AddressFactory.UniqueWith("test-actor7");

            World.Stage.Directory.Register(address1, new TestInterfaceActor());
            World.Stage.Directory.Register(address2, new TestInterfaceActor());
            World.Stage.Directory.Register(address3, new TestInterfaceActor());
            World.Stage.Directory.Register(address4, new TestInterfaceActor());
            World.Stage.Directory.Register(address5, new TestInterfaceActor());

            var scanResults = new ScanResult(7);

            Action<Optional<INoProtocol>> afterConsumer = maybe =>
            {
                Assert.True(maybe.IsPresent);
                scanResults.Found();
            };

            World.Stage.MaybeActorOf<INoProtocol>(address5).AndThenConsume(afterConsumer);
            World.Stage.MaybeActorOf<INoProtocol>(address4).AndThenConsume(afterConsumer);
            World.Stage.MaybeActorOf<INoProtocol>(address3).AndThenConsume(afterConsumer);
            World.Stage.MaybeActorOf<INoProtocol>(address2).AndThenConsume(afterConsumer);
            World.Stage.MaybeActorOf<INoProtocol>(address1).AndThenConsume(afterConsumer);

            World.Stage.MaybeActorOf<INoProtocol>(address6)
                .AndThen(maybe =>
                {
                    Assert.False(maybe.IsPresent);
                    scanResults.NotFound();
                    return maybe;
                });

            World.Stage.MaybeActorOf<INoProtocol>(address7)
                .AndThen(maybe =>
                {
                    Assert.False(maybe.IsPresent);
                    scanResults.NotFound();
                    return maybe;
                });

            Assert.Equal(5, scanResults.FoundCount);
            Assert.Equal(2, scanResults.NotFoundCount);
        }

        [Fact]
        public void TestThatProtocolIsInterface()
        {
            World.Stage.ActorFor<INoProtocol>(typeof(ParentInterfaceActor));
        }

        [Fact]
        public void TestThatProtocolIsNotInterface()
        {
            Assert.Throws<ArgumentException>(() => World.Stage.ActorFor<ParentInterfaceActor>(typeof(ParentInterfaceActor)));
        }

        [Fact]
        public void TestThatProtocolsAreInterfaces()
        {
            World.Stage.ActorFor(new[] { typeof(INoProtocol), typeof(INoProtocol) }, typeof(ParentInterfaceActor));
        }

        [Fact]
        public void TestThatProtocolsAreNotInterfaces()
        {
            Assert.Throws<ArgumentException>(()
                => World.Stage.ActorFor(new[] { typeof(INoProtocol), typeof(ParentInterfaceActor) }, typeof(ParentInterfaceActor)));
        }
        
        [Fact]
        public void TestSingleThreadRawLookupOrStartFindsActorPreviouslyStartedWithActorFor()
        {
            var address = World.AddressFactory.Unique();
            var definition = Definition.Has(() => new ParentInterfaceActor());
            World.Stage.ActorFor<INoProtocol>(definition, address);
            var existing = World.Stage.RawLookupOrStart(definition, address);
            Assert.Same(address, existing.Address);
        }
        
        [Fact]
        public void TestSingleThreadRawLookupOrStartFindsActorPreviouslyStartedWithRawLookupOrStart()
        {
            var address = World.AddressFactory.Unique();
            var definition = Definition.Has(() => new ParentInterfaceActor());
            var started = World.Stage.RawLookupOrStart(definition, address);
            var found = World.Stage.RawLookupOrStart(definition, address);
            Assert.Same(started, found);
        }

        [Fact]
        public void TestSingleThreadActorLookupOrStartFindsActorPreviouslyStartedWithActorFor()
        {
            var address = World.AddressFactory.Unique();
            var definition = Definition.Has(() => new ParentInterfaceActor());
            World.Stage.ActorFor<INoProtocol>(definition, address);
            var existing = World.Stage.ActorLookupOrStart(definition, address);
            Assert.Same(address, existing.Address);
        }
        
        [Fact]
        public void TestSingleThreadActorLookupOrStartFindsActorPreviouslyStartedWithActorLookupOrStart()
        {
            var address = World.AddressFactory.Unique();
            var definition = Definition.Has(() => new ParentInterfaceActor());
            var started = World.Stage.ActorLookupOrStart(definition, address);
            var found = World.Stage.ActorLookupOrStart(definition, address);
            Assert.Same(started, found);
        }

        [Fact]
        public void TestSingleThreadLookupOrStartFindsActorPreviouslyStartedWithActorFor()
        {
            var address = World.AddressFactory.Unique();
            var definition = Definition.Has(() => new ParentInterfaceActor());
            World.Stage.ActorFor<INoProtocol>(definition, address);
            Assert.NotNull(World.Stage.LookupOrStart<INoProtocol>(definition, address));
        }
        
        [Fact]
        public void TestSingleThreadLookupOrStartFindsActorPreviouslyStartedWithLookupOrStart()
        {
            var address = World.AddressFactory.Unique();
            var definition = Definition.Has(() => new ParentInterfaceActor());
            Assert.NotNull(World.Stage.LookupOrStart<INoProtocol>(definition, address));
            Assert.NotNull(World.Stage.LookupOrStart<INoProtocol>(definition, address));
        }
        
        [Fact]
        public async Task TestMultiThreadRawLookupOrStartFindsActorPreviouslyStartedWIthRawLookupOrStart()
        {
            var size = 1000;

            var addresses = Enumerable.Range(0, size)
                .Select(_ => World.AddressFactory.Unique())
                .ToList();

            var definition = Definition.Has(() => new ParentInterfaceActor());

            await MultithreadedLookupOrStartTest(index =>
                Task.Factory.StartNew(() => World.Stage.RawLookupOrStart(definition, addresses[index])), size);
        }
        
        [Fact]
        public async Task TestMultiThreadActorLookupOrStartFindsActorPreviouslyStartedWIthActorLookupOrStart()
        {
            var size = 1000;

            var addresses = Enumerable.Range(0, size)
                .Select(_ => World.AddressFactory.Unique())
                .ToList();

            var definition = Definition.Has(() => new ParentInterfaceActor());

            await MultithreadedLookupOrStartTest(index =>
                Task.Factory.StartNew(() => World.Stage.ActorLookupOrStart(definition, addresses[index])), size);
        }
        
        private async Task MultithreadedLookupOrStartTest(Func<int, Task<Actor>> work, int size)
        {
            var tasks = Enumerable.Range(0, size)
                .SelectMany(i => new List<int> {i, i})
                .Select(work);

            await Task.WhenAll(tasks).ContinueWith(previousTask =>
            {
                var actors = previousTask.Result;
                var results = new List<Actor>(actors.Length);
                foreach (var actor in actors)
                {
                    if (results.Any() && results.Count % 2 != 0)
                    {
                        var expected = results[^1];
                        Assert.Same(expected.Address, actor.Address);
                    }
                    results.Add(actor);
                }
            });
        }

        private class ParentInterfaceActor : Actor, INoProtocol
        {
            public static ThreadLocal<ParentInterfaceActor> Instance = new ThreadLocal<ParentInterfaceActor>();
            public ParentInterfaceActor() => Instance.Value = this;
        }

        private class TestInterfaceActor : Actor, INoProtocol
        {
            public static ThreadLocal<TestInterfaceActor> Instance = new ThreadLocal<TestInterfaceActor>();
            public TestInterfaceActor() => Instance.Value = this;
        }

        private class ScanResult
        {
            private readonly AccessSafely _safely;

            public ScanResult(int times)
            {
                var foundCount = new AtomicInteger(0);
                var notFoundCount = new AtomicInteger(0);
                _safely = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith<int>("foundCount", _ => foundCount.IncrementAndGet())
                    .ReadingWith("foundCount", foundCount.Get)
                    .WritingWith<int>("notFoundCount", _ => notFoundCount.IncrementAndGet())
                    .ReadingWith("notFoundCount", notFoundCount.Get);
            }

            public int FoundCount => _safely.ReadFrom<int>("foundCount");

            public int NotFoundCount => _safely.ReadFrom<int>("notFoundCount");

            public void Found() => _safely.WriteUsing("foundCount", 1);

            public void NotFound() => _safely.WriteUsing("notFoundCount", 1);
        }
    }
}
