using GameFramework.Fsm;

namespace GameMain
{
    public class AIIdleState : AIFsmStateBase
    {
        public AIIdleState(AIStateType state) : base(state)
        {
        }

        protected override void OnUpdate(IFsm<ActorBase> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

            ActorBase pTarget = m_Owner.Target;
            switch (m_Owner.ActorType)
            {
                case ActorType.Monster:
                    {
                        if (pTarget != null)
                        {
                            float dist = GlobalTools.GetHorizontalDistance(m_Owner.Pos, pTarget.Pos);
                            if (dist < AI.WaringDist)
                            {
                                ChangeState<AIChaseState>();
                            }
                        }
                    }
                    break;
                case ActorType.Partner:
                    {
                        if (pTarget != null)
                        {
                            float dist = GlobalTools.GetHorizontalDistance(m_Owner.Pos, pTarget.Pos);
                            if (dist < AI.WaringDist)
                            {
                                ChangeState<AIChaseState>();
                            }
                        }
                        ActorBase pHost = m_Owner.Host;
                        if (pHost != null)
                        {
                            float dist = GlobalTools.GetHorizontalDistance(m_Owner.Pos, m_Owner.Host.Pos);
                            if (dist > AI.FollowDist)
                            {
                                ChangeState<AIFollowState>();
                                return;
                            }
                        }
                    }
                    break;
                case ActorType.Player:
                    {
                        if (pTarget != null)
                        {
                            float dist = GlobalTools.GetHorizontalDistance(m_Owner.Pos, pTarget.Pos);
                            if (dist < AI.WaringDist)
                            {
                                ChangeState<AIChaseState>();
                            }
                        }
                    }
                    break;
            }
        }

    }
}
