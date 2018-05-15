using System;

namespace Vlingo.Actors
{
    public class Stowage
    {
        public bool IsDispersing { get; }
        public bool IsStowing { get; set; }

        public void DispersingMode()
        {
            throw new System.NotImplementedException();
        }

        public void StowingMode()
        {
            throw new System.NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}