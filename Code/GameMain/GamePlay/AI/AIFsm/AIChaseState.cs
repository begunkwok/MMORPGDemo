using GameFramework.Fsm;

namespace GameMain
{
    public class AIChaseState : AIFsmStateBase
    {
        protected override void OnUpdate(IFsm<ActorBase> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

            switch (m_Owner.ActorType)
            {
                case ActorType.Monster:
                {
                    if (m_Owner.Target == null)
                    {
                        return;
                    }
                    float dist = GlobalTools.GetHorizontalDistance(m_Owner.Pos, m_Owner.Target.Pos);
                    if (dist > AI.WaringDist)
                    {
                        AI.ChangeAIState<AIBackState>(AIStateType.Back);
                    }
                    else if (dist < AI.AttackDist)
                    {
                        AI.ChangeAIState<AIFightState>(AIStateType.Fight);
                        return;
                    }
                }
                    break;
                case ActorType.Partner:
                {
                    if (m_Owner.Target == null)
                    {
                        AI.ChangeAIState<AIIdleState>(AIStateType.Idle);
                        return;
                    }

                    float dist = GlobalTools.GetHorizontalDistance(m_Owner.Pos, m_Owner.Target.Pos);
                    if (dist > AI.WaringDist)
                    {
                            AI.ChangeAIState<AIIdleState>(AIStateType.Idle);
                            return;
                    }
                    else if (dist < AI.AttackDist)
                    {
                        AI.ChangeAIState<AIFightState>(AIStateType.Fight);
                        return;
                    }
                }
                    break;
                case ActorType.Player:
                {
                    if (m_Owner.Target == null)
                    {
                            AI.ChangeAIState<AIIdleState>(AIStateType.Idle);
                            return;
                    }
                    float dist = GlobalTools.GetHorizontalDistance(m_Owner.Pos, m_Owner.Target.Pos);

                    if (dist < AI.AttackDist)
                    {
                            AI.ChangeAIState<AIFightState>(AIStateType.Fight);
                            return;
                    }
                }
                    break;
            }

            if (m_Owner.Target != null)
            {
                m_Owner.ExecuteCommand(new AutoMoveCommand(m_Owner.Target));
            }

        }

   
    }
}