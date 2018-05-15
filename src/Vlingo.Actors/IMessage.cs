namespace Vlingo.Actors
{
    public interface IMessage
    {
        Actor Actor { get; }
        void Deliver();
        string Representation { get; }

        bool IsStowed { get; }
    }
}
