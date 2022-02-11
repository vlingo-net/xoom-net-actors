// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors;

public class CompletesEventuallyActor : Actor, ICompletesEventually
{
    public virtual void With(object? outcome)
    {
        try
        {
            var pooled = (PooledCompletes)outcome!;
            pooled.ClientCompletes!.With(pooled.Outcome!);
        }
        catch (Exception ex)
        {
            Logger.Error($"The eventually completed outcome '{outcome}' failed in the client because: {ex.Message}", ex);
        }
    }
}