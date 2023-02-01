// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors.Plugin.Mailbox;

public class SuspendedDeliveryOverrides
{
    private readonly AtomicBoolean _accessible;
    private readonly IList<Overrides> _overrides;

    internal SuspendedDeliveryOverrides()
    {
        _accessible = new AtomicBoolean(false);
        _overrides = new List<Overrides>();
    }

    internal bool IsEmpty => _overrides.Count == 0;

    internal bool MatchesTop(Type messageType)
    {
        var overrides = Peek();

        if (overrides != null)
        {
            return overrides.Types.Contains(messageType);
        }

        return false;
    }

    internal Overrides? Peek()
    {
        var retries = 0;
        while (true)
        {
            if(_accessible.CompareAndSet(false, true))
            {
                Overrides? temp = null;
                if (!IsEmpty)
                {
                    temp = _overrides[0];
                }
                _accessible.Set(false);
                return temp;
            }

            if(++retries > 100_000_000)
            {
                return null;
            }
        }
    }

    internal IEnumerable<Overrides> Find(string name)
    {
        var retries = 0;
        while (true)
        {
            if (_accessible.CompareAndSet(false, true))
            {
                var overridesNamed = _overrides.Where(o => o.Name == name);
                _accessible.Set(false);
                return overridesNamed;
            }

            if (++retries > 100_000_000)
            {
                Console.WriteLine(new Exception().StackTrace);
                return Enumerable.Empty<Overrides>();
            }
        }
    }

    public bool Pop(string name)
    {
        var popped = false;
        var retries = 0;
        while (true)
        {
            if(_accessible.CompareAndSet(false, true))
            {
                var elements = _overrides.Count;
                for(var index=0; index < elements; ++index)
                {
                    if (name.Equals(_overrides[index].Name))
                    {
                        if(index == 0)
                        {
                            _overrides.RemoveAt(index);
                            popped = true;
                            --elements;

                            while(index < elements)
                            {
                                if (_overrides[index].Obsolete)
                                {
                                    _overrides.RemoveAt(index);
                                    --elements;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            _overrides[index].Obsolete = true;
                        }

                        _accessible.Set(false);
                        break;
                    }
                }

                break;
            }

            if(++retries > 100_000_000)
            {
                return false;
            }
        }

        return popped;
    }

    public void Push(Overrides overrides)
    {
        var retries = 0;

        while (true)
        {
            if(_accessible.CompareAndSet(false, true))
            {
                _overrides.Add(overrides);
                _accessible.Set(false);
                break;
            }

            if(++retries > 100_000_000)
            {
                return;
            }
        }
    }
}

public class Overrides
{
    public Overrides(string name, Type[] types)
    {
        Name = name;
        Types = types;
        Obsolete = false;
    }

    public string Name { get; }
    public Type[] Types { get; }
    public bool Obsolete { get; set; }
}