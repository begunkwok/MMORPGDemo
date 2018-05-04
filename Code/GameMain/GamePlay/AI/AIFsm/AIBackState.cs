using UnityEngine;
using System.Collections;
using GameFramework.Fsm;

namespace GameMain
{
    public class AIBackState : AIFsmStateBase
    {
        public AIBackState(AIStateType state) : base(state)
        {
        }

        protected override void OnEnter(IFsm<ActorBase> fsm)
        {
            base.OnEnter(fsm);

            m_Owner.SetTarget(null);
            m_Owner.ExecuteCommand(new AutoMoveCommand(m_Owner.BornParam.Position, OnBackFinished));
        }

        protected override void OnUpdate(IFsm<ActorBase> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

            if (m_Owner.Target == null)
            {
                return;
            }

            float dist = GlobalTools.GetHorizontalDistance(m_Owner.Pos, m_Owner.Target.Pos);

            if (dist < AI.AttackDist)
            {
                ChangeState<AIFightState>();
            }
        }

        protected override void OnLeave(IFsm<ActorBase> fsm, bool isShutdown)
        {
            base.OnLeave(fsm, isShutdown);

            m_Owner.CachedTransform.localPosition = m_Owner.BornParam.Position;
            m_Owner.CachedTransform.localEulerAngles = m_Owner.BornParam.EulerAngles;
        }

        private void OnBackFinished()
        {
            ChangeState<AIIdleState>();
        }
    }
}