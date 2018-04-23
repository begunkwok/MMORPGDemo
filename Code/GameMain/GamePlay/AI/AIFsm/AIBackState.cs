using UnityEngine;
using System.Collections;
using GameFramework.Fsm;

namespace GameMain
{
    public class AIBackState : AIFsmStateBase
    {
        protected override void OnEnter(IFsm<ActorBase> fsm)
        {
            base.OnEnter(fsm);
    
            m_Owner.ExecuteCommand(new AutoMoveCommand(m_Owner.BornParam.Position, OnBackFinished));
        }

        private void OnBackFinished()
        {
            AI.ChangeAIState<AIIdleState>(AIStateType.Idle);
        }
    }
}