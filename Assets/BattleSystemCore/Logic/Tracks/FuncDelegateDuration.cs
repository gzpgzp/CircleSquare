namespace BattleSystemCore.Logic.Tracks
{
    public class FuncDelegateDuration : DurationTrack
    {
        public uint targetActorId;

        public override void Enter()
        {
            var actor = BattleSystem.Current.actorManager.GetActor(targetActorId);
            if (actor == null)
            {
                return;
            }

            // TODO: 委托逻辑
        }
    }
}