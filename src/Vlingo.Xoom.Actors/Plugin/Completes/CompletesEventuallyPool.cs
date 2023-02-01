// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors.Plugin.Completes;

public class CompletesEventuallyPool : ICompletesEventuallyProvider
{
    private readonly AtomicLong _completesEventuallyId;
    private readonly ICompletesEventually[] _pool;
    private readonly AtomicLong _poolIndex;
    private readonly long _poolSize;
    private readonly string _mailboxName;

    public CompletesEventuallyPool(int poolSize, string mailboxName)
    {
        _completesEventuallyId = new AtomicLong(0);
        _poolSize = poolSize;
        _mailboxName = mailboxName;
        _poolIndex = new AtomicLong(0);
        _pool = new ICompletesEventually[poolSize];
    }

    public void Close()
    {
        foreach(var completes in _pool)
        {
            completes.Stop();
        }
    }

    public ICompletesEventually CompletesEventually
    {
        get
        {
            int index = (int)(_poolIndex.IncrementAndGet() % _poolSize);
            return _pool[index];
        }
    }

    public void InitializeUsing(Stage stage)
    {
        for (var idx = 0; idx < _poolSize; ++idx)
        {
            _pool[idx] = stage.ActorFor<ICompletesEventually>(
                Definition.Has<CompletesEventuallyActor>(
                    Definition.NoParameters,
                    _mailboxName,
                    "completes-eventually-" + (idx + 1)));
        }
    }

    public ICompletesEventually ProvideCompletesFor(ICompletes? clientCompletes)
        => new PooledCompletes(
            _completesEventuallyId.GetAndIncrement(),
            clientCompletes,
            CompletesEventually);

    public ICompletesEventually ProvideCompletesFor(IAddress address, ICompletes? clientCompletes)
        => new PooledCompletes(
            _completesEventuallyId.GetAndIncrement(),
            clientCompletes,
            CompletesEventuallyOf(address));

    private ICompletesEventually CompletesEventuallyOf(IAddress address)
    {
        foreach (var completesEventually in _pool)
        {
            if (completesEventually.Address.Equals(address))
            {
                return completesEventually;
            }
        }
        return CompletesEventually;
    }
}