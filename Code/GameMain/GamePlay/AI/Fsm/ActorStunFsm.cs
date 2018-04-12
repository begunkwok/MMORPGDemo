using GameFramework.Fsm;

namespace GameMain
{
    public class ActorStunFsm : ActorFsmStateBase
    {
        protected override void OnEnter(IFsm<ActorBase> fsm)
        {
            base.OnEnter(fsm);
            m_Owner.ApplyRootMotion(false);

            StunCommand cmd = m_Command as StunCommand;
            m_Owner.OnStun(cmd.LastTime);
        }

        protected override void OnLeave(IFsm<ActorBase> fsm, bool isShutdown)
        {
            base.OnLeave(fsm, isShutdown);
            m_Owner.ApplyRootMotion(true);
        }
    }
}
