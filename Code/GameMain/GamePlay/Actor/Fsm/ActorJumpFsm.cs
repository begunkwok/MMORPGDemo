using GameFramework.Fsm;

namespace GameMain
{
    public class ActorJumpFsm : ActorFsmStateBase
    {
        protected override void OnEnter(IFsm<ActorBase> fsm)
        {
            base.OnEnter(fsm);
            m_Owner.OnJump();
        }
    }
}
