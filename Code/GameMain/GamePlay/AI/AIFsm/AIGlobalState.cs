using UnityEngine;
using System.Collections;
using GameFramework.Fsm;

namespace GameMain
{

    public class AIGlobalState : AIFsmStateBase
    {
        protected override void OnUpdate(IFsm<ActorBase> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

            if (m_Owner.IsDead)
            {
                AI.ChangeAIState<AIDeadState>(AIStateType.Dead);
                return;
            }
            if (AI.AIMode == AIModeType.Auto && m_Owner.Target == null)
            {
                IntervalFindEnemy();
            }
        }

        private void IntervalFindEnemy()
        {
            if (AI.FindEnemyInterval >= Constant.Define.MinFindenemyInterval)
            {
                ActorBase enemy = m_Owner.GetNearestEnemy(AI.WaringDist);
                if(enemy!=null)
                this.m_Owner.SetTarget(enemy);
                AI.FindEnemyTimer = 0;
            }
            else
            {
                AI.FindEnemyTimer += Time.deltaTime;
            }
        }
    }
}