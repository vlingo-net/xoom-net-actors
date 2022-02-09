// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace Vlingo.Xoom.Actors.PubSub
{
    internal delegate bool Operation(ISet<ISubscriber> existingSubscriber, ISubscriber givenSubscriber);

    internal static class OperationExtension
    {
        public static bool Perform(
            this Operation operation, 
            ISet<ISubscriber> existingSubscriber, 
            ISubscriber givenSubscriber)
            => operation.Invoke(existingSubscriber, givenSubscriber);
    }
}
