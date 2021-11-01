// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Vlingo.Xoom.Actors
{
    public static class ExpressionExtensions
    {
        public static IEnumerable<object> GetArguments(this NewExpression newExpression)
        {
            var arguments = new List<object>();
            foreach (var argumentExpression in newExpression.Arguments)
            {
                var conversion = Expression.Convert(argumentExpression, typeof (object));
                var l = Expression.Lambda<Func<object>>(conversion);
                var f = l.Compile();
                var res = f();

                arguments.Add(res);
            }
            
            return arguments;
        }
        
        public static Expression<Action<TActor>> ToSerializableExpression<TActor>(this Action<TActor> internalConsumer)
            => x => internalConsumer(x);
    }
}