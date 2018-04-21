using GameFramework.Fsm;

namespace GameMain
{
    public class ActorDeadFsm : ActorFsmStateBase
    {
        protected override void OnEnter(IFsm<ActorBase> fsm)
        {
            base.OnEnter(fsm);
            m_Owner.ApplyCharacterCtrl(false);
            m_Owner.OnDead(m_Command as DeadCommand);
        }
    }
}
