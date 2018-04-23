using GameFramework.Fsm;

namespace GameMain
{
    public class AIIdleState : AIFsmStateBase
    {
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
                                AI.ChangeAIState<AIChaseState>(AIStateType.Chase);
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
                                AI.ChangeAIState<AIChaseState>(AIStateType.Chase);
                            }
                        }
                        ActorBase pHost = m_Owner.Host;
                        if (pHost != null)
                        {
                            float dist = GlobalTools.GetHorizontalDistance(m_Owner.Pos, m_Owner.Host.Pos);
                            if (dist > AI.FollowDist)
                            {
                                AI.ChangeAIState<AIFollowState>(AIStateType.Follow);
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
                                AI.ChangeAIState<AIChaseState>(AIStateType.Chase);
                            }
                        }
                    }
                    break;
            }
        }

    }
}
