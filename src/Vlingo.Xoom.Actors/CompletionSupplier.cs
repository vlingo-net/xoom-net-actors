// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors;

/// <summary>
/// Supports providing a latent <see cref="ICompletes{TResult}"/> outcome by way of <see cref="ICompletesEventually"/>.
/// Used by <c>Sourced</c>, <c>ObjectEntity</c>, and <c>StateEntity</c> to provide answers
/// from methods that complete asynchronously to the original message delivery.
/// </summary>
/// <typeparam name="TReturn">The return value type of the internal <see cref="Func{TResult}"/></typeparam>
public class CompletionSupplier<TReturn>
{
    private readonly Func<TReturn> _supplier;
    private readonly ICompletesEventually _completes;
        
    /// <summary>
    /// Answer a new instance of <see cref="CompletionSupplier{TReturn}"/> if the <paramref name="supplier"/> is not <c>null</c>
    /// otherwise answer null.
    /// </summary>
    /// <param name="supplier">The <see cref="Func{TNewResult}"/> of the eventual outcome, or null if none is provided</param>
    /// <param name="completes">The CompletesEventually through which the eventual outcome is sent</param>
    /// <typeparam name="TNewResult">The return type of the given supplier, if any</typeparam>
    /// <returns><see cref="CompletionSupplier{TNewResult}"/></returns>
    public static CompletionSupplier<TNewResult>? SupplierOrNull<TNewResult>(Func<TNewResult>? supplier, ICompletesEventually completes)
    {
        if (supplier == null)
        {
            return null;
        }

        return new CompletionSupplier<TNewResult>(supplier, completes);
    }
        
    /// <summary>
    /// Completes the outcome by executing the <see cref="Func{TReturn}"/> for its answer.
    /// </summary>
    public void Complete() => _completes.With(_supplier());
        
    /// <summary>
    /// Construct my default state.
    /// </summary>
    /// <param name="supplier">The <see cref="Func{TReturn}"/> to supply the eventual outcome with which to complete</param>
    /// <param name="completes">the <see cref="ICompletesEventually"/> used to complete the eventual outcome</param>
    private CompletionSupplier(Func<TReturn> supplier, ICompletesEventually completes)
    {
        _supplier = supplier;
        _completes = completes;
    }
}