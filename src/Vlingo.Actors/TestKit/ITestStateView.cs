using System;
using System.Collections.Generic;
using System.Text;

namespace Vlingo.Actors.TestKit
{
    public interface ITestStateView
    {
        TestState ViewTestState();
    }
}
