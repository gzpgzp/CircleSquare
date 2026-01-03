namespace BattleSystemCore.Logic.Actors
{
    public class BaseActor
    {
        public ActorContext actorContext { get; private set; }

        public void OnCreate(uint actorID)
        {
            actorContext = new ActorContext(actorID);
        }

        public void Init()
        {
            
        }
    }
}