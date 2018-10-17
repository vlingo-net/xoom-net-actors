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
        private readonly object[] _protocolActors;

        public T Get<T>(int index)
        {
            return (T)_protocolActors[index];
        }

        internal Protocols(object[] protocolActors)
        {
            _protocolActors = protocolActors;
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
        public A _1;
        public B _2;

        public Two(Protocols protocols)
        {
            _1 = protocols.Get<A>(0);
            _2 = protocols.Get<B>(1);
        }
    }

    public class Three<A, B, C>
    {
        public A _1;
        public B _2;
        public C _3;

        public Three(Protocols protocols)
        {
            _1 = protocols.Get<A>(0);
            _2 = protocols.Get<B>(1);
            _3 = protocols.Get<C>(2);
        }
    }

    public class Four<A, B, C, D>
    {
        public A _1;
        public B _2;
        public C _3;
        public D _4;

        public Four(Protocols protocols)
        {
            _1 = protocols.Get<A>(0);
            _2 = protocols.Get<B>(1);
            _3 = protocols.Get<C>(2);
            _4 = protocols.Get<D>(3);
        }
    }

    public class Five<A, B, C, D, E>
    {
        public A _1;
        public B _2;
        public C _3;
        public D _4;
        public E _5;

        public Five(Protocols protocols)
        {
            _1 = protocols.Get<A>(0);
            _2 = protocols.Get<B>(1);
            _3 = protocols.Get<C>(2);
            _4 = protocols.Get<D>(3);
            _5 = protocols.Get<E>(4);
        }
    }
}
