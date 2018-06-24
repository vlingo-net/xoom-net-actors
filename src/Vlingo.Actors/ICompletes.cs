namespace Vlingo.Actors
{
    public interface ICompletes<T>
    {
        void With(T outcome);
    }
}
