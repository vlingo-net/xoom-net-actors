// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors;

public abstract class ActorProxyBase
{
    public static TNew Thunk<TNew>(ActorProxyBase proxy, Actor actor, TNew arg) => 
        proxy.IsDistributable ? Thunk(actor.LifeCycle.Environment.Stage, arg) : arg;

    public static TNew Thunk<TNew>(Stage stage, TNew arg)
    {
        if (typeof(ActorProxyBase).IsAssignableFrom(typeof(TNew)))
        {
            var b = (ActorProxyBase) (object) arg!;
            return stage.LookupOrStartThunk<TNew>(Actors.Definition.From(stage, b?.Definition, stage.World.DefaultLogger), b?.Address);
        }

        return arg;
    }

    public ActorProxyBase()
    {
    }
        
    public ActorProxyBase(Type? protocol, Definition.SerializationProxy definition, IAddress address)
    {
        Protocol = protocol;
        Definition = definition;
        Address = address;
    }

    public Type? Protocol { get; }
    public Definition.SerializationProxy? Definition { get; }
    public IAddress? Address { get; }

    public bool IsDistributable => Address?.IsDistributable ?? false;
}