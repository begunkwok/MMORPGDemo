using GameFramework.Fsm;

namespace GameMain
{
    public class ActorIdleFsm : ActorFsmStateBase
    {
        protected override void OnEnter(IFsm<ActorBase> fsm)
        {
            base.OnEnter(fsm);
            m_Owner.OnIdle();
        }
    }
}
