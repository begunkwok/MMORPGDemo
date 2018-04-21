using GameFramework.Fsm;

namespace GameMain
{
    public class ActorFsmAI : ActorAIBase
    {
        protected IFsm<ActorBase> m_Fsm;


        public ActorFsmAI(ActorBase owner, AIModeType mode, float atkDist, float followDist, float waringDist)
            : base(owner, mode, atkDist, followDist, waringDist)
        {
            FsmState<ActorBase>[] states =
            {

            };
            //m_Fsm = GameEntry.Fsm.CreateFsm(owner, states);
            //m_Fsm.Start<ActorIdleFsm>();
        }

        public override void Start()
        {
            if (AIMode == AIModeType.Hand || Owner.IsDead)
            {
                return;
            }


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
            throw new System.NotImplementedException();
        }

        public override void Clear()
        {
          //  GameEntry.Fsm.DestroyFsm(m_Fsm);
        }

        public override void ChangeAIState<T>(AIStateType stateType)
        {
            if (AIStateType != stateType)
            {
                AIStateType = stateType;
                ActorFsmStateBase state = m_Fsm.GetState<T>();
                state.ChangeState<T>();
            }
        }
    }
}
