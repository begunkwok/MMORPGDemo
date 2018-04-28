using GameFramework.Fsm;

namespace GameMain
{
    public class ActorBeatBackFsm : ActorFsmStateBase
    {
        public ActorBeatBackFsm(ActorFsmStateType state) : base(state)
        {

        }

        protected override void OnEnter(IFsm<ActorBase> fsm)
        {
            base.OnEnter(fsm);

        }
    }
}
