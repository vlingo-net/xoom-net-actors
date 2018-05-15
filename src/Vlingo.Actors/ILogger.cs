using System;

namespace Vlingo.Actors
{
    public interface ILogger
    {
        void Log(string message);
        void Log(string message, Exception ex);
    }
}