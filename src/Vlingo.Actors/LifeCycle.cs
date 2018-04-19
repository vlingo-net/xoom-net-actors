namespace Vlingo.Actors
{
    public class LifeCycle
    {
        public Address Address { get; set; }
        public Environment Environment { get; set; }
        public bool IsStopped { get; set; }
        public Definition Definition { get; set; }

        public void Stop(Actor actor)
        {
            throw new System.NotImplementedException();
        }

        public void Secure()
        {
            throw new System.NotImplementedException();
        }
    }
}