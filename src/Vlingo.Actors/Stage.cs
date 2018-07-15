// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.
using System;
using Vlingo.Actors.TestKit;

namespace Vlingo.Actors
{
    public class Stage
    {
        public World World { get; set; }

        public T ActorProxyFor<T>(Actor actor, IMailbox mailbox)
        {
            throw new System.NotImplementedException();
        }

        public T ActorFor<T>(Definition definition, Actor actor, ISupervisor supervisor, ILogger logger)
        {
            throw new NotImplementedException();
        }

        public T ActorFor<T>(Definition definition)
        {
            throw new NotImplementedException();
        }

        public Scheduler Scheduler { get; }

        public ActorProtocolActor<object>[] ActorFor<T>(
          Definition definition,
          Actor parent,
          Address maybeAddress,
          IMailbox maybeMailbox,
          ISupervisor maybeSupervisor,
          ILogger logger)
        {
            throw new NotImplementedException();
        }

        internal T ActorAs<T>(Actor parent)
        {
            throw new NotImplementedException();
        }

        internal void HandleFailureOf<T>(ISupervised supervised)
        {
            throw new NotImplementedException();
        }

        internal ISupervisor CommonSupervisorOr(ISupervisor defaultSupervisor)
        {
            throw new NotImplementedException();
        }

        public class ActorProtocolActor<T> { }

        internal TestActor<T> TestActorFor<T>(Definition definition)
        {
            throw new NotImplementedException();
        }

        internal Protocols TestActorFor(Definition definition, Type[] protocols)
        {
            throw new NotImplementedException();
        }
    }
}