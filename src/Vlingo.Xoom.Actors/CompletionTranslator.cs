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
/// Used by <c>StateObjectQueryActor</c> to provide answers from queries that complete asynchronously
/// to the original message delivery.
/// </summary>
/// <typeparam name="TOutcome">The outcome value type of the internal <see cref="Func{TOutcome, TReturn}"/></typeparam>
/// <typeparam name="TReturn">The return value type of the internal <see cref="Func{TOutcome, TReturn}"/></typeparam>
public class CompletionTranslator<TOutcome, TReturn>
{
    private readonly ICompletesEventually _completes;
    private readonly Func<TOutcome, TReturn> _translator;
        
    /// <summary>
    /// Answer a new instance of <see cref="CompletionTranslator{TOutcome, TReturn}"/> if the <paramref name="translator"/> is not <c>null</c>
    /// otherwise answer null.
    /// </summary>
    /// <param name="translator">The <see cref="Func{TOutcome, TReturn}"/> of the eventual outcome, or null if none is provided</param>
    /// <param name="completes">The CompletesEventually through which the eventual outcome is sent</param>
    /// <typeparam name="TOutcome">The outcome type of the given translator, if any</typeparam>
    /// <typeparam name="TReturn">The return type of the given translator, if any</typeparam>
    /// <returns><see cref="CompletionSupplier{TReturn}"/></returns>
    public static CompletionTranslator<TOutcome, TReturn>? TranslatorOrNull(Func<TOutcome, TReturn> translator, ICompletesEventually completes)
    {
        if (translator == null)
        {
            return null;
        }

        return new CompletionTranslator<TOutcome, TReturn>(translator, completes);
    }
        
    /// <summary>
    /// Completes the outcome by executing the <see cref="Func{TOutcome, TReturn}"/> translator to produce the answer.
    /// </summary>
    /// <param name="outcome">The <typeparamref name="TOutcome"/> outcome to be translated into a completion value</param>
    public void Complete(TOutcome outcome) => _completes.With(_translator(outcome));
        
    /// <summary>
    /// Construct my default state.
    /// </summary>
    /// <param name="translator">The <see cref="Func{TOutcome, TReturn}"/> to translate the eventual outcome with which to complete</param>
    /// <param name="completes">The <see cref="ICompletesEventually"/> used to complete the eventual outcome</param>
    private CompletionTranslator(Func<TOutcome, TReturn> translator, ICompletesEventually completes)
    {
        _translator = translator;
        _completes = completes;
    }
}