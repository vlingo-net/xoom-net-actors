using System;

namespace Vlingo.Actors
{
    public interface ILogger
    {
        bool IsEnabled { get; }
        string Name { get; }
        void Log(string message);
        void Log(string message, Exception ex);
        void Close();
    }
}