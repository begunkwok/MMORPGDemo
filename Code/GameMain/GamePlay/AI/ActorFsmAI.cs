using GameFramework.Fsm;

namespace GameMain
{
    public class ActorFsmAI : ActorAIBase
    {
        protected IFsm<ActorBase> m_AIFsm;
        protected string m_FsmName = string.Empty;

        public ActorFsmAI(ActorBase owner, AIModeType mode, float atkDist, float followDist, float waringDist, float findEnemyInterval)
            : base(owner, mode, atkDist, followDist, waringDist, findEnemyInterval)
        {
            m_FsmName = GlobalTools.Format("ActorAIFsm[{0}]", Owner.Id);

            FsmState<ActorBase>[] states =
            {
                new AIIdleState(),
                new AIFollowState(),
                new AIFleeState(),
                new AIPatrolState(),
                new AIEscapeState(),
                new AIBackState(),
                new AIFightState(),
                new AIDeadState(),
                new AIChaseState(),
                new AIGlobalState(),
            };

            m_AIFsm = GameEntry.Fsm.CreateFsm(m_FsmName, Owner as ActorBase, states);

        }

        public override void Start()
        {
            if (AIMode == AIModeType.Hand || Owner.IsDead)
            {
                return;
            }

            m_AIFsm.Start<AIIdleState>();
            this.AIStateType = AIStateType.Idle;
        }

        public override void Step()
        {
            if (AIMode == AIModeType.Hand || Owner.IsDead)
            {
                return;
            }
        }

        public override void Stop()
        {
            ChangeAIState<ActorIdleFsm>(AIStateType.Idle);
        }

        public override void Clear()
        {
            GameEntry.Fsm.DestroyFsm<ActorBase>(m_FsmName);
        }

        public override void ChangeAIState<T>(AIStateType stateType)
        {
            if (AIStateType != stateType)
            {
                AIStateType = stateType;
                ActorFsmStateBase state = m_AIFsm.GetState<T>();
                state.ChangeState<T>();
            }
        }
    }
}
