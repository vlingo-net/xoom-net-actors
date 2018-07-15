// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.
namespace Vlingo.Actors
{
    public class Protocols
    {
        private readonly object[] protocolActors;

        public T Get<T>(int index)
        {
            return (T)protocolActors[index];
        }

        protected Protocols(object[] protocolActors)
        {
            this.protocolActors = protocolActors;
        }

        public static Two<A, B> Two<A, B>(Protocols protocols)
        {
            return new Two<A, B>(protocols);
        }

        public static  Three<A, B, C> Three<A, B, C>(Protocols protocols)
        {
            return new Three<A, B, C>(protocols);
        }

        public static Four<A, B, C, D> Four<A, B, C, D>(Protocols protocols)
        {
            return new Four<A, B, C, D>(protocols);
        }

        public static Five<A, B, C, D, E> Five<A, B, C, D, E>(Protocols protocols)
        {
            return new Five<A, B, C, D, E>(protocols);
        }
    }
    public class Two<A, B>
    {
        private readonly Protocols protocols;

        public Two(Protocols protocols)
        {
            this.protocols = protocols;
        }

        public A P1()
        {
            return protocols.Get<A>(0);
        }

        public B P2()
        {
            return protocols.Get<B>(1);
        }
    }

    public class Three<A, B, C>
    {
        private readonly Protocols protocols;

        public Three(Protocols protocols)
        {
            this.protocols = protocols;
        }

        public A P1()
        {
            return protocols.Get<A>(0);
        }

        public B P2()
        {
            return protocols.Get<B>(1);
        }

        public C P3()
        {
            return protocols.Get<C>(2);
        }
    }

    public class Four<A, B, C, D>
    {
        private readonly Protocols protocols;

        public Four(Protocols protocols)
        {
            this.protocols = protocols;
        }

        public A P1()
        {
            return protocols.Get<A>(0);
        }

        public B P2()
        {
            return protocols.Get<B>(1);
        }

        public C P3()
        {
            return protocols.Get<C>(2);
        }

        public D P4()
        {
            return protocols.Get<D>(3);
        }
    }

    public class Five<A, B, C, D, E>
    {
        private readonly Protocols protocols;

        public Five(Protocols protocols)
        {
            this.protocols = protocols;
        }

        public A P1()
        {
            return protocols.Get<A>(0);
        }

        public B P2()
        {
            return protocols.Get<B>(1);
        }

        public C P3()
        {
            return protocols.Get<C>(2);
        }

        public D P4()
        {
            return protocols.Get<D>(3);
        }

        public E P5()
        {
            return protocols.Get<E>(4);
        }
    }
}
