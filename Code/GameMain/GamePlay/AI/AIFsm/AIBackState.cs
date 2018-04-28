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
    
            m_Owner.ExecuteCommand(new AutoMoveCommand(m_Owner.BornParam.Position, OnBackFinished));
        }

        private void OnBackFinished()
        {
            ChangeState<AIIdleState>();
        }
    }
}