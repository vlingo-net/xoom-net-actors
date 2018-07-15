// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.
using System;

namespace Vlingo.Actors
{
    public class Stowage
    {
        public bool IsDispersing { get; }
        public bool IsStowing { get; set; }

        internal IMessage Head { get; set; }

        public void DispersingMode()
        {
            throw new System.NotImplementedException();
        }

        public void StowingMode()
        {
            throw new System.NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        internal void Stow(IMessage message)
        {
            throw new NotImplementedException();
        }

        internal IMessage SwapWith(IMessage message)
        {
            throw new NotImplementedException();
        }
    }
}