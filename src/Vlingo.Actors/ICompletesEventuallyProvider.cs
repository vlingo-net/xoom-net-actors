using System;
using System.Collections.Generic;
using System.Text;

namespace Vlingo.Actors
{
    public interface ICompletesEventuallyProvider
    {
        void Close();
        ICompletesEventually CompletesEventually { get; }
        void InitializeUsing(Stage stage);
        ICompletesEventually ProvideCompletesFor<T>(ICompletes<T> clientCompletes);
    }
}
