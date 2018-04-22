namespace Vlingo.Actors
{
    public class World
    {
        public static int PrivateRootId = int.MaxValue;
        public static int PublicRootId = PrivateRootId - 1;
        public static int DeadlettersId = PublicRootId - 1;
        public DeadLetters DeadLetters { get; set; }

        public Stage StageNamed(string name)
        {
            throw new System.NotImplementedException();
        }
    }
}