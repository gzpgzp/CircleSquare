namespace BattleSystemCore.Logic.Actors
{
    public class ActorContext
    {
        public ActorContext(uint uID)
        {
            this.uID = uID;
        }

        private uint uID;
        public uint UID
        {
            get => uID; 
        }
    }
}