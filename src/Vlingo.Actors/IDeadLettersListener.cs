using System;
using System.Collections.Generic;
using System.Text;

namespace Vlingo.Actors
{
    public interface IDeadLettersListener
    {
        void Handle(DeadLetter deadLetter);
    }
}
